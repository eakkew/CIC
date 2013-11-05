using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CIC
{
    public partial class frmConference : Form
    {
        public frmConference()
        {
            InitializeComponent();
            conference_button.Enabled = false;
        }

        private void call_button_Click(object sender, EventArgs e)
        {
            // call the number
            ext_number_box.Enabled = false;
            call_button.Enabled = false;
        }

        private void conference_button_Click(object sender, EventArgs e)
        {
            // complete the merge line and close the form
            this.Close();
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            // do not merge the line and close the form
            this.Close();
        }
    }
}
