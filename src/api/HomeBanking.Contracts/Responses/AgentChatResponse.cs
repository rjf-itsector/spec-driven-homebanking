namespace HomeBanking.Contracts.Responses;

/// <summary>Response from the AI banking agent.</summary>
public record AgentChatResponse(
    string Response,
    List<string> ToolsUsed,
    TokenUsageDto TokenUsage
);

/// <summary>Token usage statistics for the agent interaction.</summary>
public record TokenUsageDto(
    int Input,
    int Output
);
