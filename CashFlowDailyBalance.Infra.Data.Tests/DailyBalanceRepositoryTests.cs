using CashFlowDailyBalance.Domain.Entities;
using CashFlowDailyBalance.Infra.CrossCutting.Caching;
using CashFlowDailyBalance.Infra.Data.Context;
using CashFlowDailyBalance.Infra.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashFlowDailyBalance.Infra.Data.Tests;

public class DailyBalanceRepositoryTests
{
    private readonly Mock<ICacheService> _cacheServiceMock;

    public DailyBalanceRepositoryTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        
        // Configurar o mock para simplesmente chamar o factory para buscar os dados
        _cacheServiceMock.Setup(c => c.GetOrCreateAsync<IEnumerable<DailyBalance>>(
            It.IsAny<string>(), 
            It.IsAny<Func<Task<IEnumerable<DailyBalance>>>>(), 
            It.IsAny<TimeSpan?>()))
            .Returns<string, Func<Task<IEnumerable<DailyBalance>>>, TimeSpan?>((key, factory, expiration) => factory());
            
        _cacheServiceMock.Setup(c => c.GetOrCreateAsync<DailyBalance>(
            It.IsAny<string>(), 
            It.IsAny<Func<Task<DailyBalance>>>(), 
            It.IsAny<TimeSpan?>()))
            .Returns<string, Func<Task<DailyBalance>>, TimeSpan?>((key, factory, expiration) => factory());
            
        _cacheServiceMock.Setup(c => c.GetOrCreateAsync<(IEnumerable<DailyBalance>, int)>(
            It.IsAny<string>(), 
            It.IsAny<Func<Task<(IEnumerable<DailyBalance>, int)>>>(), 
            It.IsAny<TimeSpan?>()))
            .Returns<string, Func<Task<(IEnumerable<DailyBalance>, int)>>, TimeSpan?>((key, factory, expiration) => factory());
    }

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

        var repository = new DailyBalanceRepository(context, _cacheServiceMock.Object);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count());
        
        // Verificar que o cache foi chamado
        _cacheServiceMock.Verify(c => c.GetOrCreateAsync<IEnumerable<DailyBalance>>(
            It.Is<string>(s => s == "daily_balances_all"),
            It.IsAny<Func<Task<IEnumerable<DailyBalance>>>>(),
            It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_AdicionaNovoBalancoDiario()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new DailyBalanceRepository(context, _cacheServiceMock.Object);
        
        var today = DateTime.UtcNow.Date;
        var dailyBalance = new DailyBalance(
            0, today, 1000m, 800m, 300m, 100m, DateTime.UtcNow, DateTime.UtcNow);

        // Configurar o mock para retornar null para GetByDateAsync, simulando que não existe o balanço
        _cacheServiceMock.Setup(c => c.GetOrCreateAsync<DailyBalance>(
            It.IsAny<string>(),
            It.IsAny<Func<Task<DailyBalance>>>(),
            It.IsAny<TimeSpan?>()))
            .ReturnsAsync(default(DailyBalance));

        // Act
        var result = await repository.SaveAsync(dailyBalance);

        // Assert
        Assert.NotEqual(0, result.Id); // Deve ter um ID atribuído
        Assert.Equal(1, await context.DailyBalances.CountAsync());
        var balanceInDb = await context.DailyBalances.FirstOrDefaultAsync();
        Assert.Equal(today.Date, balanceInDb.BalanceDate?.Date);
        Assert.Equal(1000m, balanceInDb.FinalBalance);
        
        // Verificar que o cache foi removido
        _cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.AtLeastOnce);
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

        var repository = new DailyBalanceRepository(context, _cacheServiceMock.Object);

        // Act
        var (result, totalCount) = await repository.GetPaginatedAsync(2, 5);

        // Assert
        Assert.Equal(5, result.Count()); // Segunda página com 5 itens por página
        Assert.Equal(15, totalCount); // Total de 15 registros
        
        // Verificar que o cache foi chamado
        _cacheServiceMock.Verify(c => c.GetOrCreateAsync<(IEnumerable<DailyBalance>, int)>(
            It.Is<string>(s => s.Contains("daily_balances_page_2")),
            It.IsAny<Func<Task<(IEnumerable<DailyBalance>, int)>>>(),
            It.IsAny<TimeSpan?>()), Times.Once);
    }
} 