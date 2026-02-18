using Microsoft.EntityFrameworkCore;
using HomeBanking.Contracts.Requests;
using HomeBanking.Contracts.Responses;
using HomeBanking.Core.Entities;
using HomeBanking.Core.Interfaces;
using HomeBanking.Infrastructure.Data;

namespace HomeBanking.Infrastructure.Services;

/// <summary>
/// Implements agent tool functions with direct database access.
/// All queries are scoped to the authenticated user.
/// </summary>
public class AgentToolService : IAgentToolService
{
    private readonly HomeBankingDbContext _context;

    public AgentToolService(HomeBankingDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<List<AccountBalanceDto>> GetAccountBalancesAsync(Guid userId)
    {
        return await _context.Accounts
            .Where(a => a.UserId == userId && a.IsActive)
            .Select(a => new AccountBalanceDto(
                a.Type.ToString(),
                a.AccountNumber,
                a.Balance,
                a.Currency
            ))
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<AgentAccountDetailDto?> GetAccountDetailsAsync(Guid userId, string accountType)
    {
        if (!Enum.TryParse<AccountType>(accountType, true, out var parsedType))
            return null;

        var account = await _context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Type == parsedType && a.IsActive);

        if (account == null)
            return null;

        return new AgentAccountDetailDto(
            account.Id,
            account.Type.ToString(),
            account.AccountNumber,
            account.Balance,
            account.Currency,
            account.IsActive,
            account.CreatedAt,
            account.Transactions.Count
        );
    }

    /// <inheritdoc />
    public async Task<(List<AgentTransactionDto> Transactions, int TotalCount)> SearchTransactionsAsync(
        Guid userId, TransactionSearchParams searchParams)
    {
        var limit = Math.Clamp(searchParams.Limit, 1, 20);

        var query = _context.Transactions
            .Include(t => t.Account)
            .Where(t => t.Account.UserId == userId && t.Account.IsActive)
            .AsQueryable();

        // Filter by account type
        if (!string.IsNullOrEmpty(searchParams.AccountType) &&
            Enum.TryParse<AccountType>(searchParams.AccountType, true, out var acctType))
        {
            query = query.Where(t => t.Account.Type == acctType);
        }

        // Filter by description (case-insensitive contains)
        if (!string.IsNullOrEmpty(searchParams.Description))
        {
            query = query.Where(t => t.Description.ToLower().Contains(searchParams.Description.ToLower()));
        }

        // Filter by category
        if (!string.IsNullOrEmpty(searchParams.Category) &&
            Enum.TryParse<TransactionCategory>(searchParams.Category, true, out var cat))
        {
            query = query.Where(t => t.Category == cat);
        }

        // Filter by amount range (absolute value)
        if (searchParams.MinAmount.HasValue)
        {
            var min = searchParams.MinAmount.Value;
            query = query.Where(t => Math.Abs(t.Amount) >= min);
        }

        if (searchParams.MaxAmount.HasValue)
        {
            var max = searchParams.MaxAmount.Value;
            query = query.Where(t => Math.Abs(t.Amount) <= max);
        }

        // Filter by date range
        if (searchParams.FromDate.HasValue)
            query = query.Where(t => t.Timestamp >= searchParams.FromDate.Value);

        if (searchParams.ToDate.HasValue)
            query = query.Where(t => t.Timestamp <= searchParams.ToDate.Value);

        var totalCount = await query.CountAsync();

        var transactions = await query
            .OrderByDescending(t => t.Timestamp)
            .Take(limit)
            .Select(t => new AgentTransactionDto(
                t.Id,
                t.Account.Type.ToString(),
                t.Type.ToString(),
                t.Amount,
                t.BalanceAfter,
                t.Description,
                t.Category.ToString(),
                t.Timestamp,
                t.ReferenceNumber
            ))
            .ToListAsync();

        return (transactions, totalCount);
    }

    /// <inheritdoc />
    public async Task<SpendingSummaryDto> GetSpendingSummaryAsync(Guid userId, SpendingSummaryParams summaryParams)
    {
        var toDate = summaryParams.ToDate ?? DateTime.UtcNow;
        var fromDate = summaryParams.FromDate ?? toDate.AddDays(-30);

        var query = _context.Transactions
            .Include(t => t.Account)
            .Where(t => t.Account.UserId == userId && t.Account.IsActive)
            .Where(t => t.Timestamp >= fromDate && t.Timestamp <= toDate)
            .Where(t => t.Amount < 0) // Only spending (negative amounts)
            .AsQueryable();

        if (!string.IsNullOrEmpty(summaryParams.AccountType) &&
            Enum.TryParse<AccountType>(summaryParams.AccountType, true, out var acctType))
        {
            query = query.Where(t => t.Account.Type == acctType);
        }

        var transactions = await query.ToListAsync();

        var totalSpending = transactions.Sum(t => Math.Abs(t.Amount));
        var days = Math.Max(1, (toDate - fromDate).Days);
        var averageDaily = totalSpending / days;

        var categories = transactions
            .GroupBy(t => t.Category)
            .Select(g => new CategorySpendingDto(
                g.Key.ToString(),
                g.Sum(t => Math.Abs(t.Amount)),
                g.Count(),
                totalSpending > 0 ? Math.Round(g.Sum(t => Math.Abs(t.Amount)) / totalSpending * 100, 1) : 0
            ))
            .OrderByDescending(c => c.Total)
            .ToList();

        return new SpendingSummaryDto(fromDate, toDate, totalSpending, categories, Math.Round(averageDaily, 2));
    }

    /// <inheritdoc />
    public async Task<List<UnusualTransactionDto>> DetectUnusualTransactionsAsync(
        Guid userId, string? accountType = null, decimal threshold = 2.0m)
    {
        var query = _context.Transactions
            .Include(t => t.Account)
            .Where(t => t.Account.UserId == userId && t.Account.IsActive)
            .Where(t => t.Amount < 0) // Only spending
            .AsQueryable();

        if (!string.IsNullOrEmpty(accountType) &&
            Enum.TryParse<AccountType>(accountType, true, out var acctType))
        {
            query = query.Where(t => t.Account.Type == acctType);
        }

        var transactions = await query.ToListAsync();

        // Group by category and compute averages
        var categoryAverages = transactions
            .GroupBy(t => t.Category)
            .Where(g => g.Count() > 1) // Need at least 2 transactions to define "unusual"
            .ToDictionary(
                g => g.Key,
                g => g.Average(t => Math.Abs(t.Amount))
            );

        var unusual = new List<UnusualTransactionDto>();
        foreach (var txn in transactions)
        {
            if (categoryAverages.TryGetValue(txn.Category, out var avg) && avg > 0)
            {
                var absAmount = Math.Abs(txn.Amount);
                var ratio = absAmount / avg;
                if (ratio >= threshold)
                {
                    unusual.Add(new UnusualTransactionDto(
                        txn.Id,
                        txn.Account.Type.ToString(),
                        txn.Description,
                        txn.Amount,
                        txn.Category.ToString(),
                        Math.Round(avg, 2),
                        Math.Round(ratio, 2),
                        txn.Timestamp
                    ));
                }
            }
        }

        return unusual.OrderByDescending(u => u.Ratio).ToList();
    }

    /// <inheritdoc />
    public async Task<AgentTransferResultDto> ExecuteTransferAsync(Guid userId, AgentTransferParams transferParams)
    {
        // Parse account types
        if (!Enum.TryParse<AccountType>(transferParams.FromAccountType, true, out var fromType))
            return new AgentTransferResultDto(false, null, null, null, $"Invalid source account type: {transferParams.FromAccountType}", DateTime.UtcNow);

        if (!Enum.TryParse<AccountType>(transferParams.ToAccountType, true, out var toType))
            return new AgentTransferResultDto(false, null, null, null, $"Invalid destination account type: {transferParams.ToAccountType}", DateTime.UtcNow);

        if (fromType == toType)
            return new AgentTransferResultDto(false, null, null, null, "Cannot transfer to the same account", DateTime.UtcNow);

        if (transferParams.Amount <= 0 || transferParams.Amount > 1_000_000)
            return new AgentTransferResultDto(false, null, null, null, "Transfer amount must be between $0.01 and $1,000,000", DateTime.UtcNow);

        var fromAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Type == fromType && a.IsActive);

        var toAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Type == toType && a.IsActive);

        if (fromAccount == null)
            return new AgentTransferResultDto(false, null, null, null, $"Source account ({transferParams.FromAccountType}) not found", DateTime.UtcNow);

        if (toAccount == null)
            return new AgentTransferResultDto(false, null, null, null, $"Destination account ({transferParams.ToAccountType}) not found", DateTime.UtcNow);

        if (fromAccount.Balance < transferParams.Amount)
            return new AgentTransferResultDto(false, null, null, null,
                $"Insufficient funds in {transferParams.FromAccountType} (balance: ${fromAccount.Balance:F2}) for a ${transferParams.Amount:F2} transfer", DateTime.UtcNow);

        // Execute transfer (same logic as TransfersController)
        var now = DateTime.UtcNow;
        var referenceNumber = $"TXN-{now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        var debitId = Guid.NewGuid();
        var creditId = Guid.NewGuid();

        var debitTransaction = new Transaction
        {
            Id = debitId,
            AccountId = fromAccount.Id,
            Type = TransactionType.Transfer,
            Amount = -transferParams.Amount,
            BalanceAfter = fromAccount.Balance - transferParams.Amount,
            Description = transferParams.Description ?? $"Transfer to {toAccount.AccountNumber}",
            Category = TransactionCategory.Other,
            Timestamp = now,
            ReferenceNumber = referenceNumber,
            RelatedTransactionId = creditId
        };

        var creditTransaction = new Transaction
        {
            Id = creditId,
            AccountId = toAccount.Id,
            Type = TransactionType.Transfer,
            Amount = transferParams.Amount,
            BalanceAfter = toAccount.Balance + transferParams.Amount,
            Description = transferParams.Description ?? $"Transfer from {fromAccount.AccountNumber}",
            Category = TransactionCategory.Other,
            Timestamp = now,
            ReferenceNumber = referenceNumber,
            RelatedTransactionId = debitId
        };

        fromAccount.Balance -= transferParams.Amount;
        fromAccount.UpdatedAt = now;
        toAccount.Balance += transferParams.Amount;
        toAccount.UpdatedAt = now;

        _context.Transactions.AddRange(debitTransaction, creditTransaction);
        await _context.SaveChangesAsync();

        return new AgentTransferResultDto(
            true,
            debitId,
            fromAccount.Balance,
            toAccount.Balance,
            null,
            now
        );
    }
}
