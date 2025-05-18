using CashFlowDailyBalance.Application.DTOs;
using CashFlowDailyBalance.Application.Interfaces;
using CashFlowDailyBalance.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<DailyBalance>> GetDailyBalance(DateTime date)
        {
            try
            {
                var dailyBalance = await _dailyBalanceService.GetDailyBalanceAsync(date);
                if (dailyBalance == null)
                    return NotFound($"Balanço para a data {date:dd/MM/yyyy} não encontrado");

                return Ok(dailyBalance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar balanço diário");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpGet("period")]
        public async Task<ActionResult<IEnumerable<DailyBalance>>> GetDailyBalancesByPeriod(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                    return BadRequest("A data inicial não pode ser maior que a data final");

                var dailyBalances = await _dailyBalanceService.GetDailyBalancesByPeriodAsync(startDate, endDate);
                return Ok(dailyBalances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar balanços diários por período");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpGet("paginated")]
        public async Task<ActionResult<PaginatedResponseDto<DailyBalance>>> GetPaginatedDailyBalances(
            [FromQuery] int page = 1, 
            [FromQuery] int size = 10)
        {
            try
            {
                size = size > 10 ? 10 : size;
                
                var (items, totalCount, totalPages) = await _dailyBalanceService.GetPaginatedDailyBalancesAsync(page, size);
                
                var response = new PaginatedResponseDto<DailyBalance>(
                    items: items,
                    pageNumber: page,
                    pageSize: size,
                    totalCount: totalCount,
                    totalPages: totalPages
                );
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar balanços diários paginados");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpPost("process/{date}")]
        public async Task<ActionResult<DailyBalance>> ProcessDailyBalance(DateTime date)
        {
            try
            {
                var processedBalance = await _dailyBalanceService.ProcessDailyBalanceAsync(date);
                return Ok(processedBalance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar balanço diário");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpPost("process-range")]
        public async Task<ActionResult<IEnumerable<DailyBalance>>> ProcessDailyBalanceRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                    return BadRequest("A data inicial não pode ser maior que a data final");

                var processedBalances = new List<DailyBalance>();
                var currentDate = startDate;

                while (currentDate <= endDate)
                {
                    var dailyBalance = await _dailyBalanceService.ProcessDailyBalanceAsync(currentDate);
                    processedBalances.Add(dailyBalance);
                    currentDate = currentDate.AddDays(1);
                }

                return Ok(processedBalances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar balanços diários por período");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpGet("summary/{date}")]
        public async Task<ActionResult<DailyBalanceDto>> GetDailyBalanceSummary(DateTime date)
        {
            try
            {
                var dailyBalance = await _dailyBalanceService.GetDailyBalanceAsync(date);
                if (dailyBalance == null)
                    return NotFound($"Balanço para a data {date:dd/MM/yyyy} não encontrado");

                var dailyBalanceDto = new DailyBalanceDto(
                    date: dailyBalance.BalanceDate ?? DateTime.MinValue,
                    previousBalance: dailyBalance.PreviousBalance,
                    totalCredits: dailyBalance.TotalCredits,
                    totalDebits: dailyBalance.TotalDebits,
                    finalBalance: dailyBalance.FinalBalance
                );

                return Ok(dailyBalanceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar resumo do balanço diário");
                return StatusCode(500, "Erro interno do servidor");
            }
        }
    }
} 