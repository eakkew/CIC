using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ININ.IceLib;
using ININ.IceLib.Connection;
using ININ.IceLib.Interactions;
using ININ.IceLib.People;
using ININ.IceLib.Dialer;
using Locus.Control;

namespace CIC
{
    public partial class frmIVRList : Form
    {
        private string mCollectUserSelect;
        private string[] AllCollectUserSelect;

        public frmIVRList(string CollectUserSelect)
        {

            InitializeComponent();
            this.mCollectUserSelect = CollectUserSelect;
        }

        private void Initial_Value()
        {
            string scope = "CIC::MainForm::CrmScreenPop()::";
            Tracing.TraceStatus(scope + "Starting.");
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new MethodInvoker(Initial_Value));
            }
            else
            {
                try
                {
                    AllCollectUserSelect = mCollectUserSelect.Split(';');
                    this.lstIVRMenu.Items.Clear();
                    foreach (string UserSelect in AllCollectUserSelect)
                    {
                        this.lstIVRMenu.Items.Add(UserSelect);
                    }
                }
                catch (System.Exception ex)
                {
                    Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        private void frmIVRList_Load(object sender, EventArgs e)
        {
            this.Initial_Value();
        }
    }
}
