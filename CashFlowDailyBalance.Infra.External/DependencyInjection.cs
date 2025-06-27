using Microsoft.Extensions.DependencyInjection;
using CashFlowDailyBalance.Domain.Interfaces;
using CashFlowDailyBalance.Infra.External.Services;

namespace CashFlowDailyBalance.Infra.External
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddExternalServices(this IServiceCollection services)
        {
            // Registra HttpClient para o serviço de transações externas
            services.AddHttpClient<IExternalTransactionService, ExternalTransactionService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            return services;
        }
    }
} 