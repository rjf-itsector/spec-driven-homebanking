namespace HomeBanking.Core.Interfaces;

/// <summary>
/// Service for generating and validating JWT tokens.
/// </summary>
public interface IJwtService
{
    /// <summary>Generates a JWT token for the given user.</summary>
    string GenerateToken(Guid userId, string email);

    /// <summary>Validates a JWT token and returns the user ID if valid.</summary>
    Guid? ValidateToken(string token);
}
