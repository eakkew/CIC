using ININ.IceLib.Connection;
using ININ.IceLib.Interactions;
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
    public partial class frmManualCall : Form
    {
        private static frmManualCall instance = null;

        public static frmManualCall getInstance()
        {
            if (instance == null || instance.IsDisposed)
            {
                instance = new frmManualCall();

            }
            return instance;
        }

        private frmManualCall()
        {
            InitializeComponent();
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void call_button_Click(object sender, EventArgs e)
        {
            // call back to main form to make a call and close this form
            string scope = "CIC::frmMain::CallToolStripButton_ButtonClick()::";
            //Tracing.TraceStatus(scope + "Starting.");
            try
            {
                //this.CallToolStripSplitButton.Enabled = false;
                if (this.CheckEmptyPhoneNumber() != true)
                {
                    Program.MainDashboard.MakeManualCall(phone_box.Text);
                }
                
                //Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }

            this.Close();
        }
        /*
         * TODO : Change function name to validate textbox
         */
        private bool CheckEmptyPhoneNumber()
        {
            //check the phone
            return !Util.form_validation_telephone_number(phone_box.Text);
        }

        private void phone_box_TextChanged(object sender, EventArgs e)
        {
            if (!Util.form_validation_telephone_number(phone_box.Text))
            {
                phone_box.ForeColor = Color.Red;
            }
            else
            {
                phone_box.ForeColor = Color.Black;
            }
        }
    }
}
