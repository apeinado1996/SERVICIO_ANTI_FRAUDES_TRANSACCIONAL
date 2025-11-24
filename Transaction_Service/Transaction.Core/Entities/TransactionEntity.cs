using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transaction.Core.Entities
{
    public class TransactionEntity
    {
        public Guid Id { get; set; }              
        public Guid ExternalId { get; set; }
        public Guid SourceAccountId { get; set; }
        public Guid TargetAccountId { get; set; }
        public int TransferTypeId { get; set; }
        public decimal Value { get; set; }
        public TransactionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }   
        public DateTime? UpdatedAt { get; set; } 
    }
}
