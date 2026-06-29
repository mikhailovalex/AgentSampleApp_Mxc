using AgentSampleApp.Mxc;

namespace AgentSampleApp.Examples;

/// <summary>
/// The smallest possible end-to-end use of the sandbox — no LLM, no UI. Useful for
/// understanding the lowest layer and for a quick smoke test that MXC is wired up.
///
/// Call <see cref="RunAsync"/> from a console <c>Main</c> (with MXC_BIN_DIR set) to run a
/// few commands and print their results.
/// </summary>
public static class SandboxQuickstart
{
    public static async Task RunAsync()
    {
        // 1. A read/write working directory the sandbox is allowed to use.
        var workspace = Path.Combine(Path.GetTempPath(), "AgentSampleApp", "quickstart");
        Directory.CreateDirectory(workspace);

        // 2. (Optional) interpreter folders to expose read-only, so `python` resolves.
        //    Discover yours with:  Split-Path (Get-Command python).Source
        string[] toolPaths =
        [
            // @"C:\Users\<you>\AppData\Local\Programs\Python\Python314",
        ];

        // 3. Create the runner. It probes the host once; check availability before using it.
        var runner = new MxcSandboxRunner(toolPaths);
        if (!runner.IsSandboxAvailable)
        {
            Console.WriteLine("MXC executor not found. Set MXC_BIN_DIR (see README Part 2).");
            return;
        }
        Console.WriteLine($"Sandbox ready: {runner.BackendSummary}\n");

        // 4. Run commands. `restricted: true` => no outbound network, workspace-only writes.
        await Show(runner, workspace, "echo hello from the sandbox");
        await Show(runner, workspace, "echo first> note.txt & type note.txt");

        // Uncomment once a Python folder is listed in toolPaths above:
        // await Show(runner, workspace, "python -c \"print(sum(range(101)))\"");
    }

    private static async Task Show(ISandboxRunner runner, string workspace, string command)
    {
        var result = await runner.RunAsync(command, restricted: true, workspaceDir: workspace);
        Console.WriteLine($"$ {command}");
        Console.WriteLine($"  exit={result.ExitCode}");
        if (!string.IsNullOrWhiteSpace(result.Stdout)) Console.WriteLine($"  stdout: {result.Stdout}");
        if (!string.IsNullOrWhiteSpace(result.Stderr)) Console.WriteLine($"  stderr: {result.Stderr}");
        Console.WriteLine();
    }
}
