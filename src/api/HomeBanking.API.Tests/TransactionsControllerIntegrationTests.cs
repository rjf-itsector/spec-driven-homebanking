using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace HomeBanking.API.Tests;

public class TransactionsControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    // Deterministic GUIDs from DataSeeder
    private static readonly Guid CheckingAccountId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public TransactionsControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetTransactions_WithAuth_Returns200WithList()
    {
        // Arrange
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory);

        // Act
        var response = await client.GetAsync($"/api/v1/accounts/{CheckingAccountId}/transactions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        var transactions = content.GetProperty("transactions");
        transactions.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetTransactions_WithPagination_ReturnsHasMoreAndCursor()
    {
        // Arrange
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory);

        // Act — request only 5 transactions (checking has 27)
        var response = await client.GetAsync(
            $"/api/v1/accounts/{CheckingAccountId}/transactions?limit=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        var transactions = content.GetProperty("transactions");
        transactions.GetArrayLength().Should().Be(5);

        var pagination = content.GetProperty("pagination");
        pagination.GetProperty("hasMore").GetBoolean().Should().BeTrue();
        pagination.GetProperty("nextCursor").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetTransaction_ById_Returns200()
    {
        // Arrange — first get a list of transactions to extract an ID
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory);
        var listResponse = await client.GetAsync(
            $"/api/v1/accounts/{CheckingAccountId}/transactions?limit=1");
        var listContent = await listResponse.Content.ReadFromJsonAsync<JsonElement>();
        var firstTx = listContent.GetProperty("transactions").EnumerateArray().First();
        var txId = firstTx.GetProperty("id").GetString();

        // Act
        var response = await client.GetAsync($"/api/v1/transactions/{txId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("id").GetString().Should().Be(txId);
        content.GetProperty("description").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetTransactions_WithoutAuth_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/accounts/{CheckingAccountId}/transactions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
