using System.Text;
using AgentSampleApp.Mxc;
using DevExpress.XtraEditors;

namespace AgentSampleApp.Ui;

/// <summary>Formatting + thread-safe text helpers shared by the example panels.</summary>
public static class SandboxResultView
{
    /// <summary>Render a sandbox run result as readable text (exit code, network, output).</summary>
    public static string Format(SandboxRunResult r)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"exit={r.ExitCode}    network={(r.Restricted ? "blocked" : "allowed")}");
        if (!string.IsNullOrWhiteSpace(r.Stdout))
        {
            sb.AppendLine("stdout:");
            sb.AppendLine(r.Stdout);
        }
        if (!string.IsNullOrWhiteSpace(r.Stderr))
        {
            sb.AppendLine("stderr:");
            sb.AppendLine(r.Stderr);
        }
        if (string.IsNullOrWhiteSpace(r.Stdout) && string.IsNullOrWhiteSpace(r.Stderr))
        {
            sb.AppendLine("(no output)");
        }
        return sb.ToString().TrimEnd();
    }

    /// <summary>Set the full text of a memo (used for direct, single-shot results).</summary>
    public static void Show(MemoEdit memo, string text)
    {
        if (memo.InvokeRequired)
        {
            memo.BeginInvoke(() => Show(memo, text));
            return;
        }
        memo.Text = text;
        ScrollToEnd(memo);
    }

    /// <summary>Append a channel-tagged line to a memo (used for the agent transcript).
    /// Safe to call from a background thread — tool callbacks fire off the UI thread.</summary>
    public static void Append(MemoEdit memo, string channel, string text)
    {
        if (memo.InvokeRequired)
        {
            memo.BeginInvoke(() => Append(memo, channel, text));
            return;
        }
        memo.Text += $"[{channel}] {text}{Environment.NewLine}{Environment.NewLine}";
        ScrollToEnd(memo);
    }

    private static void ScrollToEnd(MemoEdit memo)
    {
        memo.SelectionStart = memo.Text.Length;
        memo.SelectionLength = 0;
    }
}
