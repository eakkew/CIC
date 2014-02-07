﻿using ININ.IceLib.Connection;
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
        private float elaspedTime;
        private string[] callerList;

        public static frmDisposition getInstance(Session session, string number, string calltype = "preview")
        {
            if (instance == null || instance.IsDisposed)
            {
                instance = new frmDisposition(session, number, calltype.ToLower());
            }
            return instance;
        }

        private frmDisposition(Session session, string number, string calltype)
        {
            InitializeComponent();
            // load up finish code
            this.TimedOutInfoLabel.Text = "";
            this.dialerNumber = number;
            this.IC_Session = session;
            this.timer1.Start();
            elaspedTime = 0.0f;
            this.ActiveControl = this.finishcode_combobox;
            this.DsReasonCode = new System.Data.DataSet();
            string DsFile = Program.ApplicationPath + "\\";
            if (calltype == "predictive")
            {
                DsFile += global::CIC.Properties.Settings.Default.PredictiveReasonCode;
            }
            else if (calltype == "power" || calltype == "powered")
            {
                DsFile += global::CIC.Properties.Settings.Default.PowerReasonCode;
            }
            else
            {
                DsFile += global::CIC.Properties.Settings.Default.PreviewReasonCode;
            }

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
            callback.number = "";
            // check finish code for conditional scheduling
            if (this.finishcode_combobox.Text.ToLower() == "call loss" ||
                sReasoncode == ReasonCode.RemoteHangup)
            {
                callback.param = new CallCompletionParameters(
                    sReasoncode, this.finishcode_combobox.Text,
                    Util.getDateTimeNowPlusOffset().ToUniversalTime() , this.IC_Session.UserId, false, ININ.IceLib.Dialer.Enums.TimeReference.UTC
                );
            }
            else if (sReasoncode == ReasonCode.WrongParty)
            {
                callback.number = getNextNumber();
                if (callback.number == "")
                {
                    callback.param = new CallCompletionParameters(
                        ReasonCode.Success, "LastWrongPerson");
                }
                else
                {
                    callback.param = new CallCompletionParameters(
                        sReasoncode, this.finishcode_combobox.Text,
                        Util.getDateTimeNowPlusOffset().ToUniversalTime(), this.IC_Session.UserId, false, ININ.IceLib.Dialer.Enums.TimeReference.UTC
                    );
                }
            }
            else if (sReasoncode == ReasonCode.Scheduled)
            {
                frmSchedule schedule = frmSchedule.getInstance(dialerNumber);
                timer1.Stop();
                schedule.updateDialerNumber(dialerNumber);
                schedule.ShowDialog();
                timer1.Start();
                if (schedule.validateTime())
                {
                    callback.param = new CallCompletionParameters(
                        sReasoncode, this.finishcode_combobox.Text, 
                        schedule.getScheduledTime(), this.IC_Session.UserId, false, ININ.IceLib.Dialer.Enums.TimeReference.UTC
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
            this.finishcode_combobox.SelectedIndex = 0;
            this.timer1.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            elaspedTime += (float)timer1.Interval / 1000;
            float timeout = global::CIC.Properties.Settings.Default.DispositionTimeOut;
            if (elaspedTime >= timeout)
            {
                this.TimedOutInfoLabel.Text = "Auto Disposition activating";
                callParameter callback = new callParameter();
                callback.param = new CallCompletionParameters(ReasonCode.Success, "Auto Closed");
                Program.MainDashboard.request_break();
                Program.MainDashboard.disposition_invoke(callback, e);
                timer1.Stop();
                elaspedTime = 0.0f;
                this.Close();
                // cleanup
            }
            else if (elaspedTime >= timeout * 3 / 4)
            {
                this.TimedOutInfoLabel.ForeColor = Color.Red;
                this.TimedOutInfoLabel.Text = "Auto Disposition will be commenced in: " + (timeout - elaspedTime).ToString("0.0");
            }
            else if (elaspedTime >= timeout * 1 / 4)
            {
                this.TimedOutInfoLabel.ForeColor = Color.Black;
                this.TimedOutInfoLabel.Text = "Auto Disposition will be commenced in: " + (timeout - elaspedTime).ToString("0.0");
            }
            else
            {
                this.TimedOutInfoLabel.Text = "";
            }
        }

        public void updateDialerNumber(string dialingNumber)
        {
            this.dialerNumber = dialingNumber;
        }

        public void updateCallerList(string[] cList)
        {
            if (cList != null)
                this.callerList = cList;
        }

        private string getNextNumber()
        {
            string ret = "";
            if (this.callerList.Count() <= 1)
                return "";

            for (int i = 0; i < this.callerList.Count() - 1; i++)
            {
                if (this.dialerNumber == this.callerList[i])
                    return this.callerList[i + 1];
            }

            return ret;
        }

        private void frmDisposition_Load(object sender, EventArgs e)
        {
            this.TimedOutInfoLabel.Text = "";
            this.timer1.Start();
            elaspedTime = 0.0f;
            this.ActiveControl = this.finishcode_combobox;
        }
    }
}
