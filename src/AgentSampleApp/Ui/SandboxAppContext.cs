using AgentSampleApp.Mxc;
using Microsoft.Extensions.AI;

namespace AgentSampleApp.Ui;

/// <summary>
/// Shared dependencies handed to every screen (the chat and each sandbox example). Bundling
/// them keeps control constructors short and makes the "works without an LLM" path explicit:
/// <see cref="Chat"/> is null when no chat client could be created, and
/// <see cref="ChatUnavailableReason"/> explains why.
/// </summary>
public sealed record SandboxAppContext(
    ISandboxRunner Runner,
    IChatClient? Chat,
    string Workspace,
    string SystemPrompt,
    string? ChatUnavailableReason);
