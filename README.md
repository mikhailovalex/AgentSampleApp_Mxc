# Agent Sample — WinForms + DevExpress + MXC sandbox

A minimal **.NET 10 WinForms (DevExpress)** desktop app with a **built-in LLM agent**. The
agent chats with an OpenAI / Azure OpenAI model and has one tool — `run_code` — that
executes shell commands and scripts inside a **Microsoft eXecution Container (MXC)**
sandbox. A toggle in the UI flips the sandbox between **Restricted** (no outbound network,
filesystem confined to a workspace) and **Permissive**, so you can watch the policy change
real behavior.

This README is a hands-on guide: by the end you'll have MXC installed, the agent talking to
your model, **Python running inside the sandbox**, and four worked examples.

> ⚠️ **Proof of concept.** It depends on pre-release components: MXC itself and the
> community .NET port (`Sabbour.Mxc.Sdk` `0.1.x`, *not for production*). Treat no MXC
> profile as a hard security boundary yet.

---

## Contents

1. [Architecture](#architecture)
2. [Quick start](#quick-start)
3. [Part 1 — Prerequisites](#part-1--prerequisites)
4. [Part 2 — Install & configure MXC](#part-2--install--configure-mxc)
5. [Part 3 — Configure the LLM (OpenAI / Azure)](#part-3--configure-the-llm-openai--azure)
6. [Part 4 — Run interpreters in the sandbox (Python)](#part-4--run-interpreters-in-the-sandbox-python)
7. [Part 5 — How the agent & tools work](#part-5--how-the-agent--tools-work)
8. [Part 6 — Sandbox policy reference](#part-6--sandbox-policy-reference)
9. [Part 7 — Worked examples](#part-7--worked-examples)
10. [Configuration reference](#configuration-reference)
11. [Troubleshooting](#troubleshooting)
12. [Project structure](#project-structure)
13. [Swapping to the official .NET SDK](#swapping-to-the-official-net-sdk)

---

## Architecture

```
MainForm (DevExpress XtraForm)
   │  user text ───────────────► AgentService
   │                                 │ Microsoft.Extensions.AI IChatClient
   │                                 │  + UseFunctionInvocation()  ← auto-runs tools
   │  ◄─ assistant text / activity   │
   │                                 ▼
   │                          run_code (AIFunction)
   │                                 │
   │                                 ▼
   └───────────────────────► ISandboxRunner ──► MxcSandboxRunner
                                                   │ Sabbour.Mxc.Sdk
                                                   ▼
                                          native MXC executor (wxc-exec.exe)
```

The only file that touches the MXC SDK is [`Mxc/MxcSandboxRunner.cs`](src/AgentSampleApp/Mxc/MxcSandboxRunner.cs).
Everything else talks to the `ISandboxRunner` interface — when the official Microsoft .NET
SDK lands (`microsoft/mxc#484`), you replace that one class.

---

## Quick start

For the impatient (each step is explained in detail below):

```powershell
# 1. MXC executor
#    Download mxc-release-binaries.zip from https://github.com/microsoft/mxc/releases
#    Unzip to C:\mxc-release-binaries  (so C:\mxc-release-binaries\x64\wxc-exec.exe exists)
[Environment]::SetEnvironmentVariable("MXC_BIN_DIR", "C:\mxc-release-binaries", "User")
# → then RESTART your terminal / Visual Studio so the variable is inherited (see Part 2)

# 2. LLM key (Azure OpenAI shown; OpenAI is just OPENAI_API_KEY)
[Environment]::SetEnvironmentVariable("AZURE_OPENAI_API_KEY",  "<your-key>",  "User")
[Environment]::SetEnvironmentVariable("AZURE_OPENAI_ENDPOINT", "https://<resource>.openai.azure.com/", "User")

# 3. Point Agent:ToolPaths in appsettings.json at your Python folder (for the Python demos)

# 4. Build & run
dotnet restore
dotnet run --project src/AgentSampleApp -c Debug
```

---

## Part 1 — Prerequisites

| Requirement | Notes |
| --- | --- |
| **Windows 11 24H2+** (build 26100+) | Needed for the `processcontainer` / AppContainer backends. |
| **.NET 10 SDK** | The MXC .NET port targets `net10.0-windows`. |
| **DevExpress WinForms** | A licensed feed or 30-day trial. Set the `DevExpress.Win` version in [`AgentSampleApp.csproj`](src/AgentSampleApp/AgentSampleApp.csproj) to your installed build. Without a license you get evaluation warnings but it still builds/runs. |
| **An LLM key** | OpenAI (`OPENAI_API_KEY`) or Azure OpenAI (`AZURE_OPENAI_API_KEY` + `AZURE_OPENAI_ENDPOINT`). See [Part 3](#part-3--configure-the-llm-openai--azure). |
| **MXC native executor** | Downloaded separately; see [Part 2](#part-2--install--configure-mxc). |

---

## Part 2 — Install & configure MXC

### 2.1 The mental model (read this first)

The MXC .NET SDK **does not contain the sandbox** — it builds a *policy* (what's allowed)
and shells out to a native executor (`wxc-exec.exe`) that enforces it.

On the default tier (`appcontainer-dacl`), MXC isolates a process **on the host
filesystem** using a Windows AppContainer + DACLs. **There is no separate container image
or filesystem** — so you don't "install software into MXC". Instead, software lives on the
host and you grant the sandbox access to the folders it needs (see [Part 4](#part-4--run-interpreters-in-the-sandbox-python)).

### 2.2 Download the executor

1. Download `mxc-release-binaries.zip` from the
   [microsoft/mxc releases](https://github.com/microsoft/mxc/releases) (verified against
   `v0.6.1`, which matches the `0.6.0-alpha` policy schema this app pins).
2. Unzip it. You should get an architecture layout:

   ```
   C:\mxc-release-binaries\
   ├── x64\   wxc-exec.exe, wxc-host-prep.exe, ...
   └── arm64\ wxc-exec.exe, ...
   ```

### 2.3 Point `MXC_BIN_DIR` at the **root** (not the `x64` subfolder)

```powershell
[Environment]::SetEnvironmentVariable("MXC_BIN_DIR", "C:\mxc-release-binaries", "User")
```

> 🔴 **The #1 gotcha.** Setting a User environment variable only affects processes started
> *afterwards*. A terminal or Visual Studio that was already open will **not** see it, and
> the app crashes with `wxc-exec.exe not found`. **Fully restart** your terminal / Visual
> Studio (or reboot) after setting it. Verify a fresh process sees it:
>
> ```powershell
> [Environment]::GetEnvironmentVariable("MXC_BIN_DIR","Process")   # → C:\mxc-release-binaries
> ```

### 2.4 Probe the host (no admin required)

```powershell
& "$env:MXC_BIN_DIR\x64\wxc-exec.exe" --probe
```

This prints the **isolation tier** the host will use:

| Tier | Meaning |
| --- | --- |
| `base-container` / `processcontainer` | Highest; needs the Windows Feature Store gate (or ViVeTool keys). |
| `appcontainer-dacl` | Common fallback. Works without admin. **What this guide assumes.** |
| `windows-sandbox` | VM-style isolation with its own filesystem (separate setup). |

### 2.5 (Optional) Grant system-drive metadata — needed for some interpreters

On the `appcontainer-dacl` tier, AppContainer processes can't read metadata of the
system-drive root, so binaries like **`powershell.exe`** fail to initialize their DLLs
(`STATUS_DLL_INIT_FAILED`). To fix, run **as administrator**:

```powershell
& "C:\mxc-release-binaries\x64\wxc-host-prep.exe" prepare-system-drive
```

> **Python does not need this step** — once you grant it via `ToolPaths` (Part 4) it works
> as-is. Run host-prep only if you want PowerShell / other system tools inside the sandbox.

---

## Part 3 — Configure the LLM (OpenAI / Azure)

The model is configured in [`appsettings.json`](src/AgentSampleApp/appsettings.json) under
`Agent`. **API keys are never stored in the file** — they're read from environment
variables at runtime.

### OpenAI

```jsonc
{
  "Agent": {
    "Provider": "OpenAI",
    "Model": "gpt-4.1-mini",          // OpenAI model id
    "ApiKeyEnvVar": "OPENAI_API_KEY"  // env var that holds the key
  }
}
```
```powershell
$env:OPENAI_API_KEY = "<your-key>"
```

### Azure OpenAI

```jsonc
{
  "Agent": {
    "Provider": "AzureOpenAI",
    "Model": "gpt-4.1-mini",              // ← Azure DEPLOYMENT name, not the model id
    "Endpoint": "",                       // empty → read from AZURE_OPENAI_ENDPOINT
    "ApiKeyEnvVar": "AZURE_OPENAI_API_KEY"
  }
}
```
```powershell
$env:AZURE_OPENAI_API_KEY  = "<your-key>"
$env:AZURE_OPENAI_ENDPOINT = "https://<resource>.openai.azure.com/"
```

**Resolution rules** (implemented in [`ChatClientFactory.cs`](src/AgentSampleApp/Agent/ChatClientFactory.cs)):

- **Key** — the env var named by `ApiKeyEnvVar` is tried first; for Azure it falls back to
  the standard `AZURE_OPENAI_API_KEY`.
- **Endpoint** — `Agent:Endpoint` wins if set; otherwise the standard `AZURE_OPENAI_ENDPOINT`
  is used.

---

## Part 4 — Run interpreters in the sandbox (Python)

Because the sandbox runs on the host filesystem with DACL-gated access, an interpreter
installed outside the workspace is **invisible** until you grant its folder. A bare
`python` call otherwise fails with `python314.dll was not found` (the DLL exists — the
sandbox just can't read its folder).

Add the interpreter's install directory to **`Agent:ToolPaths`**. Each entry is mounted
**read-only** and **prepended to `PATH`**, so the model can call `python` by name:

```jsonc
{
  "Agent": {
    "ToolPaths": [
      "C:\\Users\\<you>\\AppData\\Local\\Programs\\Python\\Python314"
    ]
  }
}
```

Find your Python folder with:

```powershell
Split-Path (Get-Command python).Source     # → the directory to put in ToolPaths
```

The same mechanism works for Node.js, Git, or any portable toolchain — just add its `bin`
folder. `ToolPaths` are read-only, so they work in **Restricted** mode too; your scripts
are written to and run from the read-write workspace.

---

## Part 5 — How the agent & tools work

### Turn flow

1. You type a message → [`MainForm.SendAsync`](src/AgentSampleApp/MainForm.cs) →
   [`AgentService.SendAsync`](src/AgentSampleApp/Agent/AgentService.cs).
2. `AgentService` calls the model with the conversation history **and the list of tools**.
3. `UseFunctionInvocation()` middleware (wired in
   [`ChatClientFactory`](src/AgentSampleApp/Agent/ChatClientFactory.cs)) sees the model
   request a tool, runs the C# method, feeds the result back — looping until the model
   produces a final text answer.
4. The final text is returned and appended to the transcript.

### The `run_code` tool

Defined in [`CodeExecutionTool.cs`](src/AgentSampleApp/Agent/CodeExecutionTool.cs). A tool
is just a C# delegate wrapped by `AIFunctionFactory.Create` with a `name` and `description`
the model reads:

```csharp
return AIFunctionFactory.Create(
    async (string command, CancellationToken ct) =>
    {
        var restricted = isRestricted();                  // reads the live UI toggle
        var r = await runner.RunAsync(command, restricted, workspace, ct);
        return $"exit_code: {r.ExitCode}\nstdout:\n{r.Stdout}\nstderr:\n{r.Stderr}";
    },
    name: "run_code",
    description: "Execute a single shell command inside an MXC sandbox and return its "
               + "exit code, stdout, and stderr. Use for any computation, file work, or code.");
```

The `description` is the model's only documentation for the tool — write it carefully.

### What `MxcSandboxRunner` does for you

[`MxcSandboxRunner.RunAsync`](src/AgentSampleApp/Mxc/MxcSandboxRunner.cs) translates the
toggle into a policy and runs the command. It handles three sharp edges so the model's
commands "just work":

- **Wraps the command in `cmd.exe /s /c "…"`** — the native executor launches programs
  directly via `CreateProcessW` (no shell), so bare builtins (`echo`, `dir`, `type`), pipes
  (`|`), and redirects (`>`) would otherwise fail. `/s /c` preserves inner quoting.
- **Sets the working directory to the workspace** — without a valid, accessible CWD the
  AppContainer reports `current directory is invalid` for anything that writes a file or
  spawns a child process.
- **Cleans the output** — the executor uses a pseudo-terminal, so it strips ANSI/OSC escape
  sequences and a cosmetic Python launcher warning.

### Adding your own tool

```csharp
var weatherTool = AIFunctionFactory.Create(
    (string city) => $"It's always sunny in {city}.",
    name: "get_weather",
    description: "Return the current weather for a city.");

// pass it alongside run_code:
_agent = new AgentService(chatClient, new[] { runCodeTool, weatherTool }, config.SystemPrompt);
```

Parameters and return values are serialized for you; the model fills parameters from the
conversation.

### Ready-to-read code examples

The [`Examples/`](src/AgentSampleApp/Examples) folder contains heavily-commented, compiling
samples you can copy from:

- [`ExampleTools.cs`](src/AgentSampleApp/Examples/ExampleTools.cs) — three tool patterns: a
  pure function with an enum parameter (`calculator`), a sandbox-backed file search
  (`search_files`), and a file writer (`write_file`).
- [`ExamplePolicies.cs`](src/AgentSampleApp/Examples/ExamplePolicies.cs) — five
  `SandboxPolicy` recipes: Restricted, Permissive, with read-only tools, host allow-list,
  and a fully hardened profile (timeout + denied paths + UI lockdown).
- [`SandboxQuickstart.cs`](src/AgentSampleApp/Examples/SandboxQuickstart.cs) — the smallest
  end-to-end use of the sandbox, with no LLM or UI.

---

## Part 6 — Sandbox policy reference

A `SandboxPolicy` expresses **intent** ("no outbound network", "only this folder is
writable"); the executor enforces it at the OS boundary. Built in
[`MxcSandboxRunner.RunAsync`](src/AgentSampleApp/Mxc/MxcSandboxRunner.cs).

### `SandboxPolicy`

| Field | Type | Purpose |
| --- | --- | --- |
| `Version` | `string` | Policy schema. Pin to your executor — `v0.6.x` → `"0.6.0-alpha"`. |
| `Filesystem` | `FilesystemPolicy` | What paths are readable / writable. |
| `Network` | `NetworkPolicy` | Outbound / host allow-list. |
| `Ui` | `UiPolicy` | Window, clipboard, input-injection access. |
| `TimeoutMs` | `int?` | Hard wall-clock limit for the run. |

### `FilesystemPolicy`

| Field | Type | Purpose |
| --- | --- | --- |
| `ReadwritePaths` | `string[]` | Folders the sandbox may read **and** write (the workspace). |
| `ReadonlyPaths` | `string[]` | Folders readable only (interpreters via `ToolPaths` land here). |
| `DeniedPaths` | `string[]` | Explicitly blocked paths, even if otherwise allowed. |
| `ClearPolicyOnExit` | `bool?` | Tear down the applied ACLs when the run ends. |

### `NetworkPolicy`

| Field | Type | Purpose |
| --- | --- | --- |
| `AllowOutbound` | `bool?` | Master switch for outbound network. **The Restricted/Permissive toggle sets this.** |
| `AllowLocalNetwork` | `bool?` | Allow loopback / LAN even when outbound is off. |
| `AllowedHosts` | `string[]` | Allow-list specific hosts. |
| `BlockedHosts` | `string[]` | Block specific hosts. |
| `Proxy` | `ProxyConfig` | Route traffic through a proxy. |

### `UiPolicy`

| Field | Type | Purpose |
| --- | --- | --- |
| `AllowWindows` | `bool?` | Whether the sandboxed process may create windows. |
| `Clipboard` | `ClipboardPolicy?` | Clipboard access: `None`, `Read`, `Write`, or `All`. |
| `AllowInputInjection` | `bool?` | Synthetic keyboard/mouse input. |

### Restricted vs Permissive

The UI toggle (`toggleStrict`, default **Restricted**) maps to:

| Toggle | `Network.AllowOutbound` | Filesystem |
| --- | --- | --- |
| **Restricted** (on) | `false` — no outbound network | workspace (rw) + `ToolPaths` (ro) only |
| **Permissive** (off) | `true` — outbound allowed | same paths; network opened |

---

## Part 7 — Worked examples

Run the app, type the prompt, press **Ctrl+Enter**. The agent decides to call `run_code`;
the transcript shows `[Tool]` (the command it chose) and `[Sandbox]` (the result). All
outputs below were captured from the actual sandbox.

### Example 1 — Run code (math in Python)

> **Prompt:** *"What is 10 factorial? Use Python."*

```
[Tool]    run_code  [RESTRICTED]
          python -c "import math; print(math.factorial(10))"
[Sandbox] exit=0
          3628800
```

### Example 2 — Create and list files

> **Prompt:** *"Create three text files f0..f2 in the workspace, then list the .txt files."*

```
[Tool]    run_code  [RESTRICTED]
          python -c "[open(f'f{i}.txt','w').write('x') for i in range(3)]" & dir /b *.txt
[Sandbox] exit=0
          f0.txt
          f1.txt
          f2.txt
```

Files land in the read-write workspace (`%TEMP%\AgentSampleApp\workspace`).

### Example 3 — Search across files

> **Prompt:** *"Write a log file with an ERROR line and find it."*

```
[Tool]    run_code  [RESTRICTED]
          python -c "open('log.txt','w').write('ok\nERROR boom\nfine')" & findstr ERROR log.txt
[Sandbox] exit=0
          ERROR boom
```

Pipes and `findstr` work because commands run through `cmd.exe` with a valid working
directory.

### Example 4 — Hashing / quick utility

> **Prompt:** *"Give me the SHA-256 of the string 'hello'."*

```
[Tool]    run_code  [RESTRICTED]
          python -c "import hashlib; print(hashlib.sha256(b'hello').hexdigest())"
[Sandbox] exit=0
          2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824
```

### Bonus — see the network policy change

Flip the toggle to **Permissive** and ask the agent to fetch a URL
(`curl https://api.github.com/zen`). In **Restricted** the request is blocked by
`Network.AllowOutbound = false`; in **Permissive** it succeeds — the same command, two
policies.

---

## Configuration reference

`appsettings.json` → `Agent` section:

| Key | Meaning |
| --- | --- |
| `Provider` | `OpenAI` or `AzureOpenAI`. |
| `Model` | OpenAI model id, **or** Azure deployment name. |
| `Endpoint` | Azure endpoint; if empty, `AZURE_OPENAI_ENDPOINT` is used. |
| `ApiKeyEnvVar` | Env var holding the key; for Azure, falls back to `AZURE_OPENAI_API_KEY`. |
| `SystemPrompt` | System prompt for the agent. |
| `ToolPaths` | Host dirs mounted read-only + added to `PATH` (e.g. a Python install) so interpreters work inside the sandbox. |

Environment variables:

| Variable | Used for |
| --- | --- |
| `MXC_BIN_DIR` | Root folder containing `x64\wxc-exec.exe`. **Required.** |
| `OPENAI_API_KEY` | OpenAI key (default provider). |
| `AZURE_OPENAI_API_KEY` / `AZURE_OPENAI_ENDPOINT` | Azure OpenAI key + endpoint. |

---

## Troubleshooting

| Symptom | Cause | Fix |
| --- | --- | --- |
| `wxc-exec.exe not found` | The app's process doesn't see `MXC_BIN_DIR` (set after the process started). | Set it (Part 2.3) and **fully restart** the terminal / Visual Studio, or reboot. |
| `CreateProcessW failed: cannot find the file specified` | A bare shell builtin / pipe ran without a shell. | Handled by the `cmd.exe` wrapping in `MxcSandboxRunner`; if you bypass it, wrap commands yourself. |
| `The current directory is invalid` / `Access is denied` | No valid working directory for the sandbox. | Handled via `workingDirectory: workspaceDir`; ensure the workspace exists and is in `ReadwritePaths`. |
| `python314.dll was not found` (from inside the sandbox) | AppContainer can't read the Python folder. | Add the Python install dir to `Agent:ToolPaths` (Part 4). |
| `powershell` fails with `STATUS_DLL_INIT_FAILED` (exit `-1073741502`) | AppContainer can't read system-drive metadata. | Run `wxc-host-prep prepare-system-drive` **as admin** (Part 2.5). |
| Garbled output (`[?25l[2J…`) | Pseudo-terminal escape codes. | Stripped by `CleanOutput`; nothing to do. |
| Build shows `DX1000` / `DX1001` warnings | DevExpress is in evaluation mode. | Install your DevExpress license, or ignore for local use. |
| `run_code` says "Sandbox unavailable" | No executor found at startup. | Same as `wxc-exec.exe not found` above. |

---

## Project structure

| File | Responsibility |
| --- | --- |
| [`Program.cs`](src/AgentSampleApp/Program.cs) | Composition root: load config, build chat client + sandbox runner, launch the form. |
| [`Configuration/AgentConfig.cs`](src/AgentSampleApp/Configuration/AgentConfig.cs) | Strongly-typed `Agent` settings. |
| [`Agent/ChatClientFactory.cs`](src/AgentSampleApp/Agent/ChatClientFactory.cs) | Builds the OpenAI/Azure `IChatClient` + function-invocation pipeline. |
| [`Agent/AgentService.cs`](src/AgentSampleApp/Agent/AgentService.cs) | Holds history, exposes tools, returns final answers. |
| [`Agent/CodeExecutionTool.cs`](src/AgentSampleApp/Agent/CodeExecutionTool.cs) | Defines the `run_code` tool. |
| [`Mxc/ISandboxRunner.cs`](src/AgentSampleApp/Mxc/ISandboxRunner.cs) | Sandbox abstraction (the seam for swapping SDKs). |
| [`Mxc/MxcSandboxRunner.cs`](src/AgentSampleApp/Mxc/MxcSandboxRunner.cs) | **The only file that touches the MXC SDK.** Policy, shell wrapping, output cleanup. |
| [`MainForm.cs`](src/AgentSampleApp/MainForm.cs) | UI: input, transcript, the Restricted/Permissive toggle. |
| [`Examples/ExampleTools.cs`](src/AgentSampleApp/Examples/ExampleTools.cs) | Sample custom tools (calculator, file search, file writer). |
| [`Examples/ExamplePolicies.cs`](src/AgentSampleApp/Examples/ExamplePolicies.cs) | Sample `SandboxPolicy` recipes. |
| [`Examples/SandboxQuickstart.cs`](src/AgentSampleApp/Examples/SandboxQuickstart.cs) | Minimal no-LLM sandbox usage. |

---

## Swapping to the official .NET SDK

When `microsoft/mxc#484` ships an official NuGet package, re-implement only
[`MxcSandboxRunner`](src/AgentSampleApp/Mxc/MxcSandboxRunner.cs) against it (the proposed API
mirrors the same policy → spawn shape). Nothing else in the project should need to change.

### Known rough edges

- **Pre-release everything.** MXC is an early preview; the Sabbour port is `0.1.x` and
  explicitly not for production.
- **Package versions in `.csproj` are indicative** — restore against your feeds and bump as
  needed. The `Microsoft.Extensions.AI.OpenAI` adapter has been `AsChatClient()` vs
  `AsIChatClient()` across versions (see the note in `ChatClientFactory.cs`).
- **No streaming.** Turns are buffered (`GetResponseAsync`). Streaming + a richer transcript
  is an obvious next step.
