using HomeBanking.Contracts.Requests;
using HomeBanking.Contracts.Responses;

namespace HomeBanking.Core.Interfaces;

/// <summary>
/// Service providing agent tool functions for the AI banking assistant.
/// All methods are scoped to the authenticated user's data.
/// </summary>
public interface IAgentToolService
{
    /// <summary>Retrieves all account balances for the user.</summary>
    Task<List<AccountBalanceDto>> GetAccountBalancesAsync(Guid userId);

    /// <summary>Retrieves detailed information about a specific account by type.</summary>
    Task<AgentAccountDetailDto?> GetAccountDetailsAsync(Guid userId, string accountType);

    /// <summary>Searches transactions with optional filters.</summary>
    Task<(List<AgentTransactionDto> Transactions, int TotalCount)> SearchTransactionsAsync(Guid userId, TransactionSearchParams searchParams);

    /// <summary>Computes spending aggregates by category for a date range.</summary>
    Task<SpendingSummaryDto> GetSpendingSummaryAsync(Guid userId, SpendingSummaryParams summaryParams);

    /// <summary>Finds transactions significantly higher than category average.</summary>
    Task<List<UnusualTransactionDto>> DetectUnusualTransactionsAsync(Guid userId, string? accountType = null, decimal threshold = 2.0m);

    /// <summary>Executes a transfer between two accounts. Requires prior user confirmation.</summary>
    Task<AgentTransferResultDto> ExecuteTransferAsync(Guid userId, AgentTransferParams transferParams);
}
