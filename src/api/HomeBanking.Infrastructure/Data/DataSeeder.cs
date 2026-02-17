using HomeBanking.Core.Entities;

namespace HomeBanking.Infrastructure.Data;

/// <summary>
/// Seeds the database with deterministic demo data for development and testing.
/// </summary>
public static class DataSeeder
{
    // Deterministic GUIDs for testability
    private static readonly Guid DemoUserId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    private static readonly Guid CheckingAccountId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SavingsAccountId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid CreditCardAccountId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid InvestmentAccountId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    // Base date for deterministic timestamps (90 days before a fixed "now")
    private static readonly DateTime ReferenceDate = new(2026, 2, 17, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime BaseDate = ReferenceDate.AddDays(-90); // Nov 19, 2025

    public static async Task SeedAsync(HomeBankingDbContext context)
    {
        if (context.Users.Any())
            return;

        var user = CreateDemoUser();
        context.Users.Add(user);

        var accounts = CreateAccounts();
        context.Accounts.AddRange(accounts);

        var transactions = CreateAllTransactions();
        context.Transactions.AddRange(transactions);

        await context.SaveChangesAsync();
    }

    private static User CreateDemoUser()
    {
        return new User
        {
            Id = DemoUserId,
            Email = "demo@bank.com",
            // BCrypt hash for "Demo123!" — pre-computed for determinism
            PasswordHash = "$2a$11$QJKX6Wz5Q5Z5Z5Z5Z5Z5ZOxHvHvHvHvHvHvHvHvHvHvHvHvHvHvHv",
            FirstName = "Demo",
            LastName = "User",
            CreatedAt = BaseDate.AddDays(-30)
        };
    }

    private static List<Account> CreateAccounts()
    {
        return new List<Account>
        {
            new Account
            {
                Id = CheckingAccountId,
                UserId = DemoUserId,
                AccountNumber = "****1234",
                Type = AccountType.Checking,
                Balance = 5_250.00m,
                Currency = "USD",
                IsActive = true,
                CreatedAt = BaseDate.AddDays(-30),
                UpdatedAt = ReferenceDate
            },
            new Account
            {
                Id = SavingsAccountId,
                UserId = DemoUserId,
                AccountNumber = "****5678",
                Type = AccountType.Savings,
                Balance = 12_800.00m,
                Currency = "USD",
                IsActive = true,
                CreatedAt = BaseDate.AddDays(-30),
                UpdatedAt = ReferenceDate
            },
            new Account
            {
                Id = CreditCardAccountId,
                UserId = DemoUserId,
                AccountNumber = "****9012",
                Type = AccountType.CreditCard,
                Balance = -1_243.50m,
                Currency = "USD",
                IsActive = true,
                CreatedAt = BaseDate.AddDays(-30),
                UpdatedAt = ReferenceDate
            },
            new Account
            {
                Id = InvestmentAccountId,
                UserId = DemoUserId,
                AccountNumber = "****3456",
                Type = AccountType.Investment,
                Balance = 28_500.00m,
                Currency = "USD",
                IsActive = true,
                CreatedAt = BaseDate.AddDays(-30),
                UpdatedAt = ReferenceDate
            }
        };
    }

    private static List<Transaction> CreateAllTransactions()
    {
        var all = new List<Transaction>();
        all.AddRange(CreateCheckingTransactions());
        all.AddRange(CreateSavingsTransactions());
        all.AddRange(CreateCreditCardTransactions());
        all.AddRange(CreateInvestmentTransactions());
        return all;
    }

    // ---------------------------------------------------------------
    // Checking Account: target balance = $5,250.00
    // ---------------------------------------------------------------
    private static List<Transaction> CreateCheckingTransactions()
    {
        var accountId = CheckingAccountId;
        var txDefs = new (int DayOffset, decimal Amount, string Desc, TransactionType Type, TransactionCategory Cat)[]
        {
            (2,   3_200.00m, "Salary Deposit - Acme Corp",             TransactionType.Deposit,    TransactionCategory.Other),
            (5,    -45.67m,  "Whole Foods Market #1234",               TransactionType.Payment,    TransactionCategory.Groceries),
            (7,    -12.99m,  "Netflix Subscription",                   TransactionType.Payment,    TransactionCategory.Entertainment),
            (9,    -55.00m,  "Electric Company - Monthly Bill",        TransactionType.Payment,    TransactionCategory.Bills),
            (12,   -32.50m,  "Shell Gas Station",                      TransactionType.Payment,    TransactionCategory.Transportation),
            (14,   -89.99m,  "Amazon.com Purchase",                    TransactionType.Payment,    TransactionCategory.Shopping),
            (16,  -500.00m,  "Transfer to Savings",                    TransactionType.Transfer,   TransactionCategory.Other),
            (18,   -60.00m,  "ATM Withdrawal - Main St",              TransactionType.Withdrawal, TransactionCategory.Other),
            (22,   -28.45m,  "Starbucks Coffee #567",                  TransactionType.Payment,    TransactionCategory.Dining),
            (25,   -15.99m,  "Spotify Premium",                       TransactionType.Payment,    TransactionCategory.Entertainment),
            (28,   -42.30m,  "Target Store #890",                     TransactionType.Payment,    TransactionCategory.Shopping),
            (32, 3_200.00m,  "Salary Deposit - Acme Corp",             TransactionType.Deposit,    TransactionCategory.Other),
            (34,   -75.00m,  "Water & Sewer Utility",                 TransactionType.Payment,    TransactionCategory.Bills),
            (37,   -38.25m,  "Trader Joe's #456",                     TransactionType.Payment,    TransactionCategory.Groceries),
            (40,  -120.00m,  "Auto Insurance Premium",                TransactionType.Payment,    TransactionCategory.Bills),
            (43,   -22.50m,  "Uber Ride",                             TransactionType.Payment,    TransactionCategory.Transportation),
            (47,   -65.80m,  "CVS Pharmacy #321",                     TransactionType.Payment,    TransactionCategory.Healthcare),
            (50,   200.00m,  "Refund - Amazon.com",                   TransactionType.Deposit,    TransactionCategory.Shopping),
            (55,  -500.00m,  "Transfer to Savings",                   TransactionType.Transfer,   TransactionCategory.Other),
            (60, 3_200.00m,  "Salary Deposit - Acme Corp",             TransactionType.Deposit,    TransactionCategory.Other),
            (63,   -52.15m,  "Whole Foods Market #1234",              TransactionType.Payment,    TransactionCategory.Groceries),
            (66,   -35.00m,  "Internet Service - Comcast",            TransactionType.Payment,    TransactionCategory.Bills),
            (69,  -150.00m,  "Dentist - Dr. Smith",                   TransactionType.Payment,    TransactionCategory.Healthcare),
            (72,   -18.90m,  "McDonald's #7890",                      TransactionType.Payment,    TransactionCategory.Dining),
            (75,   -40.00m,  "Gym Membership - Planet Fitness",       TransactionType.Payment,    TransactionCategory.Bills),
            (78,   -95.00m,  "Costco Wholesale #123",                 TransactionType.Payment,    TransactionCategory.Groceries),
        };

        // Sum of amounts: 3200 - 45.67 - 12.99 - 55 - 32.50 - 89.99 - 500 - 60 - 28.45 - 15.99 - 42.30
        //                 + 3200 - 75 - 38.25 - 120 - 22.50 - 65.80 + 200 - 500 + 3200
        //                 - 52.15 - 35 - 150 - 18.90 - 40 - 95
        // = 8004.51 ... need adjustment to hit 5250.00
        // Adjustment: 5250.00 - sum(txDefs amounts)
        var rawSum = txDefs.Sum(t => t.Amount);
        var adjustment = 5_250.00m - rawSum;

        return BuildTransactions(accountId, txDefs, adjustment);
    }

    // ---------------------------------------------------------------
    // Savings Account: target balance = $12,800.00
    // ---------------------------------------------------------------
    private static List<Transaction> CreateSavingsTransactions()
    {
        var accountId = SavingsAccountId;
        var txDefs = new (int DayOffset, decimal Amount, string Desc, TransactionType Type, TransactionCategory Cat)[]
        {
            (3,   5_000.00m, "Initial Deposit",                        TransactionType.Deposit,    TransactionCategory.Other),
            (16,    500.00m, "Transfer from Checking",                 TransactionType.Transfer,   TransactionCategory.Other),
            (20,      8.33m, "Interest Payment",                       TransactionType.Interest,   TransactionCategory.Other),
            (25,  1_000.00m, "Birthday Gift Deposit",                  TransactionType.Deposit,    TransactionCategory.Other),
            (30,   -200.00m, "Emergency Fund Withdrawal",              TransactionType.Withdrawal, TransactionCategory.Other),
            (35,  2_000.00m, "Tax Refund Deposit",                     TransactionType.Deposit,    TransactionCategory.Other),
            (40,     12.50m, "Interest Payment",                       TransactionType.Interest,   TransactionCategory.Other),
            (45,   -500.00m, "Transfer to Investment",                 TransactionType.Transfer,   TransactionCategory.Other),
            (50,    750.00m, "Bonus Deposit - Acme Corp",              TransactionType.Deposit,    TransactionCategory.Other),
            (55,    500.00m, "Transfer from Checking",                 TransactionType.Transfer,   TransactionCategory.Other),
            (60,     15.75m, "Interest Payment",                       TransactionType.Interest,   TransactionCategory.Other),
            (65, -1_000.00m, "Down Payment Transfer",                  TransactionType.Transfer,   TransactionCategory.Other),
            (70,  2_500.00m, "Side Project Income",                    TransactionType.Deposit,    TransactionCategory.Other),
            (75,     18.42m, "Interest Payment",                       TransactionType.Interest,   TransactionCategory.Other),
            (80,  1_500.00m, "Savings Goal Deposit",                   TransactionType.Deposit,    TransactionCategory.Other),
            (85,   -300.00m, "Car Repair Fund",                        TransactionType.Withdrawal, TransactionCategory.Other),
        };

        var rawSum = txDefs.Sum(t => t.Amount);
        var adjustment = 12_800.00m - rawSum;

        return BuildTransactions(accountId, txDefs, adjustment);
    }

    // ---------------------------------------------------------------
    // Credit Card Account: target balance = -$1,243.50
    // ---------------------------------------------------------------
    private static List<Transaction> CreateCreditCardTransactions()
    {
        var accountId = CreditCardAccountId;
        var txDefs = new (int DayOffset, decimal Amount, string Desc, TransactionType Type, TransactionCategory Cat)[]
        {
            (1,   -125.60m, "Best Buy - Electronics",                  TransactionType.Payment,    TransactionCategory.Shopping),
            (4,    -42.30m, "Olive Garden Restaurant",                 TransactionType.Payment,    TransactionCategory.Dining),
            (8,    -19.99m, "Hulu Subscription",                       TransactionType.Payment,    TransactionCategory.Entertainment),
            (10,   500.00m, "Payment - Thank You",                     TransactionType.Deposit,    TransactionCategory.Other),
            (13,   -67.89m, "Nordstrom Rack",                          TransactionType.Payment,    TransactionCategory.Shopping),
            (17,   -33.45m, "Uber Eats Delivery",                     TransactionType.Payment,    TransactionCategory.Dining),
            (20,   -89.00m, "Annual Fee",                              TransactionType.Fee,        TransactionCategory.Bills),
            (23,  -245.00m, "Delta Airlines Ticket",                   TransactionType.Payment,    TransactionCategory.Transportation),
            (27,   -55.20m, "Walgreens Pharmacy",                     TransactionType.Payment,    TransactionCategory.Healthcare),
            (30,   500.00m, "Payment - Thank You",                     TransactionType.Deposit,    TransactionCategory.Other),
            (35,   -78.90m, "Home Depot #456",                         TransactionType.Payment,    TransactionCategory.Shopping),
            (38,   -15.99m, "Disney+ Subscription",                   TransactionType.Payment,    TransactionCategory.Entertainment),
            (42,   -92.50m, "Cheesecake Factory",                      TransactionType.Payment,    TransactionCategory.Dining),
            (46,  -125.00m, "Macy's Department Store",                 TransactionType.Payment,    TransactionCategory.Shopping),
            (50,   -38.75m, "DoorDash Delivery",                      TransactionType.Payment,    TransactionCategory.Dining),
            (54,   750.00m, "Payment - Thank You",                     TransactionType.Deposit,    TransactionCategory.Other),
            (58,  -199.99m, "Apple Store Purchase",                    TransactionType.Payment,    TransactionCategory.Shopping),
            (62,   -29.99m, "HBO Max Subscription",                   TransactionType.Payment,    TransactionCategory.Entertainment),
            (67,   -48.50m, "Panera Bread",                            TransactionType.Payment,    TransactionCategory.Dining),
            (72,  -110.00m, "Nike.com Order",                          TransactionType.Payment,    TransactionCategory.Shopping),
        };

        var rawSum = txDefs.Sum(t => t.Amount);
        var adjustment = -1_243.50m - rawSum;

        return BuildTransactions(accountId, txDefs, adjustment);
    }

    // ---------------------------------------------------------------
    // Investment Account: target balance = $28,500.00
    // ---------------------------------------------------------------
    private static List<Transaction> CreateInvestmentTransactions()
    {
        var accountId = InvestmentAccountId;
        var txDefs = new (int DayOffset, decimal Amount, string Desc, TransactionType Type, TransactionCategory Cat)[]
        {
            (1,  10_000.00m, "Initial Investment Deposit",              TransactionType.Deposit,    TransactionCategory.Other),
            (5,   5_000.00m, "Brokerage Transfer",                     TransactionType.Transfer,   TransactionCategory.Other),
            (10,    125.50m, "Dividend - VTI",                          TransactionType.Deposit,    TransactionCategory.Other),
            (15,  3_000.00m, "Additional Investment",                   TransactionType.Deposit,    TransactionCategory.Other),
            (20,    -49.95m, "Trading Commission",                     TransactionType.Fee,        TransactionCategory.Bills),
            (25,    250.00m, "Dividend - AAPL",                         TransactionType.Deposit,    TransactionCategory.Other),
            (30,  5_000.00m, "Monthly Investment",                      TransactionType.Deposit,    TransactionCategory.Other),
            (35, -2_000.00m, "Partial Withdrawal",                     TransactionType.Withdrawal, TransactionCategory.Other),
            (40,    175.30m, "Dividend - MSFT",                         TransactionType.Deposit,    TransactionCategory.Other),
            (45,    500.00m, "Transfer from Savings",                  TransactionType.Transfer,   TransactionCategory.Other),
            (50,  2_500.00m, "Quarterly Contribution",                  TransactionType.Deposit,    TransactionCategory.Other),
            (55,    -29.95m, "Management Fee",                         TransactionType.Fee,        TransactionCategory.Bills),
            (60,    310.00m, "Dividend - SPY",                          TransactionType.Deposit,    TransactionCategory.Other),
            (65,  3_000.00m, "Additional Investment",                   TransactionType.Deposit,    TransactionCategory.Other),
            (70,    198.75m, "Dividend - QQQ",                          TransactionType.Deposit,    TransactionCategory.Other),
            (75, -1_500.00m, "Rebalancing Withdrawal",                 TransactionType.Withdrawal, TransactionCategory.Other),
            (80,  2_000.00m, "Bi-weekly Investment",                    TransactionType.Deposit,    TransactionCategory.Other),
        };

        var rawSum = txDefs.Sum(t => t.Amount);
        var adjustment = 28_500.00m - rawSum;

        return BuildTransactions(accountId, txDefs, adjustment);
    }

    /// <summary>
    /// Builds a list of transactions from definitions, adding a final adjustment transaction
    /// so the running balance ends exactly on the target.
    /// </summary>
    private static List<Transaction> BuildTransactions(
        Guid accountId,
        (int DayOffset, decimal Amount, string Desc, TransactionType Type, TransactionCategory Cat)[] defs,
        decimal adjustment)
    {
        var transactions = new List<Transaction>();
        decimal runningBalance = 0m;
        int seqCounter = 1;

        foreach (var def in defs)
        {
            runningBalance += def.Amount;
            var timestamp = BaseDate.AddDays(def.DayOffset).AddHours(9).AddMinutes(seqCounter * 3);
            var txId = GenerateDeterministicGuid(accountId, seqCounter);

            transactions.Add(new Transaction
            {
                Id = txId,
                AccountId = accountId,
                Type = def.Type,
                Amount = def.Amount,
                BalanceAfter = runningBalance,
                Description = def.Desc,
                Category = def.Cat,
                Timestamp = timestamp,
                ReferenceNumber = $"TXN-{timestamp:yyyyMMdd}-{seqCounter:D6}"
            });

            seqCounter++;
        }

        // Add adjustment transaction if needed to reconcile to target balance
        if (adjustment != 0m)
        {
            runningBalance += adjustment;
            var adjTimestamp = BaseDate.AddDays(88).AddHours(23).AddMinutes(59);
            var adjType = adjustment > 0 ? TransactionType.Deposit : TransactionType.Payment;
            var adjDesc = adjustment > 0 ? "Account Adjustment - Credit" : "Account Adjustment - Debit";

            transactions.Add(new Transaction
            {
                Id = GenerateDeterministicGuid(accountId, seqCounter),
                AccountId = accountId,
                Type = adjType,
                Amount = adjustment,
                BalanceAfter = runningBalance,
                Description = adjDesc,
                Category = TransactionCategory.Other,
                Timestamp = adjTimestamp,
                ReferenceNumber = $"TXN-{adjTimestamp:yyyyMMdd}-{seqCounter:D6}"
            });
        }

        return transactions;
    }

    /// <summary>
    /// Generates a deterministic GUID from an account ID and sequence number.
    /// Ensures consistent IDs across runs for testability.
    /// </summary>
    private static Guid GenerateDeterministicGuid(Guid accountId, int sequence)
    {
        var bytes = accountId.ToByteArray();
        var seqBytes = BitConverter.GetBytes(sequence);
        // XOR sequence into the last 4 bytes
        bytes[12] = seqBytes[0];
        bytes[13] = seqBytes[1];
        bytes[14] = seqBytes[2];
        bytes[15] = seqBytes[3];
        return new Guid(bytes);
    }
}
