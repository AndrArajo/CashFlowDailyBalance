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
            return await _context.Set<DailyBalance>()
                .FirstOrDefaultAsync(b => b.BalanceDate!.Value.Date == date.Date);
        }

        public async Task<IEnumerable<DailyBalance>> GetByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Set<DailyBalance>()
                .Where(b => b.BalanceDate!.Value.Date >= startDate.Date && b.BalanceDate.Value.Date <= endDate.Date)
                .OrderBy(b => b.BalanceDate)
                .ToListAsync();
        }

        public async Task<DailyBalance> SaveAsync(DailyBalance dailyBalance)
        {
            var existingBalance = await GetByDateAsync(dailyBalance.BalanceDate!.Value);
            
            if (existingBalance == null)
            {
                dailyBalance.CreatedAt = DateTime.UtcNow;
                dailyBalance.UpdatedAt = DateTime.UtcNow;
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
