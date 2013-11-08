using ININ.IceLib.Dialer;
using ININ.IceLib.Interactions;
using ININ.IceLib.People;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CIC
{
    public enum CallerType
    {
        TimerCalled,
        ButtonClicked
    }

    public enum FormMainState
    {
        Preview,
        Calling,
        Disconnect,
        Hold,
        Mute,
        Break,
        ManualCall,
        Loggedout,
        None,

    };
    //private bool IsLoggedIntoDialer = false;

    public partial class FormMain : Form
    {
        private bool break_requested { get; set; }
        private bool BlindTransferFlag { get; set; }
        private bool IsLoggedIntoDialer { get; set; }
        private bool IsActiveConnection { get; set; }
        private bool SwapPartyFlag { get; set; }
        private string[] InteractionAttributes { get; set; }
        private ArrayList InteractionList { get; set; }

        private DataSet DsReasonCode { get; set; }
        private StatusMessageList AllStatusMessageList { get; set; }
        private UserStatusList AllStatusMessageListOfUser { get; set; }
        private UserStatus CurrentUserStatus { get; set; }
        private StatusMessageDetails AvailableStatusMessageDetails { get; set; }
        
        private NameValueCollection mDialerData { get; set; }
        private ICWorkFlow IcWorkFlow = null;
        private WorkgroupDetails ActiveWorkgroupDetails { get; set; }
        private PeopleManager mPeopleManager { get; set; }
        private ININ.IceLib.Connection.Session IC_Session = null;
        private ININ.IceLib.Dialer.DialerCallInteraction ActiveDialerInteraction = null;
        private ININ.IceLib.Dialer.DialerSession DialerSession = null;
        private ININ.IceLib.Interactions.InteractionsManager NormalInterationManager = null;
        private FormMainState prev_state = FormMainState.Preview;
        private FormMainState current_state = FormMainState.Preview;
        private InteractionState StrConnectionState = InteractionState.None;

        private static Interaction ActiveNormalInteraction { get; set; }

        private float timer;
        private string calling_phone = "0881149998";

        public bool transfer_complete = false;
 
        public FormMainState req_state_change = FormMainState.None;
       
        public FormMain()
        {
            InitializeComponent();
            this.IsActiveConnection = true; // FIXME: remove the placeholder
            this.IsLoggedIntoDialer = true; // FIXME: remove the placeholder
        }

        private void reset_timer()
        {
            timer1.Stop();
            timer = 10.0f;
        }

        public void login_workflow()
        {
            this.workflow_button_Click(null, EventArgs.Empty);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer = 10.0f;
            state_info_label.Text = "Next Calling Number: " + calling_phone;
        }

        private void workflow_button_Click(object sender, EventArgs e)
        {
            if (this.IsActiveConnection)
            {
                frmWorkflow workflow = new CIC.frmWorkflow(this.IC_Session);
                workflow.Show();
            }
            else
            {
                // TODO: change the state back to no connected.
            }
        }

        private void call_button_Click(object sender, EventArgs e)
        {   
            //change state from workflow.
            name1_panel.BackColor = Color.Yellow;
            reset_timer();
            state_info_label.Text = "Calling: " + calling_phone;

            // make a call or pickup
            placecall_or_pickup();
                
            state_change(FormMainState.Calling);
        }

        private void disconnect_button_Click(object sender, EventArgs e)
        {
            state_info_label.Text = "Disconnected from: " + calling_phone;
            try
            {
                if (this.ActiveDialerInteraction.IsConnected)
                {
                    frmDisposition disposition = new frmDisposition();
                    disposition.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                string output = String.Format("Something really bad happened: {0}", ex.Message);
                MessageBox.Show(output, "CIC Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            state_change(FormMainState.Disconnect);
        }

        private void hold_button_Click(object sender, EventArgs e)
        {
            if (this.IsLoggedIntoDialer)
            {
                if (this.ActiveDialerInteraction != null)
                {
                    if (this.ActiveDialerInteraction.IsMuted)
                    {
                        this.ActiveDialerInteraction.Mute(false);
                    }
                    this.ActiveDialerInteraction.Hold(!this.ActiveDialerInteraction.IsHeld);
                }
                else if (ActiveNormalInteraction != null)
                {
                    if (ActiveNormalInteraction.IsMuted)
                    {
                        ActiveNormalInteraction.Mute(false);
                    }
                    ActiveNormalInteraction.Hold(!ActiveNormalInteraction.IsHeld);
                }
                state_change(FormMainState.Hold);
            }
        }

        private void mute_button_Click(object sender, EventArgs e)
        {
            if (this.IsLoggedIntoDialer)
            {
                if (this.ActiveDialerInteraction != null)
                {
                    if (this.ActiveDialerInteraction.IsHeld)
                    {
                        this.ActiveDialerInteraction.Hold(false);
                    }
                    this.ActiveDialerInteraction.MuteAsync(!this.ActiveDialerInteraction.IsMuted, MuteCompleted, null);
                }
                else if (ActiveNormalInteraction != null)
                {
                    if (ActiveNormalInteraction.IsHeld)
                    {
                        ActiveNormalInteraction.Hold(false);
                    }
                    ActiveNormalInteraction.MuteAsync(!ActiveNormalInteraction.IsMuted, MuteCompleted, null);
                }
                state_change(FormMainState.Mute);
            }
        }

        private void transfer_button_Click(object sender, EventArgs e)
        {
            transfer_complete = false;
            frmTransfer transfer = new frmTransfer();
            transfer.ShowDialog();
            // check if there is still a connection or if transfer complete.
            if (transfer_complete)
            {
                state_change(FormMainState.Disconnect);
            }
        }

        private void conference_button_Click(object sender, EventArgs e)
        {
            frmConference conference = new frmConference();
            conference.ShowDialog();
        }

        private void manual_call_button_Click(object sender, EventArgs e)
        {
            if (this.IsLoggedIntoDialer)
            {
                //MessageBox.Show("Please logged into dialer first");
                frmManualCall manualCall = new frmManualCall(IC_Session, NormalInterationManager);
                manualCall.ShowDialog();
                state_change(FormMainState.ManualCall);
            }
            else
            {
                MessageBox.Show("Please logged into Dialer first");  
                /*
                 * TODO : Change state to match manual call condition
                 * 
                 */
            } 
            
        }

        private void break_button_Click(object sender, EventArgs e)
        {
            //state_change(FormMainState.Break);
            break_requested = true;
        }

        private void endbreak_button_Click(object sender, EventArgs e)
        {
            state_change(FormMainState.Preview);
        }

        private void logout_workflow_button_Click(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::LogoutToolStripMenuItem_Click(): ";
            //Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (this.IsLoggedIntoDialer)
                {
                    // FIX ME: check the state of calling
                    //if (/*this.CallStateToolStripStatusLabel.Text.ToLower().Trim() == "n/a"*/ true)
                    if (this.current_state == FormMainState.Disconnect)
                    {
                        this.LogoutGranted(sender, e);      //No call object from this campaign;permit to logging out.
                    }
                    else
                    {
                        if (this.ActiveDialerInteraction != null)
                        {
                            // TODO: validate the condition of log out request while not on break
                            //if (/*this.RequestBreakToolStripButton.Text.Trim() != "End Break"*/ false)
                            if (!this.break_requested)
                            {
                                this.break_requested = true;
                                this.break_button_Click(sender, e);               //wait for breakgrant
                                this.ActiveDialerInteraction.DialerSession.RequestLogout();
                            }
                            else
                            {
                                this.LogoutGranted(sender, e);     //already breakpermit to logging out.
                            }
                        }
                    }
                }
                else
                {
                    disconnect_normal_interaction();
                    disconnect_IC_session();
                    // TODO: disable functions as dialer is not connect
                }
                
                //Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void disconnect_IC_session()
        {
            if (this.IC_Session != null)
            {
                this.IC_Session.Disconnect();
                this.IC_Session = null;
            }
        }

        private static void disconnect_normal_interaction()
        {
            if (ActiveNormalInteraction != null)
            {
                ActiveNormalInteraction.Disconnect();
                ActiveNormalInteraction = null;
            }
        }
        
        public void workflow_invoke(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::WorkflowToolStripMenuItem_Click()::";
            //Tracing.TraceStatus(scope + "Starting.");
            try
            {
                //Tracing.TraceStatus(scope + "Logging into workflow. UserId=" + this.IC_Session.UserId + ", StationId=" + this.IC_Session.GetStationInfo().Id);
                this.IcWorkFlow = new CIC.ICWorkFlow(CIC.Program.DialingManager);
                this.DialerSession = IcWorkFlow.LogIn(((ToolStripMenuItem)sender).Text);
                this.IsLoggedIntoDialer = this.IcWorkFlow.LoginResult;
                if (this.IsLoggedIntoDialer == true)
                {
                    this.RegisterHandlers();

                    this.InitializeStatusMessageDetails();
                    //Tracing.TraceStatus(scope + "Completed.");
                    // TODO: change state to something
                }
                else
                {
                    // TODO: goto logout state
                    //Tracing.TraceStatus(scope + "WorkFlow [" + ((ToolStripMenuItem)sender).Text + "] logon Fail.Please try again.");
                }
                this.ShowActiveCallInfo(); // TODO: change to state change
            }
            catch (System.Exception ex)
            {
                // TODO: goto logout state
                //Tracing.TraceStatus(scope + "Error info.Logon to Workflow[" + ((ToolStripMenuItem)sender).Text + "] : " + ex.Message);
                //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info.Logon to Workflow[" + ((ToolStripMenuItem)sender).Text + "] : " + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }  
        }

        private void ShowActiveCallInfo()
        {
            string scope = "CIC::frmMain::ShowActiveCallInfo()::";
            //Tracing.TraceStatus(scope + "Starting.");
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new MethodInvoker(ShowActiveCallInfo));
            }
            else
            {
                if (this.IsLoggedIntoDialer)
                {
                    if (this.ActiveDialerInteraction == null)
                    {
                        // TODO: disable direction|calltype|campaignID|queuename|number|callstate|callID
                        //this.ActiveConferenceInteraction = null;
                        //this.Reset_ActiveCallSelPopMenu();
                    }
                    else
                    {
                        if (this.ActiveDialerInteraction.DialingMode == DialingMode.Regular)
                        {
                            if (global::CIC.Properties.Settings.Default.AutoAnswer == true)
                            {
                                // TODO: pickup call
                                //this.PickupToolStripButton_Click(this, new EventArgs());
                            }
                        }
                        /* TODO: show call ID
                         *       set call type to campaign call
                         *       set call state
                         *       set dialer number
                         */
                        //this.CallIdToolStripStatusLabel.Text = this.ActiveDialerInteraction.CallIdKey.ToString().Trim();
                        //this.DirectiontoolStripStatus.Text = this.ActiveDialerInteraction.Direction.ToString();
                        //this.CallTypeToolStripStatusLabel.Text = "Campaign Call";
                        //this.CampaignIdToolStripStatusLabel.Text = this.mDialerData[Properties.Settings.Default.Preview_Campaign_ATTR];
                        //this.QueueNameToolStripStatusLabel.Text = this.mDialerData[Properties.Settings.Default.Preview_QueueName_ATTR];
                        //this.NumberToolStripStatusLabel.Text = this.GetDialerNumber();
                        //this.CallStateToolStripStatusLabel.Text = this.ActiveDialerInteraction.StateDescription.ToString();
                    }
                } 
                else
                {
                    if (ActiveNormalInteraction == null)
                    {
                        // TODO: disable direction|calltype|campaignID|queuename|number|callstate|callID
                        //this.ActiveConferenceInteraction = null;
                        //this.Reset_ActiveCallSelPopMenu();
                    }
                    else
                    {
                        if (this.BlindTransferFlag == true)
                        {
                            // TODO: disable direction|calltype|campaignID|queuename|number|callstate|callID
                            //this.ActiveConferenceInteraction = null;
                            //this.Reset_ActiveCallSelPopMenu();
                        }
                        else
                        {
                            switch (ActiveNormalInteraction.State)
                            {
                                case InteractionState.None:
                                    break;
                                case InteractionState.Held:
                                    if (this.SwapPartyFlag == true)
                                    {
                                        this.SetActiveCallInfo();
                                        this.ShowActiveCallInfo();
                                    }
                                    else
                                    {
                                        // TODO: set info as below
                                        if (ActiveNormalInteraction.IsMuted == true)
                                            this.toolStripStatus.Text = "Muted";
                                        else
                                            this.toolStripStatus.Text = ActiveNormalInteraction.State.ToString();
                                        //this.DirectiontoolStripStatus.Text = ActiveNormalInteraction.Direction.ToString();
                                        //this.CallTypeToolStripStatusLabel.Text = ActiveNormalInteraction.InteractionType.ToString();
                                        //this.CampaignIdToolStripStatusLabel.Text = "Non-campaign Call";
                                        //this.QueueNameToolStripStatusLabel.Text = ActiveNormalInteraction.WorkgroupQueueName.ToString();
                                        //this.NumberToolStripStatusLabel.Text = ActiveNormalInteraction.RemoteDisplay.ToString();
                                    }
                                    //this.CallIdToolStripStatusLabel.Text = this.ActiveNormalInteraction.CallIdKey.ToString().Trim();
                                    break;
                                case InteractionState.Connected:
                                    if (ActiveNormalInteraction.IsMuted == true)
                                        this.toolStripStatus.Text = "Muted";
                                    else
                                        this.toolStripStatus.Text = ActiveNormalInteraction.State.ToString();
                                    // TODO: set info as below
                                    //this.DirectiontoolStripStatus.Text = ActiveNormalInteraction.Direction.ToString();
                                    //this.CallTypeToolStripStatusLabel.Text = ActiveNormalInteraction.InteractionType.ToString();
                                    //this.CampaignIdToolStripStatusLabel.Text = "Non-campaign Call";
                                    //this.QueueNameToolStripStatusLabel.Text = ActiveNormalInteraction.WorkgroupQueueName.ToString();
                                    //this.NumberToolStripStatusLabel.Text = ActiveNormalInteraction.RemoteDisplay.ToString();
                                    this.state_info_label.Text = "calling: " + ActiveNormalInteraction.CallIdKey.ToString().Trim();
                                    break;
                                default:
                                    // TODO: set info as below
                                    //this.DirectiontoolStripStatus.Text = ActiveNormalInteraction.Direction.ToString();
                                    //this.CallTypeToolStripStatusLabel.Text = ActiveNormalInteraction.InteractionType.ToString();
                                    //this.CampaignIdToolStripStatusLabel.Text = "Non-campaign Call";
                                    //this.QueueNameToolStripStatusLabel.Text = ActiveNormalInteraction.WorkgroupQueueName.ToString();
                                    //this.NumberToolStripStatusLabel.Text = ActiveNormalInteraction.RemoteDisplay.ToString();
                                    this.toolStripStatus.Text = ActiveNormalInteraction.State.ToString();
                                    this.state_info_label.Text = "calling: " + ActiveNormalInteraction.CallIdKey.ToString().Trim();
                                    break;
                            }
                        }
                     }
                }
                //this.SetInfoBarColor(); // TODO: update playing sound
                update_conference_status();
            }
        }

        private void update_conference_status()
        {
            // TODO update this method
            string scope = "CIC::FormMain::update_conference_status()::";
            //this.Set_ConferenceToolStrip(); // TODO: update conference status
                //
                //>>>>>>>> from here
            //if (this.InteractionList != null && this.InteractionList.Count <= 0)
            //{
            //        this.ActiveConsultInteraction = null;
            //        this.IsActiveConference_flag = false;
            //        this.ActiveConferenceInteraction = null;
            //        if (ActiveNormalInteraction != null)
            //        {
            //            ActiveNormalInteraction.Disconnect();
            //            ActiveNormalInteraction = null;
            //        }
            //        if (this.IsLoggedIntoDialer != true)
            //        {
            //            this.ActiveDialerInteraction = null;
            //        }
            //}
            // <<<<<<<< to here
            //Tracing.TraceStatus(scope + "Completed.");
        }

        private void SetActiveCallInfo()
        {
            string scope = "CIC::frmMain::SetActiveCallInfo()::";
            //Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (this.InteractionList != null)
                {
                    foreach (ININ.IceLib.Interactions.Interaction CurrentInteraction in this.InteractionList)
                    {
                        if (CurrentInteraction.State == InteractionState.Connected)
                        {
                            ActiveNormalInteraction = CurrentInteraction;
                            this.SwapPartyFlag = false;
                            break;
                        }
                    }
                    
                }
                //Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void InitializeStatusMessageDetails()
        {
            int iIndex = 0;
            int AvailableIndex = 0;
            string sIconName;
            string sIconPath = "";
            System.Drawing.Icon Status_icon = null;
            //this.AllStatusMessageList = null;
            //this.AllStatusMessageListOfUser = null;
            UserStatusUpdate statusUpdate = null;
            string scope = "CIC::frmMain::InitializeStatusMessageDetails()::";
            //Tracing.TraceStatus(scope + "Starting.");
            //this.imgcmbAgentStatus.ImageList.Images.Clear();
            //this.imgcmbAgentStatus.Items.Clear();
            try
            {
                if (this.mPeopleManager != null)
                {
                    this.AllStatusMessageList = new StatusMessageList(this.mPeopleManager);
                    this.AllStatusMessageListOfUser = new UserStatusList(this.mPeopleManager);
                    this.AllStatusMessageListOfUser.WatchedObjectsChanged += new EventHandler<WatchedObjectsEventArgs<UserStatusProperty>>(AllStatusMessageListOfUser_WatchedObjectsChanged);
                    //dusers = { Program.DialingManager.Session.UserId };   //Make value to array 
                    if (this.IsLoggedIntoDialer)
                    {
                        string[] dusers = { Program.DialingManager.Session.UserId };
                        this.AllStatusMessageListOfUser.StartWatching(  dusers );
                    }
                    else
                    {
                        string[] dusers = { this.IC_Session.UserId };
                        this.AllStatusMessageListOfUser.StartWatching(dusers);
                    }
                    this.CurrentUserStatus = this.AllStatusMessageListOfUser.GetUserStatus(Program.DialingManager.Session.UserId);
                    // TODO: recheck - potentially not using this
                    //this.imgcmbAgentStatus.ImageList = this.imsLstServerStatus;
                    //this.CmbImgAgentStatus.ImageList = this.imsLstServerStatus;
                    //this.imsLstServerStatus.Images.Clear();
                    sIconPath = CIC.Program.ResourcePath;
                    this.AllStatusMessageList.StartWatching();
                    // TODO: recheck - potentially not using this
                    //foreach (StatusMessageDetails status in this.AllStatusMessageList.GetList())
                    //{
                    //    sIconName = Util.GetFilenameFromFilePath(status.IconFileName.ToString());
                    //    sIconPath += sIconName;
                    //    if (System.IO.File.Exists(sIconPath) == true)
                    //    {
                    //        Status_icon = new System.Drawing.Icon(sIconPath);
                    //        this.imsLstServerStatus.Images.Add(status.MessageText, Status_icon);
                    //    }
                    //    else
                    //    {
                    //        this.imsLstServerStatus.Images.Add(status.MessageText, status.Icon);
                    //    }
                    //    if (status.MessageText.ToLower().Trim() == "available")
                    //    {
                    //        AvailableStatusMessageDetails = status;
                    //        AvailableIndex = iIndex;
                    //    }
                    //    if (status.MessageText.ToLower().Trim() == "do not disturb")
                    //    {
                    //        DoNotDisturbStatusMessageDetails = status;
                    //    }
                    //    this.imgcmbAgentStatus.Items.Add(new ImageComboBoxItem(status.MessageText, iIndex));
                    //    iIndex++;
                    //    //Tracing.TraceNote(scope + "Id=" + status.Id + ", MessageText=" + status.MessageText);
                    //}
                }

                //Set Current User Status Display 
                if (this.mPeopleManager != null)
                {
                    statusUpdate = new UserStatusUpdate(this.mPeopleManager);
                    if (this.CurrentUserStatus != null)
                    {
                        if (global::CIC.Properties.Settings.Default.AutoResetUserStatus == true)
                        {
                            statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                            statusUpdate.UpdateRequest();
                            //this.imgcmbAgentStatus.SetMessage(this.CurrentUserStatus.StatusMessageDetails.MessageText);
                        }
                        else
                        {
                            if (this.CurrentUserStatus.StatusMessageDetails.IsSelectableStatus == true)
                            {
                                statusUpdate.StatusMessageDetails = this.CurrentUserStatus.StatusMessageDetails;
                                statusUpdate.UpdateRequest();
                            }
                            else
                            {
                                if (this.IsLoggedIntoDialer == true)
                                {
                                    statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                                    statusUpdate.UpdateRequest();
                                }
                                else
                                {
                                    //Display last user status.
                                }
                            }
                            //if (this.imgcmbAgentStatus.Items.Count > 0)
                            //{
                            //    this.imgcmbAgentStatus.SetMessage(this.CurrentUserStatus.StatusMessageDetails.MessageText);
                            //}
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        // TODO: check the commented code and verify if we need it
        private void AllStatusMessageListOfUser_WatchedObjectsChanged(object sender, WatchedObjectsEventArgs<UserStatusProperty> e)
        {
            string scope = "CIC::frmMain::AllStatusMessageListOfUser_WatchedObjectsChanged()::";
            //Tracing.TraceStatus(scope + "Starting.");
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new EventHandler<WatchedObjectsEventArgs<UserStatusProperty>>(AllStatusMessageListOfUser_WatchedObjectsChanged), new object[] { sender, e });
            }
            else
            {
                try
                {
                    if (e.Changed.Count > 0)
                    {
                        //Tracing.TraceNote(scope + "Watched object changed");
                        ReadOnlyCollection<UserStatusProperty> userStatusProperties;
                        if (e.Changed.TryGetValue(this.IC_Session.UserId, out userStatusProperties))
                        {
                            foreach (ININ.IceLib.People.UserStatusProperty property in userStatusProperties)
                            {
                                switch (property)
                                {
                                    case UserStatusProperty.ICServers:
                                        break;
                                    case UserStatusProperty.UserId:
                                        break;
                                    case UserStatusProperty.StatusMessageDetails:
                                        if (this.mPeopleManager != null)
                                        {
                                            if (this.CurrentUserStatus != null)
                                            {
                                                ININ.IceLib.People.UserStatusUpdate statusUpdate = new UserStatusUpdate(this.mPeopleManager);
                                                if (statusUpdate != null)
                                                {
                                                    if (this.CurrentUserStatus.StatusMessageDetails.IsSelectableStatus == true)
                                                    {
                                                        //if (this.imgcmbAgentStatus.Items.Count > 0)
                                                        //{
                                                        //    if (this.imgcmbAgentStatus.SelectedIndex >= 0)
                                                        //    {
                                                        //        string DisplayStatus = this.imgcmbAgentStatus.GetMeaasageText(this.imgcmbAgentStatus.SelectedIndex);
                                                        //        if (this.CurrentUserStatus.StatusMessageDetails.MessageText.Trim() == DisplayStatus.Trim())
                                                        //        {
                                                        //            statusUpdate.StatusMessageDetails = this.CurrentUserStatus.StatusMessageDetails;
                                                        //            statusUpdate.UpdateRequest();
                                                        //        }
                                                        //    }
                                                        //}
                                                        //this.toolStriplblAdditionalStatus.Visible = false;
                                                    }
                                                    else
                                                    {
                                                        //this.toolStriplblAdditionalStatus.Text = this.CurrentUserStatus.StatusMessageDetails.MessageText;
                                                        //this.toolStriplblAdditionalStatus.Image = (Image)this.CurrentUserStatus.StatusMessageDetails.Icon.ToBitmap();
                                                        //if (this.IsLoggedIntoDialer != true)
                                                        //{
                                                        //    if (this.CurrentUserStatus.StatusMessageDetails.MessageText.ToLower().Trim() == global::CIC.Properties.Settings.Default.ACDAvailableTalkMsg.ToLower().Trim())
                                                        //    {
                                                        //        this.toolStriplblAdditionalStatus.Visible = true;
                                                        //    }
                                                        //    else if (this.CurrentUserStatus.StatusMessageDetails.MessageText.ToLower().Trim() == global::CIC.Properties.Settings.Default.ACDAgenNotAnsweringMsg.ToLower().Trim())
                                                        //    {
                                                        //        this.toolStriplblAdditionalStatus.Visible = true;
                                                        //    }
                                                        //}
                                                        //else
                                                        //{
                                                        //    this.toolStriplblAdditionalStatus.Visible = false;
                                                        //}
                                                    }
                                                }
                                                //bool bTemp = this.imgcmbAgentStatus.Enabled;
                                                //this.imgcmbAgentStatus.Enabled = true;
                                                //if (this.imgcmbAgentStatus.Items.Count > 0)
                                                //{
                                                //    this.imgcmbAgentStatus.SetMessage(this.CurrentUserStatus.StatusMessageDetails.MessageText);
                                                //}
                                                //this.imgcmbAgentStatus.Enabled = bTemp;
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    //Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        private void RegisterHandlers()
        {
            string scope = "CIC::MainForm::RegisterHandlers()::";
            //Tracing.TraceStatus(scope + "Starting.");
            try
            {
                this.DialerSession.PreviewCallAdded += new EventHandler<ININ.IceLib.Dialer.PreviewCallAddedEventArgs>(PreviewCallAdded);
                this.DialerSession.DataPop += new EventHandler<ININ.IceLib.Dialer.DataPopEventArgs>(DataPop);
                this.DialerSession.CampaignTransition += new EventHandler<CampaignTransistionEventArgs>(CampaignTransition);
                this.DialerSession.BreakGranted += new EventHandler(BreakGranted);
                this.DialerSession.LogoutGranted += new EventHandler(LogoutGranted);
                Program.mDialingManager.WorkflowStopped += new EventHandler<WorkflowStoppedEventArgs>(WorkflowStopped);
                Program.mDialingManager.WorkflowStarted += new EventHandler<WorkflowStartedEventArgs>(WorkflowStarted);
                //Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void exit_button_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void state_change(FormMainState state)
        {
            switch (state)
            {
                case FormMainState.Preview:
                    preview_state();
                    break;
                case FormMainState.Calling :
                    calling_state();
                    break;
                case FormMainState.ManualCall:
                    calling_state();
                    break;
                case FormMainState.Disconnect:
                    if (break_requested || prev_state == FormMainState.ManualCall)
                    {
                        break_requested = false;
                        break_state();
                    }
                    else
                        disconnect_state();
                    break;
                case FormMainState.Hold:
                    switch (current_state)
                    {
                        // case calling state -> change to hold state
                        case FormMainState.Calling:
                            hold_button.Text = "Unhold";
                            state_info_label.Text = "Hold call from: " + calling_phone;
                            break;
                        // case Mute state -> change to hold state.
                        case FormMainState.Mute:
                            hold_button.Text = "Unhold";
                            mute_button.Text = "Mute";
                            state_info_label.Text = "Hold call from: " + calling_phone;
                            break;
                        // case Hold state -> change to calling state
                        case FormMainState.Hold:
                            hold_button.Text = "Hold";
                            state_info_label.Text = "Continue call from: " + calling_phone;
                            state = FormMainState.Calling;
                            break;
                    }
                    hold_state();
                    break;
                case FormMainState.Mute:
                    switch (current_state)
                    {
                        // case calling state -> change to hold state
                        case FormMainState.Calling:
                            mute_button.Text = "Unmute";
                            state_info_label.Text = "Mute call from: " + calling_phone;
                            break;
                        // case Mute state -> change to hold state.
                        case FormMainState.Hold:
                            mute_button.Text = "Unmute";
                            hold_button.Text = "Hold";
                            state_info_label.Text = "Mute call from: " + calling_phone;
                            break;
                        // case Hold state -> change to calling state
                        case FormMainState.Mute:
                            mute_button.Text = "Mute";
                            state_info_label.Text = "Continue call from: " + calling_phone;
                            state = FormMainState.Calling;
                            break;
                    }
                    mute_state();
                    break;
                case FormMainState.Break:
                    break_state();
                    break;
                case FormMainState.Loggedout:
                    logged_out_state();
                    break;
            }
            prev_state = current_state;
            current_state = state;
            req_state_change = FormMainState.None;
        }

        private void reset_state()
        {
            workflow_button.Enabled = false;
            call_button.Enabled = false;
            disconnect_button.Enabled = false;
            hold_button.Enabled = false;
            mute_button.Enabled = false;
            transfer_button.Enabled = false;
            conference_button.Enabled = false;
            manual_call_button.Enabled = false;
            break_button.Enabled = false;
            endbreak_button.Enabled = false;
            logout_workflow_button.Enabled = false;
            exit_button.Enabled = false;
        }

        private void enable_all_button()
        {
            workflow_button.Enabled = true;
            call_button.Enabled = true;
            disconnect_button.Enabled = true;
            hold_button.Enabled = true;
            mute_button.Enabled = true;
            transfer_button.Enabled = true;
            conference_button.Enabled = true;
            manual_call_button.Enabled = true;
            break_button.Enabled = true;
            endbreak_button.Enabled = true;
            logout_workflow_button.Enabled = true;
            exit_button.Enabled = true;
        }
        /*
         * //TODO : seperate break case to break_request
         */
        private void preview_state()
        {
            // starts the next number in line
            timer1.Start();
            state_info_label.Text = "Next Calling Number: " + calling_phone;

            reset_state();
            call_button.Enabled = true;
        }

        private void calling_state()
        {
            reset_state();
            disconnect_button.Enabled = true;
            hold_button.Enabled = true;
            mute_button.Enabled = true;
            transfer_button.Enabled = true;
            conference_button.Enabled = true;
            break_button.Enabled = true; 
            
            // TODO hilight the calling name and number in the GUI
        }

        private void hold_state()
        {
            reset_state();
            disconnect_button.Enabled = true;
            hold_button.Enabled = true;
            mute_button.Enabled = true;
            transfer_button.Enabled = true;
            conference_button.Enabled = true;
        }

        private void  disconnect_state()
        {
            // TODO: rename typo enable_all_button()
            enable_all_button();
            disconnect_button.Enabled = false;
            hold_button.Enabled = false;
            mute_button.Enabled = false;
            transfer_button.Enabled = false;
            conference_button.Enabled = false;


            // calling a new number
            reset_timer();
        }

        private void mute_state()
        {
            reset_state();
            disconnect_button.Enabled = true;
            hold_button.Enabled = true;
            mute_button.Enabled = true;
            transfer_button.Enabled = true;
            conference_button.Enabled = true;
        }

        private void break_state()
        {
            reset_state();
            workflow_button.Enabled = true;
            manual_call_button.Enabled = true;
            endbreak_button.Enabled = true;
            logout_workflow_button.Enabled = true;
            exit_button.Enabled = true;
        }

        private void logged_out_state()
        {
            reset_state();
            workflow_button.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer -= (float)timer1.Interval / 1000;
            timer_info.Text = "Time until call: " + timer.ToString("F1");
            if (timer <= 0)
            {
                reset_timer();
                
                // make a call or pickup
                placecall(sender, e);
                
                state_change(FormMainState.Calling);
                state_info_label.Text = "Calling: " + calling_phone;
            }
        }

        public static void MakeCallCompleted(object sender,InteractionCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                ActiveNormalInteraction = e.Interaction;
            }
        }
        
        private void placecall_or_pickup()
        {
            bool mySwitch = true;
            if (!IsLoggedIntoDialer)
            {
                if (IsNormalInteractionAvailableForPickup())
                {
                    pickup();
                    return;
                }
            }
            if (ActiveDialerInteraction != null)
            {
                if (IsDialerInteractionAvailableForPickup() || ActiveDialerInteraction.IsMuted)
                {
                    pickup();
                    return;
                }
                else placecall(this, null);
            }
            //// default function
            //switch (mySwitch)
            //{
            //    case true:
            //    // TODO calling logic for calling
            //    // PlaceCallToolStripButton will enable when 
            //    // 1. WorkflowToolStripMenuItem_Click()
            //    // 2. EnabledDialerCallTools() and CallStateToolStripStatusLabel.Text == initializing
            //    placecall();
            //    break;
            //    case false:
            //    // TODO calling logic for pickup
            //    // PickupToolStripButton will enable when 
            //    // 1. EnabledNormalCallTools() and CallStateToolStripStatusLabel.Text == initializing | alerting | messaging | offering | held
            //    // 2. EnabledDialerCallTools() and CallStateToolStripStatusLabel.Text ==                alerting | messaging | offering | held | mute
            //    pickup();
            //    break;
            //}
        }

        private bool IsDialerInteractionAvailableForPickup()
        {
            return ActiveDialerInteraction.State == InteractionState.Alerting
                || ActiveDialerInteraction.State == InteractionState.Held
                || ActiveDialerInteraction.State == InteractionState.Messaging
                || ActiveDialerInteraction.State == InteractionState.Offering;
        }


        private bool IsNormalInteractionAvailableForPickup()
        {
            return ActiveNormalInteraction.State == InteractionState.Alerting
                || ActiveNormalInteraction.State == InteractionState.Held
                || ActiveNormalInteraction.State == InteractionState.Messaging
                || ActiveNormalInteraction.State == InteractionState.Offering;
        }

        //private void SetCallHistory(string phone)
        //{
        //    string scope = "CIC::frmMain::SetCallHistory()::";
        //    //Tracing.TraceStatus(scope + "Starting.");
        //    if (this.PhoneNumberToolStripTextBox.Text.Trim() != String.Empty)
        //    {
        //        this.PhoneNumberToolStripTextBox.Items.Add(this.PhoneNumberToolStripTextBox.Text);
        //    }
        //    //Tracing.TraceStatus(scope + "Completed.");
        //}



        /****************************************************
        *****************************************************
        ******************* The Logic Part ******************
        *****************************************************
        ****************************************************/

        private void WorkflowStarted(object sender, WorkflowStartedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void WorkflowStopped(object sender, WorkflowStoppedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CampaignTransition(object sender, CampaignTransistionEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DataPop(object sender, DataPopEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void PreviewCallAdded(object sender, PreviewCallAddedEventArgs e)
        {
            string scope = "CIC::MainForm::PreviewCallAdded()::";
            //Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (e.Interaction.IsWatching() != true)
                {
                    e.Interaction.AttributesChanged += new EventHandler<AttributesEventArgs>(DialerInteraction_AttributesChanged);
                    e.Interaction.StartWatching(this.InteractionAttributes);
                }
                this.ActiveDialerInteraction = e.Interaction;
                switch (e.Interaction.InteractionType)
                {
                    case InteractionType.Email:
                        break;
                    case InteractionType.Chat:
                        break;
                    case InteractionType.Callback:
                        this.Initialize_CallBack();
                        this.Initialize_ContactData();
                        this.ShowActiveCallInfo();
                        this.CrmScreenPop();
                        break;
                    case InteractionType.Call:
                        this.Initialize_ContactData();
                        this.ShowActiveCallInfo();
                        this.CrmScreenPop();
                        break;
                }
                //Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                //Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void CrmScreenPop()
        {
            throw new NotImplementedException();
        }

        private void Initialize_ContactData()
        {
            string scope = "CIC::MainForm::InitialContactData()::";
            //Tracing.TraceStatus(scope + "Starting.");
            int i = 0;
            System.Data.DataTable dt = new DataTable("ATTR_TABLE");
            dt.Columns.Add("id");
            dt.Columns.Add("attr_key");
            dt.Columns.Add("attr_value");
            if (this.ActiveDialerInteraction != null)
            {
                this.mDialerData = new NameValueCollection();
                this.mDialerData.Clear();
                foreach (KeyValuePair<string, string> pair in this.ActiveDialerInteraction.ContactData)
                {
                    this.mDialerData.Add(pair.Key.ToString().Trim(), pair.Value);
                    System.Data.DataRow dr = dt.NewRow();
                    dr["id"] = i++.ToString();
                    dr["attr_key"] = pair.Key.ToString();
                    dr["attr_value"] = pair.Value.ToString();
                    dt.Rows.Add(dr);
                }
                System.Data.DataSet Ds = new DataSet();
                Ds.Tables.Add(dt);
                Ds.Namespace = "CIC-30-DS";
                Ds.DataSetName = "IC_ATTR";
                Ds.WriteXml(Program.ApplicationPath + "\\cic_attr.xml", XmlWriteMode.WriteSchema);
            }
            //Tracing.TraceStatus(scope + "Completed.");
        }

        private void Initialize_CallBack()
        {
            string scope = "CIC::MainForm::Initial_CallBack()::";
            //Tracing.TraceStatus(scope + "Starting.");
            try
            {
                    if (this.IsLoggedIntoDialer && this.ActiveDialerInteraction != null)
                    {
                        this.ActiveDialerInteraction.AttributesChanged += new EventHandler<AttributesEventArgs>(CallBackInteraction_AttributesChanged);
                        List<string> callbackAttributeNames = new List<string>();
                        callbackAttributeNames.Add(CallbackInteractionAttributeName.RemoteName);
                        callbackAttributeNames.Add(CallbackInteractionAttributeName.RemoteId);
                        callbackAttributeNames.Add(CallbackInteractionAttributeName.CallbackPhone);
                        callbackAttributeNames.Add(CallbackInteractionAttributeName.AccountCodeId);
                        callbackAttributeNames.Add(CallbackInteractionAttributeName.WrapUpCodeId);
                        callbackAttributeNames.Add(CallbackInteractionAttributeName.WorkgroupQueueDisplayName);
                        callbackAttributeNames.Add(CallbackInteractionAttributeName.WorkgroupQueueName);
                        callbackAttributeNames.Add(CallbackInteractionAttributeName.UserQueueNames);
                        callbackAttributeNames.Add(CallbackInteractionAttributeName.StationQueueNames);
                        callbackAttributeNames.Add(CallbackInteractionAttributeName.State);
                        callbackAttributeNames.Add(CallbackInteractionAttributeName.StateDescription);
                        callbackAttributeNames.Add(CallbackInteractionAttributeName.LineQueueName);
                        callbackAttributeNames.Add(CallbackInteractionAttributeName.InteractionType);
                        callbackAttributeNames.Add(CallbackInteractionAttributeName.CallbackMessage);
                        callbackAttributeNames.Add(CallbackInteractionAttributeName.CallbackCompletion);
                        this.ActiveDialerInteraction.StartWatchingAsync(callbackAttributeNames.ToArray(), CallBackInteration_StartWatchingCompleted, null);
                    }
                
                //Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                //Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void CallBackInteration_StartWatchingCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string scope = "CIC::MainForm::CallBackInteration_StartWatchingCompleted()::";
            //Tracing.TraceStatus(scope + "Starting.");
            if (e.Error != null)
            {
                //Tracing.TraceStatus(scope + "Error info : " + e.Error.Message);
            }
            else
            {
                try
                {
                    if (this.ActiveDialerInteraction.WorkgroupQueueName.Length > 0)
                    {
                        this.ActiveWorkgroupDetails = new WorkgroupDetails(this.mPeopleManager, this.ActiveDialerInteraction.WorkgroupQueueName);
                        this.ActiveWorkgroupDetails.WatchedAttributesChanged += new EventHandler<WatchedAttributesEventArgs>(WorkGroup_WatchedAttributesChanged);
                        string[] mWorkgroupDetailsAttributeNames = { WorkgroupAttributes.WrapUpCodes,
                                                                   WorkgroupAttributes.HasMailbox,
                                                                   WorkgroupAttributes.HasQueue,
                                                                   WorkgroupAttributes.Extension,
                                                                   WorkgroupAttributes.ActiveMembers,
                                                                   WorkgroupAttributes.IsActive,
                                                                   WorkgroupAttributes.Members,
                                                                   WorkgroupAttributes.ActiveMembers,
                                                                   WorkgroupAttributes.Supervisors,
                                                                   WorkgroupAttributes.WrapUpClientTimeout
                                                                 };
                        this.ActiveWorkgroupDetails.StartWatchingAsync(mWorkgroupDetailsAttributeNames, WorkgroupDetailsStartWatchingComplete, null);
                    }
                    else
                    {
                        //wrapupCodesLabel.Visible = false;
                        //wrapupCodesComboBox.Visible = false;
                    }
                    //Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    //Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        private void WorkgroupDetailsStartWatchingComplete(object sender, AsyncCompletedEventArgs e)
        {
            // NYI
        }

        private void WorkGroup_WatchedAttributesChanged(object sender, WatchedAttributesEventArgs e)
        {
            // NYI
        }

        private void CallBackInteraction_AttributesChanged(object sender, AttributesEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<AttributesEventArgs>(CallBackInteraction_AttributesChanged), new object[] { sender, e });
            }
            else
            {
                // NYI
            }
        }

        private void DialerInteraction_AttributesChanged(object sender, AttributesEventArgs e)
        {
            string scope = "CIC::MainForm::DialerInteraction_AttributesChanged():: ";
            //Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (this.ActiveDialerInteraction != null)
                {
                    this.StrConnectionState = this.ActiveDialerInteraction.State;
                }
                else
                {
                    this.StrConnectionState = InteractionState.None;
                }
                //Tracing.TraceStatus(scope + "Completed.");
            }
            catch
            {
                //Get Connection State.
            }
        }

        private void BreakGranted(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void LogoutGranted(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::LogoutGranted(): ";
            //Tracing.TraceStatus(scope + "Starting.");
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new EventHandler<EventArgs>(LogoutGranted), new object[] { sender, e });
            }
            else
            {
                try
                {
                    switch (this.IsLoggedIntoDialer)
                    {
                        case true:
                            if (this.ActiveDialerInteraction != null)
                            {
                                this.ActiveDialerInteraction = null;
                            }
                            // TODO: need to clean up
                            //this.IcWorkFlow = null;
                            //this.DialerSession = null;
                            
                            //this.InitializeStatusMessageDetails();
                            //this.SetToDoNotDisturb_UserStatusMsg();
                            //this.CallActivityCodeToolStripComboBox.Items.Clear();
                            //this.ShowActiveCallInfo();
                            //this.CrmScreenPop();
                            state_change(FormMainState.Loggedout);
                            System.Windows.Forms.MessageBox.Show(global::CIC.Properties.Settings.Default.CompletedWorkflowMsg, "System Info.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        default:
                            if (ActiveNormalInteraction != null)
                            {
                                ActiveNormalInteraction.Disconnect();
                                ActiveNormalInteraction = null;
                            }
                            if (this.IC_Session != null)
                            {
                                this.IC_Session.Disconnect();
                                this.IC_Session = null;
                            }
                            state_change(FormMainState.Loggedout);
                            break;
                    }
                    //Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        private void placecall(object sender, EventArgs e)
        {
            // Src: PlaceCallToolStripButton_Click()
            // string scope = "CIC::MainForm::PlaceCallToolStripButton_Click(): ";
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new EventHandler<EventArgs>(placecall), new object[] { sender, e});
            }
            else
            {
                try
                {
                    // Tracing.TraceStatus(scope + "Starting.[Place Call]");
                    if (this.ActiveDialerInteraction != null)
                    {
                        this.ActiveDialerInteraction.PlacePreviewCall();
                    }
                    // Tracing.TraceStatus(scope + "Completed.[Place Call]");
                }
                catch (System.Exception ex)
                {
                    // Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                    // System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }
        
        private void pickup()
        {
            // Src: PickupToolStripButton_Click()
            // string scope = "CIC::frmMain::PickupToolStripButton_Click()::";
            // Tracing.TraceStatus(scope + "Starting.");
            try
            {
                switch (this.IsLoggedIntoDialer)
                {
                    case true:   //Log On to Dialer Server.
                        //Tracing.TraceStatus(scope + "Pickup button clicked.Log on to Dialer Server.");
                        if (this.ActiveDialerInteraction != null)
                        {
                            this.ActiveDialerInteraction.Pickup();
                        }
                        if (ActiveNormalInteraction != null)
                        {
                            switch (ActiveNormalInteraction.InteractionType)
                            {
                                case InteractionType.Email:
                                    //Show Mail form
                                    ActiveNormalInteraction.Pickup();
                                    break;
                                case InteractionType.Chat:
                                    break;
                                case InteractionType.Call:
                                    ActiveNormalInteraction.Pickup();
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    default:     // Not Log On to Dialer Server.
                        //Tracing.TraceStatus(scope + "Pickup button clicked[Basic station].");
                        if (ActiveNormalInteraction != null)
                        {
                            switch (ActiveNormalInteraction.InteractionType)
                            {
                                case InteractionType.Email:
                                    this.ViewEmailDetail(ActiveNormalInteraction);
                                    ActiveNormalInteraction.Pickup();
                                    break;
                                case InteractionType.Chat:
                                    break;
                                case InteractionType.Call:
                                    ActiveNormalInteraction.Pickup();
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                }
                // Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                // Tracing.TraceStatus("Error info." + ex.Message);
                // System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void ViewEmailDetail(Interaction ActiveNormalInteraction)
        {
            // TODO: add view email detail
        }

        private void MuteCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // TODO: check UAT for detail if we need this.
        }
    }
}
