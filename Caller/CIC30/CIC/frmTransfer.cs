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
    public partial class frmTransfer : Form
    {
        public frmTransfer()
        {
            InitializeComponent();
            transfer_button.Enabled = false;
        }

        private void call_button_Click(object sender, EventArgs e)
        {
            if (Util.form_validation_telephone_number(ext_number_box.Text))
            {
                // call the number
                call_button.Enabled = false;
                transfer_button.Enabled = true;
                ext_number_box.Enabled = false;
                cancel_button.Enabled = false;
            }
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void transfer_button_Click(object sender, EventArgs e)
        {
            // transfer the current connection to another
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
