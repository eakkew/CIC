namespace CIC
{
  partial class ScheduleCallbackForm
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScheduleCallbackForm));
        this.groupBox1 = new System.Windows.Forms.GroupBox();
        this.CallbackScheduledDateTimeLabel = new System.Windows.Forms.Label();
        this.label1 = new System.Windows.Forms.Label();
        this.panel1 = new System.Windows.Forms.Panel();
        this.OffsetTimeRadioButton = new System.Windows.Forms.RadioButton();
        this.SpecificTimeRadioButton = new System.Windows.Forms.RadioButton();
        this.SpecificTimeDateTimePicker = new System.Windows.Forms.DateTimePicker();
        this.OffsetTimeComboBox = new System.Windows.Forms.ComboBox();
        this.AnotherDayTimePicker = new System.Windows.Forms.DateTimePicker();
        this.AnotherDayRadioButton = new System.Windows.Forms.RadioButton();
        this.AnotherDayDatePicker = new System.Windows.Forms.DateTimePicker();
        this.TodayRadioButton = new System.Windows.Forms.RadioButton();
        this.groupBox3 = new System.Windows.Forms.GroupBox();
        this.GroupRadioButton = new System.Windows.Forms.RadioButton();
        this.MyselfRadioButton = new System.Windows.Forms.RadioButton();
        this.OKButton = new System.Windows.Forms.Button();
        this.CancelScheduleButton = new System.Windows.Forms.Button();
        this.groupBox2 = new System.Windows.Forms.GroupBox();
        this.ScheduleNumberTextBox = new System.Windows.Forms.TextBox();
        this.label2 = new System.Windows.Forms.Label();
        this.groupBox1.SuspendLayout();
        this.panel1.SuspendLayout();
        this.groupBox3.SuspendLayout();
        this.groupBox2.SuspendLayout();
        this.SuspendLayout();
        // 
        // groupBox1
        // 
        this.groupBox1.BackColor = System.Drawing.Color.Transparent;
        this.groupBox1.Controls.Add(this.CallbackScheduledDateTimeLabel);
        this.groupBox1.Controls.Add(this.label1);
        this.groupBox1.Controls.Add(this.panel1);
        this.groupBox1.Controls.Add(this.AnotherDayTimePicker);
        this.groupBox1.Controls.Add(this.AnotherDayRadioButton);
        this.groupBox1.Controls.Add(this.AnotherDayDatePicker);
        this.groupBox1.Controls.Add(this.TodayRadioButton);
        this.groupBox1.Location = new System.Drawing.Point(12, 12);
        this.groupBox1.Name = "groupBox1";
        this.groupBox1.Size = new System.Drawing.Size(352, 190);
        this.groupBox1.TabIndex = 0;
        this.groupBox1.TabStop = false;
        this.groupBox1.Text = "Callback Date and Time:";
        // 
        // CallbackScheduledDateTimeLabel
        // 
        this.CallbackScheduledDateTimeLabel.AutoSize = true;
        this.CallbackScheduledDateTimeLabel.Location = new System.Drawing.Point(137, 165);
        this.CallbackScheduledDateTimeLabel.Name = "CallbackScheduledDateTimeLabel";
        this.CallbackScheduledDateTimeLabel.Size = new System.Drawing.Size(104, 13);
        this.CallbackScheduledDateTimeLabel.TabIndex = 9;
        this.CallbackScheduledDateTimeLabel.Text = "yyyy-mm-dd HH:mm";
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(12, 165);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(114, 13);
        this.label1.TabIndex = 8;
        this.label1.Text = "Callback scheduled at:";
        // 
        // panel1
        // 
        this.panel1.Controls.Add(this.OffsetTimeRadioButton);
        this.panel1.Controls.Add(this.SpecificTimeRadioButton);
        this.panel1.Controls.Add(this.SpecificTimeDateTimePicker);
        this.panel1.Controls.Add(this.OffsetTimeComboBox);
        this.panel1.Location = new System.Drawing.Point(29, 43);
        this.panel1.Name = "panel1";
        this.panel1.Size = new System.Drawing.Size(310, 54);
        this.panel1.TabIndex = 1;
        // 
        // OffsetTimeRadioButton
        // 
        this.OffsetTimeRadioButton.AutoSize = true;
        this.OffsetTimeRadioButton.Checked = true;
        this.OffsetTimeRadioButton.Location = new System.Drawing.Point(4, 4);
        this.OffsetTimeRadioButton.Name = "OffsetTimeRadioButton";
        this.OffsetTimeRadioButton.Size = new System.Drawing.Size(60, 17);
        this.OffsetTimeRadioButton.TabIndex = 0;
        this.OffsetTimeRadioButton.TabStop = true;
        this.OffsetTimeRadioButton.Text = "&Offset:";
        this.OffsetTimeRadioButton.UseVisualStyleBackColor = true;
        this.OffsetTimeRadioButton.CheckedChanged += new System.EventHandler(this.OffsetTimeRadioButton_CheckedChanged);
        // 
        // SpecificTimeRadioButton
        // 
        this.SpecificTimeRadioButton.AutoSize = true;
        this.SpecificTimeRadioButton.Location = new System.Drawing.Point(4, 31);
        this.SpecificTimeRadioButton.Name = "SpecificTimeRadioButton";
        this.SpecificTimeRadioButton.Size = new System.Drawing.Size(65, 17);
        this.SpecificTimeRadioButton.TabIndex = 0;
        this.SpecificTimeRadioButton.Text = "&Specific:";
        this.SpecificTimeRadioButton.UseVisualStyleBackColor = true;
        this.SpecificTimeRadioButton.CheckedChanged += new System.EventHandler(this.SpecificTimeRadioButton_CheckedChanged);
        // 
        // SpecificTimeDateTimePicker
        // 
        this.SpecificTimeDateTimePicker.CustomFormat = "HH:mm:ss";
        this.SpecificTimeDateTimePicker.Enabled = false;
        this.SpecificTimeDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
        this.SpecificTimeDateTimePicker.Location = new System.Drawing.Point(85, 31);
        this.SpecificTimeDateTimePicker.Name = "SpecificTimeDateTimePicker";
        this.SpecificTimeDateTimePicker.ShowUpDown = true;
        this.SpecificTimeDateTimePicker.Size = new System.Drawing.Size(132, 21);
        this.SpecificTimeDateTimePicker.TabIndex = 1;
        this.SpecificTimeDateTimePicker.TabStop = false;
        this.SpecificTimeDateTimePicker.ValueChanged += new System.EventHandler(this.SpecificTimeDateTimePicker_ValueChanged);
        // 
        // OffsetTimeComboBox
        // 
        this.OffsetTimeComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
        this.OffsetTimeComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
        this.OffsetTimeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.OffsetTimeComboBox.FormattingEnabled = true;
        this.OffsetTimeComboBox.Items.AddRange(new object[] {
            "15 minutes later",
            "30 minutes later",
            "1 hour later",
            "2 hours later",
            "3 hours later",
            "4 hours later"});
        this.OffsetTimeComboBox.Location = new System.Drawing.Point(85, 3);
        this.OffsetTimeComboBox.Name = "OffsetTimeComboBox";
        this.OffsetTimeComboBox.Size = new System.Drawing.Size(133, 21);
        this.OffsetTimeComboBox.TabIndex = 1;
        this.OffsetTimeComboBox.TabStop = false;
        this.OffsetTimeComboBox.SelectedIndexChanged += new System.EventHandler(this.OffsetTimeComboBox_SelectedIndexChanged);
        // 
        // AnotherDayTimePicker
        // 
        this.AnotherDayTimePicker.CustomFormat = "HH:mm:ss";
        this.AnotherDayTimePicker.Enabled = false;
        this.AnotherDayTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
        this.AnotherDayTimePicker.Location = new System.Drawing.Point(239, 126);
        this.AnotherDayTimePicker.Name = "AnotherDayTimePicker";
        this.AnotherDayTimePicker.ShowUpDown = true;
        this.AnotherDayTimePicker.Size = new System.Drawing.Size(100, 21);
        this.AnotherDayTimePicker.TabIndex = 2;
        this.AnotherDayTimePicker.TabStop = false;
        this.AnotherDayTimePicker.ValueChanged += new System.EventHandler(this.AnotherDayTimePicker_ValueChanged);
        // 
        // AnotherDayRadioButton
        // 
        this.AnotherDayRadioButton.AutoSize = true;
        this.AnotherDayRadioButton.Location = new System.Drawing.Point(15, 103);
        this.AnotherDayRadioButton.Name = "AnotherDayRadioButton";
        this.AnotherDayRadioButton.Size = new System.Drawing.Size(89, 17);
        this.AnotherDayRadioButton.TabIndex = 0;
        this.AnotherDayRadioButton.Text = "&Another day:";
        this.AnotherDayRadioButton.UseVisualStyleBackColor = true;
        this.AnotherDayRadioButton.CheckedChanged += new System.EventHandler(this.AnotherDayRadioButton_CheckedChanged);
        // 
        // AnotherDayDatePicker
        // 
        this.AnotherDayDatePicker.Enabled = false;
        this.AnotherDayDatePicker.Location = new System.Drawing.Point(33, 126);
        this.AnotherDayDatePicker.Name = "AnotherDayDatePicker";
        this.AnotherDayDatePicker.Size = new System.Drawing.Size(200, 21);
        this.AnotherDayDatePicker.TabIndex = 1;
        this.AnotherDayDatePicker.TabStop = false;
        this.AnotherDayDatePicker.ValueChanged += new System.EventHandler(this.AnotherDayDatePicker_ValueChanged);
        // 
        // TodayRadioButton
        // 
        this.TodayRadioButton.AutoSize = true;
        this.TodayRadioButton.Checked = true;
        this.TodayRadioButton.Location = new System.Drawing.Point(15, 20);
        this.TodayRadioButton.Name = "TodayRadioButton";
        this.TodayRadioButton.Size = new System.Drawing.Size(55, 17);
        this.TodayRadioButton.TabIndex = 0;
        this.TodayRadioButton.TabStop = true;
        this.TodayRadioButton.Text = "&Today";
        this.TodayRadioButton.UseVisualStyleBackColor = true;
        this.TodayRadioButton.CheckedChanged += new System.EventHandler(this.TodayRadioButton_CheckedChanged);
        // 
        // groupBox3
        // 
        this.groupBox3.BackColor = System.Drawing.Color.Transparent;
        this.groupBox3.Controls.Add(this.GroupRadioButton);
        this.groupBox3.Controls.Add(this.MyselfRadioButton);
        this.groupBox3.Location = new System.Drawing.Point(12, 301);
        this.groupBox3.Name = "groupBox3";
        this.groupBox3.Size = new System.Drawing.Size(352, 73);
        this.groupBox3.TabIndex = 2;
        this.groupBox3.TabStop = false;
        this.groupBox3.Text = "Schedule Callback To:";
        // 
        // GroupRadioButton
        // 
        this.GroupRadioButton.AutoSize = true;
        this.GroupRadioButton.Location = new System.Drawing.Point(15, 45);
        this.GroupRadioButton.Name = "GroupRadioButton";
        this.GroupRadioButton.Size = new System.Drawing.Size(123, 17);
        this.GroupRadioButton.TabIndex = 0;
        this.GroupRadioButton.Text = "Anyone in the &group";
        this.GroupRadioButton.UseVisualStyleBackColor = true;
        this.GroupRadioButton.CheckedChanged += new System.EventHandler(this.GroupRadioButton_CheckedChanged);
        // 
        // MyselfRadioButton
        // 
        this.MyselfRadioButton.AutoSize = true;
        this.MyselfRadioButton.Checked = true;
        this.MyselfRadioButton.Location = new System.Drawing.Point(15, 21);
        this.MyselfRadioButton.Name = "MyselfRadioButton";
        this.MyselfRadioButton.Size = new System.Drawing.Size(56, 17);
        this.MyselfRadioButton.TabIndex = 0;
        this.MyselfRadioButton.TabStop = true;
        this.MyselfRadioButton.Text = "&Myself";
        this.MyselfRadioButton.UseVisualStyleBackColor = true;
        this.MyselfRadioButton.CheckedChanged += new System.EventHandler(this.MyselfRadioButton_CheckedChanged);
        // 
        // OKButton
        // 
        this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.OKButton.Location = new System.Drawing.Point(112, 391);
        this.OKButton.Name = "OKButton";
        this.OKButton.Size = new System.Drawing.Size(75, 23);
        this.OKButton.TabIndex = 3;
        this.OKButton.Text = "OK";
        this.OKButton.UseVisualStyleBackColor = true;
        this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
        // 
        // CancelScheduleButton
        // 
        this.CancelScheduleButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.CancelScheduleButton.Location = new System.Drawing.Point(203, 391);
        this.CancelScheduleButton.Name = "CancelScheduleButton";
        this.CancelScheduleButton.Size = new System.Drawing.Size(75, 23);
        this.CancelScheduleButton.TabIndex = 4;
        this.CancelScheduleButton.Text = "Cancel";
        this.CancelScheduleButton.UseVisualStyleBackColor = true;
        this.CancelScheduleButton.Click += new System.EventHandler(this.CancelScheduleButton_Click);
        // 
        // groupBox2
        // 
        this.groupBox2.BackColor = System.Drawing.Color.Transparent;
        this.groupBox2.Controls.Add(this.ScheduleNumberTextBox);
        this.groupBox2.Controls.Add(this.label2);
        this.groupBox2.Location = new System.Drawing.Point(12, 219);
        this.groupBox2.Name = "groupBox2";
        this.groupBox2.Size = new System.Drawing.Size(352, 65);
        this.groupBox2.TabIndex = 1;
        this.groupBox2.TabStop = false;
        this.groupBox2.Text = "Number to Call:";
        // 
        // ScheduleNumberTextBox
        // 
        this.ScheduleNumberTextBox.Location = new System.Drawing.Point(100, 23);
        this.ScheduleNumberTextBox.Name = "ScheduleNumberTextBox";
        this.ScheduleNumberTextBox.Size = new System.Drawing.Size(146, 21);
        this.ScheduleNumberTextBox.TabIndex = 0;
        this.ScheduleNumberTextBox.TextChanged += new System.EventHandler(this.ScheduleNumberTextBox_TextChanged);
        // 
        // label2
        // 
        this.label2.AutoSize = true;
        this.label2.Location = new System.Drawing.Point(12, 26);
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size(80, 13);
        this.label2.TabIndex = 0;
        this.label2.Text = "&Phone number:";
        // 
        // ScheduleCallbackForm
        // 
        this.AcceptButton = this.OKButton;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackgroundImage = global::CIC.Properties.Resources.bluebgBar;
        this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
        this.CancelButton = this.CancelScheduleButton;
        this.ClientSize = new System.Drawing.Size(375, 426);
        this.Controls.Add(this.groupBox2);
        this.Controls.Add(this.CancelScheduleButton);
        this.Controls.Add(this.OKButton);
        this.Controls.Add(this.groupBox3);
        this.Controls.Add(this.groupBox1);
        this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "ScheduleCallbackForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Schedule Callback";
        this.Load += new System.EventHandler(this.ScheduleCallbackForm_Load);
        this.groupBox1.ResumeLayout(false);
        this.groupBox1.PerformLayout();
        this.panel1.ResumeLayout(false);
        this.panel1.PerformLayout();
        this.groupBox3.ResumeLayout(false);
        this.groupBox3.PerformLayout();
        this.groupBox2.ResumeLayout(false);
        this.groupBox2.PerformLayout();
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.RadioButton AnotherDayRadioButton;
    private System.Windows.Forms.DateTimePicker AnotherDayDatePicker;
    private System.Windows.Forms.RadioButton TodayRadioButton;
    private System.Windows.Forms.ComboBox OffsetTimeComboBox;
    private System.Windows.Forms.RadioButton SpecificTimeRadioButton;
    private System.Windows.Forms.RadioButton OffsetTimeRadioButton;
    private System.Windows.Forms.DateTimePicker SpecificTimeDateTimePicker;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.RadioButton GroupRadioButton;
    private System.Windows.Forms.RadioButton MyselfRadioButton;
    private System.Windows.Forms.Button OKButton;
    private System.Windows.Forms.Button CancelScheduleButton;
    private System.Windows.Forms.DateTimePicker AnotherDayTimePicker;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Label CallbackScheduledDateTimeLabel;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox ScheduleNumberTextBox;
  }
}