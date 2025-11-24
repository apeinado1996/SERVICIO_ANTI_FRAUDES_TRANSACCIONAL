using Antifraud.Api.Events;
using Antifraud.Core.Entities;
using Confluent.Kafka;
using System.Text.Json;

namespace Antifraud.Api.Messaging
{
    public class TransactionProducer : IAsyncDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;

        public TransactionProducer(IConfiguration configuration)
        {
            var bootstrapServers = configuration["Kafka:BootstrapServers"];
            _topic = configuration["Kafka:TopicTransactionsValidated"];

            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task PublishValidationAsync(TransactionCreatedEvent evt, FraudCheckResult result, CancellationToken cancellationToken)
        {
            var validation = new TransactionValidatedEvent
            {
                TransactionExternalId = evt.TransactionExternalId,
                CreatedAt = evt.CreatedAt,
                Status = result.IsFraud ? "REJECTED" : "APPROVED",
                Reason = result.Reason
            };

            var message = new Message<string, string>
            {
                Key = evt.TransactionExternalId.ToString(),
                Value = JsonSerializer.Serialize(validation)
            };

            await _producer.ProduceAsync(_topic, message, cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            _producer.Flush(TimeSpan.FromSeconds(5));
            _producer.Dispose();
            await Task.CompletedTask;
        }
    }
}
