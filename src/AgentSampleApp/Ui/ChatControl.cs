using AgentSampleApp.Agent;
using AgentSampleApp.Mxc;
using DevExpress.XtraEditors;
using Microsoft.Extensions.AI;

namespace AgentSampleApp.Ui;

/// <summary>
/// The free-form chat. Identical behavior to the original single-screen app: a transcript,
/// an input box (Ctrl+Enter sends), and the Restricted/Permissive toggle that drives the
/// run_code tool's policy. Extracted into a UserControl so <see cref="MainForm"/> can host
/// it alongside the sandbox examples.
/// </summary>
public partial class ChatControl : XtraUserControl
{
    private readonly IAgentService? _agent;

    public ChatControl(SandboxAppContext context)
    {
        InitializeComponent();

        // The run_code tool reads the live toggle and reports activity to the transcript.
        AITool tool = CodeExecutionTool.Create(
            context.Runner,
            optionsProvider: () => SandboxRunOptions.ForToggle(toggleStrict.IsOn),
            workspace: context.Workspace,
            onActivity: AppendActivity);

        if (context.Chat is not null)
        {
            _agent = new AgentService(context.Chat, [tool], context.SystemPrompt);
            AppendActivity("System", context.Runner.IsSandboxAvailable
                ? "Ready. Type a message and press Ctrl+Enter."
                : "MXC executor not found — code execution is disabled. Set MXC_BIN_DIR (see README).");
        }
        else
        {
            memoInput.Enabled = false;
            btnSend.Enabled = false;
            AppendActivity("System",
                $"LLM not configured: {context.ChatUnavailableReason}\r\n" +
                "Chat needs an API key; the sandbox examples in the left panel work without one.");
        }

        btnSend.Click += async (_, _) => await SendAsync();
        memoInput.KeyDown += OnInputKeyDown;
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
        if (_agent is null) return;

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
    private void AppendActivity(string channel, string text) =>
        SandboxResultView.Append(memoTranscript, channel, text);
}
