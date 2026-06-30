using AgentSampleApp.Configuration;
using AgentSampleApp.Mxc;
using AgentSampleApp.Ui;
using AgentSampleApp.Ui.Examples;
using DevExpress.XtraBars.Navigation;
using Microsoft.Extensions.AI;

namespace AgentSampleApp;

/// <summary>
/// The application shell: an accordion navigation pane on the left and a content area that
/// hosts one screen at a time — the free-form chat plus one panel per sandbox capability.
/// </summary>
public partial class MainForm : DevExpress.XtraEditors.XtraForm
{
    public MainForm(
        IChatClient? chatClient,
        ISandboxRunner runner,
        AgentConfig config,
        string workspace,
        string? chatUnavailableReason)
    {
        InitializeComponent();

        var context = new SandboxAppContext(runner, chatClient, workspace, config.SystemPrompt, chatUnavailableReason);

        lblBackend.Text = "MXC backend: " + runner.BackendSummary;
        lblWorkspace.Text = "Workspace: " + workspace;

        // One screen per accordion item; the Tag carries the control to show.
        var chat = new ChatControl(context);
        var elemChat = Item("Chat", chat);

        var general = new AccordionControlElement { Style = ElementStyle.Group, Text = "General" };
        general.Elements.Add(elemChat);

        var sandbox = new AccordionControlElement { Style = ElementStyle.Group, Text = "Sandbox capabilities" };
        sandbox.Elements.Add(Item("Files", new FileAccessControl(context)));
        sandbox.Elements.Add(Item("Network", new NetworkControl(context)));
        sandbox.Elements.Add(Item("Timeout", new TimeoutControl(context)));
        sandbox.Elements.Add(Item("Hardened", new HardenedControl(context)));

        accordion.Elements.Add(general);
        accordion.Elements.Add(sandbox);

        accordion.SelectedElementChanged += (_, e) =>
        {
            if (e.Element?.Tag is Control content) ShowContent(content);
        };

        // Show the chat first.
        accordion.SelectedElement = elemChat;
        ShowContent(chat);
    }

    /// <summary>Build an accordion item whose Tag is the control to display when selected.</summary>
    private AccordionControlElement Item(string text, Control content)
    {
        content.Dock = DockStyle.Fill;
        content.Visible = false;
        panelContent.Controls.Add(content);
        return new AccordionControlElement { Style = ElementStyle.Item, Text = text, Tag = content };
    }

    private void ShowContent(Control content)
    {
        foreach (Control c in panelContent.Controls)
        {
            c.Visible = ReferenceEquals(c, content);
        }
        content.BringToFront();
    }
}
