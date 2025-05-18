using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashFlowDailyBalance.Domain.Entities;
using CashFlowDailyBalance.Domain.Interfaces;
using CashFlowDailyBalance.Infra.CrossCutting.Caching;
using CashFlowDailyBalance.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace CashFlowDailyBalance.Infra.Data.Repositories
{
    public class DailyBalanceRepository : IDailyBalanceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cacheService;
        private const string CACHE_KEY_PREFIX = "dailybalance_";

        public DailyBalanceRepository(ApplicationDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<DailyBalance>> GetAllAsync()
        {
            string cacheKey = $"{CACHE_KEY_PREFIX}all";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () => 
            {
                return await _context.Set<DailyBalance>().ToListAsync();
            });
        }

        public async Task<DailyBalance?> GetByDateAsync(DateTime date)
        {
            // Certifique-se que a data está em UTC
            var normalizedDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            string cacheKey = $"{CACHE_KEY_PREFIX}date_{normalizedDate:yyyyMMdd}";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () => 
            {
                return await _context.Set<DailyBalance>()
                    .FirstOrDefaultAsync(b => b.BalanceDate!.Value.Date == normalizedDate.Date);
            });
        }

        public async Task<IEnumerable<DailyBalance>> GetByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            var normalizedStartDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            var normalizedEndDate = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc);
            string cacheKey = $"{CACHE_KEY_PREFIX}period_{normalizedStartDate:yyyyMMdd}_{normalizedEndDate:yyyyMMdd}";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () => 
            {
                return await _context.Set<DailyBalance>()
                    .Where(b => b.BalanceDate!.Value.Date >= normalizedStartDate.Date && 
                              b.BalanceDate.Value.Date <= normalizedEndDate.Date)
                    .OrderBy(b => b.BalanceDate)
                    .ToListAsync();
            });
        }

        public async Task<(IEnumerable<DailyBalance> Items, int TotalCount)> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;
            string cacheKey = $"{CACHE_KEY_PREFIX}paginated_{pageNumber}_{pageSize}";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () => 
            {
                var totalCount = await _context.Set<DailyBalance>().CountAsync();

                var items = await _context.Set<DailyBalance>()
                    .OrderByDescending(b => b.BalanceDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            });
        }

        public async Task<DailyBalance> SaveAsync(DailyBalance dailyBalance)
        {
            // Certifique-se que as datas estão em UTC
            if (dailyBalance.BalanceDate.HasValue)
            {
                dailyBalance.BalanceDate = DateTime.SpecifyKind(dailyBalance.BalanceDate.Value.Date, DateTimeKind.Utc);
            }
            dailyBalance.CreatedAt = DateTime.UtcNow;
            dailyBalance.UpdatedAt = DateTime.UtcNow;
            
            var existingBalance = await GetByDateAsync(dailyBalance.BalanceDate!.Value);
            
            if (existingBalance == null)
            {
                await _context.Set<DailyBalance>().AddAsync(dailyBalance);
            }
            else
            {
                existingBalance.FinalBalance = dailyBalance.FinalBalance;
                existingBalance.PreviousBalance = dailyBalance.PreviousBalance;
                existingBalance.TotalCredits = dailyBalance.TotalCredits;
                existingBalance.TotalDebits = dailyBalance.TotalDebits;
                existingBalance.UpdatedAt = DateTime.UtcNow;
                _context.Set<DailyBalance>().Update(existingBalance);
            }
            
            await _context.SaveChangesAsync();
            
            // Invalidate related cache keys after saving
            await InvalidateCacheAsync(dailyBalance.BalanceDate!.Value);
            
            return existingBalance ?? dailyBalance;
        }
        
        private async Task InvalidateCacheAsync(DateTime date)
        {
            // Invalidate specific date cache
            var dateKey = $"{CACHE_KEY_PREFIX}date_{date:yyyyMMdd}";
            await _cacheService.RemoveAsync(dateKey);
            
            // Invalidate all cache
            await _cacheService.RemoveAsync($"{CACHE_KEY_PREFIX}all");
            
    
        }
    }
}
