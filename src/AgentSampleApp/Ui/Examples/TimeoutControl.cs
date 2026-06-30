using AgentSampleApp.Mxc;
using DevExpress.XtraEditors;

namespace AgentSampleApp.Ui.Examples;

/// <summary>
/// Demonstrates <c>TimeoutMs</c>. A Python command sleeps for a chosen number of seconds;
/// when that exceeds the timeout, the executor kills the run. Needs Python on the host
/// (granted via <c>Agent:ToolPaths</c>).
/// </summary>
public sealed class TimeoutControl : SandboxExampleControlBase
{
    private SpinEdit _timeoutMs = null!;
    private SpinEdit _sleepSeconds = null!;

    public TimeoutControl(SandboxAppContext context) : base(context)
    {
        BuildPolicyControls();
        RefreshCommand();
    }

    protected override string ExampleTitle => "Timeout — wall-clock limit";

    protected override string ExampleDescription =>
        "The sandbox enforces a hard time limit. Set the timeout below the sleep duration and " +
        "the command is killed mid-run; raise the timeout (or lower the sleep) and it completes. " +
        "The agent below runs under the same limit.";

    protected override string DefaultPrompt =>
        "Run a Python one-liner that sleeps for 5 seconds and then prints DONE.";

    private void BuildPolicyControls()
    {
        _sleepSeconds = Spin(min: 0, max: 30, value: 5);
        AddPolicyControl("Sleep (seconds):", _sleepSeconds);

        _timeoutMs = Spin(min: 500, max: 15000, value: 3000, increment: 500);
        AddPolicyControl("Timeout (ms):", _timeoutMs);
    }

    protected override SandboxRunOptions BuildOptions() =>
        new() { TimeoutMs = (int)_timeoutMs.Value };

    protected override string BuildDirectCommand()
    {
        var seconds = (int)_sleepSeconds.Value;
        return $"python -c \"import time; time.sleep({seconds}); print('completed after {seconds}s')\"";
    }

    private SpinEdit Spin(int min, int max, int value, int increment = 1)
    {
        var spin = new SpinEdit();
        spin.Properties.IsFloatValue = false;
        spin.Properties.MinValue = min;
        spin.Properties.MaxValue = max;
        spin.Properties.Increment = increment;
        spin.Value = value;
        spin.EditValueChanged += (_, _) => RefreshCommand();
        return spin;
    }
}
