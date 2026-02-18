using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using HomeBanking.Contracts.Requests;
using HomeBanking.Contracts.Responses;
using HomeBanking.Core.Interfaces;

namespace HomeBanking.API.Tests;

public class AgentMockTestWebApplicationFactory : TestWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IBankingAgentService));
            if (descriptor != null) services.Remove(descriptor);

            var mockAgent = new Mock<IBankingAgentService>();
            mockAgent
                .Setup(x => x.ChatAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<List<ConversationMessage>?>()))
                .ReturnsAsync(new AgentChatResponse(
                    "Your checking account balance is $5,250.00.",
                    new List<string> { "get_account_balances" },
                    new TokenUsageDto(450, 35)));

            services.AddScoped<IBankingAgentService>(_ => mockAgent.Object);
        });
    }
}

public class AgentUnavailableTestWebApplicationFactory : TestWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureAI:ProjectEndpoint"] = "",
                ["AzureAI:ApiKey"] = "",
                ["AzureAI:ModelDeploymentName"] = ""
            });
        });
    }
}

public class AgentControllerIntegrationTests : IClassFixture<AgentMockTestWebApplicationFactory>
{
    private readonly AgentMockTestWebApplicationFactory _factory;

    public AgentControllerIntegrationTests(AgentMockTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Chat_WithoutAuth_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new AgentChatRequest { Message = "What is my balance?" };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/agent/chat", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Chat_EmptyMessage_Returns400()
    {
        // Arrange
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory);
        var request = new { Message = "", ConversationHistory = (List<ConversationMessage>?)null };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/agent/chat", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Chat_MessageExceeds500Chars_Returns400()
    {
        // Arrange
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory);
        var longMessage = new string('A', 501);
        var request = new AgentChatRequest { Message = longMessage };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/agent/chat", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Chat_HistoryExceeds20Items_Returns400()
    {
        // Arrange
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory);
        var history = Enumerable.Range(0, 21)
            .Select(i => new ConversationMessage { Role = "user", Content = $"Message {i}" })
            .ToList();
        var request = new AgentChatRequest { Message = "Hello", ConversationHistory = history };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/agent/chat", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Chat_InvalidRoleInHistory_Returns400()
    {
        // Arrange
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory);
        var history = new List<ConversationMessage>
        {
            new() { Role = "system", Content = "You are a helpful assistant." }
        };
        var request = new AgentChatRequest { Message = "Hello", ConversationHistory = history };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/agent/chat", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Chat_ValidRequest_Returns200WithExpectedData()
    {
        // Arrange
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory);
        var request = new AgentChatRequest { Message = "What is my balance?" };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/agent/chat", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("response").GetString().Should().Be("Your checking account balance is $5,250.00.");
        body.GetProperty("toolsUsed").EnumerateArray().Should().ContainSingle()
            .Which.GetString().Should().Be("get_account_balances");
    }

    [Fact]
    public async Task Chat_ValidRequest_ResponseContainsAllExpectedFields()
    {
        // Arrange
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory);
        var request = new AgentChatRequest { Message = "What is my balance?" };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/agent/chat", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("response", out _).Should().BeTrue();
        body.TryGetProperty("toolsUsed", out _).Should().BeTrue();
        body.TryGetProperty("tokenUsage", out var tokenUsage).Should().BeTrue();
        tokenUsage.GetProperty("input").GetInt32().Should().Be(450);
        tokenUsage.GetProperty("output").GetInt32().Should().Be(35);
    }
}

public class AgentControllerAvailabilityTests : IClassFixture<AgentUnavailableTestWebApplicationFactory>
{
    private readonly AgentUnavailableTestWebApplicationFactory _factory;

    public AgentControllerAvailabilityTests(AgentUnavailableTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Chat_AgentUnavailable_Returns503()
    {
        // Arrange
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory);
        var request = new AgentChatRequest { Message = "What is my balance?" };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/agent/chat", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task Chat_AgentUnavailable_ResponseContainsMessage()
    {
        // Arrange
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory);
        var request = new AgentChatRequest { Message = "What is my balance?" };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/agent/chat", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("message").GetString().Should().NotBeNullOrEmpty();
    }
}
