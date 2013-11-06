namespace CIC
{
    partial class frmMailDocView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMailDocView));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.StatusToolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.MailsplitContainer = new System.Windows.Forms.SplitContainer();
            this.grvMailingList = new System.Windows.Forms.DataGridView();
            this.rtfBody = new System.Windows.Forms.RichTextBox();
            this.BtnToolStrip = new System.Windows.Forms.ToolStrip();
            this.btnSend = new System.Windows.Forms.ToolStripButton();
            this.btnNew = new System.Windows.Forms.ToolStripButton();
            this.btnForward = new System.Windows.Forms.ToolStripButton();
            this.btnReply = new System.Windows.Forms.ToolStripButton();
            this.btnClose = new System.Windows.Forms.ToolStripButton();
            this.FromToolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.txtSender = new System.Windows.Forms.ToolStripTextBox();
            this.ToToolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.txtTo = new System.Windows.Forms.ToolStripTextBox();
            this.CcToolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.txtCC = new System.Windows.Forms.ToolStripTextBox();
            this.SubjectToolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.txtSubject = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.StatusToolStrip.SuspendLayout();
            this.MailsplitContainer.Panel1.SuspendLayout();
            this.MailsplitContainer.Panel2.SuspendLayout();
            this.MailsplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grvMailingList)).BeginInit();
            this.BtnToolStrip.SuspendLayout();
            this.FromToolStrip.SuspendLayout();
            this.ToToolStrip.SuspendLayout();
            this.CcToolStrip.SuspendLayout();
            this.SubjectToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.StatusToolStrip);
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.MailsplitContainer);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(893, 287);
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 2);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(893, 437);
            this.toolStripContainer1.TabIndex = 0;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.BackgroundImage = global::CIC.Properties.Resources.bluebgBar;
            this.toolStripContainer1.TopToolStripPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.BtnToolStrip);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.FromToolStrip);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.ToToolStrip);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.CcToolStrip);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.SubjectToolStrip);
            // 
            // StatusToolStrip
            // 
            this.StatusToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.StatusToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel3});
            this.StatusToolStrip.Location = new System.Drawing.Point(3, 0);
            this.StatusToolStrip.Name = "StatusToolStrip";
            this.StatusToolStrip.Size = new System.Drawing.Size(114, 25);
            this.StatusToolStrip.TabIndex = 0;
            this.StatusToolStrip.Text = "toolStrip6";
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Image = global::CIC.Properties.Resources.information;
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(102, 22);
            this.toolStripLabel3.Text = "toolStripLabel3";
            // 
            // MailsplitContainer
            // 
            this.MailsplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MailsplitContainer.Location = new System.Drawing.Point(0, 0);
            this.MailsplitContainer.Name = "MailsplitContainer";
            // 
            // MailsplitContainer.Panel1
            // 
            this.MailsplitContainer.Panel1.BackColor = System.Drawing.Color.Transparent;
            this.MailsplitContainer.Panel1.Controls.Add(this.grvMailingList);
            this.MailsplitContainer.Panel1Collapsed = true;
            // 
            // MailsplitContainer.Panel2
            // 
            this.MailsplitContainer.Panel2.BackColor = System.Drawing.Color.Transparent;
            this.MailsplitContainer.Panel2.Controls.Add(this.rtfBody);
            this.MailsplitContainer.Size = new System.Drawing.Size(893, 287);
            this.MailsplitContainer.SplitterDistance = 208;
            this.MailsplitContainer.TabIndex = 0;
            // 
            // grvMailingList
            // 
            this.grvMailingList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grvMailingList.Dock = System.Windows.Forms.DockStyle.Top;
            this.grvMailingList.Location = new System.Drawing.Point(0, 0);
            this.grvMailingList.Name = "grvMailingList";
            this.grvMailingList.Size = new System.Drawing.Size(208, 287);
            this.grvMailingList.TabIndex = 0;
            this.grvMailingList.SelectionChanged += new System.EventHandler(this.grvMailingList_SelectionChanged);
            // 
            // rtfBody
            // 
            this.rtfBody.BackColor = System.Drawing.SystemColors.Info;
            this.rtfBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtfBody.Location = new System.Drawing.Point(0, 0);
            this.rtfBody.Name = "rtfBody";
            this.rtfBody.Size = new System.Drawing.Size(893, 287);
            this.rtfBody.TabIndex = 0;
            this.rtfBody.Text = "";
            // 
            // BtnToolStrip
            // 
            this.BtnToolStrip.BackColor = System.Drawing.Color.Transparent;
            this.BtnToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.BtnToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSend,
            this.btnNew,
            this.btnForward,
            this.btnReply,
            this.btnClose});
            this.BtnToolStrip.Location = new System.Drawing.Point(3, 0);
            this.BtnToolStrip.Name = "BtnToolStrip";
            this.BtnToolStrip.Size = new System.Drawing.Size(298, 25);
            this.BtnToolStrip.TabIndex = 4;
            // 
            // btnSend
            // 
            this.btnSend.Image = global::CIC.Properties.Resources.Pager1;
            this.btnSend.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(53, 22);
            this.btnSend.Text = "Send";
            // 
            // btnNew
            // 
            this.btnNew.Image = global::CIC.Properties.Resources.pencil;
            this.btnNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(51, 22);
            this.btnNew.Text = "New";
            // 
            // btnForward
            // 
            this.btnForward.Image = global::CIC.Properties.Resources.OutOfOffice;
            this.btnForward.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnForward.Name = "btnForward";
            this.btnForward.Size = new System.Drawing.Size(70, 22);
            this.btnForward.Text = "Forward";
            // 
            // btnReply
            // 
            this.btnReply.Image = global::CIC.Properties.Resources.OutOfTown;
            this.btnReply.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnReply.Name = "btnReply";
            this.btnReply.Size = new System.Drawing.Size(56, 22);
            this.btnReply.Text = "Reply";
            // 
            // btnClose
            // 
            this.btnClose.Image = global::CIC.Properties.Resources.Unlock;
            this.btnClose.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(56, 22);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // FromToolStrip
            // 
            this.FromToolStrip.BackColor = System.Drawing.Color.Transparent;
            this.FromToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.FromToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.txtSender});
            this.FromToolStrip.Location = new System.Drawing.Point(3, 25);
            this.FromToolStrip.Name = "FromToolStrip";
            this.FromToolStrip.Size = new System.Drawing.Size(352, 25);
            this.FromToolStrip.TabIndex = 1;
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.AutoSize = false;
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(38, 22);
            this.toolStripButton1.Text = "From";
            // 
            // txtSender
            // 
            this.txtSender.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.txtSender.Name = "txtSender";
            this.txtSender.Size = new System.Drawing.Size(300, 25);
            // 
            // ToToolStrip
            // 
            this.ToToolStrip.BackColor = System.Drawing.Color.Transparent;
            this.ToToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.ToToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton3,
            this.txtTo});
            this.ToToolStrip.Location = new System.Drawing.Point(3, 50);
            this.ToToolStrip.Name = "ToToolStrip";
            this.ToToolStrip.Size = new System.Drawing.Size(352, 25);
            this.ToToolStrip.TabIndex = 2;
            this.ToToolStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.ToToolStrip_ItemClicked);
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.AutoSize = false;
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton3.Image")));
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(38, 22);
            this.toolStripButton3.Text = "To";
            // 
            // txtTo
            // 
            this.txtTo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.txtTo.Name = "txtTo";
            this.txtTo.Size = new System.Drawing.Size(300, 25);
            // 
            // CcToolStrip
            // 
            this.CcToolStrip.BackColor = System.Drawing.Color.Transparent;
            this.CcToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.CcToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.txtCC});
            this.CcToolStrip.Location = new System.Drawing.Point(3, 75);
            this.CcToolStrip.Name = "CcToolStrip";
            this.CcToolStrip.Size = new System.Drawing.Size(352, 25);
            this.CcToolStrip.TabIndex = 3;
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.AutoSize = false;
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(38, 22);
            this.toolStripLabel1.Text = "Cc";
            // 
            // txtCC
            // 
            this.txtCC.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.txtCC.Name = "txtCC";
            this.txtCC.Size = new System.Drawing.Size(300, 25);
            // 
            // SubjectToolStrip
            // 
            this.SubjectToolStrip.BackColor = System.Drawing.Color.Transparent;
            this.SubjectToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.SubjectToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel2,
            this.txtSubject});
            this.SubjectToolStrip.Location = new System.Drawing.Point(3, 100);
            this.SubjectToolStrip.Name = "SubjectToolStrip";
            this.SubjectToolStrip.Size = new System.Drawing.Size(560, 25);
            this.SubjectToolStrip.TabIndex = 5;
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(46, 22);
            this.toolStripLabel2.Text = "Subject";
            // 
            // txtSubject
            // 
            this.txtSubject.AcceptsTab = true;
            this.txtSubject.BackColor = System.Drawing.SystemColors.Info;
            this.txtSubject.Name = "txtSubject";
            this.txtSubject.Size = new System.Drawing.Size(500, 25);
            // 
            // frmMailDocView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(891, 436);
            this.Controls.Add(this.toolStripContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmMailDocView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Email Viewer";
            this.TopMost = true;
            this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.StatusToolStrip.ResumeLayout(false);
            this.StatusToolStrip.PerformLayout();
            this.MailsplitContainer.Panel1.ResumeLayout(false);
            this.MailsplitContainer.Panel2.ResumeLayout(false);
            this.MailsplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grvMailingList)).EndInit();
            this.BtnToolStrip.ResumeLayout(false);
            this.BtnToolStrip.PerformLayout();
            this.FromToolStrip.ResumeLayout(false);
            this.FromToolStrip.PerformLayout();
            this.ToToolStrip.ResumeLayout(false);
            this.ToToolStrip.PerformLayout();
            this.CcToolStrip.ResumeLayout(false);
            this.CcToolStrip.PerformLayout();
            this.SubjectToolStrip.ResumeLayout(false);
            this.SubjectToolStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStrip FromToolStrip;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripTextBox txtSender;
        private System.Windows.Forms.ToolStrip ToToolStrip;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.ToolStripTextBox txtTo;
        private System.Windows.Forms.ToolStrip CcToolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripTextBox txtCC;
        private System.Windows.Forms.ToolStrip BtnToolStrip;
        private System.Windows.Forms.ToolStripButton btnSend;
        private System.Windows.Forms.ToolStripButton btnNew;
        private System.Windows.Forms.ToolStripButton btnForward;
        private System.Windows.Forms.ToolStripButton btnReply;
        private System.Windows.Forms.ToolStripButton btnClose;
        private System.Windows.Forms.ToolStrip SubjectToolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripTextBox txtSubject;
        private System.Windows.Forms.ToolStrip StatusToolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.SplitContainer MailsplitContainer;
        private System.Windows.Forms.DataGridView grvMailingList;
        private System.Windows.Forms.RichTextBox rtfBody;
    }
}