namespace Transaction.Api.DTOs.Requests
{
    public class GetTransactionStatusRequest
    {
        public Guid TransactionExternalId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
