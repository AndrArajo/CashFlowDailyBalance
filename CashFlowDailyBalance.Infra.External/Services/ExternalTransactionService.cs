using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CashFlowDailyBalance.Domain.Entities;
using CashFlowDailyBalance.Domain.Interfaces;
using CashFlowDailyBalance.Infra.External.DTOs;
using System.ComponentModel.DataAnnotations;

namespace CashFlowDailyBalance.Infra.External.Services
{
    public class ExternalTransactionService : IExternalTransactionService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExternalTransactionService> _logger;
        private readonly string _baseUrl;
        private readonly CashFlowTransactions.GrpcService.TransactionService.TransactionServiceClient _transactionServiceClient;

        public ExternalTransactionService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ExternalTransactionService> logger,
            CashFlowTransactions.GrpcService.TransactionService.TransactionServiceClient transactionServiceClient)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = configuration["TRANSACTION_API_URL"]
                       ?? configuration["TransactionApiUrl"]
                       ?? configuration["ConnectionStrings:TransactionApiUrl"]
                       ?? "http://cashflow-transaction";

            // Não define BaseAddress aqui pois o HttpClient pode ser compartilhado
            if (_httpClient.BaseAddress == null && !string.IsNullOrEmpty(_baseUrl))
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
            }

            _transactionServiceClient = transactionServiceClient;
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

            _logger.LogInformation("Iniciando busca paginada via gRPC - Endpoint: {Endpoint}", endpoint);

            while (hasMorePages)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                try
                {
                    var request = new CashFlowTransactions.GrpcService.GetTransactionsRequest
                    {
                        PageNumber = pageNumber,
                        PageSize = 100
                    };
                    
                    _logger.LogDebug("Executando chamada gRPC GetTransactions - Página: {PageNumber}, PageSize: {PageSize}", 
                        pageNumber, request.PageSize);
                    
                    var response = await _transactionServiceClient.GetTransactionsAsync(request);
                    
                    stopwatch.Stop();
                    
                    _logger.LogDebug("Chamada gRPC concluída em {ElapsedMs}ms - Página: {PageNumber}, Itens retornados: {ItemCount}, Tem próxima página: {HasNextPage}", 
                        stopwatch.ElapsedMilliseconds, pageNumber, response.Items.Count, response.HasNextPage);

                    if (response.Items.Count > 0)
                    {
                        var transctions = response.Items.Select(dto => new Transaction
                        {
                            Id = dto.Id,
                            Description = dto.Description, 
                            Amount = (decimal) dto.Amount,
                            CreatedAt = dto.CreatedAt.ToDateTime(),
                            Origin = dto.Origin,
                            TransactionDate = dto.TransactionDate.ToDateTime(),
                            Type = (Domain.Enums.TransactionType) dto.Type,
                           
                        });
                        allTransactions.AddRange(transctions);

                        hasMorePages = response.HasNextPage;
                        pageNumber++;

                        _logger.LogDebug("Processadas {TransactionCount} transações da página {CurrentPage}",
                            transctions.Count(), pageNumber - 1);
                    }
                    else
                    {
                        hasMorePages = false;
                        _logger.LogDebug("Nenhum item retornado na página {PageNumber}, finalizando busca", pageNumber);
                    }
                }
                catch (Grpc.Core.RpcException ex)
                {
                    stopwatch.Stop();
                    hasMorePages = false;
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    hasMorePages = false;
                    _logger.LogError(ex, "Erro inesperado ao buscar página {PageNumber} em {ElapsedMs}ms",
                         pageNumber, stopwatch.ElapsedMilliseconds);
                    throw;
                }
            }

            _logger.LogInformation("Busca paginada via gRPC concluída - Total de transações: {TotalCount}, Páginas processadas: {PagesProcessed}", 
                allTransactions.Count, pageNumber - 1);

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