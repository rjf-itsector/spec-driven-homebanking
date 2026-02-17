namespace HomeBanking.Contracts.Responses;

public record AccountDto(
    Guid Id,
    string AccountNumber,
    string Type,
    decimal Balance,
    string Currency,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record AccountDetailDto(
    Guid Id,
    string AccountNumber,
    string Type,
    decimal Balance,
    string Currency,
    bool IsActive,
    int TransactionCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
