using ININ.IceLib.Connection;
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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using log4net;

namespace CIC
{
    public enum CallerType
    {
        TimerCalled,
        ButtonClicked
    }

    public enum FormMainState
    {
        Connected,              // connect to workflow
        Preview,                // got information from workflow, timer starts
        Predictive,             // got information from workflow, waiting to pickup call
        Calling,                // Call started
        ConferenceCall,         // connected to conference call
        PreviewCall,            // workflow call connected
        ManualCall,             // manual call connected
        Hold,                   // hold current call
        Mute,                   // mute current call
        Break,                  // pause current workflow not to put new set of information after disposition
        Loggedout,              // log out from workflow
        Disconnected,           // disconnect from workflow
        None,                   // nothing at all or error state
    };
    //private bool IsLoggedIntoDialer = false;

    public partial class FormMain : Form
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ScheduleCallbackForm frmScheduleCallbackForm { get; set; }

        private bool break_requested { get; set; }
        private bool break_granted { get; set; }
        private bool BlindTransferFlag { get; set; }
        private bool IsMuted { get; set; }
        private bool IsActiveConnection { get; set; }
        private bool SwapPartyFlag { get; set; }
        private bool ExitFlag { get; set; }
        private bool IsManualDialing { get; set; }
        private bool IsActiveConference_flag { get; set; }
        private int AutoReconnect = 2;
        private string[] InteractionAttributes { get; set; }
        private ArrayList InteractionList { get; set; }
        private string callingNumber { get; set; }
        private string ScheduleAgent { get; set; }
        private string CallBackPhone { get; set; }
        private string CallerHost { get; set; }
        private string AlertSoundFileType { get; set; }
        private float timer = global::CIC.Properties.Settings.Default.CountdownTime;


        private bool IsPlayAlerting { get; set; }
        private Locus.Control.MP3Player cicMp3Player { get; set; }
        private System.Media.SoundPlayer soundPlayer { get; set; }

        private DateTime currentTime { get; set; }
        private DateTime CallBackDateTime { get; set; }

        private FormMainState prev_state = FormMainState.Preview;
        private FormMainState current_state = FormMainState.Preview;
        private DataSet DsReasonCode { get; set; }
        private StatusMessageList AllStatusMessageList { get; set; }
        private UserStatusList AllStatusMessageListOfUser { get; set; }
        private UserStatus CurrentUserStatus { get; set; }
        private WorkgroupDetails ActiveWorkgroupDetails { get; set; }
        private PeopleManager mPeopleManager { get; set; }
        private Session IC_Session = null;
        private DialerSession DialerSession = null;
        private InteractionState StrConnectionState = InteractionState.None;
        private InteractionQueue m_InteractionQueue { get; set; }
        private DialerCallInteraction ActiveDialerInteraction = null;
        private EmailInteraction ActiveNormalEmailInteraction { get; set; }
        private CallbackInteraction ActiveCallbackInteraction { get; set; }
        private InteractionsManager NormalInterationManager = null;

        private StatusMessageDetails AvailableStatusMessageDetails { get; set; }
        private StatusMessageDetails DoNotDisturbStatusMessageDetails { get; set; }

        private static ICWorkFlow IcWorkFlow = null;
        private static Interaction ActiveNormalInteraction { get; set; }
        private static Interaction ActiveConsultInteraction { get; set; }
        private static InteractionConference ActiveConferenceInteraction = null;
        private static NameValueCollection mDialerData { get; set; }

        public FormMain()
        {
            ExitFlag = false;
            InitializeComponent(); 
            state_change(FormMainState.Disconnected);
            InitializeSession();
        }

        private void InitializeSession()
        {
            string scope = "CIC::frmMain::InitialAllComponents()::";
            log.Info(scope + " Starting");
            bool bResult = false;
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(InitializeSession));
            }
            else
            {
                try
                {
                    if (global::CIC.Program.m_Session == null)
                    {
                        global::CIC.Program.m_Session = new Session();
                        global::CIC.Program.IcStation = new ICStation(global::CIC.Program.m_Session);
                        global::CIC.Program.m_Session.SetAutoReconnectInterval(this.AutoReconnect);   //Time in seccond to Reconnected.
                        global::CIC.Program.m_Session.ConnectionStateChanged += new EventHandler<ConnectionStateChangedEventArgs>(mSession_Changed);
                        global::CIC.Program.IcStation.LogIn(
                            global::CIC.Program.mLoginParam.WindowsAuthentication, global::CIC.Program.mLoginParam.UserId,
                            global::CIC.Program.mLoginParam.Password, global::CIC.Program.mLoginParam.Server,
                            global::CIC.Program.mLoginParam.StationType, global::CIC.Program.mLoginParam.StationId,
                            global::CIC.Program.mLoginParam.PhoneNumber, global::CIC.Program.mLoginParam.Persistent,
                            this.SessionConnectCompleted, null
                        );
                    }

                    ININ.IceLib.Connection.Session session = global::CIC.Program.m_Session;
                    Program.Initialize_dialingManager(session);
                
                    if (session != null)
                    {
                        bResult = this.SetActiveSession(session);
                        if (this.IC_Session != null)
                        {
                            ININ.IceLib.Connection.ConnectionState mConnectionState;
                            try
                            {
                                mConnectionState = this.IC_Session.ConnectionState;
                            }
                            catch
                            {
                                mConnectionState = ININ.IceLib.Connection.ConnectionState.None;

                            }

                            if (mConnectionState == ININ.IceLib.Connection.ConnectionState.Up)
                            {
                                this.Initial_NormalInteraction();
                                this.InitializeQueueWatcher();
                                this.BeginInvoke(new MethodInvoker(connected_state));
                                log.Info(scope + "Completed.");
                            }
                            else
                            {
                                //No active connection. 
                                state_change(FormMainState.Disconnected);
                                log.Warn(scope + "Cannot log on to station.please try again.");
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    state_change(FormMainState.Disconnected);
                    log.Error(ex.Message);
                }
            }
        }

        private void InitializeQueueWatcher()
        {
            string scope = "CIC::MainForm::InitializeQueueWatcher():: ";
            log.Info(scope + "Starting.");
            try
            {
                log.Info(scope + "Creating instance of InteractionQueue");
                if (this.NormalInterationManager != null)
                {
                    this.m_InteractionQueue = new ININ.IceLib.Interactions.InteractionQueue(this.NormalInterationManager, new QueueId(QueueType.MyInteractions, this.IC_Session.UserId));
                    log.Info(scope + "Attaching event handlers");
                    this.m_InteractionQueue.InteractionAdded += new EventHandler<InteractionAttributesEventArgs>(m_InteractionQueue_InteractionAdded);
                    this.m_InteractionQueue.InteractionChanged += new EventHandler<InteractionAttributesEventArgs>(m_InteractionQueue_InteractionChanged);
                    this.m_InteractionQueue.InteractionRemoved += new EventHandler<InteractionEventArgs>(m_InteractionQueue_InteractionRemoved);
                    this.m_InteractionQueue.ConferenceInteractionAdded += new EventHandler<ConferenceInteractionAttributesEventArgs>(m_InteractionQueue_ConferenceInteractionAdded);
                    this.m_InteractionQueue.ConferenceInteractionChanged += new EventHandler<ConferenceInteractionAttributesEventArgs>(m_InteractionQueue_ConferenceInteractionChanged);
                    this.m_InteractionQueue.ConferenceInteractionRemoved += new EventHandler<ConferenceInteractionEventArgs>(m_InteractionQueue_ConferenceInteractionRemoved);
                    log.Info(scope + "Start watching for queue events");
                    this.Initialize_InteractionAttributes();
                    this.m_InteractionQueue.StartWatchingAsync(this.InteractionAttributes, null, null);
                }
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                log.Error(scope + "Error info." + ex.Message);
            }
        }

        private void m_InteractionQueue_ConferenceInteractionRemoved(object sender, ConferenceInteractionEventArgs e)
        {
            string scope = "CIC::MainForm::m_InteractionQueue_ConferenceInteractionChanged():: ";
            log.Info(scope + "Starting.");
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<ConferenceInteractionEventArgs>(m_InteractionQueue_ConferenceInteractionRemoved), new object[] { sender, e });
            }
            else
            {
                try
                {
                    if (!e.Interaction.IsWatching())
                    {
                        e.Interaction.AttributesChanged += new EventHandler<AttributesEventArgs>(DialerInteraction_AttributesChanged);
                        e.Interaction.StartWatching(this.InteractionAttributes);
                    }
                    switch (e.Interaction.InteractionType)
                    {
                        case InteractionType.Email:
                            break;
                        case InteractionType.Chat:
                            break;
                        case InteractionType.Callback:
                            break;
                        case InteractionType.Call:
                            ActiveNormalInteraction = e.Interaction;
                            if (e.ConferenceItem.IsDisconnected)
                            {
                                this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                            }
                            break;
                        default:
                            break;
                    }
                    this.Set_ConferenceToolStrip();
                    this.ShowActiveCallInfo();
                    log.Info(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    log.Error(scope + "Error info." + ex.Message);
                }
            }
        }

        private void Set_ConferenceToolStrip()
        {
            string scope = "CIC::MainForm::Set_ConferenceToolStrip():: ";
            log.Info(scope + "Starting.");
            bool AllConferencePartyConected = true;
            try
            {
                if (InteractionList.Count >= 2)
                {
                    if (ActiveConferenceInteraction == null)
                    {
                        for (int i = 0; i < this.InteractionList.Count; i++)
                        {
                            if (((ININ.IceLib.Interactions.Interaction)this.InteractionList[i]) != null &&
                                ((ININ.IceLib.Interactions.Interaction)this.InteractionList[i]).State != InteractionState.Connected &&
                                ((ININ.IceLib.Interactions.Interaction)this.InteractionList[i]).State != InteractionState.Held)
                            {
                                AllConferencePartyConected = false;
                                break;
                            }
                        }
                    }
                }
                else if (ActiveConferenceInteraction != null)
                {
                    this.state_change(FormMainState.ConferenceCall);
                }
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                //this.CreateConferenceToolStripButton.Enabled = false;
                //this.LeaveConferenceToolStripButton.Enabled = false;
                log.Error(scope + "Error info." + ex.Message);
            }
        }

        private void m_InteractionQueue_ConferenceInteractionChanged(object sender, ConferenceInteractionAttributesEventArgs e)
        {
            string scope = "CIC::MainForm::m_InteractionQueue_ConferenceInteractionChanged():: ";
            log.Info(scope + "Starting.");
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<ConferenceInteractionAttributesEventArgs>(
                    m_InteractionQueue_ConferenceInteractionChanged), new object[] { sender, e });
            }
            else
            {
                try
                {
                    if (!e.Interaction.IsWatching())
                    {
                        e.Interaction.AttributesChanged += 
                            new EventHandler<AttributesEventArgs>(DialerInteraction_AttributesChanged);
                        e.Interaction.StartWatching(this.InteractionAttributes);
                    }
                    if (!e.ConferenceItem.IsWatching())
                    {
                        e.ConferenceItem.StartWatching(this.InteractionAttributes);
                    }
                    switch (e.Interaction.InteractionType)
                    {
                        case InteractionType.Email:
                            break;
                        case InteractionType.Chat:
                            break;
                        case InteractionType.Callback:
                            break;
                        case InteractionType.Call:
                            ActiveNormalInteraction = e.Interaction;
                            if (e.ConferenceItem.IsDisconnected)
                            {
                                this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                            }
                            break;
                        default:
                            break;
                    }

                    this.Set_ConferenceToolStrip();
                    this.ShowActiveCallInfo();
                    log.Info(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    log.Error(scope + "Error info." + ex.Message);
                    this.ShowActiveCallInfo();
                }
            }
        }
        
        private void m_InteractionQueue_ConferenceInteractionAdded(object sender, ConferenceInteractionAttributesEventArgs e)
        {
            string scope = "CIC::MainForm::m_InteractionQueue_ConferenceInteractionAdded():: ";
            log.Info(scope + "Starting.");
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<ConferenceInteractionAttributesEventArgs>(
                    m_InteractionQueue_ConferenceInteractionAdded), new object[] { sender, e });
            }
            else
            {
                try
                {
                    if (!e.Interaction.IsWatching())
                    {
                        e.Interaction.AttributesChanged += new EventHandler<AttributesEventArgs>(
                            DialerInteraction_AttributesChanged);
                        e.Interaction.StartWatching(this.InteractionAttributes);
                    }
                    this.IsActiveConference_flag = true;
                    switch (e.Interaction.InteractionType)
                    {
                        case InteractionType.Email:
                            break;
                        case InteractionType.Chat:
                            break;
                        case InteractionType.Callback:
                            break;
                        case InteractionType.Call:
                            if (!e.Interaction.IsDisconnected)
                            {
                                ActiveConferenceInteraction = new InteractionConference(
                                    this.NormalInterationManager, e.Interaction.InteractionType,
                                    e.ConferenceItem.ConferenceId);
                                ActiveNormalInteraction = e.Interaction;
                                this.ShowActiveCallInfo();
                            }
                            break;
                        default:
                            break;
                    }
                    log.Info(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    log.Error(scope + "Error info." + ex.Message);
                }
            }
        }

        private void m_InteractionQueue_InteractionRemoved(object sender, InteractionEventArgs e)
        {
            string scope = "CIC::MainForm::m_InteractionQueue_InteractionRemoved():: ";
            log.Info(scope + "Starting.");
            try
            {
                if (!e.Interaction.IsWatching())
                {
                    e.Interaction.AttributesChanged += new EventHandler<AttributesEventArgs>(NormalInteraction_AttributesChanged);
                    e.Interaction.StartWatching(this.InteractionAttributes);
                }
                switch (e.Interaction.InteractionType)
                {
                    case InteractionType.Email:
                        ActiveNormalInteraction = e.Interaction;
                        if (ActiveNormalInteraction != null)
                        {
                            if (ActiveNormalInteraction.IsDisconnected)
                            {
                                this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                                this.CallerHost = "";
                                ActiveNormalInteraction = this.GetAvailableInteractionFromList();
                                if (ActiveNormalInteraction.State != InteractionState.None)  //chk EIC_STATE
                                {
                                    this.ShowActiveCallInfo();
                                }
                            }
                        }
                        break;
                    case InteractionType.Chat:
                        //
                        break;
                    case InteractionType.Callback:
                        ActiveNormalInteraction = e.Interaction;
                        if (ActiveNormalInteraction != null)
                        {
                            if (ActiveNormalInteraction.IsDisconnected)
                            {
                                this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                                this.CallerHost = "";
                                ActiveNormalInteraction = this.GetAvailableInteractionFromList();
                                if (ActiveNormalInteraction != null)
                                {
                                    this.StrConnectionState = ActiveNormalInteraction.State;
                                }
                                else
                                {
                                    this.StrConnectionState = InteractionState.None; //"None"
                                }
                                if (ActiveNormalInteraction.State != InteractionState.None)  //chk EIC_STATE
                                {
                                    this.ShowActiveCallInfo();
                                }
                            }
                        }
                        break;
                    case InteractionType.Call:
                        ActiveNormalInteraction = e.Interaction;
                        if (ActiveNormalInteraction.IsDisconnected)
                        {
                            this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                            this.CallerHost = "";
                            ActiveNormalInteraction = this.GetAvailableInteractionFromList();
                            if (ActiveNormalInteraction != null)
                            {
                                this.StrConnectionState = ActiveNormalInteraction.State;
                            }
                            else
                            {
                                this.StrConnectionState = InteractionState.None; //"None"
                            }
                            this.ShowActiveCallInfo();
                            
                        }
                        break;
                }
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                log.Error(scope + "Error info." + ex.Message);
                this.ResetActiveCallInfo();
                this.CallerHost = "";
                if (ActiveNormalInteraction != null)
                {
                    try
                    {
                        ActiveNormalInteraction.Disconnect();
                    }
                    catch
                    {
                        //Emty catch block
                    }
                    this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                }
                ActiveNormalInteraction = null;
                this.ShowActiveCallInfo();
            }
        }

        private void ResetActiveCallInfo()
        {
            string scope = "CIC::frmMain::ResetActiveCallInfo()::";
            log.Info(scope + "Starting.");
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(ResetActiveCallInfo));
            }
            else
            {
                if (ActiveConsultInteraction != null)
                {
                    this.SetInfoBarColor();
                    this.transfer_button.Enabled = true;
                }
                else
                {
                    if (this.BlindTransferFlag)
                    {
                        this.InteractionList.Clear();
                        this.reset_info_on_dashboard();

                        this.state_info_label.Text = "Connected to: unknown";
                        //this.DirectiontoolStripStatus.Text = "None";
                        //this.CallTypeToolStripStatusLabel.Text = "N/A";
                        //this.CampaignIdToolStripStatusLabel.Text = "N/A";
                        //this.QueueNameToolStripStatusLabel.Text = "N/A";
                        //this.NumberToolStripStatusLabel.Text = "N/A";
                        //this.CallStateToolStripStatusLabel.Text = "N/A";
                        this.SetInfoBarColor();
                        this.transfer_button.Enabled = true;
                    }
                }
                log.Info(scope + "Completed.");
            }
        }

        private void reset_info_on_dashboard()
        {
            this.reset_color_panel();
            this.contractNo_box.Text = "";
            this.license_plate_box.Text = "";
            this.product_name_box.Text = "";
            this.name1_box1.Text = "";
            this.name2_box1.Text = "";
            this.name3_box1.Text = "";
            this.name4_box1.Text = "";
            this.name5_box1.Text = "";
            this.name6_box2.Text = "";
            this.name1_box2.Text = "";
            this.name2_box2.Text = "";
            this.name3_box2.Text = "";
            this.name4_box2.Text = "";
            this.name5_box2.Text = "";
            this.name6_box2.Text = "";
            this.aging_box.Text = "";
            this.base_debt_box.Text = "";
            this.number_due_box.Text = "";
            this.last_amount_payment_box.Text = "";
            this.last_date_payment_box.Text = "";
            this.initial_amount_box.Text = "";
            this.monthly_payment_box.Text = "";
            this.debt_status_box.Text = "";
            this.start_overdue_date_box.Text = "";
            this.followup_status_box.Text = "";
            this.payment_appoint_box.Text = "";
            this.date_callback_box.Text = "";
            this.callingNumber = "";

            this.toolStripCallIDLabel.Text = "N/A";
            this.toolStripDirectionLabel.Text = "N/A";
            this.toolStripCallTypeLabel.Text = "N/A";
            this.toolStripCampaignIDLabel.Text = "N/A";
        }

        private void SetInfoBarColor()
        {
            string scope = "CIC::frmMain::SetInfoBarColor()::";
            log.Info(scope + "Starting.");
            if (IcWorkFlow.LoginResult)
            {
                switch (this.StrConnectionState)
                {
                    case InteractionState.Alerting:
                        this.Start_AlertingWav();
                        break;
                    case InteractionState.Offering:
                        this.Start_AlertingWav();
                        break;
                    case InteractionState.Held:
                        break;
                    case InteractionState.Connected:
                        if (!IsMuted)
                            this.Stop_AlertingWav();
                        break;
                    default:
                        this.Stop_AlertingWav();
                        break;
                }
                this.EnabledDialerCallTools();
            }
            else
            {
                switch (this.StrConnectionState)
                {
                    case InteractionState.Alerting:
                        this.Start_AlertingWav();
                        break;
                    case InteractionState.Offering:
                        this.Start_AlertingWav();
                        break;
                    default:
                        this.Stop_AlertingWav();
                        break;
                }
                this.EnabledDialerCallTools();
            }
            log.Info(scope + "Completed.");
        }

        private void EnabledDialerCallTools()
        {
            string scope = "CIC::frmMain::EnabledNormalCallTools()::";
            log.Info(scope + "Starting.");
            //Color OldColor = this.TelephonyToolStrip.BackColor;  //Save Original Trasparent Color
            if (break_requested)
            {
                this.break_button.Enabled = false;
            }
            else
            {
                this.break_button.Enabled = true;
            }
            switch (this.StrConnectionState)
            {
                case InteractionState.System:
                    //this.DispositionToolStripButton.Enabled = false;
                    //this.CallActivityCodeToolStripComboBox.Enabled = false;
                    //this.PlaceCallToolStripButton.Enabled = true;
                    //this.SkipCallToolStripButton.Enabled = true;
                    //this.CallToolStripSplitButton.Enabled = false;
                    //this.PickupToolStripButton.Enabled = false;
                    //this.MuteToolStripButton.Enabled = false;
                    //this.MuteToolStripButton.Checked = this.IsMuted;
                    //this.HoldToolStripButton.Checked = false;
                    //this.HoldToolStripButton.Enabled = false;
                    //this.DisconnectToolStripButton.Enabled = false;
                    //this.DialpadToolStripDropDownButton.Enabled = false;
                    break;
                case InteractionState.Alerting:
                    //this.DispositionToolStripButton.Enabled = true;
                    //this.CallActivityCodeToolStripComboBox.Enabled = true;
                    //this.PlaceCallToolStripButton.Enabled = false;
                    //this.SkipCallToolStripButton.Enabled = false;
                    //if (this.IsDialingEnabled == true)
                    //{
                    //    this.CallToolStripSplitButton.Enabled = true;
                    //}
                    //else
                    //{
                    //    this.CallToolStripSplitButton.Enabled = false;
                    //}
                    //this.PickupToolStripButton.Enabled = true;
                    //this.MuteToolStripButton.Enabled = true;
                    //this.HoldToolStripButton.Enabled = true;
                    //this.MuteToolStripButton.Checked = this.IsMuted;
                    //this.HoldToolStripButton.Checked = false;
                    //this.SkipCallToolStripButton.Enabled = true;
                    //this.DisconnectToolStripButton.Enabled = true;
                    //this.DialpadToolStripDropDownButton.Enabled = true;
                    //this.TelephonyToolStrip.BackColor = Color.Aqua;
                    //this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case InteractionState.Messaging:
                    //this.DispositionToolStripButton.Enabled = true;
                    ////this.CallActivityCodeToolStripComboBox.SelectedIndex = -1;
                    //this.CallActivityCodeToolStripComboBox.Enabled = true;
                    //this.PlaceCallToolStripButton.Enabled = false;
                    //this.SkipCallToolStripButton.Enabled = false;
                    //if (this.IsDialingEnabled == true)
                    //{
                    //    this.CallToolStripSplitButton.Enabled = true;
                    //}
                    //else
                    //{
                    //    this.CallToolStripSplitButton.Enabled = false;
                    //}
                    //this.PickupToolStripButton.Enabled = true;
                    //this.MuteToolStripButton.Enabled = true;
                    //this.HoldToolStripButton.Enabled = true;
                    //this.MuteToolStripButton.Checked = this.IsMuted;
                    //this.HoldToolStripButton.Checked = false;
                    //this.SkipCallToolStripButton.Enabled = true;
                    //this.DisconnectToolStripButton.Enabled = true;
                    //this.DialpadToolStripDropDownButton.Enabled = true;
                    //this.TelephonyToolStrip.BackColor = Color.Aqua;
                    //this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case InteractionState.Offering:
                    //this.DispositionToolStripButton.Enabled = true;
                    //this.CallActivityCodeToolStripComboBox.Enabled = true;
                    //this.PlaceCallToolStripButton.Enabled = false;
                    //this.SkipCallToolStripButton.Enabled = false;
                    //if (this.IsDialingEnabled == true)
                    //{
                    //    this.CallToolStripSplitButton.Enabled = true;
                    //}
                    //else
                    //{
                    //    this.CallToolStripSplitButton.Enabled = false;
                    //}
                    //this.PickupToolStripButton.Enabled = true;
                    //this.MuteToolStripButton.Enabled = true;
                    //this.HoldToolStripButton.Enabled = true;
                    //this.MuteToolStripButton.Checked = this.IsMuted;
                    //this.HoldToolStripButton.Checked = false;
                    //this.DisconnectToolStripButton.Enabled = true;
                    //this.DialpadToolStripDropDownButton.Enabled = true;
                    //this.TelephonyToolStrip.BackColor = Color.Aqua;
                    //this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                //case "dialing":
                //    this.DispositionToolStripButton.Enabled = true;
                //    this.CallActivityCodeToolStripComboBox.Enabled = true;
                //    this.PlaceCallToolStripButton.Enabled = false;
                //    this.SkipCallToolStripButton.Enabled = false;
                //    this.CallToolStripSplitButton.Enabled = false;
                //    this.PickupToolStripButton.Enabled = false;
                //    this.MuteToolStripButton.Enabled = false;
                //    this.HoldToolStripButton.Enabled = false;
                //    this.MuteToolStripButton.Checked = this.IsMuted;
                //    this.HoldToolStripButton.Checked = false;
                //    this.DisconnectToolStripButton.Enabled = true;
                //    this.DialpadToolStripDropDownButton.Enabled = false;
                //    this.TelephonyToolStrip.BackColor = Color.Aqua;
                //    this.TelephonyToolStrip.BackColor = OldColor;
                //    break;
                case InteractionState.Proceeding:
                    //this.DispositionToolStripButton.Enabled = true;
                    //this.CallActivityCodeToolStripComboBox.Enabled = true;
                    //this.PlaceCallToolStripButton.Enabled = false;
                    //this.SkipCallToolStripButton.Enabled = false;
                    //this.CallToolStripSplitButton.Enabled = false;
                    //this.PickupToolStripButton.Enabled = false;
                    //this.MuteToolStripButton.Enabled = false;
                    //this.HoldToolStripButton.Enabled = false;
                    //this.MuteToolStripButton.Checked = this.IsMuted;
                    //this.HoldToolStripButton.Checked = false;
                    //this.DisconnectToolStripButton.Enabled = true;
                    //this.DialpadToolStripDropDownButton.Enabled = false;
                    //this.TelephonyToolStrip.BackColor = Color.Aqua;
                    //this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case InteractionState.Held:
                    //this.DispositionToolStripButton.Enabled = true;
                    //this.CallActivityCodeToolStripComboBox.Enabled = true;
                    //this.PlaceCallToolStripButton.Enabled = false;
                    //this.SkipCallToolStripButton.Enabled = false;
                    //if (this.IsDialingEnabled == true)
                    //{
                    //    this.CallToolStripSplitButton.Enabled = true;
                    //}
                    //else
                    //{
                    //    this.CallToolStripSplitButton.Enabled = false;
                    //}
                    //this.PickupToolStripButton.Enabled = true;
                    //this.MuteToolStripButton.Enabled = true;
                    //this.HoldToolStripButton.Enabled = true;
                    //this.MuteToolStripButton.Checked = this.IsMuted;
                    //this.HoldToolStripButton.Checked = true;
                    //this.DisconnectToolStripButton.Enabled = true;
                    //this.DialpadToolStripDropDownButton.Enabled = true;
                    //this.TelephonyToolStrip.BackColor = Color.Aqua;
                    //this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case InteractionState.Connected:
                    //if (IsMuted)
                    //{
                    //    this.DispositionToolStripButton.Enabled = true;
                    //    this.CallActivityCodeToolStripComboBox.Enabled = true;
                    //    this.PlaceCallToolStripButton.Enabled = false;
                    //    this.SkipCallToolStripButton.Enabled = false;
                    //    if (this.IsDialingEnabled == true)
                    //    {
                    //        this.CallToolStripSplitButton.Enabled = true;
                    //    }
                    //    else
                    //    {
                    //        this.CallToolStripSplitButton.Enabled = false;
                    //    }
                    //    this.PickupToolStripButton.Enabled = true;
                    //    this.MuteToolStripButton.Enabled = true;
                    //    this.HoldToolStripButton.Enabled = true;
                    //    this.MuteToolStripButton.Checked = this.IsMuted;
                    //    this.HoldToolStripButton.Checked = false;
                    //    this.DisconnectToolStripButton.Enabled = true;
                    //    this.DialpadToolStripDropDownButton.Enabled = true;
                    //    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    //    this.TelephonyToolStrip.BackColor = OldColor;
                    //}
                    //else
                    //{
                    //    this.DispositionToolStripButton.Enabled = true;
                    //    this.CallActivityCodeToolStripComboBox.Enabled = true;
                    //    this.PlaceCallToolStripButton.Enabled = false;
                    //    this.SkipCallToolStripButton.Enabled = false;
                    //    if (this.IsDialingEnabled == true)
                    //    {
                    //        this.CallToolStripSplitButton.Enabled = true;
                    //    }
                    //    else
                    //    {
                    //        this.CallToolStripSplitButton.Enabled = false;
                    //    }
                    //    this.PickupToolStripButton.Enabled = false;
                    //    this.MuteToolStripButton.Enabled = true;
                    //    this.HoldToolStripButton.Enabled = true;
                    //    this.MuteToolStripButton.Checked = this.IsMuted;
                    //    this.HoldToolStripButton.Checked = false;
                    //    this.DisconnectToolStripButton.Enabled = true;
                    //    this.DialpadToolStripDropDownButton.Enabled = true;
                    //    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    //    this.TelephonyToolStrip.BackColor = OldColor;
                    //}
                    break;
                case InteractionState.None:
                    //this.DispositionToolStripButton.Enabled = false;
                    //this.CallActivityCodeToolStripComboBox.Enabled = false;
                    //this.PlaceCallToolStripButton.Enabled = false;
                    //this.SkipCallToolStripButton.Enabled = false;
                    //this.CallToolStripSplitButton.Enabled = false;
                    //this.PickupToolStripButton.Enabled = false;
                    //this.MuteToolStripButton.Enabled = false;
                    //this.HoldToolStripButton.Enabled = false;
                    //this.MuteToolStripButton.Checked = this.IsMuted;
                    //this.HoldToolStripButton.Checked = false;
                    //this.DisconnectToolStripButton.Enabled = false;
                    //this.DialpadToolStripDropDownButton.Enabled = false;
                    //this.TelephonyToolStrip.BackColor = Color.Aqua;
                    //this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                default:
                    //if (this.CallStateToolStripStatusLabel.Text.ToLower().Trim().Substring(0, 3).Equals("acd"))
                    //{
                    //    this.DispositionToolStripButton.Enabled = true;
                    //    this.CallActivityCodeToolStripComboBox.Enabled = true;
                    //    this.PlaceCallToolStripButton.Enabled = false;
                    //    this.SkipCallToolStripButton.Enabled = false;
                    //    this.CallToolStripSplitButton.Enabled = false;
                    //    this.PickupToolStripButton.Enabled = false;
                    //    this.MuteToolStripButton.Enabled = false;
                    //    this.HoldToolStripButton.Enabled = false;
                    //    this.MuteToolStripButton.Checked = this.IsMuted;
                    //    this.HoldToolStripButton.Checked = false;
                    //    this.DisconnectToolStripButton.Enabled = true;
                    //    this.DialpadToolStripDropDownButton.Enabled = false;
                    //    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    //    this.TelephonyToolStrip.BackColor = OldColor;
                    //}
                    //else if (this.CallStateToolStripStatusLabel.Text.ToLower().Trim().Substring(0, 10).Equals("disconnect"))
                    //{
                    //    this.DispositionToolStripButton.Enabled = true;
                    //    this.CallActivityCodeToolStripComboBox.Enabled = true;
                    //    this.PlaceCallToolStripButton.Enabled = false;
                    //    this.SkipCallToolStripButton.Enabled = false;
                    //    this.CallToolStripSplitButton.Enabled = false;
                    //    this.PickupToolStripButton.Enabled = false;
                    //    this.MuteToolStripButton.Enabled = false;
                    //    this.HoldToolStripButton.Enabled = false;
                    //    this.MuteToolStripButton.Checked = this.IsMuted;
                    //    this.HoldToolStripButton.Checked = false;
                    //    this.DisconnectToolStripButton.Enabled = false;
                    //    this.DialpadToolStripDropDownButton.Enabled = false;
                    //    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    //    this.TelephonyToolStrip.BackColor = OldColor;
                    //}
                    //else
                    //{
                    //    this.DispositionToolStripButton.Enabled = false;
                    //    this.CallActivityCodeToolStripComboBox.Enabled = false;
                    //    this.PlaceCallToolStripButton.Enabled = false;
                    //    this.SkipCallToolStripButton.Enabled = false;
                    //    this.CallToolStripSplitButton.Enabled = false;
                    //    this.PickupToolStripButton.Enabled = false;
                    //    this.MuteToolStripButton.Enabled = false;
                    //    this.HoldToolStripButton.Enabled = false;
                    //    this.MuteToolStripButton.Checked = this.IsMuted;
                    //    this.HoldToolStripButton.Checked = false;
                    //    this.DisconnectToolStripButton.Enabled = false;
                    //    this.DialpadToolStripDropDownButton.Enabled = false;
                    //    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    //    this.TelephonyToolStrip.BackColor = OldColor;
                    //}
                    break;
            }
            log.Info(scope + "Completed.");
        }

        private void Stop_AlertingWav()
        {
            string scope = "CIC::MainForm::Stop_AlertingWav():: ";
            log.Info(scope + "Starting.");
            if (AlertSoundFileType != null && AlertSoundFileType.ToLower().Trim() == "mp3")
            {
                if (this.cicMp3Player != null)
                {
                    this.cicMp3Player.Stop();
                }
            }
            else
            {
                try
                {
                    if (this.IsPlayAlerting)
                    {
                        if (this.soundPlayer != null)
                        {
                            this.soundPlayer.Stop();
                        }
                        this.IsPlayAlerting = false;
                    }
                }
                catch (System.Exception ex)
                {
                    this.IsPlayAlerting = false;
                    log.Error(scope + "Error info." + ex.Message);
                    //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        private void Start_AlertingWav()
        {
            string scope = "CIC::MainForm::Start_AlertingWav():: ";
            log.Info(scope + "Starting.");
            string sPathWavPath = "";
            bool AlertFlag = false;
            bool Play_Looping = false;
            int RingCount = 0;
            int PlayCount = 0;
            if (IcWorkFlow.LoginResult)
            {
                    sPathWavPath = CIC.Program.ResourcePath + global::CIC.Properties.Settings.Default.DialerAlertSound;
                    AlertFlag = global::CIC.Properties.Settings.Default.DialerAlerting;
                    Play_Looping = global::CIC.Properties.Settings.Default.DialerLooping;
                    RingCount = global::CIC.Properties.Settings.Default.DialerRingCount;
            }
            else
            {
                    sPathWavPath = CIC.Program.ResourcePath + global::CIC.Properties.Settings.Default.NormalAlertSound;
                    AlertFlag = global::CIC.Properties.Settings.Default.NormalAlertting;
                    Play_Looping = global::CIC.Properties.Settings.Default.NormalLooping;
                    RingCount = global::CIC.Properties.Settings.Default.NormalRingCount;
            }

            this.AlertSoundFileType = sPathWavPath.Trim().Substring(sPathWavPath.Trim().Length - 3, 3);
            if (AlertSoundFileType.ToLower().Trim() == "mp3")
            {
                this.cicMp3Player = new Locus.Control.MP3Player();
                this.cicMp3Player.Play(sPathWavPath, false);
            }
            else if (AlertFlag)
            {
                try
                {
                    if (this.CurrentUserStatus != null &&
                        !this.CurrentUserStatus.StatusMessageDetails.IsDoNotDisturbStatus)
                    {
                        if (System.IO.File.Exists(sPathWavPath))
                        {
                            if (!this.IsPlayAlerting)
                            {
                                this.soundPlayer = new System.Media.SoundPlayer(sPathWavPath);
                                this.IsPlayAlerting = true;
                                if (Play_Looping)
                                {
                                    this.soundPlayer.PlayLooping();
                                }
                                else
                                {
                                    if (RingCount <= 0)
                                    {
                                        this.soundPlayer.PlaySync();
                                    }
                                    else
                                    {
                                        PlayCount = 0;
                                        while (RingCount >= PlayCount)
                                        {
                                            this.soundPlayer.PlaySync();
                                            PlayCount++;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            log.Error(scope + "Error info. : WAV File not found.");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    this.IsPlayAlerting = false;
                    log.Error(scope + "Error info." + ex.Message);
                }
            }
        }

        private void m_InteractionQueue_InteractionChanged(object sender, InteractionAttributesEventArgs e)
        {
            string scope = "CIC::MainForm::m_InteractionQueue_InteractionChanged():: ";
            log.Info(scope + "Starting.");
            try
            {
                if (!e.Interaction.IsWatching())
                {
                    e.Interaction.AttributesChanged += new EventHandler<AttributesEventArgs>(NormalInteraction_AttributesChanged);
                    e.Interaction.StartWatching(this.InteractionAttributes);
                }
                switch (e.Interaction.InteractionType)
                {
                    case InteractionType.Email:
                        //
                        break;
                    case InteractionType.Chat:
                        //
                        break;
                    case InteractionType.Callback:
                        //
                        break;
                    case InteractionType.Call:
                        if (ActiveNormalInteraction.IsDisconnected && e.Interaction.IsConnected)
                        {
                            state_info_label.Text = "Connected to: " + callingNumber;
                        }
                        ActiveNormalInteraction = e.Interaction;
                        this.StrConnectionState = ActiveNormalInteraction.State;
                        if (ActiveNormalInteraction != null)
                        {
                            if (ActiveNormalInteraction.IsDisconnected)
                            {
                                this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                                ActiveNormalInteraction = this.GetAvailableInteractionFromList();
                            }
                            else
                            {
                                // TODO: check if there's no blind transfer flag allow pickup
                            }
                        }
                        if (this.BlindTransferFlag)
                        {
                            this.ResetActiveCallInfo();
                        }
                        this.ShowActiveCallInfo();
                        break;
                    default:
                        ActiveNormalInteraction = e.Interaction;
                        if (ActiveNormalInteraction != null)
                        {
                            if (ActiveNormalInteraction.IsDisconnected)
                            {
                                this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                                ActiveNormalInteraction = this.GetAvailableInteractionFromList();
                            }
                        }
                        if (this.BlindTransferFlag)
                        {
                            this.StrConnectionState = InteractionState.None;
                        }
                        this.ShowActiveCallInfo();
                        break;
                }

                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                log.Error(scope + "Error info." + ex.Message);
                if (ActiveNormalInteraction != null)
                {
                    this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                    ActiveNormalInteraction = this.GetAvailableInteractionFromList();
                    if (this.BlindTransferFlag)
                    {
                        this.BlindTransferFlag = false;
                        this.StrConnectionState = InteractionState.None;
                    }
                    this.ShowActiveCallInfo();
                }
            }
        }

        private void RemoveNormalInteractionFromList(Interaction Interaction_Object)
        {
            string scope = "CIC::frmMain::RemoveNormalInteractionFromList(Interaction_Object)::";      //Over load II
            log.Info(scope + "Starting.");
            int retIndex = -1;
            int i = 0;
            try
            {
                if (Interaction_Object != null)
                {
                    if (this.InteractionList != null)
                    {
                        if (this.InteractionList.Count > 0)
                        {
                            do    //Remove junk CallObject
                            {
                                retIndex = -1;
                                for (i = 0; i < this.InteractionList.Count; i++)
                                {
                                    if (((ININ.IceLib.Interactions.Interaction)this.InteractionList[i]) == null)
                                    {
                                        retIndex = i;
                                        break;
                                    }
                                    else
                                    {
                                        if (((ININ.IceLib.Interactions.Interaction)this.InteractionList[i]).IsDisconnected)
                                        {
                                            retIndex = i;
                                            break;
                                        }
                                    }
                                }
                                if (this.InteractionList != null)
                                {
                                    if ((retIndex >= 0) && (retIndex < this.InteractionList.Count))
                                    {
                                        this.InteractionList.RemoveAt(retIndex);
                                    }
                                }
                            }
                            while (retIndex >= 0);
                            if (this.InteractionList != null)
                            {
                                for (i = 0; i < this.InteractionList.Count; i++)
                                {
                                    if (((ININ.IceLib.Interactions.Interaction)this.InteractionList[i]).InteractionId.Id == Interaction_Object.InteractionId.Id)
                                    {
                                        retIndex = i;
                                        break;
                                    }
                                }
                                if ((retIndex >= 0) && (retIndex < this.InteractionList.Count))
                                {
                                    ActiveNormalInteraction = (ININ.IceLib.Interactions.Interaction)this.InteractionList[retIndex];
                                    this.InteractionList.RemoveAt(retIndex);
                                }
                                else
                                {
                                    //
                                }
                            }
                            if (this.InteractionList != null)
                            {
                                if (this.InteractionList.Count <= 0)
                                {
                                    this.IsMuted = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        ActiveConsultInteraction = null;
                        ActiveConferenceInteraction = null;
                    }
                }
                else
                {
                    if (this.InteractionList != null)
                    {
                        this.InteractionList.Clear();
                    }
                    this.IsMuted = false;
                }
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                log.Error(scope + "Error info." + ex.Message);
            }
        }

        private Interaction GetAvailableInteractionFromList()
        {
            string scope = "CIC::frmMain::GetAvailableInteractionFromList()::";
            log.Info(scope + "Starting.");
            ININ.IceLib.Interactions.Interaction retInteraction = null;
            if (this.InteractionList != null)
            {
                if (this.InteractionList.Count > 0)
                {
                    foreach (ININ.IceLib.Interactions.Interaction CurrentInteraction in this.InteractionList)
                    {
                        if (CurrentInteraction != null)
                        {
                            if (!CurrentInteraction.IsDisconnected)
                            {
                                retInteraction = CurrentInteraction;
                                break;
                            }
                        }
                    }
                }
            }
            log.Info(scope + "Completed.");
            return retInteraction;
        }

        private void m_InteractionQueue_InteractionAdded(object sender, InteractionAttributesEventArgs e)
        {
            string scope = "CIC::MainForm::m_InteractionQueue_InteractionAdded():: ";
            log.Info(scope + "Starting.");
            try
            {
                if (!e.Interaction.IsWatching())
                {
                    e.Interaction.AttributesChanged += new EventHandler<AttributesEventArgs>(NormalInteraction_AttributesChanged);
                    e.Interaction.StartWatching(this.InteractionAttributes);
                }
                if (this.IsMuted)
                {
                    e.Interaction.Mute(true);
                }
                switch (e.Interaction.InteractionType)
                {
                    case InteractionType.Email:
                        ActiveNormalEmailInteraction = 
                            new EmailInteraction(this.NormalInterationManager, e.Interaction.InteractionId);
                        //this.SetActiveEmailQueue();
                        ActiveNormalInteraction = e.Interaction;
                        this.ShowActiveCallInfo();
                        break;
                    case InteractionType.Chat:
                        //
                        break;
                    case InteractionType.Callback:
                        this.ActiveCallbackInteraction = 
                            new CallbackInteraction(this.NormalInterationManager, e.Interaction.InteractionId);
                        ActiveNormalInteraction = e.Interaction;
                        this.StrConnectionState = ActiveNormalInteraction.State;
                        this.ShowActiveCallInfo();
                        break;
                    case InteractionType.Call:
                        if (!e.Interaction.IsDisconnected)
                        {
                            this.Add_InteractionListObject(e.Interaction);
                            ActiveNormalInteraction = e.Interaction;
                            this.StrConnectionState = ActiveNormalInteraction.State;
                            if (ActiveNormalInteraction.GetStringAttribute("CallerHost") != null)
                            {
                                if (ActiveNormalInteraction.GetStringAttribute("CallerHost").ToString().Trim() != "")
                                {
                                    this.CallerHost = ActiveNormalInteraction.GetStringAttribute("CallerHost");
                                }
                                else
                                {
                                    ININ.IceLib.Connection.SessionSettings session_Setting = 
                                        ActiveNormalInteraction.InteractionsManager.Session.GetSessionSettings();
                                    this.CallerHost = session_Setting.MachineName.ToString();
                                }
                            }
                            this.ShowActiveCallInfo();
                            if (!this.IsManualDialing)
                            {

                            }
                            else
                            {
                                this.IsManualDialing = false;
                            }
                        }
                        else
                        {
                        }
                        break;
                    default:
                        break;
                }
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                log.Error(scope + "Error info." + ex.Message);
            }
        }

        private void NormalInteraction_AttributesChanged(object sender, AttributesEventArgs e)
        {
            string scope = "CIC::MainForm::NormalInteraction_AttributesChanged():: ";
            log.Info(scope + "Starting.");
            try
            {
                if (ActiveNormalInteraction != null)
                {
                    this.StrConnectionState = ActiveNormalInteraction.State;
                }
                else
                {
                    if (this.InteractionList != null)
                    {
                        if (this.InteractionList.Count <= 0)
                        {
                            this.StrConnectionState = InteractionState.None;
                        }
                        else
                        {
                            //
                        }
                    }
                }
                log.Info(scope + "Completed.");
            }
            catch
            {
                //Get Connection State.
                this.StrConnectionState = InteractionState.None;
            }
        }

        private void Add_InteractionListObject(Interaction interaction)
        {
            int chk_idx = -1;
            string scope = "CIC::MainForm::Add_InteractionListObject():: ";
            log.Info(scope + "Starting.");
            try
            {
                if (interaction != null && this.InteractionList != null)
                {
                    for (int i = 0; i < this.InteractionList.Count; i++)
                    {
                        if (((Interaction)this.InteractionList[i]).InteractionId == interaction.InteractionId)
                        {
                            chk_idx = i;
                            break;
                        }
                    }
                    if ((chk_idx >= 0) && (chk_idx < this.InteractionList.Count))
                    {
                        //Update
                        this.InteractionList[chk_idx] = interaction;
                    }
                    else
                    {
                        //Insert
                        this.InteractionList.Add(interaction);
                    }
                }
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                log.Error(scope + "Error info." + ex.Message);
            }
        }

        private void Initialize_InteractionAttributes()
        {
            string scope = "CIC::MainForm::Initial_InteractionAttributes():: ";
            log.Info(scope + "Starting.");
            this.InteractionAttributes = new string[] 
            {
                InteractionAttributeName.AccountCodeId,
                InteractionAttributeName.AlertSound,
                InteractionAttributeName.CallIdKey,
                InteractionAttributeName.Capabilities,
                InteractionAttributeName.ClientMessage,
                InteractionAttributeName.ConferenceId,
                InteractionAttributeName.DeallocationTime,
                InteractionAttributeName.Direction,
                InteractionAttributeName.DisconnectionTime,
                InteractionAttributeName.DisconnectRingNoAnswer,
                InteractionAttributeName.ImmediateAccess,
                InteractionAttributeName.InitiationTime,
                InteractionAttributeName.InteractionId,
                InteractionAttributeName.InteractionType,
                InteractionAttributeName.LineQueueName,
                InteractionAttributeName.LocalAddress,
                InteractionAttributeName.LocalId,
                InteractionAttributeName.LocalName,
                InteractionAttributeName.Log,
                InteractionAttributeName.Monitors,
                InteractionAttributeName.MonitorsCombinedCount,
                InteractionAttributeName.MonitorType,
                InteractionAttributeName.Muted,
                InteractionAttributeName.Notes,
                InteractionAttributeName.OrbitQueueName,
                InteractionAttributeName.PopApplication,
                InteractionAttributeName.Private,
                InteractionAttributeName.Recorders,
                InteractionAttributeName.RecordersCombinedCount,
                InteractionAttributeName.RemoteAddress,
                InteractionAttributeName.RemoteId,
                InteractionAttributeName.RemoteName,
                InteractionAttributeName.State,
                InteractionAttributeName.StateDescription,
                InteractionAttributeName.StationQueueNames,
                InteractionAttributeName.SupervisorMonitors,
                InteractionAttributeName.SupervisorRecorders,
                InteractionAttributeName.UserQueueNames,
                InteractionAttributeName.WorkgroupQueueDisplayName,
                InteractionAttributeName.WorkgroupQueueName,
                InteractionAttributeName.WrapUpCodeId
            };
            log.Info(scope + "Completed.");
        }

        private void Initial_NormalInteraction()
        {
            string scope = "CIC::MainForm::Initial_NormalInteraction()::";
            log.Info(scope + "Starting.");
            try
            {
                log.Info(scope + "Getting an instance of Normal InteractionsManager.");
                this.NormalInterationManager = InteractionsManager.GetInstance(global::CIC.Program.m_Session);
                if (this.InteractionList == null)
                {
                    this.InteractionList = new System.Collections.ArrayList();
                }
                else
                {
                    this.InteractionList.Clear();
                }
                log.Info(scope + "Getting an instance of PeopleManager[Normal Interactions].");
                this.mPeopleManager = PeopleManager.GetInstance(this.IC_Session);
                //this.WebBrowserStatusToolStripStatusLabel.Text = "";
                //if (this.sCollectUserSelect != null)
                //{
                //    if (this.sCollectUserSelect.Trim() != "")
                //    {
                //        this.IVRMenu.Enabled = true;
                //    }
                //    else
                //    {
                //        this.IVRMenu.Enabled = false;
                //    }
                //}
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                log.Error(scope + "Error info." + ex.Message);
            }
        }

        private void SessionConnectCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string scope = "CIC::frmMain::SessionConnectCompleted()::";
            log.Info(scope + "Starting");
            this.MustChangePassword();
            log.Info(scope + "Completed.");
        }

        private void MustChangePassword()
        {
            string scope = "CIC::MainForm::MustChangePassword()::";
            log.Info(scope + "Starting.");
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(MustChangePassword));
            }
            else
            {
                if (global::CIC.Program.m_Session != null)
                {
                    ININ.IceLib.Connection.Extensions.Security SecurityObject = 
                        new ININ.IceLib.Connection.Extensions.Security(global::CIC.Program.m_Session);
                    ININ.IceLib.Connection.Extensions.PasswordPolicy passwordPolicyObject = SecurityObject.GetPasswordPolicy();
                    if (passwordPolicyObject.MustChangePassword)
                    {
                        this.ShowChangePasswordDialog();
                    }
                }
            }
            log.Info(scope + "Complete.");
        }

        private void ShowChangePasswordDialog()
        {
            CIC.frmChangePassword changePasswordObject = new frmChangePassword();
            changePasswordObject.Show();
        }

        private void mSession_Changed(object sender, ConnectionStateChangedEventArgs e)
        {
            string scope = "CIC::MainForm::mSession_Changed()::";
            log.Info(scope + "Starting.");

            // TODO: clean up this function
            Application.DoEvents();
            switch (e.State)
            {
                case ININ.IceLib.Connection.ConnectionState.Attempting:
                    this.BeginInvoke(new MethodInvoker(disconnect_state));
                    break;
                case ININ.IceLib.Connection.ConnectionState.Up:
                    if (this.IsActiveConnection == false)
                    {
                        global::CIC.Program.IcStation.ConnectionTimes = 0;
                        this.IsActiveConnection = true;       //Set to ActiveConnection.
                        //this.SetStatusBarStripMsg();
                    }
                    //this.BeginInvoke(new MethodInvoker(login_workflow)); 
                    
                    this.InitializeDialerSession();
                    this.SetActiveSession(Program.m_Session);
                    log.Info(scope + "Completed.");
                    this.Initial_NormalInteraction();
                    this.InitializeQueueWatcher();
                    this.BeginInvoke(new MethodInvoker(connected_state));
                    this.state_info_label.Text = "Connected to the server.";
                    break;
                case ININ.IceLib.Connection.ConnectionState.Down:
                    if (this.IsActiveConnection)
                    {
                        this.IsActiveConnection = false;       //Set to InActiveConnection.
                        this.DisposeQueueWatcher();
                        this.BeginInvoke(new MethodInvoker(disconnect_state));
                        //this.SetStatusBarStripMsg();
                    }

                    if (global::CIC.Program.m_Session != null)
                    {
                        global::CIC.Program.m_Session.Disconnect();
                        global::CIC.Program.m_Session = null;
                    }

                    if (!ExitFlag)
                    {
                        global::CIC.Program.m_Session = new Session();
                        global::CIC.Program.m_Session.ConnectionStateChanged +=
                            new EventHandler<ConnectionStateChangedEventArgs>(mSession_Changed);
                        global::CIC.Program.IcStation.CurrentSession = global::CIC.Program.m_Session;
                        try
                        {
                            global::CIC.Program.IcStation.ICConnect();
                        }
                        catch
                        {

                        }
                    }
                    //this.SetStatusBarStripMsg();
                    break;
                case ININ.IceLib.Connection.ConnectionState.None:
                    global::CIC.Program.IcStation.ICConnect();
                    //this.SetStatusBarStripMsg();
                    break;
                default:
                    break;
            }
        }

        private void DisposeQueueWatcher()
        {
            string scope = "CIC::MainForm::Dispose_QueueWatcher():: ";
            log.Info(scope + "Starting.");
            try
            {
                log.Info(scope + "Creating instance of InteractionQueue");
                if (this.m_InteractionQueue != null)
                {
                    log.Info(scope + "Attaching event handlers");
                    this.m_InteractionQueue.InteractionAdded -= new EventHandler<InteractionAttributesEventArgs>(this.m_InteractionQueue_InteractionAdded);
                    this.m_InteractionQueue.InteractionChanged -= new EventHandler<InteractionAttributesEventArgs>(m_InteractionQueue_InteractionChanged);
                    this.m_InteractionQueue.InteractionRemoved -= new EventHandler<InteractionEventArgs>(m_InteractionQueue_InteractionRemoved);
                    this.m_InteractionQueue.ConferenceInteractionAdded -= new EventHandler<ConferenceInteractionAttributesEventArgs>(m_InteractionQueue_ConferenceInteractionAdded);
                    this.m_InteractionQueue.ConferenceInteractionChanged -= new EventHandler<ConferenceInteractionAttributesEventArgs>(m_InteractionQueue_ConferenceInteractionChanged);
                    this.m_InteractionQueue.ConferenceInteractionRemoved -= new EventHandler<ConferenceInteractionEventArgs>(m_InteractionQueue_ConferenceInteractionRemoved);
                    this.m_InteractionQueue.StopWatchingAsync(null, null);
                    this.m_InteractionQueue = null;
                }
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                log.Error(scope + "Error info." + ex.Message);
            }
        }

        private bool SetActiveSession(ININ.IceLib.Connection.Session session)
        {
            bool bResult = false;
            string scope = "CIC::frmMain::SetActiveSession()::";
            log.Info(scope + "Starting.");
            if (session == null)
            {
                log.Warn(scope + "Null reference session.");
                throw new ArgumentNullException("Null reference session.");
            }
            else
            {
                log.Info(scope + "Completed.");
                bResult = true;
                this.IC_Session = session;
            }
            return bResult;
        }

        private void reset_timer()
        {
            if (!previewCallTimer.Enabled)
                previewCallTimer.Enabled = true;
            previewCallTimer.Stop();
            timer = global::CIC.Properties.Settings.Default.CountdownTime;
            log.Info("Timer is reset");
        }
        
        private void restart_timer()
        {
            reset_timer();
            //timer1.Start();
        }

        public void login_workflow()
        {
            this.workflow_button_Click(null, EventArgs.Empty);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            previewCallTimer.Enabled = false;
        }

        private void workflow_button_Click(object sender, EventArgs e)
        {
            if (this.IsActiveConnection)
            {
                frmWorkflow workflow = new CIC.frmWorkflow(global::CIC.Program.m_Session);
                workflow.Show();
            }
            else
            {
                state_change(FormMainState.Disconnected);
            }
        }

        private void call_button_Click(object sender, EventArgs e)
        {   
            //change state from workflow.
            // name1_panel.BackColor = Color.Yellow;
            // reset_timer();

            // make a call or pickup
            placecall_or_pickup();
                
            state_change(FormMainState.PreviewCall);
        }

        private void disconnect_button_Click(object sender, EventArgs e)
        {
            tryDisconnect();

            //this.ShowActiveCallInfo();
            this.reset_info_on_dashboard();
            state_info_label.Text = "Disconnected.";
            this.state_change(FormMainState.Preview);
        }

        private void tryDisconnect()
        {
            try
            {
                if (ActiveDialerInteraction != null &&
                    !ActiveDialerInteraction.IsDisconnected &&
                    (this.current_state == FormMainState.PreviewCall || this.current_state == FormMainState.ConferenceCall))
                {
                    frmDisposition disposition = new frmDisposition();
                    disposition.ShowDialog();
                }
                if (IcWorkFlow != null &&
                    IcWorkFlow.LoginResult &&
                    this.IC_Session != null &&
                    this.IC_Session.ConnectionState == ININ.IceLib.Connection.ConnectionState.Up)
                {
                    if (ActiveDialerInteraction != null)
                    {
                        if (!ActiveDialerInteraction.IsDisconnected)
                            ActiveDialerInteraction.Disconnect();
                        if (ActiveNormalInteraction != null && !ActiveNormalInteraction.IsDisconnected)
                        {
                            this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                            ActiveNormalInteraction.Disconnect();
                        }
                        if (ActiveConsultInteraction != null && !ActiveConsultInteraction.IsDisconnected)
                        {
                            this.RemoveNormalInteractionFromList(ActiveConsultInteraction);
                            ActiveConsultInteraction.Disconnect();
                        }
                    }
                }
                else
                { // Not Log On to Dialer Server.
                    if (this.ActiveDialerInteraction != null && !this.ActiveDialerInteraction.IsDisconnected)
                    {
                        this.ActiveDialerInteraction.Disconnect();
                        this.ActiveDialerInteraction = null;
                    }
                    if (ActiveNormalInteraction != null && !ActiveNormalInteraction.IsDisconnected)
                    {
                        this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                        ActiveNormalInteraction.Disconnect();
                        ActiveNormalInteraction = this.GetNormalInteractionFromList();
                    }
                    else
                    {
                        ActiveNormalInteraction = this.GetNormalInteractionFromList();
                        if (ActiveNormalInteraction != null)
                        {
                            ActiveNormalInteraction.Disconnect();
                        }
                    }
                    if (ActiveConsultInteraction != null && !ActiveConsultInteraction.IsDisconnected)
                    {
                        ActiveConsultInteraction.Disconnect();
                        ActiveConsultInteraction = null;
                    }

                    if (this.InteractionList.Count <= 0)
                    {
                        ActiveConferenceInteraction = null;
                        ActiveConsultInteraction = null;
                    }
                }

            }
            catch (Exception ex)
            {
                string output = String.Format("Something really bad happened: {0}", ex.Message);
                MessageBox.Show(output, "CIC Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                log.ErrorFormat("Something really bad happened: {0}", ex.Message);
            }
        }

        private Interaction GetNormalInteractionFromList()
        {
            string scope = "CIC::frmMain::GetNormalInteractionFromList()::";
            log.Info(scope + "Starting.");
            ININ.IceLib.Interactions.Interaction retInteraction = null;
            if (this.InteractionList != null)
            {
                if (InteractionList.Count > 0)
                {
                    foreach (Interaction CurrentInteraction in this.InteractionList)
                    {
                        retInteraction = CurrentInteraction;
                        break;
                    }
                }
            }
            log.Info(scope + "Completed.");
            return retInteraction;
        }

        private void hold_button_Click(object sender, EventArgs e)
        {
            if (IcWorkFlow.LoginResult)
            {
                if (ActiveNormalInteraction != null)
                {
                    if (ActiveNormalInteraction.IsMuted)
                    {
                        ActiveNormalInteraction.Mute(false);
                    }
                    ActiveNormalInteraction.Hold(!ActiveNormalInteraction.IsHeld);
                    state_change(FormMainState.Hold);
                    return;
                }

                if (this.ActiveDialerInteraction != null)
                {
                    if (!this.ActiveDialerInteraction.IsDisconnected)
                    {
                        if (this.ActiveDialerInteraction.IsMuted)
                        {
                            this.ActiveDialerInteraction.Mute(false);
                        }
                        this.ActiveDialerInteraction.Hold(!this.ActiveDialerInteraction.IsHeld);
                        state_change(FormMainState.Hold);
                    }
                }
            }
        }

        private void mute_button_Click(object sender, EventArgs e)
        {
            if (IcWorkFlow.LoginResult)
            {
                if (ActiveNormalInteraction != null)
                {
                    if (ActiveNormalInteraction.IsHeld)
                    {
                        ActiveNormalInteraction.Hold(false);
                    }
                    ActiveNormalInteraction.MuteAsync(!ActiveNormalInteraction.IsMuted, MuteCompleted, null);
                    state_change(FormMainState.Mute);
                    return;
                }

                if (this.ActiveDialerInteraction != null)
                {
                    if (this.ActiveDialerInteraction.IsHeld)
                    {
                        this.ActiveDialerInteraction.Hold(false);
                    }
                    this.ActiveDialerInteraction.MuteAsync(!this.ActiveDialerInteraction.IsMuted, MuteCompleted, null);
                    state_change(FormMainState.Mute);
                }
            }
        }

        private void transfer_button_Click(object sender, EventArgs e)
        {
            frmTransfer transfer = new frmTransfer();
            transfer.ShowDialog();
        }

        private void conference_button_Click(object sender, EventArgs e)
        {
            frmConference conference = frmConference.getInstance();
            conference.ShowDialog();
        }

        private void manual_call_button_Click(object sender, EventArgs e)
        {
            if (IcWorkFlow == null || !IcWorkFlow.LoginResult)
            {
                frmManualCall manualCall = new frmManualCall(NormalInterationManager);
                manualCall.ShowDialog();
                state_change(FormMainState.ManualCall);
            }
            else
            {
                this.manual_call_button.Enabled = false;
            }
        }

        private void break_button_Click(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::break_button_Click()::";
            log.Info(scope + "Starting.");
            try
            {
                if (!IcWorkFlow.LoginResult && this.ActiveDialerInteraction == null)
                {
                    break_requested = false;
                }
                else
                {
                    if (break_requested)
                    {
                        break_requested = false;
                    }
                    else
                    {
                        this.ActiveDialerInteraction.DialerSession.RequestBreak();
                        break_requested = true;
                        break_requested_state();
                    }
                }
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                log.ErrorFormat(scope + "Error info." + ex.Message);
            }
        }

        private void endbreak_button_Click(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::endbreak_button_Click()::";
            log.Info(scope + "Starting.");
            try
            {
                if (this.ActiveDialerInteraction == null)
                {
                    break_requested = false;
                }
                else
                {
                    if (this.current_state == FormMainState.Break)
                    {
                            this.ActiveDialerInteraction.DialerSession.EndBreak();
                            break_requested = false;
                            break_granted = false;
                            this.state_info_label.Text = "Break ended. Waiting for a new call from workflow.";
                            log.Info(scope + "Complete.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                log.ErrorFormat(scope + "Error info." + ex.Message);
            }
        }

        private void logout_workflow_button_Click(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::LogoutToolStripMenuItem_Click(): ";
            log.Info(scope + "Starting.");
            try
            {
                if (IcWorkFlow.LoginResult)
                {
                    // TODO: check the state of calling from the commented one
                    //if (this.CallStateToolStripStatusLabel.Text.ToLower().Trim() == "n/a")
                    if (this.current_state == FormMainState.Disconnected)
                    {
                        this.LogoutGranted(sender, e);      //No call object from this campaign;permit to logging out.
                    }
                    else
                    {
                        if (this.ActiveDialerInteraction != null)
                        {
                            if (!this.break_granted)
                            {
                                this.break_granted = true;
                                this.break_button_Click(sender, e);               //wait for breakgrant
                                this.ActiveDialerInteraction.DialerSession.RequestLogoutAsync(LogoutGranted, null);
                                
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
                    state_change(FormMainState.Loggedout);
                    if (ExitFlag)
                        this.Close();
                }
                
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                log.ErrorFormat(scope + "Error info." + ex.Message);
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
        
        public void disposition_invoke(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::DispositionToolStripButton_Click(): ";
            string sFinishcode = (String)sender;
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<EventArgs>(disposition_invoke), new object[] { sender, e });
            }
            else
            {
                log.Info(scope + "Starting.[Disposition]");
                try
                {
                    if (IcWorkFlow.LoginResult)
                    {
                        if (this.ActiveDialerInteraction != null)
                        {
                            ININ.IceLib.People.UserStatusUpdate statusUpdate = new UserStatusUpdate(this.mPeopleManager);
                            if (this.ActiveDialerInteraction.DialingMode == DialingMode.Preview ||
                                this.ActiveDialerInteraction.DialingMode == DialingMode.Regular)
                            {
                                ININ.IceLib.Dialer.ReasonCode sReasoncode = this.GetReasonCode(sFinishcode);
                                ININ.IceLib.Dialer.CallCompletionParameters callCompletionParameters = 
                                    new ININ.IceLib.Dialer.CallCompletionParameters(sReasoncode, sFinishcode);
                                if (this.ActiveDialerInteraction.IsDisconnected == false)
                                {
                                    this.ActiveDialerInteraction.Disconnect();
                                }

                                if (sReasoncode == ReasonCode.Scheduled)
                                {
                                    List<string> attributeNamesToWatch = this.SetAttributeList();
                                    if (!this.ActiveDialerInteraction.IsWatching())
                                    {
                                        this.ActiveDialerInteraction.StartWatching(attributeNamesToWatch.ToArray());
                                    }
                                    if (this.frmScheduleCallbackForm.ScheduleCallbackResult == false)
                                    {
                                        return;
                                    }
                                    this.ActiveDialerInteraction.ChangeWatchedAttributesAsync(
                                        attributeNamesToWatch.ToArray(), null, true, ChangeWatchedAttributesCompleted, null);
                                    this.ActiveDialerInteraction.CallComplete(
                                        new CallCompletionParameters(sReasoncode, sFinishcode, this.CallBackDateTime, this.ScheduleAgent, false));
                                }
                                else
                                {
                                    this.ActiveDialerInteraction.CallComplete(callCompletionParameters);
                                }

                                if (this.break_granted)
                                {
                                    if (this.AvailableStatusMessageDetails != null)
                                    {
                                        statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                                        statusUpdate.UpdateRequest();
                                    }
                                }

                                this.reset_info_on_dashboard();
                            }
                            else if (this.ActiveDialerInteraction.DialingMode == DialingMode.Precise)
                            {
                                //
                            }
                            else if (this.ActiveDialerInteraction.DialingMode == DialingMode.Agentless)
                            {
                                //
                            }
                            else if (this.ActiveDialerInteraction.DialingMode == DialingMode.OwnAgentCallback)
                            {
                                //
                            }
                            else if (this.ActiveDialerInteraction.DialingMode == DialingMode.OwnAgentCallback_Preview)
                            {
                                //
                            }
                            else
                            {
                                //Other Mode!
                            }
                        }
                    }

                    if (this.break_granted || current_state == FormMainState.ManualCall)
                    {
                        break_requested = false;
                        state_change(FormMainState.Break);
                    }
                    else
                    {
                        state_change(FormMainState.Preview);
                    }
                    log.Info(scope + "Completed.[Disposition]");
                }
                catch (ININ.IceLib.IceLibException ex)
                {
                    log.Error(scope + "Error info." + ex.Message);
                }
            }
        }

        public void workflow_invoke(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::WorkflowToolStripMenuItem_Click()::";
            log.Info(scope + "Starting.");
            try
            {
                log.Info(scope + "Logging into workflow. UserId=" + this.IC_Session.UserId + ", StationId=" + this.IC_Session.GetStationInfo().Id);
                IcWorkFlow = new CIC.ICWorkFlow(CIC.Program.DialingManager);
                this.DialerSession = IcWorkFlow.LogIn(((String)sender));
                //IcWorkFlow.LoginResult = IcWorkFlow.LoginResult;
                if (IcWorkFlow.LoginResult)
                {
                    this.InitializeDialerSession();
                    this.SetActiveSession(Program.m_Session);
                    log.Info(scope + "Completed.");
                    this.Initial_NormalInteraction();
                    this.InitializeQueueWatcher();
                    this.UpdateUserStatus();
                    this.ShowActiveCallInfo();
                }
                else
                {
                    log.Warn(scope + "WorkFlow [" + ((ToolStripMenuItem)sender).Text + "] logon Fail. Please try again.");
                }
            }
            catch (System.Exception ex)
            {
                this.state_change(FormMainState.Disconnected);
                log.Error(scope + "Error info.Logon to Workflow[" + ((ToolStripMenuItem)sender).Text + "] : " + ex.Message);
            }  
        }

        public void conference_invoke(string transferTxtDestination)
        {
            string scope = "CIC::frmMain::CreateConferenceToolStripButton_Click()::";
            log.Info(scope + "Starting.");
            int idx = 0;
            ININ.IceLib.Interactions.Interaction[] TmpInteraction;
            try
            {
                if (IcWorkFlow.LoginResult)
                {
                    if (this.ActiveDialerInteraction != null &&
                        ActiveNormalInteraction != null && 
                        this.InteractionList != null)
                    {
                        if (this.InteractionList.Count > 1)
                        {
                            TmpInteraction = new ININ.IceLib.Interactions.Interaction[this.InteractionList.Count];
                            foreach (ININ.IceLib.Interactions.Interaction interact in this.InteractionList)
                            {
                                if (interact.InteractionType == InteractionType.Call)
                                {
                                    if (!interact.IsDisconnected)
                                    {
                                        TmpInteraction[idx] = interact;
                                        idx++;
                                    }
                                }
                            }
                            this.NormalInterationManager.MakeNewConferenceAsync(TmpInteraction, MakeNewConferenceCompleted, null);
                        }
                    }
                }
                else
                {
                    if (ActiveNormalInteraction != null &&
                        this.InteractionList != null)
                    {
                        if (this.InteractionList.Count > 1)
                        {
                            TmpInteraction = new ININ.IceLib.Interactions.Interaction[this.InteractionList.Count];
                            foreach (ININ.IceLib.Interactions.Interaction interact in this.InteractionList)
                            {
                                if (interact.InteractionType == InteractionType.Call)
                                {
                                    if (!interact.IsDisconnected)
                                    {
                                        TmpInteraction[idx] = interact;
                                        idx++;
                                    }
                                }
                            }
                            this.NormalInterationManager.MakeNewConferenceAsync(TmpInteraction, MakeNewConferenceCompleted, null);
                        }
                    }
                }
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                // TODO: change state
                log.Error(scope + "Error info." + ex.Message);
            }
        }

        private void MakeNewConferenceCompleted(object sender, MakeNewConferenceCompletedEventArgs e)
        {
            string scope = "CIC::frmMain::MakeNewConferenceCompleted()::";
            log.Info(scope + "Starting.");
            try
            {
                //Conference variable
                ActiveConferenceInteraction = e.InteractionConference;
                bool ConferenceCancel = e.Cancelled;
                object ConferenceuserState = e.UserState;
                System.Exception ConferenceErrMsg = e.Error;
                ActiveConsultInteraction = null;
                state_info_label.Text = "Conferencing";
                state_change(FormMainState.ConferenceCall);
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                log.Error(scope + "Error info." + ex.Message);
            }
        }

        public void transfer_invoke(string transferTxtDestination)
        {
            string scope = "CIC::frmMain::TransferNowToolStripButton_Click()::";
            log.Info(scope + "Starting.");
            this.BlindTransferFlag = false;
            try
            {
                if (IcWorkFlow.LoginResult)
                {
                    if (this.ActiveDialerInteraction != null && this.current_state == FormMainState.PreviewCall)
                    {
                        if (ActiveConsultInteraction != null)
                        {
                            //Tracing.TraceNote(scope + "Performing consult transfer");
                            ActiveDialerInteraction.ConsultTransferAsync(ActiveConsultInteraction.InteractionId, TransferCompleted, null);
                            this.RemoveNormalInteractionFromList(ActiveConsultInteraction);

                            // complete workflow
                            string sFinishcode = global::CIC.Properties.Settings.Default.ReasonCode_Transfereded;
                            ININ.IceLib.Dialer.ReasonCode sReasoncode = ININ.IceLib.Dialer.ReasonCode.Transferred;
                            CallCompletionParameters callCompletionParameters = new CallCompletionParameters(sReasoncode, sFinishcode);
                            this.ActiveDialerInteraction.CallComplete(callCompletionParameters);

                            // update user status
                            ININ.IceLib.People.UserStatusUpdate statusUpdate = new UserStatusUpdate(this.mPeopleManager);
                            statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                            statusUpdate.UpdateRequest();
                        }
                    }
                    
                    if (ActiveNormalInteraction != null && this.current_state == FormMainState.ManualCall)
                    {
                        if (ActiveConsultInteraction != null)
                        {
                            log.Info(scope + "Performing consult transfer");
                            if (ActiveConsultInteraction.InteractionId != ActiveNormalInteraction.InteractionId &&
                                ActiveNormalInteraction.InteractionId != ActiveDialerInteraction.InteractionId)
                            {
                                ActiveNormalInteraction.ConsultTransferAsync(ActiveConsultInteraction.InteractionId, TransferCompleted, null);
                                this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                                this.RemoveNormalInteractionFromList(ActiveConsultInteraction);
                                this.BlindTransferFlag = true;
                            }
                            else
                            {
                                ActiveNormalInteraction = ActiveDialerInteraction;
                                if (this.InteractionList != null && this.InteractionList.Count > 1)
                                {
                                    foreach (ININ.IceLib.Interactions.Interaction CurrentInteraction in this.InteractionList)
                                    {
                                        if (CurrentInteraction.InteractionType == InteractionType.Call &&
                                            CurrentInteraction.InteractionId != ActiveConsultInteraction.InteractionId &&
                                            CurrentInteraction.InteractionId != ActiveDialerInteraction.InteractionId)
                                        {
                                                ActiveNormalInteraction = CurrentInteraction;  //Find Consult Call
                                                break;
                                        }
                                    }

                                    if (ActiveNormalInteraction != null)
                                    {
                                        ActiveNormalInteraction.ConsultTransferAsync(ActiveConsultInteraction.InteractionId, TransferCompleted, null);
                                        this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                                        this.RemoveNormalInteractionFromList(ActiveConsultInteraction);
                                        this.BlindTransferFlag = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            log.Info(scope + "Performing blind transfer");
                            if (transferTxtDestination != "")
                            {
                                ActiveNormalInteraction.BlindTransfer(transferTxtDestination);
                                this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                            }
                        }
                    }
                }
                this.ResetActiveCallInfo();
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                this.ResetActiveCallInfo();
                log.Error(scope + "Error info." + ex.Message);
            }
        }

        private void TransferCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.BlindTransferFlag = true;
            this.ShowActiveCallInfo();
            this.BlindTransferFlag = false;
            this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
            state_change(FormMainState.Connected);
            state_info_label.Text = "Transfer complete.";
        }

        private void UpdateUserStatus()
        {
            int iIndex = 0;
            int AvailableIndex = 0;
            string sIconName;
            string sIconPath = "";
            System.Drawing.Icon Status_icon = null;
            this.AllStatusMessageList = null;
            this.AllStatusMessageListOfUser = null;
            UserStatusUpdate statusUpdate = null;
            string scope = "CIC::frmMain::InitializeStatusMessageDetails()::";
            log.Info(scope + "Starting.");
            try
            {
                switch (IcWorkFlow.LoginResult)
                {
                    case true: //Log On to Workflow
                        if (this.mPeopleManager != null)
                        {
                            this.AllStatusMessageList = new StatusMessageList(this.mPeopleManager);
                            this.AllStatusMessageListOfUser = new UserStatusList(this.mPeopleManager);
                            this.AllStatusMessageListOfUser.WatchedObjectsChanged += new EventHandler<WatchedObjectsEventArgs<UserStatusProperty>>(AllStatusMessageListOfUser_WatchedObjectsChanged);
                            string[] dusers = { Program.DialingManager.Session.UserId };   //Make value to array 
                            this.AllStatusMessageListOfUser.StartWatching(dusers);
                            this.CurrentUserStatus = this.AllStatusMessageListOfUser.GetUserStatus(Program.DialingManager.Session.UserId);
                            sIconPath = CIC.Program.ResourcePath;
                            this.AllStatusMessageList.StartWatching();
                            foreach (StatusMessageDetails status in this.AllStatusMessageList.GetList())
                            {
                                sIconName = Util.GetFilenameFromFilePath(status.IconFileName.ToString());
                                sIconPath += sIconName;
                                if (System.IO.File.Exists(sIconPath))
                                {
                                    Status_icon = new System.Drawing.Icon(sIconPath);
                                }

                                if (status.MessageText.ToLower().Trim() == "available")
                                {
                                    AvailableStatusMessageDetails = status;
                                    AvailableIndex = iIndex;
                                }

                                if (status.MessageText.ToLower().Trim() == "do not disturb")
                                {
                                    //DoNotDisturbStatusMessageDetails = status;
                                }
                                iIndex++;
                                log.Info(scope + "Id=" + status.Id + ", MessageText=" + status.MessageText);
                            }
                        }
                        break;
                    default:    //Not Log On to Workflow
                        log.Info(scope + "Creating instance of StatusMessageList");
                        if (this.mPeopleManager != null)
                        {
                            string[] nusers = { this.IC_Session.UserId };   //Make value to array 
                            this.AllStatusMessageList = new StatusMessageList(this.mPeopleManager);
                            this.AllStatusMessageListOfUser = new UserStatusList(this.mPeopleManager);
                            this.AllStatusMessageListOfUser.WatchedObjectsChanged += 
                                new EventHandler<WatchedObjectsEventArgs<UserStatusProperty>>(AllStatusMessageListOfUser_WatchedObjectsChanged);
                            this.AllStatusMessageListOfUser.StartWatching(nusers);
                            this.CurrentUserStatus = this.AllStatusMessageListOfUser.GetUserStatus(this.IC_Session.UserId);
                            sIconPath = CIC.Program.ResourcePath;
                            this.AllStatusMessageList.StartWatching();
                            foreach (StatusMessageDetails status in this.AllStatusMessageList.GetList())
                            {
                                if (status.IsSelectableStatus)
                                {
                                    sIconName = Util.GetFilenameFromFilePath(status.IconFileName.ToString());
                                    sIconPath += sIconName;
                                    if (System.IO.File.Exists(sIconPath))
                                    {
                                        Status_icon = new System.Drawing.Icon(sIconPath);
                                    }
                                    if (status.MessageText.ToLower().Trim() == "available")
                                    {
                                        AvailableStatusMessageDetails = status;
                                        AvailableIndex = iIndex;
                                    }
                                    if (status.MessageText.ToLower().Trim() == "do not disturb")
                                    {
                                        //DoNotDisturbStatusMessageDetails = status;
                                    }
                                    iIndex++;
                                }
                               log.Info(scope + "Id=" + status.Id + ", MessageText=" + status.MessageText);
                            }
                        }
                        break;
                }
                //Set Current User Status Display 
                if (this.mPeopleManager != null)
                {
                    statusUpdate = new UserStatusUpdate(this.mPeopleManager);
                    if (this.CurrentUserStatus != null)
                    {
                        if (global::CIC.Properties.Settings.Default.AutoResetUserStatus)
                        {
                            statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                            statusUpdate.UpdateRequest();
                        }
                        else
                        {
                            if (this.CurrentUserStatus.StatusMessageDetails.IsSelectableStatus)
                            {
                                statusUpdate.StatusMessageDetails = this.CurrentUserStatus.StatusMessageDetails;
                                statusUpdate.UpdateRequest();
                            }
                            else
                            {
                                if (IcWorkFlow.LoginResult)
                                {
                                    statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                                    statusUpdate.UpdateRequest();
                                }
                                else
                                {
                                    //Display last user status.
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                log.Error(scope + "Error info." + ex.Message);
            }
        }

        private void AllStatusMessageListOfUser_WatchedObjectsChanged(object sender, WatchedObjectsEventArgs<UserStatusProperty> e)
        {
            throw new NotImplementedException();
        }

        private void ShowActiveCallInfo()
        {
            string scope = "CIC::frmMain::ShowActiveCallInfo()::";
            log.Info(scope + "Starting.");
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(ShowActiveCallInfo));
            }
            else
            {
                if (IcWorkFlow.LoginResult)
                {
                    if (this.ActiveDialerInteraction == null)
                    {
                        // TODO: disable direction|calltype|campaignID|queuename|number|callstate|callID
                        //this.ActiveConferenceInteraction = null;
                    }
                    else
                    {
                        if (this.ActiveDialerInteraction.DialingMode == DialingMode.Regular)
                        {
                            if (global::CIC.Properties.Settings.Default.AutoAnswer)
                            {
                                this.pickup();
                                //this.PickupToolStripButton_Click(this, new EventArgs());
                            }
                        }
                        /* TODO: show call ID
                         *       set call type to campaign call
                         *       set call state
                         *       set dialer number
                         */
                        update_info_on_dashboard();
                        this.toolStripCallIDLabel.Text = ActiveDialerInteraction.CallIdKey.ToString().Trim();
                        this.toolStripDirectionLabel.Text = ActiveDialerInteraction.Direction.ToString();
                        this.toolStripCallTypeLabel.Text = "Campaign Call(" + ActiveDialerInteraction.DialingMode.ToString() +")";
                        try
                        {
                            this.toolStripCampaignIDLabel.Text = mDialerData[Properties.Settings.Default.Preview_Campaign_ATTR];
                        }
                        catch (Exception ex)
                        { }
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
                    }
                    else
                    {
                        if (this.BlindTransferFlag)
                        {
                            // TODO: disable direction|calltype|campaignID|queuename|number|callstate|callID
                            //this.ActiveConferenceInteraction = null;
                        }
                        else
                        {
                            switch (ActiveNormalInteraction.State)
                            {
                                case InteractionState.None:
                                    break;
                                case InteractionState.Held:
                                    if (this.SwapPartyFlag)
                                    {
                                        this.SetActiveCallInfo();
                                        this.ShowActiveCallInfo();
                        
                                        // restart timer and reset call index
                                        this.BeginInvoke(new MethodInvoker(restart_timer));
                                    }
                                    else
                                    {
                                        // TODO: set info as below
                                        if (ActiveNormalInteraction.IsMuted)
                                            this.toolStripStatus.Text = "Muted";
                                        else
                                            this.toolStripStatus.Text = ActiveNormalInteraction.State.ToString();


                                        this.toolStripCallIDLabel.Text = "N/A";
                                        this.toolStripDirectionLabel.Text = ActiveNormalInteraction.Direction.ToString();
                                        this.toolStripCallTypeLabel.Text = ActiveNormalInteraction.InteractionType.ToString();
                                        try
                                        {
                                            this.toolStripCampaignIDLabel.Text = "Non-campaign Call";
                                        }
                                        catch (Exception ex)
                                        { }
                                        //this.DirectiontoolStripStatus.Text = ActiveNormalInteraction.Direction.ToString();
                                        //this.CallTypeToolStripStatusLabel.Text = ActiveNormalInteraction.InteractionType.ToString();
                                        //this.CampaignIdToolStripStatusLabel.Text = "Non-campaign Call";
                                        //this.QueueNameToolStripStatusLabel.Text = ActiveNormalInteraction.WorkgroupQueueName.ToString();
                                        //this.NumberToolStripStatusLabel.Text = ActiveNormalInteraction.RemoteDisplay.ToString();
                                    }
                                    //this.CallIdToolStripStatusLabel.Text = this.ActiveNormalInteraction.CallIdKey.ToString().Trim();
                                    break;
                                case InteractionState.Connected:
                                    if (ActiveNormalInteraction.IsMuted)
                                        this.toolStripStatus.Text = "Muted";
                                    else
                                        this.toolStripStatus.Text = ActiveNormalInteraction.State.ToString();
                                    // TODO: set info as below
                                    this.toolStripCallIDLabel.Text = "N/A";
                                    this.toolStripDirectionLabel.Text = ActiveNormalInteraction.Direction.ToString();
                                    this.toolStripCallTypeLabel.Text = ActiveNormalInteraction.InteractionType.ToString();
                                    try
                                    {
                                        this.toolStripCampaignIDLabel.Text = "Non-campaign Call";
                                    }
                                    catch (Exception ex)
                                    { }
                                    //this.DirectiontoolStripStatus.Text = ActiveNormalInteraction.Direction.ToString();
                                    //this.CallTypeToolStripStatusLabel.Text = ActiveNormalInteraction.InteractionType.ToString();
                                    //this.CampaignIdToolStripStatusLabel.Text = "Non-campaign Call";
                                    //this.QueueNameToolStripStatusLabel.Text = ActiveNormalInteraction.WorkgroupQueueName.ToString();
                                    //this.NumberToolStripStatusLabel.Text = ActiveNormalInteraction.RemoteDisplay.ToString();
                                    break;
                                default:
                                    // TODO: set info as below
                                    this.toolStripCallIDLabel.Text = "N/A";
                                    this.toolStripDirectionLabel.Text = ActiveNormalInteraction.Direction.ToString();
                                    this.toolStripCallTypeLabel.Text = ActiveNormalInteraction.InteractionType.ToString();
                                    try
                                    {
                                        this.toolStripCampaignIDLabel.Text = "Non-campaign Call";
                                    }
                                    catch (Exception ex)
                                    { }
                                    //this.DirectiontoolStripStatus.Text = ActiveNormalInteraction.Direction.ToString();
                                    //this.CallTypeToolStripStatusLabel.Text = ActiveNormalInteraction.InteractionType.ToString();
                                    //this.CampaignIdToolStripStatusLabel.Text = "Non-campaign Call";
                                    //this.QueueNameToolStripStatusLabel.Text = ActiveNormalInteraction.WorkgroupQueueName.ToString();
                                    this.toolStripStatus.Text = ActiveNormalInteraction.State.ToString();
                                    break;
                            }
                        }
                     }
                }
                this.SetInfoBarColor();
                update_conference_status();
            }
            log.Info(scope + "Completed.");
        }

        private void update_info_on_dashboard()
        {
            Dictionary<string, string> data = this.ActiveDialerInteraction.ContactData;
            this.contractNo_box.Text = data.ContainsKey("is_attr_ContractNumber") ? data["is_attr_ContractNumber"] : "";
            this.license_plate_box.Text = data.ContainsKey("is_attr_CarLicenseNumber") ? data["is_attr_CarLicenseNumber"] : "";
            this.product_name_box.Text = data.ContainsKey("is_attr_ProductName") ? data["is_attr_ProductName"] : "";
            this.name1_box1.Text = data.ContainsKey("is_attr_FullName_Relation1") ? data["is_attr_FullName_Relation1"] : "";
            this.name2_box1.Text = data.ContainsKey("is_attr_FullName_Relation2") ? data["is_attr_FullName_Relation2"] : "";
            this.name3_box1.Text = data.ContainsKey("is_attr_FullName_Relation3") ? data["is_attr_FullName_Relation3"] : "";
            this.name4_box1.Text = data.ContainsKey("is_attr_FullName_Relation4") ? data["is_attr_FullName_Relation4"] : "";
            this.name5_box1.Text = data.ContainsKey("is_attr_FullName_Relation5") ? data["is_attr_FullName_Relation5"] : "";
            this.name6_box1.Text = data.ContainsKey("is_attr_FullName_Relation6") ? data["is_attr_FullName_Relation6"] : "";
            this.name1_box2.Text = data.ContainsKey("is_attr_PhoneNo1") ? data["is_attr_PhoneNo1"] : "";
            this.name2_box2.Text = data.ContainsKey("is_attr_PhoneNo2") ? data["is_attr_PhoneNo2"] : "";
            this.name3_box2.Text = data.ContainsKey("is_attr_PhoneNo3") ? data["is_attr_PhoneNo3"] : "";
            this.name4_box2.Text = data.ContainsKey("is_attr_PhoneNo4") ? data["is_attr_PhoneNo4"] : "";
            this.name5_box2.Text = data.ContainsKey("is_attr_PhoneNo5") ? data["is_attr_PhoneNo5"] : "";
            this.name6_box2.Text = data.ContainsKey("is_attr_PhoneNo6") ? data["is_attr_PhoneNo6"] : "";
            this.aging_box.Text = data.ContainsKey("is_attr_Aging") ? data["is_attr_Aging"] : "";
            this.number_due_box.Text = data.ContainsKey("is_attr_NumberDue") ? data["is_attr_NumberDue"] : "";
            this.last_date_payment_box.Text = data.ContainsKey("is_attr_LastReceiveDatePayment") ? getDateTimeString( data["is_attr_LastReceiveDatePayment"]) : "";
            this.debt_status_box.Text = data.ContainsKey("is_attr_DebtStatus") ? data["is_attr_DebtStatus"] : "";
            this.start_overdue_date_box.Text = data.ContainsKey("is_attr_StartOverDueDate") ? getDateTimeString( data["is_attr_StartOverDueDate"]) : "";
            this.followup_status_box.Text = data.ContainsKey("is_attr_FollowupStatus") ? data["is_attr_FollowupStatus"] : "";
            this.payment_appoint_box.Text = data.ContainsKey("is_attr_PaymentAppoint") ? getDateTimeString(data["is_attr_PaymentAppoint"]) : "";
            this.date_callback_box.Text = data.ContainsKey("is_attr_DateAppointCallBack") ? getDateTimeString( data["is_attr_DateAppointCallBack"], destFormat: "dd/MM/yyyy HH:mm") : "";
            this.callingNumber = data.ContainsKey("is_attr_numbertodial") ? data["is_attr_numbertodial"] : "";

            update_currency_on_dashboard(data);

            this.state_info_label.Text = "Next Calling Number: " + callingNumber;
            this.CrmScreenPop();
        }

        private void update_currency_on_dashboard(Dictionary<string, string> data)
        {
            // deal with currency data
            string lastReceiveAmount = data.ContainsKey("is_attr_LastReceiveAmountPayment") ? data["is_attr_LastReceiveAmountPayment"] : "";
            string initialAmount = data.ContainsKey("is_attr_InitialAmount") ? data["is_attr_InitialAmount"] : "";
            string monthlyPayment = data.ContainsKey("is_attr_MonthlyPayment") ? data["is_attr_MonthlyPayment"] : "";
            string baseDebt = data.ContainsKey("is_attr_BaseDebt") ? data["is_attr_BaseDebt"] : "";
            try
            {
                this.last_amount_payment_box.Text = double.Parse(lastReceiveAmount).ToString("C2", CultureInfo.CreateSpecificCulture("th"));
            }
            catch (Exception ex)
            {
                this.last_amount_payment_box.Text = lastReceiveAmount;
                log.Error("the data in last_amount_payment_box cannot be parse to currency format");
            }

            try
            {
                this.initial_amount_box.Text = decimal.Parse(initialAmount).ToString("C2", CultureInfo.CreateSpecificCulture("th"));
            }
            catch (Exception ex)
            {
                this.initial_amount_box.Text = initialAmount;
                log.Error("the data in initial_amount_box cannot be parse to currency format");
            }

            try
            {
                this.monthly_payment_box.Text = decimal.Parse(monthlyPayment).ToString("C2", CultureInfo.CreateSpecificCulture("th"));
            }
            catch (Exception ex)
            {
                this.monthly_payment_box.Text = monthlyPayment;
                log.Error("the data in monthly_payment_box cannot be parse to currency format");
            }

            try
            {
                this.base_debt_box.Text = decimal.Parse(baseDebt).ToString("C2", CultureInfo.CreateSpecificCulture("th"));
            }
            catch (Exception ex)
            {
                this.base_debt_box.Text = baseDebt;
                log.Error("the data in base_debt_box cannot be parse to currency format");
            }
        }

        private void update_conference_status()
        {
            // TODO update this method
            string scope = "CIC::FormMain::update_conference_status()::";
            log.Info(scope + "Started.");

            //this.Set_ConferenceToolStrip();
            if (this.InteractionList != null && this.InteractionList.Count <= 0)
            {
                    ActiveConsultInteraction = null;
                    this.IsActiveConference_flag = false;
                    ActiveConferenceInteraction = null;
                    if (ActiveNormalInteraction != null)
                    {
                        ActiveNormalInteraction.Disconnect();
                        ActiveNormalInteraction = null;
                    }
                    if (!IcWorkFlow.LoginResult)
                    {
                        this.ActiveDialerInteraction = null;
                    }
            }
            log.Info(scope + "Completed.");
        }

        private void SetActiveCallInfo()
        {
            string scope = "CIC::frmMain::SetActiveCallInfo()::";
            log.Info(scope + "Starting.");
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
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                log.Error(scope + "Error info." + ex.Message);
            }
        }

        private void InitializeDialerSession()
        {
            string scope = "CIC::MainForm::RegisterHandlers()::";
            log.Info(scope + "Starting.");
            try
            {
                this.DialerSession.PreviewCallAdded += new EventHandler<ININ.IceLib.Dialer.PreviewCallAddedEventArgs>(PreviewCallAdded);
                this.DialerSession.DataPop += new EventHandler<ININ.IceLib.Dialer.DataPopEventArgs>(DataPop);
                this.DialerSession.CampaignTransition += new EventHandler<CampaignTransistionEventArgs>(CampaignTransition);
                this.DialerSession.BreakGranted += new EventHandler(BreakGranted);
                this.DialerSession.LogoutGranted += new EventHandler(LogoutGranted);
                Program.mDialingManager.WorkflowStopped += new EventHandler<WorkflowStoppedEventArgs>(WorkflowStopped);
                Program.mDialingManager.WorkflowStarted += new EventHandler<WorkflowStartedEventArgs>(WorkflowStarted);
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                log.Error(scope + "Error info." + ex.Message);
            }
        }

        private void DisposeDialerSession()
        {
            string scope = "CIC::MainForm::DisposeDialerSession()::";
            log.Info(scope + "Starting.");
            try
            {
                this.DialerSession.PreviewCallAdded -= new EventHandler<ININ.IceLib.Dialer.PreviewCallAddedEventArgs>(PreviewCallAdded);
                this.DialerSession.DataPop -= new EventHandler<ININ.IceLib.Dialer.DataPopEventArgs>(DataPop);
                this.DialerSession.CampaignTransition -= new EventHandler<CampaignTransistionEventArgs>(CampaignTransition);
                this.DialerSession.BreakGranted -= new EventHandler(BreakGranted);
                this.DialerSession.LogoutGranted -= new EventHandler(LogoutGranted);
                Program.mDialingManager.WorkflowStopped -= new EventHandler<WorkflowStoppedEventArgs>(WorkflowStopped);
                Program.mDialingManager.WorkflowStarted -= new EventHandler<WorkflowStartedEventArgs>(WorkflowStarted);
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                log.Error(scope + "Error info." + ex.Message);
            }
        }

        private void exit_button_Click(object sender, EventArgs e)
        {
            this.ExitFlag = true;
            this.Close();
        }

        private void state_change(FormMainState state)
        {
            // TODO: implement all states
            switch (state)
                {
                    case FormMainState.Preview:
                        preview_state();
                        log.Info("State Changed: Preview");
                        break;
                    case FormMainState.Connected:
                        connected_state();
                        log.Info("State Changed: Connected");
                        break;
                    case FormMainState.Calling:
                        calling_state();
                        log.Info("State Changed: Calling");
                        break;
                    case FormMainState.PreviewCall :
                        preview_call_state();
                        log.Info("State Changed: Preview Call");
                        break;
                    case FormMainState.ConferenceCall:
                        preview_call_state();
                        log.Info("State Changed: Conference Call");
                        break;
                    case FormMainState.ManualCall:
                        preview_call_state();
                        log.Info("State Changed: Manual Call");
                        break;
                    case FormMainState.Hold:
                        switch (current_state)
                        {
                            // case calling state -> change to hold state
                            case FormMainState.PreviewCall:
                            case FormMainState.ConferenceCall:
                            case FormMainState.ManualCall:
                                hold_button.Text = "Unhold";
                                state_info_label.Text = "Hold call from: " + callingNumber;
                                hold_state();
                                log.Info("State Changed: Hold");
                                break;
                            // case Mute state -> change to hold state.
                            case FormMainState.Mute:
                                hold_button.Text = "Unhold";
                                mute_button.Text = "Mute";
                                state_info_label.Text = "Hold call from: " + callingNumber;
                                hold_state();
                                log.Info("State Changed: Hold");
                                break;
                            // case Hold state -> change to calling state
                            case FormMainState.Hold:
                                hold_button.Text = "Hold";
                                state_info_label.Text = "Continue call from: " + callingNumber;
                                state_change(FormMainState.PreviewCall);
                                log.Info("State Changed: Unhold -> Preview Call");
                                break;
                        }
                        break;
                    case FormMainState.Mute:
                        switch (current_state)
                        {
                            // case calling state -> change to hold state
                            case FormMainState.PreviewCall:
                            case FormMainState.ConferenceCall:
                            case FormMainState.ManualCall:
                                mute_button.Text = "Unmute";
                                state_info_label.Text = "Mute call from: " + callingNumber;
                                mute_state();
                                log.Info("State Changed: Mute");
                                break;
                            // case Mute state -> change to hold state.
                            case FormMainState.Hold:
                                mute_button.Text = "Unmute";
                                hold_button.Text = "Hold";
                                state_info_label.Text = "Mute call from: " + callingNumber;
                                mute_state();
                                log.Info("State Changed: Mute");
                                break;
                            // case Mute state -> change to calling state
                            case FormMainState.Mute:
                                mute_button.Text = "Mute";
                                state_info_label.Text = "Continue call from: " + callingNumber;
                                state = FormMainState.PreviewCall;
                                state_change(FormMainState.PreviewCall);
                                log.Info("State Changed: Unmute -> Preview Call");
                                break;
                        }
                        break;
                    case FormMainState.Disconnected:
                        disconnect_state();
                        log.Info("State Changed: Disconnected");
                        break;
                    case FormMainState.Break:
                        if (break_requested)
                        {
                            break_state();
                            log.Info("State Changed: Break");
                        }
                        else
                        {
                            preview_state();
                            log.Info("State Changed: Unbreak -> Preview");
                        }
                        break;
                    case FormMainState.Loggedout:
                        logged_out_state();
                        log.Info("State Changed: Logged Out");
                        break;
                }
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

        private void connected_state()
        {
            // starts the next number in line
            // timer1.Start();

            reset_state();
            workflow_button.Enabled = true;
            manual_call_button.Enabled = true;
            logout_workflow_button.Enabled = true;
            exit_button.Enabled = true;

            prev_state = current_state;
            current_state = FormMainState.Connected;
        }

        private void preview_state()
        {
            // starts the next number in line
            // timer1.Start();

            reset_state();
            call_button.Enabled = true;
            break_button.Enabled = true;

            prev_state = current_state;
            current_state = FormMainState.Preview;
            state_info_label.Text = "Acquired information from workflow.";
        }

        private void calling_state()
        {
            reset_state();
            disconnect_button.Enabled = true;

            prev_state = current_state;
            current_state = FormMainState.Calling;
            state_info_label.Text = "Calling: " + callingNumber;
        }

        private void preview_call_state()
        {
            reset_state();
            disconnect_button.Enabled = true;
            hold_button.Enabled = true;
            mute_button.Enabled = true;
            transfer_button.Enabled = true;
            conference_button.Enabled = true;
            break_button.Enabled = true;

            prev_state = current_state;
            current_state = FormMainState.PreviewCall;
        }

        private void hold_state()
        {
            reset_state();
            disconnect_button.Enabled = true;
            hold_button.Enabled = true;
            mute_button.Enabled = true;
            break_button.Enabled = !break_requested;

            if (current_state != FormMainState.Mute)
            {
                prev_state = current_state;
            }
            current_state = FormMainState.Hold;
            state_info_label.Text = "Connected to: " + callingNumber + " (Held)";
        }

        private void disconnect_state()
        {
            reset_state();
            exit_button.Enabled = true;
            // calling a new number
            reset_timer();
            prev_state = current_state;
            current_state = FormMainState.Disconnected;
        }

        private void mute_state()
        {
            reset_state();
            disconnect_button.Enabled = true;
            hold_button.Enabled = true;
            mute_button.Enabled = true;
            break_button.Enabled = !break_requested;

            if (current_state != FormMainState.Hold)
            {
                prev_state = current_state;
            }
            current_state = FormMainState.Mute;
            state_info_label.Text = "Connected to: " + callingNumber + " (Muted)";
        }

        private void break_state()
        {
            reset_state();
            manual_call_button.Enabled = true;
            endbreak_button.Enabled = true;
            logout_workflow_button.Enabled = true;

            prev_state = current_state;
            current_state = FormMainState.Break;
        }
        
        private void break_requested_state()
        {
            break_button.Enabled = break_requested;
        }

        private void logged_out_state()
        {
            reset_state();
            workflow_button.Enabled = true;
            manual_call_button.Enabled = true;
            exit_button.Enabled = true;

            prev_state = current_state;
            current_state = FormMainState.Loggedout;
        }

        private void disable_break_request()
        {
            break_button.Enabled = false;
        }
            
        private void disable_logout()
        {
            logout_workflow_button.Enabled = false;
        }

        private void enable_transfer()
        {
            transfer_button.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer -= (float)previewCallTimer.Interval / 1000;
            timer_info.Text = "Time until call: " + timer.ToString("F1");
            if (timer <= 0)
            {
                reset_timer();
                
                // make a call or pickup
                placecall(sender, e);
                
                state_change(FormMainState.PreviewCall);
            }
        }

        public void MakePreviewCallComplete(object sender, AsyncCompletedEventArgs e)
        {
            //state_info_label.Text = "Connected to: " + this.ActiveDialerInteraction.ContactData["is_attr_numbertodial"];
            state_change(FormMainState.Calling);
        }

        public void MakeCallCompleted(object sender,InteractionCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                ActiveNormalInteraction = e.Interaction;
            }
            // TODO: should update message to conntected to: ####
            //state_info_label.Text = "Connected to: " + ActiveNormalInteraction.
            state_change(FormMainState.ManualCall);
        }

        private void ChangeWatchedAttributesCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.StackTrace, e.Error.Message);
                log.Warn(e.Error.StackTrace + " :: " + e.Error.Message);
                return;
            }
        }

        private void placecall_or_pickup()
        {
            if (!IcWorkFlow.LoginResult)
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

        /****************************************************
        *****************************************************
        ******************* The Logic Part ******************
        *****************************************************
        ****************************************************/

        private void WorkflowStarted(object sender, WorkflowStartedEventArgs e)
        {
            string scope = "CIC::MainForm::WorkflowStarted()::";
            log.Info(scope + "Starting.");

            disable_logout();
            log.Info(scope + "Completed.");
        }

        private void WorkflowStopped(object sender, WorkflowStoppedEventArgs e)
        {
            string scope = "CIC::MainForm::WorkflowStopped()::";
            log.Info(scope + "Starting.");
            //this.TransferPanelToolStripButton.Enabled = true;
            //this.RequestBreakToolStripButton.Visible = false;
            enable_transfer();
            this.disable_break_request();
            log.Info(scope + "Completed.");
        }

        private void CampaignTransition(object sender, CampaignTransistionEventArgs e)
        {
            // NYI
            int i = 0;
        }

        /*
         * convert a string of dateTime from `oldFormat` into `destFormat`
         */
        private string getDateTimeString(String datetime, 
            String oldFormat = "yyyy-dd-MM HH:mm", String destFormat = "dd/MM/yyyy")
        {
            try
            { 
                DateTime dt = DateTime.ParseExact(datetime, oldFormat,
                                       null);
                return  dt.ToString(destFormat);
            }
            catch (Exception ex)
            {

            }
            return "";
        }

        private ReasonCode GetReasonCode(string sFinishcode)
        {
            ININ.IceLib.Dialer.ReasonCode sRet = 0;
            switch (sFinishcode.ToLower().Trim())
            {
                case "busy":
                    sRet = ReasonCode.Busy;
                    break;
                case "deleted":
                    sRet = ReasonCode.Deleted;
                    break;
                case "failure":
                    sRet = ReasonCode.Failure;
                    break;
                case "fax":
                    sRet = ReasonCode.Fax;
                    break;
                case "machine":
                    sRet = ReasonCode.Machine;
                    break;
                case "noanswer":
                    sRet = ReasonCode.NoAnswer;
                    break;
                case "remotehangup":
                    sRet = ReasonCode.RemoteHangup;
                    break;
                case "scheduled":
                    sRet = ReasonCode.Scheduled;
                    break;
                case "sit":
                    sRet = ReasonCode.SIT;
                    break;
                case "wrongparty":
                    sRet = ReasonCode.WrongParty;
                    break;
                case "success":
                    sRet = ReasonCode.Success;
                    break;
                default:
                    sRet = ReasonCode.Success;
                    break;
            }
            
            return sRet;
        }

        private void DataPop(object sender, DataPopEventArgs e)
        {
            string scope = "CIC::MainForm::DataPop()::";
            log.Info(scope + "Starting.");
            try
            {
                if (!e.Interaction.IsWatching())
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

                        // restart timer and reset call index
                        this.BeginInvoke(new MethodInvoker(restart_timer));
                        this.BeginInvoke(new MethodInvoker(preview_call_state));
                        break;
                    case InteractionType.Call:
                        this.Initialize_ContactData();
                        this.ShowActiveCallInfo();

                        // restart timer and reset call index
                        this.BeginInvoke(new MethodInvoker(restart_timer));
                        this.BeginInvoke(new MethodInvoker(preview_call_state));
                        //this.state_change(FormMainState.Preview);
                        break;
                }
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                log.Error(scope + "Error info : " + ex.Message);
            }
        }

        // Get new infomation set
        private void PreviewCallAdded(object sender, PreviewCallAddedEventArgs e)
        {
            string scope = "CIC::MainForm::PreviewCallAdded()::";
            log.Info(scope + "Starting.");
            try
            {
                if (!e.Interaction.IsWatching())
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

                        // restart timer and reset call index
                        this.BeginInvoke(new MethodInvoker(restart_timer));
                        this.BeginInvoke(new MethodInvoker(preview_state));
                        
                        break;
                    case InteractionType.Call:
                        this.Initialize_ContactData();
                        this.ShowActiveCallInfo();

                        // restart timer and reset call index
                        this.BeginInvoke(new MethodInvoker(restart_timer));
                        // TODO: need to check whether it is predictive or preview
                        this.BeginInvoke(new MethodInvoker(preview_state));
                        break;
                }
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                log.Error(scope + "Error info : " + ex.Message);
            }
        }

        private void CrmScreenPop()
        {
            string scope = "CIC::MainForm::CrmScreenPop()::";
            log.Info(scope + "Starting.");
            string FullyUrl = "";
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(CrmScreenPop));
            }
            else
            {
                try
                {
                    // http://[MS-CRM Server name]/CRM/main.aspx?
                    // etn=col_collection_history&pagetype=entityrecord&extraqs=col_contract_no={Parameter1}&col_ phone_id={Parameter2}&col_call_id={Parameter3}
                    // Parameter1 = ProductRefID (ฟิลด์ที่ 1 ใน spec) is_attr_ProductRefID
                    // Parameter2 = Ref_PhoneNo1 - Ref_PhoneNo6  (ฟิลด์ที่ 32-37 ใน spec อยู่ที่ขณะนั้นโทรติดที่ 
                    // PhoneNoอะไร) is_attr_Ref_PhoneNo1
                    // Parameter3 = Call_id (Id ของการโทรออก) is_attr_callid
                    Dictionary<string, string> data = ActiveDialerInteraction.ContactData;
                    string productID = data.ContainsKey("is_attr_numbertodial") ? data["is_attr_numbertodial"] : "";
                    string refCallID = getRefCallID(data);
                    string callID = data.ContainsKey("is_attr_callid") ? data["is_attr_callid"] : "";

                    string baseURI = "http://" + global::CIC.Properties.Settings.Default["MSCRMServerName"];
                    baseURI += "etn=col_collection_history&";
                    baseURI += "pagetype=entityrecord&";
                    baseURI += string.Format("extraqs=col_contract_no={0}&", productID);
                    baseURI += string.Format("col_ phone_id={0}&", refCallID);
                    baseURI += string.Format("col_call_id={0}", callID);

                    Process.Start("baseURI");
                    log.Info(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    log.Error(scope + "Error info : " + ex.Message);
                    //this.MainWebBrowser.Url = new System.Uri(global::CIC.Properties.Settings.Default.StartupUrl, System.UriKind.Absolute);
                }
            }
        }

        private string getRefCallID(Dictionary<string, string> data)
        {
            string refCallID = "";
            if (data.ContainsKey("is_attr_Ref_PhoneNo1") && data["is_attr_Ref_PhoneNo1"] == callingNumber)
                refCallID = data["is_attr_Ref_PhoneNo1"];
            if (data.ContainsKey("is_attr_Ref_PhoneNo2") && data["is_attr_Ref_PhoneNo2"] == callingNumber)
                refCallID = data["is_attr_Ref_PhoneNo2"];
            if (data.ContainsKey("is_attr_Ref_PhoneNo3") && data["is_attr_Ref_PhoneNo3"] == callingNumber)
                refCallID = data["is_attr_Ref_PhoneNo3"];
            if (data.ContainsKey("is_attr_Ref_PhoneNo4") && data["is_attr_Ref_PhoneNo4"] == callingNumber)
                refCallID = data["is_attr_Ref_PhoneNo4"];
            if (data.ContainsKey("is_attr_Ref_PhoneNo5") && data["is_attr_Ref_PhoneNo5"] == callingNumber)
                refCallID = data["is_attr_Ref_PhoneNo5"];
            if (data.ContainsKey("is_attr_Ref_PhoneNo6") && data["is_attr_Ref_PhoneNo6"] == callingNumber)
                refCallID = data["is_attr_Ref_PhoneNo6"];

            return refCallID;
        }

        private void Initialize_ContactData()
        {
            string scope = "CIC::MainForm::InitialContactData()::";
            log.Info(scope + "Starting.");
            int i = 0;
            System.Data.DataTable dt = new DataTable("ATTR_TABLE");
            dt.Columns.Add("id");
            dt.Columns.Add("attr_key");
            dt.Columns.Add("attr_value");
            if (this.ActiveDialerInteraction != null)
            {
                mDialerData = new NameValueCollection();
                mDialerData.Clear();
                foreach (KeyValuePair<string, string> pair in this.ActiveDialerInteraction.ContactData)
                {
                    mDialerData.Add(pair.Key.ToString().Trim(), pair.Value);
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
            log.Info(scope + "Completed.");
        }

        private void Initialize_CallBack()
        {
            string scope = "CIC::MainForm::Initial_CallBack()::";
            log.Info(scope + "Starting.");
            try
            {
                    if (IcWorkFlow.LoginResult && this.ActiveDialerInteraction != null)
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
                
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                log.Error(scope + "Error info : " + ex.Message);
            }
        }

        private void InitializeStatusMessageDetails()
        {
            // TODO: clean up unused variables
            string scope = "CIC::FormMain::InitializeStatusMessageDetails()";
            int iIndex = 0;
            int AvailableIndex = 0;

            this.AllStatusMessageList = null;
            this.AllStatusMessageListOfUser = null;
            UserStatusUpdate statusUpdate = null;
            try
            {
               if (IcWorkFlow.LoginResult)
                {
                    if (this.mPeopleManager != null)
                    {
                        this.AllStatusMessageList = new StatusMessageList(this.mPeopleManager);
                        this.AllStatusMessageListOfUser = new UserStatusList(this.mPeopleManager);
                        this.AllStatusMessageListOfUser.WatchedObjectsChanged += 
                            new EventHandler<WatchedObjectsEventArgs<UserStatusProperty>>(AllStatusMessageListOfUser_WatchedObjectsChanged);
                        string[] dusers = { Program.DialingManager.Session.UserId };   //Make value to array 
                        this.AllStatusMessageListOfUser.StartWatching(dusers);
                        this.CurrentUserStatus = this.AllStatusMessageListOfUser.GetUserStatus(Program.DialingManager.Session.UserId);
                        this.AllStatusMessageList.StartWatching();
                        foreach (StatusMessageDetails status in this.AllStatusMessageList.GetList())
                        {
                            if (status.MessageText.ToLower().Trim() == "available")
                            {
                                AvailableStatusMessageDetails = status;
                                AvailableIndex = iIndex;
                            }
                            if (status.MessageText.ToLower().Trim() == "do not disturb")
                            {
                                DoNotDisturbStatusMessageDetails = status;
                            }
                            iIndex++;
                            log.Info(scope + "Id=" + status.Id + ", MessageText=" + status.MessageText);
                        }
                    }
               }
               else if (this.mPeopleManager != null)
                {
                    string[] nusers = { this.IC_Session.UserId };   //Make value to array 
                    this.AllStatusMessageList = new StatusMessageList(this.mPeopleManager);
                    this.AllStatusMessageListOfUser = new UserStatusList(this.mPeopleManager);
                    this.AllStatusMessageListOfUser.WatchedObjectsChanged += 
                        new EventHandler<WatchedObjectsEventArgs<UserStatusProperty>>(AllStatusMessageListOfUser_WatchedObjectsChanged);
                    this.AllStatusMessageListOfUser.StartWatching(nusers);
                    this.CurrentUserStatus = this.AllStatusMessageListOfUser.GetUserStatus(this.IC_Session.UserId);
                    this.AllStatusMessageList.StartWatching();
                    foreach (StatusMessageDetails status in this.AllStatusMessageList.GetList())
                    {
                        if (status.IsSelectableStatus)
                        {
                            if (status.MessageText.ToLower().Trim() == "available")
                            {
                                AvailableStatusMessageDetails = status;
                                AvailableIndex = iIndex;
                            }
                            if (status.MessageText.ToLower().Trim() == "do not disturb")
                            {
                                DoNotDisturbStatusMessageDetails = status;
                            }
                            iIndex++;
                        }
                        log.Info(scope + "Id=" + status.Id + ", MessageText=" + status.MessageText);
                    }
                }
                

                //Set Current User Status Display 
                if (this.mPeopleManager != null)
                {
                    statusUpdate = new UserStatusUpdate(this.mPeopleManager);
                    if (this.CurrentUserStatus != null)
                    {
                        if (global::CIC.Properties.Settings.Default.AutoResetUserStatus)
                        {
                            statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                            statusUpdate.UpdateRequest();
                        }
                        else
                        {
                            if (this.CurrentUserStatus.StatusMessageDetails.IsSelectableStatus)
                            {
                                statusUpdate.StatusMessageDetails = this.CurrentUserStatus.StatusMessageDetails;
                                statusUpdate.UpdateRequest();
                            }
                            else if (IcWorkFlow.LoginResult)
                            {
                                statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                                statusUpdate.UpdateRequest();
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                log.Error(scope + "Error info." + ex.Message);
            }
        }

        private void CallBackInteration_StartWatchingCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string scope = "CIC::MainForm::CallBackInteration_StartWatchingCompleted()::";
            log.Info(scope + "Starting.");
            if (e.Error != null)
            {
                log.Error(scope + "Error info : " + e.Error.Message);
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
                    log.Info(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    log.Error(scope + "Error info : " + ex.Message);
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

        private List<string> SetAttributeList()
        {
            string DialerNumber = this.GetDialerNumber();
            if (DialerNumber.Trim() != "")
            {
                this.frmScheduleCallbackForm = new CIC.ScheduleCallbackForm(DialerNumber);
            }
            else
            {
                this.frmScheduleCallbackForm = new CIC.ScheduleCallbackForm(this.ActiveDialerInteraction.RemoteId);
            }
            this.frmScheduleCallbackForm.ShowDialog();
            this.CallBackDateTime = this.frmScheduleCallbackForm.CallbackDateTime;
            this.CallBackPhone = this.frmScheduleCallbackForm.ScheduledNumber;
            switch (this.frmScheduleCallbackForm.CallbackType)
            {
                case CIC.Utils.CallbackType.CampaignWide:
                    this.ScheduleAgent = "";
                    break;
                case CIC.Utils.CallbackType.OwnAgent:
                    this.ScheduleAgent = this.IC_Session.UserId;
                    break;
                default:
                    this.ScheduleAgent = "";
                    break;
            }
            List<string> callbackAttributeNames = new List<string>();
            callbackAttributeNames.Add(CallbackInteractionAttributeName.RemoteName);
            callbackAttributeNames.Add(CallbackInteractionAttributeName.RemoteId);
            callbackAttributeNames.Add(this.CallBackPhone);
            callbackAttributeNames.Add(CallbackInteractionAttributeName.CallbackMessage);
            callbackAttributeNames.Add(CallbackInteractionAttributeName.CallbackCompletion);
            return callbackAttributeNames;
        }

        public void MakeManualCall(string number)
        {
            string scope = "CIC::MainForm::MakeManualCall()::";
            log.Info(scope + "CIC::FormMain::MakeManualCall(string)::");
            callingNumber = number;
            this.state_info_label.Text = "Next Calling Number: " + callingNumber;
            CallInteractionParameters callParams =
                new CallInteractionParameters(number, CallMadeStage.Allocated);
            SessionSettings sessionSetting = Program.m_Session.GetSessionSettings();
            callParams.AdditionalAttributes.Add("CallerHost", sessionSetting.MachineName.ToString());
            this.NormalInterationManager.MakeCallAsync(callParams, MakeCallCompleted, null);
        }

        public void MakeConsultCall(string transferTxtDestination)
        {
            string scope = "CIC::frmMain::MakeConsultCallToolStripButton_Click()::";
            log.Info(scope + "Starting.");
            ININ.IceLib.Interactions.CallInteractionParameters callParams = null;
            try
            {
                if (IcWorkFlow.LoginResult)
                {
                    //Log On to Dialer Server.  use same normal to call before using dialer object to blind/consult transfer.
                    log.Info(scope + "Call button clicked. Log On to Dialer Server.");
                    if (transferTxtDestination != "")
                    {
                        log.Info(scope + "Making consult call to " + transferTxtDestination);
                        callParams = new CallInteractionParameters(transferTxtDestination, CallMadeStage.Allocated);
                        if (NormalInterationManager != null)
                        {
                            callingNumber = transferTxtDestination;
                            NormalInterationManager.ConsultMakeCallAsync(callParams, MakeConsultCompleted, null);
                        }
                    }
                }
                //this.EnabledTransferToolStripDisplayed();
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                // TODO: Activate this code
                //this.ResetActiveCallInfo();
                log.Error(scope + "Error info." + ex.Message);
            }
        }

        public void DisconnectConsultCall()
        {
            string scope = "CIC::frmMain::CancelTransferToolStripButton_Click()::";
            log.Info(scope + "Starting.");
            try
            {
                if (IcWorkFlow.LoginResult)
                {
                    if (ActiveDialerInteraction != null && ActiveNormalInteraction != null)
                    {
                        if (InteractionList.Count > 0)
                        {
                            if (ActiveConsultInteraction != null)
                            {
                                ActiveConsultInteraction.Disconnect();
                                // TODO: activate this code
                                //this.RemoveNormalInteractionFromList(ActiveConsultInteraction);
                                ActiveConsultInteraction = null;
                            }
                            else
                            {
                                ActiveConsultInteraction = ActiveNormalInteraction;
                                ActiveConsultInteraction.Disconnect();
                                // TODO: activate this code
                                //this.RemoveNormalInteractionFromList(this.ActiveConsultInteraction);
                                ActiveConsultInteraction = null;
                            }
                            if (InteractionList != null)
                            {
                                foreach (ININ.IceLib.Interactions.Interaction CurrentInteraction in InteractionList)
                                {
                                    if (CurrentInteraction.IsHeld)
                                    {
                                        ActiveNormalInteraction = CurrentInteraction;
                                        ActiveNormalInteraction.Pickup();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (ActiveNormalInteraction != null)
                    {
                        if (InteractionList.Count > 0)
                        {
                            if (ActiveConsultInteraction != null)
                            {
                                ActiveConsultInteraction.Disconnect();
                                // TODO: activate this code
                                //this.RemoveNormalInteractionFromList(this.ActiveConsultInteraction);
                                ActiveConsultInteraction = null;
                            }
                            else
                            {
                                ActiveConsultInteraction = ActiveNormalInteraction;
                                ActiveConsultInteraction.Disconnect();
                                // TODO: activate this code
                                //this.RemoveNormalInteractionFromList(this.ActiveConsultInteraction);
                                ActiveConsultInteraction = null;
                            }
                            if (InteractionList != null)
                            {
                                foreach (ININ.IceLib.Interactions.Interaction CurrentInteraction in InteractionList)
                                {
                                    if (CurrentInteraction.IsHeld)
                                    {
                                        ActiveNormalInteraction = CurrentInteraction;
                                        ActiveNormalInteraction.Pickup();
                                        break;
                                    }
                                }
                            }
                        }

                    }
                }
                this.ShowActiveCallInfo();
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                // TODO: activate this code
                //this.EnabledTransferToolStripDisplayed();
                log.Error(scope + "Error info." + ex.Message);
            }
        }

        private void MakeConsultCompleted(object sender, InteractionCompletedEventArgs e)
        {
            state_info_label.Text = "Consulting:" + callingNumber;
            ActiveConsultInteraction = e.Interaction;
        }

        private string GetDialerNumber()
        {
            string DialerNumber = "";
            string AlternatePreview_ATTR = Properties.Settings.Default.AlternatePreviewNumbers;
            string[] AlternatePreviewNoATTRCollection;
            string scope = "CIC::frmMain::GetDialerNumber()::";
            log.Info(scope + "Starting.");

            if (mDialerData[Properties.Settings.Default.Preview_Number_ATTR].ToString().Trim() == String.Empty)
            {
                if (AlternatePreview_ATTR != String.Empty)
                {
                    AlternatePreviewNoATTRCollection = AlternatePreview_ATTR.Split(';');
                    foreach (string PreviewNoATTR in AlternatePreviewNoATTRCollection)
                    {
                        if (PreviewNoATTR.Trim() != String.Empty)
                        {
                            if (mDialerData[PreviewNoATTR.Trim()].Trim() != String.Empty)
                            {
                                DialerNumber = mDialerData[PreviewNoATTR.Trim()];
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                DialerNumber = mDialerData[Properties.Settings.Default.Preview_Number_ATTR].ToString().Trim();
            }
            log.Info(scope + "Completed.");
            return DialerNumber;
        }

        private void DialerInteraction_AttributesChanged(object sender, AttributesEventArgs e)
        {
            string scope = "CIC::MainForm::DialerInteraction_AttributesChanged():: ";
            log.Info(scope + "Starting.");
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
                log.Info(scope + "Completed.");
            }
            catch
            {
                //Get Connection State.
            }
        }

        private void BreakGranted(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::BreakGranted(): ";
            log.Info(scope + "Starting.");
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<EventArgs>(BreakGranted), new object[] { sender, e });
            }
            else
            {
                try
                {
                    switch (break_requested)
                    {
                        case true:
                            state_change(FormMainState.Break);
                            // TODO: add message that user is on break on dashboards
                            this.SetToDoNotDisturb_UserStatusMsg();
                            this.state_info_label.Text = "On Break.";
                            break;
                        default:
                            state_change(FormMainState.Break);
                            break;
                    }
                    
                    log.Info(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    log.Error(scope + "Error info." + ex.Message);
                }
            }
            //throw new NotImplementedException();
        }

        private void LogoutGranted(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::LogoutGranted(): ";
            //Tracing.TraceStatus(scope + "Starting.");
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<EventArgs>(LogoutGranted), new object[] { sender, e });
            }
            else
            {
                try
                {
                    switch (IcWorkFlow.LoginResult)
                    {
                        case true:
                            if (this.ActiveDialerInteraction != null)
                            {
                                this.ActiveDialerInteraction = null;
                            }
                            // TODO: need to clean up
                            IcWorkFlow = null;
                            this.DialerSession = null;
                            
                            //this.InitializeStatusMessageDetails();
                            this.SetToDoNotDisturb_UserStatusMsg();
                            state_change(FormMainState.Loggedout);
                            System.Windows.Forms.MessageBox.Show(
                                global::CIC.Properties.Settings.Default.CompletedWorkflowMsg,
                                "System Info.", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    // TODO: add more clean up state
                    if (ExitFlag)
                        this.Close();
                    //Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        private void SetToDoNotDisturb_UserStatusMsg()
        {
            string scope = "CIC::MainForm::SetToDoNotDisturb_UserStatusMsg(): ";
            ININ.IceLib.People.UserStatusUpdate statusUpdate = null;
            try
            {
                //Tracing.TraceStatus(scope + "Starting.");
                if (this.DoNotDisturbStatusMessageDetails != null)
                {
                    if (this.mPeopleManager != null)
                    {
                        statusUpdate = new UserStatusUpdate(this.mPeopleManager);
                        statusUpdate.StatusMessageDetails = this.DoNotDisturbStatusMessageDetails;
                        statusUpdate.UpdateRequest();
                    }
                }

                //Tracing.TraceStatus(scope + "completed.");
            }
            catch (System.Exception ex)
            {
                //Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void placecall(object sender, EventArgs e)
        {
            // Src: PlaceCallToolStripButton_Click()
            // string scope = "CIC::MainForm::PlaceCallToolStripButton_Click(): ";
            if (this.InvokeRequired)
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
                        Dictionary<string, string> data = this.ActiveDialerInteraction.ContactData;
                        if (data.ContainsKey("is_attr_numbertodial"))
                        {
                            state_info_label.Text = "Calling: " + data["is_attr_numbertodial"];
                            String phone1 = data["is_attr_PhoneNo1"];
                            // find a strategy to make a call by using PlacePreviewCall() and make a normal call for the other 6 numbers 
                            if (data["is_attr_numbertodial"].CompareTo(data["is_attr_PhoneNo1"]) == 0)
                            {
                                reset_color_panel();
                                name1_panel.BackColor = Color.Yellow;
                            }
                            else if (data["is_attr_numbertodial"].CompareTo(data["is_attr_PhoneNo2"]) == 0)
                            {
                                reset_color_panel();
                                name2_panel.BackColor = Color.Yellow;
                            }
                            else if (data["is_attr_numbertodial"].CompareTo(data["is_attr_PhoneNo3"]) == 0)
                            {
                                reset_color_panel();
                                name3_panel.BackColor = Color.Yellow;
                            }
                            else if (data["is_attr_numbertodial"].CompareTo(data["is_attr_PhoneNo4"]) == 0)
                            {
                                reset_color_panel();
                                name4_panel.BackColor = Color.Yellow;
                            }
                            else if (data["is_attr_numbertodial"].CompareTo(data["is_attr_PhoneNo5"]) == 0)
                            {
                                reset_color_panel();
                                name5_panel.BackColor = Color.Yellow;
                            }
                            else if (data["is_attr_numbertodial"].CompareTo(data["is_attr_PhoneNo6"]) == 0)
                            {
                                reset_color_panel();
                                name6_panel.BackColor = Color.Yellow;
                            }

                            this.callingNumber = data["is_attr_numbertodial"];
                            state_info_label.Text = "calling: " + this.callingNumber;
                            this.ActiveDialerInteraction.PlacePreviewCallAsync(MakePreviewCallComplete, null);
                            this.toolStripStatus.Text = (this.ActiveDialerInteraction != null) ?
                                this.ActiveDialerInteraction.State.ToString() + ":" +
                                this.ActiveDialerInteraction.StateDescription.ToString() : "N/A";
                            reset_timer();
                            
                        }
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
                switch (IcWorkFlow.LoginResult)
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
        
        private void reset_color_panel()
        {
            name1_panel.BackColor = SystemColors.Control;
            name2_panel.BackColor = SystemColors.Control;
            name3_panel.BackColor = SystemColors.Control;
            name4_panel.BackColor = SystemColors.Control;
            name5_panel.BackColor = SystemColors.Control;
            name6_panel.BackColor = SystemColors.Control;
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            DisposeQueueWatcher();
            DisposeSession();
            DisposeDialerSession();
            tryDisconnect();
            tryDisconnectAllInteractions();
            if (this.ActiveDialerInteraction != null)
            {
                try
                {
                    this.ActiveDialerInteraction.DialerSession.RequestLogout();
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void tryDisconnectAllInteractions()
        {
            if (this.InteractionList != null)
            {
                foreach (ININ.IceLib.Interactions.Interaction CurrentInteraction in this.InteractionList)
                {
                    if (CurrentInteraction != null)
                    {
                        try
                        {
                            CurrentInteraction.Disconnect();
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                this.InteractionList.Clear();
                this.InteractionList = null;
            }
        }

        private void DisposeSession()
        {
            string scope = "CIC::MainForm::Dispose_QueueWatcher():: ";
            //Tracing.TraceStatus(scope + "Starting.");
            try
            {
                //Tracing.TraceStatus(scope + "Creating instance of InteractionQueue");
                if (global::CIC.Program.m_Session != null)
                {
                    global::CIC.Program.m_Session.ConnectionStateChanged -= new EventHandler<ConnectionStateChangedEventArgs>(mSession_Changed);
                }
                //Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void dateTimeTimer_Tick(object sender, EventArgs e)
        {
            toolStripDatetime.Text = DateTime.Now.ToString("F");
        }

        private void workflow_button_EnabledChanged(object sender, EventArgs e)
        {
            if (workflow_button.Enabled)
            {
                workflow_button.BackgroundImage = CIC.Properties.Resources.Icon_LogInWF;
            }
            else
            {
                workflow_button.BackgroundImage = CIC.Properties.Resources.disable_Icon_LogInWF;
            }
        }

        private void call_button_EnabledChanged(object sender, EventArgs e)
        {
            if (call_button.Enabled)
            {
                call_button.BackgroundImage = CIC.Properties.Resources.Icon_Call;
            }
            else
            {
                call_button.BackgroundImage = CIC.Properties.Resources.disable_Icon_Call;
            }
        }

        private void disconnect_button_EnabledChanged(object sender, EventArgs e)
        {
            if (disconnect_button.Enabled)
            {
                disconnect_button.BackgroundImage = CIC.Properties.Resources.Icon_Disconnect;
            }
            else
            {
                disconnect_button.BackgroundImage = CIC.Properties.Resources.disable_Icon_Disconnect;
            }
        }

        private void hold_button_EnabledChanged(object sender, EventArgs e)
        {
            if (hold_button.Enabled)
            {
                hold_button.BackgroundImage = CIC.Properties.Resources.Icon_Hold;
            }
            else
            {
                hold_button.BackgroundImage = CIC.Properties.Resources.disable_Icon_Hold;
            }
        }

        private void mute_button_EnabledChanged(object sender, EventArgs e)
        {
            if (mute_button.Enabled)
            {
                mute_button.BackgroundImage = CIC.Properties.Resources.Icon_Mute;
            }
            else
            {
                mute_button.BackgroundImage = CIC.Properties.Resources.disable_Icon_Mute;
            }
        }

        private void transfer_button_EnabledChanged(object sender, EventArgs e)
        {
            if (transfer_button.Enabled)
            {
                transfer_button.BackgroundImage = CIC.Properties.Resources.Icon_Transfer;
            }
            else
            {
                transfer_button.BackgroundImage = CIC.Properties.Resources.disable_Icon_Transfer;
            }
        }

        private void conference_button_EnabledChanged(object sender, EventArgs e)
        {
            if (conference_button.Enabled)
            {
                conference_button.BackgroundImage = CIC.Properties.Resources.Icon_Conference;
            }
            else
            {
                conference_button.BackgroundImage = CIC.Properties.Resources.disable_Icon_Conference;
            }
        }

        private void manual_call_button_EnabledChanged(object sender, EventArgs e)
        {
            if (manual_call_button.Enabled)
            {
                manual_call_button.BackgroundImage = CIC.Properties.Resources.Icon_ManualCall;
            }
            else
            {
                manual_call_button.BackgroundImage = CIC.Properties.Resources.disable_Icon_ManualCall;
            }
        }

        private void break_button_EnabledChanged(object sender, EventArgs e)
        {
            if (break_button.Enabled)
            {
                break_button.BackgroundImage = CIC.Properties.Resources.Icon_Break;
            }
            else
            {
                break_button.BackgroundImage = CIC.Properties.Resources.disable_Icon_Break;
            }
        }

        private void endbreak_button_EnabledChanged(object sender, EventArgs e)
        {
            if (endbreak_button.Enabled)
            {
                endbreak_button.BackgroundImage = CIC.Properties.Resources.Icon_EndBreak;
            }
            else
            {
                endbreak_button.BackgroundImage = CIC.Properties.Resources.disable_Icon_EndBreak;
            }
        }

        private void logout_workflow_button_EnabledChanged(object sender, EventArgs e)
        {
            if (logout_workflow_button.Enabled)
            {
                logout_workflow_button.BackgroundImage = CIC.Properties.Resources.Icon_LogoutWF;
            }
            else
            {
                logout_workflow_button.BackgroundImage = CIC.Properties.Resources.disable_Icon_LogoutWF;
            }
        }

        private void exit_button_EnabledChanged(object sender, EventArgs e)
        {
            if (exit_button.Enabled)
            {
                exit_button.BackgroundImage = CIC.Properties.Resources.Icon_Exit;
            }
            else
            {
                exit_button.BackgroundImage = CIC.Properties.Resources.disable_Icon_Exit;
            }
        }
    }
}
