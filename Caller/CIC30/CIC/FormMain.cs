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
        private int AutoReconnect = 2;
        private bool IsActiveConnection { get; set; }
        private bool SwapPartyFlag { get; set; }
        private string[] InteractionAttributes { get; set; }
        private ArrayList InteractionList { get; set; }

        private DataSet DsReasonCode { get; set; }
        private Interaction mInteraction { get; set; }
        private StatusMessageList AllStatusMessageList { get; set; }
        private UserStatusList AllStatusMessageListOfUser { get; set; }
        private UserStatus CurrentUserStatus { get; set; }
        private StatusMessageDetails AvailableStatusMessageDetails { get; set; }
        private WorkgroupDetails ActiveWorkgroupDetails { get; set; }
        private InteractionsManager NormalInterationManager = null;
        private PeopleManager mPeopleManager { get; set; }
        private DialerCallInteraction ActiveDialerInteraction = null;
        private ININ.IceLib.Connection.Session IC_Session = null;
        private ININ.IceLib.Dialer.DialerSession DialerSession = null;
        private FormMainState prev_state = FormMainState.Preview;
        private FormMainState current_state = FormMainState.Preview;
        private InteractionState StrConnectionState = InteractionState.None;

        private static ICWorkFlow IcWorkFlow = null;
        private static Interaction ActiveNormalInteraction { get; set; }
        private static Interaction ActiveConsultInteraction { get; set; }
        private static InteractionConference ActiveConferenceInteraction = null;
        private static NameValueCollection mDialerData { get; set; }

        private float timer;
        private string calling_phone = "0881149998";

        public bool transfer_complete = false;
 
        public FormMainState req_state_change = FormMainState.None;
        
        // index for running place call number
        private int call_idx = 0;
       
        public FormMain()
        {
            InitializeComponent(); 
            state_change(FormMainState.Disconnect);
            InitializeSession();
            this.IsActiveConnection = true; // FIXME: remove the placeholder
        }

        private void InitializeSession()
        {
            string scope = "CIC::frmMain::InitialAllComponents()::";
            //Tracing.TraceStatus(scope + "Starting");
            bool bResult = false;
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new MethodInvoker(InitializeSession));
            }
            else
            {
                try
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

                    ININ.IceLib.Connection.Session session = global::CIC.Program.m_Session;
                    Program.Initialize_dialingManager(session);
                
                    if (session != null)
                    {
                        bResult = this.SetActiveSession(session);
                        if (this.IC_Session != null)
                        {
                            // TODO: Revise these chunk
                            //this.Load_ApplicationSkin();
                            //this.Additional_InitializeComponent();
                            ININ.IceLib.Connection.ConnectionState mConnectionState;
                            try
                            {
                                mConnectionState = this.IC_Session.ConnectionState;
                            }
                            catch
                            {
                                mConnectionState = ININ.IceLib.Connection.ConnectionState.None;

                            }
                            ///this.SetStatusBarStripMsg();
                            if (mConnectionState == ININ.IceLib.Connection.ConnectionState.Up)
                            {
                                this.Initial_NormalInteraction();
                                this.InitializeQueueWatcher();
                                //this.UnifiedMessaging_StartWatching();
                               
                                //Tracing.TraceStatus(scope + "Completed.");
                            }
                            else
                            {
                                //No active connection. 
                                // TODO: set state to no active connection

                                //Tracing.TraceStatus(scope + "Cannot log on to station.please try again.");
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    // TODO: set state to no active connection
                    //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        private void InitializeQueueWatcher()
        {
            string scope = "CIC::MainForm::InitializeQueueWatcher():: ";
            //Tracing.TraceStatus(scope + "Starting.");
            try
            {
                //Tracing.TraceStatus(scope + "Creating instance of InteractionQueue");
                if (this.NormalInterationManager != null)
                {
                    this.m_InteractionQueue = new ININ.IceLib.Interactions.InteractionQueue(this.NormalInterationManager, new QueueId(QueueType.MyInteractions, this.IC_Session.UserId));
                    //Tracing.TraceStatus(scope + "Attaching event handlers");
                    this.m_InteractionQueue.InteractionAdded += new EventHandler<InteractionAttributesEventArgs>(m_InteractionQueue_InteractionAdded);
                    this.m_InteractionQueue.InteractionChanged += new EventHandler<InteractionAttributesEventArgs>(m_InteractionQueue_InteractionChanged);
                    this.m_InteractionQueue.InteractionRemoved += new EventHandler<InteractionEventArgs>(m_InteractionQueue_InteractionRemoved);
                    this.m_InteractionQueue.ConferenceInteractionAdded += new EventHandler<ConferenceInteractionAttributesEventArgs>(m_InteractionQueue_ConferenceInteractionAdded);
                    this.m_InteractionQueue.ConferenceInteractionChanged += new EventHandler<ConferenceInteractionAttributesEventArgs>(m_InteractionQueue_ConferenceInteractionChanged);
                    this.m_InteractionQueue.ConferenceInteractionRemoved += new EventHandler<ConferenceInteractionEventArgs>(m_InteractionQueue_ConferenceInteractionRemoved);
                    //Tracing.TraceStatus(scope + "Start watching for queue events");
                    this.Initialize_InteractionAttributes();
                    this.m_InteractionQueue.StartWatchingAsync(this.InteractionAttributes, null, null);
                }
                //Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void m_InteractionQueue_ConferenceInteractionRemoved(object sender, ConferenceInteractionEventArgs e)
        {
            string scope = "CIC::MainForm::m_InteractionQueue_ConferenceInteractionChanged():: ";
            //Tracing.TraceStatus(scope + "Starting.");
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<ConferenceInteractionEventArgs>(m_InteractionQueue_ConferenceInteractionRemoved), new object[] { sender, e });
            }
            else
            {
                try
                {
                    if (e.Interaction.IsWatching() != true)
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
                            if (e.ConferenceItem.IsDisconnected == true)
                            {
                                // this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
                            }
                            break;
                        default:
                            break;
                    }
                    //this.Set_ConferenceToolStrip();
                    this.ShowActiveCallInfo();
                    //Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        /*
         * TODO : add this.Set_ConferenceToolStrip();
         */

        private void m_InteractionQueue_ConferenceInteractionChanged(object sender, ConferenceInteractionAttributesEventArgs e)
        {
            string scope = "CIC::MainForm::m_InteractionQueue_ConferenceInteractionChanged():: ";
            //Tracing.TraceStatus(scope + "Starting.");
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<ConferenceInteractionAttributesEventArgs>(m_InteractionQueue_ConferenceInteractionChanged), new object[] { sender, e });
            }
            else
            {
                try
                {
                    if (e.Interaction.IsWatching() != true)
                    {
                        e.Interaction.AttributesChanged += new EventHandler<AttributesEventArgs>(DialerInteraction_AttributesChanged);
                        e.Interaction.StartWatching(this.InteractionAttributes);
                    }
                    if (e.ConferenceItem.IsWatching() != true)
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
                            if (e.ConferenceItem.IsDisconnected == true)
                            {
                                this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                            }
                            break;
                        default:
                            break;
                    }
                    //this.Set_ConferenceToolStrip();
                    this.ShowActiveCallInfo();
                    //Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                    this.ShowActiveCallInfo();
                }
            }
        }
        /*
         * TODO : add IsActiveConference_frag
         */
        private void m_InteractionQueue_ConferenceInteractionAdded(object sender, ConferenceInteractionAttributesEventArgs e)
        {
            string scope = "CIC::MainForm::m_InteractionQueue_ConferenceInteractionAdded():: ";
            //Tracing.TraceStatus(scope + "Starting.");
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<ConferenceInteractionAttributesEventArgs>(m_InteractionQueue_ConferenceInteractionAdded), new object[] { sender, e });
            }
            else
            {
                try
                {
                    if (e.Interaction.IsWatching() != true)
                    {
                        e.Interaction.AttributesChanged += new EventHandler<AttributesEventArgs>(DialerInteraction_AttributesChanged);
                        e.Interaction.StartWatching(this.InteractionAttributes);
                    }
                    //this.IsActiveConference_flag = true;
                    switch (e.Interaction.InteractionType)
                    {
                        case InteractionType.Email:
                            break;
                        case InteractionType.Chat:
                            break;
                        case InteractionType.Callback:
                            //
                            break;
                        case InteractionType.Call:
                            if (e.Interaction.IsDisconnected != true)
                            {
                                ActiveConferenceInteraction = new InteractionConference(this.NormalInterationManager, e.Interaction.InteractionType, e.ConferenceItem.ConferenceId);
                                ActiveNormalInteraction = e.Interaction;
                                this.ShowActiveCallInfo();
                            }
                            break;
                        default:
                            break;
                    }
                    //Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                }
            }
        }

        /*
         * TODO: resetActiveInfo?
         */
        private void m_InteractionQueue_InteractionRemoved(object sender, InteractionEventArgs e)
        {
            string scope = "CIC::MainForm::m_InteractionQueue_InteractionRemoved():: ";
            //Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (e.Interaction.IsWatching() != true)
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
                            if (ActiveNormalInteraction.IsDisconnected == true)
                            {
                                this.Remove_ActiveCallSelPopMenu();
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
                            if (ActiveNormalInteraction.IsDisconnected == true)
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
                        if (ActiveNormalInteraction.IsConnected != true)
                        {
                            if (ActiveNormalInteraction != null)
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
                        }
                        break;
                }
                //Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                //this.ResetActiveCallInfo();
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

        private void m_InteractionQueue_InteractionChanged(object sender, InteractionAttributesEventArgs e)
        {
            string scope = "CIC::MainForm::m_InteractionQueue_InteractionChanged():: ";
            //Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (e.Interaction.IsWatching() != true)
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
                        ActiveNormalInteraction = e.Interaction;
                        this.StrConnectionState = ActiveNormalInteraction.State;
                        if (ActiveNormalInteraction != null)
                        {
                            if (ActiveNormalInteraction.IsDisconnected == true)
                            {
                                // TODO: activate this code
                                //this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
                                ActiveNormalInteraction = this.GetAvailableInteractionFromList();
                            }
                            else
                            {
                                if (!this.BlindTransferFlag)
                                {
                                    this.Set_ActiveCallSelPopMenu();
                                }
                            }
                        }
                        if (this.BlindTransferFlag)
                        {
                            // TODO: activate this code
                            //this.ResetActiveCallInfo();
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

                //Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                if (ActiveNormalInteraction != null)
                {
                    this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                    this.Remove_ActiveCallSelPopMenu();
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
        /*
         * * TODO : Add ctmActiveCallSelection and Check this stuff
         * 
         */
        private void Remove_ActiveCallSelPopMenu()
        {
            /*
            string scope = "CIC::MainForm::Remove_ActiveCallSelPopMenu():: ";
            //Tracing.TraceStatus(scope + "Starting.");
            int RemoveIndex = -1;
            string MenuValue = "";
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new MethodInvoker(Remove_ActiveCallSelPopMenu));
            }
            else
            {
                try
                {
                    if (ActiveNormalInteraction != null)
                    {
                        MenuValue = ActiveNormalInteraction.InteractionId.Id.ToString();
                        if (this.ctmActiveCallSelection != null)
                        {
                            for (int i = 0; i < this.ctmActiveCallSelection.Items.Count; i++)
                            {
                                if (((System.Windows.Forms.ToolStripMenuItem)this.ctmActiveCallSelection.Items[i]).Name.Trim() == MenuValue.Trim())
                                {
                                    RemoveIndex = i;
                                    break;
                                }
                            }
                            if ((RemoveIndex < this.ctmActiveCallSelection.Items.Count) && (RemoveIndex >= 0))
                            {
                                if (this.ctmActiveCallSelection.Items.Count > 0)
                                {
                                    this.ctmActiveCallSelection.Items.RemoveAt(RemoveIndex);
                                }
                            }
                            if (this.ctmActiveCallSelection.Items.Count <= 0)
                            {
                                this.Reset_ActiveCallSelPopMenu();
                            }
                            if (this.ctmActiveCallSelection.Items.Count <= 0)
                            {
                                this.ActiveNormalInteraction = null;
                                this.ActiveConsultInteraction = null;
                                this.ActiveConferenceInteraction = null;
                            }
                        }
                    }
                    else
                    {
                        this.ActiveNormalInteraction = this.GetAvailableInteractionFromList();
                        if (this.ActiveNormalInteraction != null)
                        {
                            MenuValue = this.ActiveNormalInteraction.InteractionId.Id.ToString();
                            if (this.ctmActiveCallSelection != null)
                            {
                                for (int i = 0; i < this.ctmActiveCallSelection.Items.Count; i++)
                                {
                                    if (((System.Windows.Forms.ToolStripMenuItem)this.ctmActiveCallSelection.Items[i]).Name.Trim() == MenuValue.Trim())
                                    {
                                        RemoveIndex = i;
                                        break;
                                    }
                                }
                                if ((RemoveIndex < this.ctmActiveCallSelection.Items.Count) && (RemoveIndex >= 0))
                                {
                                    if (this.ctmActiveCallSelection.Items.Count > 0)
                                    {
                                        this.ctmActiveCallSelection.Items.RemoveAt(RemoveIndex);
                                    }
                                }
                                if (this.ctmActiveCallSelection.Items.Count <= 0)
                                {
                                    this.Reset_ActiveCallSelPopMenu();
                                }
                                if (this.ctmActiveCallSelection.Items.Count <= 0)
                                {
                                    this.ActiveNormalInteraction = null;
                                    this.ActiveConsultInteraction = null;
                                    this.ActiveConferenceInteraction = null;
                                }
                            }
                        }
                        else
                        {
                            this.Reset_ActiveCallSelPopMenu();
                            this.ActiveNormalInteraction = null;
                            this.ActiveConsultInteraction = null;
                            this.ActiveConferenceInteraction = null;
                        }
                    }
                    Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
             * */
            
        }

        /*
         * TODO : add InteractionID
         */
        private void RemoveNormalInteractionFromList(Interaction ActiveNormalInteraction)
        {
            /*
            string scope = "CIC::frmMain::RemoveNormalInteractionFromList(InteractionID)::";      //Over load I
            //Tracing.TraceStatus(scope + "Starting.");
            int retIndex = -1;
            int i = 0;
            if (InteractionID != null)
            {
                if (this.InteractionList != null)
                {
                    if (this.InteractionList.Count > 0)
                    {
                        for (i = 0; i < this.InteractionList.Count; i++)
                        {
                            if (((ININ.IceLib.Interactions.Interaction)this.InteractionList[i]).InteractionId.Id == InteractionID.Id)
                            {
                                retIndex = i;
                                break;
                            }
                            i++;
                        }
                        if (retIndex >= 0)
                        {
                            FormMain.ActiveNormalInteraction = (ININ.IceLib.Interactions.Interaction)this.InteractionList[retIndex];
                            this.InteractionList.RemoveAt(retIndex);
                        }
                        //}
                        //}
                    }
                    else
                    {
                        this.InteractionList.Clear();
                        this.IsMuted = false;
                    }
                    this.Remove_ActiveCallSelPopMenu();
                    //Tracing.TraceStatus(scope + "Completed.");
                    //throw new NotImplementedException();
                }
            }
             */
        }

        private void Set_ActiveCallSelPopMenu()
        {
            string scope = "CIC::MainForm::set_ActiveCallSelPopMenu():: ";
            //Tracing.TraceStatus(scope + "Starting.");
            string MenuNameValue = "";
            string MenuNameText = "";
            int EmtyMenuIndex = -1;
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new MethodInvoker(Set_ActiveCallSelPopMenu));
            }
            else
            {
                try
                {
                    if (ActiveNormalInteraction != null)
                    {
                        switch (ActiveNormalInteraction.Direction)
                        {
                            case InteractionDirection.Incoming:
                                MenuNameText = "{" + ActiveNormalInteraction.InteractionId.Id.ToString() + "} [" + ActiveNormalInteraction.LocalName + "] <- [" + ActiveNormalInteraction.RemoteDisplay + "]";
                                break;
                            case InteractionDirection.Outgoing:
                                MenuNameText = "{" + ActiveNormalInteraction.InteractionId.Id.ToString() + "} [" + ActiveNormalInteraction.LocalName + "] -> [" + ActiveNormalInteraction.RemoteDisplay + "]";
                                break;
                            default:
                                MenuNameText = "{" + ActiveNormalInteraction.InteractionId.Id.ToString() + "} [" + ActiveNormalInteraction.LocalName + "] <?> [" + ActiveNormalInteraction.RemoteDisplay + "]";
                                break;
                        }
                        MenuNameValue = ActiveNormalInteraction.InteractionId.Id.ToString();
                        if (MenuNameValue != null)
                        {
                            // TODO: recheck this code
                            //if (ActiveNormalInteraction.ImmediateAccess == true)
                            //{
                            //    System.Windows.Forms.ToolStripMenuItem menu = new System.Windows.Forms.ToolStripMenuItem(MenuNameValue, null, POPSelMenu_Clicked);
                            //    bool IsExist = false;
                            //    menu.Name = MenuNameValue;
                            //    menu.Text = MenuNameText;
                            //    switch (ActiveNormalInteraction.InteractionType)
                            //    {
                            //        case InteractionType.Email:
                            //            menu.Image = global::CIC.Properties.Resources.Exchange;
                            //            break;
                            //        case InteractionType.Chat:
                            //            menu.Image = global::CIC.Properties.Resources.information;
                            //            break;
                            //        case InteractionType.Call:
                            //            if (ActiveNormalInteraction.ConferenceId.Id.ToString().Trim() != "0")
                            //            {
                            //                menu.Image = global::CIC.Properties.Resources.Training;     //Conference Object
                            //            }
                            //            else
                            //            {
                            //                // TODO: check this code
                            //                //this.Set_ConferenceToolStrip();
                            //                menu.Image = global::CIC.Properties.Resources.HmPhone1;
                            //            }
                            //            break;
                            //        default:
                            //            menu.Image = global::CIC.Properties.Resources.HmPhone1;
                            //            break;
                            //    }
                            //    menu.Checked = false;
                            //    if (this.ctmActiveCallSelection != null)
                            //    {
                            //        for (int i = 0; i < this.ctmActiveCallSelection.Items.Count; i++)
                            //        {
                            //            if (((System.Windows.Forms.ToolStripMenuItem)this.ctmActiveCallSelection.Items[i]).Text.Trim() == "Empty")
                            //            {
                            //                EmtyMenuIndex = i;
                            //                break;
                            //            }
                            //        }
                            //        if (EmtyMenuIndex >= 0)
                            //        {
                            //            this.ctmActiveCallSelection.Items.RemoveAt(EmtyMenuIndex);
                            //        }
                            //        for (int i = 0; i < this.ctmActiveCallSelection.Items.Count; i++)
                            //        {
                            //            if (((System.Windows.Forms.ToolStripMenuItem)this.ctmActiveCallSelection.Items[i]).Name.Trim() == MenuNameValue.Trim())
                            //            {
                            //                IsExist = true;
                            //                break;
                            //            }
                            //        }
                            //        if (IsExist != true)
                            //        {
                            //            this.ctmActiveCallSelection.Items.Add(menu);
                            //        }
                            //    }
                            //}
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

        private Interaction GetAvailableInteractionFromList()
        {
            string scope = "CIC::frmMain::GetAvailableInteractionFromList()::";
            //Tracing.TraceStatus(scope + "Starting.");
            ININ.IceLib.Interactions.Interaction retInteraction = null;
            if (this.InteractionList != null)
            {
                if (this.InteractionList.Count > 0)
                {
                    foreach (ININ.IceLib.Interactions.Interaction CurrentInteraction in this.InteractionList)
                    {
                        if (CurrentInteraction != null)
                        {
                            if (CurrentInteraction.IsDisconnected != true)
                            {
                                retInteraction = CurrentInteraction;
                                break;
                            }
                        }
                    }
                }
            }
            //Tracing.TraceStatus(scope + "Completed.");
            return retInteraction;
        }

        private void m_InteractionQueue_InteractionAdded(object sender, InteractionAttributesEventArgs e)
        {
            string scope = "CIC::MainForm::m_InteractionQueue_InteractionAdded():: ";
            //Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (e.Interaction.IsWatching() != true)
                {
                    e.Interaction.AttributesChanged += new EventHandler<AttributesEventArgs>(NormalInteraction_AttributesChanged);
                    e.Interaction.StartWatching(this.InteractionAttributes);
                }
                if (this.IsMuted == true)
                {
                    e.Interaction.Mute(true);
                }
                switch (e.Interaction.InteractionType)
                {
                    case InteractionType.Email:
                        ActiveNormalEmailInteraction = new EmailInteraction(this.NormalInterationManager, e.Interaction.InteractionId);
                        //this.SetActiveEmailQueue();
                        ActiveNormalInteraction = e.Interaction;
                        this.ShowActiveCallInfo();
                        break;
                    case InteractionType.Chat:
                        //
                        break;
                    case InteractionType.Callback:
                        this.ActiveCallbackInteraction = new CallbackInteraction(this.NormalInterationManager, e.Interaction.InteractionId);
                        ActiveNormalInteraction = e.Interaction;
                        this.StrConnectionState = ActiveNormalInteraction.State;
                        this.ShowActiveCallInfo();
                        break;
                    case InteractionType.Call:
                        if (e.Interaction.IsDisconnected != true)
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
                                    ININ.IceLib.Connection.SessionSettings session_Setting = ActiveNormalInteraction.InteractionsManager.Session.GetSessionSettings();
                                    this.CallerHost = session_Setting.MachineName.ToString();
                                }
                            }
                            this.ShowActiveCallInfo();
                            if (this.IsManualDialing != true)
                            {
                                this.CrmScreenPop();
                            }
                            else
                            {
                                this.IsManualDialing = false;
                            }
                        }
                        break;
                    default:
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

        private void NormalInteraction_AttributesChanged(object sender, AttributesEventArgs e)
        {
            string scope = "CIC::MainForm::NormalInteraction_AttributesChanged():: ";
            //Tracing.TraceStatus(scope + "Starting.");
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
                //Tracing.TraceStatus(scope + "Completed.");
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
            //Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (mInteraction != null)
                {
                    if (this.InteractionList != null)
                    {
                        for (int i = 0; i < this.InteractionList.Count; i++)
                        {
                            if (((ININ.IceLib.Interactions.Interaction)this.InteractionList[i]).InteractionId == mInteraction.InteractionId)
                            {
                                chk_idx = i;
                                break;
                            }
                        }
                        if ((chk_idx >= 0) && (chk_idx < this.InteractionList.Count))
                        {
                            //Update
                            this.InteractionList[chk_idx] = mInteraction;
                        }
                        else
                        {
                            //Insert
                            this.InteractionList.Add(mInteraction);
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

        private void Initialize_InteractionAttributes()
        {
            string scope = "CIC::MainForm::Initial_InteractionAttributes():: ";
            //Tracing.TraceStatus(scope + "Starting.");
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
        }

        private void Initial_NormalInteraction()
        {
            string scope = "CIC::MainForm::Initial_NormalInteraction()::";
            //Tracing.TraceStatus(scope + "Starting.");
            try
            {
                //Tracing.TraceStatus(scope + "Getting an instance of Normal InteractionsManager.");
                this.NormalInterationManager = InteractionsManager.GetInstance(this.IC_Session);
                if (this.InteractionList == null)
                {
                    this.InteractionList = new System.Collections.ArrayList();
                }
                else
                {
                    this.InteractionList.Clear();
                }
                //Tracing.TraceStatus(scope + "Getting an instance of PeopleManager[Normal Interactions].");
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
                //Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void SessionConnectCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // TODO: implement ln 477
        }

        private void mSession_Changed(object sender, ConnectionStateChangedEventArgs e)
        {
            // TODO: 
            Application.DoEvents();
            switch (e.State)
            {
                case ININ.IceLib.Connection.ConnectionState.Attempting:
                    state_change(FormMainState.Disconnect);
                    break;
                case ININ.IceLib.Connection.ConnectionState.Up:
                    if (this.IsActiveConnection == false)
                    {
                        global::CIC.Program.IcStation.ConnectionTimes = 0;
                        this.IsActiveConnection = true;       //Set to ActiveConnection.
                        //this.SetStatusBarStripMsg();
                        //this.InitialAllComponents();
                        this.login_workflow();
                    }
                    break;
                case ININ.IceLib.Connection.ConnectionState.Down:
                    this.IsActiveConnection = false;       //Set to InActiveConnection.
                        //this.Dispose_QueueWatcher();
                        //this.DisabledAll();
                        //this.SetStatusBarStripMsg();

                    if (global::CIC.Program.m_Session != null)
                    {
                        global::CIC.Program.m_Session.Disconnect();
                        global::CIC.Program.m_Session = null;
                    }
                    global::CIC.Program.m_Session = new Session();
                    global::CIC.Program.m_Session.ConnectionStateChanged += new EventHandler<ConnectionStateChangedEventArgs>(mSession_Changed);
                    global::CIC.Program.IcStation.CurrentSession = global::CIC.Program.m_Session;
                    try
                    {
                        global::CIC.Program.IcStation.ICConnect();
                    }
                    catch
                    {

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

        private bool SetActiveSession(ININ.IceLib.Connection.Session session)
        {
            bool bResult = false;
            string scope = "CIC::frmMain::SetActiveSession()::";
            //Tracing.TraceStatus(scope + "Starting.");
            if (session == null)
            {
                //Tracing.TraceStatus(scope + "Null reference session.");
                throw new ArgumentNullException("Null reference session.");
            }
            else
            {
                //Tracing.TraceStatus(scope + "Completed.");
                bResult = true;
                this.IC_Session = session;
            }
            return bResult;
        }

        private void reset_timer()
        {
            timer1.Stop();
            timer = 10.0f;
        }
        
        private void restart_timer()
        {
            reset_timer();
            timer1.Start();
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
                frmWorkflow workflow = new CIC.frmWorkflow(global::CIC.Program.m_Session);
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
            // name1_panel.BackColor = Color.Yellow;
            // reset_timer();
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

            if (break_requested || prev_state == FormMainState.ManualCall)
            {
                break_requested = false;
                state_change(FormMainState.Break);
            }
            else
                state_change(FormMainState.Disconnect);
        }

        private void hold_button_Click(object sender, EventArgs e)
        {
            if (IcWorkFlow.LoginResult)
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
            if (IcWorkFlow.LoginResult)
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
            if (IcWorkFlow.LoginResult)
            {
                //MessageBox.Show("Please logged into dialer first");
                frmManualCall manualCall = new frmManualCall(NormalInterationManager);
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

            try
            {
                if (this.ActiveDialerInteraction == null)
                {
                    break_requested = false;
                }
                else
                {
                    switch (!break_requested)
                    {
                        case true:
                            this.ActiveDialerInteraction.DialerSession.RequestBreak();
                            break_requested = true;
                            break_requested_state();
                            //this.RequestBreakToolStripButton.Text = "Break Pending";
                            /*if (this.WorkLogoutFlag == true)
                            {
                                System.Windows.Forms.MessageBox.Show(global::CIC.Properties.Settings.Default.IncompletedCall, "Error Info.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }*/
                            break;
                        default:
                            break_requested = false;
                            break_requested_state();
                            //this.RequestBreakToolStripButton.Text = "Break Pending";
                            /*if (this.WorkLogoutFlag == true)
                            {
                                System.Windows.Forms.MessageBox.Show(global::CIC.Properties.Settings.Default.IncompletedCall, "Error Info.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }*/
                            break;
                    }
                }
                //Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                //this.RequestBreakToolStripButton.Enabled = false;
                //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void endbreak_button_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.ActiveDialerInteraction == null)
                {
                    break_requested = false;
                }
                else
                {
                    switch (break_requested)
                    {
                        case true:
                            //this.SetToAvailable_UserStatusMsg();
                            this.ActiveDialerInteraction.DialerSession.EndBreak();
                            break_requested = false;
                            break_requested_state();
                            //this.RequestBreakToolStripButton.Text = "Request Break";
                            break;
                        default :
                            break_requested = true;
                            break_requested_state();
                            //this.RequestBreakToolStripButton.Text = "Break Pending";
                            /*if (this.WorkLogoutFlag == true)
                            {
                                System.Windows.Forms.MessageBox.Show(global::CIC.Properties.Settings.Default.IncompletedCall, "Error Info.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }*/
                            break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                //this.RequestBreakToolStripButton.Enabled = false;
                //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void logout_workflow_button_Click(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::LogoutToolStripMenuItem_Click(): ";
            //Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (IcWorkFlow.LoginResult)
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
                //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
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
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new EventHandler<EventArgs>(disposition_invoke), new object[] { sender, e });
            }
            else
            {
                //Tracing.TraceStatus(scope + "Starting.[Disposition]");
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
                                ININ.IceLib.Dialer.CallCompletionParameters callCompletionParameters = new ININ.IceLib.Dialer.CallCompletionParameters(sReasoncode, sFinishcode);
                                if (this.ActiveDialerInteraction.IsDisconnected == false)
                                {
                                    this.ActiveDialerInteraction.Disconnect();
                                }

                                if (sReasoncode == ReasonCode.Scheduled)
                                {
                                    List<string> attributeNamesToWatch = this.SetAttributeList();
                                    if (this.ActiveDialerInteraction.IsWatching() != true)
                                    {
                                        this.ActiveDialerInteraction.StartWatching(attributeNamesToWatch.ToArray());
                                    }
                                    if (this.frmScheduleCallbackForm.ScheduleCallbackResult == false)
                                    {
                                        return;
                                    }
                                    this.ActiveDialerInteraction.ChangeWatchedAttributesAsync(attributeNamesToWatch.ToArray(), null, true, ChangeWatchedAttributesCompleted, null);
                                    this.ActiveDialerInteraction.CallComplete(new CallCompletionParameters(sReasoncode, sFinishcode, this.CallBackDateTime, this.ScheduleAgent, false));
                                }
                                else
                                {
                                    this.ActiveDialerInteraction.CallComplete(callCompletionParameters);
                                }

                                // TODO: check what this chuck does. wtf?
                                //if (this.BreakStatus.Trim() != "End Break")
                                //{
                                //    if (this.AvailableStatusMessageDetails != null)
                                //    {
                                //        this.userManualStatusChangeFlag = true;
                                //        statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                                //        statusUpdate.UpdateRequest();
                                //        this.imgcmbAgentStatus.SetMessage(this.AvailableStatusMessageDetails.MessageText);  //Set Available status for a new call.
                                //    }
                                //}
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
                    // TODO: change state.
                    //Tracing.TraceStatus(scope + "Completed.[Disposition]");
                }
                catch (ININ.IceLib.IceLibException ex)
                {
                    //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        public void workflow_invoke(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::WorkflowToolStripMenuItem_Click()::";
            //Tracing.TraceStatus(scope + "Starting.");
            try
            {
                //Tracing.TraceStatus(scope + "Logging into workflow. UserId=" + this.IC_Session.UserId + ", StationId=" + this.IC_Session.GetStationInfo().Id);
                IcWorkFlow = new CIC.ICWorkFlow(CIC.Program.DialingManager);
                this.DialerSession = IcWorkFlow.LogIn(((String)sender));
                //IcWorkFlow.LoginResult = IcWorkFlow.LoginResult;
                if (IcWorkFlow.LoginResult)
                {
                    this.RegisterHandlers();

                    //Tracing.TraceStatus(scope + "Completed.");
                    // TODO: change state to something
                    this.Initial_NormalInteraction();
                    this.UpdateUserStatus();
                }
                else
                {
                    // TODO: goto logout state
                    //Tracing.TraceStatus(scope + "WorkFlow [" + ((ToolStripMenuItem)sender).Text + "] logon Fail.Please try again.");
                }
                //this.ShowActiveCallInfo(); // TODO: change to state change
            }
            catch (System.Exception ex)
            {
                // TODO: goto logout state
                //Tracing.TraceStatus(scope + "Error info.Logon to Workflow[" + ((ToolStripMenuItem)sender).Text + "] : " + ex.Message);
                //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info.Logon to Workflow[" + ((ToolStripMenuItem)sender).Text + "] : " + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }  
        }

        public void conference_invoke(string transferTxtDestination)
        {
            string scope = "CIC::frmMain::CreateConferenceToolStripButton_Click()::";
            //Tracing.TraceStatus(scope + "Starting.");
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
                                    if (interact.IsDisconnected != true)
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
                                    if (interact.IsDisconnected != true)
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
                // TODO: change state
                //this.CreateConferenceToolStripButton.Enabled = false;
                //this.LeaveConferenceToolStripButton.Enabled = true;
                //Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                // TODO: change state
                //this.CreateConferenceToolStripButton.Enabled = false;
                //this.LeaveConferenceToolStripButton.Enabled = false;
                //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void MakeNewConferenceCompleted(object sender, MakeNewConferenceCompletedEventArgs e)
        {
            string scope = "CIC::frmMain::MakeNewConferenceCompleted()::";
            //Tracing.TraceStatus(scope + "Starting.");
            try
            {
                //Conference variable
                ActiveConferenceInteraction = e.InteractionConference;
                bool ConferenceCancel = e.Cancelled;
                object ConferenceuserState = e.UserState;
                System.Exception ConferenceErrMsg = e.Error;
                ActiveConsultInteraction = null;
                //Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        public void transfer_invoke(string transferTxtDestination)
        {
            string scope = "CIC::frmMain::TransferNowToolStripButton_Click()::";
            //Tracing.TraceStatus(scope + "Starting.");
            this.BlindTransferFlag = false;
            try
            {
                if (IcWorkFlow.LoginResult && ActiveNormalInteraction != null && this.ActiveDialerInteraction != null)
                {
                    if (ActiveConsultInteraction != null)
                    {
                        //Tracing.TraceNote(scope + "Performing consult transfer");
                        ActiveNormalInteraction.ConsultTransferAsync(ActiveConsultInteraction.InteractionId, TransferCompleted, null);
                    }
                    else
                    {
                        if (transferTxtDestination != "")
                        {
                            if (ActiveNormalInteraction != null)
                            {
                                ActiveNormalInteraction.BlindTransfer(transferTxtDestination);
                            }
                            //Tracing.TraceNote(scope + "Performing blind transfer");
                        }
                    }
                    ININ.IceLib.People.UserStatusUpdate statusUpdate = new UserStatusUpdate(this.mPeopleManager);
                    string sFinishcode = global::CIC.Properties.Settings.Default.ReasonCode_Transfereded;
                    ININ.IceLib.Dialer.ReasonCode sReasoncode = ININ.IceLib.Dialer.ReasonCode.Transferred;
                    CallCompletionParameters callCompletionParameters = new CallCompletionParameters(sReasoncode, sFinishcode);
                    this.ActiveDialerInteraction.CallComplete(callCompletionParameters);
                    statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                    statusUpdate.UpdateRequest();
                    //this.imgcmbAgentStatus.SetMessage(this.AvailableStatusMessageDetails.MessageText);  //Set Available status for a new call.
                }
                else
                {
                    if (ActiveNormalInteraction != null)
                    {
                        if (ActiveConsultInteraction != null)
                        {
                            //Tracing.TraceNote(scope + "Performing consult transfer");
                            if (ActiveConsultInteraction.InteractionId != ActiveNormalInteraction.InteractionId)
                            {
                                ActiveNormalInteraction.ConsultTransferAsync(ActiveConsultInteraction.InteractionId, TransferCompleted, null);
                                // TODO: activate these code
                                //this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
                                //this.RemoveNormalInteractionFromList(this.ActiveConsultInteraction);
                                this.BlindTransferFlag = true;
                            }
                            else
                            {
                                ActiveConsultInteraction = null;
                                if (this.InteractionList != null)
                                {
                                    if (this.InteractionList.Count > 0)
                                    {
                                        foreach (ININ.IceLib.Interactions.Interaction CurrentInteraction in this.InteractionList)
                                        {
                                            if (CurrentInteraction.IsDisconnected != true)
                                            {
                                                if (CurrentInteraction.InteractionId != ActiveNormalInteraction.InteractionId)
                                                {
                                                    ActiveConsultInteraction = CurrentInteraction;  //Find Consult Call
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                //
                                            }
                                        }
                                        if (ActiveConsultInteraction != null)
                                        {
                                            ActiveNormalInteraction.ConsultTransferAsync(ActiveConsultInteraction.InteractionId, TransferCompleted, null);
                                            // TODO: activate these code
                                            //this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
                                            //this.RemoveNormalInteractionFromList(this.ActiveConsultInteraction);
                                            this.BlindTransferFlag = true;
                                        }
                                    }
                                    else
                                    {
                                        if (transferTxtDestination != "")
                                        {
                                            ActiveNormalInteraction.BlindTransfer(transferTxtDestination);
                                            // TODO: activate this code
                                            //this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //Tracing.TraceNote(scope + "Performing blind transfer");
                            if (transferTxtDestination != "")
                            {
                                ActiveNormalInteraction.BlindTransfer(transferTxtDestination);
                                // TODO: activate this code
                                //this.RemoveNormalInteractionFromList(ActiveNormalInteraction);
                            }
                        }
                    }
                
                }
                // TODO: activate this code
                //this.ResetActiveCallInfo();
                //Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                // TODO: activate this code
                //this.ResetActiveCallInfo();
                //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void TransferCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.BlindTransferFlag = true;
            this.ShowActiveCallInfo();
            this.BlindTransferFlag = false;
            // TODO: activate this code
            //this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
            //this.toolStripButtonClear_Click(sender, new System.EventArgs()); 
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
            //Tracing.TraceStatus(scope + "Starting.");
            //this.imgcmbAgentStatus.ImageList.Images.Clear();
            //this.imgcmbAgentStatus.Items.Clear();
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
                            //this.imgcmbAgentStatus.ImageList = this.imsLstServerStatus;
                            //this.CmbImgAgentStatus.ImageList = this.imsLstServerStatus;
                            //this.imsLstServerStatus.Images.Clear();
                            sIconPath = CIC.Program.ResourcePath;
                            this.AllStatusMessageList.StartWatching();
                            foreach (StatusMessageDetails status in this.AllStatusMessageList.GetList())
                            {
                                sIconName = Util.GetFilenameFromFilePath(status.IconFileName.ToString());
                                sIconPath += sIconName;
                                if (System.IO.File.Exists(sIconPath) == true)
                                {
                                    Status_icon = new System.Drawing.Icon(sIconPath);
                                    //this.imsLstServerStatus.Images.Add(status.MessageText, Status_icon);
                                }
                                else
                                {
                                    //this.imsLstServerStatus.Images.Add(status.MessageText, status.Icon);
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
                               // this.imgcmbAgentStatus.Items.Add(new ImageComboBoxItem(status.MessageText, iIndex));
                                iIndex++;
                                //Tracing.TraceNote(scope + "Id=" + status.Id + ", MessageText=" + status.MessageText);
                            }
                        }
                        break;
                    default:    //Not Log On to Workflow
                        //Tracing.TraceNote(scope + "Creating instance of StatusMessageList");
                        if (this.mPeopleManager != null)
                        {
                            string[] nusers = { this.IC_Session.UserId };   //Make value to array 
                            this.AllStatusMessageList = new StatusMessageList(this.mPeopleManager);
                            this.AllStatusMessageListOfUser = new UserStatusList(this.mPeopleManager);
                            this.AllStatusMessageListOfUser.WatchedObjectsChanged += new EventHandler<WatchedObjectsEventArgs<UserStatusProperty>>(AllStatusMessageListOfUser_WatchedObjectsChanged);
                            this.AllStatusMessageListOfUser.StartWatching(nusers);
                            this.CurrentUserStatus = this.AllStatusMessageListOfUser.GetUserStatus(this.IC_Session.UserId);
                            //this.imgcmbAgentStatus.ImageList = this.imsLstServerStatus;
                            //this.CmbImgAgentStatus.ImageList = this.imsLstServerStatus;
                            //this.imsLstServerStatus.Images.Clear();
                            sIconPath = CIC.Program.ResourcePath;
                            this.AllStatusMessageList.StartWatching();
                            foreach (StatusMessageDetails status in this.AllStatusMessageList.GetList())
                            {
                                if (status.IsSelectableStatus == true)
                                {
                                    sIconName = Util.GetFilenameFromFilePath(status.IconFileName.ToString());
                                    sIconPath += sIconName;
                                    if (System.IO.File.Exists(sIconPath) == true)
                                    {
                                        Status_icon = new System.Drawing.Icon(sIconPath);
                                        //this.imsLstServerStatus.Images.Add(status.MessageText, Status_icon);
                                    }
                                    else
                                    {
                                        //this.imsLstServerStatus.Images.Add(status.MessageText, status.Icon);
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
                                    //imgcmbAgentStatus.Items.Add(new ImageComboBoxItem(status.MessageText, iIndex));
                                    iIndex++;
                                }
                               // Tracing.TraceNote(scope + "Id=" + status.Id + ", MessageText=" + status.MessageText);
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
                //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void AllStatusMessageListOfUser_WatchedObjectsChanged(object sender, WatchedObjectsEventArgs<UserStatusProperty> e)
        {
            throw new NotImplementedException();
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
                        update_info_on_dashboard();
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
                        if (this.BlindTransferFlag == true)
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
                                    if (this.SwapPartyFlag == true)
                                    {
                                        this.SetActiveCallInfo();
                                        this.ShowActiveCallInfo();
                        
                                        // restart timer and reset call index
                                        restart_timer();
                                        call_idx = 0;
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

        private void update_info_on_dashboard()
        {
            this.contractNo_box.Text = this.ActiveDialerInteraction.ContactData["is_attr_ContractNumber"];
            this.license_plate_box.Text = this.ActiveDialerInteraction.ContactData["is_attr_CarLicenseNumber"];
            this.product_name_box.Text = this.ActiveDialerInteraction.ContactData["is_attr_ProductName"];
            this.name1_box1.Text = this.ActiveDialerInteraction.ContactData["is_attr_FullName_Relation1"];
            this.name2_box1.Text = this.ActiveDialerInteraction.ContactData["is_attr_FullName_Relation2"];
            this.name3_box1.Text = this.ActiveDialerInteraction.ContactData["is_attr_FullName_Relation3"];
            this.name4_box1.Text = this.ActiveDialerInteraction.ContactData["is_attr_FullName_Relation4"];
            this.name5_box1.Text = this.ActiveDialerInteraction.ContactData["is_attr_FullName_Relation5"];
            this.name6_box2.Text = this.ActiveDialerInteraction.ContactData["is_attr_FullName_Relation6"];
            this.name1_box2.Text = this.ActiveDialerInteraction.ContactData["is_attr_PhoneNo1"];
            this.name2_box2.Text = this.ActiveDialerInteraction.ContactData["is_attr_PhoneNo2"];
            this.name3_box2.Text = this.ActiveDialerInteraction.ContactData["is_attr_PhoneNo3"];
            this.name4_box2.Text = this.ActiveDialerInteraction.ContactData["is_attr_PhoneNo4"];
            this.name5_box2.Text = this.ActiveDialerInteraction.ContactData["is_attr_PhoneNo5"];
            this.name6_box2.Text = this.ActiveDialerInteraction.ContactData["is_attr_PhoneNo6"];
            this.aging_box.Text = this.ActiveDialerInteraction.ContactData["is_attr_Aging"];
            this.base_debt_box.Text = this.ActiveDialerInteraction.ContactData["is_attr_BaseDebt"];
            this.number_due_box.Text = this.ActiveDialerInteraction.ContactData["is_attr_NumberDue"];
            this.last_amount_payment_box.Text = this.ActiveDialerInteraction.ContactData["is_attr_LastReceiveAmountPayment"];
            this.last_date_payment_box.Text = this.ActiveDialerInteraction.ContactData["is_attr_LastReceiveDatePayment"];
            this.initial_amount_box.Text = this.ActiveDialerInteraction.ContactData["is_attr_InitialAmount"];
            this.monthly_payment_box.Text = this.ActiveDialerInteraction.ContactData["is_attr_MonthlyPayment"];
            this.debt_status_box.Text = this.ActiveDialerInteraction.ContactData["is_attr_DebStuatus"];
            this.start_overdue_date_box.Text = this.ActiveDialerInteraction.ContactData["is_attr_StartOverDueDate"];
            this.followup_status_box.Text = this.ActiveDialerInteraction.ContactData["is_attr_FollowupStatus"];
            this.payment_appoint_box.Text = this.ActiveDialerInteraction.ContactData["is_attr_PaymentAppoint"];
            this.date_callback_box.Text = this.ActiveDialerInteraction.ContactData["is_attr_DateAppointCallback"];
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
            //        if (IcWorkFlow.LoginResult != true)
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
                //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
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
                //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
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
                    if (break_requested)
                        break_state();
                    else
                        preview_state();
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
            // timer1.Start();
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
            break_button.Enabled = !break_requested;
        }

        private void  disconnect_state()
        {
            // TODO: rename typo enable_all_button()
            reset_state();
            workflow_button.Enabled = true;
            exit_button.Enabled = true;
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
            break_button.Enabled = !break_requested;
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
        
        private void break_requested_state()
        {
            break_button.Enabled = break_requested;
        }


        private void logged_out_state()
        {
            reset_state();
            workflow_button.Enabled = true;
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

        private void ChangeWatchedAttributesCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.StackTrace, e.Error.Message);
                return;
            }
        }

        private void placecall_or_pickup()
        {
            bool mySwitch = true;
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
            string scope = "CIC::MainForm::WorkflowStarted()::";
            //Tracing.TraceStatus(scope + "Starting.");

            disable_logout();
            //Tracing.TraceStatus(scope + "Completed.");
        }

        private void WorkflowStopped(object sender, WorkflowStoppedEventArgs e)
        {
            string scope = "CIC::MainForm::WorkflowStopped()::";
            //Tracing.TraceStatus(scope + "Starting.");
            //this.TransferPanelToolStripButton.Enabled = true;
            //this.RequestBreakToolStripButton.Visible = false;
            enable_transfer();
            disable_break_request();
            //Tracing.TraceStatus(scope + "Completed.");
        }

        private void CampaignTransition(object sender, CampaignTransistionEventArgs e)
        {
            // NYI
            int i = 0;
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
                        
                        // restart timer and reset call index
                        restart_timer();
                        call_idx = 0;
                        
                        this.CrmScreenPop();
                        break;
                    case InteractionType.Call:
                        this.Initialize_ContactData();
                        this.ShowActiveCallInfo();
                        
                        // restart timer and reset call index
                        restart_timer();
                        call_idx = 0;
                        
                        this.CrmScreenPop();
                        break;
                }
                //Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                //Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        // Get new infomation set
        private void PreviewCallAdded(object sender, PreviewCallAddedEventArgs e)
        {
            string scope = "CIC::MainForm::PreviewCallAdded()::";
            //Tracing.TraceStatus(scope + "Starting.");
            try
            {
                //if (e.Interaction.IsWatching() != true)
                //{
                //    e.Interaction.AttributesChanged += new EventHandler<AttributesEventArgs>(DialerInteraction_AttributesChanged);
                //    e.Interaction.StartWatching(this.InteractionAttributes);
                //}
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
                        restart_timer();
                        call_idx = 0;
                        
                        this.CrmScreenPop();
                        break;
                    case InteractionType.Call:
                        this.Initialize_ContactData();
                        this.ShowActiveCallInfo();
                        
                        // restart timer and reset call index
                        restart_timer();
                        call_idx = 0;
                        
                        this.CrmScreenPop();
                        break;
                }
                //Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                //Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void CrmScreenPop()
        {
            string scope = "CIC::MainForm::CrmScreenPop()::";
            //Tracing.TraceStatus(scope + "Starting.");
            string FullyUrl = "";
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new MethodInvoker(CrmScreenPop));
            }
            else
            {
                try
                {
                    // TODO: figure out what to do with url page
                    //switch (IcWorkFlow.LoginResult)
                    //{
                    //    case true:
                    //        FullyUrl = this.GetFullyScreenUrl(mDialerData);
                    //        this.CrmScreenPop_RefreshSession();
                    //        this.CrmScreenPop_SetMainEntry(FullyUrl);
                    //        this.CrmScreenPop_RetriveLastSessionID();
                    //        break;
                    //    default:
                    //        if (Properties.Settings.Default.StartupUrl != null)
                    //        {
                    //            this.CrmScreenPop_RetriveIVRValue();
                    //            this.CrmScreenPop_SetACDEntry();
                    //        }
                    //        break;
                    //}
                    //Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    //Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                    //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                    //this.MainWebBrowser.Url = new System.Uri(global::CIC.Properties.Settings.Default.StartupUrl, System.UriKind.Absolute);
                }
            }
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
            //Tracing.TraceStatus(scope + "Completed.");
        }

        private void Initialize_CallBack()
        {
            string scope = "CIC::MainForm::Initial_CallBack()::";
            //Tracing.TraceStatus(scope + "Starting.");
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
                    //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
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
            //Tracing.TraceStatus(scope + "Call button clicked.Log On to Basic station.");
            CallInteractionParameters callParams =
                new CallInteractionParameters(number, CallMadeStage.Allocated);
            SessionSettings sessionSetting = Program.m_Session.GetSessionSettings();
            callParams.AdditionalAttributes.Add("CallerHost", sessionSetting.MachineName.ToString());
            //this.IsManualDialing = true;
            this.NormalInterationManager.MakeCallAsync(callParams, FormMain.MakeCallCompleted, null);
            //this.SetCallHistory();
        }

        public void MakeConsultCall(string transferTxtDestination)
        {
            string scope = "CIC::frmMain::MakeConsultCallToolStripButton_Click()::";
            //Tracing.TraceStatus(scope + "Starting.");
            ININ.IceLib.Interactions.CallInteractionParameters callParams = null;
            try
            {
                switch (IcWorkFlow.LoginResult)
                {
                    case true:   //Log On to Dialer Server.  use same normal to call before using dialer object to blind/consult transfer.
                        //Tracing.TraceStatus(scope + "Call button clicked. Log On to Dialer Server.");
                        if (transferTxtDestination != "")
                        {
                            //Tracing.TraceStatus(scope + "Making consult call to " + transferTxtDestination);
                            callParams = new CallInteractionParameters(transferTxtDestination, CallMadeStage.Allocated);
                            if (NormalInterationManager != null)
                            {
                                NormalInterationManager.ConsultMakeCallAsync(callParams, MakeConsultCompleted, null);
                            }
                        }
                        break;
                    default:     // Not Log On to Dialer Server.
                        //Tracing.TraceStatus(scope + "Call button clicked. Not log on to Dialer Server.");
                        if (transferTxtDestination != "")
                        {
                            //Tracing.TraceStatus(scope + "Making consult call to " + transferTxtDestination);
                            callParams = new CallInteractionParameters(transferTxtDestination, CallMadeStage.Allocated);
                        }
                        if (callParams != null)
                        {
                            NormalInterationManager.ConsultMakeCallAsync(callParams, MakeConsultCompleted, null);
                        }
                        break;
                }
                //this.EnabledTransferToolStripDisplayed();
                //Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                // TODO: Activate this code
                //this.ResetActiveCallInfo();
                //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        public void DisconnectConsultCall()
        {
            string scope = "CIC::frmMain::CancelTransferToolStripButton_Click()::";
            //Tracing.TraceStatus(scope + "Starting.");
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
                                    if (CurrentInteraction.IsHeld == true)
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
                                    if (CurrentInteraction.IsHeld == true)
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
                // TODO: activate this code
                //this.ShowActiveCallInfo();
                //Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                // TODO: activate this code
                //this.EnabledTransferToolStripDisplayed();
                //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private static void MakeConsultCompleted(object sender, InteractionCompletedEventArgs e)
        {
            ActiveConsultInteraction = e.Interaction;
        }

        private string GetDialerNumber()
        {
            string DialerNumber = "";
            string AlternatePreview_ATTR = Properties.Settings.Default.AlternatePreviewNumbers;
            string[] AlternatePreviewNoATTRCollection;
            string scope = "CIC::frmMain::GetDialerNumber()::";
            //Tracing.TraceStatus(scope + "Starting.");

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
            //Tracing.TraceStatus(scope + "Completed.");
            return DialerNumber;
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
            string scope = "CIC::MainForm::BreakGranted(): ";
            //Tracing.TraceStatus(scope + "Starting.");
            if (this.InvokeRequired == true)
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
                            //this.RequestBreakToolStripButton.Text = "End Break";

                            //this.SetToDoNotDisturb_UserStatusMsg();
                            /*
                             * Note : need CIC to use WorkLogoutFlag 
                            */
                            
                            //if (this.WorkLogoutFlag != true)
                            //{
                                // Shiw Break Status Message.
                                // System.Windows.Forms.MessageBox.Show(global::CIC.Properties.Settings.Default.CompletedBreak, "System Info.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            //}
                            break;
                        default:
                            state_change(FormMainState.Break);
                            //this.RequestBreakToolStripButton.Text = "Request Break";
                            break;
                    }
                    
                    //Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    //Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
            //throw new NotImplementedException();
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
                    switch (IcWorkFlow.LoginResult)
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
                    //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
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
                        state_info_label.Text = "Calling: " + this.ActiveDialerInteraction.ContactData["is_attr_numbertodial"];
                        
                        // find a strategy to make a call by using PlacePreviewCall() and make a normal call for the other 6 numbers 
                        switch (call_idx)
                        {
                            case 0:
                                this.ActiveDialerInteraction.PlacePreviewCall();
                                break;
                            case 1:
                                if (name1_box2.Text.Length == 0)
                                {
                                    call_idx = 2;
                                    goto case 2;
                                }
                                else 
                                {
                                    reset_color_panel();
                                    name1_panel.BackColor = Color.Yellow;
                                    // TODO: make a call by a number in name1_box2.Text
                                }
                                break;
                            case 2:
                            if (name1_box2.Text.Length == 0)
                                {
                                    call_idx = 3;
                                    goto case 3;
                                }
                                else 
                                {
                                    reset_color_panel();
                                    name2_panel.BackColor = Color.Yellow;
                                    // TODO: make a call by a number in name2_box2.Text
                                }
                                break;
                            case 3:
                                if (name1_box2.Text.Length == 0)
                                {
                                    call_idx = 4;
                                    goto case 4;
                                }
                                else 
                                {
                                    reset_color_panel();
                                    name3_panel.BackColor = Color.Yellow;
                                    // TODO: make a call by a number in name3_box2.Text
                                }
                                break;
                            case 4:
                                if (name1_box2.Text.Length == 0)
                                {
                                    call_idx = 5;
                                    goto case 5;
                                }
                                else 
                                {
                                    reset_color_panel();
                                    name4_panel.BackColor = Color.Yellow;
                                    // TODO: make a call by a number in name4_box2.Text
                                }
                                break;
                            case 5:
                                if (name1_box2.Text.Length == 0)
                                {
                                    call_idx = 6;
                                    goto case 6;
                                }
                                else 
                                {
                                    reset_color_panel();
                                    name5_panel.BackColor = Color.Yellow;
                                    // TODO: make a call by a number in name5_box2.Text
                                }
                                break;
                            case 6:
                                if (name1_box2.Text.Length == 0)
                                {
                                    break;
                                }
                                else 
                                {
                                    reset_color_panel();
                                    name6_panel.BackColor = Color.Yellow;
                                    // TODO: make a call by a number in name6_box2.Text
                                }
                                break;
                        }
                        call_idx++;
                        
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

        public ScheduleCallbackForm frmScheduleCallbackForm { get; set; }

        public DateTime CallBackDateTime { get; set; }

        public string CallBackPhone { get; set; }

        public string ScheduleAgent { get; set; }

        public InteractionQueue m_InteractionQueue { get; set; }

        public bool IsMuted { get; set; }

        public EmailInteraction ActiveNormalEmailInteraction { get; set; }

        public CallbackInteraction ActiveCallbackInteraction { get; set; }

        public string CallerHost { get; set; }

        public bool IsManualDialing { get; set; }
    }
}
