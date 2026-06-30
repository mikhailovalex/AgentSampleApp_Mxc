namespace AgentSampleApp.Ui.Examples;

partial class SandboxExampleControlBase
{
    private System.ComponentModel.IContainer components = null;

    private DevExpress.XtraLayout.LayoutControl layoutControl;
    private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroupRoot;

    private DevExpress.XtraEditors.LabelControl lblTitle;
    private DevExpress.XtraEditors.LabelControl lblDescription;
    private DevExpress.XtraEditors.LabelControl lblCmdCaption;
    private DevExpress.XtraEditors.SimpleButton btnRunDirect;
    private DevExpress.XtraEditors.MemoEdit memoCommand;
    private DevExpress.XtraEditors.MemoEdit memoResult;
    private DevExpress.XtraEditors.MemoEdit memoPrompt;
    private DevExpress.XtraEditors.SimpleButton btnAskAgent;
    private DevExpress.XtraEditors.MemoEdit memoAgent;

    private DevExpress.XtraLayout.LayoutControlGroup groupPolicy;
    private DevExpress.XtraLayout.LayoutControlGroup groupResult;
    private DevExpress.XtraLayout.LayoutControlGroup groupAgent;
    private DevExpress.XtraLayout.LayoutControlGroup groupAsk;

    private DevExpress.XtraLayout.LayoutControlItem layoutItemTitle;
    private DevExpress.XtraLayout.LayoutControlItem layoutItemDescription;
    private DevExpress.XtraLayout.LayoutControlItem layoutItemCmdCaption;
    private DevExpress.XtraLayout.LayoutControlItem layoutItemRunBtn;
    private DevExpress.XtraLayout.LayoutControlItem layoutItemCommand;
    private DevExpress.XtraLayout.LayoutControlItem layoutItemResult;
    private DevExpress.XtraLayout.LayoutControlItem layoutItemPrompt;
    private DevExpress.XtraLayout.EmptySpaceItem emptyAsk;
    private DevExpress.XtraLayout.LayoutControlItem layoutItemAskBtn;
    private DevExpress.XtraLayout.LayoutControlItem layoutItemAgent;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent() {
        layoutControl = new DevExpress.XtraLayout.LayoutControl();
        lblTitle = new DevExpress.XtraEditors.LabelControl();
        lblDescription = new DevExpress.XtraEditors.LabelControl();
        lblCmdCaption = new DevExpress.XtraEditors.LabelControl();
        btnRunDirect = new DevExpress.XtraEditors.SimpleButton();
        memoCommand = new DevExpress.XtraEditors.MemoEdit();
        memoResult = new DevExpress.XtraEditors.MemoEdit();
        memoPrompt = new DevExpress.XtraEditors.MemoEdit();
        btnAskAgent = new DevExpress.XtraEditors.SimpleButton();
        memoAgent = new DevExpress.XtraEditors.MemoEdit();
        layoutControlGroupRoot = new DevExpress.XtraLayout.LayoutControlGroup();
        layoutItemTitle = new DevExpress.XtraLayout.LayoutControlItem();
        layoutItemDescription = new DevExpress.XtraLayout.LayoutControlItem();
        groupPolicy = new DevExpress.XtraLayout.LayoutControlGroup();
        groupAgent = new DevExpress.XtraLayout.LayoutControlGroup();
        layoutItemPrompt = new DevExpress.XtraLayout.LayoutControlItem();
        groupAsk = new DevExpress.XtraLayout.LayoutControlGroup();
        emptyAsk = new DevExpress.XtraLayout.EmptySpaceItem();
        layoutItemAskBtn = new DevExpress.XtraLayout.LayoutControlItem();
        layoutItemAgent = new DevExpress.XtraLayout.LayoutControlItem();
        groupResult = new DevExpress.XtraLayout.LayoutControlGroup();
        layoutItemResult = new DevExpress.XtraLayout.LayoutControlItem();
        layoutItemCmdCaption = new DevExpress.XtraLayout.LayoutControlItem();
        layoutItemRunBtn = new DevExpress.XtraLayout.LayoutControlItem();
        layoutItemCommand = new DevExpress.XtraLayout.LayoutControlItem();
        ((System.ComponentModel.ISupportInitialize)layoutControl).BeginInit();
        layoutControl.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)memoCommand.Properties).BeginInit();
        ((System.ComponentModel.ISupportInitialize)memoResult.Properties).BeginInit();
        ((System.ComponentModel.ISupportInitialize)memoPrompt.Properties).BeginInit();
        ((System.ComponentModel.ISupportInitialize)memoAgent.Properties).BeginInit();
        ((System.ComponentModel.ISupportInitialize)layoutControlGroupRoot).BeginInit();
        ((System.ComponentModel.ISupportInitialize)layoutItemTitle).BeginInit();
        ((System.ComponentModel.ISupportInitialize)layoutItemDescription).BeginInit();
        ((System.ComponentModel.ISupportInitialize)groupPolicy).BeginInit();
        ((System.ComponentModel.ISupportInitialize)groupAgent).BeginInit();
        ((System.ComponentModel.ISupportInitialize)layoutItemPrompt).BeginInit();
        ((System.ComponentModel.ISupportInitialize)groupAsk).BeginInit();
        ((System.ComponentModel.ISupportInitialize)emptyAsk).BeginInit();
        ((System.ComponentModel.ISupportInitialize)layoutItemAskBtn).BeginInit();
        ((System.ComponentModel.ISupportInitialize)layoutItemAgent).BeginInit();
        ((System.ComponentModel.ISupportInitialize)groupResult).BeginInit();
        ((System.ComponentModel.ISupportInitialize)layoutItemResult).BeginInit();
        ((System.ComponentModel.ISupportInitialize)layoutItemCmdCaption).BeginInit();
        ((System.ComponentModel.ISupportInitialize)layoutItemRunBtn).BeginInit();
        ((System.ComponentModel.ISupportInitialize)layoutItemCommand).BeginInit();
        SuspendLayout();
        // 
        // layoutControl
        // 
        layoutControl.Controls.Add(lblTitle);
        layoutControl.Controls.Add(lblDescription);
        layoutControl.Controls.Add(lblCmdCaption);
        layoutControl.Controls.Add(btnRunDirect);
        layoutControl.Controls.Add(memoCommand);
        layoutControl.Controls.Add(memoResult);
        layoutControl.Controls.Add(memoPrompt);
        layoutControl.Controls.Add(btnAskAgent);
        layoutControl.Controls.Add(memoAgent);
        layoutControl.Dock = DockStyle.Fill;
        layoutControl.Location = new Point(0, 0);
        layoutControl.Name = "layoutControl";
        layoutControl.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new Rectangle(3127, 597, 812, 500);
        layoutControl.Root = layoutControlGroupRoot;
        layoutControl.Size = new Size(700, 743);
        layoutControl.TabIndex = 0;
        // 
        // lblTitle
        // 
        lblTitle.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblTitle.Appearance.Options.UseFont = true;
        lblTitle.Location = new Point(14, 14);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(43, 28);
        lblTitle.StyleController = layoutControl;
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Title";
        // 
        // lblDescription
        // 
        lblDescription.Appearance.Options.UseTextOptions = true;
        lblDescription.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
        lblDescription.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
        lblDescription.Location = new Point(14, 46);
        lblDescription.Name = "lblDescription";
        lblDescription.Size = new Size(672, 16);
        lblDescription.StyleController = layoutControl;
        lblDescription.TabIndex = 1;
        lblDescription.Text = "description";
        // 
        // lblCmdCaption
        // 
        lblCmdCaption.Location = new Point(28, 365);
        lblCmdCaption.Name = "lblCmdCaption";
        lblCmdCaption.Size = new Size(152, 16);
        lblCmdCaption.StyleController = layoutControl;
        lblCmdCaption.TabIndex = 2;
        lblCmdCaption.Text = "Direct command (no LLM):";
        // 
        // btnRunDirect
        // 
        btnRunDirect.Location = new Point(184, 365);
        btnRunDirect.Name = "btnRunDirect";
        btnRunDirect.Size = new Size(488, 27);
        btnRunDirect.StyleController = layoutControl;
        btnRunDirect.TabIndex = 3;
        btnRunDirect.Text = "Run (direct)";
        // 
        // memoCommand
        // 
        memoCommand.Location = new Point(28, 178);
        memoCommand.Name = "memoCommand";
        memoCommand.Properties.ReadOnly = true;
        memoCommand.Properties.WordWrap = false;
        memoCommand.Size = new Size(644, 37);
        memoCommand.StyleController = layoutControl;
        memoCommand.TabIndex = 4;
        // 
        // memoResult
        // 
        memoResult.Location = new Point(28, 219);
        memoResult.Name = "memoResult";
        memoResult.Properties.ReadOnly = true;
        memoResult.Size = new Size(644, 142);
        memoResult.StyleController = layoutControl;
        memoResult.TabIndex = 5;
        // 
        // memoPrompt
        // 
        memoPrompt.Location = new Point(28, 450);
        memoPrompt.Name = "memoPrompt";
        memoPrompt.Size = new Size(644, 43);
        memoPrompt.StyleController = layoutControl;
        memoPrompt.TabIndex = 6;
        // 
        // btnAskAgent
        // 
        btnAskAgent.Location = new Point(501, 688);
        btnAskAgent.Name = "btnAskAgent";
        btnAskAgent.Size = new Size(171, 27);
        btnAskAgent.StyleController = layoutControl;
        btnAskAgent.TabIndex = 7;
        btnAskAgent.Text = "Ask agent";
        // 
        // memoAgent
        // 
        memoAgent.Location = new Point(28, 497);
        memoAgent.Name = "memoAgent";
        memoAgent.Properties.ReadOnly = true;
        memoAgent.Size = new Size(644, 187);
        memoAgent.StyleController = layoutControl;
        memoAgent.TabIndex = 8;
        // 
        // layoutControlGroupRoot
        // 
        layoutControlGroupRoot.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
        layoutControlGroupRoot.GroupBordersVisible = false;
        layoutControlGroupRoot.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] { layoutItemTitle, layoutItemDescription, groupPolicy, groupAgent, groupResult });
        layoutControlGroupRoot.Name = "Root";
        layoutControlGroupRoot.Size = new Size(700, 743);
        layoutControlGroupRoot.TextVisible = false;
        // 
        // layoutItemTitle
        // 
        layoutItemTitle.Control = lblTitle;
        layoutItemTitle.Location = new Point(0, 0);
        layoutItemTitle.Name = "layoutItemTitle";
        layoutItemTitle.Size = new Size(676, 32);
        layoutItemTitle.TextVisible = false;
        // 
        // layoutItemDescription
        // 
        layoutItemDescription.Control = lblDescription;
        layoutItemDescription.Location = new Point(0, 32);
        layoutItemDescription.Name = "layoutItemDescription";
        layoutItemDescription.Size = new Size(676, 20);
        layoutItemDescription.TextVisible = false;
        // 
        // groupPolicy
        // 
        groupPolicy.Location = new Point(0, 52);
        groupPolicy.Name = "groupPolicy";
        groupPolicy.Size = new Size(676, 72);
        groupPolicy.Text = "Sandbox policy";
        // 
        // groupAgent
        // 
        groupAgent.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] { layoutItemPrompt, groupAsk, layoutItemAgent });
        groupAgent.Location = new Point(0, 396);
        groupAgent.Name = "groupAgent";
        groupAgent.Size = new Size(676, 323);
        groupAgent.Text = "Ask the agent (LLM runs the same policy)";
        // 
        // layoutItemPrompt
        // 
        layoutItemPrompt.Control = memoPrompt;
        layoutItemPrompt.Location = new Point(0, 0);
        layoutItemPrompt.Name = "layoutItemPrompt";
        layoutItemPrompt.Size = new Size(648, 47);
        layoutItemPrompt.TextVisible = false;
        // 
        // groupAsk
        // 
        groupAsk.GroupBordersVisible = false;
        groupAsk.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] { emptyAsk, layoutItemAskBtn });
        groupAsk.Location = new Point(0, 238);
        groupAsk.Name = "groupAsk";
        groupAsk.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
        groupAsk.Size = new Size(648, 31);
        groupAsk.TextVisible = false;
        // 
        // emptyAsk
        // 
        emptyAsk.Location = new Point(0, 0);
        emptyAsk.Name = "emptyAsk";
        emptyAsk.Size = new Size(473, 31);
        // 
        // layoutItemAskBtn
        // 
        layoutItemAskBtn.Control = btnAskAgent;
        layoutItemAskBtn.Location = new Point(473, 0);
        layoutItemAskBtn.Name = "layoutItemAskBtn";
        layoutItemAskBtn.Size = new Size(175, 31);
        layoutItemAskBtn.TextVisible = false;
        // 
        // layoutItemAgent
        // 
        layoutItemAgent.Control = memoAgent;
        layoutItemAgent.Location = new Point(0, 47);
        layoutItemAgent.Name = "layoutItemAgent";
        layoutItemAgent.Size = new Size(648, 191);
        layoutItemAgent.TextVisible = false;
        // 
        // groupResult
        // 
        groupResult.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] { layoutItemResult, layoutItemCmdCaption, layoutItemRunBtn, layoutItemCommand });
        groupResult.Location = new Point(0, 124);
        groupResult.Name = "groupResult";
        groupResult.Size = new Size(676, 272);
        groupResult.Text = "Direct result";
        // 
        // layoutItemResult
        // 
        layoutItemResult.Control = memoResult;
        layoutItemResult.Location = new Point(0, 41);
        layoutItemResult.Name = "layoutItemResult";
        layoutItemResult.Size = new Size(648, 146);
        layoutItemResult.TextVisible = false;
        // 
        // layoutItemCmdCaption
        // 
        layoutItemCmdCaption.Control = lblCmdCaption;
        layoutItemCmdCaption.Location = new Point(0, 187);
        layoutItemCmdCaption.Name = "layoutItemCmdCaption";
        layoutItemCmdCaption.Size = new Size(156, 31);
        layoutItemCmdCaption.TextVisible = false;
        // 
        // layoutItemRunBtn
        // 
        layoutItemRunBtn.Control = btnRunDirect;
        layoutItemRunBtn.Location = new Point(156, 187);
        layoutItemRunBtn.Name = "layoutItemRunBtn";
        layoutItemRunBtn.Size = new Size(492, 31);
        layoutItemRunBtn.TextVisible = false;
        // 
        // layoutItemCommand
        // 
        layoutItemCommand.Control = memoCommand;
        layoutItemCommand.Location = new Point(0, 0);
        layoutItemCommand.Name = "layoutItemCommand";
        layoutItemCommand.Size = new Size(648, 41);
        layoutItemCommand.TextVisible = false;
        // 
        // SandboxExampleControlBase
        // 
        AutoScaleDimensions = new SizeF(7F, 16F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(layoutControl);
        Name = "SandboxExampleControlBase";
        Size = new Size(700, 743);
        ((System.ComponentModel.ISupportInitialize)layoutControl).EndInit();
        layoutControl.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)memoCommand.Properties).EndInit();
        ((System.ComponentModel.ISupportInitialize)memoResult.Properties).EndInit();
        ((System.ComponentModel.ISupportInitialize)memoPrompt.Properties).EndInit();
        ((System.ComponentModel.ISupportInitialize)memoAgent.Properties).EndInit();
        ((System.ComponentModel.ISupportInitialize)layoutControlGroupRoot).EndInit();
        ((System.ComponentModel.ISupportInitialize)layoutItemTitle).EndInit();
        ((System.ComponentModel.ISupportInitialize)layoutItemDescription).EndInit();
        ((System.ComponentModel.ISupportInitialize)groupPolicy).EndInit();
        ((System.ComponentModel.ISupportInitialize)groupAgent).EndInit();
        ((System.ComponentModel.ISupportInitialize)layoutItemPrompt).EndInit();
        ((System.ComponentModel.ISupportInitialize)groupAsk).EndInit();
        ((System.ComponentModel.ISupportInitialize)emptyAsk).EndInit();
        ((System.ComponentModel.ISupportInitialize)layoutItemAskBtn).EndInit();
        ((System.ComponentModel.ISupportInitialize)layoutItemAgent).EndInit();
        ((System.ComponentModel.ISupportInitialize)groupResult).EndInit();
        ((System.ComponentModel.ISupportInitialize)layoutItemResult).EndInit();
        ((System.ComponentModel.ISupportInitialize)layoutItemCmdCaption).EndInit();
        ((System.ComponentModel.ISupportInitialize)layoutItemRunBtn).EndInit();
        ((System.ComponentModel.ISupportInitialize)layoutItemCommand).EndInit();
        ResumeLayout(false);
    }
}
