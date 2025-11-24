using Antifraud.Core.Interface;
using Antifraud.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antifraud.Infrastructure.Repositories
{
    public class TransactionReadRepository : ITransactionReadRepository
    {
        private readonly AntiFraudDbContext _dbContext;

        public TransactionReadRepository(AntiFraudDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<decimal> GetDailyTotalForSourceAsync(
            Guid sourceAccountId,
            DateTime date,
            CancellationToken cancellationToken)
        {
            var targetDate = date.Date;

            return await _dbContext.Transactions.Where(t => t.SourceAccountId == sourceAccountId && t.CreatedAt.Date == targetDate).SumAsync(t => t.Value, cancellationToken);
        }
    }
}
