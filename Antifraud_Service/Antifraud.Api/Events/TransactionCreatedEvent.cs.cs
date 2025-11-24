namespace Antifraud.Api.Events
{
    public class TransactionCreatedEvent
    {
        public Guid TransactionExternalId { get; set; }
        public Guid SourceAccountId { get; set; }
        public Guid TargetAccountId { get; set; }
        public int TranferTypeId { get; set; }
        public decimal Value { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
