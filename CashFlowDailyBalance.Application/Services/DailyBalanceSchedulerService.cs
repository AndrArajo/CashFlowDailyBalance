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

            while (!stoppingToken.IsCancellationRequested)
            {
                if (await _semaphore.WaitAsync(0))
                {
                    try
                    {
                        
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var dailyBalanceService = scope.ServiceProvider.GetRequiredService<IDailyBalanceService>();
                            
                            var today = DateTime.UtcNow.Date;
                            await dailyBalanceService.ProcessDailyBalanceAsync(today);
                            
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