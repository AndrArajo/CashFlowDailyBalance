using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashFlowDailyBalance.Domain.Entities;

namespace CashFlowDailyBalance.Domain.Interfaces
{
    public interface IDailyBalanceRepository
    {
    
        Task<DailyBalance> SaveAsync(DailyBalance dailyBalance);
        

        Task<IEnumerable<DailyBalance>> GetAllAsync();
        
  
        Task<DailyBalance?> GetByDateAsync(DateTime date);
        
 
        Task<IEnumerable<DailyBalance>> GetByPeriodAsync(DateTime startDate, DateTime endDate);
    }
}
