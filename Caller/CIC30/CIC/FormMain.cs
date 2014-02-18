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
        Connected,              // connect to session
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
        Disconnected,           // disconnect from session
        None,                   // nothing at all or error state
    };
    //private bool IsLoggedIntoDialer = false;

    public partial class FormMain : Form
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ScheduleCallbackForm frmScheduleCallbackForm { get; set; }


        private bool isConsulting { get; set; }
        private bool break_requested { get; set; }
        private bool break_granted { get; set; }
        private bool BlindTransferFlag { get; set; }
        private bool IsMuted { get; set; }
        private bool IsActiveConnection { get; set; }
        private bool SwapPartyFlag { get; set; }
        private bool ExitFlag { get; set; }
        private bool IsManualDialing { get; set; }
        private bool IsActiveConference_flag { get; set; }
        private bool isConnectedCall { get; set; }
        private int AutoReconnect = 2;
        private string[] InteractionAttributes { get; set; }
        private ArrayList InteractionList { get; set; }
        private string callingNumber { get; set; }
        private string ScheduleAgent { get; set; }
        private string CallBackPhone { get; set; }
        private string CallerHost { get; set; }
        private string cachedURI { get; set; }
        private string AlertSoundFileType { get; set; }
        private float timer = global::CIC.Properties.Settings.Default.CountdownTime;
        private float callingTime = global::CIC.Properties.Settings.Default.CountdownTime;


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
            isConsulting = false;
            isFirstTimeLogin = true;
            InitializeComponent();
            JulianCalendar cal = new JulianCalendar();
            // version
            this.Text = "Outbound Telephony Dialer Client v.1.0." + cal.GetDayOfYear(DateTime.Now) + "a";
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
                        log.Info(scope + "Creating instance of Session");
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
                        log.Info(scope + "Complete creating instance of Session");
                    }

                    ININ.IceLib.Connection.Session session = global::CIC.Program.m_Session;
                    log.Info(scope + "Creating instance of Dialing Manager");
                    Program.Initialize_dialingManager(session);
                    log.Info(scope + "Completed creating instance of Dialing Manager");
                
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
                                if (this.isFirstTimeLogin)
                                {
                                    this.login_workflow();
                                    this.isFirstTimeLogin = false;
                                }
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
                        log.Info(scope + "Register Event Attribute Change and Start watching attributes");
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
                            ActiveNormalInteraction = e.Interaction;
                            if (e.ConferenceItem.IsDisconnected)
                            {
                                this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                            }
                            else
                            {
                                //this.Set_ConferenceToolStrip();
                            }
                            break;
                        case InteractionType.Call:
                            ActiveNormalInteraction = e.Interaction;
                            if (e.ConferenceItem.IsDisconnected)
                            {
                                this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                            }
                            else
                            {
                                //this.Set_ConferenceToolStrip();
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

        private void Set_ConferenceToolStrip()
        {
            string scope = "CIC::MainForm::Set_ConferenceToolStrip():: ";
            log.Info(scope + "Starting.");
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
                        log.Info(scope + "Register Event Attribute Change and Start watching attributes");
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
                            ActiveNormalInteraction = e.Interaction;
                            if (e.ConferenceItem.IsDisconnected)
                            {
                                this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                            }
                            else
                            {
                                //this.Set_ConferenceToolStrip();
                            }
                            break;
                        case InteractionType.Call:
                            ActiveNormalInteraction = e.Interaction;
                            if (e.ConferenceItem.IsDisconnected)
                            {
                                this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                            }
                            else
                            {
                                //this.Set_ConferenceToolStrip();
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
                        log.Info(scope + "Register Event Attribute Change and start watching attrubute change");
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
                        case InteractionType.Call:
                            if (!e.Interaction.IsDisconnected)
                            {
                                log.Info(scope + "Create new conference interaction");
                                ActiveConferenceInteraction = new InteractionConference(
                                    this.NormalInterationManager, e.Interaction.InteractionType,
                                    e.ConferenceItem.ConferenceId);
                                ActiveNormalInteraction = e.Interaction;
                                this.disable_when_line_disconnect();
                                this.disable_hold_and_mute();
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
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<InteractionEventArgs>(m_InteractionQueue_InteractionRemoved), new object[] { sender, e });
            }
            else
            {
                try
                {
                    if (!e.Interaction.IsWatching())
                    {
                        log.Info(scope + "Register Event Attribute Change and start watching attributes");
                        e.Interaction.AttributesChanged += new EventHandler<AttributesEventArgs>(NormalInteraction_AttributesChanged);
                        e.Interaction.StartWatching(this.InteractionAttributes);
                    }
                    switch (e.Interaction.InteractionType)
                    {
                        case InteractionType.Email:
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
                                    //this.disable_when_line_disconnect();
                                    //this.disable_hold_and_mute();
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
                                //this.disable_when_line_disconnect();
                                //this.disable_hold_and_mute();
                                //this.reset_info_on_dashboard();
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
                            log.Warn(scope + "Disconnecting Active Normal Interaction");
                            ActiveNormalInteraction.Disconnect();
                            log.Warn(scope + "Finish Disconnecting Active Normal Interaction");
                        }
                        catch
                        {
                            //Emty catch block
                        }
                        this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                    }
                    ActiveNormalInteraction = null;
                }
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
                }
                else
                {
                    if (this.BlindTransferFlag)
                    {
                        if (this.InteractionList.Count > 0)
                        {
                            this.InteractionList.Clear();
                        }
                        //this.update_state_info_label("Connected to: " + this.GetDialerNumber());
                        update_break_status_label("");
                        this.SetInfoBarColor();
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
            this.name6_box1.Text = "";
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

            this.update_break_status_label("");
            this.toolStripStatus.Text = "N/A";
            timer_info.Text = "Time until call: N/A";
            this.toolStripCallIDLabel.Text = "N/A";
            this.toolStripDirectionLabel.Text = "N/A";
            this.toolStripCallTypeLabel.Text = "N/A";
            this.toolStripCampaignIDLabel.Text = "N/A";
        }

        private void SetInfoBarColor()
        {
            string scope = "CIC::frmMain::SetInfoBarColor()::";
            log.Info(scope + "Starting.");
            if (IcWorkFlow != null && IcWorkFlow.LoginResult)
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
                };
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
            if (IcWorkFlow != null && IcWorkFlow.LoginResult)
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
                log.Info(scope + "Starting CIC MP3 player creation");
                this.cicMp3Player = new Locus.Control.MP3Player();
                log.Info(scope + "Complete CIC MP3 player creation");
                log.Info(scope + "Start CIC MP3 player play()");
                this.cicMp3Player.Play(sPathWavPath, false);
                log.Info(scope + "Complete CIC MP3 player play()");
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
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<InteractionAttributesEventArgs>(m_InteractionQueue_InteractionChanged), new object[] { sender, e });
            }
            else
            {
                try
                {
                    if (!e.Interaction.IsWatching())
                    {
                        log.Info(scope + "Register Event Attribute Change and start watching attributes");
                        e.Interaction.AttributesChanged += new EventHandler<AttributesEventArgs>(NormalInteraction_AttributesChanged);
                        e.Interaction.StartWatching(this.InteractionAttributes);
                    }
                    switch (e.Interaction.InteractionType)
                    {
                        case InteractionType.Email:
                            //
                            break;
                        case InteractionType.Chat:
                            break;
                        case InteractionType.Callback:
                        case InteractionType.Call:
                            ActiveNormalInteraction = e.Interaction;
                            if (ActiveNormalInteraction.State == InteractionState.Connected && !ActiveNormalInteraction.IsMuted)
                            {
                                this.BeginInvoke(new MethodInvoker(enable_when_repickup));
                                if (this.StrConnectionState == InteractionState.Proceeding)
                                {
                                    if (IcWorkFlow != null && IcWorkFlow.LoginResult &&
                                        !isConsulting)
                                    {
                                        if (!this.IsManualDialing)
                                        {
                                            this.BeginInvoke(new MethodInvoker(CrmScreenPop));
                                        }
                                        this.BeginInvoke(new MethodInvoker(reset_call_timer));
                                    }
                                }
                                this.state_change(FormMainState.PreviewCall);
                            }
                            this.StrConnectionState = ActiveNormalInteraction.State;
                            toolStripStatus.Text = this.StrConnectionState.ToString();
                            if (this.StrConnectionState == InteractionState.InternalDisconnect ||
                                this.StrConnectionState == InteractionState.ExternalDisconnect)
                            {
                                if (!this.isCurrentConsultCall())
                                {
                                    this.BeginInvoke(new MethodInvoker(disable_when_line_disconnect));
                                    this.BeginInvoke(new MethodInvoker(disable_hold_and_mute));
                                    this.update_state_info_label("Disconnected.");
                                }
                            }
                            else if (this.StrConnectionState == InteractionState.Connected)
                            {
                                if (this.isConsulting)
                                {
                                    this.update_state_info_label("Consulting.");
                                }
                                else
                                {
                                    if (!ActiveNormalInteraction.IsMuted)
                                    {
                                        this.update_state_info_label("Connected to: " + this.GetDialerNumber());
                                        update_break_status_label("");
                                    }
                                }
                            }
                            if (this.BlindTransferFlag)
                            {
                                this.ResetActiveCallInfo();
                            }
                            else
                            {
                                this.ShowActiveCallInfo();
                            }
                            if (ActiveNormalInteraction != null)
                            {
                                if (ActiveNormalInteraction.IsDisconnected)
                                {
                                    this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                                    ActiveNormalInteraction = this.GetAvailableInteractionFromList();
                                    //this.BeginInvoke(new MethodInvoker(disable_when_line_disconnect));
                                    //this.BeginInvoke(new MethodInvoker(disable_hold_and_mute));
                                }
                            }
                            break;
                        default:
                            ActiveNormalInteraction = e.Interaction;
                            this.StrConnectionState = ActiveNormalInteraction.State;
                            toolStripStatus.Text = this.StrConnectionState.ToString();
                            if (ActiveNormalInteraction != null)
                            {
                                if (ActiveNormalInteraction.IsDisconnected)
                                {
                                    this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                                    ActiveNormalInteraction = this.GetAvailableInteractionFromList();
                                    //this.BeginInvoke(new MethodInvoker(disable_when_line_disconnect));
                                    //this.BeginInvoke(new MethodInvoker(disable_hold_and_mute));
                                }
                            }
                            if (this.StrConnectionState == InteractionState.InternalDisconnect ||
                                this.StrConnectionState == InteractionState.ExternalDisconnect)
                            {
                                this.BeginInvoke(new MethodInvoker(disable_when_line_disconnect));
                                this.BeginInvoke(new MethodInvoker(disable_hold_and_mute));
                            }
                            if (this.BlindTransferFlag)
                            {
                                this.StrConnectionState = InteractionState.None;
                            }
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
                    }
                }
            }
        }

        private bool isCurrentConsultCall()
        {
            if (ActiveConsultInteraction ==  null)
                return false;
            return (ActiveNormalInteraction.InteractionId == ActiveConsultInteraction.InteractionId);
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
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<InteractionAttributesEventArgs>(m_InteractionQueue_InteractionAdded), new object[] { sender, e });
            }
            else
            {
                try
                {
                    if (!e.Interaction.IsWatching())
                    {
                        log.Info(scope + "Register Event Attribute Change and start watching attributes");
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
                            log.Info(scope + "Starting Email Interaction Creation");
                            ActiveNormalEmailInteraction =
                                new EmailInteraction(this.NormalInterationManager, e.Interaction.InteractionId);
                            log.Info(scope + "Completed Email Interaction Creation");
                            //this.SetActiveEmailQueue();
                            ActiveNormalInteraction = e.Interaction;
                            break;
                        case InteractionType.Chat:
                            //
                            break;
                        case InteractionType.Callback:
                            log.Info(scope + "Starting Callback Interaction Creation");
                            this.ActiveCallbackInteraction =
                                new CallbackInteraction(this.NormalInterationManager, e.Interaction.InteractionId);
                            log.Info(scope + "Completed Callback Interaction Creation");
                            ActiveNormalInteraction = e.Interaction;
                            this.StrConnectionState = ActiveNormalInteraction.State;
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
                                        log.Info(scope + "Starting GetSessionSettings()");
                                        ININ.IceLib.Connection.SessionSettings session_Setting =
                                            ActiveNormalInteraction.InteractionsManager.Session.GetSessionSettings();
                                        log.Info(scope + "Completed GetSessionSettings()");
                                        this.CallerHost = session_Setting.MachineName.ToString();
                                    }
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
                    log.Info(scope + "Starting Security Creation");
                    ININ.IceLib.Connection.Extensions.Security SecurityObject =
                        new ININ.IceLib.Connection.Extensions.Security(global::CIC.Program.m_Session);
                    log.Info(scope + "Completed Security Creation");
                    log.Info(scope + "Starting Password Policy Creation");
                    ININ.IceLib.Connection.Extensions.PasswordPolicy passwordPolicyObject = SecurityObject.GetPasswordPolicy();
                    log.Info(scope + "Completed Password Policy Creation");
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
            changePasswordObject.ShowDialog();
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
                    if (IcWorkFlow != null && !IcWorkFlow.LoginResult)
                    {
                        this.BeginInvoke(new MethodInvoker(login_workflow));
                    }
                    try
                    {
                        this.SetActiveSession(Program.m_Session);
                    }
                    catch (Exception ex)
                    {
                        log.Error(scope + "Error info." + ex.Message);
                    }
                    this.Initial_NormalInteraction();
                    this.InitializeQueueWatcher();

                    if (this.isFirstTimeLogin)
                    {
                        this.login_workflow();
                        this.isFirstTimeLogin = false;
                    }
                    this.BeginInvoke(new MethodInvoker(connected_state));
                    log.Info(scope + "Completed.");
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
                        log.Info(scope + "Starting Session Creation");
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
                        log.Info(scope + "Completed Session Creation");
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
            string scope = "CIC::MainForm::Dispose_QueueWatcher()::";
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
            this.update_state_info_label("Next calling number: " + this.GetDialerNumber());
        }
        
        private void restart_timer()
        {
            reset_timer();
            previewCallTimer.Start();
        }

        private void reset_call_timer()
        {
            if (!callingTimer.Enabled)
                callingTimer.Enabled = true;
            callingTimer.Stop();
            callingTime = global::CIC.Properties.Settings.Default.CallingWaitTime;
            log.Info("Calling time is reset");
        }

        private void restart_call_timer()
        {
            reset_call_timer();
            callingTimer.Start();
        }

        public void login_workflow()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(login_workflow));
            }
            else
            {
                this.workflow_button_Click(null, EventArgs.Empty);
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            previewCallTimer.Enabled = false;
            toolStripUsernameLabel.Text = global::CIC.Properties.Settings.Default.UserId;
        }

        private void workflow_button_Click(object sender, EventArgs e)
        {
            if (this.IsActiveConnection)
            {
                frmWorkflow workflow = frmWorkflow.getInstance(global::CIC.Program.m_Session); //new CIC.frmWorkflow(global::CIC.Program.m_Session);
                workflow.ShowDialog();
            }
            else
            {
                state_change(FormMainState.Disconnected);
            }
        }

        private void call_button_Click(object sender, EventArgs e)
        {   
            // make a call or pickup
            placecall(this, null);
            state_change(FormMainState.Calling);
        }

        private void disconnect_button_Click(object sender, EventArgs e)
        {
            tryDisconnect();
            tryDisconnectAllInteractions();
            if (!this.IsManualDialing)
            {
                frmDisposition disposition = frmDisposition.getInstance(
                    this.IC_Session, this.GetDialerNumber(), this.toolStripCallTypeLabel.Text); //new frmDisposition();
                disposition.updateDialerNumber(this.GetDialerNumber());
                disposition.updateCallerList(this.GetDialerListNumber());
                disposition.ShowDialog();
            }
            hold_button.Text = "";
            mute_button.Text = "";
            //this.ShowActiveCallInfo();
            this.update_state_info_label("Disconnected.");
        }

        private void tryDisconnect()
        {
            string scope = "CIC::MainForm::tryDisconnect()::";
            log.Info(scope + "Starting");
            if (!this.IsManualDialing &&
                this.IC_Session != null &&
                this.IC_Session.ConnectionState == ININ.IceLib.Connection.ConnectionState.Up)
            {
                log.Info(scope + "try disconnecting logged in workflow interactions");

                if (ActiveDialerInteraction != null)
                {
                    if (ActiveConsultInteraction != null && !ActiveConsultInteraction.IsDisconnected)
                    {
                        try
                        {
                            log.Info(scope + "Starting Consult Interaction Disconnect");
                            this.RemoveNormalInteractionFromList(ActiveConsultInteraction);
                            ActiveConsultInteraction.Disconnect();
                            log.Info(scope + "Completed Consult Interaction Disconnect");
                        }
                        catch (Exception ex)
                        {
                            log.Error(scope + "Error info." + ex.Message);
                        }
                    }
                    if (ActiveNormalInteraction != null && !ActiveNormalInteraction.IsDisconnected)
                    {
                        try
                        {
                            log.Info(scope + "Starting Normal Interaction Disconnect");
                            this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                            ActiveNormalInteraction.Disconnect();
                            log.Info(scope + "Completed Normal Interaction Disconnect");
                        }
                        catch (Exception ex)
                        {
                            log.Error(scope + "Error info." + ex.Message);
                        }
                    }
                    if (!ActiveDialerInteraction.IsDisconnected)
                    {
                        try
                        {
                            log.Info(scope + "Starting Dialer Interaction Disconnect");
                            ActiveDialerInteraction.Disconnect();
                            log.Info(scope + "Completed Dialer Interaction Disconnect");
                        }
                        catch (Exception ex)
                        {
                            log.Error(scope + "Error info." + ex.Message);
                        }
                    }
                    this.state_change(FormMainState.Predictive);
                }
            }
            else
            { // Not Log On to Dialer Server.
                log.Info(scope + "try disconnecting logged out workflow interactions");
                if (this.ActiveDialerInteraction != null && !this.ActiveDialerInteraction.IsDisconnected)
                {
                    try
                    { 
                        log.Info(scope + "Starting Dialer Interaction Disconnect");
                        this.ActiveDialerInteraction.Disconnect();
                        this.ActiveDialerInteraction = null;
                        log.Info(scope + "Completed Dialer interaction Disconnect");
                    }
                    catch (Exception ex)
                    {
                        log.Error(scope + "Error info." + ex.Message);
                    }
                }
                if (ActiveNormalInteraction != null && !ActiveNormalInteraction.IsDisconnected)
                {
                    try
                    { 
                        log.Info(scope + "Statring Normal Interaction Disconnect");
                        this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                        ActiveNormalInteraction.Disconnect();
                        log.Info(scope + "Complete Normal Interaction Disconnect");
                        ActiveNormalInteraction = this.GetNormalInteractionFromList();
                    }
                    catch (Exception ex)
                    {
                        log.Error(scope + "Error info." + ex.Message);
                    }
                }
                else
                {
                    ActiveNormalInteraction = this.GetNormalInteractionFromList();
                    if (ActiveNormalInteraction != null)
                    {
                        try
                        { 
                            log.Info(scope + "Starting Normal Interaction Disconnect");
                            ActiveNormalInteraction.Disconnect();
                            log.Info(scope + "Completed Normal Interaction Disconnect");
                        }
                        catch (Exception ex)
                        {
                            log.Error(scope + "Error info." + ex.Message);
                        }
                    }
                }
                if (ActiveConsultInteraction != null && !ActiveConsultInteraction.IsDisconnected)
                {
                    try
                    { 
                        log.Info(scope + "Starting Consult Interaction Disconnect");
                        ActiveConsultInteraction.Disconnect();
                        ActiveConsultInteraction = null;
                        log.Info(scope + "Completed Consult Interaction Disconnect");
                    }
                    catch (Exception ex)
                    {
                        log.Error(scope + "Error info." + ex.Message);
                    }
                }

                if (this.InteractionList != null && this.InteractionList.Count <= 0)
                {
                    ActiveConferenceInteraction = null;
                    ActiveConsultInteraction = null;
                }
                this.state_change(FormMainState.Connected);
            }
            isConsulting = false;
            this.IsActiveConference_flag = false;
            this.reset_info_on_dashboard();
            log.Info(scope + "Completed.");
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
            string scope = "CIC::frmMain::hold_button_Click()::";
            if (!this.IsManualDialing)
            {
                if (this.ActiveDialerInteraction != null)
                {
                    if (!this.ActiveDialerInteraction.IsDisconnected)
                    {
                        if (this.ActiveDialerInteraction.IsMuted)
                        {
                            log.Info(scope + "Starting Dialer Interaction Unmute");
                            this.ActiveDialerInteraction.Mute(false);
                            log.Info(scope + "Completed Dialer Interaction Unmute");
                        }
                        log.Info(scope + "Starting Dialer Interaction Hold/Unhold");
                        this.ActiveDialerInteraction.Hold(!this.ActiveDialerInteraction.IsHeld);
                        log.Info(scope + "Completed Dialer Interaction Hold/Unhold");
                        state_change(FormMainState.Hold);
                        return;
                    }
                }
                if (ActiveNormalInteraction != null)
                {
                    if (ActiveNormalInteraction.IsMuted)
                    {
                        log.Info(scope + "Starting Normal Interaction Unmute");
                        ActiveNormalInteraction.Mute(false);
                        log.Info(scope + "Completed Normal interaction Unmute");
                    }
                    log.Info(scope + "Starting Normal Interaction Hold/Unhold");
                    ActiveNormalInteraction.Hold(!ActiveNormalInteraction.IsHeld);
                    log.Info(scope + "Completed Normal Interaction Hold/Unhold");
                    state_change(FormMainState.Hold);
                    return;
                }
            }
            else
            {
                if (ActiveNormalInteraction != null)
                {
                    if (ActiveNormalInteraction.IsMuted)
                    {
                        log.Info(scope + "Starting Normal Interaction Unmute");
                        ActiveNormalInteraction.Mute(false);
                        log.Info(scope + "Completed Normal interaction Unmute");
                    }
                    log.Info(scope + "Starting Normal Interaction Hold/Unhold");
                    ActiveNormalInteraction.Hold(!ActiveNormalInteraction.IsHeld);
                    log.Info(scope + "Completed Normal Interaction Hold/Unhold");
                    state_change(FormMainState.Hold);
                    return;
                }
            }
        }

        private void mute_button_Click(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::mute_button_Click()::";
            if (!this.IsManualDialing)
            {
                if (this.ActiveDialerInteraction != null)
                {
                    if (this.ActiveDialerInteraction.IsHeld)
                    {
                        log.Info(scope + "Starting Dialer Interaction Unhold");
                        this.ActiveDialerInteraction.Hold(false);
                        log.Info(scope + "Complete Dialer Interaction Unhold");
                    }
                    log.Info(scope + "Statring Dialer Interaction Mute/Unmute");
                    this.ActiveDialerInteraction.Mute(!this.ActiveDialerInteraction.IsMuted);
                    log.Info(scope + "Completed Dialer Interaction Mute/Unmute");
                    state_change(FormMainState.Mute);
                    return;
                }
                if (ActiveNormalInteraction != null)
                {
                    if (ActiveNormalInteraction.IsHeld)
                    {
                        log.Info(scope + "Starting Normal Interaction Unhold");
                        ActiveNormalInteraction.Hold(false);
                        log.Info(scope + "Completed Normal Interaction Unhold");
                    }
                    log.Info(scope + "Starting Normal Interaction Mute/Unmute");
                    ActiveNormalInteraction.Mute(!ActiveNormalInteraction.IsMuted);
                    log.Info(scope + "Completed Normal Interaction Mute/Unmute");
                    state_change(FormMainState.Mute);
                    return;
                }
            }
            else
            {
                if (ActiveNormalInteraction != null)
                {
                    if (ActiveNormalInteraction.IsHeld)
                    {
                        log.Info(scope + "Starting Normal Interaction Unhold");
                        ActiveNormalInteraction.Hold(false);
                        log.Info(scope + "Completed Normal Interaction Unhold");
                    }
                    log.Info(scope + "Starting Normal Interaction Mute/Unmute");
                    ActiveNormalInteraction.Mute(!ActiveNormalInteraction.IsMuted);
                    log.Info(scope + "Completed Normal Interaction Mute/Unmute");
                    state_change(FormMainState.Mute);
                    return;
                }
            }
        }

        private void transfer_button_Click(object sender, EventArgs e)
        {
            frmTransfer transfer = frmTransfer.getInstance();
            transfer.ShowDialog();
        }

        private void conference_button_Click(object sender, EventArgs e)
        {
            frmConference conference = frmConference.getInstance();
            conference.ShowDialog();
        }

        private void manual_call_button_Click(object sender, EventArgs e)
        {
            if (IcWorkFlow == null || !IcWorkFlow.LoginResult || this.isOnBreak)
            {
                frmManualCall manualCall = frmManualCall.getInstance();
                manualCall.ShowDialog();
            }
            else
            {
                this.manual_call_button.Enabled = false;
            }
        }

        public void request_break()
        {
            string scope = "CIC::FormMain::request_break()::";
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(request_break));
            }
            else
            {
                log.Info(scope + "Starting");
                this.break_requested = true;
                log.Info(scope + "Starting Dialer Interaction Request Break");
                this.ActiveDialerInteraction.DialerSession.RequestBreak();
                log.Info(scope + "Completed Dialer Interaction Request Break");
                log.Info(scope + "Completed");
            }
        }

        private void break_button_Click(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::break_button_Click()::";
            log.Info(scope + "Starting.");
            try
            {
                if (IcWorkFlow != null && !IcWorkFlow.LoginResult && this.ActiveDialerInteraction == null)
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
                        break_requested = true;
                        break_requested_state();
                    }
                    break_button.Enabled = false;
                }
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                log.Error(scope + "Error info." + ex.Message);
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
                    break_granted = false;
                    this.isOnBreak = false;
                    this.endbreak_button.Enabled = false;
                }
                else
                {
                    if (this.current_state == FormMainState.Break)
                    {
                        log.Info(scope + "Starting Dialer Interaction EndBreak");
                        this.ActiveDialerInteraction.DialerSession.EndBreak();
                        log.Info(scope + "Completed Dialer Interaction EndBreak");
                        try
                        {
                            log.Info(scope + "Starting Update User Status");
                            ININ.IceLib.People.UserStatusUpdate statusUpdate = new UserStatusUpdate(this.mPeopleManager);
                            statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                            statusUpdate.UpdateRequest();
                            log.Info(scope + "Complete Update User Status");
                        }
                        catch (Exception ex)
                        {
                            log.Error(scope + "Error info." + ex.Message);
                        }
                        break_requested = false;
                        break_granted = false;
                        this.isOnBreak = false;
                        this.endbreak_button.Enabled = false;
                        this.manual_call_button.Enabled = false;
                        this.update_break_status_label("Break ended. Waiting for a new call from workflow.");
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
                if (IcWorkFlow == null)
                {
                    state_change(FormMainState.Connected);
                    return;
                }
                if (IcWorkFlow.LoginResult)
                {
                    // TODO: check the state of calling from the commented one
                    //if (this.CallStateToolStripStatusLabel.Text.ToLower().Trim() == "n/a")
                    if (this.current_state == FormMainState.Disconnected)
                    {
                        IcWorkFlow.DialerSession.RequestLogoutAsync(LogoutGranted, null);
                    }
                    else
                    {
                        if (this.ActiveDialerInteraction != null)
                        {
                            if (!this.break_requested)
                            {
                                this.break_requested = true;
                                this.request_break();               //wait for breakgrant
                                log.Info(scope + "Starting Dialer Interaction Request Logout");
                                this.ActiveDialerInteraction.DialerSession.RequestLogoutAsync(LogoutGranted, null);
                                log.Info(scope + "Completed Dialer Interaction Request Logout");
                            }
                            else
                            {
                                this.ActiveDialerInteraction.DialerSession.RequestLogoutAsync(LogoutGranted, null);
                            }
                        }
                        else
                        {
                            IcWorkFlow.DialerSession.RequestLogoutAsync(LogoutGranted, null);
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
            string scope = "CIC::MainForm::disconnect_IC_session(): ";
            if (this.IC_Session != null)
            {
                log.Info(scope + "Starting IC_Session Disconnect");
                this.IC_Session.Disconnect();
                this.IC_Session = null;
                log.Info(scope + "Completed IC_Session Disconnect");
            }
        }

        private static void disconnect_normal_interaction()
        {
            string scope = "CIC::MainForm::disconnect_normal_interaction(): ";
            if (ActiveNormalInteraction != null)
            {
                log.Info(scope + "Starting Normal Interaction Disconnect");
                ActiveNormalInteraction.Disconnect();
                log.Info(scope + "Completed Normal Interaction Disconnect");
                ActiveNormalInteraction = null;
            }
        }
        
        public void disposition_invoke(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::DispositionToolStripButton_Click(): ";
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
                            if (this.break_requested)
                            {
                                this.request_break();
                            }
                            ININ.IceLib.People.UserStatusUpdate statusUpdate = new UserStatusUpdate(this.mPeopleManager);
                            if (this.ActiveDialerInteraction.DialingMode == DialingMode.Preview ||
                                this.ActiveDialerInteraction.DialingMode == DialingMode.Regular ||
                                this.ActiveDialerInteraction.DialingMode == DialingMode.OwnAgentCallback ||
                                this.ActiveDialerInteraction.DialingMode == DialingMode.OwnAgentCallback_Preview)
                            {
                                if (this.ActiveDialerInteraction.IsDisconnected == false)
                                {
                                    log.Info(scope + "Starting Dialer Interaction Disconnect");
                                    this.ActiveDialerInteraction.Disconnect();
                                    log.Info(scope + "Completed Dialer Interaction Disconnect");
                                }

                                callParameter callparam = (callParameter)sender;
                                if (callparam.number != null && callparam.number != "")
                                {
                                    ActiveDialerInteraction.ContactData["is_attr_schedphone"] = callparam.number;
                                    ActiveDialerInteraction.UpdateCallData();
                                }
                                
                                try
                                {
                                    log.Info(scope + "Starting Dialer Interaction CallComplete");
                                    this.ActiveDialerInteraction.CallCompleteAsync(callparam.param, completedCallback, null);
                                    log.Info(scope + "Completed Dialer Interaction CallComplete");
                                }
                                catch (Exception ex)
                                {
                                    log.Error(scope + "Call Complete Failed: " + ex.Message);
                                }
                                if (!this.break_granted)
                                {
                                    if (this.AvailableStatusMessageDetails != null)
                                    {
                                        log.Info(scope + "Starting Update User Status.");
                                        try
                                        {
                                            statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                                            statusUpdate.UpdateRequest();
                                            log.Info(scope + "Completed Update User Status.");
                                        }
                                        catch (Exception ex)
                                        {
                                            log.Error(scope + "Could not update User Status. Error info." + ex.Message);
                                        }
                                    }
                                }
                            }
                            else if (this.ActiveDialerInteraction.DialingMode == DialingMode.Precise)
                            {
                                //
                            }
                            else if (this.ActiveDialerInteraction.DialingMode == DialingMode.Agentless)
                            {
                                //
                            }
                            else
                            {
                                //Other Mode!
                            }
                        }
                    }

                    if (this.break_granted || this.IsManualDialing)
                    {
                        break_requested = false;
                        state_change(FormMainState.Break);
                    }
                    else
                    {
                        state_change(FormMainState.Predictive);
                    }
                    log.Info(scope + "Completed.[Disposition]");
                }
                catch (ININ.IceLib.IceLibException ex)
                {
                    log.Error(scope + "Error info." + ex.Message);
                }
            }
        }

        private void completedCallback(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                log.Warn("CompletedCall is incompleted" + e.Error.StackTrace + " :: " + e.Error.Message);
                return;
            }
        }

        public void workflow_invoke(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<EventArgs>(workflow_invoke), new object[] { sender, e });
            }
            else
            {
                string scope = "CIC::MainForm::WorkflowToolStripMenuItem_Click()::";
                log.Info(scope + "Starting.");
                try
                {
                    log.Info(scope + "Logging into workflow. UserId=" + this.IC_Session.UserId + ", StationId=" + this.IC_Session.GetStationInfo().Id);
                    if (IcWorkFlow != null && IcWorkFlow.LoginResult)
                    {
                        this.state_change(FormMainState.Predictive);
                    }
                    else
                    {
                        IcWorkFlow = new CIC.ICWorkFlow(CIC.Program.DialingManager);
                        this.DialerSession = IcWorkFlow.LogIn(((String)sender));
                        this.toolStripWorkflowLabel.Text = (string)sender;
                        //IcWorkFlow.LoginResult = IcWorkFlow.LoginResult;
                        if (IcWorkFlow.LoginResult)
                        {
                            this.InitializeDialerSession();
                            this.SetActiveSession(Program.m_Session);
                            this.Initial_NormalInteraction();
                            this.InitializeQueueWatcher();
                            this.UpdateUserStatus();
                            this.state_change(FormMainState.Predictive);
                            this.SetToAvailable_UserStatusMsg();
                            this.endbreak_button_Click(sender, e);
                            this.update_state_info_label("Logged into Workflow");
                            log.Info(scope + "Completed.");
                        }
                        else
                        {
                            log.Warn(scope + "WorkFlow [" + ((string)sender) + "] logon Fail. Please try again.");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    log.Error(scope + "Error info.Logon to Workflow[" + ((string)sender) + "] : " + ex.Message);
                }
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
                //if (IcWorkFlow != null && IcWorkFlow.LoginResult && !this.IsManualDialing)
                if (!this.IsManualDialing)
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
                                if (interact.InteractionType == InteractionType.Call ||
                                    interact.InteractionType == InteractionType.Callback)
                                {
                                    if (!interact.IsDisconnected)
                                    {
                                        TmpInteraction[idx] = interact;
                                        idx++;
                                    }
                                }
                            }
                            try
                            {
                                log.Info(scope + "Starting Normal Interaction Make New Conference");
                                this.NormalInterationManager.MakeNewConferenceAsync(TmpInteraction, MakeNewConferenceCompleted, null);
                                log.Info(scope + "Complete Normal Interaction Make New Conference");
                            }
                            catch (Exception ex)
                            {
                                // TODO: change state
                                log.Error(scope + "Error info." + ex.Message);
                            }
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
                                if (interact.InteractionType == InteractionType.Call ||
                                    interact.InteractionType == InteractionType.Callback)
                                {
                                    if (!interact.IsDisconnected)
                                    {
                                        TmpInteraction[idx] = interact;
                                        idx++;
                                    }
                                }
                            }
                            try
                            {
                                log.Info(scope + "Starting Normal Interaction Make New Conference");
                                this.NormalInterationManager.MakeNewConferenceAsync(TmpInteraction, MakeNewConferenceCompleted, null);
                                log.Info(scope + "Complete Normal Interaction Make New Conference");
                            }
                            catch (Exception ex)
                            {
                                // TODO: change state
                                log.Error(scope + "Error info." + ex.Message);
                            }
                        }
                    }
                }
                this.isConsulting = false;
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
                this.update_state_info_label("Conferencing");
                state_change(FormMainState.ConferenceCall);
                this.disable_when_line_disconnect();
                this.disable_hold_and_mute();
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {

                log.Error(scope + "Error info." + ex.Message);
            }
        }

        public void transfer_invoke(string transferTxtDestination)
        {
            string scope = "CIC::frmMain::transfer_invoke()::";
            log.Info(scope + "Starting.");
            this.BlindTransferFlag = false;
            try
            {
                //if (IcWorkFlow != null && IcWorkFlow.LoginResult)
                if (!this.IsManualDialing)
                {
                    if (this.ActiveDialerInteraction != null)
                    {
                        if (ActiveConsultInteraction != null)
                        {
                            //Tracing.TraceNote(scope + "Performing consult transfer");
                            log.Info(scope + "Starting Dialer Interaction Consult Transfer");
                            ActiveDialerInteraction.ConsultTransferAsync(ActiveConsultInteraction.InteractionId, WorkflowTransferCompleted, null);
                            log.Info(scope + "Completed Dialer Interaction Consult Transfer");
                            this.RemoveNormalInteractionFromList(ActiveConsultInteraction);

                            // complete workflow
                            string sFinishcode = global::CIC.Properties.Settings.Default.ReasonCode_Transfereded;
                            ININ.IceLib.Dialer.ReasonCode sReasoncode = ININ.IceLib.Dialer.ReasonCode.Transferred;
                            CallCompletionParameters callCompletionParameters = new CallCompletionParameters(sReasoncode, sFinishcode);
                            log.Info(scope + "Starting Dialer Interaction CallComplete");
                            this.ActiveDialerInteraction.CallComplete(callCompletionParameters);
                            log.Info(scope + "Completed Dialer Interaction CallComplete");

                            try
                            {
                                // update user status
                                log.Info(scope + "Starting Update User Status");
                                ININ.IceLib.People.UserStatusUpdate statusUpdate = new UserStatusUpdate(this.mPeopleManager);
                                statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                                statusUpdate.UpdateRequest();
                                log.Info(scope + "Complete Update User Status");
                            }
                            catch (Exception ex)
                            {
                                log.Info(scope + "Error info." + ex.Message);
                            }
                            if (this.break_requested)
                            {
                                this.request_break();
                            }
                        }
                    }
                }
                else
                {
                    if (ActiveNormalInteraction != null)
                    {
                        if (ActiveConsultInteraction != null)
                        {
                            log.Info(scope + "Performing consult transfer");
                            if (ActiveConsultInteraction.InteractionId != ActiveNormalInteraction.InteractionId)
                            {
                                log.Info(scope + "Starting Normal Interaction Consult Transfer");
                                ActiveNormalInteraction.ConsultTransferAsync(ActiveConsultInteraction.InteractionId, ManualTransferCompleted, null);
                                this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                                this.RemoveNormalInteractionFromList(ActiveConsultInteraction);
                                this.BlindTransferFlag = true;
                                log.Info(scope + "Completed Normal Interaction Consult Transfer");
                            }
                            else
                            {
                                ActiveNormalInteraction = ActiveDialerInteraction;
                                if (this.InteractionList != null && this.InteractionList.Count > 1)
                                {
                                    foreach (ININ.IceLib.Interactions.Interaction CurrentInteraction in this.InteractionList)
                                    {
                                        if (CurrentInteraction.InteractionType == InteractionType.Call &&
                                            CurrentInteraction.InteractionId != ActiveConsultInteraction.InteractionId)
                                        {
                                            ActiveNormalInteraction = CurrentInteraction;  //Find Consult Call
                                            break;
                                        }
                                    }

                                    if (ActiveNormalInteraction != null)
                                    {
                                        log.Info(scope + "Starting Consult Interaction Consult Transfer");
                                        ActiveConsultInteraction.ConsultTransferAsync(ActiveNormalInteraction.InteractionId, ManualTransferCompleted, null);
                                        this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                                        this.RemoveNormalInteractionFromList(ActiveConsultInteraction);
                                        this.BlindTransferFlag = true;
                                        log.Info(scope + "Completed Consult Interaction Consult Transfer");
                                    }
                                }
                            }
                        }
                        else
                        {
                            log.Info(scope + "Performing blind transfer");
                            if (transferTxtDestination != "")
                            {
                                log.Info(scope + "Starting Normal Interaction Blind Transfer");
                                ActiveNormalInteraction.BlindTransfer(transferTxtDestination);
                                this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                                log.Info(scope + "Complete Normal Interaction Blind Transfer");
                            }
                        }
                    }
                }
                this.isConsulting = false;
                this.IsManualDialing = false;
                this.reset_info_on_dashboard();
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                this.ResetActiveCallInfo();
                log.Error(scope + "Error info." + ex.Message);
            }
        }

        private void WorkflowTransferCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string scope = "CIC::frmMain::WorkflowTransferCompleted()::";
            log.Info(scope + "Starting.");
            this.BlindTransferFlag = true;
            this.isConsulting = false;
            this.reset_info_on_dashboard();
            this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
            state_change(FormMainState.Predictive);
            this.update_state_info_label("Transfer complete.");
            log.Info(scope + "Completed");
        }

        private void ManualTransferCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string scope = "CIC::frmMain::ManualTransferCompleted()::";
            log.Info(scope + "Starting.");
            this.BlindTransferFlag = true;
            this.reset_info_on_dashboard();
            this.BlindTransferFlag = false;
            this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
            state_change(FormMainState.Connected);
            this.update_state_info_label("Transfer complete.");
            log.Info(scope + "Completed");
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
                if (IcWorkFlow != null)
                {
                    switch (IcWorkFlow.LoginResult)
                    {
                        case true: //Log On to Workflow
                            if (this.mPeopleManager != null)
                            {
                                log.Info(scope + "Starting User Status Initialization");
                                this.AllStatusMessageList = new StatusMessageList(this.mPeopleManager);
                                this.AllStatusMessageListOfUser = new UserStatusList(this.mPeopleManager);
                                log.Info(scope + "Completed User Status Initialization");
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
                                        DoNotDisturbStatusMessageDetails = status;
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
                                log.Info(scope + "Starting User Status Initialization");
                                this.AllStatusMessageList = new StatusMessageList(this.mPeopleManager);
                                this.AllStatusMessageListOfUser = new UserStatusList(this.mPeopleManager);
                                log.Info(scope + "Completed User Status Initialization");
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
                                            DoNotDisturbStatusMessageDetails = status;
                                        }
                                        iIndex++;
                                    }
                                    log.Info(scope + "Id=" + status.Id + ", MessageText=" + status.MessageText);
                                }
                            }
                            break;
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
                            log.Info(scope + "Starting Update User Status");
                            statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                            statusUpdate.UpdateRequest();
                            log.Info(scope + "Completed Update User Status");
                        }
                        else
                        {
                            if (this.CurrentUserStatus.StatusMessageDetails.IsSelectableStatus)
                            {
                                log.Info(scope + "Starting Update User Status");
                                statusUpdate.StatusMessageDetails = this.CurrentUserStatus.StatusMessageDetails;
                                statusUpdate.UpdateRequest();
                                log.Info(scope + "Completed Update User Status");
                            }
                            else
                            {
                                if (IcWorkFlow.LoginResult)
                                {
                                    log.Info(scope + "Starting Update User Status");
                                    statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                                    statusUpdate.UpdateRequest();
                                    log.Info(scope + "Complete Update User Status");
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
                try
                {
                    if (IcWorkFlow != null && IcWorkFlow.LoginResult)
                    {
                        if (this.ActiveDialerInteraction == null)
                        {
                            this.toolStripCallIDLabel.Text = "N/A";
                            this.toolStripDirectionLabel.Text = "N/A";
                            this.toolStripCallTypeLabel.Text = "N/A";
                            this.toolStripCampaignIDLabel.Text = "N/A";
                            ActiveConferenceInteraction = null;
                        }
                        else
                        {
                            if (this.ActiveDialerInteraction.DialingMode == DialingMode.Regular ||
                                this.ActiveDialerInteraction.DialingMode == DialingMode.OwnAgentCallback)
                            {
                                if (!this.isConnectedCall)
                                {
                                    if (global::CIC.Properties.Settings.Default.AutoAnswer)
                                    {
                                        this.pickup();
                                        this.isConnectedCall = true;
                                        this.IsManualDialing = false;
                                        this.isConsulting = false;
                                    }
                                    else
                                    {
                                        this.call_button.Enabled = true;
                                    }
                                }
                            }
                            this.toolStripCallIDLabel.Text = ActiveDialerInteraction.ContactData.ContainsKey("is_attr_callid") ?
                                ActiveDialerInteraction.ContactData["is_attr_callid"] : "";
                            this.toolStripDirectionLabel.Text = ActiveDialerInteraction.Direction.ToString();
                            this.toolStripCallTypeLabel.Text = "Campaign Call(" + ActiveDialerInteraction.DialingMode.ToString() + ")";
                            try
                            {
                                this.toolStripCampaignIDLabel.Text = mDialerData[Properties.Settings.Default.Preview_Campaign_ATTR];
                            }
                            catch (Exception ex)
                            {
                                log.Warn(scope + "Error info. " + ex.Message);
                            }
                        }
                    }
                    else
                    {
                        if (ActiveNormalInteraction == null)
                        {
                            this.toolStripCallIDLabel.Text = "N/A";
                            this.toolStripDirectionLabel.Text = "N/A";
                            this.toolStripCallTypeLabel.Text = "N/A";
                            this.toolStripCampaignIDLabel.Text = "N/A";
                            ActiveConferenceInteraction = null;
                        }
                        else
                        {
                            if (this.BlindTransferFlag)
                            {
                                this.toolStripCallIDLabel.Text = "N/A";
                                this.toolStripDirectionLabel.Text = "N/A";
                                this.toolStripCallTypeLabel.Text = "N/A";
                                this.toolStripCampaignIDLabel.Text = "N/A";
                                ActiveConferenceInteraction = null;
                            }
                            else
                            {
                                if (ActiveNormalInteraction == null)
                                    return;
                                switch (ActiveNormalInteraction.State)
                                {
                                    case InteractionState.None:
                                        break;
                                    case InteractionState.Held:
                                        if (this.SwapPartyFlag)
                                        {
                                            this.SetActiveCallInfo();
                                            this.ShowActiveCallInfo();
                                        }
                                        else
                                        {
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
                                            {
                                                log.Warn(scope + "Error info. " + ex.Message);
                                            }
                                        }
                                        break;
                                    case InteractionState.Connected:
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
                                        {
                                            log.Warn(scope + "Error info. " + ex.Message);
                                        }
                                        break;
                                    default:
                                        this.toolStripCallIDLabel.Text = "N/A";
                                        this.toolStripDirectionLabel.Text = ActiveNormalInteraction.Direction.ToString();
                                        this.toolStripCallTypeLabel.Text = ActiveNormalInteraction.InteractionType.ToString();
                                        try
                                        {
                                            this.toolStripCampaignIDLabel.Text = "Non-campaign Call";
                                        }
                                        catch (Exception ex)
                                        {
                                            log.Warn(scope + "Error info. " + ex.Message);
                                        }
                                        if (ActiveNormalInteraction != null)
                                            this.toolStripStatus.Text = ActiveNormalInteraction.State.ToString();
                                        break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(scope + "Error info." + ex.Message);
                }
                this.SetInfoBarColor();
                update_conference_status();
            }
            log.Info(scope + "Completed.");
        }

        private void update_info_on_dashboard()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(update_info_on_dashboard));
            }
            else
            {
                string scope = "CIC::frmMain::update_info_on_dashboard()::";
                log.Info(scope + "Starting.");
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
                this.last_date_payment_box.Text = data.ContainsKey("is_attr_LastReceiveDatePayment") ? getDateTimeString(
                        data["is_attr_LastReceiveDatePayment"], (string)global::CIC.Properties.Settings.Default["ServerDateTimeFormat"]
                    ) : "";
                this.debt_status_box.Text = data.ContainsKey("is_attr_DebtStatus") ? data["is_attr_DebtStatus"] : "";
                this.start_overdue_date_box.Text = data.ContainsKey("is_attr_StartOverDueDate") ? getDateTimeString(
                        data["is_attr_StartOverDueDate"], (string)global::CIC.Properties.Settings.Default["ServerDateTimeFormat"]
                    ) : "";
                this.followup_status_box.Text = data.ContainsKey("is_attr_FollowupStatus") ? data["is_attr_FollowupStatus"] : "";
                this.payment_appoint_box.Text = data.ContainsKey("is_attr_PaymentAppoint") ? getDateTimeString(
                        data["is_attr_PaymentAppoint"], (string)global::CIC.Properties.Settings.Default["ServerDateTimeFormat"]
                    ) : "";
                this.date_callback_box.Text = data.ContainsKey("is_attr_DateAppointCallBack") ? getDateTimeString(
                        data["is_attr_DateAppointCallBack"], oldFormat: (string)global::CIC.Properties.Settings.Default["ServerDateTimeFormat"],
                        destFormat: "dd/MM/yyyy HH:mm"
                    ) : "";
                this.callingNumber = data.ContainsKey("is_attr_numbertodial") ? data["is_attr_numbertodial"] : "";

                update_currency_on_dashboard(data);

                this.toolStripStatus.Text = this.ActiveDialerInteraction.StateDescription;
                if (data.ContainsKey("is_attr_STATUS") && data["is_attr_STATUS"].ToLower() == "complete")
                {
                    this.break_button.Enabled = true;
                    this.update_state_info_label("Workflow Completed.");
                    this.request_break();
                }
                log.Info(scope + "Completed.");
            }
        }

        private void update_currency_on_dashboard(Dictionary<string, string> data)
        {
            string scope = "CIC::frmMain::update_currency_on_dashboard()::";
            log.Info(scope + "Starting.");
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
                log.Error("the data in last_amount_payment_box cannot be parse to currency format: " + ex.Message);
            }

            try
            {
                this.initial_amount_box.Text = decimal.Parse(initialAmount).ToString("C2", CultureInfo.CreateSpecificCulture("th"));
            }
            catch (Exception ex)
            {
                this.initial_amount_box.Text = initialAmount;
                log.Error("the data in initial_amount_box cannot be parse to currency format: " + ex.Message);
            }

            try
            {
                this.monthly_payment_box.Text = decimal.Parse(monthlyPayment).ToString("C2", CultureInfo.CreateSpecificCulture("th"));
            }
            catch (Exception ex)
            {
                this.monthly_payment_box.Text = monthlyPayment;
                log.Error("the data in monthly_payment_box cannot be parse to currency format: " + ex.Message);
            }

            try
            {
                this.base_debt_box.Text = decimal.Parse(baseDebt).ToString("C2", CultureInfo.CreateSpecificCulture("th"));
            }
            catch (Exception ex)
            {
                this.base_debt_box.Text = baseDebt;
                log.Error("the data in base_debt_box cannot be parse to currency format: " + ex.Message);
            }
            log.Info(scope + "Completed.");
        }

        private void update_conference_status()
        {
            string scope = "CIC::FormMain::update_conference_status()::";
            log.Info(scope + "Started.");

            if (this.InteractionList != null && this.InteractionList.Count <= 0)
            {
                    ActiveConsultInteraction = null;
                    this.IsActiveConference_flag = false;
                    ActiveConferenceInteraction = null;
                    if (ActiveNormalInteraction != null)
                    {
                        try
                        {
                            log.Info(scope + "Starting Normal Interaction Disconnect");
                            ActiveNormalInteraction.Disconnect();
                            log.Info(scope + "Completed Normal Interaction Disconnect");
                        }
                        catch (Exception ex)
                        {
                            log.Error(scope + "Error info." + ex.Message);
                        }
                        ActiveNormalInteraction = null;
                    }
                    if (IcWorkFlow != null && !IcWorkFlow.LoginResult)
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
            string scope = "CIC::MainForm::exit_button_Click()::";
            log.Info(scope + "Starting.");
            this.ExitFlag = true;
            log.Info(scope + "Completing.");
            this.Close();
        }

        private void state_change(FormMainState state)
        {
            string scope = "CIC::MainForm::state_change()::";
            log.Info(scope + "Starting.");
            // TODO: implement all states
            switch (state)
            {
                case FormMainState.Predictive:
                    if (this.isOnBreak)
                    {
                        break_state();
                        log.Info("State Changed: Break");
                    }
                    else
                    { 
                        predictive_state();
                        log.Info("State Changed: Predictive");
                    }
                    break;
                case FormMainState.Preview:
                    if (this.isOnBreak)
                    {
                        break_state();
                        log.Info("State Changed: Break");
                    }
                    else
                    {
                        preview_state();
                        log.Info("State Changed: Preview");
                    }
                    break;
                case FormMainState.Connected:
                    if (this.isOnBreak)
                    {
                        break_state();
                        log.Info("State Changed: Break");
                    }
                    else
                    {
                        connected_state();
                        log.Info("State Changed: Connected");
                    }
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
                    conference_call_state();
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
                        case FormMainState.Calling:
                        case FormMainState.PreviewCall:
                        case FormMainState.ConferenceCall:
                        case FormMainState.ManualCall:
                            hold_button.Text = "Unhold";
                            hold_state();
                            log.Info("State Changed: Hold");
                            break;
                        // case Mute state -> change to hold state.
                        case FormMainState.Mute:
                            hold_button.Text = "Unhold";
                            hold_state();
                            log.Info("State Changed: Hold");
                            break;
                        // case Hold state -> change to calling state
                        case FormMainState.Hold:
                            hold_button.Text = "";
                            state_change(FormMainState.PreviewCall);
                            this.BeginInvoke(new MethodInvoker(enable_when_repickup));
                            log.Info("State Changed: Unhold -> Preview Call");
                            break;
                    }
                    break;
                case FormMainState.Mute:
                    switch (current_state)
                    {
                        // case calling state -> change to hold state
                        case FormMainState.Calling:
                        case FormMainState.PreviewCall:
                        case FormMainState.ConferenceCall:
                        case FormMainState.ManualCall:
                            mute_button.Text = "Unmute";
                            mute_state();
                            log.Info("State Changed: Mute");
                            break;
                        // case Mute state -> change to hold state.
                        case FormMainState.Hold:
                            mute_button.Text = "Unmute";
                            mute_state();
                            log.Info("State Changed: Mute");
                            break;
                        // case Mute state -> change to calling state
                        case FormMainState.Mute:
                            state = FormMainState.PreviewCall;
                            mute_button.Text = "";
                            state_change(FormMainState.PreviewCall);
                            this.BeginInvoke(new MethodInvoker(enable_when_repickup));
                            log.Info("State Changed: Unmute -> Preview Call");
                            break;
                    }
                    break;
                case FormMainState.Disconnected:
                    disconnect_state();
                    log.Info("State Changed: Disconnected");
                    break;
                case FormMainState.Break:
                    break_state();
                    log.Info("State Changed: Break");
                        
                    break;
                case FormMainState.Loggedout:
                    logged_out_state();
                    log.Info("State Changed: Logged Out");
                    break;
            }
            log.Info(scope + "Completed");
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
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(connected_state));
            }
            else
            {
                string scope = "CIC::FormMain::connected_state()::";
                log.Debug(scope + "Starting");
                reset_state();
                workflow_button.Enabled = true;
                exit_button.Enabled = true;

                this.update_state_info_label("Connected to the server.");

                prev_state = current_state;
                current_state = FormMainState.Connected;
                log.Debug(scope + "Completed");
            }
        }

        private void preview_state()
        {
            string scope = "CIC::FormMain::preview_state()::";
            log.Debug(scope + "Starting");
            reset_state();
            call_button.Enabled = true;
            break_button.Enabled = !break_requested && !IsManualDialing;
            update_state_info_label("Connected to: " + this.GetDialerNumber());
            update_break_status_label("");
            prev_state = current_state;
            current_state = FormMainState.Preview;

            log.Debug(scope + "Completed");
        }

        private void predictive_state()
        {
            string scope = "CIC::FormMain::predictive_state()::";
            log.Debug(scope + "Starting");
            reset_state();
            break_button.Enabled = !break_requested && !IsManualDialing;
            logout_workflow_button.Enabled = true;
            prev_state = current_state;
            current_state = FormMainState.Predictive;
            log.Debug(scope + "Completed");
        }

        private void calling_state()
        {
            string scope = "CIC::FormMain::calling_state()::";
            log.Debug(scope + "Starting");
            reset_state();
            disconnect_button.Enabled = true;

            prev_state = current_state;
            current_state = FormMainState.Calling;
            this.update_state_info_label("Calling: " + callingNumber);
            log.Debug(scope + "Completed");
        }

        private void conference_call_state()
        {
            string scope = "CIC::FormMain::preview_call_state()::";
            log.Debug(scope + "Starting");
            if (this.current_state == FormMainState.PreviewCall)
                return;

            reset_state();
            disconnect_button.Enabled = true;

            break_button.Enabled = !break_requested && !IsManualDialing;

            prev_state = current_state;
            current_state = FormMainState.ConferenceCall;
            log.Debug(scope + "Completed");
        }

        private void preview_call_state()
        {
            string scope = "CIC::FormMain::preview_call_state()::";
            log.Debug(scope + "Starting");
            if (this.current_state == FormMainState.PreviewCall)
                return;

            reset_state();
            disconnect_button.Enabled = true;
            hold_button.Enabled = true;
            mute_button.Enabled = true;

            if (!this.IsActiveConference_flag)
            {
                transfer_button.Enabled = true;
                conference_button.Enabled = true;
            }
            break_button.Enabled = !break_requested && !IsManualDialing;

            prev_state = current_state;
            current_state = FormMainState.PreviewCall;
            log.Debug(scope + "Completed");
        }

        private void hold_state()
        {
            string scope = "CIC::FormMain::hold_state()::";
            log.Debug(scope + "Starting");
            reset_state();
            disconnect_button.Enabled = true;
            hold_button.Enabled = true;
            mute_button.Enabled = false;
            break_button.Enabled = !break_requested && !IsManualDialing;

            if (current_state != FormMainState.Mute)
            {
                prev_state = current_state;
            }
            current_state = FormMainState.Hold;
            this.update_state_info_label("Connected to: " + this.GetDialerNumber() + " (Held)");
            log.Debug(scope + "Completed");
        }

        private void disconnect_state()
        {
            string scope = "CIC::FormMain::disconnect_state()::";
            log.Debug(scope + "Starting");
            reset_state();
            exit_button.Enabled = true;

            reset_timer();
            prev_state = current_state;
            current_state = FormMainState.Disconnected;
            log.Debug(scope + "Completed");
        }

        private void mute_state()
        {
            string scope = "CIC::FormMain::mute_state()::";
            log.Debug(scope + "Starting");
            reset_state();
            disconnect_button.Enabled = true;
            hold_button.Enabled = false;
            mute_button.Enabled = true;
            break_button.Enabled = !break_requested && !IsManualDialing;

            if (current_state != FormMainState.Hold)
            {
                prev_state = current_state;
            }
            current_state = FormMainState.Mute;
            this.update_state_info_label("Connected to: " + this.GetDialerNumber() + " (Muted)");
            log.Debug(scope + "Completed");
        }

        private void break_state()
        {
            string scope = "CIC::FormMain::break_state()::";
            log.Debug(scope + "Starting");
            reset_state();
            manual_call_button.Enabled = true;
            endbreak_button.Enabled = true;
            logout_workflow_button.Enabled = true;

            prev_state = current_state;
            current_state = FormMainState.Break;
            this.isOnBreak = true;
            log.Debug(scope + "Completed");
        }
        
        private void break_requested_state()
        {
            break_button.Enabled = !break_requested && IcWorkFlow != null && IcWorkFlow.LoginResult;
        }

        private void logged_out_state()
        {
            string scope = "CIC::FormMain::logged_out_state()::";
            log.Debug(scope + "Starting");
            reset_state();
            workflow_button.Enabled = true;
            exit_button.Enabled = true;

            prev_state = current_state;
            current_state = FormMainState.Loggedout;
            this.update_state_info_label("Logged out");
            log.Debug(scope + "Completed");
        }

        private void disable_break_request()
        {
            string scope = "CIC::FormMain::disable_break_request()::";
            log.Debug(scope + "Starting");
            break_button.Enabled = false;
            log.Debug(scope + "Completed");
        }
            
        private void disable_logout()
        {
            string scope = "CIC::FormMain::disable_logout()::";
            log.Debug(scope + "Starting");
            logout_workflow_button.Enabled = false;
            log.Debug(scope + "Completed");
        }

        private void disable_when_line_disconnect()
        {
            string scope = "CIC::FormMain::disable_when_line_disconnect()::";
            log.Debug(scope + "Starting");
            conference_button.Enabled = false;
            transfer_button.Enabled = false;
            log.Debug(scope + "Completed");
        }

        private void disable_hold_and_mute()
        {
            string scope = "CIC::FormMain::disable_when_line_disconnect()::";
            log.Debug(scope + "Starting");
            hold_button.Enabled = false;
            mute_button.Enabled = false;
            log.Debug(scope + "Completed");

        }

        private void enable_when_repickup()
        {
            string scope = "CIC::FormMain::enable_when_repickup()::";
            log.Debug(scope + "Starting");
            mute_button.Enabled = true;
            hold_button.Enabled = true;
            conference_button.Enabled = true;
            transfer_button.Enabled = true;
            log.Debug(scope + "Completed");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer -= (float)previewCallTimer.Interval / 1000;
            timer_info.Text = "Time until call: " + timer.ToString("F1");
            if (timer <= 0)
            {
                reset_timer();
                update_state_info_label("Calling: " + this.GetDialerNumber());
                
                // make a call or pickup
                placecall(sender, e);
            }
        }

        private void callingTimer_Tick(object sender, EventArgs e)
        {
        }

        public void MakePreviewCallComplete(object sender, AsyncCompletedEventArgs e)
        {
            //state_info_label.Text = "Connected to: " + this.ActiveDialerInteraction.ContactData["is_attr_numbertodial"];
            state_change(FormMainState.Calling);
        }

        public void MakeManualCallCompleted(object sender,InteractionCompletedEventArgs e)
        {
            string scope = "CIC::MainForm::MakeCallCompleted()::";
            log.Info(scope + "Starting");
            if (!e.Cancelled)
            {
                ActiveNormalInteraction = e.Interaction;
            }

            this.reset_info_on_dashboard();
            state_change(FormMainState.Calling);
            log.Info(scope + "Completed");
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
            this.disable_break_request();
            log.Info(scope + "Completed.");
        }

        private void CampaignTransition(object sender, CampaignTransistionEventArgs e)
        {
            // NYI
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
                log.Warn("date/time cannot be parsed: get(" + datetime + ") expected in " + oldFormat + "format: "+ex.Message);
            }
            return "";
        }

        private void DataPop(object sender, DataPopEventArgs e)
        {
            string scope = "CIC::MainForm::DataPop()::";
            log.Info(scope + "Starting.");
            try
            {
                if (!e.Interaction.IsWatching())
                {
                    log.Info(scope + "Register Event Attribute Change and start watching attributes");
                    e.Interaction.AttributesChanged += new EventHandler<AttributesEventArgs>(DialerInteraction_AttributesChanged);
                    e.Interaction.StartWatching(this.InteractionAttributes);
                }
                this.ActiveDialerInteraction = e.Interaction;
                update_info_on_dashboard();
                switch (e.Interaction.InteractionType)
                {
                    case InteractionType.Email:
                        break;
                    case InteractionType.Chat:
                        break;
                    case InteractionType.Callback:
                        this.Initialize_CallBack();
                        this.Initialize_ContactData();
                        if (this.current_state != FormMainState.Calling)
                            this.update_state_info_label("Acquired call from workflow.");
                        update_break_status_label("");
                        this.ShowActiveCallInfo();
                        break;
                    case InteractionType.Call:
                        this.Initialize_ContactData();

                        if (this.current_state != FormMainState.Calling)
                            this.update_state_info_label("Acquired call from workflow.");
                        update_break_status_label("");
                        this.ShowActiveCallInfo();
                        break;
                }
                this.highlight_call();
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
                    log.Info(scope + "Register Event Attributes Change and start watching attributes");
                    e.Interaction.AttributesChanged += new EventHandler<AttributesEventArgs>(DialerInteraction_AttributesChanged);
                    e.Interaction.StartWatching(this.InteractionAttributes);
                }
                this.ActiveDialerInteraction = e.Interaction;
                update_info_on_dashboard();
                switch (e.Interaction.InteractionType)
                {
                    case InteractionType.Email:
                        break;
                    case InteractionType.Chat:
                        break;
                    case InteractionType.Callback:
                        this.Initialize_CallBack();
                        this.Initialize_ContactData();

                        // restart timer and reset call index
                        this.BeginInvoke(new MethodInvoker(preview_state));
                        this.BeginInvoke(new MethodInvoker(restart_timer));
                        this.update_state_info_label("Acquired information from workflow.");
                        update_break_status_label("");
                        break;
                    case InteractionType.Call:
                        this.Initialize_ContactData();

                        // restart timer and reset call index
                        // TODO: need to check whether it is predictive or preview
                        this.BeginInvoke(new MethodInvoker(preview_state));
                        this.BeginInvoke(new MethodInvoker(restart_timer));
                        this.update_state_info_label("Acquired information from workflow.");
                        update_break_status_label("");
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
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(CrmScreenPop));
            }
            else
            {
                try
                {
                    // http://[MS-CRM Server name]/CRM/main.aspx?
                    // etn=col_collection_history&pagetype=entityrecord&extraqs=col_contract_no={Parameter1}&col_phone_id={Parameter2}&col_call_id={Parameter3}
                    // Parameter1 = ProductRefID (ฟิลด์ที่ 1 ใน spec) is_attr_ProductRefID
                    // Parameter2 = Ref_PhoneNo1 - Ref_PhoneNo6  (ฟิลด์ที่ 32-37 ใน spec อยู่ที่ขณะนั้นโทรติดที่ 
                    // PhoneNoอะไร) is_attr_Ref_PhoneNo1
                    // Parameter3 = Call_id (Id ของการโทรออก) is_attr_callid
                    Dictionary<string, string> data = ActiveDialerInteraction.ContactData;
                    string productID = data.ContainsKey("is_attr_ProductRefID") ? data["is_attr_ProductRefID"] : "";
                    string refCallID = getRefCallID(data);
                    string callID = data.ContainsKey("is_attr_callid") ? data["is_attr_callid"] : "";

                    string baseURI = "http://" + global::CIC.Properties.Settings.Default["MSCRMServerName"];
                    baseURI += "etn=col_collection_history&";
                    baseURI += "pagetype=entityrecord&extraqs=col_contract_no";
                    baseURI += string.Format("=%7b{0}%7d&col_phone_id=%7b{1}%7d&col_call_id={2}"
                        , productID, refCallID, callID).Replace("=", "%3d").Replace("{", "%7b").Replace("}", "%7d").Replace("&", "%26");
                    if (cachedURI != baseURI)
                    {
                        log.Info("process.start : " + baseURI);
                        cachedURI = baseURI;
                        Process.Start(baseURI);
                    }
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
            string number = data.ContainsKey("is_attr_numbertodial") ? data["is_attr_numbertodial"] : "";
            if (number == "")
                return refCallID;

            if (data.ContainsKey("is_attr_PhoneNo1") && data["is_attr_PhoneNo1"] == number)
                refCallID = data.ContainsKey("is_attr_Ref_PhoneNo1") ? data["is_attr_Ref_PhoneNo1"] : "";
            if (data.ContainsKey("is_attr_PhoneNo2") && data["is_attr_PhoneNo2"] == number)
                refCallID = data.ContainsKey("is_attr_Ref_PhoneNo2") ? data["is_attr_Ref_PhoneNo2"] : "";
            if (data.ContainsKey("is_attr_PhoneNo3") && data["is_attr_PhoneNo3"] == number)
                refCallID = data.ContainsKey("is_attr_Ref_PhoneNo3") ? data["is_attr_Ref_PhoneNo3"] : "";
            if (data.ContainsKey("is_attr_PhoneNo4") && data["is_attr_PhoneNo4"] == number)
                refCallID = data.ContainsKey("is_attr_Ref_PhoneNo4") ? data["is_attr_Ref_PhoneNo4"] : "";
            if (data.ContainsKey("is_attr_PhoneNo5") && data["is_attr_PhoneNo5"] == number)
                refCallID = data.ContainsKey("is_attr_Ref_PhoneNo5") ? data["is_attr_Ref_PhoneNo5"] : "";
            if (data.ContainsKey("is_attr_PhoneNo6") && data["is_attr_PhoneNo6"] == number)
                refCallID = data.ContainsKey("is_attr_Ref_PhoneNo6") ? data["is_attr_Ref_PhoneNo6"] : "";

            return refCallID;
        }

        private void Initialize_ContactData()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(Initialize_ContactData));
            }
            else
            {
                string scope = "CIC::MainForm::InitialContactData()::";
                log.Info(scope + "Starting.");
                this.isConnectedCall = false;
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
                            log.Info(scope + "Starting Update User Status");
                            statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                            statusUpdate.UpdateRequest();
                            log.Info(scope + "Completed Update User Status");
                        }
                        else
                        {
                            if (this.CurrentUserStatus.StatusMessageDetails.IsSelectableStatus)
                            {
                                log.Info(scope + "Starting Update User Status");
                                statusUpdate.StatusMessageDetails = this.CurrentUserStatus.StatusMessageDetails;
                                statusUpdate.UpdateRequest();
                                log.Info(scope + "Completed Update User Status");
                            }
                            else if (IcWorkFlow.LoginResult)
                            {
                                log.Info(scope + "Starting Update User Status");
                                statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                                statusUpdate.UpdateRequest();
                                log.Info(scope + "Completed Update User Status");
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
                        log.Info(scope + "Starting Active Workgroup Start Watching");
                        this.ActiveWorkgroupDetails.StartWatchingAsync(mWorkgroupDetailsAttributeNames, WorkgroupDetailsStartWatchingComplete, null);
                        log.Info(scope + "Completed Active Workgroup Start Watching");
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

        public void MakeManualCall(string number)
        {
            string scope = "CIC::MainForm::MakeManualCall()::";
            log.Info(scope + "starting");
            try
            {
                callingNumber = number;
                this.update_state_info_label("Manual Calling Number: " + callingNumber);
                CallInteractionParameters callParams =
                    new CallInteractionParameters(number, CallMadeStage.Allocated);
                SessionSettings sessionSetting = Program.m_Session.GetSessionSettings();
                callParams.AdditionalAttributes.Add("CallerHost", sessionSetting.MachineName.ToString());
                log.Info(scope + "Start Normal Interaction Make Call");
                this.NormalInterationManager.MakeCallAsync(callParams, MakeManualCallCompleted, null);
                log.Info(scope + "Completed Normal Interaction Make Call");
                this.IsManualDialing = true;
                this.reset_info_on_dashboard();
                state_change(FormMainState.ManualCall);
            }
            catch (Exception ex)
            {
                log.Warn(scope + " error(reason: " + ex + ")");
            }
            log.Info(scope + "complete");
        }

        public void MakeConsultCall(string transferTxtDestination)
        {
            string scope = "CIC::frmMain::MakeConsultCallToolStripButton_Click()::";
            log.Info(scope + "Starting.");
            ININ.IceLib.Interactions.CallInteractionParameters callParams = null;
            try
            {
                //Log On to Dialer Server.  use same normal to call before using dialer object to blind/consult transfer.
                log.Info(scope + "Call button clicked. Log On to Dialer Server.");
                if (transferTxtDestination != "")
                {
                    log.Info(scope + "Making consult call to " + transferTxtDestination);
                    callParams = new CallInteractionParameters(transferTxtDestination, CallMadeStage.Allocated);
                    if (NormalInterationManager != null)
                    {
                        //callingNumber = transferTxtDestination;
                        log.Info(scope + "Starting Normal Interaction Make Consult Call");
                        NormalInterationManager.ConsultMakeCallAsync(callParams, MakeConsultCompleted, null);
                        log.Info(scope + "Completed Normal Interaction Make Consult Call");
                        this.isConsulting = true;
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
                if (IcWorkFlow != null && IcWorkFlow.LoginResult)
                {
                    if (ActiveDialerInteraction != null && ActiveNormalInteraction != null)
                    {
                        if (InteractionList.Count > 0)
                        {
                            if (ActiveConsultInteraction != null)
                            {
                                log.Info(scope + "Starting Consult Interaction Disconnect");
                                ActiveConsultInteraction.Disconnect();
                                this.RemoveNormalInteractionFromList(ActiveConsultInteraction);
                                log.Info(scope + "Completed Consult Interaction Disconnect");
                                ActiveConsultInteraction = null;
                            }

                            if (InteractionList != null)
                            {
                                if (ActiveDialerInteraction.IsHeld)
                                {
                                    log.Info(scope + "Starting Normal Interaction Pickup");
                                    ActiveNormalInteraction = ActiveDialerInteraction;
                                    ActiveNormalInteraction.Pickup();
                                    log.Info(scope + "Completed Normal Interaction Pickup");
                                }
                                else
                                {
                                    foreach (ININ.IceLib.Interactions.Interaction CurrentInteraction in InteractionList)
                                    {
                                        if (CurrentInteraction.IsHeld)
                                        {
                                            log.Info(scope + "Starting Normal Interaction Pickup");
                                            ActiveNormalInteraction = CurrentInteraction;
                                            ActiveNormalInteraction.Pickup();
                                            log.Info(scope + "Completed Normal Interaction Pickup");
                                            break;
                                        }
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
                                log.Info(scope + "Starting Consult Interaction Disconnect");
                                ActiveConsultInteraction.Disconnect();
                                this.RemoveNormalInteractionFromList(ActiveConsultInteraction);
                                log.Info(scope + "Completed Consult Interaction Disconnect");
                                ActiveConsultInteraction = null;
                            }

                            if (InteractionList != null)
                            {
                                foreach (ININ.IceLib.Interactions.Interaction CurrentInteraction in InteractionList)
                                {
                                    if (CurrentInteraction.IsHeld)
                                    {
                                        log.Info(scope + "Starting Normal Interaction Pickup");
                                        ActiveNormalInteraction = CurrentInteraction;
                                        ActiveNormalInteraction.Pickup();
                                        log.Info(scope + "Completed Normal Interaction Pickup");
                                        break;
                                    }
                                }
                            }
                        }

                    }
                }
                this.isConsulting = false;
                update_state_info_label("Connected to: " + this.GetDialerNumber());
                update_break_status_label("");
                this.BeginInvoke(new MethodInvoker(enable_when_repickup));
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
            this.update_state_info_label("Consulting.");
            ActiveConsultInteraction = e.Interaction;
        }

        public string[] GetDialerListNumber()
        {
            if (IsManualDialing)
                return null;

            List<string> callList = new List<string>();
            Dictionary<string, string> data = this.ActiveDialerInteraction.ContactData;

            if (data.ContainsKey("is_attr_PhoneNo1") && data["is_attr_PhoneNo1"] != "")
                callList.Add(data["is_attr_PhoneNo1"]);
            if (data.ContainsKey("is_attr_PhoneNo2") && data["is_attr_PhoneNo2"] != "")
                callList.Add(data["is_attr_PhoneNo2"]);
            if (data.ContainsKey("is_attr_PhoneNo3") && data["is_attr_PhoneNo3"] != "")
                callList.Add(data["is_attr_PhoneNo3"]);
            if (data.ContainsKey("is_attr_PhoneNo4") && data["is_attr_PhoneNo4"] != "")
                callList.Add(data["is_attr_PhoneNo5"]);
            if (data.ContainsKey("is_attr_PhoneNo5") && data["is_attr_PhoneNo5"] != "")
                callList.Add(data["is_attr_PhoneNo5"]);
            if (data.ContainsKey("is_attr_PhoneNo6") && data["is_attr_PhoneNo6"] != "")
                callList.Add(data["is_attr_PhoneNo6"]);

            return callList.ToArray();
        }

        public string GetDialerNumber()
        {
            string scope = "CIC::frmMain::GetDialerNumber()::";
            string DialerNumber = "";
            log.Info(scope + "Starting.");
            if (!this.IsManualDialing)
            {
                try
                {
                    string AlternatePreview_ATTR = Properties.Settings.Default.AlternatePreviewNumbers;
                    string[] AlternatePreviewNoATTRCollection;

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
                }
                catch (Exception ex)
                {
                    log.Error(scope + "Error info." + ex.Message);
                }
            }
            else
            {
                DialerNumber = this.callingNumber;
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
                            this.SetToDoNotDisturb_UserStatusMsg();
                            state_change(FormMainState.Break);
                            this.update_break_status_label("On Break.");
                            break;
                        default:
                            break;
                    }
                    this.break_requested = false;
                    log.Info(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    log.Error(scope + "Error info." + ex.Message);
                }
            }
        }

        private void LogoutGranted(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::LogoutGranted(): ";
            log.Info(scope + "Starting.");
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
                            tryDisconnect();
                            tryDisconnectAllInteractions();
                            IcWorkFlow = null;
                            this.DialerSession = null;
                            
                            this.SetToDoNotDisturb_UserStatusMsg();
                            state_change(FormMainState.Loggedout);
                            System.Windows.Forms.MessageBox.Show(
                                global::CIC.Properties.Settings.Default.CompletedWorkflowMsg,
                                "System Info.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.toolStripWorkflowLabel.Text = "N/A";
                            break;
                        default:
                            if (ActiveNormalInteraction != null)
                            {
                                log.Info(scope + "Starting Normal Interaction Disconnect");
                                ActiveNormalInteraction.Disconnect();
                                ActiveNormalInteraction = null;
                                log.Info(scope + "Completed Normal Interaction Disconnect");
                            }
                            if (this.IC_Session != null)
                            {
                                log.Info(scope + "Starting IC_Session Disconnect");
                                this.IC_Session.Disconnect();
                                this.IC_Session = null;
                                log.Info(scope + "Completed IC_Session Disconnect");
                            }
                            state_change(FormMainState.Loggedout);
                            break;
                    }
                    // TODO: add more clean up state
                    break_requested = false;
                    break_granted = false;
                    this.isOnBreak = false;
                    log.Info(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    log.Error(scope + "Error info." + ex.Message);
                }
            }
        }

        private void SetToAvailable_UserStatusMsg()
        {
            string scope = "CIC::MainForm::SetToDoNotDisturb_UserStatusMsg(): ";
            ININ.IceLib.People.UserStatusUpdate statusUpdate = null;
            try
            {
                log.Info(scope + "Starting.");
                if (this.AvailableStatusMessageDetails != null)
                {
                    if (this.mPeopleManager != null)
                    {
                        log.Info(scope + "Starting Update User Status");
                        statusUpdate = new UserStatusUpdate(this.mPeopleManager);
                        statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                        statusUpdate.UpdateRequest();
                        log.Info(scope + "Completed Update User Status");
                    }
                }

                log.Info(scope + "completed.");
            }
            catch (System.Exception ex)
            {
                log.Error(scope + "Error info : " + ex.Message);
            }
        }

        private void SetToDoNotDisturb_UserStatusMsg()
        {
            string scope = "CIC::MainForm::SetToDoNotDisturb_UserStatusMsg(): ";
            ININ.IceLib.People.UserStatusUpdate statusUpdate = null;
            try
            {
                log.Info(scope + "Starting.");
                if (this.DoNotDisturbStatusMessageDetails != null)
                {
                    if (this.mPeopleManager != null)
                    {
                        log.Info(scope + "Starting Update User Status");
                        statusUpdate = new UserStatusUpdate(this.mPeopleManager);
                        statusUpdate.StatusMessageDetails = this.DoNotDisturbStatusMessageDetails;
                        statusUpdate.UpdateRequest();
                        log.Info(scope + "Completed Update User Status");
                    }
                }

                log.Info(scope + "completed.");
            }
            catch (System.Exception ex)
            {
                log.Error(scope + "Error info : " + ex.Message);
            }
        }

        // Src: PlaceCallToolStripButton_Click()
        private void placecall(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::placecall(): ";

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<EventArgs>(placecall), new object[] { sender, e});
            }
            else
            {
                try
                {
                    log.Info(scope + "Starting.[Place Call]");
                    if (this.ActiveDialerInteraction != null)
                    {
                        Dictionary<string, string> data = this.ActiveDialerInteraction.ContactData;
                        if (data.ContainsKey("is_attr_numbertodial"))
                        {
                            this.update_state_info_label("Calling: " + data["is_attr_numbertodial"]);
                            highlight_call();

                            this.callingNumber = data["is_attr_numbertodial"];
                            log.Info(scope + "Starting Dialer Interaction Place Preview Call");
                            this.ActiveDialerInteraction.PlacePreviewCallAsync(MakePreviewCallComplete, null);
                            log.Info(scope + "Completed Dialer Interaction Place Preview Call");
                            this.IsManualDialing = false;
                            restart_call_timer();
                            this.toolStripStatus.Text = (this.ActiveDialerInteraction != null) ?
                                this.ActiveDialerInteraction.State.ToString() + ":" +
                                this.ActiveDialerInteraction.StateDescription.ToString() : "N/A";
                            reset_timer();
                        }
                    }
                    log.Info(scope + "Completed.[Place Call]");
                }
                catch (System.Exception ex)
                {
                    log.Error(scope + "Error info : " + ex.Message);
                }
            }
        }

        private void highlight_call()
        {
            string scope = "CIC::MainForm::higilight_call(): ";

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(highlight_call));
            }
            else
            {
                if (ActiveDialerInteraction == null)
                {
                    log.Warn(scope + "ActiveDialerInteraction is null");
                    return;
                }
                Dictionary<string, string> data = this.ActiveDialerInteraction.ContactData;
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
            }
        }

        // Src: PickupToolStripButton_Click()
        private void pickup()
        {
            string scope = "CIC::frmMain::pickup()::";
            log.Info(scope + "Starting.");
            try
            {
                switch (IcWorkFlow.LoginResult)
                {
                    case true:   //Log On to Dialer Server.
                        log.Info(scope + "Pickup button clicked. Log on to Dialer Server.");
                        if (this.ActiveDialerInteraction != null && !this.isConsulting)
                        {
                            try
                            {
                                log.Info(scope + "Starting Dialer Interaction Pickup");
                                this.ActiveDialerInteraction.Pickup();
                                log.Info(scope + "Completed Dialer Interaction Pickup");
                            }
                            catch (Exception ex)
                            {
                                log.Warn(scope + "Pickup fail. Reason " + ex.Message);
                            }
                            if (!this.IsManualDialing)
                            {
                                this.CrmScreenPop();
                                update_state_info_label("Connected to: " + this.GetDialerNumber());
                                update_break_status_label("");
                            }
                        }
                        if (ActiveNormalInteraction != null && !this.isConsulting)
                        {
                            try
                            {
                                switch (ActiveNormalInteraction.InteractionType)
                                {
                                    case InteractionType.Email:
                                        //Show Mail form
                                        log.Info(scope + "Starting Normal Interaction Pickup");
                                        ActiveNormalInteraction.Pickup();
                                        log.Info(scope + "Completed Normal Interaction Pickup");
                                        break;
                                    case InteractionType.Chat:
                                        break;
                                    case InteractionType.Call:
                                        log.Info(scope + "Starting Normal Interaction Pickup");
                                        ActiveNormalInteraction.Pickup();
                                        log.Info(scope + "Completed Normal Interaction Pickup");
                                        break;
                                    default:
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Warn(scope + "Pickup fail. Reason " + ex.Message);
                            }
                        }
                        state_change(FormMainState.PreviewCall);
                        break;
                    default:     // Not Log On to Dialer Server.
                        log.Info(scope + "Pickup button clicked[Basic station].");
                        if (ActiveNormalInteraction != null)
                        {
                            try
                            {
                                switch (ActiveNormalInteraction.InteractionType)
                                {
                                    case InteractionType.Email:
                                        log.Info(scope + "Starting Normal Interaction Pickup");
                                        ActiveNormalInteraction.Pickup();
                                        log.Info(scope + "Completed Normal Interaction Pickup");
                                        this.update_state_info_label("Connected to: " + callingNumber);
                                        update_break_status_label("");
                                        break;
                                    case InteractionType.Chat:
                                        break;
                                    case InteractionType.Call:
                                        log.Info(scope + "Starting Normal Interaction Pickup");
                                        ActiveNormalInteraction.Pickup();
                                        log.Info(scope + "Completed Normal Interaction Pickup");
                                        this.update_state_info_label("Connected to: " + callingNumber);
                                        update_break_status_label("");
                                        break;
                                    default:
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Warn(scope + "Pickup fail. Reason " + ex.Message);
                            }
                        }
                        state_change(FormMainState.ManualCall);
                        break;
                }
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                log.Error("Error info." + ex.Message);
            }
        }

        public delegate void MyDelegate(string myArg);

        private void update_break_status_label(string info)
        {
            if (this.InvokeRequired)
            {
                object[] myArray = new object[1];
                myArray[0] = info;
                this.BeginInvoke(new MyDelegate(update_break_status_label), myArray);
            }
            else
            {
                this.break_status_label.Text = info;
            }
        }

        private void update_state_info_label(string info)
        {
            if (this.InvokeRequired)
            {
                object[] myArray = new object[1];
                myArray[0] = info;
                this.BeginInvoke(new MyDelegate(update_state_info_label), myArray);
            }
            else
            {
                this.state_info_label.Text = info;
            }
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
            string scope = "CIC::frmMain::FormMain_FormClosed()::";
            DisposeQueueWatcher();
            DisposeDialerSession();
            DisposeSession();
            tryDisconnect();
            tryDisconnectAllInteractions();
            if (this.ActiveDialerInteraction != null)
            {
                try
                {
                    log.Info(scope + "Starting Dialer Session Request Logout");
                    this.ActiveDialerInteraction.DialerSession.RequestLogout();
                    log.Info(scope + "Completed Dialer Session Request Logout");
                }
                catch (Exception ex)
                {
                    log.Error("DialerSession cannot RequestLogout: " + ex.Message);
                }
            }
        }

        private void tryDisconnectAllInteractions()
        {
            string scope = "CIC::frmMain::tryDisconnectAllInteractions()::";
            if (this.InteractionList != null)
            {
                foreach (ININ.IceLib.Interactions.Interaction CurrentInteraction in this.InteractionList)
                {
                    if (CurrentInteraction != null)
                    {
                        try
                        {
                            log.Info(scope + "Starting Interaction Disconnect");
                            CurrentInteraction.Disconnect();
                            log.Info(scope + "Completed Interaction Disconnect");
                        }
                        catch (Exception ex)
                        {
                            log.Error("CurrentInteraction cannot disconnect!: " + ex.Message);
                        }
                    }
                }
                this.InteractionList.Clear();
            }
        }

        private void DisposeSession()
        {
            string scope = "CIC::MainForm::DisposeSession():: ";
            log.Info(scope + "Starting.");
            try
            {
                log.Info(scope + "Creating instance of InteractionQueue");
                if (global::CIC.Program.m_Session != null)
                {
                    global::CIC.Program.m_Session.ConnectionStateChanged -= new EventHandler<ConnectionStateChangedEventArgs>(mSession_Changed);
                }
                global::CIC.Program.m_Session.Disconnect();
                log.Info(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                log.Error(scope + "Error info." + ex.Message);
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

        public bool isOnBreak { get; set; }


        public bool isFirstTimeLogin { get; set; }
    }
}
