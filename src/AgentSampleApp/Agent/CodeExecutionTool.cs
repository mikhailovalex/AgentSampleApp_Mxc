using System.Text;
using AgentSampleApp.Mxc;
using Microsoft.Extensions.AI;

namespace AgentSampleApp.Agent;

/// <summary>
/// Factory for the agent's single tool: <c>run_code</c>. The tool runs a shell
/// command inside the MXC sandbox and returns exit code + stdout/stderr to the model.
/// </summary>
public static class CodeExecutionTool
{
    /// <param name="runner">Sandbox runner.</param>
    /// <param name="isRestricted">Reads the live UI toggle (restricted vs permissive).</param>
    /// <param name="workspace">Working directory exposed to the sandbox.</param>
    /// <param name="onActivity">Sink for surfacing tool/sandbox activity in the UI
    /// (channel, text). Invoked from a background thread — marshal to the UI thread.</param>
    public static AITool Create(
        ISandboxRunner runner,
        Func<bool> isRestricted,
        string workspace,
        Action<string, string> onActivity)
    {
        return AIFunctionFactory.Create(
            async (string command, CancellationToken cancellationToken) =>
            {
                if (!runner.IsSandboxAvailable)
                {
                    const string msg =
                        "Sandbox unavailable: no MXC executor was found on this host, so code " +
                        "execution is disabled. Tell the user to install the MXC executor and set " +
                        "MXC_BIN_DIR. Do not attempt to run code another way.";
                    onActivity("Sandbox", "unavailable — execution refused");
                    return msg;
                }

                var restricted = isRestricted();
                onActivity("Tool",
                    $"run_code  [{(restricted ? "RESTRICTED: no network, workspace only" : "PERMISSIVE: network + broader fs")}]\n{command}");

                var r = await runner.RunAsync(command, restricted, workspace, cancellationToken);

                onActivity("Sandbox",
                    $"exit={r.ExitCode}\n{Preview(r.Stdout)}" +
                    (string.IsNullOrWhiteSpace(r.Stderr) ? "" : $"\n[stderr] {Preview(r.Stderr)}"));

                var sb = new StringBuilder();
                sb.Append("exit_code: ").Append(r.ExitCode).Append('\n');
                if (!string.IsNullOrEmpty(r.Stdout)) sb.Append("stdout:\n").Append(r.Stdout).Append('\n');
                if (!string.IsNullOrEmpty(r.Stderr)) sb.Append("stderr:\n").Append(r.Stderr).Append('\n');
                return sb.ToString();
            },
            name: "run_code",
            description:
                "Execute a single shell command (for example: python -c \"...\", node -e \"...\", " +
                "or a plain shell one-liner) inside a Microsoft eXecution Container (MXC) sandbox and " +
                "return its exit code, stdout, and stderr. Use this for any computation, file work, or " +
                "code the user asks you to run. The sandbox confines filesystem writes to the app " +
                "workspace and, when restricted, blocks outbound network access.");
    }

    private static string Preview(string s, int max = 600) =>
        s.Length <= max ? s : s[..max] + $"\n…(+{s.Length - max} chars)";
}
