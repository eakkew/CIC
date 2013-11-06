namespace CIC
{
    partial class frmManualCall
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
            this.manual_call_label = new System.Windows.Forms.Label();
            this.phone_box = new System.Windows.Forms.TextBox();
            this.call_button = new System.Windows.Forms.Button();
            this.cancel_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // manual_call_label
            // 
            this.manual_call_label.AutoSize = true;
            this.manual_call_label.Location = new System.Drawing.Point(13, 29);
            this.manual_call_label.Name = "manual_call_label";
            this.manual_call_label.Size = new System.Drawing.Size(68, 13);
            this.manual_call_label.TabIndex = 0;
            this.manual_call_label.Text = "Tel. Number:";
            // 
            // phone_box
            // 
            this.phone_box.Location = new System.Drawing.Point(87, 26);
            this.phone_box.MaxLength = 10;
            this.phone_box.Name = "phone_box";
            this.phone_box.Size = new System.Drawing.Size(100, 20);
            this.phone_box.TabIndex = 1;
            // 
            // call_button
            // 
            this.call_button.Location = new System.Drawing.Point(31, 52);
            this.call_button.Name = "call_button";
            this.call_button.Size = new System.Drawing.Size(75, 23);
            this.call_button.TabIndex = 2;
            this.call_button.Text = "Call";
            this.call_button.UseVisualStyleBackColor = true;
            this.call_button.Click += new System.EventHandler(this.call_button_Click);
            // 
            // cancel_button
            // 
            this.cancel_button.Location = new System.Drawing.Point(112, 52);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(75, 23);
            this.cancel_button.TabIndex = 3;
            this.cancel_button.Text = "Cancel";
            this.cancel_button.UseVisualStyleBackColor = true;
            this.cancel_button.Click += new System.EventHandler(this.cancel_button_Click);
            // 
            // frmManualCall
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(220, 97);
            this.Controls.Add(this.cancel_button);
            this.Controls.Add(this.call_button);
            this.Controls.Add(this.phone_box);
            this.Controls.Add(this.manual_call_label);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmManualCall";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Manual Call";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label manual_call_label;
        private System.Windows.Forms.TextBox phone_box;
        private System.Windows.Forms.Button call_button;
        private System.Windows.Forms.Button cancel_button;
    }
}