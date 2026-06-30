using AgentSampleApp.Mxc;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

namespace AgentSampleApp.Ui.Examples;

/// <summary>
/// Demonstrates composing a hardened profile: a denied path that overrides a read grant,
/// a UI lock-down, and a timeout. The denied-path and timeout effects are visible from a
/// console command; the UI fields (windows / clipboard / input injection) apply to GUI
/// subprocesses, so they set real policy without a visible console effect — the value here
/// is showing how the fields combine.
/// </summary>
public sealed class HardenedControl : SandboxExampleControlBase
{
    // Same file the Files example uses: granted read-only here, so toggling Deny shows the
    // denied path overriding the grant.
    private static readonly string DemoFile =
        Path.Combine(Path.GetTempPath(), "AgentSampleApp", "outside", "secret.txt");

    private ToggleSwitch _denyFolder = null!;
    private ToggleSwitch _allowWindows = null!;
    private ComboBoxEdit _clipboard = null!;
    private ToggleSwitch _allowInput = null!;
    private SpinEdit _timeoutMs = null!;

    public HardenedControl(SandboxAppContext context) : base(context)
    {
        EnsureDemoFile();
        BuildPolicyControls();
        RefreshCommand();
    }

    protected override string ExampleTitle => "Hardened — composed lock-down";

    protected override string ExampleDescription =>
        "The demo file is granted read-only. Turn Deny on and the denied path wins over that " +
        "grant — yet the harmless computation after it still runs. The UI fields apply to GUI " +
        "subprocesses, so they shape the policy without changing this console output.";

    protected override string DefaultPrompt =>
        $"Try to read the file at {DemoFile}. If it is blocked, just compute 2+2 instead.";

    private void BuildPolicyControls()
    {
        _timeoutMs = new SpinEdit();
        _timeoutMs.Properties.IsFloatValue = false;
        _timeoutMs.Properties.MinValue = 1000;
        _timeoutMs.Properties.MaxValue = 30000;
        _timeoutMs.Properties.Increment = 1000;
        _timeoutMs.Value = 10000;
        AddPolicyControl("Timeout (ms):", _timeoutMs);

        _allowInput = Toggle("Blocked", "Allowed", isOn: false);
        AddPolicyControl("Input injection:", _allowInput);

        _clipboard = new ComboBoxEdit();
        _clipboard.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
        _clipboard.Properties.Items.AddRange(new object[]
        {
            ClipboardAccess.None, ClipboardAccess.Read, ClipboardAccess.Write, ClipboardAccess.All,
        });
        _clipboard.SelectedItem = ClipboardAccess.None;
        AddPolicyControl("Clipboard:", _clipboard);

        _allowWindows = Toggle("Blocked", "Allowed", isOn: false);
        AddPolicyControl("Create windows:", _allowWindows);

        _denyFolder = Toggle("Off", "On", isOn: true);
        _denyFolder.Toggled += (_, _) => RefreshCommand();
        AddPolicyControl("Deny demo folder:", _denyFolder);
    }

    protected override SandboxRunOptions BuildOptions()
    {
        var dir = Path.GetDirectoryName(DemoFile) ?? string.Empty;
        string[] denied = _denyFolder.IsOn ? [dir] : [];
        return new SandboxRunOptions
        {
            ReadonlyPaths = [dir],
            DeniedPaths = denied,
            TimeoutMs = (int)_timeoutMs.Value,
            AllowWindows = _allowWindows.IsOn,
            Clipboard = (ClipboardAccess)(_clipboard.SelectedItem ?? ClipboardAccess.None),
            AllowInputInjection = _allowInput.IsOn,
        };
    }

    protected override string BuildDirectCommand() =>
        $"type \"{DemoFile}\" & echo --- & set /a 2+2";

    private ToggleSwitch Toggle(string off, string on, bool isOn)
    {
        var toggle = new ToggleSwitch { IsOn = isOn };
        toggle.Properties.OffText = off;
        toggle.Properties.OnText = on;
        return toggle;
    }

    private static void EnsureDemoFile()
    {
        var dir = Path.GetDirectoryName(DemoFile)!;
        Directory.CreateDirectory(dir);
        if (!File.Exists(DemoFile))
        {
            File.WriteAllText(DemoFile, "TOP SECRET: launch codes are 0000 0000\r\n");
        }
    }
}
