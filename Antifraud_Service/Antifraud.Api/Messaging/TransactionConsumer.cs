using Antifraud.Api.Events;
using Antifraud.Core.Entities;
using Antifraud.Core.Interface;
using Confluent.Kafka;
using System.Text.Json;

namespace Antifraud.Api.Messaging
{
    public class TransactionConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TransactionConsumer> _logger;

        public TransactionConsumer(
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            ILogger<TransactionConsumer> logger)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var bootstrap = _configuration["Kafka:BootstrapServers"];
            var topic = _configuration["Kafka:TopicTransactions"];

            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrap,
                GroupId = "antifraud-service",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            consumer.Subscribe(topic);

            _logger.LogInformation("TransactionConsumer started for topic {Topic}", topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(stoppingToken);

                    if (string.IsNullOrWhiteSpace(cr.Message.Value))
                        continue;

                    var created = JsonSerializer.Deserialize<TransactionCreatedEvent>(cr.Message.Value);

                    if (created is null)
                        continue;

                    using var scope = _scopeFactory.CreateScope();

                    var antiFraudService = scope.ServiceProvider.GetRequiredService<IAntiFraudService>();
                    var producer = scope.ServiceProvider.GetRequiredService<TransactionProducer>();

                    var incoming = new IncomingTransaction
                    {
                        TransactionExternalId = created.TransactionExternalId,
                        SourceAccountId = created.SourceAccountId,
                        TargetAccountId = created.TargetAccountId,
                        TransferTypeId = created.TranferTypeId,
                        Value = created.Value,
                        CreatedAt = created.CreatedAt
                    };

                    var result = await antiFraudService.AnalyzeAsync(incoming, stoppingToken);

                    await producer.PublishValidationAsync(created, result, stoppingToken);

                    _logger.LogInformation("Transaction {ExternalId} validated as {Status}", created.TransactionExternalId, result.IsFraud ? "REJECTED" : "APPROVED");
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming from Kafka.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in TransactionConsumer.");
                }
            }
        }
    }
}
