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
    public partial class frmDisposition : Form
    {
        private DataSet DsReasonCode;

        private static frmDisposition instance = null;

        public static frmDisposition getInstance()
        {
            if (instance == null)
            {
                instance = new frmDisposition();

            }
            return instance;
        }

        private frmDisposition()
        {
            InitializeComponent();
            // load up finish code
            this.DsReasonCode = new System.Data.DataSet();
            string DsFile = Program.ApplicationPath + "\\ic_reason_code.xml";
            if (System.IO.File.Exists(DsFile) == true)
            {
                this.DsReasonCode.ReadXml(DsFile, XmlReadMode.InferSchema);
                if (this.DsReasonCode != null)
                {
                    if (this.DsReasonCode.Tables.Count > 0)
                    {
                        for (int i = 0; i < this.DsReasonCode.Tables[0].Rows.Count; i++)
                        {
                            this.finishcode_combobox.Items.Add(this.DsReasonCode.Tables[0].Rows[i]["finish_code"].ToString());
                        }
                    }
                }
            }
            if (this.finishcode_combobox.Items.Count > 0)
            {
                this.finishcode_combobox.SelectedIndex = 0;
            }
        }

        private void save_button_Click(object sender, EventArgs e)
        {
            // save data and callback to main form to save stuff
            Program.MainDashboard.disposition_invoke(this.finishcode_combobox.Text , e);
            this.Close();
        }

        private void frmDisposition_FormClosed(object sender, FormClosedEventArgs e)
        {
            // message FormMain to change the flow accordingly
            // some finish code needs to open a schedule callback form
        }
    }
}
