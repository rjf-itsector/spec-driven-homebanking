using System.ComponentModel.DataAnnotations;

namespace HomeBanking.Contracts.Requests;

/// <summary>Request body for the agent chat endpoint.</summary>
public record AgentChatRequest
{
    /// <summary>The user's message to the agent.</summary>
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public required string Message { get; init; }

    /// <summary>Previous conversation history for context.</summary>
    public List<ConversationMessage>? ConversationHistory { get; init; }
}

/// <summary>A single message in the conversation history.</summary>
public record ConversationMessage
{
    /// <summary>The role of the message sender ("user" or "assistant").</summary>
    [Required]
    public required string Role { get; init; }

    /// <summary>The message content.</summary>
    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public required string Content { get; init; }
}
