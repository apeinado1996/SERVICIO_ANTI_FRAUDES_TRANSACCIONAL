using Antifraud.Core.Entities;
using Antifraud.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antifraud.Core.Rules
{
    public class DailyLimitRule : IAntiFraudRule
    {
        private readonly ITransactionReadRepository _transactionReadRepository;

        public DailyLimitRule(ITransactionReadRepository transactionReadRepository)
        {
            _transactionReadRepository = transactionReadRepository;
        }

        public string Reason => "Daily transaction limit exceeded (20000).";

        public async Task<bool> IsFraudulentAsync(IncomingTransaction tx, CancellationToken cancellationToken)
        {
            var date = tx.CreatedAt.Date;

            var dailyTotal = await _transactionReadRepository.GetDailyTotalForSourceAsync(tx.SourceAccountId, date, cancellationToken);

            var newTotal = dailyTotal;

            return newTotal > 20000m;
        }
    }
}
