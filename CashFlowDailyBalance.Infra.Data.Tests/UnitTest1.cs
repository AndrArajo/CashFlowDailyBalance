using CashFlowDailyBalance.Domain.Entities;
using CashFlowDailyBalance.Domain.Enums;
using CashFlowDailyBalance.Infra.Data.Context;
using CashFlowDailyBalance.Infra.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashFlowDailyBalance.Infra.Data.Tests;

public class TransactionRepositoryTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        return context;
    }

    [Fact]
    public async Task GetByDateAsync_RetornaTransacoesDaDataEspecificada()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        
        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);
        var tomorrow = today.AddDays(1);

        // Adicionar transações de teste
        context.Transactions.AddRange(
            new Transaction(100m, TransactionType.Credit, today, "Crédito hoje"),
            new Transaction(50m, TransactionType.Credit, today, "Outro crédito hoje"),
            new Transaction(30m, TransactionType.Debit, today, "Débito hoje"),
            new Transaction(200m, TransactionType.Credit, yesterday, "Crédito ontem"),
            new Transaction(20m, TransactionType.Debit, tomorrow, "Débito amanhã")
        );
        await context.SaveChangesAsync();

        var repository = new TransactionRepository(context);

        // Act
        var result = await repository.GetByDateAsync(today);

        // Assert
        Assert.Equal(3, result.Count());
        Assert.All(result, t => Assert.Equal(today.Date, t.TransactionDate.Date));
    }

    [Fact]
    public async Task GetByPeriodAsync_RetornaTransacoesNoIntervaloDeDataEspecificado()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        
        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);
        var twoDaysAgo = today.AddDays(-2);
        var tomorrow = today.AddDays(1);
        var inTwoDays = today.AddDays(2);

        // Adicionar transações de teste
        context.Transactions.AddRange(
            new Transaction(100m, TransactionType.Credit, today, "Crédito hoje"),
            new Transaction(200m, TransactionType.Credit, yesterday, "Crédito ontem"),
            new Transaction(300m, TransactionType.Credit, twoDaysAgo, "Crédito anteontem"),
            new Transaction(20m, TransactionType.Debit, tomorrow, "Débito amanhã"),
            new Transaction(30m, TransactionType.Debit, inTwoDays, "Débito em dois dias")
        );
        await context.SaveChangesAsync();

        var repository = new TransactionRepository(context);

        // Act
        var result = await repository.GetByPeriodAsync(yesterday, tomorrow);

        // Assert
        Assert.Equal(3, result.Count());
        Assert.All(result, t => 
            Assert.True(
                t.TransactionDate.Date >= yesterday.Date && 
                t.TransactionDate.Date <= tomorrow.Date
            )
        );
    }
}
