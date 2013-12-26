namespace CIC
{
    partial class frmTransfer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTransfer));
            this.call_button = new System.Windows.Forms.Button();
            this.transfer_button = new System.Windows.Forms.Button();
            this.cancel_button = new System.Windows.Forms.Button();
            this.ext_number_panel = new System.Windows.Forms.Label();
            this.ext_number_box = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // call_button
            // 
            this.call_button.Location = new System.Drawing.Point(12, 58);
            this.call_button.Name = "call_button";
            this.call_button.Size = new System.Drawing.Size(75, 23);
            this.call_button.TabIndex = 0;
            this.call_button.Text = "&Call";
            this.call_button.UseVisualStyleBackColor = true;
            this.call_button.Click += new System.EventHandler(this.call_button_Click);
            // 
            // transfer_button
            // 
            this.transfer_button.Location = new System.Drawing.Point(93, 57);
            this.transfer_button.Name = "transfer_button";
            this.transfer_button.Size = new System.Drawing.Size(75, 23);
            this.transfer_button.TabIndex = 1;
            this.transfer_button.Text = "&Transfer";
            this.transfer_button.UseVisualStyleBackColor = true;
            this.transfer_button.Click += new System.EventHandler(this.transfer_button_Click);
            // 
            // cancel_button
            // 
            this.cancel_button.Location = new System.Drawing.Point(175, 57);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(75, 23);
            this.cancel_button.TabIndex = 2;
            this.cancel_button.Text = "C&ancel";
            this.cancel_button.UseVisualStyleBackColor = true;
            this.cancel_button.Click += new System.EventHandler(this.cancel_button_Click);
            // 
            // ext_number_panel
            // 
            this.ext_number_panel.AutoSize = true;
            this.ext_number_panel.Location = new System.Drawing.Point(12, 25);
            this.ext_number_panel.Name = "ext_number_panel";
            this.ext_number_panel.Size = new System.Drawing.Size(94, 13);
            this.ext_number_panel.TabIndex = 3;
            this.ext_number_panel.Text = "Extension number:";
            // 
            // ext_number_box
            // 
            this.ext_number_box.Location = new System.Drawing.Point(112, 18);
            this.ext_number_box.MaxLength = 255;
            this.ext_number_box.Name = "ext_number_box";
            this.ext_number_box.Size = new System.Drawing.Size(100, 20);
            this.ext_number_box.TabIndex = 4;
            this.ext_number_box.TextChanged += new System.EventHandler(this.ext_number_box_TextChanged);
            // 
            // frmTransfer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(274, 92);
            this.ControlBox = false;
            this.Controls.Add(this.ext_number_box);
            this.Controls.Add(this.ext_number_panel);
            this.Controls.Add(this.cancel_button);
            this.Controls.Add(this.transfer_button);
            this.Controls.Add(this.call_button);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmTransfer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Transfer a Call";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.frmTransfer_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button call_button;
        private System.Windows.Forms.Button transfer_button;
        private System.Windows.Forms.Button cancel_button;
        private System.Windows.Forms.Label ext_number_panel;
        private System.Windows.Forms.TextBox ext_number_box;
    }
}