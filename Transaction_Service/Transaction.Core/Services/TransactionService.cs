using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction.Core.Entities;
using Transaction.Core.Interface;

namespace Transaction.Core.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;

        public TransactionService(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<TransactionEntity> CreateTransactionAsync(Guid sourceAccountId, Guid targetAccountId, int transferTypeId, decimal value, CancellationToken cancellationToken = default)
        {
            var transaction = new TransactionEntity
            {
                Id = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                SourceAccountId = sourceAccountId,
                TargetAccountId = targetAccountId,
                TransferTypeId = transferTypeId,
                Value = value,
                Status = TransactionStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            await _transactionRepository.AddAsync(transaction, cancellationToken);
            await _transactionRepository.SaveChangesAsync(cancellationToken);

            return transaction;
        }

        public Task<TransactionEntity?> GetTransactionStatusAsync(Guid externalId, DateTime createdAt, CancellationToken cancellationToken = default)
        {
            return _transactionRepository.GetByExternalIdAsync(externalId, createdAt, cancellationToken);
        }

        public async Task ApplyValidationResultAsync(Guid externalId, string status, string reason, CancellationToken cancellationToken)
        {
            await _transactionRepository.ApplyValidationResultAsync(externalId, status, reason, cancellationToken);
        }
    }
}
