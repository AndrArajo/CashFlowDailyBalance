namespace CashFlowDailyBalance.Application.DTOs
{
    public class DailyBalanceDto
    {
        public DateTime Data { get; set; }
        public decimal SaldoAnterior { get; set; }
        public decimal TotalCreditos { get; set; }
        public decimal TotalDebitos { get; set; }
        public decimal SaldoFinal { get; set; }
        
        public DailyBalanceDto(DateTime data, decimal saldoAnterior, decimal totalCreditos, decimal totalDebitos, decimal saldoFinal)
        {
            Data = data;
            SaldoAnterior = saldoAnterior;
            TotalCreditos = totalCreditos;
            TotalDebitos = totalDebitos;
            SaldoFinal = saldoFinal;
        }
    }
} 