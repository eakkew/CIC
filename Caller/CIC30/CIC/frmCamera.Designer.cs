namespace CIC
{
    partial class frmCamera
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmCamera));
            this.popMnuVideo = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cameraSettingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewLocalVideoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewRemoteVideoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.portSettingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MyipcamVideo = new Locus.Control.IPCAMVideo();
            this.lblSender = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.popMnuVideo.SuspendLayout();
            this.SuspendLayout();
            // 
            // popMnuVideo
            // 
            this.popMnuVideo.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cameraSettingToolStripMenuItem,
            this.viewLocalVideoToolStripMenuItem,
            this.viewRemoteVideoToolStripMenuItem,
            this.portSettingToolStripMenuItem});
            this.popMnuVideo.Name = "popMnuVideo";
            this.popMnuVideo.Size = new System.Drawing.Size(172, 92);
            // 
            // cameraSettingToolStripMenuItem
            // 
            this.cameraSettingToolStripMenuItem.Name = "cameraSettingToolStripMenuItem";
            this.cameraSettingToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.cameraSettingToolStripMenuItem.Text = "Camera Setting";
            // 
            // viewLocalVideoToolStripMenuItem
            // 
            this.viewLocalVideoToolStripMenuItem.Name = "viewLocalVideoToolStripMenuItem";
            this.viewLocalVideoToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.viewLocalVideoToolStripMenuItem.Text = "Remote/ Local Video";
            this.viewLocalVideoToolStripMenuItem.Click += new System.EventHandler(this.viewLocalVideoToolStripMenuItem_Click);
            // 
            // viewRemoteVideoToolStripMenuItem
            // 
            this.viewRemoteVideoToolStripMenuItem.Name = "viewRemoteVideoToolStripMenuItem";
            this.viewRemoteVideoToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.viewRemoteVideoToolStripMenuItem.Text = "Start/Stop Camera";
            this.viewRemoteVideoToolStripMenuItem.Click += new System.EventHandler(this.viewRemoteVideoToolStripMenuItem_Click);
            // 
            // portSettingToolStripMenuItem
            // 
            this.portSettingToolStripMenuItem.Name = "portSettingToolStripMenuItem";
            this.portSettingToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.portSettingToolStripMenuItem.Text = "Port Setting";
            // 
            // MyipcamVideo
            // 
            this.MyipcamVideo.ExtensionNumber = 0;
            this.MyipcamVideo.LocalIP = "127.0.0.1";
            this.MyipcamVideo.LocalPort = 8001;
            this.MyipcamVideo.LocalSIP = "UNKNOWN@UNKNOWN_IP";
            this.MyipcamVideo.Location = new System.Drawing.Point(1, 1);
            this.MyipcamVideo.Name = "MyipcamVideo";
            this.MyipcamVideo.Preview = false;
            this.MyipcamVideo.RemoteIP = "UNKNOWN_IP";
            this.MyipcamVideo.RemotePort = 8000;
            this.MyipcamVideo.RemoteSIP = "UNKNOWN@UNKNOWN_IP";
            this.MyipcamVideo.ShowLocalVideo = true;
            this.MyipcamVideo.Size = new System.Drawing.Size(324, 308);
            this.MyipcamVideo.TabIndex = 1;
            this.MyipcamVideo.VideoHeight = 308;
            this.MyipcamVideo.VideoSize = new System.Drawing.Size(324, 308);
            this.MyipcamVideo.VideoWidth = 324;
            // 
            // lblSender
            // 
            this.lblSender.BackColor = System.Drawing.Color.Transparent;
            this.lblSender.Location = new System.Drawing.Point(-2, 312);
            this.lblSender.Name = "lblSender";
            this.lblSender.Size = new System.Drawing.Size(46, 23);
            this.lblSender.TabIndex = 2;
            this.lblSender.Text = "Sender";
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Location = new System.Drawing.Point(50, 312);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(264, 23);
            this.label2.TabIndex = 3;
            this.label2.Text = "Unknow";
            // 
            // frmCamera
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::CIC.Properties.Resources.loginBG;
            this.ClientSize = new System.Drawing.Size(326, 335);
            this.ContextMenuStrip = this.popMnuVideo;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblSender);
            this.Controls.Add(this.MyipcamVideo);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmCamera";
            this.Text = "Camera";
            this.Load += new System.EventHandler(this.frmCamera_Load);
            this.popMnuVideo.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip popMnuVideo;
        private System.Windows.Forms.ToolStripMenuItem cameraSettingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewLocalVideoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewRemoteVideoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem portSettingToolStripMenuItem;
        private Locus.Control.IPCAMVideo MyipcamVideo;
        private System.Windows.Forms.Label lblSender;
        private System.Windows.Forms.Label label2;
    }
}