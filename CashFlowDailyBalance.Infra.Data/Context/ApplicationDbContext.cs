using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashFlowDailyBalance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashFlowDailyBalance.Infra.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<DailyBalance> DailyBalances { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("transactions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();

                entity.Property(e => e.Amount)
                    .HasColumnName("amount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(200)
                    .IsRequired(false);

                entity.Property(e => e.Origin)
                    .HasColumnName("origin")
                    .HasMaxLength(100)
                    .IsRequired(false);

                entity.Property(e => e.TransactionDate)
                    .HasColumnName("transaction_date")
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .IsRequired();
            });

            modelBuilder.Entity<DailyBalance>(entity =>
            {
                entity.ToTable("daily_balances");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();

                entity.Property(e => e.BalanceDate)
                    .HasColumnName("balance_date")
                    .IsRequired();

                entity.Property(e => e.FinalBalance)
                    .HasColumnName("final_balance")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.PreviousBalance)
                    .HasColumnName("previous_balance")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.TotalCredits)
                    .HasColumnName("total_credits")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.TotalDebits)
                    .HasColumnName("total_debits")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired();

                // Índice único para garantir que não haja balanços duplicados para a mesma data
                entity.HasIndex(e => e.BalanceDate).IsUnique();
            });
        }
    }
}
