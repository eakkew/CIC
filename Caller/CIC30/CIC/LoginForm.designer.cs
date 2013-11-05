namespace CIC
{
  partial class frmICStation
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmICStation));
        this.label1 = new System.Windows.Forms.Label();
        this.label2 = new System.Windows.Forms.Label();
        this.PasswordTextBox = new System.Windows.Forms.TextBox();
        this.label3 = new System.Windows.Forms.Label();
        this.label4 = new System.Windows.Forms.Label();
        this.WindowsAuthenticationCheckBox = new System.Windows.Forms.CheckBox();
        this.UserIdTextBox = new System.Windows.Forms.TextBox();
        this.PersistentCheckBox = new System.Windows.Forms.CheckBox();
        this.StationIdTextBox = new System.Windows.Forms.TextBox();
        this.label6 = new System.Windows.Forms.Label();
        this.StationTypeComboBox = new System.Windows.Forms.ComboBox();
        this.label5 = new System.Windows.Forms.Label();
        this.PhoneNumberTextBox = new System.Windows.Forms.TextBox();
        this.ServerTextBox = new System.Windows.Forms.TextBox();
        this.CancelLoginButton = new System.Windows.Forms.Button();
        this.LoginButton = new System.Windows.Forms.Button();
        this.lblLogInStatusMsg = new System.Windows.Forms.Label();
        this.LogInPreviewStatus = new ININ.Client.Common.UI.WaitLoader();
        this.chkSavepassword = new System.Windows.Forms.CheckBox();
        this.SuspendLayout();
        // 
        // label1
        // 
        resources.ApplyResources(this.label1, "label1");
        this.label1.BackColor = System.Drawing.Color.Transparent;
        this.label1.Name = "label1";
        // 
        // label2
        // 
        resources.ApplyResources(this.label2, "label2");
        this.label2.BackColor = System.Drawing.Color.Transparent;
        this.label2.Name = "label2";
        // 
        // PasswordTextBox
        // 
        resources.ApplyResources(this.PasswordTextBox, "PasswordTextBox");
        this.PasswordTextBox.Name = "PasswordTextBox";
        this.PasswordTextBox.TextChanged += new System.EventHandler(this.PasswordTextBox_TextChanged);
        // 
        // label3
        // 
        resources.ApplyResources(this.label3, "label3");
        this.label3.BackColor = System.Drawing.Color.Transparent;
        this.label3.Name = "label3";
        // 
        // label4
        // 
        resources.ApplyResources(this.label4, "label4");
        this.label4.BackColor = System.Drawing.Color.Transparent;
        this.label4.Name = "label4";
        // 
        // WindowsAuthenticationCheckBox
        // 
        resources.ApplyResources(this.WindowsAuthenticationCheckBox, "WindowsAuthenticationCheckBox");
        this.WindowsAuthenticationCheckBox.BackColor = System.Drawing.Color.Transparent;
        this.WindowsAuthenticationCheckBox.Checked = global::CIC.Properties.Settings.Default.UseWindowsAuthentication;
        this.WindowsAuthenticationCheckBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::CIC.Properties.Settings.Default, "UseWindowsAuthentication", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
        this.WindowsAuthenticationCheckBox.Name = "WindowsAuthenticationCheckBox";
        this.WindowsAuthenticationCheckBox.UseVisualStyleBackColor = false;
        this.WindowsAuthenticationCheckBox.CheckedChanged += new System.EventHandler(this.WindowsAuthenticationCheckBox_CheckedChanged);
        // 
        // UserIdTextBox
        // 
        this.UserIdTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::CIC.Properties.Settings.Default, "UserId", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
        resources.ApplyResources(this.UserIdTextBox, "UserIdTextBox");
        this.UserIdTextBox.Name = "UserIdTextBox";
        this.UserIdTextBox.Text = global::CIC.Properties.Settings.Default.UserId;
        this.UserIdTextBox.TextChanged += new System.EventHandler(this.UserIdTextBox_TextChanged);
        // 
        // PersistentCheckBox
        // 
        resources.ApplyResources(this.PersistentCheckBox, "PersistentCheckBox");
        this.PersistentCheckBox.BackColor = System.Drawing.Color.Transparent;
        this.PersistentCheckBox.Checked = global::CIC.Properties.Settings.Default.UsePersistentConnection;
        this.PersistentCheckBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::CIC.Properties.Settings.Default, "UsePersistentConnection", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
        this.PersistentCheckBox.Name = "PersistentCheckBox";
        this.PersistentCheckBox.UseVisualStyleBackColor = false;
        // 
        // StationIdTextBox
        // 
        this.StationIdTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::CIC.Properties.Settings.Default, "StationId", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
        resources.ApplyResources(this.StationIdTextBox, "StationIdTextBox");
        this.StationIdTextBox.Name = "StationIdTextBox";
        this.StationIdTextBox.Text = global::CIC.Properties.Settings.Default.StationId;
        // 
        // label6
        // 
        resources.ApplyResources(this.label6, "label6");
        this.label6.BackColor = System.Drawing.Color.Transparent;
        this.label6.Name = "label6";
        // 
        // StationTypeComboBox
        // 
        this.StationTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.StationTypeComboBox.FormattingEnabled = true;
        resources.ApplyResources(this.StationTypeComboBox, "StationTypeComboBox");
        this.StationTypeComboBox.Name = "StationTypeComboBox";
        this.StationTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.StationTypeComboBox_SelectedIndexChanged);
        // 
        // label5
        // 
        resources.ApplyResources(this.label5, "label5");
        this.label5.BackColor = System.Drawing.Color.Transparent;
        this.label5.Name = "label5";
        // 
        // PhoneNumberTextBox
        // 
        this.PhoneNumberTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::CIC.Properties.Settings.Default, "Phone", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
        resources.ApplyResources(this.PhoneNumberTextBox, "PhoneNumberTextBox");
        this.PhoneNumberTextBox.Name = "PhoneNumberTextBox";
        this.PhoneNumberTextBox.Text = global::CIC.Properties.Settings.Default.Phone;
        this.PhoneNumberTextBox.TextChanged += new System.EventHandler(this.PhoneNumberTextBox_TextChanged);
        // 
        // ServerTextBox
        // 
        this.ServerTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::CIC.Properties.Settings.Default, "Server", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
        resources.ApplyResources(this.ServerTextBox, "ServerTextBox");
        this.ServerTextBox.Name = "ServerTextBox";
        this.ServerTextBox.Text = global::CIC.Properties.Settings.Default.Server;
        this.ServerTextBox.TextChanged += new System.EventHandler(this.ServerTextBox_TextChanged);
        // 
        // CancelLoginButton
        // 
        this.CancelLoginButton.BackgroundImage = global::CIC.Properties.Resources.bluebgBar;
        this.CancelLoginButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.CancelLoginButton.Image = global::CIC.Properties.Resources.flag_red;
        resources.ApplyResources(this.CancelLoginButton, "CancelLoginButton");
        this.CancelLoginButton.Name = "CancelLoginButton";
        this.CancelLoginButton.UseVisualStyleBackColor = true;
        this.CancelLoginButton.Click += new System.EventHandler(this.CancelLoginButton_Click);
        // 
        // LoginButton
        // 
        this.LoginButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.LoginButton.Image = global::CIC.Properties.Resources.flag_green;
        resources.ApplyResources(this.LoginButton, "LoginButton");
        this.LoginButton.Name = "LoginButton";
        this.LoginButton.UseVisualStyleBackColor = true;
        this.LoginButton.Click += new System.EventHandler(this.LoginButton_Click);
        // 
        // lblLogInStatusMsg
        // 
        this.lblLogInStatusMsg.BackColor = System.Drawing.Color.Transparent;
        resources.ApplyResources(this.lblLogInStatusMsg, "lblLogInStatusMsg");
        this.lblLogInStatusMsg.Name = "lblLogInStatusMsg";
        // 
        // LogInPreviewStatus
        // 
        this.LogInPreviewStatus.AnimationInterval = 100;
        this.LogInPreviewStatus.BackColor = System.Drawing.Color.Transparent;
        this.LogInPreviewStatus.CircleColor = System.Drawing.Color.LimeGreen;
        resources.ApplyResources(this.LogInPreviewStatus, "LogInPreviewStatus");
        this.LogInPreviewStatus.Name = "LogInPreviewStatus";
        // 
        // chkSavepassword
        // 
        resources.ApplyResources(this.chkSavepassword, "chkSavepassword");
        this.chkSavepassword.BackColor = System.Drawing.Color.Transparent;
        this.chkSavepassword.Checked = true;
        this.chkSavepassword.CheckState = System.Windows.Forms.CheckState.Checked;
        this.chkSavepassword.Name = "chkSavepassword";
        this.chkSavepassword.UseVisualStyleBackColor = false;
        this.chkSavepassword.CheckedChanged += new System.EventHandler(this.chkSavepassword_CheckedChanged);
        // 
        // frmICStation
        // 
        resources.ApplyResources(this, "$this");
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.PersistentCheckBox);
        this.Controls.Add(this.WindowsAuthenticationCheckBox);
        this.Controls.Add(this.StationIdTextBox);
        this.Controls.Add(this.label1);
        this.Controls.Add(this.label6);
        this.Controls.Add(this.chkSavepassword);
        this.Controls.Add(this.StationTypeComboBox);
        this.Controls.Add(this.label5);
        this.Controls.Add(this.UserIdTextBox);
        this.Controls.Add(this.PhoneNumberTextBox);
        this.Controls.Add(this.LogInPreviewStatus);
        this.Controls.Add(this.label4);
        this.Controls.Add(this.label2);
        this.Controls.Add(this.lblLogInStatusMsg);
        this.Controls.Add(this.PasswordTextBox);
        this.Controls.Add(this.CancelLoginButton);
        this.Controls.Add(this.LoginButton);
        this.Controls.Add(this.ServerTextBox);
        this.Controls.Add(this.label3);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.KeyPreview = true;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "frmICStation";
        this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
        this.Load += new System.EventHandler(this.LoginForm_Load);
        this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LoginForm_FormClosing);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox UserIdTextBox;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox PasswordTextBox;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox ServerTextBox;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox PhoneNumberTextBox;
    private System.Windows.Forms.CheckBox WindowsAuthenticationCheckBox;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.TextBox StationIdTextBox;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.ComboBox StationTypeComboBox;
    private System.Windows.Forms.CheckBox PersistentCheckBox;
    private System.Windows.Forms.Button CancelLoginButton;
    private System.Windows.Forms.Button LoginButton;
    private System.Windows.Forms.Label lblLogInStatusMsg;
    private ININ.Client.Common.UI.WaitLoader LogInPreviewStatus;
    private System.Windows.Forms.CheckBox chkSavepassword;
  }
}