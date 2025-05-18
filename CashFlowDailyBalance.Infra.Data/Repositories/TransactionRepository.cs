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
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            return await _context.Transactions
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByDateAsync(DateTime date)
        {
            // Certifique-se que a data está em UTC
            var normalizedDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            
            return await _context.Transactions
                    .Where(t => t.TransactionDate.Date == normalizedDate.Date)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            // Certifique-se que as datas estão em UTC
            var normalizedStartDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            var normalizedEndDate = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc);
            
            return await _context.Transactions
                    .Where(t => t.TransactionDate.Date >= normalizedStartDate.Date && 
                               t.TransactionDate.Date <= normalizedEndDate.Date)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
        }
    }
}
