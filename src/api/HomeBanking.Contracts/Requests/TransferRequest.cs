namespace HomeBanking.Contracts.Requests;

public record TransferRequest(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string Description
);
