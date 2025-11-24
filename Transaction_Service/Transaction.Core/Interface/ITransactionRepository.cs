using Transaction.Core.Entities;

namespace Transaction.Core.Interface
{
    public interface ITransactionRepository
    {
        Task AddAsync(TransactionEntity transaction, CancellationToken cancellationToken = default);
        Task<TransactionEntity?> GetByExternalIdAsync(Guid externalId, DateTime createdAt, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task ApplyValidationResultAsync(Guid externalId, string status, string reason, CancellationToken cancellationToken = default);
    }
}
