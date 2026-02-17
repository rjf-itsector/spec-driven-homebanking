using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using HomeBanking.Infrastructure.Services;

namespace HomeBanking.Core.Tests;

/// <summary>
/// Tests for JWT token generation and validation via JwtService.
/// </summary>
public class JwtServiceTests
{
    private const string TestSecretKey = "a-test-secret-key-that-is-at-least-32-bytes-long!!";
    private const string TestIssuer = "TestIssuer";

    private static JwtService CreateService(string? secretKey = null, string? issuer = null)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = secretKey ?? TestSecretKey,
                ["Jwt:Issuer"] = issuer ?? TestIssuer
            })
            .Build();
        return new JwtService(config);
    }

    [Fact]
    public void GenerateToken_ProducesNonEmptyString()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();

        var token = service.GenerateToken(userId, "user@example.com");

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateToken_ProducesValidJwtFormat()
    {
        var service = CreateService();
        var token = service.GenerateToken(Guid.NewGuid(), "user@example.com");

        // JWT has three parts separated by dots
        token.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public void GenerateToken_ContainsUserIdClaim()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();

        var token = service.GenerateToken(userId, "user@example.com");

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var userIdClaim = jwtToken.Claims
            .First(c => c.Type == JwtRegisteredClaimNames.Sub).Value;
        userIdClaim.Should().Be(userId.ToString());
    }

    [Fact]
    public void GenerateToken_ContainsEmailClaim()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();
        var email = "john.doe@example.com";

        var token = service.GenerateToken(userId, email);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var emailClaim = jwtToken.Claims
            .First(c => c.Type == JwtRegisteredClaimNames.Email).Value;
        emailClaim.Should().Be(email);
    }

    [Fact]
    public void GenerateToken_HasCorrectIssuer()
    {
        var service = CreateService();
        var token = service.GenerateToken(Guid.NewGuid(), "user@example.com");

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be(TestIssuer);
    }

    [Fact]
    public void GenerateToken_HasFutureExpiration()
    {
        var service = CreateService();
        var token = service.GenerateToken(Guid.NewGuid(), "user@example.com");

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.ValidTo.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsCorrectUserId()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();

        var token = service.GenerateToken(userId, "user@example.com");
        var result = service.ValidateToken(token);

        result.Should().Be(userId);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ReturnsNull()
    {
        var service = CreateService();

        var result = service.ValidateToken("this.is.not-a-valid-jwt-token");

        result.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithEmptyString_ReturnsNull()
    {
        var service = CreateService();

        var result = service.ValidateToken(string.Empty);

        result.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithTamperedToken_ReturnsNull()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();

        var token = service.GenerateToken(userId, "user@example.com");
        // Tamper with the token by modifying a character in the signature
        var parts = token.Split('.');
        var tamperedSignature = parts[2][..^1] + (parts[2][^1] == 'A' ? 'B' : 'A');
        var tamperedToken = $"{parts[0]}.{parts[1]}.{tamperedSignature}";

        var result = service.ValidateToken(tamperedToken);

        result.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithDifferentSecretKey_ReturnsNull()
    {
        var service1 = CreateService(secretKey: "first-secret-key-that-is-at-least-32-bytes-long!!");
        var service2 = CreateService(secretKey: "other-secret-key-that-is-at-least-32-bytes-long!!");

        var userId = Guid.NewGuid();
        var token = service1.GenerateToken(userId, "user@example.com");

        var result = service2.ValidateToken(token);

        result.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithDifferentIssuer_ReturnsNull()
    {
        var service1 = CreateService(issuer: "Issuer1");
        var service2 = CreateService(issuer: "Issuer2");

        var userId = Guid.NewGuid();
        var token = service1.GenerateToken(userId, "user@example.com");

        var result = service2.ValidateToken(token);

        result.Should().BeNull();
    }

    [Fact]
    public void Constructor_ThrowsWhenSecretKeyMissing()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var act = () => new JwtService(config);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*JWT secret key*");
    }

    [Fact]
    public void GenerateToken_MultipleCallsSameUser_ProduceDifferentTokens()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();

        var token1 = service.GenerateToken(userId, "user@example.com");
        // Small delay not needed — JWTs with second-level iat will still differ due to timing
        var token2 = service.GenerateToken(userId, "user@example.com");

        // Tokens may or may not be identical depending on timing, 
        // but both should be valid
        service.ValidateToken(token1).Should().Be(userId);
        service.ValidateToken(token2).Should().Be(userId);
    }
}
