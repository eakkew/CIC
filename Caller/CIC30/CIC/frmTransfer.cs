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
        
        private static frmTransfer instance = null;

        public static frmTransfer getInstance()
        {
            if (instance == null || instance.IsDisposed)
            {
                instance = new frmTransfer();

            }
            return instance;
        }
        /*
         * TODO change from public to private
         */
        private frmTransfer()
        {
            InitializeComponent();
            transfer_button.Enabled = false;
            call_button.Enabled = false;
            this.ActiveControl = ext_number_box;
        }

        private void call_button_Click(object sender, EventArgs e)
        {
            if (Util.form_validation_telephone_number(ext_number_box.Text))
            {
                // TODO: hold current line
                Program.MainDashboard.MakeConsultCall(ext_number_box.Text);
                call_button.Enabled = false;
                transfer_button.Enabled = true;
                ext_number_box.Enabled = false;
                cancel_button.Enabled = true;
            }
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            // TODO: unhold current line
            // TODO: make sure the calling was disconnected
            Program.MainDashboard.DisconnectConsultCall();
            call_button.Enabled = true;
            transfer_button.Enabled = false;
            ext_number_box.Enabled = true;
            ext_number_box.Text = "";
            cancel_button.Enabled = true;
            this.Close();
        }

        private void transfer_button_Click(object sender, EventArgs e)
        {
            // transfer the current connection to another
            Program.MainDashboard.transfer_invoke(ext_number_box.Text);
            call_button.Enabled = true;
            transfer_button.Enabled = false;
            ext_number_box.Enabled = true;
            ext_number_box.Text = "";
            cancel_button.Enabled = true;
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

        private void frmTransfer_Load(object sender, EventArgs e)
        {
            transfer_button.Enabled = false;
            call_button.Enabled = false;
            this.ActiveControl = ext_number_box;
        }
        

    }
}
