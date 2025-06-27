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
            services.AddHttpClient<IExternalTransactionService, ExternalTransactionService>((serviceProvider, client) =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                
                // Configurar User-Agent e headers padrão
                client.DefaultRequestHeaders.Add("User-Agent", "CashFlowDailyBalance/1.0");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            return services;
        }
    }
} 