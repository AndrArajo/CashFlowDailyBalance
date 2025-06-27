using CashFlowDailyBalance.Application.Services;
using CashFlowDailyBalance.Domain.Entities;
using CashFlowDailyBalance.Domain.Enums;
using CashFlowDailyBalance.Domain.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashFlowDailyBalance.Application.Tests;

public class DailyBalanceServiceTests
{
    private readonly Mock<IExternalTransactionService> _externalTransactionServiceMock;
    private readonly Mock<IDailyBalanceRepository> _dailyBalanceRepositoryMock;
    private readonly DailyBalanceService _dailyBalanceService;

    public DailyBalanceServiceTests()
    {
        _externalTransactionServiceMock = new Mock<IExternalTransactionService>();
        _dailyBalanceRepositoryMock = new Mock<IDailyBalanceRepository>();
        _dailyBalanceService = new DailyBalanceService(
            _externalTransactionServiceMock.Object,
            _dailyBalanceRepositoryMock.Object);
    }

    [Fact]
    public async Task ProcessDailyBalanceAsync_WithTransactionsOnSameDay_ShouldCalculateBalanceCorrectly()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;

        var transactions = new List<Transaction>
        {
            new Transaction(100m, TransactionType.Credit, date, "Crédito 1"),
            new Transaction(50m, TransactionType.Credit, date, "Crédito 2"),
            new Transaction(30m, TransactionType.Debit, date, "Débito 1")
        };

        _externalTransactionServiceMock.Setup(x => x.GetByDateAsync(date))
            .ReturnsAsync(transactions);

        var previousDayBalance = new DailyBalance(
            1, date.AddDays(-1), 200m, 150m, 100m, 50m, 
            DateTime.UtcNow, DateTime.UtcNow);

        var allBalances = new List<DailyBalance> { previousDayBalance };

        _dailyBalanceRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(allBalances);

        _dailyBalanceRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<DailyBalance>()))
            .ReturnsAsync((DailyBalance balance) => 
            {
                balance.Id = 2;
                return balance;
            });

        // Act
        var result = await _dailyBalanceService.ProcessDailyBalanceAsync(date);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(date, result.BalanceDate);
        // Saldo Final = Saldo Anterior (200) + Créditos (150) - Débitos (30) = 320
        Assert.Equal(320m, result.FinalBalance);
        Assert.Equal(200m, result.PreviousBalance);
        Assert.Equal(150m, result.TotalCredits);
        Assert.Equal(30m, result.TotalDebits);

        _externalTransactionServiceMock.Verify(x => x.GetByDateAsync(date), Times.Once);
        _dailyBalanceRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
        _dailyBalanceRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<DailyBalance>()), Times.Once);
    }

    [Fact]
    public async Task ProcessDailyBalanceAsync_WithoutPreviousBalance_ShouldUseZeroAsPreviousBalance()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;

        var transactions = new List<Transaction>
        {
            new Transaction(100m, TransactionType.Credit, date, "Crédito 1"),
            new Transaction(30m, TransactionType.Debit, date, "Débito 1")
        };

        _externalTransactionServiceMock.Setup(x => x.GetByDateAsync(date))
            .ReturnsAsync(transactions);

        var allBalances = new List<DailyBalance>(); // Lista vazia, sem balanço anterior

        _dailyBalanceRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(allBalances);

        _dailyBalanceRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<DailyBalance>()))
            .ReturnsAsync((DailyBalance balance) => 
            {
                balance.Id = 1;
                return balance;
            });

        // Act
        var result = await _dailyBalanceService.ProcessDailyBalanceAsync(date);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(date, result.BalanceDate);
        // Saldo Final = Saldo Anterior (0) + Créditos (100) - Débitos (30) = 70
        Assert.Equal(70m, result.FinalBalance);
        Assert.Equal(0m, result.PreviousBalance);
        Assert.Equal(100m, result.TotalCredits);
        Assert.Equal(30m, result.TotalDebits);
    }
}
