namespace CIC
{
    partial class frmDisposition
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDisposition));
            this.save_button = new System.Windows.Forms.Button();
            this.finishcode_label = new System.Windows.Forms.Label();
            this.finishcode_combobox = new System.Windows.Forms.ComboBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.TimedOutInfoLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // save_button
            // 
            this.save_button.Location = new System.Drawing.Point(121, 61);
            this.save_button.Name = "save_button";
            this.save_button.Size = new System.Drawing.Size(75, 23);
            this.save_button.TabIndex = 0;
            this.save_button.Text = "Save";
            this.save_button.UseVisualStyleBackColor = true;
            this.save_button.Click += new System.EventHandler(this.save_button_Click);
            // 
            // finishcode_label
            // 
            this.finishcode_label.AutoSize = true;
            this.finishcode_label.Location = new System.Drawing.Point(50, 38);
            this.finishcode_label.Name = "finishcode_label";
            this.finishcode_label.Size = new System.Drawing.Size(65, 13);
            this.finishcode_label.TabIndex = 1;
            this.finishcode_label.Text = "Finish Code:";
            // 
            // finishcode_combobox
            // 
            this.finishcode_combobox.FormattingEnabled = true;
            this.finishcode_combobox.Location = new System.Drawing.Point(121, 35);
            this.finishcode_combobox.Name = "finishcode_combobox";
            this.finishcode_combobox.Size = new System.Drawing.Size(230, 21);
            this.finishcode_combobox.TabIndex = 2;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // TimedOutInfoLabel
            // 
            this.TimedOutInfoLabel.AutoSize = true;
            this.TimedOutInfoLabel.Location = new System.Drawing.Point(121, 13);
            this.TimedOutInfoLabel.Name = "TimedOutInfoLabel";
            this.TimedOutInfoLabel.Size = new System.Drawing.Size(35, 13);
            this.TimedOutInfoLabel.TabIndex = 3;
            this.TimedOutInfoLabel.Text = "label1";
            // 
            // frmDisposition
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 115);
            this.ControlBox = false;
            this.Controls.Add(this.TimedOutInfoLabel);
            this.Controls.Add(this.finishcode_combobox);
            this.Controls.Add(this.finishcode_label);
            this.Controls.Add(this.save_button);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDisposition";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Disposition";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmDisposition_FormClosed);
            this.Load += new System.EventHandler(this.frmDisposition_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button save_button;
        private System.Windows.Forms.Label finishcode_label;
        private System.Windows.Forms.ComboBox finishcode_combobox;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label TimedOutInfoLabel;
    }
}