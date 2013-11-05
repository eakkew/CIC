using ININ.IceLib.Connection;
using ININ.IceLib.Interactions;
using ININ.IceLib.Interactions.InteractionsManager;
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
        private Session IC_Session;
        private InteractionsManager NormalInterationManager;
        public frmManualCall()
        {
            InitializeComponent();
        }

        public frmManualCall(Session session, InteractionsManager manager)
        {
            this.IC_Session = session;
            this.NormalInterationManager = manager;
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
                    //Tracing.TraceStatus(scope + "Call button clicked.Log On to Basic station.");
                    ININ.IceLib.Interactions.CallInteractionParameters callParams = new ININ.IceLib.Interactions.CallInteractionParameters(phone_box.Text, CallMadeStage.Allocated);
                    ININ.IceLib.Connection.SessionSettings sessionSetting = this.IC_Session.GetSessionSettings();
                    callParams.AdditionalAttributes.Add("CallerHost", sessionSetting.MachineName.ToString());
                    //this.IsManualDialing = true;
                    this.NormalInterationManager.MakeCallAsync(callParams, FormMain.MakeCallCompleted, null);
                    //this.SetCallHistory();
                }
                
                //Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }

            this.Close();
        }

        private bool CheckEmptyPhoneNumber()
        {
            //check the phone
            return false;
        }
    }
}
