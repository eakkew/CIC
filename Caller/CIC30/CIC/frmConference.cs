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
        private static frmConference instance = null;

        public static frmConference getInstance()
        {
            if (instance == null)
            {
                instance = new frmConference();

            }
            return instance;
        }

        private frmConference()
        {
            InitializeComponent();
            conference_button.Enabled = false;
        }

        private void call_button_Click(object sender, EventArgs e)
        {
            if (Util.form_validation_telephone_number(ext_number_box.Text))
            {
                // TODO: hold current call
                // call the number
                Program.MainDashboard.MakeConsultCall(ext_number_box.Text);
                ext_number_box.Enabled = false;
                conference_button.Enabled = true;
                call_button.Enabled = false;
            }
        }

        private void conference_button_Click(object sender, EventArgs e)
        {
            // TODO: 
            // complete the merge line and close the form
            Program.MainDashboard.conference_invoke(this.ext_number_box.Text);
            ext_number_box.Enabled = true;
            conference_button.Enabled = false;
            call_button.Enabled = true;
            this.Close();
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            // TODO: do not merge the line and close the form
            // TODO: unhold line
            Program.MainDashboard.DisconnectConsultCall();
            this.Close();
        }

        private void ext_number_box_TextChanged(object sender, EventArgs e)
        {
            if (!Util.form_validation_telephone_number(ext_number_box.Text))
            {
                ext_number_box.ForeColor = Color.Red;
            }
            else
            {
                ext_number_box.ForeColor = Color.Black;
            }
        }

    }
}
