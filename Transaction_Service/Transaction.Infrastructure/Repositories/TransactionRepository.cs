using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction.Core.Entities;
using Transaction.Core.Interface;
using Transaction.Infrastructure.Data;

namespace Transaction.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly TransactionDbContext _dbContext;

        public TransactionRepository(TransactionDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(TransactionEntity transaction, CancellationToken cancellationToken = default)
        {
            await _dbContext.Transactions.AddAsync(transaction, cancellationToken);
        }

        public async Task<TransactionEntity?> GetByExternalIdAsync(Guid externalId, DateTime createdAt, CancellationToken cancellationToken = default)
        {
            var date = createdAt.Date;

            return await _dbContext.Transactions.Where(t => t.ExternalId == externalId && t.CreatedAt.Date == date).FirstOrDefaultAsync(cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task ApplyValidationResultAsync(Guid externalId, string status, string reason, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Transactions
                .FirstOrDefaultAsync(t => t.ExternalId == externalId, cancellationToken);

            if (entity is null)
                return;

            entity.Status = status.Equals("APPROVED", StringComparison.OrdinalIgnoreCase)
                ? TransactionStatus.Approved
                : TransactionStatus.Rejected;

            entity.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
