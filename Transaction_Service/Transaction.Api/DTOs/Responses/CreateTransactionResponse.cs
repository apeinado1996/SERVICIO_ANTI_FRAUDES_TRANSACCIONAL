using Transaction.Core.Entities;

namespace Transaction.Api.DTOs.Responses
{
    public class CreateTransactionResponse
    {
        public Guid TransactionExternalId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public decimal Value { get; set; }
    }
}
