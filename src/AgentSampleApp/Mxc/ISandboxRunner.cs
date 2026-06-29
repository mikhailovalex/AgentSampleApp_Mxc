namespace AgentSampleApp.Mxc;

/// <summary>
/// Abstraction over the execution sandbox. Keeping the agent behind this
/// interface means the only file that references the MXC SDK is
/// <see cref="MxcSandboxRunner"/> — when the official Microsoft .NET SDK
/// (microsoft/mxc#484) ships, you replace one class, not the agent.
/// </summary>
public interface ISandboxRunner
{
    /// <summary>True when a native MXC executor was found on this host.</summary>
    bool IsSandboxAvailable { get; }

    /// <summary>Human-readable summary of the detected backend(s).</summary>
    string BackendSummary { get; }

    /// <param name="command">Shell command line to execute, e.g. python -c "...".</param>
    /// <param name="restricted">When true, block outbound network and confine the
    /// filesystem to <paramref name="workspaceDir"/>.</param>
    /// <param name="workspaceDir">Read/write working directory exposed to the sandbox.</param>
    Task<SandboxRunResult> RunAsync(
        string command,
        bool restricted,
        string workspaceDir,
        CancellationToken cancellationToken = default);
}
