using AgentSampleApp.Mxc;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

namespace AgentSampleApp.Ui.Examples;

/// <summary>
/// Demonstrates <c>FilesystemPolicy</c>. The same file is read or written under different
/// grants, so you can watch the sandbox allow or deny the exact same command.
/// </summary>
public sealed class FileAccessControl : SandboxExampleControlBase
{
    private enum FileAccess { None, ReadOnly, ReadWrite, Denied }

    // A demo file deliberately placed OUTSIDE the workspace, so by default it is invisible
    // to the sandbox until its folder is granted. Created on the host (not the sandbox).
    private static readonly string DemoFile =
        Path.Combine(Path.GetTempPath(), "AgentSampleApp", "outside", "secret.txt");

    private TextEdit _pathEdit = null!;
    private RadioGroup _accessRadio = null!;
    private ToggleSwitch _opToggle = null!;

    public FileAccessControl(SandboxAppContext context) : base(context)
    {
        EnsureDemoFile();
        BuildPolicyControls();
        RefreshCommand();
    }

    protected override string ExampleTitle => "Files — filesystem policy";

    protected override string ExampleDescription =>
        "The workspace is always read/write. A file outside it is invisible unless you grant " +
        "its folder. It starts read-only so the file reads; switch to None or Denied to watch " +
        "the same command get blocked ('Denied' wins even over a read grant).";

    protected override string DefaultPrompt =>
        $"Read the file at {DemoFile} and show me its contents.";

    private void BuildPolicyControls()
    {
        _opToggle = new ToggleSwitch();
        _opToggle.Properties.OffText = "Read";
        _opToggle.Properties.OnText = "Write";
        _opToggle.Toggled += (_, _) => RefreshCommand();
        AddPolicyControl("Operation:", _opToggle);

        _accessRadio = new RadioGroup();
        _accessRadio.Properties.Columns = 4;
        _accessRadio.Properties.Items.AddRange(new[]
        {
            new RadioGroupItem(FileAccess.None, "None"),
            new RadioGroupItem(FileAccess.ReadOnly, "Read-only"),
            new RadioGroupItem(FileAccess.ReadWrite, "Read-write"),
            new RadioGroupItem(FileAccess.Denied, "Denied (read+deny)"),
        });
        _accessRadio.EditValue = FileAccess.ReadOnly;
        _accessRadio.SelectedIndexChanged += (_, _) => RefreshCommand();
        _accessRadio.AutoSizeInLayoutControl = true;
        AddPolicyControl("Folder access:", _accessRadio);

        _pathEdit = new TextEdit { Text = DemoFile };
        _pathEdit.EditValueChanged += (_, _) => RefreshCommand();
        AddPolicyControl("File path:", _pathEdit);

        var btnBrowse = new SimpleButton { Text = "Browse…" };
        btnBrowse.Click += (_, _) =>
        {
            using var dlg = new OpenFileDialog();
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _pathEdit.Text = dlg.FileName;
                RefreshCommand();
            }
        };
        AddPolicyControl(string.Empty, btnBrowse);
    }

    protected override SandboxRunOptions BuildOptions()
    {
        var dir = Path.GetDirectoryName(_pathEdit.Text) ?? string.Empty;
        return (FileAccess)_accessRadio.EditValue switch
        {
            FileAccess.ReadOnly => new SandboxRunOptions { ReadonlyPaths = [dir] },
            FileAccess.ReadWrite => new SandboxRunOptions { ReadwritePaths = [dir] },
            // Grant read AND deny, to show DeniedPaths wins over a read grant.
            FileAccess.Denied => new SandboxRunOptions { ReadonlyPaths = [dir], DeniedPaths = [dir] },
            _ => new SandboxRunOptions(), // None — outside the workspace, so inaccessible.
        };
    }

    protected override string BuildDirectCommand()
    {
        var path = _pathEdit.Text;
        return _opToggle.IsOn
            ? $"echo sandbox wrote at %TIME%>> \"{path}\" & type \"{path}\""
            : $"type \"{path}\"";
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
