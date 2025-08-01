using CashFlowDailyBalance.Application.DTOs;
using CashFlowDailyBalance.Domain.Entities;

namespace CashFlowDailyBalance.Application.Interfaces
{
    public interface IDailyBalanceService
    {
   
        Task<DailyBalance> ProcessDailyBalanceAsync(DateTime date);

   
        Task<DailyBalance?> GetDailyBalanceAsync(DateTime date);


        Task<IEnumerable<DailyBalance>> GetDailyBalancesByPeriodAsync(DateTime startDate, DateTime endDate);
        
        Task<PaginatedResponseDto<DailyBalanceDto>> GetDailyBalancesAsync(int pageNumber, int pageSize);
    }
} 