using CashFlowDailyBalance.Application.Interfaces;
using CashFlowDailyBalance.Application.Services;
using CashFlowDailyBalance.Domain.Interfaces;
using CashFlowDailyBalance.Infra.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlowDailyBalance.Infra.IoC
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IDailyBalanceRepository, DailyBalanceRepository>();

            // Services
            services.AddScoped<IDailyBalanceService, DailyBalanceService>();
            
            return services;
        }
    }
} 