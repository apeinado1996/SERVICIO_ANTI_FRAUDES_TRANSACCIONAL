using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction.Core.Entities;

namespace Transaction.Core.Services
{
    public interface ITransactionService
    {
        Task<TransactionEntity> CreateTransactionAsync(Guid sourceAccountId, Guid targetAccountId, int transferTypeId, decimal value, CancellationToken cancellationToken = default);
        Task<TransactionEntity?> GetTransactionStatusAsync(Guid externalId, DateTime createdAt, CancellationToken cancellationToken = default);
        Task ApplyValidationResultAsync(Guid transactionExternalId, string status, string reason, CancellationToken cancellationToken);
    }
}
