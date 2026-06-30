using AgentSampleApp.Agent;
using AgentSampleApp.Configuration;
using AgentSampleApp.Mxc;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Sabbour.Mxc.Sdk.Sandbox;

namespace AgentSampleApp;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        // Standard WinForms init (visual styles, high-DPI, text rendering).
        ApplicationConfiguration.Initialize();

        // Optional: pick a DevExpress skin here, e.g.
        // DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle(DevExpress.LookAndFeel.SkinStyle.WXI);

        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var agentConfig = configuration.GetSection("Agent").Get<AgentConfig>() ?? new AgentConfig();

        // Per-run workspace that the sandbox is allowed to read/write.
        var workspace = Path.Combine(Path.GetTempPath(), "AgentSampleApp", "workspace");
        Directory.CreateDirectory(workspace);

        ISandboxRunner runner = new MxcSandboxRunner(agentConfig.ToolPaths);

        // The chat client is optional: if no API key/endpoint is configured we still launch,
        // with the chat and the per-example "Ask agent" sections disabled. The direct
        // (no-LLM) sandbox examples work regardless.
        IChatClient? chatClient = null;
        string? chatUnavailableReason = null;
        try
        {
            chatClient = ChatClientFactory.Create(agentConfig);
        }
        catch (Exception ex)
        {
            chatUnavailableReason = ex.Message;
        }

        Application.Run(new MainForm(chatClient, runner, agentConfig, workspace, chatUnavailableReason));
    }
}
