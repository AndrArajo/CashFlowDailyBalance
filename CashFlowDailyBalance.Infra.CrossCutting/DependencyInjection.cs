using CashFlowDailyBalance.Infra.CrossCutting.Caching;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlowDailyBalance.Infra.CrossCutting
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCrossCuttingServices(this IServiceCollection services)
        {
            // Adicionar serviço de cache
            services.AddSingleton<ICacheService, RedisCacheService>();
            
            return services;
        }
    }
} 