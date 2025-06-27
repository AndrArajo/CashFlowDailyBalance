using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CashFlowDailyBalance.Domain.Entities;
using CashFlowDailyBalance.Domain.Interfaces;

namespace CashFlowDailyBalance.Infra.External.Services
{
    public class ExternalTransactionService : IExternalTransactionService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExternalTransactionService> _logger;
        private readonly string _baseUrl;

        public ExternalTransactionService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ExternalTransactionService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = configuration.GetConnectionString("TransactionApiUrl") 
                       ?? throw new ArgumentNullException(nameof(configuration), "TransactionApiUrl não configurada");

            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Buscando todas as transações da API externa");
                
                var response = await _httpClient.GetAsync("/api/Transactions");
                response.EnsureSuccessStatusCode();

                var transactions = await response.Content.ReadFromJsonAsync<IEnumerable<Transaction>>();
                
                _logger.LogInformation("Foram encontradas {Count} transações", transactions?.Count() ?? 0);
                
                return transactions ?? Enumerable.Empty<Transaction>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro ao fazer requisição para a API de transações");
                throw new InvalidOperationException("Erro na comunicação com a API de transações", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao buscar transações");
                throw;
            }
        }

        public async Task<IEnumerable<Transaction>> GetByDateAsync(DateTime date)
        {
            try
            {
                _logger.LogInformation("Buscando transações da API externa para a data {Date}", date.Date);
                
                var dateString = date.ToString("yyyy-MM-dd");
                var response = await _httpClient.GetAsync($"/api/Transactions?date={dateString}");
                response.EnsureSuccessStatusCode();

                var transactions = await response.Content.ReadFromJsonAsync<IEnumerable<Transaction>>();
                
                _logger.LogInformation("Foram encontradas {Count} transações para a data {Date}", 
                    transactions?.Count() ?? 0, date.Date);
                
                return transactions ?? Enumerable.Empty<Transaction>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro ao fazer requisição para a API de transações para a data {Date}", date);
                throw new InvalidOperationException("Erro na comunicação com a API de transações", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao buscar transações para a data {Date}", date);
                throw;
            }
        }

        public async Task<IEnumerable<Transaction>> GetByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Buscando transações da API externa para o período {StartDate} - {EndDate}", 
                    startDate.Date, endDate.Date);
                
                var startDateString = startDate.ToString("yyyy-MM-dd");
                var endDateString = endDate.ToString("yyyy-MM-dd");
                var response = await _httpClient.GetAsync($"/api/Transactions?startDate={startDateString}&endDate={endDateString}");
                response.EnsureSuccessStatusCode();

                var transactions = await response.Content.ReadFromJsonAsync<IEnumerable<Transaction>>();
                
                _logger.LogInformation("Foram encontradas {Count} transações para o período {StartDate} - {EndDate}", 
                    transactions?.Count() ?? 0, startDate.Date, endDate.Date);
                
                return transactions ?? Enumerable.Empty<Transaction>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro ao fazer requisição para a API de transações para o período {StartDate} - {EndDate}", 
                    startDate, endDate);
                throw new InvalidOperationException("Erro na comunicação com a API de transações", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao buscar transações para o período {StartDate} - {EndDate}", 
                    startDate, endDate);
                throw;
            }
        }
    }
} 