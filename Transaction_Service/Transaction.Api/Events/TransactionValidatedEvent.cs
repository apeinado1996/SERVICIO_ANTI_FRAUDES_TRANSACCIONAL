namespace Transaction.Api.Events
{
    public class TransactionValidatedEvent
    {
        public Guid TransactionExternalId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
