# Agent Sample — WinForms + DevExpress + MXC

A minimal .NET WinForms (DevExpress) desktop app with a **built-in agent**. The agent
chats with an LLM and has one tool, `run_code`, that executes shell commands inside a
**Microsoft eXecution Container (MXC)** sandbox. A toggle in the UI flips the sandbox
policy between *Restricted* (no outbound network, filesystem confined to the app
workspace) and *Permissive* — the same before/after demo MXC's own SDK ships with,
wired into a real app.

> This is a proof of concept to validate the "agent inside the app, sandboxed by MXC"
> approach. It is not production-ready, and it depends on experimental, pre-release
> components (MXC itself, and the community .NET port).

## Architecture

```
MainForm (DevExpress XtraForm)
   │  user text ───────────────► AgentService
   │                                 │ Microsoft.Extensions.AI IChatClient
   │                                 │  + UseFunctionInvocation()
   │  ◄─ assistant text / activity   │
   │                                 ▼
   │                          run_code (AIFunction)
   │                                 │
   │                                 ▼
   └───────────────────────► ISandboxRunner ──► MxcSandboxRunner
                                                   │ Sabbour.Mxc.Sdk
                                                   ▼
                                          native MXC executor (wxc-exec / lxc-exec)
```

The only file that touches the MXC SDK is `Mxc/MxcSandboxRunner.cs`. When the official
Microsoft .NET SDK lands (tracking issue: `microsoft/mxc#484`), you replace that one
class and leave the agent untouched.

## Prerequisites

- **Windows 11 24H2+** (build 26100+) for the default `processcontainer` backend.
- **.NET 10 SDK** (the MXC .NET port targets .NET 10).
- **DevExpress WinForms** license + access to your DevExpress NuGet feed. Set the
  `DevExpress.Win` version in `AgentSampleApp.csproj` to your installed build.
- **An LLM key.** Default provider is OpenAI; set `OPENAI_API_KEY`. For Azure OpenAI,
  set `Agent:Provider` to `AzureOpenAI` and provide the key and endpoint via the standard
  `AZURE_OPENAI_API_KEY` and `AZURE_OPENAI_ENDPOINT` environment variables. You can still
  override these from `appsettings.json` (`Agent:Endpoint`, `Agent:ApiKeyEnvVar`).
- **The MXC native executor** + `MXC_BIN_DIR` (see Host setup). Without it, the app
  still launches but `run_code` refuses to execute and says so.

## Host setup (MXC executor)

The .NET SDK builds policy and shells out to the native executor; it does not contain it.

1. Download `mxc-release-binaries.zip` from the
   [microsoft/mxc releases](https://github.com/microsoft/mxc/releases) (latest verified
   against `v0.6.1`).
2. Unzip it, then point `MXC_BIN_DIR` at the folder that contains the architecture
   subfolder (`x64` or `arm64`):

   ```powershell
   $env:MXC_BIN_DIR = "C:\mxc-bin"   # so C:\mxc-bin\x64\wxc-exec.exe exists
   ```

3. Confirm which backend the host will select (read-only, no admin):

   ```powershell
   & "$env:MXC_BIN_DIR\x64\wxc-exec.exe" --probe
   ```

The default `processcontainer` backend runs without admin on 24H2+. Its highest tier
(`base-container`) sits behind a Windows Feature Store gate; if the probe reports
`E_NOTIMPL`, either pin policy schema `0.4.0-alpha` (AppContainer fallback) or enable the
velocity feature keys with ViVeTool and reboot — see the MXC README. The state-aware
session lifecycle (`isolation_session`) needs a Windows Insider build and is not used by
this sample (it runs one-shot commands).

## Build & run

```powershell
dotnet restore
dotnet build -c Release
dotnet run --project src/AgentSampleApp -c Release
```

Then: type a request (e.g. *"compute the sha256 of the string hello and write it to
out.txt"*), press **Ctrl+Enter**. Watch the transcript: `[Tool]` shows the command the
agent chose, `[Sandbox]` shows the result. Flip the toggle to *Permissive* and ask it to
`curl https://api.github.com/zen` to see the network policy change behavior.

## Configuration (`appsettings.json`)

| Key                  | Meaning                                              |
| -------------------- | ---------------------------------------------------- |
| `Agent:Provider`     | `OpenAI` or `AzureOpenAI`                             |
| `Agent:Model`        | OpenAI model id, or Azure deployment name            |
| `Agent:Endpoint`     | Azure OpenAI endpoint; if empty, `AZURE_OPENAI_ENDPOINT` is used |
| `Agent:ApiKeyEnvVar` | Env var name holding the API key; for Azure, falls back to `AZURE_OPENAI_API_KEY` |
| `Agent:SystemPrompt` | System prompt for the agent                          |
| `Agent:ToolPaths`    | Host dirs mounted read-only + added to PATH (e.g. a Python install) so interpreters work inside the sandbox |

For Azure OpenAI the simplest setup is to leave `Agent:Endpoint`/`Agent:ApiKeyEnvVar`
at their defaults and just set the standard environment variables:

```powershell
$env:AZURE_OPENAI_API_KEY  = "<your-key>"
$env:AZURE_OPENAI_ENDPOINT = "https://<your-resource>.openai.azure.com/"
```

## Known caveats / where this is rough

- **Pre-release everything.** MXC is an early preview; the Sabbour port is experimental
  (`0.1.x`) and explicitly *not for production*. Treat no MXC profile as a hard security
  boundary yet — the upstream README says so too.
- **Package versions in the `.csproj` are indicative.** Restore against your feeds and
  bump as needed. In particular, the `Microsoft.Extensions.AI.OpenAI` adapter method has
  been `AsChatClient()` vs `AsIChatClient()` across versions — see the note in
  `ChatClientFactory.cs`.
- **No streaming.** Turns are buffered (`GetResponseAsync`). Streaming + a richer
  transcript (e.g. `RichEditControl`) is an obvious next step.
- **Couldn't compile in the authoring environment.** This was written on Linux without a
  DevExpress license, so it has not been built. Expect minor designer/version fixes when
  you open it in Visual Studio on Windows.

## Swapping to the official .NET SDK later

When `microsoft/mxc#484` ships an official NuGet package, re-implement
`MxcSandboxRunner` against it (the proposed API mirrors the same policy → spawn shape:
declare a policy, one-shot or state-aware spawn, stdio pass-through). Nothing else in the
project should need to change.
