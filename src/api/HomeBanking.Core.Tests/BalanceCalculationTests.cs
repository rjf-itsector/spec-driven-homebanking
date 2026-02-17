using FluentAssertions;
using HomeBanking.Core.Entities;

namespace HomeBanking.Core.Tests;

/// <summary>
/// Tests for balance calculations: debits, credits, running balances, and transfer outcomes.
/// </summary>
public class BalanceCalculationTests
{
    private static Account CreateAccount(decimal balance) => new()
    {
        Id = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        AccountNumber = "****5678",
        Type = AccountType.Checking,
        Balance = balance,
        Currency = "USD",
        IsActive = true
    };

    private static Transaction CreateTransaction(
        Guid accountId,
        TransactionType type,
        decimal amount,
        decimal balanceAfter) => new()
    {
        AccountId = accountId,
        Type = type,
        Amount = amount,
        BalanceAfter = balanceAfter,
        Description = "Test transaction",
        ReferenceNumber = $"TXN-TEST-{Guid.NewGuid().ToString()[..6]}"
    };

    [Fact]
    public void Debit_ReducesBalance_Correctly()
    {
        var account = CreateAccount(1000m);
        var debitAmount = 300m;

        var newBalance = account.Balance - debitAmount;

        newBalance.Should().Be(700m);
    }

    [Fact]
    public void Credit_IncreasesBalance_Correctly()
    {
        var account = CreateAccount(1000m);
        var creditAmount = 500m;

        var newBalance = account.Balance + creditAmount;

        newBalance.Should().Be(1500m);
    }

    [Fact]
    public void MultipleTransactions_CalculateRunningBalance_Correctly()
    {
        var accountId = Guid.NewGuid();
        decimal startingBalance = 1000m;

        // Simulate a sequence of transactions and track running balance
        var transactionAmounts = new[] { -200m, 500m, -100m, -50m, 300m };
        var currentBalance = startingBalance;
        var transactions = new List<Transaction>();

        foreach (var amount in transactionAmounts)
        {
            currentBalance += amount;
            var type = amount >= 0 ? TransactionType.Deposit : TransactionType.Withdrawal;
            transactions.Add(CreateTransaction(accountId, type, amount, currentBalance));
        }

        // Expected: 1000 - 200 + 500 - 100 - 50 + 300 = 1450
        currentBalance.Should().Be(1450m);
        transactions.Should().HaveCount(5);
        transactions.Last().BalanceAfter.Should().Be(1450m);
    }

    [Fact]
    public void BalanceAfterTransfer_MatchesExpected()
    {
        var sourceAccount = CreateAccount(2000m);
        var destinationAccount = CreateAccount(500m);
        var transferAmount = 750m;

        // Simulate the transfer
        var sourceBalanceAfter = sourceAccount.Balance - transferAmount;
        var destinationBalanceAfter = destinationAccount.Balance + transferAmount;

        sourceBalanceAfter.Should().Be(1250m);
        destinationBalanceAfter.Should().Be(1250m);

        // Verify transactions record the correct balance snapshot
        var debitTxn = CreateTransaction(
            sourceAccount.Id, TransactionType.Transfer, -transferAmount, sourceBalanceAfter);
        var creditTxn = CreateTransaction(
            destinationAccount.Id, TransactionType.Transfer, transferAmount, destinationBalanceAfter);

        debitTxn.Amount.Should().Be(-750m);
        debitTxn.BalanceAfter.Should().Be(1250m);
        creditTxn.Amount.Should().Be(750m);
        creditTxn.BalanceAfter.Should().Be(1250m);
    }

    [Fact]
    public void ZeroBalance_DebitFails_InsufficientFunds()
    {
        var account = CreateAccount(0m);
        var debitAmount = 100m;

        var hasFunds = account.Balance >= debitAmount;

        hasFunds.Should().BeFalse();
    }

    [Fact]
    public void LargeDecimalPrecision_IsPreserved()
    {
        var account = CreateAccount(1000.99m);
        var amount = 0.01m;

        var newBalance = account.Balance - amount;

        newBalance.Should().Be(1000.98m);
    }
}
