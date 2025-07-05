using CashFlowDailyBalance.Application.DTOs;
using CashFlowDailyBalance.Application.Interfaces;
using CashFlowDailyBalance.Domain.Entities;
using CashFlowDailyBalance.Domain.Enums;
using CashFlowDailyBalance.Domain.Interfaces;

namespace CashFlowDailyBalance.Application.Services
{
    public class DailyBalanceService : IDailyBalanceService
    {
        private readonly IExternalTransactionService _externalTransactionService;
        private readonly IDailyBalanceRepository _dailyBalanceRepository;

        public DailyBalanceService(IExternalTransactionService externalTransactionService, IDailyBalanceRepository dailyBalanceRepository)
        {
            _externalTransactionService = externalTransactionService;
            _dailyBalanceRepository = dailyBalanceRepository;
        }

        public async Task<DailyBalance> ProcessDailyBalanceAsync(DateTime date)
        {
            var normalizedDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            
            var transactions = await _externalTransactionService.GetByDateAsync(normalizedDate);
            
            decimal totalCredits = transactions
                .Where(t => t.Type == TransactionType.Credit)
                .Sum(t => t.Amount);
                
            decimal totalDebits = transactions
                .Where(t => t.Type == TransactionType.Debit)
                .Sum(t => t.Amount);
            
            var previousDay = normalizedDate.AddDays(-1);
            var previousBalance           = await GetDailyBalanceAsync(previousDay);
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
            var normalizedDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            
            var dailyBalances = await _dailyBalanceRepository.GetAllAsync();
            return dailyBalances.FirstOrDefault(b => b.BalanceDate?.Date == normalizedDate.Date);
        }

        public async Task<IEnumerable<DailyBalance>> GetDailyBalancesByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            var normalizedStartDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            var normalizedEndDate = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc);
            
            var dailyBalances = await _dailyBalanceRepository.GetAllAsync();
            return dailyBalances.Where(b => 
                b.BalanceDate?.Date >= normalizedStartDate.Date && 
                b.BalanceDate?.Date <= normalizedEndDate.Date);
        }
        
        public async Task<PaginatedResponseDto<DailyBalanceDto>> GetDailyBalancesAsync(int pageNumber, int pageSize)
        {
            // Validar e normalizar valores de paginação
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;
            pageSize = pageSize > 100 ? 100 : pageSize; // Limitar máximo
            
            var (items, totalCount) = await _dailyBalanceRepository.GetPaginatedAsync(pageNumber, pageSize);
            
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            // Mapear manualmente para DTOs
            var dailyBalanceDtos = items.Select(db => new DailyBalanceDto(
                db.BalanceDate ?? DateTime.MinValue,
                db.PreviousBalance,
                db.TotalCredits,
                db.TotalDebits,
                db.FinalBalance
            )).ToList();
            
            // Retornar resultado paginado com valores normalizados
            return new PaginatedResponseDto<DailyBalanceDto>(
                dailyBalanceDtos,
                pageNumber,
                pageSize,
                totalCount,
                totalPages);
        }
    }
} 