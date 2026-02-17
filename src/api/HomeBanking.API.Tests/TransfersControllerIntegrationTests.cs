using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using HomeBanking.Contracts.Requests;

namespace HomeBanking.API.Tests;

public class TransfersControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    // Deterministic GUIDs from DataSeeder
    private static readonly Guid CheckingAccountId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SavingsAccountId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public TransfersControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateTransfer_Valid_Returns201()
    {
        // Arrange
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory);
        var request = new TransferRequest(CheckingAccountId, SavingsAccountId, 100.00m, "Test transfer");

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/transfers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("transferId").GetString().Should().NotBeNullOrEmpty();
        content.GetProperty("debitTransactionId").GetString().Should().NotBeNullOrEmpty();
        content.GetProperty("creditTransactionId").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateTransfer_InsufficientFunds_Returns400()
    {
        // Arrange
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory);
        var request = new TransferRequest(CheckingAccountId, SavingsAccountId, 999_999.00m, "Too much");

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/transfers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTransfer_SameAccount_Returns400()
    {
        // Arrange
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory);
        var request = new TransferRequest(CheckingAccountId, CheckingAccountId, 100.00m, "Same account");

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/transfers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTransfer_NegativeAmount_Returns400()
    {
        // Arrange
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory);
        var request = new TransferRequest(CheckingAccountId, SavingsAccountId, -100.00m, "Negative");

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/transfers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTransfer_WithoutAuth_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new TransferRequest(CheckingAccountId, SavingsAccountId, 100.00m, "No auth");

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/transfers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
