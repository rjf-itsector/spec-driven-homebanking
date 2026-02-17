namespace HomeBanking.Contracts.Responses;

public record TransactionDto(
    Guid Id,
    string Type,
    decimal Amount,
    decimal BalanceAfter,
    string Description,
    string Category,
    DateTime Timestamp,
    string ReferenceNumber,
    Guid? RelatedTransactionId
);
