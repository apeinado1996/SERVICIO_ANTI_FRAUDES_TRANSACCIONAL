using Confluent.Kafka;
using System.Text.Json;
using Transaction.Api.Events;

namespace Transaction.Api.Messaging
{
    public interface ITransactionProducer
    {
        Task PublishAsync(TransactionCreatedEvent evt, CancellationToken cancellationToken);
    }

    public class TransactionProducer : ITransactionProducer, IAsyncDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;

        public TransactionProducer(IConfiguration configuration)
        {
            var bootstrapServers = configuration["Kafka:BootstrapServers"];
            _topic = configuration["Kafka:TopicTransactions"];

            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task PublishAsync(TransactionCreatedEvent @event, CancellationToken cancellationToken = default)
        {
            var key = @event.TransactionExternalId.ToString();
            var value = JsonSerializer.Serialize(@event);

            var message = new Message<string, string>
            {
                Key = key,
                Value = value
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
