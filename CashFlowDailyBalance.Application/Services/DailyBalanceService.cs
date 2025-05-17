using CashFlowDailyBalance.Application.Interfaces;
using CashFlowDailyBalance.Domain.Entities;
using CashFlowDailyBalance.Domain.Enums;
using CashFlowDailyBalance.Domain.Interfaces;

namespace CashFlowDailyBalance.Application.Services
{
    public class DailyBalanceService : IDailyBalanceService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IDailyBalanceRepository _dailyBalanceRepository;

        public DailyBalanceService(ITransactionRepository transactionRepository, IDailyBalanceRepository dailyBalanceRepository)
        {
            _transactionRepository = transactionRepository;
            _dailyBalanceRepository = dailyBalanceRepository;
        }

        public async Task<DailyBalance> ProcessDailyBalanceAsync(DateTime date)
        {
            var normalizedDate = date.Date;
            
            var transactions = await _transactionRepository.GetByDateAsync(normalizedDate);
            
            decimal totalCredits = transactions
                .Where(t => t.Type == TransactionType.Credit)
                .Sum(t => t.Amount);
                
            decimal totalDebits = transactions
                .Where(t => t.Type == TransactionType.Debit)
                .Sum(t => t.Amount);
            
            var previousDay = normalizedDate.AddDays(-1);
            var previousBalance = await GetDailyBalanceAsync(previousDay);
            decimal previousBalanceAmount = previousBalance?.FinalBalance ?? 0;
            
            decimal finalBalance = previousBalanceAmount + totalCredits - totalDebits;
            
            var dailyBalance = new DailyBalance(
                id: 0, 
                balanceDate: normalizedDate,
                finalBalance: finalBalance,
                previousBalance: previousBalanceAmount,
                totalCredits: totalCredits,
                totalDebits: totalDebits,
                createdAt: DateTime.UtcNow,
                updatedAt: DateTime.UtcNow
            );
            
            var savedBalance = await _dailyBalanceRepository.SaveAsync(dailyBalance);
            
            return savedBalance;
        }

        public async Task<DailyBalance?> GetDailyBalanceAsync(DateTime date)
        {
            var normalizedDate = date.Date;
            
            var dailyBalances = await _dailyBalanceRepository.GetAllAsync();
            return dailyBalances.FirstOrDefault(b => b.BalanceDate?.Date == normalizedDate);
        }

        public async Task<IEnumerable<DailyBalance>> GetDailyBalancesByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            var normalizedStartDate = startDate.Date;
            var normalizedEndDate = endDate.Date;
            

            var dailyBalances = await _dailyBalanceRepository.GetAllAsync();
            return dailyBalances.Where(b => 
                b.BalanceDate?.Date >= normalizedStartDate && 
                b.BalanceDate?.Date <= normalizedEndDate);
        }
    }
} 