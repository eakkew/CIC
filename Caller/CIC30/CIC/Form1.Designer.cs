namespace CIC
{
    partial class frmI3RptViewer
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
            this.I3RptViewer = new CrystalDecisions.Windows.Forms.CrystalReportViewer();
            this.SuspendLayout();
            // 
            // I3RptViewer
            // 
            this.I3RptViewer.ActiveViewIndex = -1;
            this.I3RptViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.I3RptViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.I3RptViewer.Location = new System.Drawing.Point(0, 0);
            this.I3RptViewer.Name = "I3RptViewer";
            this.I3RptViewer.SelectionFormula = "";
            this.I3RptViewer.Size = new System.Drawing.Size(843, 512);
            this.I3RptViewer.TabIndex = 0;
            this.I3RptViewer.ViewTimeSelectionFormula = "";
            // 
            // frmI3RptViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(843, 512);
            this.Controls.Add(this.I3RptViewer);
            this.Name = "frmI3RptViewer";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.frmI3RptViewer_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private CrystalDecisions.Windows.Forms.CrystalReportViewer I3RptViewer;
    }
}