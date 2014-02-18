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
            if (instance == null || instance.IsDisposed)
            {
                instance = new frmConference();

            }
            return instance;
        }

        private frmConference()
        {
            InitializeComponent();
            ext_number_box.Enabled = true;
            ext_number_box.Text = "";
            call_button.Enabled = false;
            conference_button.Enabled = false;
            this.ActiveControl = ext_number_box;
        }

        private void call_button_Click(object sender, EventArgs e)
        {
            if (Util.form_validation_telephone_number(ext_number_box.Text))
            {
                Program.MainDashboard.MakeConsultCall(ext_number_box.Text);
                ext_number_box.Enabled = false;
                conference_button.Enabled = true;
                call_button.Enabled = false;
            }
        }

        private void conference_button_Click(object sender, EventArgs e)
        {
            Program.MainDashboard.conference_invoke();
            ext_number_box.Enabled = true;
            conference_button.Enabled = false;
            call_button.Enabled = true;
            this.Close();
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            // clean up
            ext_number_box.Enabled = true;
            ext_number_box.Text = "";
            call_button.Enabled = true;
            conference_button.Enabled = false;

            Program.MainDashboard.DisconnectConsultCall();
            this.Close();
        }

        private void ext_number_box_TextChanged(object sender, EventArgs e)
        {
            if (!Util.form_validation_telephone_number(ext_number_box.Text))
            {
                ext_number_box.ForeColor = Color.Red;

                this.call_button.Enabled = false;
            }
            else
            {
                ext_number_box.ForeColor = Color.Black;

                this.call_button.Enabled = true;
            }
        }

        private void frmConference_Load(object sender, EventArgs e)
        {
            ext_number_box.Enabled = true;
            ext_number_box.Text = "";
            call_button.Enabled = false;
            conference_button.Enabled = false;
            this.ActiveControl = ext_number_box;
        }

    }
}
