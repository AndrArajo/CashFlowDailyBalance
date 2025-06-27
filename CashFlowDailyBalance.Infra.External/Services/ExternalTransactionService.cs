using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CashFlowDailyBalance.Domain.Entities;
using CashFlowDailyBalance.Domain.Interfaces;
using CashFlowDailyBalance.Infra.External.DTOs;

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
            _baseUrl = configuration["TRANSACTION_API_URL"] 
                       ?? configuration["TransactionApiUrl"] 
                       ?? configuration["ConnectionStrings:TransactionApiUrl"]
                       ?? "cashflow-transaction";

            // Não define BaseAddress aqui pois o HttpClient pode ser compartilhado
            if (_httpClient.BaseAddress == null && !string.IsNullOrEmpty(_baseUrl))
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
            }
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Buscando todas as transações da API externa");
                
                var allTransactions = await GetAllPagesAsync("/api/Transaction");
                
                _logger.LogInformation("Foram encontradas {Count} transações", allTransactions.Count());
                
                return allTransactions;
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

        private async Task<List<Transaction>> GetAllPagesAsync(string endpoint)
        {
            var allTransactions = new List<Transaction>();
            var pageNumber = 1;
            bool hasMorePages = true;

            while (hasMorePages)
            {
                var pageEndpoint = $"{endpoint}?pageNumber={pageNumber}&pageSize=100";
                var url = _httpClient.BaseAddress != null ? pageEndpoint : $"{_baseUrl}{pageEndpoint}";
                
                _logger.LogInformation("Buscando página {PageNumber} do endpoint {Endpoint}", pageNumber, endpoint);
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponseDto<PaginatedDataDto<TransactionDto>>>();
                
                if (apiResponse?.Success == true && apiResponse.Data?.Items != null)
                {
                    var transactions = apiResponse.Data.Items.Select(dto => dto.ToTransaction()).ToList();
                    allTransactions.AddRange(transactions);
                    
                    hasMorePages = apiResponse.Data.HasNextPage;
                    pageNumber++;
                    
                    _logger.LogInformation("Página {PageNumber}: {Count} transações encontradas", 
                        pageNumber - 1, transactions.Count);
                }
                else
                {
                    hasMorePages = false;
                    if (apiResponse?.Success != true)
                    {
                        _logger.LogWarning("API retornou success=false: {Message}", apiResponse?.Message);
                    }
                }
            }

            return allTransactions;
        }

        public async Task<IEnumerable<Transaction>> GetByDateAsync(DateTime date)
        {
            try
            {
                _logger.LogInformation("Buscando transações da API externa para a data {Date}", date.Date);
                
                var dateString = date.ToString("yyyy-MM-dd");
                var endpoint = $"/api/Transaction?date={dateString}";
                var allTransactions = await GetAllPagesAsync(endpoint);
                
                _logger.LogInformation("Foram encontradas {Count} transações para a data {Date}", 
                    allTransactions.Count(), date.Date);
                
                return allTransactions;
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
                var endpoint = $"/api/Transaction?startDate={startDateString}&endDate={endDateString}";
                var allTransactions = await GetAllPagesAsync(endpoint);
                
                _logger.LogInformation("Foram encontradas {Count} transações para o período {StartDate} - {EndDate}", 
                    allTransactions.Count(), startDate.Date, endDate.Date);
                
                return allTransactions;
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