namespace HomeBanking.Core.Exceptions;

/// <summary>
/// Thrown when the AI agent service is unreachable or unavailable.
/// </summary>
public class AgentUnavailableException : Exception
{
    public AgentUnavailableException() : base("Agent service is unavailable.") { }
    public AgentUnavailableException(string message) : base(message) { }
    public AgentUnavailableException(string message, Exception innerException) : base(message, innerException) { }
}
