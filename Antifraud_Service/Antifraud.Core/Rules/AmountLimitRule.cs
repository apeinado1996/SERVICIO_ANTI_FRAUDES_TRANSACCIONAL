using Antifraud.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antifraud.Core.Rules
{
    public class AmountLimitRule : IAntiFraudRule
    {
        public string Reason => "Transaction exceeds allowed maximum amount (2000).";

        public Task<bool> IsFraudulentAsync(IncomingTransaction tx, CancellationToken cancellationToken)
        {
            return Task.FromResult(tx.Value > 2000);
        }
    }
}
