using ININ.IceLib;
using ININ.IceLib.Connection;
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

        private frmWorkflow()
        {

        }

        private static frmWorkflow instance = null;
        private Session mSession;

        public static frmWorkflow getInstance(Session IC_Session)
        {
            if (instance == null || instance.IsDisposed)
            {
                instance = new frmWorkflow(IC_Session);

            }
            return instance;
        }

        private frmWorkflow(Session IC_Session)
        {
            string scope = "CIC::MainForm::LoginToolStripMenuItem_DropDownOpening()::";
            InitializeComponent();
            mSession = IC_Session;
        }

        private void login_button_Click(object sender, EventArgs e)
        {
            //do stuff to select workflow
            if (this.workflow_combobox.SelectedItem != null)
            {
                Program.MainDashboard.workflow_invoke(this.workflow_combobox.SelectedItem, e);
                this.Close();
            }
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            // gracefully exit this Form
            this.Close();
        }

        private void frmWorkflow_FormClosed(object sender, FormClosedEventArgs e)
        {
            workflow_combobox.Items.Clear();
        }

        private void frmWorkflow_Load(object sender, EventArgs e)
        {
            try
            {
                Program.Initialize_dialingManager(mSession);
                string[] workflows = Program.mDialingManager.GetAvailableWorkflows();

                workflow_combobox.Items.Clear();
                workflow_combobox.SelectedItem = null;
                workflow_combobox.SelectedText = "";
                if (workflows.Length > 0)
                {
                    foreach (string workflow in workflows)
                    {
                        //ToolStripMenuItem menu = new ToolStripMenuItem((string)workflow, null, WorkflowToolStripMenuItem_Click);
                        //menu.Image = global::CIC.Properties.Resources.pin_green;
                        this.workflow_combobox.Items.Add(workflow);
                    }
                }
                else
                {
                    ToolStripMenuItem menu = new ToolStripMenuItem("No Workflows Available");
                    menu.Enabled = false;
                    this.workflow_combobox.Items.Add(menu);
                }
                this.workflow_combobox.SelectedIndex = 0;
                this.ActiveControl = workflow_combobox;
            }
            catch (ININ.IceLib.IceLibException ex)
            {
                string output = String.Format("Cannot retrieving available workflows: {0}", ex.Message);
                //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                MessageBox.Show(output, "CIC Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                string output = String.Format("Something really bad happened: {0}", ex.Message);
                MessageBox.Show(output, "CIC Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
