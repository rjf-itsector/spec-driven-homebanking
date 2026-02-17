namespace HomeBanking.Contracts.Responses;

public record TransferResponse(
    Guid TransferId,
    Guid DebitTransactionId,
    Guid CreditTransactionId,
    decimal FromAccountBalance,
    decimal ToAccountBalance,
    DateTime Timestamp
);
