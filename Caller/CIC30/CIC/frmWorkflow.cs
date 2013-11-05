using ININ.IceLib;
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
    public partial class frmWorkflow : Form
    {
        private string[] Workflows;

        public frmWorkflow()
        {
            InitializeComponent();
            // load up workflows into combobox
            try
            {
                this.Workflows = Program.DialingManager.GetAvailableWorkflows();
            }
            catch (System.Exception ex)
            {
                string output = String.Format("Cannot retrieving available workflows: {0}", ex.Message);
                //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                //MessageBox.Show(output, "CIC Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            workflow_combobox.Items.Clear();


            if (workflow_combobox.Items.Count > 0)
            {
                workflow_combobox.Enabled = true;
                foreach (string workflow in this.Workflows)
                {
                    workflow_combobox.Items.Add(workflow);
                }

            }
            else
            {
                workflow_combobox.Items.Add("No workflows available");
                workflow_combobox.SelectedIndex = 0;
                workflow_combobox.Enabled = false;
            }
            

        }

        private void login_button_Click(object sender, EventArgs e)
        {
            //do stuff to select workflow
            this.Close();
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            // gracefully exit this Form
            this.Close();
        }

        private void frmWorkflow_FormClosed(object sender, FormClosedEventArgs e)
        {
            global::CIC.Program.WorkflowFormClosed = true;
        }
    }
}
