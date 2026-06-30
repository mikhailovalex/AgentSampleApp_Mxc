using AgentSampleApp.Mxc;
using DevExpress.XtraEditors;

namespace AgentSampleApp.Ui.Examples;

/// <summary>
/// Demonstrates <c>NetworkPolicy</c>. The same HTTP request is blocked or allowed by the
/// outbound toggle. Needs Python on the host (granted via <c>Agent:ToolPaths</c>) and, of
/// course, internet access.
///
/// Per-host allow/block lists (<c>NetworkPolicy.AllowedHosts</c>) are not yet supported by the
/// Windows executor — it rejects any policy that sets them — so this panel toggles outbound as
/// a whole.
/// </summary>
public sealed class NetworkControl : SandboxExampleControlBase
{
    private const string DefaultUrl = "https://api.github.com/zen";

    private ToggleSwitch _outbound = null!;
    private TextEdit _url = null!;

    public NetworkControl(SandboxAppContext context) : base(context)
    {
        BuildPolicyControls();
        RefreshCommand();
    }

    protected override string ExampleTitle => "Network — outbound policy";

    protected override string ExampleDescription =>
        "With outbound blocked, the request fails at the OS boundary — no network leaves the " +
        "sandbox. Turn it on and the same command succeeds. (Per-host allow/block lists are not " +
        "supported on Windows executors yet, so this is a whole-network toggle.)";

    protected override string DefaultPrompt =>
        $"Fetch {DefaultUrl} and show me the response body.";

    private void BuildPolicyControls()
    {
        _url = new TextEdit { Text = DefaultUrl };
        _url.EditValueChanged += (_, _) => RefreshCommand();
        AddPolicyControl("URL:", _url);

        _outbound = new ToggleSwitch();
        _outbound.Properties.OffText = "Blocked";
        _outbound.Properties.OnText = "Allowed";
        _outbound.Toggled += (_, _) => RefreshCommand();
        AddPolicyControl("Outbound network:", _outbound);
    }

    protected override SandboxRunOptions BuildOptions() =>
        new() { AllowOutbound = _outbound.IsOn };

    protected override string BuildDirectCommand()
    {
        var url = _url.Text;
        return "python -c \"import urllib.request as u; " +
               $"print(u.urlopen('{url}', timeout=10).read().decode('utf-8','replace')[:300])\"";
    }
}
