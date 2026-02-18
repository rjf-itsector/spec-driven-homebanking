using HomeBanking.Contracts.Requests;
using HomeBanking.Contracts.Responses;

namespace HomeBanking.Core.Interfaces;

/// <summary>
/// Service for AI banking agent chat interactions.
/// </summary>
public interface IBankingAgentService
{
    /// <summary>
    /// Sends a message to the banking agent and returns the response.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <param name="message">The user's chat message.</param>
    /// <param name="conversationHistory">Optional prior conversation messages for context.</param>
    /// <returns>The agent's response with metadata.</returns>
    Task<AgentChatResponse> ChatAsync(Guid userId, string message, List<ConversationMessage>? conversationHistory);
}
