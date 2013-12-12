namespace CIC
{
    partial class frmConference
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmConference));
            this.ext_number_box = new System.Windows.Forms.TextBox();
            this.ext_number_panel = new System.Windows.Forms.Label();
            this.cancel_button = new System.Windows.Forms.Button();
            this.conference_button = new System.Windows.Forms.Button();
            this.call_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ext_number_box
            // 
            this.ext_number_box.Location = new System.Drawing.Point(118, 15);
            this.ext_number_box.MaxLength = 255;
            this.ext_number_box.Name = "ext_number_box";
            this.ext_number_box.Size = new System.Drawing.Size(100, 20);
            this.ext_number_box.TabIndex = 9;
            this.ext_number_box.TextChanged += new System.EventHandler(this.ext_number_box_TextChanged);
            // 
            // ext_number_panel
            // 
            this.ext_number_panel.AutoSize = true;
            this.ext_number_panel.Location = new System.Drawing.Point(18, 22);
            this.ext_number_panel.Name = "ext_number_panel";
            this.ext_number_panel.Size = new System.Drawing.Size(94, 13);
            this.ext_number_panel.TabIndex = 8;
            this.ext_number_panel.Text = "Extension number:";
            // 
            // cancel_button
            // 
            this.cancel_button.Location = new System.Drawing.Point(181, 54);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(75, 23);
            this.cancel_button.TabIndex = 7;
            this.cancel_button.Text = "C&ancel";
            this.cancel_button.UseVisualStyleBackColor = true;
            this.cancel_button.Click += new System.EventHandler(this.cancel_button_Click);
            // 
            // conference_button
            // 
            this.conference_button.Location = new System.Drawing.Point(99, 54);
            this.conference_button.Name = "conference_button";
            this.conference_button.Size = new System.Drawing.Size(75, 23);
            this.conference_button.TabIndex = 6;
            this.conference_button.Text = "C&onference";
            this.conference_button.UseVisualStyleBackColor = true;
            this.conference_button.Click += new System.EventHandler(this.conference_button_Click);
            // 
            // call_button
            // 
            this.call_button.Location = new System.Drawing.Point(18, 55);
            this.call_button.Name = "call_button";
            this.call_button.Size = new System.Drawing.Size(75, 23);
            this.call_button.TabIndex = 5;
            this.call_button.Text = "&Call";
            this.call_button.UseVisualStyleBackColor = true;
            this.call_button.Click += new System.EventHandler(this.call_button_Click);
            // 
            // frmConference
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(274, 92);
            this.ControlBox = false;
            this.Controls.Add(this.ext_number_box);
            this.Controls.Add(this.ext_number_panel);
            this.Controls.Add(this.cancel_button);
            this.Controls.Add(this.conference_button);
            this.Controls.Add(this.call_button);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmConference";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Conference Call";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ext_number_box;
        private System.Windows.Forms.Label ext_number_panel;
        private System.Windows.Forms.Button cancel_button;
        private System.Windows.Forms.Button conference_button;
        private System.Windows.Forms.Button call_button;
    }
}