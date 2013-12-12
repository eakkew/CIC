using ININ.IceLib.Connection;
using ININ.IceLib.Dialer;
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
    struct callParameter
    {
        public CallCompletionParameters param;
        public string number;
    }

    public partial class frmDisposition : Form
    {
        private DataSet DsReasonCode;

        private static frmDisposition instance = null;
        private string dialerNumber;
        private Session IC_Session;

        public static frmDisposition getInstance(Session session, string number)
        {
            if (instance == null || instance.IsDisposed)
            {
                instance = new frmDisposition(session, number);

            }
            return instance;
        }

        private frmDisposition(Session session, string number)
        {
            InitializeComponent();
            // load up finish code
            this.dialerNumber = number;
            this.IC_Session = session;
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
            this.ActiveControl = finishcode_combobox;
        }

        private void save_button_Click(object sender, EventArgs e)
        {
            // save data and callback to main form to save stuff
            ReasonCode sReasoncode = Util.GetReasonCode(this.finishcode_combobox.Text);
            callParameter callback = new callParameter();
            if (sReasoncode == ReasonCode.Scheduled)
            {
                frmSchedule schedule = frmSchedule.getInstance(dialerNumber);
                schedule.ShowDialog();
                if (schedule.validateTime())
                {
                    callback.param = new CallCompletionParameters(
                        sReasoncode, this.finishcode_combobox.Text, 
                        schedule.getScheduledTime(), this.IC_Session.UserId, false
                    );
                    callback.number = schedule.getNumber();
                }
                else
                {
                    return;
                }
            }
            else
            {
                callback.param = new ININ.IceLib.Dialer.CallCompletionParameters(
                    sReasoncode, sReasoncode.ToString());
            }
            Program.MainDashboard.disposition_invoke(callback, e);
            this.Close();
        }

        private void frmDisposition_FormClosed(object sender, FormClosedEventArgs e)
        {
            // message FormMain to change the flow accordingly
            // some finish code needs to open a schedule callback form
        }
    }
}
