using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CIC
{
    public partial class frmCamera : Form
    {
        public frmCamera()
        {
            InitializeComponent();
        }

        private void frmCamera_Load(object sender, EventArgs e)
        {
            //
        }

        private void viewLocalVideoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //MyipcamVideo
            if (MyipcamVideo.ShowLocalVideo != true)
            {
                MyipcamVideo.ShowLocalVideo = true;
            }
            else
            {
                MyipcamVideo.ShowLocalVideo = false;
                MyipcamVideo.Preview = false;
            }
        }

        public string RemoteHostIP
        {
            get
            {
                return MyipcamVideo.RemoteIP;
            }
            set
            {
                if (value.Trim() != "")
                {
                    MyipcamVideo.RemoteIP = value;
                }
            }
        }
    
        private void viewRemoteVideoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MyipcamVideo.Preview != true)
            {
                MyipcamVideo.Preview = true;
                MyipcamVideo.ShowLocalVideo = true;
            }
            else
            {
                MyipcamVideo.Preview = false;
            }
        }
    
    }
}
