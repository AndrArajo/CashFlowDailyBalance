using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CashFlowDailyBalance.Domain.Entities
{
    public sealed class DailyBalance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("balance_date")]
        public DateTime? BalanceDate { get; set; }

        [Required]
        [Column("final_balance", TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        public decimal FinalBalance { get; set; }

        [Required]
        [Column("final_balance", TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        public decimal PreviousBalance { get; set; }

        [Required]
        [Column("total_credits", TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        public decimal TotalCredits { get; set; }

        [Required]
        [Column("total_debits", TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        public decimal TotalDebits { get; set; }


        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DailyBalance(int id, DateTime? balanceDate, decimal finalBalance, decimal previousBalance, decimal totalCredits, decimal totalDebits, DateTime createdAt, DateTime updatedAt)
        {
            Id = id;
            BalanceDate = balanceDate;
            FinalBalance = finalBalance;
            PreviousBalance = previousBalance;
            TotalCredits = totalCredits;
            TotalDebits = totalDebits;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }
    }
}
