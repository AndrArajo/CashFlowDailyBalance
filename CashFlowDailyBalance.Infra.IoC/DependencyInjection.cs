using CashFlowDailyBalance.Application.Interfaces;
using CashFlowDailyBalance.Application.Services;
using CashFlowDailyBalance.Domain.Interfaces;
using CashFlowDailyBalance.Infra.CrossCutting;
using CashFlowDailyBalance.Infra.Data.Context;
using CashFlowDailyBalance.Infra.Data.Repositories;
using CashFlowDailyBalance.Infra.External;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using Npgsql;

namespace CashFlowDailyBalance.Infra.IoC
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuração do banco de dados
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(
                configuration.GetConnectionString("DefaultConnection"))
            {
                MaxPoolSize = 100,         // Tamanho máximo do pool
                MinPoolSize = 10,          // Tamanho mínimo do pool
                ConnectionIdleLifetime = 60, // Manter conexão viva por 60 segundos
                Pooling = true,            // Habilitar pooling
                Timeout = 30               // Timeout em segundos
            };
            
            var connectionString = connectionStringBuilder.ToString();
            Debug.WriteLine($"Connection string: {connectionString}");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            // Configuração explícita do DbContext com PostgreSQL
            services.AddDbContext<ApplicationDbContext>(options => 
            {
                options.UseNpgsql(connectionString, npgsqlOptions => 
                {
                    // Configurar timeout de comando
                    npgsqlOptions.CommandTimeout(30);
                    
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                    
                    npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });
            }, ServiceLifetime.Scoped);

            // Configuração dos serviços de infraestrutura transversal (cache, etc.)
            services.AddCrossCuttingServices();

            // Repositories
            services.AddScoped<IDailyBalanceRepository, DailyBalanceRepository>();

            // External Services
            services.AddExternalServices();

            // Services
            services.AddScoped<IDailyBalanceService, DailyBalanceService>();
            
            // Scheduled background service
            services.AddHostedService<DailyBalanceSchedulerService>();
            
            return services;
        }
    }
} 