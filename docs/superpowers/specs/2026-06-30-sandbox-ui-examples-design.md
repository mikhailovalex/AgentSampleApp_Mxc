# Sandbox capability UI examples — design

Date: 2026-06-30

## Goal

The app currently exposes a single chat screen with one `Restricted / Permissive`
toggle that only flips `Network.AllowOutbound`. The MXC `SandboxPolicy` is far richer
(filesystem read/write/denied paths, network allow-lists, UI policy, execution
timeout), and none of it is visible in the UI. Add an interactive gallery of UI
examples — not console samples — that each drive one sandbox capability through
dedicated controls (toggles, fields, sliders) and show how the policy changes real
behavior.

Each example must be runnable two ways:

- **Direct** — a `Run` button that calls `ISandboxRunner` directly. No LLM, no API key,
  deterministic.
- **Agent** — a natural-language prompt sent to an `AgentService` whose `run_code` tool
  is bound to that example's live policy controls.

## Shell layout

`MainForm` becomes a shell, not the chat:

- `AccordionControl` docked left (navigation).
- A content `PanelControl` filling the rest hosts one `UserControl` at a time.
- A thin status strip on top, always visible: `MXC backend: …` and `Workspace: …`.

Accordion items:

- **General** → `Chat` (the existing chat, extracted into `ChatControl`).
- **Sandbox capabilities** → `Files`, `Network`, `Timeout`, `Hardened`.

Extracting the chat into `ChatControl` is an intentional refactor: `MainForm`'s
responsibility shifts from "the chat" to "the shell".

## Sandbox seam extension

Today `ISandboxRunner.RunAsync(command, bool restricted, workspace, ct)` can only
express network on/off. To genuinely demonstrate denied paths, allow-lists, timeout and
UI policy, introduce an **application-level** options type (not an SDK type, so the
invariant "only `MxcSandboxRunner` references the MXC SDK" still holds):

```csharp
// Mxc/SandboxRunOptions.cs
public sealed record SandboxRunOptions
{
    public bool AllowOutbound { get; init; }
    public IReadOnlyList<string> ReadwritePaths { get; init; } = []; // extra, beyond workspace
    public IReadOnlyList<string> ReadonlyPaths  { get; init; } = [];
    public IReadOnlyList<string> DeniedPaths    { get; init; } = [];
    public IReadOnlyList<string> AllowedHosts   { get; init; } = [];
    public int? TimeoutMs { get; init; }
    public bool AllowWindows { get; init; } = true;
    public ClipboardAccess Clipboard { get; init; } = ClipboardAccess.All;
    public bool AllowInputInjection { get; init; } = true;

    public static SandboxRunOptions ForToggle(bool restricted) =>
        new() { AllowOutbound = !restricted };
}

public enum ClipboardAccess { None, Read, Write, All }
```

Changes:

- `ISandboxRunner` gains `RunAsync(command, SandboxRunOptions, workspace, ct)`.
- The existing `RunAsync(command, bool restricted, workspace, ct)` stays and delegates
  via `SandboxRunOptions.ForToggle(restricted)`, so `ExampleTools.cs` and other callers
  compile unchanged.
- `MxcSandboxRunner` maps `SandboxRunOptions` → `SandboxPolicy`:
  - `Filesystem.ReadwritePaths = [workspace, ...options.ReadwritePaths]`
  - `Filesystem.ReadonlyPaths  = [..._toolPaths, ...options.ReadonlyPaths]`
  - `Filesystem.DeniedPaths    = options.DeniedPaths`
  - `Network.AllowOutbound = options.AllowOutbound`; `AllowedHosts` when non-empty
  - `TimeoutMs`, and a `UiPolicy` from `AllowWindows` / `Clipboard` / `AllowInputInjection`
  - PATH prefix continues to use `_toolPaths` only.

## Dual mode (direct + agent)

- **Direct**: `await runner.RunAsync(command, BuildOptions(), workspace)` → render
  exit/stdout/stderr.
- **Agent**: each example lazily builds its own
  `AgentService(chatClient, [run_code], systemPrompt)`. `CodeExecutionTool.Create`
  changes its `Func<bool> isRestricted` parameter to `Func<SandboxRunOptions>` so the
  tool reads the example's live controls. The shared `IChatClient` is fine — history
  lives in each `AgentService`. `ChatControl` passes
  `() => SandboxRunOptions.ForToggle(toggleStrict.IsOn)`.

This makes the teaching point explicit: access is decided by the policy, not the model.
Set Files access to `None` and even the agent gets "access denied"; switch to
`Read-only` and it succeeds.

## The four examples

Each panel: policy controls → `Run (direct)` + result view (exit/stdout/stderr) →
`Ask agent` (prompt + mini-transcript).

| Example | Controls | Direct command | Demonstrates |
| --- | --- | --- | --- |
| **Files** | file path (`ButtonEdit` + browse), `RadioGroup`: None / Read-only / Read-write / Denied; `Read` and `Write` buttons | `type "<path>"` / `echo <ts> >> "<path>"` | `FilesystemPolicy`: same file reads/writes or "Access denied" by grant |
| **Network** | `ToggleSwitch` Outbound; Allowed-hosts field; URL field | `python -c "urllib.request.urlopen('<url>')…"` | `NetworkPolicy`: request passes/blocked; allow-list scopes hosts |
| **Timeout** | `SpinEdit`/`TrackBar` TimeoutMs; `SpinEdit` sleep seconds | `python -c "time.sleep(n); print('completed')"` | `TimeoutMs`: long command killed, short one finishes |
| **Hardened** | `ToggleSwitch` Block system path; `AllowWindows`; Clipboard combo; `AllowInputInjection`; Timeout | `type C:\Windows\System32\config\SAM` then `echo 2+2` | Composed hardened profile: denied path blocked, legit work still runs |

Notes:

- **Files** creates a demo file outside the workspace on load
  (`%TEMP%\AgentSampleApp\outside\secret.txt`) via host code, so `None` is blocked by
  default and grants open access.
- **Hardened**: `AllowWindows` / `Clipboard` / `AllowInputInjection` affect GUI
  subprocesses, so a console command mainly shows the denied-path and timeout effects.
  The toggles still set real policy fields; the value is demonstrating how to *compose*
  the profile. This is stated in the panel.

## Robustness

- **No API key**: `Program.cs` currently shows a `MessageBox` and exits when
  `ChatClientFactory.Create` throws. Change it to catch the error, launch with
  `IChatClient? = null`, and have the `Ask agent` sections show "LLM not configured:
  <reason>" while `Run (direct)` stays fully functional. This is the "without LLM" mode.
- **Sandbox unavailable** (no `MXC_BIN_DIR`): direct results show the clear "sandbox
  unavailable" message (as `CodeExecutionTool` already does); a banner is shown in the
  status strip.

## Files

New: `Mxc/SandboxRunOptions.cs`; `Ui/ChatControl.cs` (+ Designer);
`Ui/Examples/FileAccessControl`, `NetworkControl`, `TimeoutControl`, `HardenedControl`
(each + Designer); `Ui/SandboxResultView.cs` (shared result rendering helper).

Changed: `Mxc/ISandboxRunner.cs`, `Mxc/MxcSandboxRunner.cs`, `Agent/CodeExecutionTool.cs`,
`MainForm.cs` / `MainForm.Designer.cs`, `Program.cs`, a short `README.md` section.

Designer files are hand-authored in the style of the existing `MainForm.Designer.cs`.

## Out of scope

- No streaming, no new providers, no automated UI tests.
- Preserve the existing uncommitted edits in `AgentConfig.cs` and `appsettings.json`.
- All code/comments/README content in English.
