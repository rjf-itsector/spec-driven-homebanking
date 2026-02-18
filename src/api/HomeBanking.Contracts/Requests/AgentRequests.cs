namespace HomeBanking.Contracts.Requests;

/// <summary>Parameters for searching transactions via the agent.</summary>
public record TransactionSearchParams(
    string? AccountType = null,
    string? Description = null,
    string? Category = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Limit = 10
);

/// <summary>Parameters for generating a spending summary.</summary>
public record SpendingSummaryParams(
    string? AccountType = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
);

/// <summary>Parameters for executing a transfer via the agent.</summary>
public record AgentTransferParams(
    string FromAccountType,
    string ToAccountType,
    decimal Amount,
    string? Description = null
);
