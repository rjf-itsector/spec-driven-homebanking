using FluentAssertions;
using HomeBanking.Core.Entities;

namespace HomeBanking.Core.Tests;

/// <summary>
/// Tests for entity creation, properties, defaults, and navigation properties.
/// </summary>
public class EntityTests
{
    [Fact]
    public void User_Properties_SetCorrectly()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = "hashed_password_123",
            FirstName = "John",
            LastName = "Doe"
        };

        user.Id.Should().Be(userId);
        user.Email.Should().Be("test@example.com");
        user.PasswordHash.Should().Be("hashed_password_123");
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
    }

    [Fact]
    public void User_Defaults_AreCorrect()
    {
        var user = new User
        {
            Email = "default@example.com",
            PasswordHash = "hash"
        };

        user.FirstName.Should().Be("Demo");
        user.LastName.Should().Be("User");
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.Accounts.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Account_Properties_SetCorrectly()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var account = new Account
        {
            Id = accountId,
            UserId = userId,
            AccountNumber = "****9999",
            Type = AccountType.Savings,
            Balance = 5000.50m,
            Currency = "EUR",
            IsActive = false
        };

        account.Id.Should().Be(accountId);
        account.UserId.Should().Be(userId);
        account.AccountNumber.Should().Be("****9999");
        account.Type.Should().Be(AccountType.Savings);
        account.Balance.Should().Be(5000.50m);
        account.Currency.Should().Be("EUR");
        account.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Account_Defaults_AreCorrect()
    {
        var account = new Account
        {
            UserId = Guid.NewGuid(),
            AccountNumber = "****0001"
        };

        account.Currency.Should().Be("USD");
        account.IsActive.Should().BeTrue();
        account.Type.Should().Be(AccountType.Checking);
        account.Balance.Should().Be(0m);
        account.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        account.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Account_NavigationProperties_Initialized()
    {
        var account = new Account
        {
            UserId = Guid.NewGuid(),
            AccountNumber = "****0002"
        };

        account.Transactions.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Transaction_Properties_SetCorrectly()
    {
        var transactionId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var relatedId = Guid.NewGuid();
        var timestamp = new DateTime(2026, 2, 17, 10, 0, 0, DateTimeKind.Utc);

        var transaction = new Transaction
        {
            Id = transactionId,
            AccountId = accountId,
            Type = TransactionType.Transfer,
            Amount = -500m,
            BalanceAfter = 1500m,
            Description = "Transfer to savings",
            Category = TransactionCategory.Other,
            Timestamp = timestamp,
            ReferenceNumber = "TXN-20260217-ABC123",
            RelatedTransactionId = relatedId
        };

        transaction.Id.Should().Be(transactionId);
        transaction.AccountId.Should().Be(accountId);
        transaction.Type.Should().Be(TransactionType.Transfer);
        transaction.Amount.Should().Be(-500m);
        transaction.BalanceAfter.Should().Be(1500m);
        transaction.Description.Should().Be("Transfer to savings");
        transaction.Category.Should().Be(TransactionCategory.Other);
        transaction.Timestamp.Should().Be(timestamp);
        transaction.ReferenceNumber.Should().Be("TXN-20260217-ABC123");
        transaction.RelatedTransactionId.Should().Be(relatedId);
    }

    [Fact]
    public void Transaction_Defaults_AreCorrect()
    {
        var transaction = new Transaction
        {
            AccountId = Guid.NewGuid(),
            Description = "Test",
            ReferenceNumber = "REF-001"
        };

        transaction.Id.Should().Be(Guid.Empty);
        transaction.Type.Should().Be(TransactionType.Deposit);
        transaction.Amount.Should().Be(0m);
        transaction.BalanceAfter.Should().Be(0m);
        transaction.Category.Should().Be(TransactionCategory.Groceries);
        transaction.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        transaction.RelatedTransactionId.Should().BeNull();
    }

    [Theory]
    [InlineData(AccountType.Checking, 0)]
    [InlineData(AccountType.Savings, 1)]
    [InlineData(AccountType.CreditCard, 2)]
    [InlineData(AccountType.Investment, 3)]
    public void AccountType_HasExpectedValues(AccountType type, int expectedValue)
    {
        ((int)type).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(TransactionType.Deposit, 0)]
    [InlineData(TransactionType.Withdrawal, 1)]
    [InlineData(TransactionType.Transfer, 2)]
    [InlineData(TransactionType.Payment, 3)]
    [InlineData(TransactionType.Fee, 4)]
    [InlineData(TransactionType.Interest, 5)]
    public void TransactionType_HasExpectedValues(TransactionType type, int expectedValue)
    {
        ((int)type).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(TransactionCategory.Groceries, 0)]
    [InlineData(TransactionCategory.Dining, 1)]
    [InlineData(TransactionCategory.Transportation, 2)]
    [InlineData(TransactionCategory.Shopping, 3)]
    [InlineData(TransactionCategory.Bills, 4)]
    [InlineData(TransactionCategory.Healthcare, 5)]
    [InlineData(TransactionCategory.Entertainment, 6)]
    [InlineData(TransactionCategory.Other, 7)]
    public void TransactionCategory_HasExpectedValues(TransactionCategory category, int expectedValue)
    {
        ((int)category).Should().Be(expectedValue);
    }

    [Fact]
    public void User_Accounts_NavigationProperty_Initialized()
    {
        var user = new User
        {
            Email = "nav@test.com",
            PasswordHash = "hash"
        };

        user.Accounts.Should().NotBeNull().And.BeEmpty();
    }
}
