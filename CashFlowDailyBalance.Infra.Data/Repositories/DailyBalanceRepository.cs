using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashFlowDailyBalance.Domain.Entities;
using CashFlowDailyBalance.Domain.Interfaces;
using CashFlowDailyBalance.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace CashFlowDailyBalance.Infra.Data.Repositories
{
    public class DailyBalanceRepository : IDailyBalanceRepository
    {
        private readonly ApplicationDbContext _context;

        public DailyBalanceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DailyBalance>> GetAllAsync()
        {
            return await _context.Set<DailyBalance>().ToListAsync();
        }

        public async Task<DailyBalance?> GetByDateAsync(DateTime date)
        {
            // Certifique-se que a data está em UTC
            var normalizedDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            
            return await _context.Set<DailyBalance>()
                .FirstOrDefaultAsync(b => b.BalanceDate!.Value.Date == normalizedDate.Date);
        }

        public async Task<IEnumerable<DailyBalance>> GetByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            // Certifique-se que as datas estão em UTC
            var normalizedStartDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            var normalizedEndDate = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc);
            
            return await _context.Set<DailyBalance>()
                .Where(b => b.BalanceDate!.Value.Date >= normalizedStartDate.Date && b.BalanceDate.Value.Date <= normalizedEndDate.Date)
                .OrderBy(b => b.BalanceDate)
                .ToListAsync();
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
            return existingBalance ?? dailyBalance;
        }
    }
}
