using Confluent.Kafka;
using System.Text.Json;
using Transaction.Api.Events;
using Transaction.Core.Services;

namespace Transaction.Api.Messaging
{
    public class TransactionValidatedConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TransactionValidatedConsumer> _logger;

        public TransactionValidatedConsumer(
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            ILogger<TransactionValidatedConsumer> logger)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = Task.Run(() => RunConsumerLoopAsync(stoppingToken), stoppingToken);
            return Task.CompletedTask;
        }

        private async Task RunConsumerLoopAsync(CancellationToken stoppingToken)
        {
            var bootstrap = _configuration["Kafka:BootstrapServers"];
            var topic = _configuration["Kafka:TopicTransactionsValidated"];

            if (string.IsNullOrWhiteSpace(bootstrap) || string.IsNullOrWhiteSpace(topic))
            {
                _logger.LogWarning("Kafka configuration is invalid.", bootstrap, topic);
                return;
            }

            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrap,
                GroupId = "transaction-service-validator",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            try
            {
                consumer.Subscribe(topic);
                _logger.LogInformation("TransactionValidatedConsumer started listening on topic {Topic}", topic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to topic {Topic}. Consumer will stop.", topic);
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(TimeSpan.FromSeconds(1));
                    if (cr is null || cr.Message is null || string.IsNullOrWhiteSpace(cr.Message.Value))
                        continue;

                    var evt = JsonSerializer.Deserialize<TransactionValidatedEvent>(cr.Message.Value);
                    if (evt is null)
                        continue;

                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<ITransactionService>();

                    await service.ApplyValidationResultAsync(
                        evt.TransactionExternalId,
                        evt.Status,
                        evt.Reason,
                        stoppingToken);

                    _logger.LogInformation("Updated transaction {Id} with status {Status}", evt.TransactionExternalId, evt.Status);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error in TransactionValidatedConsumer.");
                    await Task.Delay(1000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in TransactionValidatedConsumer.");
                    await Task.Delay(1000, stoppingToken);
                }
            }

            try
            {
                consumer.Close();
            }
            catch
            {
            }

            _logger.LogInformation("TransactionValidatedConsumer stopped.");
        }
    }
}
