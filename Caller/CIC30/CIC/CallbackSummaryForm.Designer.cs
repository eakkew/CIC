namespace CIC
{
  partial class CallbackSummaryForm
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
      this.CallbacksTodayTextBox = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.CallbacksTomorowTextBox = new System.Windows.Forms.TextBox();
      this.StatusLabel = new System.Windows.Forms.Label();
      this.QueryProgressBar = new System.Windows.Forms.ProgressBar();
      this.CloseButton = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // CallbacksTodayTextBox
      // 
      this.CallbacksTodayTextBox.Location = new System.Drawing.Point(189, 16);
      this.CallbacksTodayTextBox.Name = "CallbacksTodayTextBox";
      this.CallbacksTodayTextBox.Size = new System.Drawing.Size(100, 21);
      this.CallbacksTodayTextBox.TabIndex = 0;
      this.CallbacksTodayTextBox.TabStop = false;
      this.CallbacksTodayTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 19);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(154, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Callbacks scheduled for today:";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(12, 49);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(172, 13);
      this.label2.TabIndex = 2;
      this.label2.Text = "Callbacks scheduled for tomorrow:";
      // 
      // CallbacksTomorowTextBox
      // 
      this.CallbacksTomorowTextBox.Location = new System.Drawing.Point(189, 46);
      this.CallbacksTomorowTextBox.Name = "CallbacksTomorowTextBox";
      this.CallbacksTomorowTextBox.Size = new System.Drawing.Size(100, 21);
      this.CallbacksTomorowTextBox.TabIndex = 0;
      this.CallbacksTomorowTextBox.TabStop = false;
      this.CallbacksTomorowTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // StatusLabel
      // 
      this.StatusLabel.Location = new System.Drawing.Point(12, 82);
      this.StatusLabel.Name = "StatusLabel";
      this.StatusLabel.Size = new System.Drawing.Size(277, 47);
      this.StatusLabel.TabIndex = 3;
      this.StatusLabel.Text = "Querying...";
      this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
      // 
      // QueryProgressBar
      // 
      this.QueryProgressBar.Location = new System.Drawing.Point(15, 101);
      this.QueryProgressBar.Name = "QueryProgressBar";
      this.QueryProgressBar.Size = new System.Drawing.Size(274, 12);
      this.QueryProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
      this.QueryProgressBar.TabIndex = 4;
      // 
      // CloseButton
      // 
      this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.CloseButton.Location = new System.Drawing.Point(113, 132);
      this.CloseButton.Name = "CloseButton";
      this.CloseButton.Size = new System.Drawing.Size(78, 23);
      this.CloseButton.TabIndex = 5;
      this.CloseButton.Text = "Close";
      this.CloseButton.UseVisualStyleBackColor = true;
      //this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
      // 
      // CallbackSummaryForm
      // 
      this.AcceptButton = this.CloseButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.CloseButton;
      this.ClientSize = new System.Drawing.Size(300, 164);
      this.Controls.Add(this.CloseButton);
      this.Controls.Add(this.QueryProgressBar);
      this.Controls.Add(this.StatusLabel);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.CallbacksTomorowTextBox);
      this.Controls.Add(this.CallbacksTodayTextBox);
      this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "CallbackSummaryForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Callback Summary";
      //this.Load += new System.EventHandler(this.CallbackSummaryForm_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox CallbacksTodayTextBox;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox CallbacksTomorowTextBox;
    private System.Windows.Forms.Label StatusLabel;
    private System.Windows.Forms.ProgressBar QueryProgressBar;
    private System.Windows.Forms.Button CloseButton;
  }
}