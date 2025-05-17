namespace CashFlowDailyBalance.Application.DTOs
{
    public class DailyBalanceDto
    {
        public DateTime Date { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal TotalCredits { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal FinalBalance { get; set; }
        
        public DailyBalanceDto(DateTime date, decimal previousBalance, decimal totalCredits, decimal totalDebits, decimal finalBalance)
        {
            Date = date;
            PreviousBalance = previousBalance;
            TotalCredits = totalCredits;
            TotalDebits = totalDebits;
            FinalBalance = finalBalance;
        }
    }
} 