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
            return await _context.Transactions
                .Where(t => t.TransactionDate.Date == date.Date)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Transactions
                .Where(t => t.TransactionDate.Date >= startDate.Date && t.TransactionDate.Date <= endDate.Date)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }
    }
}
