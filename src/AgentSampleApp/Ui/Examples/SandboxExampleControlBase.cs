using AgentSampleApp.Agent;
using AgentSampleApp.Mxc;
using DevExpress.XtraEditors;

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
/// The chrome lives in <c>SandboxExampleControlBase.Designer.cs</c> (a DevExpress
/// <see cref="DevExpress.XtraLayout.LayoutControl"/>) so the panels open in the WinForms
/// designer. The class is concrete with a parameterless constructor for that reason; the
/// real app uses the <see cref="SandboxAppContext"/> constructor, and subclasses fill
/// <see cref="PolicyGroup"/> with their policy controls.
/// </summary>
public partial class SandboxExampleControlBase : XtraUserControl
{
    // Steers the model toward commands that actually run in this sandbox. PowerShell fails to
    // initialize under the AppContainer tier (STATUS_DLL_INIT_FAILED), which would mask the
    // policy effect the example is meant to show. Appended to the configured system prompt.
    private const string AgentHint =
        "\n\nEnvironment notes for run_code: prefer cmd.exe built-ins (type, dir, set /a) or " +
        "`python`. Do NOT use PowerShell — it fails to initialize in this sandbox. If a command " +
        "is blocked by the sandbox policy, report that result plainly; do not suggest running it " +
        "outside the sandbox, because demonstrating the policy is the whole point.";

    protected SandboxAppContext Context { get; private set; } = null!;

    /// <summary>Adds one labeled policy control to the "Sandbox policy"
    /// <see cref="DevExpress.XtraLayout.LayoutControlGroup"/> as a layout item. The
    /// <see cref="DevExpress.XtraLayout.LayoutControl"/> reparents the control and arranges it;
    /// the returned item lets the caller tune sizing if needed.</summary>
    protected DevExpress.XtraLayout.LayoutControlItem AddPolicyControl(string caption, Control field)
    {
        field.Dock = DockStyle.None;
        field.Name = Guid.NewGuid().ToString();
        return (DevExpress.XtraLayout.LayoutControlItem)groupPolicy.AddItem(caption, field);
    }

    /// <summary>Parameterless constructor for the WinForms designer. Not used at runtime.</summary>
    public SandboxExampleControlBase()
    {
        InitializeComponent();
    }

    protected SandboxExampleControlBase(SandboxAppContext context) : this()
    {
        Context = context;
        InitializeRuntime();
    }

    // --- Subclass contract -------------------------------------------------

    protected virtual string ExampleTitle => string.Empty;
    protected virtual string ExampleDescription => string.Empty;
    protected virtual string DefaultPrompt => string.Empty;

    /// <summary>Build the sandbox policy from the current state of the policy controls.</summary>
    protected virtual SandboxRunOptions BuildOptions() => new();

    /// <summary>The exact shell command the Direct button runs (also shown to the user).</summary>
    protected virtual string BuildDirectCommand() => string.Empty;

    /// <summary>Subclasses call this after their controls change so the shown command stays live.</summary>
    protected void RefreshCommand() => memoCommand.Text = BuildDirectCommand();

    // --- Runtime wiring ----------------------------------------------------

    private void InitializeRuntime()
    {
        lblTitle.Text = ExampleTitle;
        lblDescription.Text = ExampleDescription;

        btnRunDirect.Click += async (_, _) => await RunDirectAsync();
        btnAskAgent.Click += async (_, _) => await AskAgentAsync();

        if (Context.Chat is null)
        {
            memoPrompt.Enabled = false;
            btnAskAgent.Enabled = false;
            memoAgent.Text =
                $"LLM not configured: {Context.ChatUnavailableReason}\r\n" +
                "The Direct button above still works without an API key.";
        }
        else
        {
            memoPrompt.Text = DefaultPrompt;
        }
    }

    // --- Run paths ---------------------------------------------------------

    private async Task RunDirectAsync()
    {
        var command = BuildDirectCommand();
        memoCommand.Text = command;

        if (!Context.Runner.IsSandboxAvailable)
        {
            SandboxResultView.Show(memoResult,
                "Sandbox unavailable: no MXC executor was found. Set MXC_BIN_DIR (see README Part 2).");
            return;
        }

        SetBusy(true);
        try
        {
            var result = await Context.Runner.RunAsync(command, BuildOptions(), Context.Workspace);
            SandboxResultView.Show(memoResult, SandboxResultView.Format(result));
        }
        catch (Exception ex)
        {
            SandboxResultView.Show(memoResult, "Error: " + ex.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task AskAgentAsync()
    {
        if (Context.Chat is null) return;

        var prompt = memoPrompt.Text.Trim();
        if (prompt.Length == 0) return;

        // A fresh agent per ask: bind a run_code tool to this panel's live policy and give the
        // model no memory of earlier (differently-policied) attempts, so the visible transcript
        // stays an honest "this policy -> this result" log instead of the model repeating a
        // stale refusal after you change a control.
        var agent = new AgentService(
            Context.Chat,
            [CodeExecutionTool.Create(Context.Runner, BuildOptions, Context.Workspace,
                (channel, text) => SandboxResultView.Append(memoAgent, channel, text))],
            Context.SystemPrompt + AgentHint);

        SandboxResultView.Append(memoAgent, "You", prompt);
        SetBusy(true);
        try
        {
            var reply = await agent.SendAsync(prompt);
            SandboxResultView.Append(memoAgent, "Agent", reply);
        }
        catch (Exception ex)
        {
            SandboxResultView.Append(memoAgent, "Error", ex.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        btnRunDirect.Enabled = !busy;
        btnAskAgent.Enabled = !busy && Context.Chat is not null;
        memoPrompt.Enabled = !busy && Context.Chat is not null;
        btnRunDirect.Text = busy ? "Working…" : "Run (direct)";
    }
}
