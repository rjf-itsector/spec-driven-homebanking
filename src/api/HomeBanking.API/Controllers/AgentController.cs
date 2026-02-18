using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HomeBanking.Contracts.Requests;
using HomeBanking.Contracts.Responses;
using HomeBanking.Core.Exceptions;
using HomeBanking.Core.Interfaces;

namespace HomeBanking.API.Controllers;

/// <summary>
/// Controller for AI banking agent chat interactions.
/// </summary>
[ApiController]
[Route("api/v1/agent")]
[Authorize]
public class AgentController : ControllerBase
{
    private readonly IBankingAgentService _agentService;
    private readonly ILogger<AgentController> _logger;

    // Simple in-memory rate limiter (per-user, per-minute)
    private static readonly ConcurrentDictionary<string, Queue<DateTime>> _rateLimiter = new();
    private const int RateLimitPerMinute = 10;

    public AgentController(IBankingAgentService agentService, ILogger<AgentController> logger)
    {
        _agentService = agentService;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Send a message to the banking agent and receive a response.
    /// </summary>
    /// <param name="request">The chat message and optional conversation history.</param>
    /// <returns>The agent's response with metadata.</returns>
    /// <response code="200">Agent response returned successfully.</response>
    /// <response code="400">Invalid request (empty message, message too long, too many history items).</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="429">Rate limit exceeded (10 requests per minute).</response>
    /// <response code="503">Agent service is unavailable.</response>
    [HttpPost("chat")]
    [ProducesResponseType(typeof(AgentChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Chat([FromBody] AgentChatRequest request)
    {
        var userId = GetCurrentUserId();

        // Validate message
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "Message is required" });
        }

        if (request.Message.Length > 500)
        {
            return BadRequest(new { message = "Message must not exceed 500 characters" });
        }

        // Validate conversation history
        if (request.ConversationHistory != null)
        {
            if (request.ConversationHistory.Count > 20)
            {
                return BadRequest(new { message = "Conversation history must not exceed 20 messages" });
            }

            foreach (var msg in request.ConversationHistory)
            {
                if (string.IsNullOrWhiteSpace(msg.Role) || (msg.Role != "user" && msg.Role != "assistant"))
                {
                    return BadRequest(new { message = "Each history message must have role 'user' or 'assistant'" });
                }

                if (string.IsNullOrWhiteSpace(msg.Content) || msg.Content.Length > 2000)
                {
                    return BadRequest(new { message = "Each history message content must be between 1 and 2000 characters" });
                }
            }
        }

        // Rate limiting
        if (IsRateLimited(userId.ToString()))
        {
            Response.Headers.Append("Retry-After", "60");
            return StatusCode(StatusCodes.Status429TooManyRequests, new { message = "Rate limit exceeded. Try again in 60 seconds." });
        }

        try
        {
            var response = await _agentService.ChatAsync(userId, request.Message, request.ConversationHistory);
            return Ok(response);
        }
        catch (AgentUnavailableException ex)
        {
            _logger.LogWarning(ex, "Agent service unavailable");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Agent service is temporarily unavailable. Please try again later." });
        }
    }

    private static bool IsRateLimited(string userKey)
    {
        var now = DateTime.UtcNow;
        var queue = _rateLimiter.GetOrAdd(userKey, _ => new Queue<DateTime>());

        lock (queue)
        {
            // Remove entries older than 1 minute
            while (queue.Count > 0 && queue.Peek() < now.AddMinutes(-1))
            {
                queue.Dequeue();
            }

            if (queue.Count >= RateLimitPerMinute)
            {
                return true;
            }

            queue.Enqueue(now);
            return false;
        }
    }
}
