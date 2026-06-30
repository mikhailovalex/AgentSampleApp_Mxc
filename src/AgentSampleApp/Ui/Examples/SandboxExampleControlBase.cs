using AgentSampleApp.Agent;
using AgentSampleApp.Mxc;
using DevExpress.XtraEditors;
using Microsoft.Extensions.AI;

namespace AgentSampleApp.Ui.Examples;

/// <summary>
/// Common chrome for a sandbox example panel. A subclass supplies a title, a description,
/// the controls that shape the policy, the direct command to run, and a suggested LLM
/// prompt. The base wires the two ways to run the same policy:
///
/// <list type="bullet">
/// <item><b>Direct</b> — calls <see cref="ISandboxRunner"/> with <see cref="BuildOptions"/>.
/// No LLM, no API key.</item>
/// <item><b>Agent</b> — sends the prompt to an <see cref="AgentService"/> whose
/// <c>run_code</c> tool is bound to the same <see cref="BuildOptions"/>, so the model runs
/// under the exact policy the controls describe.</item>
/// </list>
///
/// The layout is built in code (not a .Designer.cs file) because the panels are control-heavy
/// and share this single scaffold; only the policy controls differ per example.
/// </summary>
public abstract class SandboxExampleControlBase : XtraUserControl
{
    private MemoEdit _memoCommand = null!;
    private MemoEdit _memoResult = null!;
    private MemoEdit _memoPrompt = null!;
    private MemoEdit _memoAgent = null!;
    private SimpleButton _btnRunDirect = null!;
    private SimpleButton _btnAskAgent = null!;

    // Steers the model toward commands that actually run in this sandbox. PowerShell fails to
    // initialize under the AppContainer tier (STATUS_DLL_INIT_FAILED), which would mask the
    // policy effect the example is meant to show. Appended to the configured system prompt.
    private const string AgentHint =
        "\n\nEnvironment notes for run_code: prefer cmd.exe built-ins (type, dir, set /a) or " +
        "`python`. Do NOT use PowerShell — it fails to initialize in this sandbox. If a command " +
        "is blocked by the sandbox policy, report that result plainly; do not suggest running it " +
        "outside the sandbox, because demonstrating the policy is the whole point.";

    protected SandboxAppContext Context { get; }

    /// <summary>Container the subclass fills with its policy controls (toggles, fields, …).</summary>
    protected GroupControl PolicyGroup { get; private set; } = null!;

    protected SandboxExampleControlBase(SandboxAppContext context)
    {
        Context = context;
        BuildChrome();
    }

    // --- Subclass contract -------------------------------------------------

    protected abstract string ExampleTitle { get; }
    protected abstract string ExampleDescription { get; }
    protected abstract string DefaultPrompt { get; }

    /// <summary>Build the sandbox policy from the current state of the policy controls.</summary>
    protected abstract SandboxRunOptions BuildOptions();

    /// <summary>The exact shell command the Direct button runs (also shown to the user).</summary>
    protected abstract string BuildDirectCommand();

    /// <summary>Height reserved for <see cref="PolicyGroup"/>; override if a panel needs more.</summary>
    protected virtual int PolicyHeight => 150;

    /// <summary>Subclasses call this after their controls change so the shown command stays live.</summary>
    protected void RefreshCommand() => _memoCommand.Text = BuildDirectCommand();

    // --- Chrome ------------------------------------------------------------

    private void BuildChrome()
    {
        var lblTitle = new LabelControl
        {
            Text = ExampleTitle,
            Dock = DockStyle.Fill,
            Appearance = { Font = new Font("Segoe UI", 12F, FontStyle.Bold) },
        };
        lblTitle.Appearance.Options.UseFont = true;

        var lblDescription = new LabelControl
        {
            Text = ExampleDescription,
            Dock = DockStyle.Fill,
            AutoSizeMode = LabelAutoSizeMode.None,
        };
        lblDescription.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;

        PolicyGroup = new GroupControl { Text = "Sandbox policy", Dock = DockStyle.Fill };

        var cmdBar = new PanelControl { Dock = DockStyle.Fill };
        ((System.ComponentModel.ISupportInitialize)cmdBar).BeginInit();
        cmdBar.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
        _btnRunDirect = new SimpleButton { Text = "Run (direct)", Dock = DockStyle.Right, Width = 140 };
        _btnRunDirect.Click += async (_, _) => await RunDirectAsync();
        var lblCmdCaption = new LabelControl
        {
            Text = "Direct command (no LLM):",
            Dock = DockStyle.Left,
            AutoSizeMode = LabelAutoSizeMode.None,
            Width = 200,
        };
        cmdBar.Controls.Add(lblCmdCaption);
        cmdBar.Controls.Add(_btnRunDirect);
        ((System.ComponentModel.ISupportInitialize)cmdBar).EndInit();

        _memoCommand = ReadonlyMemo();
        _memoCommand.Properties.WordWrap = false;

        var groupResult = new GroupControl { Text = "Direct result", Dock = DockStyle.Fill };
        _memoResult = ReadonlyMemo();
        _memoResult.Dock = DockStyle.Fill;
        groupResult.Controls.Add(_memoResult);

        var groupAgent = BuildAgentGroup();

        // One column; absolute rows for the fixed chrome, two percentage rows split the rest
        // between the direct result and the agent transcript.
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7,
            Padding = new Padding(10),
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));            // title
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));            // description
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, PolicyHeight));  // policy
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));            // command bar
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));            // command text
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 35));            // direct result
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 65));            // agent

        root.Controls.Add(lblTitle, 0, 0);
        root.Controls.Add(lblDescription, 0, 1);
        root.Controls.Add(PolicyGroup, 0, 2);
        root.Controls.Add(cmdBar, 0, 3);
        root.Controls.Add(_memoCommand, 0, 4);
        root.Controls.Add(groupResult, 0, 5);
        root.Controls.Add(groupAgent, 0, 6);

        Controls.Add(root);

        if (Context.Chat is null)
        {
            _memoPrompt.Enabled = false;
            _btnAskAgent.Enabled = false;
            _memoAgent.Text =
                $"LLM not configured: {Context.ChatUnavailableReason}\r\n" +
                "The Direct button above still works without an API key.";
        }
        else
        {
            _memoPrompt.Text = DefaultPrompt;
        }
    }

    private GroupControl BuildAgentGroup()
    {
        var group = new GroupControl { Text = "Ask the agent (LLM runs the same policy)", Dock = DockStyle.Fill };

        _memoAgent = ReadonlyMemo();
        _memoAgent.Dock = DockStyle.Fill;

        var askBar = new PanelControl { Dock = DockStyle.Top, Height = 34 };
        ((System.ComponentModel.ISupportInitialize)askBar).BeginInit();
        askBar.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
        _btnAskAgent = new SimpleButton { Text = "Ask agent", Dock = DockStyle.Right, Width = 140 };
        _btnAskAgent.Click += async (_, _) => await AskAgentAsync();
        askBar.Controls.Add(_btnAskAgent);
        ((System.ComponentModel.ISupportInitialize)askBar).EndInit();

        _memoPrompt = new MemoEdit { Dock = DockStyle.Top, Height = 48 };

        // Add the fill control first, then the docked bars (last added docks outermost).
        group.Controls.Add(_memoAgent);
        group.Controls.Add(askBar);
        group.Controls.Add(_memoPrompt);
        return group;
    }

    private static MemoEdit ReadonlyMemo()
    {
        var memo = new MemoEdit();
        memo.Properties.ReadOnly = true;
        memo.Properties.ScrollBars = ScrollBars.Vertical;
        return memo;
    }

    // --- Run paths ---------------------------------------------------------

    private async Task RunDirectAsync()
    {
        var command = BuildDirectCommand();
        _memoCommand.Text = command;

        if (!Context.Runner.IsSandboxAvailable)
        {
            SandboxResultView.Show(_memoResult,
                "Sandbox unavailable: no MXC executor was found. Set MXC_BIN_DIR (see README Part 2).");
            return;
        }

        SetBusy(true);
        try
        {
            var result = await Context.Runner.RunAsync(command, BuildOptions(), Context.Workspace);
            SandboxResultView.Show(_memoResult, SandboxResultView.Format(result));
        }
        catch (Exception ex)
        {
            SandboxResultView.Show(_memoResult, "Error: " + ex.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task AskAgentAsync()
    {
        if (Context.Chat is null) return;

        var prompt = _memoPrompt.Text.Trim();
        if (prompt.Length == 0) return;

        // A fresh agent per ask: bind a run_code tool to this panel's live policy and give the
        // model no memory of earlier (differently-policied) attempts, so the visible transcript
        // stays an honest "this policy -> this result" log instead of the model repeating a
        // stale refusal after you change a control.
        var agent = new AgentService(
            Context.Chat,
            [CodeExecutionTool.Create(Context.Runner, BuildOptions, Context.Workspace,
                (channel, text) => SandboxResultView.Append(_memoAgent, channel, text))],
            Context.SystemPrompt + AgentHint);

        SandboxResultView.Append(_memoAgent, "You", prompt);
        SetBusy(true);
        try
        {
            var reply = await agent.SendAsync(prompt);
            SandboxResultView.Append(_memoAgent, "Agent", reply);
        }
        catch (Exception ex)
        {
            SandboxResultView.Append(_memoAgent, "Error", ex.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        _btnRunDirect.Enabled = !busy;
        _btnAskAgent.Enabled = !busy && Context.Chat is not null;
        _memoPrompt.Enabled = !busy && Context.Chat is not null;
        _btnRunDirect.Text = busy ? "Working…" : "Run (direct)";
    }
}
