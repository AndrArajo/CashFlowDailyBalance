using Microsoft.Extensions.DependencyInjection;
using CashFlowDailyBalance.Domain.Interfaces;
using CashFlowDailyBalance.Infra.External.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

            // Configuração do cliente gRPC com logs detalhados
            services.AddGrpcClient<CashFlowTransactions.GrpcService.TransactionService.TransactionServiceClient>((serviceProvider, options) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var logger = serviceProvider.GetRequiredService<ILogger<ExternalTransactionService>>();

                // Busca a URL do serviço gRPC usando a mesma lógica do ExternalTransactionService
                var grpcUrl = configuration["TRANSACTION_GRPC_URL"]
                           ?? configuration["TransactionGrpcUrl"]
                           ?? configuration["ConnectionStrings:TransactionGrpcUrl"]
                           ?? "http://cashflow-transaction:5052"; // Porta padrão gRPC

                logger.LogInformation("Configurando cliente gRPC para URL: {GrpcUrl}", grpcUrl);

                options.Address = new Uri(grpcUrl);
            })
            .ConfigureChannel(options =>
            {
                // Configurações avançadas de canal para logs e performance
                options.LoggerFactory = options.ServiceProvider.GetRequiredService<ILoggerFactory>();

                // Configurar timeout e keep-alive
                options.HttpHandler = new SocketsHttpHandler()
                {
                    PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                    EnableMultipleHttp2Connections = true
                };

                // Habilitar logs detalhados
                options.ThrowOperationCanceledOnCancellation = true;
            });
           

            // Registra o serviço de transações externas
            services.AddScoped<IExternalTransactionService, ExternalTransactionService>();

            return services;
        }
    }
} 