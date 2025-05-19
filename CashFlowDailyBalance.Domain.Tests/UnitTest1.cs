using CashFlowDailyBalance.Domain.Entities;
using CashFlowDailyBalance.Domain.Enums;
using System;

namespace CashFlowDailyBalance.Domain.Tests;

public class TransactionTests
{
    [Fact]
    public void Transaction_ShouldBeCreatedSuccessfully()
    {
        // Arrange
        var amount = 100.50m;
        var type = TransactionType.Credit;
        var transactionDate = DateTime.UtcNow;
        var description = "Pagamento de serviço";
        var origin = "Cliente X";

        // Act
        var transaction = new Transaction(amount, type, transactionDate, description, origin);

        // Assert
        Assert.Equal(amount, transaction.Amount);
        Assert.Equal(type, transaction.Type);
        Assert.Equal(transactionDate, transaction.TransactionDate);
        Assert.Equal(description, transaction.Description);
        Assert.Equal(origin, transaction.Origin);
        Assert.NotEqual(DateTime.MinValue, transaction.CreatedAt);
    }

    [Fact]
    public void Transaction_ShouldBeCreatedSuccessfullyWithoutDescriptionAndOrigin()
    {
        // Arrange
        var amount = 50.25m;
        var type = TransactionType.Debit;
        var transactionDate = DateTime.UtcNow;

        // Act
        var transaction = new Transaction(amount, type, transactionDate);

        // Assert
        Assert.Equal(amount, transaction.Amount);
        Assert.Equal(type, transaction.Type);
        Assert.Equal(transactionDate, transaction.TransactionDate);
        Assert.Null(transaction.Description);
        Assert.Null(transaction.Origin);
        Assert.NotEqual(DateTime.MinValue, transaction.CreatedAt);
    }
}
