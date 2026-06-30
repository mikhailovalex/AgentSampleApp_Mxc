namespace AgentSampleApp;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private DevExpress.XtraEditors.PanelControl panelStatus;
    private DevExpress.XtraEditors.LabelControl lblBackend;
    private DevExpress.XtraEditors.LabelControl lblWorkspace;
    private DevExpress.XtraBars.Navigation.AccordionControl accordion;
    private DevExpress.XtraEditors.PanelControl panelContent;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent() {
        panelStatus = new DevExpress.XtraEditors.PanelControl();
        lblWorkspace = new DevExpress.XtraEditors.LabelControl();
        lblBackend = new DevExpress.XtraEditors.LabelControl();
        accordion = new DevExpress.XtraBars.Navigation.AccordionControl();
        panelContent = new DevExpress.XtraEditors.PanelControl();
        ((System.ComponentModel.ISupportInitialize)panelStatus).BeginInit();
        panelStatus.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)accordion).BeginInit();
        ((System.ComponentModel.ISupportInitialize)panelContent).BeginInit();
        SuspendLayout();
        // 
        // panelStatus
        // 
        panelStatus.Controls.Add(lblWorkspace);
        panelStatus.Controls.Add(lblBackend);
        panelStatus.Dock = DockStyle.Top;
        panelStatus.Location = new Point(0, 0);
        panelStatus.Name = "panelStatus";
        panelStatus.Size = new Size(960, 34);
        panelStatus.TabIndex = 0;
        // 
        // lblWorkspace
        // 
        lblWorkspace.Location = new Point(360, 9);
        lblWorkspace.Name = "lblWorkspace";
        lblWorkspace.Size = new Size(83, 16);
        lblWorkspace.TabIndex = 1;
        lblWorkspace.Text = "Workspace: …";
        // 
        // lblBackend
        // 
        lblBackend.Location = new Point(12, 9);
        lblBackend.Name = "lblBackend";
        lblBackend.Size = new Size(97, 16);
        lblBackend.TabIndex = 0;
        lblBackend.Text = "MXC backend: …";
        // 
        // accordion
        // 
        accordion.AllowItemSelection = true;
        accordion.Dock = DockStyle.Left;
        accordion.Location = new Point(0, 34);
        accordion.Name = "accordion";
        accordion.Size = new Size(210, 595);
        accordion.TabIndex = 1;
        // 
        // panelContent
        // 
        panelContent.Dock = DockStyle.Fill;
        panelContent.Location = new Point(210, 34);
        panelContent.Name = "panelContent";
        panelContent.Size = new Size(750, 595);
        panelContent.TabIndex = 2;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 16F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(960, 629);
        Controls.Add(panelContent);
        Controls.Add(accordion);
        Controls.Add(panelStatus);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Agent Sample — WinForms + DevExpress + MXC";
        ((System.ComponentModel.ISupportInitialize)panelStatus).EndInit();
        panelStatus.ResumeLayout(false);
        panelStatus.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)accordion).EndInit();
        ((System.ComponentModel.ISupportInitialize)panelContent).EndInit();
        ResumeLayout(false);
    }
}
