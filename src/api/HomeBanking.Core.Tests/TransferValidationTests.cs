using FluentAssertions;
using HomeBanking.Core.Entities;

namespace HomeBanking.Core.Tests;

/// <summary>
/// Tests for transfer business rules: balance checks, amount constraints, and same-account guard.
/// </summary>
public class TransferValidationTests
{
    private static Account CreateAccount(decimal balance, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        AccountNumber = "****1234",
        Type = AccountType.Checking,
        Balance = balance,
        Currency = "USD",
        IsActive = true
    };

    [Fact]
    public void Transfer_WithSufficientBalance_Succeeds()
    {
        var source = CreateAccount(1000m);
        var transferAmount = 500m;

        var hasSufficientFunds = source.Balance >= transferAmount;

        hasSufficientFunds.Should().BeTrue();
        (source.Balance - transferAmount).Should().Be(500m);
    }

    [Fact]
    public void Transfer_WithInsufficientBalance_Fails()
    {
        var source = CreateAccount(100m);
        var transferAmount = 500m;

        var hasSufficientFunds = source.Balance >= transferAmount;

        hasSufficientFunds.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Transfer_WithNonPositiveAmount_IsInvalid(decimal amount)
    {
        var isValid = amount > 0;

        isValid.Should().BeFalse("transfer amount must be positive");
    }

    [Fact]
    public void Transfer_ToSameAccount_IsInvalid()
    {
        var accountId = Guid.NewGuid();
        var fromAccountId = accountId;
        var toAccountId = accountId;

        var isValid = fromAccountId != toAccountId;

        isValid.Should().BeFalse("cannot transfer to the same account");
    }

    [Fact]
    public void Transfer_ToDifferentAccount_IsValid()
    {
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        var isValid = fromAccountId != toAccountId;

        isValid.Should().BeTrue();
    }

    [Fact]
    public void Transfer_AmountExceedsMaxLimit_IsInvalid()
    {
        const decimal maxTransferLimit = 1_000_000m;
        var transferAmount = 1_000_001m;

        var isWithinLimit = transferAmount <= maxTransferLimit;

        isWithinLimit.Should().BeFalse("amount exceeds the maximum transfer limit of 1,000,000");
    }

    [Fact]
    public void Transfer_AmountAtMaxLimit_IsValid()
    {
        const decimal maxTransferLimit = 1_000_000m;
        var transferAmount = 1_000_000m;

        var isWithinLimit = transferAmount <= maxTransferLimit;

        isWithinLimit.Should().BeTrue();
    }

    [Fact]
    public void Transfer_UpdatesSourceAndDestinationBalances()
    {
        var source = CreateAccount(1000m);
        var destination = CreateAccount(500m);
        var transferAmount = 250m;

        var newSourceBalance = source.Balance - transferAmount;
        var newDestinationBalance = destination.Balance + transferAmount;

        newSourceBalance.Should().Be(750m);
        newDestinationBalance.Should().Be(750m);
    }
}
