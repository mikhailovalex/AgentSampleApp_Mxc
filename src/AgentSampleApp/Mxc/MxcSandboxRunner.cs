using System.Text.RegularExpressions;
using Sabbour.Mxc.Sdk;
using Sabbour.Mxc.Sdk.Errors;

namespace AgentSampleApp.Mxc;

/// <summary>
/// Runs commands inside a Microsoft eXecution Container via the experimental
/// community .NET port (Sabbour.Mxc.Sdk). The SDK builds an MXC policy/config
/// and shells out to the native executor (wxc-exec.exe on Windows,
/// lxc-exec on Linux). The executor is located via the MXC_BIN_DIR environment
/// variable; see README "Host setup".
/// </summary>
public sealed class MxcSandboxRunner : ISandboxRunner
{
    private readonly PlatformSupport _support;
    private readonly IReadOnlyList<string> _toolPaths;

    /// <param name="toolPaths">Host directories to mount read-only and add to PATH
    /// (e.g. a Python install). See <see cref="Configuration.AgentConfig.ToolPaths"/>.</param>
    public MxcSandboxRunner(IReadOnlyList<string>? toolPaths = null)
    {
        _toolPaths = toolPaths ?? [];

        // Probe once at startup. This does not require admin.
        _support = MxcSdk.GetPlatformSupport();
    }

    public bool IsSandboxAvailable => _support.IsSupported;

    public string BackendSummary =>
        _support.IsSupported
            ? $"{string.Join(", ", _support.AvailableMethods)} (tier: {_support.IsolationTier})"
            : "executor not found — set MXC_BIN_DIR";

    /// <summary>
    /// Restricted/Permissive shortcut. Delegates to the options-based overload so there is
    /// a single code path that talks to the SDK.
    /// </summary>
    public Task<SandboxRunResult> RunAsync(
        string command,
        bool restricted,
        string workspaceDir,
        CancellationToken cancellationToken = default)
        => RunAsync(command, SandboxRunOptions.ForToggle(restricted), workspaceDir, cancellationToken);

    public async Task<SandboxRunResult> RunAsync(
        string command,
        SandboxRunOptions options,
        string workspaceDir,
        CancellationToken cancellationToken = default)
    {
        // A SandboxPolicy expresses *intent* ("no outbound network", "only this
        // folder is writable"). The SDK translates it to a backend config and the
        // executor enforces it at the kernel boundary.
        var policy = BuildPolicy(options, workspaceDir);

        // The native executor launches the command directly via CreateProcessW — there is
        // no shell in between. Bare builtins (echo, type), pipes, and redirects therefore
        // fail with "CreateProcessW failed". Wrap in cmd.exe so the model's shell one-liners
        // behave as run_code advertises. /s /c keeps inner quoting intact: cmd strips only
        // the first and last quote and runs everything between them verbatim.
        //
        // Prepend the tool directories to PATH so their executables resolve by name
        // (the model can call `python` instead of a full path).
        var pathPrefix = _toolPaths.Count > 0
            ? $"set \"PATH={string.Join(";", _toolPaths)};%PATH%\" & "
            : string.Empty;
        var shellCommand = $"cmd.exe /s /c \"{pathPrefix}{command}\"";

        // The result's Restricted flag tracks the network dimension, matching the original
        // toggle semantics (restricted == no outbound network).
        var restricted = !options.AllowOutbound;

        try
        {
            // SpawnSandboxAsync == the TypeScript spawnSandboxAsync: buffered one-shot run.
            // workingDirectory must be a path the sandbox can access (the workspace is in
            // ReadwritePaths); without it the AppContainer tier reports "current directory
            // is invalid" for anything that spawns a child process or writes a file.
            var result = await MxcSdk.SpawnSandboxAsync(
                shellCommand,
                policy,
                workingDirectory: workspaceDir,
                cancellationToken: cancellationToken);
            return new SandboxRunResult(
                result.ExitCode,
                CleanOutput(result.Stdout),
                CleanOutput(result.Stderr),
                restricted,
                BackendSummary);
        }
        catch (MxcException ex)
        {
            // Surface structured executor errors (BackendUnavailable, MalformedRequest, ...)
            return new SandboxRunResult(
                ExitCode: -1,
                Stdout: string.Empty,
                Stderr: $"MXC error [{ex.Code}]: {ex.Message}",
                Restricted: restricted,
                Backend: BackendSummary);
        }
    }

    /// <summary>
    /// Translates an application-level <see cref="SandboxRunOptions"/> into the SDK's
    /// <see cref="SandboxPolicy"/>. The workspace is always read/write; the configured tool
    /// paths are always read-only; the per-run path lists layer on top.
    /// </summary>
    private SandboxPolicy BuildPolicy(SandboxRunOptions options, string workspaceDir)
    {
        // Interpreters (Python, Node, ...) live outside the workspace; grant the sandbox
        // read access to their install folders or they fail to load their own DLLs
        // (surfaces as "python314.dll was not found" from inside).
        string[] readonlyPaths = [.. _toolPaths, .. options.ReadonlyPaths];
        string[] readwritePaths = [workspaceDir, .. options.ReadwritePaths];

        // Only attach a UI policy when something is actually locked down. Leaving Ui null
        // preserves the permissive default the Restricted/Permissive toggle has always used.
        UiPolicy? ui = null;
        if (!options.AllowWindows
            || options.Clipboard != ClipboardAccess.All
            || !options.AllowInputInjection)
        {
            ui = new UiPolicy
            {
                AllowWindows = options.AllowWindows,
                Clipboard = MapClipboard(options.Clipboard),
                AllowInputInjection = options.AllowInputInjection,
            };
        }

        return new SandboxPolicy
        {
            // Pin the schema to the executor you ship. v0.6.x executors -> 0.6.0-alpha.
            Version = "0.6.0-alpha",
            TimeoutMs = options.TimeoutMs,
            // Network access is all-or-nothing: the Windows executor rejects per-host
            // allow/block lists, so only AllowOutbound is set here.
            Network = new NetworkPolicy
            {
                AllowOutbound = options.AllowOutbound,
            },
            Filesystem = new FilesystemPolicy
            {
                ReadwritePaths = readwritePaths,
                ReadonlyPaths = readonlyPaths,
                DeniedPaths = options.DeniedPaths.Count > 0 ? [.. options.DeniedPaths] : null,
            },
            Ui = ui,
        };
    }

    private static ClipboardPolicy MapClipboard(ClipboardAccess access) => access switch
    {
        ClipboardAccess.None => ClipboardPolicy.None,
        ClipboardAccess.Read => ClipboardPolicy.Read,
        ClipboardAccess.Write => ClipboardPolicy.Write,
        _ => ClipboardPolicy.All,
    };

    // ANSI/OSC control sequences emitted by the executor's pseudo-terminal
    // (cursor moves, screen clears, the window-title escape naming wxc-exec.exe).
    private static readonly Regex TerminalSequences =
        new(@"\x1B\][^\x07\x1B]*(?:\x07|\x1B\\)|\x1B[@-_][0-?]*[ -/]*[@-~]", RegexOptions.Compiled);

    // Harmless launcher warning CPython prints when it can't resolve its own real path
    // under the AppContainer tier. It precedes every Python run; drop it as pure noise.
    private static readonly Regex CosmeticNoise =
        new(@"^Failed to find real location of .*\r?\n?", RegexOptions.Compiled | RegexOptions.Multiline);

    /// <summary>
    /// The executor runs the command under a pseudo-terminal, so stdout/stderr arrive
    /// wrapped in terminal control sequences and a couple of cosmetic warnings. Strip
    /// them so the transcript shows clean text.
    /// </summary>
    private static string CleanOutput(string? s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        s = TerminalSequences.Replace(s, string.Empty);
        s = CosmeticNoise.Replace(s, string.Empty);
        return s.Trim();
    }
}
