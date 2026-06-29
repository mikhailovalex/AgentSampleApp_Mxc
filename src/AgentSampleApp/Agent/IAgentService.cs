namespace AgentSampleApp.Agent;

/// <summary>A minimal multi-turn agent: keeps history and runs one turn at a time.</summary>
public interface IAgentService
{
    /// <summary>Sends a user message and returns the assistant's final text reply.
    /// Any tool calls the model makes are executed during this call.</summary>
    Task<string> SendAsync(string userMessage, CancellationToken cancellationToken = default);
}
