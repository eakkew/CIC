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
            if (Util.form_validation_telephone_number(ext_number_box.Text))
            {
                // TODO: hold current call
                // call the number
                ext_number_box.Enabled = false;
                conference_button.Enabled = true;
                call_button.Enabled = false;
            }
        }

        private void conference_button_Click(object sender, EventArgs e)
        {
            // TODO: 
            // complete the merge line and close the form
            //if (this.TransferTxtDestination.Text.Trim() != "")

            this.Close();
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            // TODO: do not merge the line and close the form
            // TODO: unhold line
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
