namespace HomeBanking.Contracts.Responses;

public record LoginResponse(
    string Token,
    int ExpiresIn,
    UserDto User
);

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName
);
