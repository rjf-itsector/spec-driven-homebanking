namespace HomeBanking.Contracts.Responses;

/// <summary>Account balance info for agent tool response.</summary>
public record AccountBalanceDto(
    string AccountType,
    string AccountNumber,
    decimal Balance,
    string Currency
);

/// <summary>Account detail info for agent tool response.</summary>
public record AgentAccountDetailDto(
    Guid AccountId,
    string AccountType,
    string AccountNumber,
    decimal Balance,
    string Currency,
    bool IsActive,
    DateTime CreatedAt,
    int TransactionCount
);

/// <summary>Transaction search result for agent.</summary>
public record AgentTransactionDto(
    Guid Id,
    string AccountType,
    string Type,
    decimal Amount,
    decimal BalanceAfter,
    string Description,
    string Category,
    DateTime Timestamp,
    string ReferenceNumber
);

/// <summary>Category spending summary entry.</summary>
public record CategorySpendingDto(
    string Category,
    decimal Total,
    int TransactionCount,
    decimal Percentage
);

/// <summary>Full spending summary response.</summary>
public record SpendingSummaryDto(
    DateTime FromDate,
    DateTime ToDate,
    decimal TotalSpending,
    List<CategorySpendingDto> Categories,
    decimal AverageDaily
);

/// <summary>Unusual transaction entry.</summary>
public record UnusualTransactionDto(
    Guid Id,
    string AccountType,
    string Description,
    decimal Amount,
    string Category,
    decimal CategoryAverage,
    decimal Ratio,
    DateTime Timestamp
);

/// <summary>Agent transfer result.</summary>
public record AgentTransferResultDto(
    bool Success,
    Guid? TransferId,
    decimal? FromBalance,
    decimal? ToBalance,
    string? Error,
    DateTime Timestamp
);
