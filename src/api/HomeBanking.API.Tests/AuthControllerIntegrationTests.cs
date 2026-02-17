using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using HomeBanking.Contracts.Requests;
using HomeBanking.Contracts.Responses;

namespace HomeBanking.API.Tests;

public class AuthControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AuthControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithToken()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest("demo@bank.com", "Demo123!"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.ExpiresIn.Should().BeGreaterThan(0);
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be("demo@bank.com");
        result.User.FirstName.Should().Be("Demo");
        result.User.LastName.Should().Be("User");
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest("demo@bank.com", "WrongPassword!"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithWrongEmail_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest("wrong@email.com", "Demo123!"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInvalidBody_Returns400()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act — send empty strings which fail FluentValidation (NotEmpty, EmailAddress)
        var response = await client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest("", ""));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
