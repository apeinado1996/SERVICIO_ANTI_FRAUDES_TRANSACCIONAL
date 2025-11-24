using Antifraud.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antifraud.Core.Interface
{
    public interface IAntiFraudService
    {
        Task<FraudCheckResult> AnalyzeAsync(IncomingTransaction tx, CancellationToken cancellationToken);
    }
}
