using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace HomeBanking.API.Tests;

public class AccountsControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    // Deterministic GUIDs from DataSeeder
    private static readonly Guid CheckingAccountId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SavingsAccountId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public AccountsControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAccounts_WithAuth_Returns200WithAccounts()
    {
        // Arrange
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/accounts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        var accounts = content.GetProperty("accounts");
        accounts.GetArrayLength().Should().Be(4);
    }

    [Fact]
    public async Task GetAccountById_WithAuth_Returns200WithDetails()
    {
        // Arrange
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory);

        // Act
        var response = await client.GetAsync($"/api/v1/accounts/{CheckingAccountId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
        content.GetProperty("accountNumber").GetString().Should().Be("****1234");
        content.GetProperty("type").GetString().Should().Be("Checking");
        content.GetProperty("balance").GetDecimal().Should().Be(5_250.00m);
        content.GetProperty("currency").GetString().Should().Be("USD");
        content.GetProperty("isActive").GetBoolean().Should().BeTrue();
        content.GetProperty("transactionCount").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAccounts_WithoutAuth_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/accounts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAccount_NonExistent_Returns404()
    {
        // Arrange
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/v1/accounts/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
