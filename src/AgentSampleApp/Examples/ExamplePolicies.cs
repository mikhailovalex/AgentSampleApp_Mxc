using Sabbour.Mxc.Sdk;

namespace AgentSampleApp.Examples;

/// <summary>
/// Worked examples of <see cref="SandboxPolicy"/> configurations. A policy declares
/// *intent* — what the sandboxed process may touch — and the native executor enforces it.
///
/// These are reference recipes: copy the one that matches your scenario into
/// <c>MxcSandboxRunner.RunAsync</c> (or build your own from the same fields). See the
/// "Sandbox policy reference" section of the README for every field.
/// </summary>
public static class ExamplePolicies
{
    private const string Schema = "0.6.0-alpha"; // pin to your wxc-exec.exe (v0.6.x).

    /// <summary>
    /// Recipe 1 — Restricted (the app's default). No outbound network; the only writable
    /// location is the workspace. This is the safe baseline for untrusted code.
    /// </summary>
    public static SandboxPolicy Restricted(string workspace) => new()
    {
        Version = Schema,
        Network = new NetworkPolicy { AllowOutbound = false },
        Filesystem = new FilesystemPolicy { ReadwritePaths = [workspace] },
    };

    /// <summary>
    /// Recipe 2 — Permissive. Outbound network is allowed (e.g. to fetch a URL or install a
    /// package). Filesystem is still confined to the workspace.
    /// </summary>
    public static SandboxPolicy Permissive(string workspace) => new()
    {
        Version = Schema,
        Network = new NetworkPolicy { AllowOutbound = true },
        Filesystem = new FilesystemPolicy { ReadwritePaths = [workspace] },
    };

    /// <summary>
    /// Recipe 3 — Workspace + read-only tools. Grants read access to interpreter folders
    /// (Python, Node, ...) so their executables and DLLs load. This is exactly what the
    /// app's <c>Agent:ToolPaths</c> setting produces.
    /// </summary>
    public static SandboxPolicy WithTools(string workspace, params string[] toolDirs) => new()
    {
        Version = Schema,
        Network = new NetworkPolicy { AllowOutbound = false },
        Filesystem = new FilesystemPolicy
        {
            ReadwritePaths = [workspace],
            ReadonlyPaths = toolDirs,
        },
    };

    /// <summary>
    /// Recipe 4 — Host allow-list. Outbound is on, but only the named hosts are reachable —
    /// a middle ground between Restricted and fully Permissive.
    ///
    /// Note: current Windows executors do not support per-host allow/block lists — they reject
    /// any policy that sets them with "network.allowedHosts / network.blockedHosts are not yet
    /// supported on Windows". Use this on an executor that supports it, or rely on
    /// <see cref="NetworkPolicy.AllowOutbound"/> plus a proxy.
    /// </summary>
    public static SandboxPolicy NetworkAllowList(string workspace, params string[] allowedHosts) => new()
    {
        Version = Schema,
        Network = new NetworkPolicy
        {
            AllowOutbound = true,
            AllowedHosts = allowedHosts,   // e.g. "api.github.com", "pypi.org"
        },
        Filesystem = new FilesystemPolicy { ReadwritePaths = [workspace] },
    };

    /// <summary>
    /// Recipe 5 — Hardened. Read-only tools, denied paths even when otherwise allowed, a
    /// hard time limit, and UI access fully locked down. Use for the least-trusted code.
    /// </summary>
    public static SandboxPolicy Hardened(string workspace, string[] toolDirs) => new()
    {
        Version = Schema,
        TimeoutMs = 10_000, // kill the run after 10 seconds
        Network = new NetworkPolicy { AllowOutbound = false },
        Filesystem = new FilesystemPolicy
        {
            ReadwritePaths = [workspace],
            ReadonlyPaths = toolDirs,
            DeniedPaths = [@"C:\Windows\System32\config"], // explicitly off-limits
            ClearPolicyOnExit = true,                      // tear down ACLs when done
        },
        Ui = new UiPolicy
        {
            AllowWindows = false,
            Clipboard = ClipboardPolicy.None,   // None | Read | Write | All
            AllowInputInjection = false,
        },
    };
}
