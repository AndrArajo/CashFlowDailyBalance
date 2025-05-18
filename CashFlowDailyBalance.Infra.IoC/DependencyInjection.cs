using CashFlowDailyBalance.Application.Interfaces;
using CashFlowDailyBalance.Application.Services;
using CashFlowDailyBalance.Domain.Interfaces;
using CashFlowDailyBalance.Infra.CrossCutting;
using CashFlowDailyBalance.Infra.Data.Context;
using CashFlowDailyBalance.Infra.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace CashFlowDailyBalance.Infra.IoC
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuração do banco de dados
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            Debug.WriteLine($"Connection string: {connectionString}");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            // Configuração explícita do DbContext com PostgreSQL
            services.AddDbContext<ApplicationDbContext>(options => 
            {
                options.UseNpgsql(connectionString);
            }, ServiceLifetime.Scoped);

            // Configuração dos serviços de infraestrutura transversal (cache, etc.)
            services.AddCrossCuttingServices();

            // Repositories
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IDailyBalanceRepository, DailyBalanceRepository>();

            // Services
            services.AddScoped<IDailyBalanceService, DailyBalanceService>();
            
            // Scheduled background service
            services.AddHostedService<DailyBalanceSchedulerService>();
            
            return services;
        }
    }
} 