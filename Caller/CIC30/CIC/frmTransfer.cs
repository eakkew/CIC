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
                // TODO: hold current line
                // call the number
                call_button.Enabled = false;
                transfer_button.Enabled = true;
                ext_number_box.Enabled = false;
                cancel_button.Enabled = false;
            }
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            // TODO: unhold current line
            // TODO: make sure the calling was disconnected
            this.Close();
        }

        private void transfer_button_Click(object sender, EventArgs e)
        {
            // transfer the current connection to another
            
            //if (this.ActiveNormalInteraction != null)
            //{
            //    this.ActiveNormalInteraction.BlindTransfer(this.TransferTxtDestination.Text);
            //}
            //Tracing.TraceNote(scope + "Performing blind transfer");


            //ININ.IceLib.People.UserStatusUpdate statusUpdate = new UserStatusUpdate(this.mPeopleManager);
            //string sFinishcode = global::CIC.Properties.Settings.Default.ReasonCode_Transfereded;
            //ININ.IceLib.Dialer.ReasonCode sReasoncode = ININ.IceLib.Dialer.ReasonCode.Transferred;
            //CallCompletionParameters callCompletionParameters = new CallCompletionParameters(sReasoncode, sFinishcode);
            //this.ActiveDialerInteraction.CallComplete(callCompletionParameters);
            //statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
            //statusUpdate.UpdateRequest();
            //this.imgcmbAgentStatus.SetMessage(this.AvailableStatusMessageDetails.MessageText);  //Set Available status for a new call.
                            
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
