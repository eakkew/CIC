using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ININ.IceLib;
using ININ.IceLib.Connection;
using ININ.IceLib.Interactions;
using ININ.IceLib.People;
using ININ.IceLib.Dialer;
using ININ.IceLib.UnifiedMessaging;
using Locus.Control;

namespace CIC
{
    public partial class frmMailDocView : Form
    {
        private System.Collections.ArrayList mEmailResponseList = null;
        private System.Data.DataSet DsMailList = null;
        //private ININ.IceLib.Interactions.EmailResponse emailresponse = null;
        private ININ.IceLib.Interactions.Interaction mInteraction = null;

        public frmMailDocView()
        {
            InitializeComponent();
        }

        public frmMailDocView(System.Collections.ArrayList EmailResponseList, ININ.IceLib.Interactions.Interaction interaction)
        {
            InitializeComponent();
            int i = 0;
            if (EmailResponseList != null)
            {
                this.DsMailList = new System.Data.DataSet();
                DsMailList.DataSetName = "Mailing List";
                System.Data.DataTable dt = new System.Data.DataTable("Mail List");
                dt.Columns.Add("FROM");
                dt.Columns.Add("TO");
                dt.Columns.Add("CC");
                dt.Columns.Add("BCC");
                dt.Columns.Add("SUBJECT");
                dt.Columns.Add("ATTACT");
                dt.Columns.Add("BODY");
                for (i = 0; i < EmailResponseList.Count; i++)
                {
                    System.Data.DataRow dr = dt.NewRow();
                    dr["FROM"] = ((ININ.IceLib.Interactions.EmailResponse)EmailResponseList[i]).Sender.DisplayName.ToString();
                    dr["TO"]  = ((ININ.IceLib.Interactions.EmailResponse)EmailResponseList[i]).ToRecipients[0].DisplayName.ToString();
                    //dr["CC"];
                    //dr["BCC"];
                    dr["SUBJECT"] = ((ININ.IceLib.Interactions.EmailResponse)EmailResponseList[i]).Subject;
                    //dr["ATTACT"];
                    dr["BODY"] = ((ININ.IceLib.Interactions.EmailResponse)EmailResponseList[i]).Body;
                }
                this.DsMailList.Tables.Add(dt);

                this.grvMailingList.DataSource = this.DsMailList;
                this.mEmailResponseList = EmailResponseList;
            }
        }
       
        private void ShowDocumentDetail()
        {
            if(this.mInteraction != null)
            {
                //this.mEmailResponseList.
            }
        }

        public frmMailDocView(System.Collections.ArrayList EmailResponseList)
        {
            InitializeComponent();

            if (EmailResponseList != null)
            {
                this.DsMailList = new System.Data.DataSet();
                DsMailList.DataSetName = "Mailing List";
                System.Data.DataTable dt = new System.Data.DataTable("Mail List");
                dt.Columns.Add("FROM");
                dt.Columns.Add("TO");
                dt.Columns.Add("CC");
                dt.Columns.Add("BCC");
                dt.Columns.Add("SUBJECT");
                dt.Columns.Add("ATTACT");
                dt.Columns.Add("BODY");
                for (int i = 0; i < EmailResponseList.Count; i++)
                {
                    System.Data.DataRow dr = dt.NewRow();
                    dr["FROM"] = ((ININ.IceLib.Interactions.EmailResponse)EmailResponseList[i]).Sender.DisplayName.ToString();
                    dr["TO"]  = ((ININ.IceLib.Interactions.EmailResponse)EmailResponseList[i]).ToRecipients[0].DisplayName.ToString();
                    //dr["CC"];
                    //dr["BCC"];
                    dr["SUBJECT"] = ((ININ.IceLib.Interactions.EmailResponse)EmailResponseList[i]).Subject;
                    //dr["ATTACT"];
                    dr["BODY"] = ((ININ.IceLib.Interactions.EmailResponse)EmailResponseList[i]).Body;
                }
                this.DsMailList.Tables.Add(dt);

                this.grvMailingList.DataSource = this.DsMailList;
                this.mEmailResponseList = EmailResponseList;
            }
        }

        public frmMailDocView(ININ.IceLib.Interactions.EmailResponse emailResponse)
        {
            InitializeComponent();
            if (emailResponse != null)
            {
                this.txtSender.Text = emailResponse.Sender.DisplayName.ToString();
                this.txtTo.Text = emailResponse.ToRecipients[0].DisplayName.ToString();
                this.txtCC.Text = emailResponse.CcRecipients[0].DisplayName.ToString();
                this.txtSubject.Text = emailResponse.Subject.ToString();
                this.rtfBody.Text = emailResponse.Body.ToString();
            }
        }

        private void ToToolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void grvMailingList_SelectionChanged(object sender, EventArgs e)
        {
            //DsMailList
        }

        
    }
}
