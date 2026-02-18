using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using HomeBanking.Contracts.Requests;
using HomeBanking.Core.Entities;
using HomeBanking.Infrastructure.Data;
using HomeBanking.Infrastructure.Services;

namespace HomeBanking.Core.Tests;

public class AgentToolServiceTests : IDisposable
{
    private readonly HomeBankingDbContext _context;
    private readonly AgentToolService _service;
    private readonly Guid _userId = Guid.NewGuid();

    public AgentToolServiceTests()
    {
        var options = new DbContextOptionsBuilder<HomeBankingDbContext>()
            .UseInMemoryDatabase($"AgentToolTests_{Guid.NewGuid()}")
            .Options;
        _context = new HomeBankingDbContext(options);
        _service = new AgentToolService(_context);
        SeedTestData();
    }

    private void SeedTestData()
    {
        // Create user
        var user = new User
        {
            Id = _userId,
            Email = "test@test.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "User"
        };
        _context.Users.Add(user);

        // Create accounts
        var checking = new Account
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            AccountNumber = "****1234",
            Type = AccountType.Checking,
            Balance = 5000m,
            Currency = "USD",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
        var savings = new Account
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            AccountNumber = "****5678",
            Type = AccountType.Savings,
            Balance = 10000m,
            Currency = "USD",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
        var creditCard = new Account
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            AccountNumber = "****9012",
            Type = AccountType.CreditCard,
            Balance = -500m,
            Currency = "USD",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
        _context.Accounts.AddRange(checking, savings, creditCard);

        // Create transactions for checking
        var txns = new List<Transaction>
        {
            new()
            {
                Id = Guid.NewGuid(), AccountId = checking.Id, Type = TransactionType.Payment,
                Amount = -45.67m, BalanceAfter = 4954.33m, Description = "Whole Foods Market",
                Category = TransactionCategory.Groceries, Timestamp = DateTime.UtcNow.AddDays(-5),
                ReferenceNumber = "TXN-001"
            },
            new()
            {
                Id = Guid.NewGuid(), AccountId = checking.Id, Type = TransactionType.Payment,
                Amount = -12.99m, BalanceAfter = 4941.34m, Description = "Netflix Subscription",
                Category = TransactionCategory.Entertainment, Timestamp = DateTime.UtcNow.AddDays(-4),
                ReferenceNumber = "TXN-002"
            },
            new()
            {
                Id = Guid.NewGuid(), AccountId = checking.Id, Type = TransactionType.Payment,
                Amount = -89.99m, BalanceAfter = 4851.35m, Description = "Amazon Purchase",
                Category = TransactionCategory.Shopping, Timestamp = DateTime.UtcNow.AddDays(-3),
                ReferenceNumber = "TXN-003"
            },
            new()
            {
                Id = Guid.NewGuid(), AccountId = checking.Id, Type = TransactionType.Payment,
                Amount = -28.45m, BalanceAfter = 4822.90m, Description = "Starbucks Coffee",
                Category = TransactionCategory.Dining, Timestamp = DateTime.UtcNow.AddDays(-2),
                ReferenceNumber = "TXN-004"
            },
            new()
            {
                Id = Guid.NewGuid(), AccountId = checking.Id, Type = TransactionType.Deposit,
                Amount = 3200.00m, BalanceAfter = 8022.90m, Description = "Salary Deposit",
                Category = TransactionCategory.Other, Timestamp = DateTime.UtcNow.AddDays(-1),
                ReferenceNumber = "TXN-005"
            },
            // Large grocery transaction for unusual detection
            new()
            {
                Id = Guid.NewGuid(), AccountId = checking.Id, Type = TransactionType.Payment,
                Amount = -250.00m, BalanceAfter = 7772.90m, Description = "Costco Wholesale",
                Category = TransactionCategory.Groceries, Timestamp = DateTime.UtcNow.AddDays(-1),
                ReferenceNumber = "TXN-006"
            },
            // More dining transactions
            new()
            {
                Id = Guid.NewGuid(), AccountId = checking.Id, Type = TransactionType.Payment,
                Amount = -15.00m, BalanceAfter = 7757.90m, Description = "McDonald's",
                Category = TransactionCategory.Dining, Timestamp = DateTime.UtcNow.AddDays(-6),
                ReferenceNumber = "TXN-007"
            },
        };
        _context.Transactions.AddRange(txns);

        // Create transactions for credit card
        var ccTxns = new List<Transaction>
        {
            new()
            {
                Id = Guid.NewGuid(), AccountId = creditCard.Id, Type = TransactionType.Payment,
                Amount = -125.00m, BalanceAfter = -625m, Description = "Best Buy Electronics",
                Category = TransactionCategory.Shopping, Timestamp = DateTime.UtcNow.AddDays(-3),
                ReferenceNumber = "TXN-CC-001"
            },
            new()
            {
                Id = Guid.NewGuid(), AccountId = creditCard.Id, Type = TransactionType.Payment,
                Amount = -42.30m, BalanceAfter = -667.30m, Description = "Olive Garden",
                Category = TransactionCategory.Dining, Timestamp = DateTime.UtcNow.AddDays(-2),
                ReferenceNumber = "TXN-CC-002"
            },
        };
        _context.Transactions.AddRange(ccTxns);

        _context.SaveChanges();
    }

    public void Dispose() => _context.Dispose();

    // ──────────────────────────────────────────────────────────
    // GetAccountBalances
    // ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAccountBalances_Returns_AllActiveAccounts()
    {
        var result = await _service.GetAccountBalancesAsync(_userId);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAccountBalances_Returns_CorrectBalances()
    {
        var result = await _service.GetAccountBalancesAsync(_userId);

        var checking = result.FirstOrDefault(a => a.AccountType == "Checking");
        checking.Should().NotBeNull();
        checking!.Balance.Should().Be(5000m);

        var savings = result.FirstOrDefault(a => a.AccountType == "Savings");
        savings.Should().NotBeNull();
        savings!.Balance.Should().Be(10000m);

        var creditCard = result.FirstOrDefault(a => a.AccountType == "CreditCard");
        creditCard.Should().NotBeNull();
        creditCard!.Balance.Should().Be(-500m);
    }

    [Fact]
    public async Task GetAccountBalances_Returns_Empty_WhenNoAccounts()
    {
        var nonExistentUserId = Guid.NewGuid();

        var result = await _service.GetAccountBalancesAsync(nonExistentUserId);

        result.Should().BeEmpty();
    }

    // ──────────────────────────────────────────────────────────
    // GetAccountDetails
    // ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAccountDetails_Returns_AccountDetails_ForValidType()
    {
        var result = await _service.GetAccountDetailsAsync(_userId, "Checking");

        result.Should().NotBeNull();
        result!.AccountType.Should().Be("Checking");
        result.AccountNumber.Should().Be("****1234");
        result.Balance.Should().Be(5000m);
        result.Currency.Should().Be("USD");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetAccountDetails_Returns_Null_ForInvalidType()
    {
        var result = await _service.GetAccountDetailsAsync(_userId, "Invalid");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAccountDetails_Returns_Null_ForMissingType()
    {
        // No Investment account was seeded
        var result = await _service.GetAccountDetailsAsync(_userId, "Investment");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAccountDetails_Includes_TransactionCount()
    {
        var result = await _service.GetAccountDetailsAsync(_userId, "Checking");

        result.Should().NotBeNull();
        result!.TransactionCount.Should().Be(7); // 7 checking transactions seeded
    }

    // ──────────────────────────────────────────────────────────
    // SearchTransactions
    // ──────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchTransactions_Returns_AllTransactions_NoFilter()
    {
        var searchParams = new TransactionSearchParams();

        var (transactions, totalCount) = await _service.SearchTransactionsAsync(_userId, searchParams);

        // 7 checking + 2 credit card = 9 total
        totalCount.Should().Be(9);
        // Default limit is 10, so all 9 returned
        transactions.Should().HaveCount(9);
        // Ordered by timestamp desc
        transactions.Should().BeInDescendingOrder(t => t.Timestamp);
    }

    [Fact]
    public async Task SearchTransactions_Filters_ByDescription_CaseInsensitive()
    {
        var searchParams = new TransactionSearchParams(Description: "whole foods");

        var (transactions, totalCount) = await _service.SearchTransactionsAsync(_userId, searchParams);

        totalCount.Should().Be(1);
        transactions.Should().HaveCount(1);
        transactions[0].Description.Should().Be("Whole Foods Market");
    }

    [Fact]
    public async Task SearchTransactions_Filters_ByCategory()
    {
        var searchParams = new TransactionSearchParams(Category: "Groceries");

        var (transactions, totalCount) = await _service.SearchTransactionsAsync(_userId, searchParams);

        // 2 grocery transactions: Whole Foods + Costco
        totalCount.Should().Be(2);
        transactions.Should().HaveCount(2);
        transactions.Should().OnlyContain(t => t.Category == "Groceries");
    }

    [Fact]
    public async Task SearchTransactions_Filters_ByAmountRange()
    {
        // Amounts are filtered by absolute value; filter for txns between 40 and 100
        var searchParams = new TransactionSearchParams(MinAmount: 40m, MaxAmount: 100m);

        var (transactions, totalCount) = await _service.SearchTransactionsAsync(_userId, searchParams);

        // Should match: Whole Foods (45.67), Amazon (89.99), Starbucks (no – 28.45 abs <40),
        // Olive Garden (42.30), Netflix – 12.99 abs <40
        transactions.Should().OnlyContain(t => Math.Abs(t.Amount) >= 40m && Math.Abs(t.Amount) <= 100m);
        totalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SearchTransactions_Filters_ByDateRange()
    {
        var fromDate = DateTime.UtcNow.AddDays(-3);
        var toDate = DateTime.UtcNow;

        var searchParams = new TransactionSearchParams(FromDate: fromDate, ToDate: toDate);

        var (transactions, totalCount) = await _service.SearchTransactionsAsync(_userId, searchParams);

        totalCount.Should().BeGreaterThan(0);
        transactions.Should().OnlyContain(t => t.Timestamp >= fromDate && t.Timestamp <= toDate);
    }

    [Fact]
    public async Task SearchTransactions_Filters_Combined()
    {
        var searchParams = new TransactionSearchParams(
            AccountType: "Checking",
            Category: "Dining"
        );

        var (transactions, totalCount) = await _service.SearchTransactionsAsync(_userId, searchParams);

        // Starbucks + McDonald's (checking dining), NOT Olive Garden (credit card dining)
        totalCount.Should().Be(2);
        transactions.Should().HaveCount(2);
        transactions.Should().OnlyContain(t => t.Category == "Dining" && t.AccountType == "Checking");
    }

    [Fact]
    public async Task SearchTransactions_Enforces_DefaultLimit()
    {
        // Default limit is 10, so with 9 total transactions, all are returned
        var searchParams = new TransactionSearchParams();

        var (transactions, totalCount) = await _service.SearchTransactionsAsync(_userId, searchParams);

        transactions.Count.Should().BeLessThanOrEqualTo(10);
    }

    [Fact]
    public async Task SearchTransactions_Enforces_MaxLimit()
    {
        // Limit > 20 should be clamped to 20
        var searchParams = new TransactionSearchParams(Limit: 50);

        var (transactions, totalCount) = await _service.SearchTransactionsAsync(_userId, searchParams);

        transactions.Count.Should().BeLessThanOrEqualTo(20);
    }

    [Fact]
    public async Task SearchTransactions_Returns_Empty_WhenNoMatches()
    {
        var searchParams = new TransactionSearchParams(Description: "NonExistentMerchant12345");

        var (transactions, totalCount) = await _service.SearchTransactionsAsync(_userId, searchParams);

        totalCount.Should().Be(0);
        transactions.Should().BeEmpty();
    }

    // ──────────────────────────────────────────────────────────
    // GetSpendingSummary
    // ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSpendingSummary_Returns_CategoryTotals()
    {
        var summaryParams = new SpendingSummaryParams();

        var result = await _service.GetSpendingSummaryAsync(_userId, summaryParams);

        result.Categories.Should().NotBeEmpty();
        result.TotalSpending.Should().BeGreaterThan(0);

        // Verify known categories exist
        result.Categories.Should().Contain(c => c.Category == "Groceries");
        result.Categories.Should().Contain(c => c.Category == "Dining");
        result.Categories.Should().Contain(c => c.Category == "Shopping");
    }

    [Fact]
    public async Task GetSpendingSummary_Returns_CorrectPercentages()
    {
        var summaryParams = new SpendingSummaryParams();

        var result = await _service.GetSpendingSummaryAsync(_userId, summaryParams);

        // All percentages should sum to approximately 100%
        var totalPercentage = result.Categories.Sum(c => c.Percentage);
        totalPercentage.Should().BeApproximately(100m, 0.5m);

        // Each percentage should be between 0 and 100
        result.Categories.Should().OnlyContain(c => c.Percentage >= 0 && c.Percentage <= 100);
    }

    [Fact]
    public async Task GetSpendingSummary_Calculates_AverageDaily()
    {
        var summaryParams = new SpendingSummaryParams();

        var result = await _service.GetSpendingSummaryAsync(_userId, summaryParams);

        result.AverageDaily.Should().BeGreaterThan(0);
        // AverageDaily = TotalSpending / days
        var days = Math.Max(1, (result.ToDate - result.FromDate).Days);
        var expectedAvg = Math.Round(result.TotalSpending / days, 2);
        result.AverageDaily.Should().Be(expectedAvg);
    }

    [Fact]
    public async Task GetSpendingSummary_Filters_ByAccountType()
    {
        var summaryParams = new SpendingSummaryParams(AccountType: "Checking");

        var result = await _service.GetSpendingSummaryAsync(_userId, summaryParams);

        // Checking spending: Whole Foods (45.67), Netflix (12.99), Amazon (89.99),
        // Starbucks (28.45), Costco (250.00), McDonald's (15.00) = 442.10
        // Salary Deposit is positive, so excluded
        result.TotalSpending.Should().Be(45.67m + 12.99m + 89.99m + 28.45m + 250.00m + 15.00m);
    }

    [Fact]
    public async Task GetSpendingSummary_Returns_Zero_WhenNoSpending()
    {
        var noSpendUserId = Guid.NewGuid();
        var summaryParams = new SpendingSummaryParams();

        var result = await _service.GetSpendingSummaryAsync(noSpendUserId, summaryParams);

        result.TotalSpending.Should().Be(0);
        result.Categories.Should().BeEmpty();
        result.AverageDaily.Should().Be(0);
    }

    // ──────────────────────────────────────────────────────────
    // DetectUnusualTransactions
    // ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DetectUnusualTransactions_Detects_LargeTransactions()
    {
        // Grocery txns: Whole Foods $45.67 + Costco $250.00 → avg = $147.835
        // Costco ratio = 250 / 147.835 ≈ 1.69 — not flagged at default threshold 2.0
        // But if we use a lower threshold or check differently...
        // Actually at default 2.0 threshold: only transactions with ratio >= 2.0 are flagged.
        // Let's use threshold 1.5 explicitly to catch the Costco transaction.
        var result = await _service.DetectUnusualTransactionsAsync(_userId, threshold: 1.5m);

        result.Should().NotBeEmpty();
        result.Should().Contain(u => u.Description == "Costco Wholesale");
    }

    [Fact]
    public async Task DetectUnusualTransactions_CustomThreshold()
    {
        // With a lower threshold of 1.3, more transactions should be flagged
        var result = await _service.DetectUnusualTransactionsAsync(_userId, threshold: 1.3m);

        result.Should().NotBeEmpty();
        // Costco ($250) vs avg grocery ($147.835) ratio ~1.69 should be flagged
        result.Should().Contain(u => u.Description == "Costco Wholesale");
    }

    [Fact]
    public async Task DetectUnusualTransactions_No_Unusual_Returns_Empty()
    {
        // With a very high threshold, nothing should be flagged
        var result = await _service.DetectUnusualTransactionsAsync(_userId, threshold: 100m);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DetectUnusualTransactions_SingleTransactionInCategory_NotFlagged()
    {
        // Entertainment (Netflix) and Other (Salary) have only 1 transaction each
        // They should never appear in unusual detection since you need >1 in a category
        var result = await _service.DetectUnusualTransactionsAsync(_userId, threshold: 0.1m);

        result.Should().NotContain(u => u.Category == "Entertainment");
        // "Other" category only has positive amounts (Salary Deposit), so won't appear anyway
    }

    // ──────────────────────────────────────────────────────────
    // ExecuteTransfer
    // ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteTransfer_Successful_Transfer()
    {
        var transferParams = new AgentTransferParams("Checking", "Savings", 1000m, "Test transfer");

        var result = await _service.ExecuteTransferAsync(_userId, transferParams);

        result.Success.Should().BeTrue();
        result.Error.Should().BeNull();
        result.TransferId.Should().NotBeNull();
        result.FromBalance.Should().Be(4000m); // 5000 - 1000
        result.ToBalance.Should().Be(11000m);  // 10000 + 1000

        // Verify DB state
        var checking = await _context.Accounts.FirstAsync(a => a.UserId == _userId && a.Type == AccountType.Checking);
        var savings = await _context.Accounts.FirstAsync(a => a.UserId == _userId && a.Type == AccountType.Savings);
        checking.Balance.Should().Be(4000m);
        savings.Balance.Should().Be(11000m);
    }

    [Fact]
    public async Task ExecuteTransfer_InsufficientFunds_Error()
    {
        var transferParams = new AgentTransferParams("Checking", "Savings", 999999m);

        var result = await _service.ExecuteTransferAsync(_userId, transferParams);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Insufficient funds");
    }

    [Fact]
    public async Task ExecuteTransfer_SameAccount_Error()
    {
        var transferParams = new AgentTransferParams("Checking", "Checking", 100m);

        var result = await _service.ExecuteTransferAsync(_userId, transferParams);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("same account");
    }

    [Fact]
    public async Task ExecuteTransfer_InvalidAccountType_Error()
    {
        var transferParams = new AgentTransferParams("Invalid", "Savings", 100m);

        var result = await _service.ExecuteTransferAsync(_userId, transferParams);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Invalid");
    }

    [Fact]
    public async Task ExecuteTransfer_AmountZero_Error()
    {
        var transferParams = new AgentTransferParams("Checking", "Savings", 0m);

        var result = await _service.ExecuteTransferAsync(_userId, transferParams);

        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExecuteTransfer_AmountExceedsMax_Error()
    {
        var transferParams = new AgentTransferParams("Checking", "Savings", 1_000_001m);

        var result = await _service.ExecuteTransferAsync(_userId, transferParams);

        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExecuteTransfer_AccountNotFound_Error()
    {
        // User has no Investment account
        var transferParams = new AgentTransferParams("Checking", "Investment", 100m);

        var result = await _service.ExecuteTransferAsync(_userId, transferParams);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }
}
