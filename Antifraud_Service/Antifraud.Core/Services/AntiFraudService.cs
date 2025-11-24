using Antifraud.Core.Entities;
using Antifraud.Core.Rules;

namespace Antifraud.Core.Services
{
    public class AntiFraudService : Interface.IAntiFraudService
    {
        private readonly IEnumerable<IAntiFraudRule> _rules;

        public AntiFraudService(IEnumerable<IAntiFraudRule> rules)
        {
            _rules = rules;
        }

        public async Task<FraudCheckResult> AnalyzeAsync(IncomingTransaction tx, CancellationToken cancellationToken)
        {
            foreach (var rule in _rules)
            {
                if (await rule.IsFraudulentAsync(tx, cancellationToken))
                {
                    return new FraudCheckResult
                    {
                        IsFraud = true,
                        Reason = rule.Reason
                    };
                }
            }

            return new FraudCheckResult
            {
                IsFraud = false,
                Reason = "Approved"
            };
        }
    }
}
