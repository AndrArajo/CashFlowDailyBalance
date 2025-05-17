using System;
using System.Threading;
using System.Threading.Tasks;
using CashFlowDailyBalance.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CashFlowDailyBalance.Application.Services
{
    public class DailyBalanceSchedulerService : BackgroundService
    {
        private readonly ILogger<DailyBalanceSchedulerService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly TimeSpan _interval = TimeSpan.FromHours(1);

        public DailyBalanceSchedulerService(
            ILogger<DailyBalanceSchedulerService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serviço de agendamento de balanço diário iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                // Verifica se já existe uma execução em andamento
                if (await _semaphore.WaitAsync(0))
                {
                    try
                    {
                        _logger.LogInformation("Iniciando processamento de balanço diário: {time}", DateTimeOffset.Now);
                        
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var dailyBalanceService = scope.ServiceProvider.GetRequiredService<IDailyBalanceService>();
                            
                            // Processa o balanço para o dia atual, usando UTC
                            var today = DateTime.UtcNow.Date;
                            await dailyBalanceService.ProcessDailyBalanceAsync(today);
                            
                            _logger.LogInformation("Processamento de balanço diário concluído com sucesso para: {date}", today);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro durante o processamento de balanço diário");
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }
                else
                {
                    _logger.LogWarning("Uma execução do processamento de balanço diário já está em andamento. Pulando esta execução.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
} 