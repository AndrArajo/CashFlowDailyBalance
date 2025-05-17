using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashFlowDailyBalance.Domain.Entities;

namespace CashFlowDailyBalance.Domain.Interfaces
{
    public interface ITransactionRepository
    {
   
        Task<IEnumerable<Transaction>> GetAllAsync();

 
        Task<IEnumerable<Transaction>> GetByDateAsync(DateTime date);

        Task<IEnumerable<Transaction>> GetByPeriodAsync(DateTime startDate, DateTime endDate);
    }
}
