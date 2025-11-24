using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antifraud.Core.Interface
{
    public interface ITransactionReadRepository
    {
        Task<decimal> GetDailyTotalForSourceAsync(Guid sourceAccountId, DateTime date, CancellationToken cancellationToken);
    }
}
