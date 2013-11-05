namespace CIC
{
  partial class UserStatusGridView
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserStatusGridView));
        this.UserStatusDataGridView = new System.Windows.Forms.DataGridView();
        this.AvailabilityColumn = new System.Windows.Forms.DataGridViewImageColumn();
        this.UserColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.StatusColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.ctnUserMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
        this.callToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.mobiletoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.mailToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.dataGridViewImageColumn1 = new System.Windows.Forms.DataGridViewImageColumn();
        this.IconsImageList = new System.Windows.Forms.ImageList(this.components);
        this.tooltipUsrExtension = new System.Windows.Forms.ToolTip(this.components);
        this.linkLabel1 = new System.Windows.Forms.LinkLabel();
        ((System.ComponentModel.ISupportInitialize)(this.UserStatusDataGridView)).BeginInit();
        this.ctnUserMenu.SuspendLayout();
        this.SuspendLayout();
        // 
        // UserStatusDataGridView
        // 
        this.UserStatusDataGridView.AllowUserToAddRows = false;
        this.UserStatusDataGridView.AllowUserToDeleteRows = false;
        this.UserStatusDataGridView.AllowUserToResizeColumns = false;
        this.UserStatusDataGridView.AllowUserToResizeRows = false;
        dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
        this.UserStatusDataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
        this.UserStatusDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this.UserStatusDataGridView.BackgroundColor = System.Drawing.Color.White;
        this.UserStatusDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.UserStatusDataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
        this.UserStatusDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.UserStatusDataGridView.ColumnHeadersVisible = false;
        this.UserStatusDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.AvailabilityColumn,
            this.UserColumn,
            this.StatusColumn});
        this.UserStatusDataGridView.ContextMenuStrip = this.ctnUserMenu;
        this.UserStatusDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
        this.UserStatusDataGridView.GridColor = System.Drawing.Color.Cornsilk;
        this.UserStatusDataGridView.Location = new System.Drawing.Point(0, 0);
        this.UserStatusDataGridView.MultiSelect = false;
        this.UserStatusDataGridView.Name = "UserStatusDataGridView";
        this.UserStatusDataGridView.ReadOnly = true;
        this.UserStatusDataGridView.RowHeadersVisible = false;
        dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
        this.UserStatusDataGridView.RowsDefaultCellStyle = dataGridViewCellStyle2;
        this.UserStatusDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.UserStatusDataGridView.Size = new System.Drawing.Size(287, 211);
        this.UserStatusDataGridView.TabIndex = 0;
        this.UserStatusDataGridView.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.UserStatusDataGridView_RowEnter);
        this.UserStatusDataGridView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.UserStatusDataGridView_MouseMove);
        this.UserStatusDataGridView.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.UserStatusDataGridView_CellMouseDown);
        this.UserStatusDataGridView.SelectionChanged += new System.EventHandler(this.UserStatusDataGridView_SelectionChanged);
        this.UserStatusDataGridView.MouseEnter += new System.EventHandler(this.UserStatusDataGridView_MouseEnter);
        this.UserStatusDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.UserStatusDataGridView_CellContentClick);
        // 
        // AvailabilityColumn
        // 
        this.AvailabilityColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
        this.AvailabilityColumn.HeaderText = "Availability";
        this.AvailabilityColumn.Name = "AvailabilityColumn";
        this.AvailabilityColumn.ReadOnly = true;
        this.AvailabilityColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
        this.AvailabilityColumn.Width = 30;
        // 
        // UserColumn
        // 
        this.UserColumn.HeaderText = "User";
        this.UserColumn.Name = "UserColumn";
        this.UserColumn.ReadOnly = true;
        // 
        // StatusColumn
        // 
        this.StatusColumn.HeaderText = "Status";
        this.StatusColumn.Name = "StatusColumn";
        this.StatusColumn.ReadOnly = true;
        // 
        // ctnUserMenu
        // 
        this.ctnUserMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.callToolStripMenuItem,
            this.mobiletoolStripMenuItem,
            this.mailToToolStripMenuItem});
        this.ctnUserMenu.Name = "ctnUserMenu";
        this.ctnUserMenu.Size = new System.Drawing.Size(121, 70);
        // 
        // callToolStripMenuItem
        // 
        this.callToolStripMenuItem.Image = global::CIC.Properties.Resources.HmPhone1;
        this.callToolStripMenuItem.Name = "callToolStripMenuItem";
        this.callToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
        this.callToolStripMenuItem.Text = "Phone :";
        this.callToolStripMenuItem.Click += new System.EventHandler(this.callToolStripMenuItem_Click);
        // 
        // mobiletoolStripMenuItem
        // 
        this.mobiletoolStripMenuItem.Image = global::CIC.Properties.Resources.MobPhone;
        this.mobiletoolStripMenuItem.Name = "mobiletoolStripMenuItem";
        this.mobiletoolStripMenuItem.Size = new System.Drawing.Size(120, 22);
        this.mobiletoolStripMenuItem.Text = "Mobile : ";
        this.mobiletoolStripMenuItem.Click += new System.EventHandler(this.mobiletoolStripMenuItem_Click);
        // 
        // mailToToolStripMenuItem
        // 
        this.mailToToolStripMenuItem.Image = global::CIC.Properties.Resources.Exchange;
        this.mailToToolStripMenuItem.Name = "mailToToolStripMenuItem";
        this.mailToToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
        this.mailToToolStripMenuItem.Text = "Mail : ";
        this.mailToToolStripMenuItem.Click += new System.EventHandler(this.mailToToolStripMenuItem_Click);
        // 
        // dataGridViewImageColumn1
        // 
        this.dataGridViewImageColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
        this.dataGridViewImageColumn1.HeaderText = "Availability";
        this.dataGridViewImageColumn1.Image = ((System.Drawing.Image)(resources.GetObject("dataGridViewImageColumn1.Image")));
        this.dataGridViewImageColumn1.Name = "dataGridViewImageColumn1";
        this.dataGridViewImageColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
        this.dataGridViewImageColumn1.Width = 30;
        // 
        // IconsImageList
        // 
        this.IconsImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("IconsImageList.ImageStream")));
        this.IconsImageList.TransparentColor = System.Drawing.Color.Transparent;
        this.IconsImageList.Images.SetKeyName(0, "Offline");
        this.IconsImageList.Images.SetKeyName(1, "NotAvailable");
        this.IconsImageList.Images.SetKeyName(2, "Available");
        this.IconsImageList.Images.SetKeyName(3, "OnPhone");
        // 
        // linkLabel1
        // 
        this.linkLabel1.AutoSize = true;
        this.linkLabel1.Location = new System.Drawing.Point(184, 161);
        this.linkLabel1.Name = "linkLabel1";
        this.linkLabel1.Size = new System.Drawing.Size(55, 13);
        this.linkLabel1.TabIndex = 2;
        this.linkLabel1.TabStop = true;
        this.linkLabel1.Text = "linkLabel1";
        // 
        // UserStatusGridView
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.UserStatusDataGridView);
        this.Controls.Add(this.linkLabel1);
        this.Name = "UserStatusGridView";
        this.Size = new System.Drawing.Size(287, 211);
        ((System.ComponentModel.ISupportInitialize)(this.UserStatusDataGridView)).EndInit();
        this.ctnUserMenu.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.DataGridView UserStatusDataGridView;
    private System.Windows.Forms.DataGridViewImageColumn dataGridViewImageColumn1;
    private System.Windows.Forms.ImageList IconsImageList;
    private System.Windows.Forms.DataGridViewImageColumn AvailabilityColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn UserColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn StatusColumn;
    private System.Windows.Forms.ToolTip tooltipUsrExtension;
    private System.Windows.Forms.ContextMenuStrip ctnUserMenu;
    private System.Windows.Forms.ToolStripMenuItem callToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem mailToToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem mobiletoolStripMenuItem;
    private System.Windows.Forms.LinkLabel linkLabel1;
  }
}
