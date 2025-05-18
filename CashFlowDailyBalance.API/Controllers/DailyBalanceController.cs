using CashFlowDailyBalance.API.Models;
using CashFlowDailyBalance.Application.DTOs;
using CashFlowDailyBalance.Application.Interfaces;
using CashFlowDailyBalance.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CashFlowDailyBalance.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DailyBalanceController : ControllerBase
    {
        private readonly ILogger<DailyBalanceController> _logger;
        private readonly IDailyBalanceService _dailyBalanceService;

        public DailyBalanceController(
            ILogger<DailyBalanceController> logger,
            IDailyBalanceService dailyBalanceService)
        {
            _logger = logger;
            _dailyBalanceService = dailyBalanceService;
        }

        [HttpGet("{date}")]
        [ProducesResponseType(typeof(ApiResponse<DailyBalance>), 200)]
        [ProducesResponseType(typeof(ApiResponse<DailyBalance>), 404)]
        [ProducesResponseType(typeof(ApiResponse<DailyBalance>), 500)]
        public async Task<ActionResult<ApiResponse<DailyBalance>>> GetDailyBalance(DateTime date)
        {
            try
            {
                var dailyBalance = await _dailyBalanceService.GetDailyBalanceAsync(date);
                if (dailyBalance == null)
                {
                    var notFoundResponse = ApiResponse<DailyBalance>.NotFound($"Balanço para a data {date:dd/MM/yyyy} não encontrado");
                    return NotFound(notFoundResponse);
                }

                var response = ApiResponse<DailyBalance>.Ok(dailyBalance, $"Balanço do dia {date:dd/MM/yyyy} obtido com sucesso");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar balanço diário");
                var errorResponse = ApiResponse<DailyBalance>.Error("Erro interno do servidor");
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("period")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DailyBalance>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DailyBalance>>), 400)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DailyBalance>>), 500)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DailyBalance>>>> GetDailyBalancesByPeriod(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    var badRequestResponse = ApiResponse<IEnumerable<DailyBalance>>.BadRequest("A data inicial não pode ser maior que a data final");
                    return BadRequest(badRequestResponse);
                }

                var dailyBalances = await _dailyBalanceService.GetDailyBalancesByPeriodAsync(startDate, endDate);
                var response = ApiResponse<IEnumerable<DailyBalance>>.Ok(
                    dailyBalances, 
                    $"Balanços diários no período de {startDate:dd/MM/yyyy} a {endDate:dd/MM/yyyy} obtidos com sucesso"
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar balanços diários por período");
                var errorResponse = ApiResponse<IEnumerable<DailyBalance>>.Error("Erro interno do servidor");
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("paginated")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<DailyBalance>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<DailyBalance>>), 500)]
        public async Task<ActionResult<ApiResponse<PaginatedResponseDto<DailyBalance>>>> GetPaginatedDailyBalances(
            [FromQuery] int page = 1, 
            [FromQuery] int size = 10)
        {
            try
            {
                // Limitar o tamanho máximo a 10
                size = size > 10 ? 10 : size;
                
                var (items, totalCount, totalPages) = await _dailyBalanceService.GetPaginatedDailyBalancesAsync(page, size);
                
                var paginatedResponse = new PaginatedResponseDto<DailyBalance>(
                    items: items,
                    pageNumber: page,
                    pageSize: size,
                    totalCount: totalCount,
                    totalPages: totalPages
                );

                var response = ApiResponse<PaginatedResponseDto<DailyBalance>>.Ok(
                    paginatedResponse, 
                    $"Balanços diários paginados obtidos com sucesso. Página {page} de {totalPages}."
                );
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar balanços diários paginados");
                var errorResponse = ApiResponse<PaginatedResponseDto<DailyBalance>>.Error("Erro interno do servidor");
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("process/{date}")]
        [ProducesResponseType(typeof(ApiResponse<DailyBalance>), 200)]
        [ProducesResponseType(typeof(ApiResponse<DailyBalance>), 500)]
        public async Task<ActionResult<ApiResponse<DailyBalance>>> ProcessDailyBalance(DateTime date)
        {
            try
            {
                var processedBalance = await _dailyBalanceService.ProcessDailyBalanceAsync(date);
                var response = ApiResponse<DailyBalance>.Ok(
                    processedBalance, 
                    $"Balanço do dia {date:dd/MM/yyyy} processado com sucesso"
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar balanço diário");
                var errorResponse = ApiResponse<DailyBalance>.Error("Erro interno do servidor");
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("process-range")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DailyBalance>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DailyBalance>>), 400)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DailyBalance>>), 500)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DailyBalance>>>> ProcessDailyBalanceRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    var badRequestResponse = ApiResponse<IEnumerable<DailyBalance>>.BadRequest("A data inicial não pode ser maior que a data final");
                    return BadRequest(badRequestResponse);
                }

                var processedBalances = new List<DailyBalance>();
                var currentDate = startDate;

                while (currentDate <= endDate)
                {
                    var dailyBalance = await _dailyBalanceService.ProcessDailyBalanceAsync(currentDate);
                    processedBalances.Add(dailyBalance);
                    currentDate = currentDate.AddDays(1);
                }

                var response = ApiResponse<IEnumerable<DailyBalance>>.Ok(
                    processedBalances, 
                    $"Balanços diários processados com sucesso no período de {startDate:dd/MM/yyyy} a {endDate:dd/MM/yyyy}"
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar balanços diários por período");
                var errorResponse = ApiResponse<IEnumerable<DailyBalance>>.Error("Erro interno do servidor");
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("summary/{date}")]
        [ProducesResponseType(typeof(ApiResponse<DailyBalanceDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<DailyBalanceDto>), 404)]
        [ProducesResponseType(typeof(ApiResponse<DailyBalanceDto>), 500)]
        public async Task<ActionResult<ApiResponse<DailyBalanceDto>>> GetDailyBalanceSummary(DateTime date)
        {
            try
            {
                var dailyBalance = await _dailyBalanceService.GetDailyBalanceAsync(date);
                if (dailyBalance == null)
                {
                    var notFoundResponse = ApiResponse<DailyBalanceDto>.NotFound($"Balanço para a data {date:dd/MM/yyyy} não encontrado");
                    return NotFound(notFoundResponse);
                }

                var dailyBalanceDto = new DailyBalanceDto(
                    date: dailyBalance.BalanceDate ?? DateTime.MinValue,
                    previousBalance: dailyBalance.PreviousBalance,
                    totalCredits: dailyBalance.TotalCredits,
                    totalDebits: dailyBalance.TotalDebits,
                    finalBalance: dailyBalance.FinalBalance
                );

                var response = ApiResponse<DailyBalanceDto>.Ok(
                    dailyBalanceDto, 
                    $"Resumo do balanço do dia {date:dd/MM/yyyy} obtido com sucesso"
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar resumo do balanço diário");
                var errorResponse = ApiResponse<DailyBalanceDto>.Error("Erro interno do servidor");
                return StatusCode(500, errorResponse);
            }
        }
    }
} 