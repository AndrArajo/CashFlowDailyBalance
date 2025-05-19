using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashFlowDailyBalance.Domain.Enums;

namespace CashFlowDailyBalance.Domain.Entities
{
    public sealed class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [MaxLength(200)]
        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("amount", TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        public decimal Amount { get; set; }

        [Required]
        [Column("type")]
        [EnumDataType(typeof(TransactionType))]
        public TransactionType Type { get; set; }

        [MaxLength(100)]
        [Column("origin")]
        public string? Origin { get; set; }

        [Required]
        [Column("transaction_date")]
        public DateTime TransactionDate { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;



        [MaxLength(100)]
        [Column("message_id")]
        public string? MessageId { get; set; }

        // Construtor para Entity Framework
        public Transaction()
        {
        }

        public Transaction(decimal amount, TransactionType type, DateTime transactionDate, string? description = null, string? origin = null)
        {
            Amount = amount;
            Type = type;
            TransactionDate = transactionDate;
            Description = description;
            Origin = origin;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
