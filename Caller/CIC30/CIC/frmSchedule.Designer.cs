namespace CIC
{
    partial class frmSchedule
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSchedule));
            this.phone_box = new System.Windows.Forms.TextBox();
            this.hour_combobox = new System.Windows.Forms.ComboBox();
            this.minute_combobox = new System.Windows.Forms.ComboBox();
            this.phone_label = new System.Windows.Forms.Label();
            this.time_label = new System.Windows.Forms.Label();
            this.save_button = new System.Windows.Forms.Button();
            this.cancel_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // phone_box
            // 
            this.phone_box.Location = new System.Drawing.Point(155, 30);
            this.phone_box.MaxLength = 11;
            this.phone_box.Name = "phone_box";
            this.phone_box.Size = new System.Drawing.Size(86, 20);
            this.phone_box.TabIndex = 1;
            // 
            // hour_combobox
            // 
            this.hour_combobox.FormattingEnabled = true;
            this.hour_combobox.Location = new System.Drawing.Point(155, 57);
            this.hour_combobox.Name = "hour_combobox";
            this.hour_combobox.Size = new System.Drawing.Size(40, 21);
            this.hour_combobox.TabIndex = 2;
            this.hour_combobox.SelectedIndexChanged += new System.EventHandler(this.hour_combobox_SelectedIndexChanged);
            // 
            // minute_combobox
            // 
            this.minute_combobox.FormattingEnabled = true;
            this.minute_combobox.Location = new System.Drawing.Point(201, 57);
            this.minute_combobox.Name = "minute_combobox";
            this.minute_combobox.Size = new System.Drawing.Size(40, 21);
            this.minute_combobox.TabIndex = 3;
            this.minute_combobox.SelectedIndexChanged += new System.EventHandler(this.minute_combobox_SelectedIndexChanged);
            // 
            // phone_label
            // 
            this.phone_label.AutoSize = true;
            this.phone_label.Location = new System.Drawing.Point(87, 33);
            this.phone_label.Name = "phone_label";
            this.phone_label.Size = new System.Drawing.Size(62, 13);
            this.phone_label.TabIndex = 4;
            this.phone_label.Text = "Tel Number";
            // 
            // time_label
            // 
            this.time_label.AutoSize = true;
            this.time_label.Location = new System.Drawing.Point(71, 60);
            this.time_label.Name = "time_label";
            this.time_label.Size = new System.Drawing.Size(78, 13);
            this.time_label.TabIndex = 5;
            this.time_label.Text = "Schedule Time";
            // 
            // save_button
            // 
            this.save_button.Location = new System.Drawing.Point(90, 84);
            this.save_button.Name = "save_button";
            this.save_button.Size = new System.Drawing.Size(75, 23);
            this.save_button.TabIndex = 6;
            this.save_button.Text = "&Save";
            this.save_button.UseVisualStyleBackColor = true;
            this.save_button.Click += new System.EventHandler(this.save_button_Click);
            // 
            // cancel_button
            // 
            this.cancel_button.Location = new System.Drawing.Point(171, 84);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(75, 23);
            this.cancel_button.TabIndex = 7;
            this.cancel_button.Text = "C&ancel";
            this.cancel_button.UseVisualStyleBackColor = true;
            this.cancel_button.Click += new System.EventHandler(this.cancel_button_Click);
            // 
            // frmSchedule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(332, 146);
            this.ControlBox = false;
            this.Controls.Add(this.cancel_button);
            this.Controls.Add(this.save_button);
            this.Controls.Add(this.time_label);
            this.Controls.Add(this.phone_label);
            this.Controls.Add(this.minute_combobox);
            this.Controls.Add(this.hour_combobox);
            this.Controls.Add(this.phone_box);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSchedule";
            this.Text = "Schedule Call Back";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmSchedule_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox phone_box;
        private System.Windows.Forms.ComboBox hour_combobox;
        private System.Windows.Forms.ComboBox minute_combobox;
        private System.Windows.Forms.Label phone_label;
        private System.Windows.Forms.Label time_label;
        private System.Windows.Forms.Button save_button;
        private System.Windows.Forms.Button cancel_button;

    }
}