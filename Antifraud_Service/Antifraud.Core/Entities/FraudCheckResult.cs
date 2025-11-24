using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antifraud.Core.Entities
{
    public class FraudCheckResult
    {
        public bool IsFraud { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
