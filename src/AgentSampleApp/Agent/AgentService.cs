using Microsoft.Extensions.AI;

namespace AgentSampleApp.Agent;

/// <summary>
/// Thin wrapper over an <see cref="IChatClient"/> that holds conversation history
/// and exposes the registered tools on every turn. The function-invocation
/// middleware (added in <see cref="ChatClientFactory"/>) runs the tools and feeds
/// their results back to the model, so <see cref="SendAsync"/> returns only the
/// final natural-language answer.
/// </summary>
public sealed class AgentService : IAgentService
{
    private readonly IChatClient _chat;
    private readonly ChatOptions _options;
    private readonly List<ChatMessage> _history = new();

    public AgentService(IChatClient chat, IReadOnlyList<AITool> tools, string systemPrompt)
    {
        _chat = chat;
        _options = new ChatOptions { Tools = tools.ToList() };
        _history.Add(new ChatMessage(ChatRole.System, systemPrompt));
    }

    public async Task<string> SendAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        _history.Add(new ChatMessage(ChatRole.User, userMessage));

        ChatResponse response = await _chat
            .GetResponseAsync(_history, _options, cancellationToken)
            .ConfigureAwait(false);

        // Persist assistant + any tool messages so the next turn has full context.
        _history.AddRange(response.Messages);

        return response.Text;
    }
}
