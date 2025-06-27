using CashFlowDailyBalance.Domain.Entities;

namespace CashFlowDailyBalance.Domain.Interfaces
{
    public interface IExternalTransactionService
    {
        Task<IEnumerable<Transaction>> GetAllAsync();
        Task<IEnumerable<Transaction>> GetByDateAsync(DateTime date);
        Task<IEnumerable<Transaction>> GetByPeriodAsync(DateTime startDate, DateTime endDate);
    }
} 