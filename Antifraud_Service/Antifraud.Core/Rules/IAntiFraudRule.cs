using Antifraud.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antifraud.Core.Rules
{
    public interface IAntiFraudRule
    {
        Task<bool> IsFraudulentAsync(IncomingTransaction tx, CancellationToken cancellationToken);
        string Reason { get; }
    }
}
