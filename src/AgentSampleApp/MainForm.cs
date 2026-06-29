using AgentSampleApp.Agent;
using AgentSampleApp.Configuration;
using AgentSampleApp.Mxc;
using Microsoft.Extensions.AI;

namespace AgentSampleApp;

public partial class MainForm : DevExpress.XtraEditors.XtraForm
{
    private readonly IAgentService _agent;
    private readonly ISandboxRunner _runner;

    public MainForm(IChatClient chatClient, ISandboxRunner runner, AgentConfig config, string workspace)
    {
        InitializeComponent();

        _runner = runner;

        // The run_code tool reads the live toggle and reports activity to the transcript.
        AITool tool = CodeExecutionTool.Create(
            runner,
            isRestricted: () => toggleStrict.IsOn,
            workspace: workspace,
            onActivity: AppendActivity);

        _agent = new AgentService(chatClient, new[] { tool }, config.SystemPrompt);

        lblBackend.Text = "MXC backend: " + runner.BackendSummary;

        AppendActivity("System", runner.IsSandboxAvailable
            ? $"Ready. Code runs inside an MXC sandbox. Workspace: {workspace}"
            : "MXC executor not found — code execution is disabled. Set MXC_BIN_DIR (see README).");

        btnSend.Click += async (_, _) => await SendAsync();
        memoInput.KeyDown += OnInputKeyDown;
        ActiveControl = memoInput;
    }

    private async void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        // Ctrl+Enter sends; plain Enter inserts a newline.
        if (e.Control && e.KeyCode == Keys.Enter)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            await SendAsync();
        }
    }

    private async Task SendAsync()
    {
        var text = memoInput.Text.Trim();
        if (text.Length == 0) return;

        AppendActivity("You", text);
        memoInput.Text = string.Empty;
        SetBusy(true);
        try
        {
            var reply = await _agent.SendAsync(text);
            AppendActivity("Agent", reply);
        }
        catch (Exception ex)
        {
            AppendActivity("Error", ex.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        btnSend.Enabled = !busy;
        memoInput.Enabled = !busy;
        btnSend.Text = busy ? "Working…" : "Send\r\n(Ctrl+Enter)";
        if (!busy) ActiveControl = memoInput;
    }

    /// <summary>Thread-safe transcript append (tool/sandbox callbacks fire off the UI thread).</summary>
    private void AppendActivity(string channel, string text)
    {
        if (memoTranscript.InvokeRequired)
        {
            memoTranscript.BeginInvoke(() => AppendActivity(channel, text));
            return;
        }

        memoTranscript.Text += $"[{channel}] {text}{Environment.NewLine}{Environment.NewLine}";

        // Scroll to the end.
        memoTranscript.SelectionStart = memoTranscript.Text.Length;
        memoTranscript.SelectionLength = 0;
    }
}
