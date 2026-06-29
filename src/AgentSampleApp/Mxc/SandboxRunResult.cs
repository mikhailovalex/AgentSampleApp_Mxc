namespace AgentSampleApp.Mxc;

/// <summary>Outcome of running a command through the MXC sandbox.</summary>
public sealed record SandboxRunResult(
    int ExitCode,
    string Stdout,
    string Stderr,
    bool Restricted,
    string Backend);
