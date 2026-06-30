namespace AgentSampleApp.Ui;

partial class ChatControl
{
    private System.ComponentModel.IContainer components = null;

    private DevExpress.XtraEditors.PanelControl panelTop;
    private DevExpress.XtraEditors.ToggleSwitch toggleStrict;
    private DevExpress.XtraEditors.MemoEdit memoTranscript;
    private DevExpress.XtraEditors.PanelControl panelBottom;
    private DevExpress.XtraEditors.MemoEdit memoInput;
    private DevExpress.XtraEditors.SimpleButton btnSend;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.panelTop = new DevExpress.XtraEditors.PanelControl();
        this.toggleStrict = new DevExpress.XtraEditors.ToggleSwitch();
        this.memoTranscript = new DevExpress.XtraEditors.MemoEdit();
        this.panelBottom = new DevExpress.XtraEditors.PanelControl();
        this.memoInput = new DevExpress.XtraEditors.MemoEdit();
        this.btnSend = new DevExpress.XtraEditors.SimpleButton();
        ((System.ComponentModel.ISupportInitialize)(this.panelTop)).BeginInit();
        this.panelTop.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.toggleStrict.Properties)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.memoTranscript.Properties)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.panelBottom)).BeginInit();
        this.panelBottom.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.memoInput.Properties)).BeginInit();
        this.SuspendLayout();
        //
        // panelTop
        //
        this.panelTop.Controls.Add(this.toggleStrict);
        this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
        this.panelTop.Location = new System.Drawing.Point(0, 0);
        this.panelTop.Name = "panelTop";
        this.panelTop.Size = new System.Drawing.Size(690, 48);
        this.panelTop.TabIndex = 0;
        //
        // toggleStrict
        //
        this.toggleStrict.Location = new System.Drawing.Point(12, 11);
        this.toggleStrict.Name = "toggleStrict";
        this.toggleStrict.Properties.OffText = "Permissive";
        this.toggleStrict.Properties.OnText = "Restricted";
        this.toggleStrict.Size = new System.Drawing.Size(170, 25);
        this.toggleStrict.TabIndex = 0;
        this.toggleStrict.IsOn = true;
        //
        // memoTranscript
        //
        this.memoTranscript.Dock = System.Windows.Forms.DockStyle.Fill;
        this.memoTranscript.Location = new System.Drawing.Point(0, 48);
        this.memoTranscript.Name = "memoTranscript";
        this.memoTranscript.Properties.ReadOnly = true;
        this.memoTranscript.Properties.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.memoTranscript.Size = new System.Drawing.Size(690, 292);
        this.memoTranscript.TabIndex = 1;
        //
        // panelBottom
        //
        this.panelBottom.Controls.Add(this.memoInput);
        this.panelBottom.Controls.Add(this.btnSend);
        this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.panelBottom.Location = new System.Drawing.Point(0, 340);
        this.panelBottom.Name = "panelBottom";
        this.panelBottom.Size = new System.Drawing.Size(690, 110);
        this.panelBottom.TabIndex = 2;
        //
        // memoInput
        //
        this.memoInput.Dock = System.Windows.Forms.DockStyle.Fill;
        this.memoInput.Location = new System.Drawing.Point(2, 2);
        this.memoInput.Name = "memoInput";
        this.memoInput.Size = new System.Drawing.Size(568, 106);
        this.memoInput.TabIndex = 0;
        //
        // btnSend
        //
        this.btnSend.Dock = System.Windows.Forms.DockStyle.Right;
        this.btnSend.Location = new System.Drawing.Point(570, 2);
        this.btnSend.Name = "btnSend";
        this.btnSend.Size = new System.Drawing.Size(118, 106);
        this.btnSend.TabIndex = 1;
        this.btnSend.Text = "Send\r\n(Ctrl+Enter)";
        //
        // ChatControl
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.memoTranscript);
        this.Controls.Add(this.panelBottom);
        this.Controls.Add(this.panelTop);
        this.Name = "ChatControl";
        this.Size = new System.Drawing.Size(690, 450);
        ((System.ComponentModel.ISupportInitialize)(this.toggleStrict.Properties)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.panelTop)).EndInit();
        this.panelTop.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.memoTranscript.Properties)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.memoInput.Properties)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.panelBottom)).EndInit();
        this.panelBottom.ResumeLayout(false);
        this.ResumeLayout(false);
    }
}
