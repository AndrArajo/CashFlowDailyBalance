using CashFlowDailyBalance.Domain.Entities;
using System;

namespace CashFlowDailyBalance.Domain.Tests;

public class DailyBalanceTests
{
    [Fact]
    public void DailyBalance_ShouldBeCreatedSuccessfully()
    {
        // Arrange
        var id = 1;
        var balanceDate = DateTime.UtcNow.Date;
        var finalBalance = 1000.50m;
        var previousBalance = 900.25m;
        var totalCredits = 200.25m;
        var totalDebits = 100.00m;
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow;

        // Act
        var dailyBalance = new DailyBalance(
            id, 
            balanceDate, 
            finalBalance, 
            previousBalance, 
            totalCredits, 
            totalDebits, 
            createdAt, 
            updatedAt);

        // Assert
        Assert.Equal(id, dailyBalance.Id);
        Assert.Equal(balanceDate, dailyBalance.BalanceDate);
        Assert.Equal(finalBalance, dailyBalance.FinalBalance);
        Assert.Equal(previousBalance, dailyBalance.PreviousBalance);
        Assert.Equal(totalCredits, dailyBalance.TotalCredits);
        Assert.Equal(totalDebits, dailyBalance.TotalDebits);
        Assert.Equal(createdAt, dailyBalance.CreatedAt);
        Assert.Equal(updatedAt, dailyBalance.UpdatedAt);
    }
} 