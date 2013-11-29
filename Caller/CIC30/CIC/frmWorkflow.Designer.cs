namespace CIC
{
    partial class frmWorkflow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmWorkflow));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.workflow_combobox = new System.Windows.Forms.ComboBox();
            this.workflow_label = new System.Windows.Forms.Label();
            this.login_button = new System.Windows.Forms.Button();
            this.cancel_button = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.workflow_combobox);
            this.groupBox1.Controls.Add(this.workflow_label);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(296, 75);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Connection";
            // 
            // workflow_combobox
            // 
            this.workflow_combobox.FormattingEnabled = true;
            this.workflow_combobox.Location = new System.Drawing.Point(83, 32);
            this.workflow_combobox.Name = "workflow_combobox";
            this.workflow_combobox.Size = new System.Drawing.Size(197, 21);
            this.workflow_combobox.TabIndex = 1;
            // 
            // workflow_label
            // 
            this.workflow_label.AutoSize = true;
            this.workflow_label.Location = new System.Drawing.Point(22, 35);
            this.workflow_label.Name = "workflow_label";
            this.workflow_label.Size = new System.Drawing.Size(55, 13);
            this.workflow_label.TabIndex = 0;
            this.workflow_label.Text = "Workflow:";
            // 
            // login_button
            // 
            this.login_button.Location = new System.Drawing.Point(75, 93);
            this.login_button.Name = "login_button";
            this.login_button.Size = new System.Drawing.Size(75, 23);
            this.login_button.TabIndex = 1;
            this.login_button.Text = "Login";
            this.login_button.UseVisualStyleBackColor = true;
            this.login_button.Click += new System.EventHandler(this.login_button_Click);
            // 
            // cancel_button
            // 
            this.cancel_button.Location = new System.Drawing.Point(156, 93);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(75, 23);
            this.cancel_button.TabIndex = 2;
            this.cancel_button.Text = "Cancel";
            this.cancel_button.UseVisualStyleBackColor = true;
            this.cancel_button.Click += new System.EventHandler(this.cancel_button_Click);
            // 
            // frmWorkflow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(328, 126);
            this.ControlBox = false;
            this.Controls.Add(this.cancel_button);
            this.Controls.Add(this.login_button);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmWorkflow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Logon to Workflow";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmWorkflow_FormClosed);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox workflow_combobox;
        private System.Windows.Forms.Label workflow_label;
        private System.Windows.Forms.Button login_button;
        private System.Windows.Forms.Button cancel_button;
    }
}