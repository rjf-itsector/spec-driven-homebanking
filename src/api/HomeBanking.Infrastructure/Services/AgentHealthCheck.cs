using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace HomeBanking.Infrastructure.Services;

/// <summary>
/// Health check for the Azure AI Foundry Agent Service connectivity.
/// Reports Degraded (not Unhealthy) if the agent is unavailable,
/// ensuring the main application health remains Healthy.
/// </summary>
public class AgentHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AgentHealthCheck> _logger;

    public AgentHealthCheck(IConfiguration configuration, ILogger<AgentHealthCheck> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var endpoint = _configuration["AzureAI:ProjectEndpoint"];
        var modelDeployment = _configuration["AzureAI:ModelDeploymentName"];
        var apiKey = _configuration["AzureAI:ApiKey"];

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Agent health: Degraded — AzureAI configuration is incomplete");
            return Task.FromResult(HealthCheckResult.Degraded(
                "Azure AI endpoint or API key is not configured. Agent features are unavailable.",
                data: new Dictionary<string, object>
                {
                    { "endpoint_configured", false },
                    { "model_deployment", modelDeployment ?? "not set" }
                }));
        }

        // Endpoint is configured - report healthy
        // Note: We don't make actual HTTP calls during health checks to avoid
        // adding latency and consuming tokens. The actual connectivity is
        // validated on first agent request.
        _logger.LogDebug("Agent health: Healthy — endpoint configured");
        return Task.FromResult(HealthCheckResult.Healthy(
            "Azure AI Foundry endpoint is configured.",
            data: new Dictionary<string, object>
            {
                { "endpoint_configured", true },
                { "model_deployment", modelDeployment ?? "gpt-4o" }
            }));
    }
}
