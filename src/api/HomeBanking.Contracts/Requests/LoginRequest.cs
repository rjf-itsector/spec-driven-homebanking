namespace HomeBanking.Contracts.Requests;

public record LoginRequest(
    string Email,
    string Password
);
