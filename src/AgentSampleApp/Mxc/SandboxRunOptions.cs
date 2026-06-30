namespace AgentSampleApp.Mxc;

/// <summary>
/// Application-level description of the sandbox policy for a single run. This is
/// deliberately <b>not</b> the MXC SDK's <c>SandboxPolicy</c>: keeping it app-owned
/// preserves the invariant that only <see cref="MxcSandboxRunner"/> references the SDK.
/// <see cref="MxcSandboxRunner"/> translates these fields into a real policy.
///
/// The workspace passed to <c>RunAsync</c> is always read/write; the path lists below
/// layer on top of it.
/// </summary>
public sealed record SandboxRunOptions
{
    /// <summary>Allow outbound network. Maps to <c>NetworkPolicy.AllowOutbound</c>.</summary>
    public bool AllowOutbound { get; init; }

    /// <summary>Extra read/write folders beyond the workspace.</summary>
    public IReadOnlyList<string> ReadwritePaths { get; init; } = [];

    /// <summary>Extra read-only folders (in addition to the configured tool paths).</summary>
    public IReadOnlyList<string> ReadonlyPaths { get; init; } = [];

    /// <summary>Folders blocked even if otherwise readable/writable.</summary>
    public IReadOnlyList<string> DeniedPaths { get; init; } = [];

    // Note: per-host allow/block lists (NetworkPolicy.AllowedHosts / BlockedHosts) are not yet
    // supported by the Windows executor — it rejects any policy that sets them — so they are
    // intentionally not surfaced here. Network access is all-or-nothing via AllowOutbound.

    /// <summary>Hard wall-clock limit for the run, in milliseconds. Null = no limit.</summary>
    public int? TimeoutMs { get; init; }

    /// <summary>Whether the sandboxed process may create windows (GUI subprocesses).</summary>
    public bool AllowWindows { get; init; } = true;

    /// <summary>Clipboard access granted to the sandboxed process.</summary>
    public ClipboardAccess Clipboard { get; init; } = ClipboardAccess.All;

    /// <summary>Whether the sandboxed process may inject synthetic keyboard/mouse input.</summary>
    public bool AllowInputInjection { get; init; } = true;

    /// <summary>
    /// Convenience for the original Restricted/Permissive toggle: restricted blocks
    /// outbound network and confines the filesystem to the workspace; everything else
    /// stays at its permissive default.
    /// </summary>
    public static SandboxRunOptions ForToggle(bool restricted) =>
        new() { AllowOutbound = !restricted };
}

/// <summary>Mirror of the SDK's clipboard policy so callers don't reference the SDK.</summary>
public enum ClipboardAccess
{
    None,
    Read,
    Write,
    All,
}
