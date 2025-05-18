using CashFlowDailyBalance.Domain.Entities;
using CashFlowDailyBalance.Infra.Data.Context;
using CashFlowDailyBalance.Infra.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CashFlowDailyBalance.Infra.Data.Tests;

public class DailyBalanceRepositoryTests
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
    public async Task GetAllAsync_RetornaTodosOsBalancosDiarios()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        
        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);
        var twoDaysAgo = today.AddDays(-2);

        // Adicionar balanços diários de teste
        context.DailyBalances.AddRange(
            new DailyBalance(0, today, 1000m, 800m, 300m, 100m, DateTime.UtcNow, DateTime.UtcNow),
            new DailyBalance(0, yesterday, 800m, 600m, 250m, 50m, DateTime.UtcNow, DateTime.UtcNow),
            new DailyBalance(0, twoDaysAgo, 600m, 400m, 300m, 100m, DateTime.UtcNow, DateTime.UtcNow)
        );
        await context.SaveChangesAsync();

        var repository = new DailyBalanceRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task SaveAsync_AdicionaNovoBalancoDiario()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new DailyBalanceRepository(context);
        
        var today = DateTime.UtcNow.Date;
        var dailyBalance = new DailyBalance(
            0, today, 1000m, 800m, 300m, 100m, DateTime.UtcNow, DateTime.UtcNow);

        // Act
        var result = await repository.SaveAsync(dailyBalance);

        // Assert
        Assert.NotEqual(0, result.Id); // Deve ter um ID atribuído
        Assert.Equal(1, await context.DailyBalances.CountAsync());
        var balanceInDb = await context.DailyBalances.FirstOrDefaultAsync();
        Assert.Equal(today.Date, balanceInDb.BalanceDate?.Date);
        Assert.Equal(1000m, balanceInDb.FinalBalance);
    }

    [Fact]
    public async Task GetPaginatedAsync_RetornaPaginaCorreto()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        
        // Adicionar 15 balanços diários
        for (int i = 1; i <= 15; i++)
        {
            var date = DateTime.UtcNow.Date.AddDays(-i);
            context.DailyBalances.Add(new DailyBalance(
                0, date, 1000m + i, 800m + i, 300m, 100m, DateTime.UtcNow, DateTime.UtcNow));
        }
        await context.SaveChangesAsync();

        var repository = new DailyBalanceRepository(context);

        // Act
        var (result, totalCount) = await repository.GetPaginatedAsync(2, 5);

        // Assert
        Assert.Equal(5, result.Count()); // Segunda página com 5 itens por página
        Assert.Equal(15, totalCount); // Total de 15 registros
    }
} 