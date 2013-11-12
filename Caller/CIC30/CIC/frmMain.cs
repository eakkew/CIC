using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    public partial class frmMain : Form
    {
        #region Variable Declaration

        private ININ.IceLib.Connection.Session IC_Session = null;
        private Locus.Control.Dialpad m_Dialpad = null;
        private Locus.Control.Dialpad m_TransferDialpad = null;
        private Locus.Control.EnhancedWebBrowser MainWebBrowser = null;
        private bool IsLoggedIntoDialer = false;
        private ININ.IceLib.People.PeopleManager mPeopleManager = null;
        private ININ.IceLib.People.UserStatusList AllStatusMessageListOfUser = null;
        private ININ.IceLib.People.UserStatus CurrentUserStatus = null;
        private ININ.IceLib.People.StatusMessageDetails DoNotDisturbStatusMessageDetails = null;
        private ININ.IceLib.People.StatusMessageList AllStatusMessageList = null;
        private ININ.IceLib.Interactions.InteractionsManager NormalInterationManager = null;  //Calling Manager to outbound call [To create outbound call].
        private ININ.IceLib.Interactions.Interaction ActiveNormalInteraction = null;           //CallBack Object Of Normal Interaction.[Handle of IN/OUT Object]
        private ININ.IceLib.Dialer.DialerCallInteraction ActiveDialerInteraction = null;       //CallBack Object Of Dialer Interaction.[Handle of IN/OUT Object]
        private ININ.IceLib.Interactions.InteractionQueue m_InteractionQueue;
        private CIC.frmCamera Camera = null;
        private CIC.ICWorkFlow IcWorkFlow = null;
        private ININ.IceLib.Dialer.DialerSession DialerSession = null;
        private System.Collections.Specialized.NameValueCollection mDialerData = null;
        private bool userManualStatusChangeFlag = false;
        private ININ.IceLib.Interactions.Interaction ActiveConsultInteraction = null;
        private System.Data.DataSet DsReasonCode = null;
        private CIC.frmIVRList IVRMenuList = null;
        private CIC.frmAboutBox AboutBox = new frmAboutBox();
        private string sCollectUserSelect = "";
        private System.Collections.ArrayList InteractionList = new System.Collections.ArrayList();
        private ININ.IceLib.People.StatusMessageDetails AvailableStatusMessageDetails = null;
        private ININ.IceLib.Interactions.InteractionConference ActiveConferenceInteraction = null;
        private ININ.IceLib.Interactions.EmailInteraction ActiveNormalEmailInteraction = null;
        private ININ.IceLib.UnifiedMessaging.UnifiedMessagingManager NormalUnifiedMessagingManager = null;
        private ININ.IceLib.Interactions.EmailResponse emailResponse = null;
        //private ININ.IceLib.Interactions.RecorderInteraction ActiveRecorderInteraction = null;
        //private ININ.IceLib.Interactions.ChatInteraction ActiveChatInteraction = null;
        private ININ.IceLib.Interactions.CallbackInteraction ActiveCallbackInteraction = null;
        private System.Collections.ArrayList EmailResponseList = new System.Collections.ArrayList();
        private ININ.IceLib.People.WorkgroupDetails ActiveWorkgroupDetails = null;
        private bool IsManualDialing = false;
        private string BreakStatus = "";
        private Locus.Control.MP3Player cicMp3Player = null;
        private string AlertSoundFileType = "";
        private List<string> _WorkgroupDetailsAttributeNames = new List<string>();
        private string[] InteractionAttributes = null;
        private CIC.frmMailDocView EmailDocView = null;
        private CIC.ScheduleCallbackForm frmScheduleCallbackForm = null;
        private bool IsInitialUICompleted = false;
        private System.DateTime CallBackDateTime;
        private string CallBackPhone;
        private bool BlindTransferFlag = false;
        private int CtmMenuIndex = -1;
        private string ScheduleAgent;
        private string CallerHost = "";
        private string StrConnectionState = "";
        private string crmSID = "";
        private System.Media.SoundPlayer soundPlayer = null;
        private bool IsPlayAlerting = false;
        private bool SwapPartyFlag = false;
        private string[] workflows = null;
        private bool IsMuted = false;
        private bool IsActiveConnection = false;
        private bool WorkLogoutFlag = false;
        private bool IsDialingEnabled = false;
        private bool IsActiveConference_flag = false;
        private int AutoReConnected = 2;    //Fixed default value to 2.

        #endregion

        #region Additional InitializeComponent Function

        private void Additional_InitializeComponent()
        {
            string scope = "CIC::frmMain::Additional_InitializeComponent()::";
            Tracing.TraceStatus(scope + " Starting.");
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new MethodInvoker(Additional_InitializeComponent));
            }
            else
            {
                try
                {
                    if (this.IsInitialUICompleted != true)
                    {
                        this.IsInitialUICompleted = true;
                        //DialPad ToolStrip
                        this.m_Dialpad = new Locus.Control.Dialpad();
                        this.DialpadToolStripDropDownButton.DropDown.Items.Clear();
                        this.DialpadToolStripDropDownButton.DropDown.Items.Add(new ToolStripControlHost(m_Dialpad));
                        this.m_Dialpad.KeyPressed += new Locus.Control.Dialpad.KeyPressedHandler(m_Dialpad_KeyPressed);

                        //m_TransferDialpad ToolStrip
                        this.m_TransferDialpad = new Locus.Control.Dialpad();
                        this.toolStripDropDownTransferDialPad.DropDown.Items.Clear();
                        this.toolStripDropDownTransferDialPad.DropDown.Items.Add(new ToolStripControlHost(this.m_TransferDialpad));
                        this.m_TransferDialpad.KeyPressed += new Locus.Control.Dialpad.KeyPressedHandler(m_TransferDialpad_KeyPressed);

                        //WebBrowser Control ToolStrip
                        this.toolStripContainer1.TopToolStripPanel.Controls.Remove(WebBrowserCtrlToolStrip);  //Remove it from pannel container
                        this.WebBrowserCtrlToolStrip.Dock = DockStyle.None;
                        this.WebBrowserToolStripDropDownButton.DropDownItems.Clear();
                        this.WebBrowserToolStripDropDownButton.DropDownItems.Add(new ToolStripControlHost(WebBrowserCtrlToolStrip));

                        //WebBrowser View 
                        this.MainWebBrowser = new Locus.Control.EnhancedWebBrowser();
                        this.MainWebBrowser.Dock = DockStyle.Fill;
                        //Local Url
                        //this.MainWebBrowser.Url = new System.Uri(Program.ApplicationPath+"\\home\\home.html", System.UriKind.Absolute);

                        //CRM LogIN URL 
                        if (Properties.Settings.Default.StartupUrl != null)
                        {
                            if (Properties.Settings.Default.AutoCRMSignOn == true)
                            {
                                string sUrl = System.String.Format(Properties.Settings.Default.StartupUrl.ToString(), global::CIC.Program.mLoginParam.UserId , global::CIC.Program.mLoginParam.Password);
                                this.MainWebBrowser.Url = new System.Uri(sUrl, System.UriKind.Absolute);
                            }
                            else
                            {
                                this.MainWebBrowser.Url = new System.Uri(Properties.Settings.Default.StartupUrl, System.UriKind.Absolute);
                            }
                        }

                        string sDoc = this.MainWebBrowser.DocumentText;
                        this.crmSID = this.GetStringSection(sDoc, Properties.Settings.Default.startSearchKey, Properties.Settings.Default.endSearchKey);
                        //this.crmSID = "SID=27062998039393";  //Fixed for test

                        this.MainWebBrowser.IsWebBrowserContextMenuEnabled = global::CIC.Properties.Settings.Default.Display_WebMenu;
                        this.MainWebBrowser.ScriptErrorsSuppressed = true;
                        this.MainWebBrowser.ProgressChanged += new WebBrowserProgressChangedEventHandler(WebBrowser_ProgressChanged);
                        this.MainWebBrowser.Navigated += new WebBrowserNavigatedEventHandler(WebBrowser_Navigated);
                        this.MainWebBrowser.CanGoBackChanged += new EventHandler(WebBrowser_CanGoBackChanged);
                        this.MainWebBrowser.CanGoForwardChanged += new EventHandler(WebBrowser_CanGoForwardChanged);
                        this.MainWebBrowser.StatusTextChanged += new EventHandler(WebBrowser_StatusTextChanged);
                        this.MainSplitContainer.Panel1.Controls.Add(this.MainWebBrowser);
                        this.MainSplitContainer.Panel1.Controls.SetChildIndex(this.MainWebBrowser, 0);

                        //Hide Transfer Pannel
                        this.MainSplitContainer.Panel2Collapsed = true;

                        //FullScreen ToolStrip
                        this.fullScreenToolStripMenuItem.Checked = true;

                        //Agent Status ToolStrip
                        if (this.AgentStatusToolStrip.Items.Count > 0)
                        {
                            this.AgentStatusToolStrip.Items.RemoveAt(1);  //Time to reconnected
                            this.AgentStatusToolStrip.Items.Insert(1, new ToolStripControlHost(this.imgcmbAgentStatus));
                        }

                        //Set size of userStatusGridView
                        this.muserStatusGridView.Dock = DockStyle.Fill;

                        //Enabled Camera
                        this.mnuCamera.Visible = Properties.Settings.Default.CamEnable;

                        //Set Application Title
                        this.Text = Properties.Settings.Default.ApplicationTitle;

                        //
                        if (global::CIC.Properties.Settings.Default.DisplayIVR_Screen == true)
                        {
                            this.IVRMenu.Visible = true;
                        }
                        else
                        {
                            this.IVRMenu.Visible = false;
                        }
                        //Set ToolStrip Location
                        MainMnuStrip.Location = new Point(3, 0);
                        WebbrowserToolStrip.Location = new Point(256, 0);//537
                        AgentStatusToolStrip.Location = new Point(362, 0);
                        TelephonyToolStrip.Location = new Point(3, 25);
                        WorkflowCallToolStrip.Location = new Point(3, 50);
                        this.LogoutToolStripMenuItem.Enabled = false;
                        this.LeaveConferenceToolStripButton.Visible = false;
                    }
                    Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        #endregion

        #region Form Object

        private void InitialAllComponents()
        {
            string scope = "CIC::frmMain::InitialAllComponents()::";
            Tracing.TraceStatus(scope + "Starting");
            ININ.IceLib.Connection.Session session = global::CIC.Program.m_Session;
            bool bResult = false;
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new MethodInvoker(InitialAllComponents));
            }
            else
            {
                try
                {
                    if (session != null)
                    {
                        bResult = this.SetActiveSession(session);
                        if (this.IC_Session != null)
                        {
                            this.Load_ApplicationSkin();
                            this.Additional_InitializeComponent();
                            ININ.IceLib.Connection.ConnectionState mConnectionState;
                            try
                            {
                                mConnectionState = this.IC_Session.ConnectionState;
                            }
                            catch
                            {
                                mConnectionState = ININ.IceLib.Connection.ConnectionState.None;

                            }
                            this.SetStatusBarStripMsg();
                            if (mConnectionState == ININ.IceLib.Connection.ConnectionState.Up)
                            {
                                this.Initial_NormalInteraction();
                                this.InitializeQueueWatcher();
                                this.UnifiedMessaging_StartWatching();
                                if (this.muserStatusGridView != null)
                                {
                                    this.muserStatusGridView.Initialize(this.IC_Session);
                                }
                                this.InitializeStatusMessageDetails();
                                if (this.IsDialingEnabled == true)
                                {
                                    this.CallToolStripSplitButton.Enabled = true;
                                }
                                else
                                {
                                    this.CallToolStripSplitButton.Enabled = false;
                                }
                                this.TelephonyToolStrip.Enabled = true;
                                this.TransferTxtDestination.Enabled = true;
                                this.PickupToolStripButton.Enabled = false;
                                this.MuteToolStripButton.Enabled = false;
                                this.HoldToolStripButton.Enabled = false;
                                this.DisconnectToolStripButton.Enabled = false;
                                this.imgcmbAgentStatus.Enabled = true;
                                this.PhoneNumberToolStripTextBox.Text = "";
                                this.DialpadToolStripDropDownButton.Enabled = true;
                                this.PlaceCallToolStripButton.Enabled = false;
                                this.SkipCallToolStripButton.Enabled = false;
                                this.CallActivityCodeToolStripComboBox.Enabled = false;
                                this.DispositionToolStripButton.Enabled = false;
                                this.RequestBreakToolStripButton.Enabled = false;
                                this.MakeConsultCallToolStripButton.Enabled = false;
                                this.TransferNowToolStripButton.Enabled = false;
                                this.SpeakToCallerToolStripButton.Enabled = false;
                                this.CancelTransferToolStripButton.Enabled = false;
                                this.CreateConferenceToolStripButton.Enabled = false;
                                this.LeaveConferenceToolStripButton.Enabled = false;
                                if (this.IsInitialUICompleted == true)
                                {
                                    //CRM LogIN URL 
                                    //this.MainWebBrowser.Url = new System.Uri(Properties.Settings.Default.StartupUrl, System.UriKind.Absolute);
                                    if (Properties.Settings.Default.StartupUrl != null)
                                    {
                                        if (Properties.Settings.Default.AutoCRMSignOn == true)
                                        {
                                            string sUrl = System.String.Format(Properties.Settings.Default.StartupUrl.ToString(), global::CIC.Program.mLoginParam.UserId, global::CIC.Program.mLoginParam.Password);
                                            this.MainWebBrowser.Url = new System.Uri(sUrl, System.UriKind.Absolute);
                                        }
                                        else
                                        {
                                            this.MainWebBrowser.Url = new System.Uri(Properties.Settings.Default.StartupUrl, System.UriKind.Absolute);
                                        }
                                    }                                    
                                    string sDoc = this.MainWebBrowser.DocumentText;
                                    this.crmSID = this.GetStringSection(sDoc, Properties.Settings.Default.startSearchKey, Properties.Settings.Default.endSearchKey);
                                    //this.crmSID = "SID=27062998039393";  //Fixed for test
                                }
                                Tracing.TraceStatus(scope + "Completed.");
                            }
                            else
                            {
                                //No active connection. 
                                this.CallToolStripSplitButton.Enabled = false;
                                this.PickupToolStripButton.Enabled = false;
                                this.MuteToolStripButton.Enabled = false;
                                this.HoldToolStripButton.Enabled = false;
                                this.DisconnectToolStripButton.Enabled = false;
                                this.TelephonyToolStrip.Enabled = false;
                                this.TransferTxtDestination.Enabled = false;
                                this.imgcmbAgentStatus.Enabled = false;
                                this.imgcmbAgentStatus.Items.Clear();
                                this.PhoneNumberToolStripTextBox.Text = "";
                                this.DialpadToolStripDropDownButton.Enabled = false;
                                this.PlaceCallToolStripButton.Enabled = false;
                                this.SkipCallToolStripButton.Enabled = false;
                                this.CallActivityCodeToolStripComboBox.Enabled = false;
                                this.DispositionToolStripButton.Enabled = false;
                                this.RequestBreakToolStripButton.Enabled = false;
                                this.RequestBreakToolStripButton.Visible = false;
                                this.imgcmbAgentStatus.Enabled = false;
                                Tracing.TraceStatus(scope + "Cannot log on to station.please try again.");
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    this.CallToolStripSplitButton.Enabled = false;
                    this.PickupToolStripButton.Enabled = false;
                    this.MuteToolStripButton.Enabled = false;
                    this.HoldToolStripButton.Enabled = false;
                    this.DisconnectToolStripButton.Enabled = false;
                    this.imgcmbAgentStatus.Enabled = false;
                    this.imgcmbAgentStatus.Items.Clear();
                    this.PhoneNumberToolStripTextBox.Text = "";
                    this.DialpadToolStripDropDownButton.Enabled = false;
                    this.PlaceCallToolStripButton.Enabled = false;
                    this.SkipCallToolStripButton.Enabled = false;
                    this.CallActivityCodeToolStripComboBox.Enabled = false;
                    this.DispositionToolStripButton.Enabled = false;
                    this.RequestBreakToolStripButton.Enabled = false;
                    this.RequestBreakToolStripButton.Visible = false;
                    this.imgcmbAgentStatus.Enabled = false;
                    Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        public frmMain()
        {
            string scope = "CIC::frmMain::frmMain()::";  //Support Switch Over
            Tracing.TraceStatus(scope + "Starting");
            try
            {
                this.InitializeComponent();
                if (global::CIC.Properties.Settings.Default.SingleSignOn == true)
                {
                    this.toolStripMenuChangePassword.Visible = false;
                }
                else
                {
                    if (global::CIC.Program.mLoginParam.WindowsAuthentication == true)
                    {
                        this.toolStripMenuChangePassword.Visible = false;
                    }
                    else
                    {
                        this.toolStripMenuChangePassword.Visible = true;
                    }
                }
                Application.DoEvents();
                global::CIC.Program.m_Session = new Session();
                global::CIC.Program.IcStation = new ICStation(global::CIC.Program.m_Session);
                global::CIC.Program.m_Session.SetAutoReconnectInterval(this.AutoReConnected);   //Time in seccond to Reconnected.
                global::CIC.Program.m_Session.ConnectionStateChanged += new EventHandler<ConnectionStateChangedEventArgs>(mSession_Changed);
                global::CIC.Program.IcStation.LogIn(global::CIC.Program.mLoginParam.WindowsAuthentication, global::CIC.Program.mLoginParam.UserId, global::CIC.Program.mLoginParam.Password, global::CIC.Program.mLoginParam.Server, global::CIC.Program.mLoginParam.StationType, global::CIC.Program.mLoginParam.StationId, global::CIC.Program.mLoginParam.PhoneNumber, global::CIC.Program.mLoginParam.Persistent, this.SessionConnectCompleted, null);
                this.InitialAllComponents();
            }
            catch (System.Exception ex)
            {
                this.CallToolStripSplitButton.Enabled = false;
                this.PickupToolStripButton.Enabled = false;
                this.MuteToolStripButton.Enabled = false;
                this.HoldToolStripButton.Enabled = false;
                this.DisconnectToolStripButton.Enabled = false;
                this.imgcmbAgentStatus.Enabled = false;
                this.imgcmbAgentStatus.Items.Clear();
                this.PhoneNumberToolStripTextBox.Text = "";
                this.DialpadToolStripDropDownButton.Enabled = false;
                this.PlaceCallToolStripButton.Enabled = false;
                this.SkipCallToolStripButton.Enabled = false;
                this.CallActivityCodeToolStripComboBox.Enabled = false;
                this.DispositionToolStripButton.Enabled = false;
                this.RequestBreakToolStripButton.Enabled = false;
                this.RequestBreakToolStripButton.Visible = false;
                this.imgcmbAgentStatus.Enabled = false;
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            } 
        }

        private void DisabledAll()
        {
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new MethodInvoker(DisabledAll));
            }
            else
            {
                this.CallToolStripSplitButton.Enabled = false;
                this.imgcmbAgentStatus.Items.Clear();
                this.imgcmbAgentStatus.Enabled = false;
                this.TelephonyToolStrip.Enabled = false;
                this.TransferTxtDestination.Enabled = false;
                this.TransferTxtDestination.Text = "";
                this.TelephonyToolStrip.Text = "";
                this.muserStatusGridView.ClearItem();
            }
        }

        private void mSession_Changed(System.Object Sender, ININ.IceLib.Connection.ConnectionStateChangedEventArgs e)
        {
            Application.DoEvents();
            switch (e.State)
            {
                case ININ.IceLib.Connection.ConnectionState.Attempting:
                    break;
                case ININ.IceLib.Connection.ConnectionState.Up:
                    if (this.IsActiveConnection == false)
                    {
                        global::CIC.Program.IcStation.ConnectionTimes = 0;
                        this.IsActiveConnection = true;       //Set to ActiveConnection.
                        this.SetStatusBarStripMsg();
                        this.InitialAllComponents();
                    }
                    break;
                case ININ.IceLib.Connection.ConnectionState.Down:
                    if (this.IsActiveConnection == true)
                    {
                        this.IsActiveConnection = false;       //Set to InActiveConnection.
                        this.Dispose_QueueWatcher();
                        this.DisabledAll();
                        this.SetStatusBarStripMsg();
                    }
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
                    this.SetStatusBarStripMsg();
                    break;
                case ININ.IceLib.Connection.ConnectionState.None:
                    global::CIC.Program.IcStation.ICConnect();
                    this.SetStatusBarStripMsg();
                    break;
                default:
                    break;
            }
        }

        private void SessionConnectCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string scope = "CIC::frmMain::SessionConnectCompleted()::";
            Tracing.TraceStatus(scope + "Starting");
            this.MustChangePassword();
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void Load_ApplicationSkin()
        {
            string scope = "CIC::frmMain::Load_ApplicationSkin()::";
            Tracing.TraceStatus(scope + "Starting");
            try
            {
                this.toolStripContainer1.TopToolStripPanel.BackgroundImage = global::CIC.Program.AppImageList.Images["MainBackground"];
                this.toolStripContainer1.TopToolStripPanel.BackgroundImageLayout = ImageLayout.Stretch;
                this.StatusBarStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["MainBackground"];
                this.StatusBarStrip.BackgroundImageLayout = ImageLayout.Stretch;
                this.TransferToolStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["TransferBar"];
                this.TransferToolStrip.BackgroundImageLayout = ImageLayout.Stretch;
                this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["NormalBar_Normal"];
                this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            if (global::CIC.Properties.Settings.Default.Animate == true)
            {
                RECT rcFrom = new RECT();
                RECT rcTo = new RECT();
                Rectangle screenBounds;
                screenBounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                rcFrom.Left = screenBounds.Width / 2;
                rcFrom.Top = screenBounds.Height / 2;
                rcFrom.Right = rcFrom.Left;
                rcFrom.Bottom = rcFrom.Top;
                rcTo.Left = (screenBounds.Width / 2) - this.Width / 2;
                rcTo.Top = (screenBounds.Height / 2) - this.Height / 2;
                rcTo.Right = rcTo.Left + this.Width;
                rcTo.Bottom = rcTo.Top + this.Height;
                this.DrawFrameAnimation(rcFrom, rcTo);
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.exitToolStripMenuItem.Enabled == false)
            {
                e.Cancel = true;
                System.Windows.Forms.MessageBox.Show(global::CIC.Properties.Settings.Default.IncompletedWorkflowMsg, "Information.", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if (global::CIC.Properties.Settings.Default.Animate == true)
                {
                    bool b;
                    b = Native.AnimateWindow(this.Handle, 1000, Native.AW_BLEND | Native.AW_HIDE);
                    base.OnClosing(e);
                }
            }
        }

        private void DrawFrameAnimation(RECT rcFrom, RECT rcTo)
        {
            int nSteps = 15;
            IntPtr hdc;
            IntPtr hdcForm;
            IntPtr hdcCompatible;
            IntPtr hbmForm;
            Native.GdiFlush();
            System.Threading.Thread.Sleep(50);
            hdc = Native.GetDC(IntPtr.Zero);
            hdcForm = Native.GetDC(this.Handle);
            hdcCompatible = Native.CreateCompatibleDC(hdcForm);
            hbmForm = Native.CreateCompatibleBitmap(hdcForm, this.Width, this.Height);
            Native.SelectObject(hdcCompatible, hbmForm);
            Native.BitBlt(hdcCompatible, 0, 0, this.Width, this.Height, hdcForm, 0, 0, Native.SRCCOPY);
            for (int i = 0; i < nSteps; i++)
            {
                RECT curRect = new RECT();
                int xStart;
                int yStart;
                double fraction = (double)i / (double)nSteps;
                curRect.Left = rcFrom.Left + (int)((double)(rcTo.Left - rcFrom.Left) * fraction);
                curRect.Top = rcFrom.Top + (int)((double)(rcTo.Top - rcFrom.Top) * fraction);
                curRect.Right = rcFrom.Right + (int)((double)(rcTo.Right - rcFrom.Right) * fraction);
                curRect.Bottom = rcFrom.Bottom + (int)((double)(rcTo.Bottom - rcFrom.Bottom) * fraction);
                xStart = (this.Bounds.Right - this.Bounds.Left) - this.Width / 2 - ((curRect.Right - curRect.Left) / 2);
                yStart = (this.Bounds.Bottom - this.Bounds.Top) - this.Height / 2 - ((curRect.Bottom - curRect.Top) / 2);
                Native.BitBlt(hdc, curRect.Left, curRect.Top, (curRect.Right - curRect.Left), (curRect.Bottom - curRect.Top),hdcCompatible, 0, 0, Native.SRCCOPY);
                Native.GdiFlush();
                System.Threading.Thread.Sleep(20);
            }
            Native.ReleaseDC(this.Handle, hdcForm);
            Native.DeleteDC(hdcCompatible);
            Native.DeleteObject(hbmForm);
            Native.ReleaseDC(IntPtr.Zero, hdc);
        }

        private void toolStripMenuChangePassword_Click(object sender, EventArgs e)
        {
            this.ShowChangePasswordDialog();
        }

        private void ShowChangePasswordDialog()
        {
            CIC.frmChangePassword changePasswordObject = new frmChangePassword();
            changePasswordObject.Show();
        }

        private void MustChangePassword()
        {
            string scope = "CIC::MainForm::MustChangePassword()::";
            Tracing.TraceStatus(scope + "Starting.");
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new MethodInvoker(MustChangePassword));
            }
            else
            {
                if (global::CIC.Program.m_Session != null)
                {
                    ININ.IceLib.Connection.Extensions.Security SecurityObject = new ININ.IceLib.Connection.Extensions.Security(global::CIC.Program.m_Session);
                    ININ.IceLib.Connection.Extensions.PasswordPolicy passwordPolicyObject = SecurityObject.GetPasswordPolicy();
                    if (passwordPolicyObject.MustChangePassword == true)
                    {
                        this.ShowChangePasswordDialog();
                    }
                }
            }
            Tracing.TraceStatus(scope + "Starting.");
        }

        #endregion

        #region Setup Active Session Function
        
        private bool SetActiveSession(ININ.IceLib.Connection.Session session)
        {
            bool bResult = false;
            string scope = "CIC::frmMain::SetActiveSession()::";
            Tracing.TraceStatus(scope + "Starting.");
            if (session == null)
            {
                Tracing.TraceStatus(scope + "Null reference session.");
                throw new ArgumentNullException("Null reference session.");
            }
            else
            {
                Tracing.TraceStatus(scope + "Completed.");
                bResult = true;
                this.IC_Session = session;
            }
            return bResult;
        }
        
        #endregion

        #region Application Status Bar Function

        private void SetStatusBarStripMsg()
        {
            string scope = "CIC::frmMain::SetStatusBarStripMsg()::";
            Tracing.TraceStatus(scope + "Starting.");
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new MethodInvoker(SetStatusBarStripMsg));
            }
            else
            {
                try
                {
                    //Application.DoEvents();
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
                        switch (mConnectionState)
                        {
                            case ININ.IceLib.Connection.ConnectionState.Up:
                                this.TelephonyServerToolStripStatusLabel.Text = this.IC_Session.ICServer;
                                this.TelephonyServerToolStripStatusLabel.Image = global::CIC.Properties.Resources.Inet_Con;
                                this.TelephonyUserToolStripStatusLabel.Text = this.IC_Session.UserId;
                                this.TelephonyStationToolStripStatusLabel.Text = this.IC_Session.GetStationInfo().Id;
                                Tracing.TraceStatus(scope + "Completed.");
                                break;
                            case ININ.IceLib.Connection.ConnectionState.Down:
                                this.TransferStatusToolStripLabel.Text = "Not connected";
                                this.TelephonyServerToolStripStatusLabel.Text = "Not connected";
                                this.TelephonyServerToolStripStatusLabel.Image = global::CIC.Properties.Resources.Inet_DisCon;
                                this.TelephonyUserToolStripStatusLabel.Text = "Not connected";
                                this.TelephonyStationToolStripStatusLabel.Text = "Not connected";
                                Tracing.TraceStatus(scope + "No Active session.");
                                break;
                            case ININ.IceLib.Connection.ConnectionState.Attempting:
                                break;
                            case ININ.IceLib.Connection.ConnectionState.None:
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        this.TransferStatusToolStripLabel.Text = "Not connected";
                        this.TelephonyServerToolStripStatusLabel.Text = "Not connected";
                        this.TelephonyServerToolStripStatusLabel.Image = global::CIC.Properties.Resources.Inet_DisCon;
                        this.TelephonyUserToolStripStatusLabel.Text = "Not connected";
                        this.TelephonyStationToolStripStatusLabel.Text = "Not connected";
                        Tracing.TraceStatus(scope + "No Active session.");
                    }
                }
                catch (System.Exception ex)
                {
                    Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        #endregion

        #region Dialpad Function

        private void m_TransferDialpad_KeyPressed(object sender, DialpadEventArgs e)
        {
            string scope = "CIC::frmMain::m_TransferDialpad_KeyPressed_KeyPressed():: ";
            Tracing.TraceStatus(scope + "Key pressed. Key=" + e.Key.ToString());
            try
            {
                if (e.Key.ToString().Trim() != "C")
                {
                    this.TransferTxtDestination.Text += e.Key.ToString();
                }
                else
                {
                    this.TransferTxtDestination.Text = "";
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error occurred while playing digits." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void m_Dialpad_KeyPressed(object sender, DialpadEventArgs e)
        {
            string scope = "CIC::frmMain::m_Dialpad_KeyPressed():: ";
            Tracing.TraceStatus(scope + "Key pressed. Key=" + e.Key.ToString());
            try
            {
                switch (this.IsLoggedIntoDialer)
                {
                    case true:
                        if (this.ActiveDialerInteraction != null)
                        {
                            if (this.ActiveDialerInteraction.IsConnected == true)
                            {
                                if (e.Key.ToString().Trim() != "C")
                                {
                                    this.ActiveDialerInteraction.PlayDigits(e.Key.ToString());
                                }
                            }
                        }
                        break;
                    default:
                        if (this.ActiveNormalInteraction != null)
                        {
                            if (this.ActiveNormalInteraction.IsConnected == true)
                            {
                                CallInteraction call = this.ActiveNormalInteraction as CallInteraction;
                                call.PlayDigits(e.Key.ToString());
                            }
                            else
                            {
                                if (e.Key.ToString().Trim() != "C")
                                {
                                    this.PhoneNumberToolStripTextBox.Text += e.Key.ToString();
                                }
                                else
                                {
                                    this.PhoneNumberToolStripTextBox.Text = "";
                                }
                            }
                        }
                        else
                        {
                            if (e.Key.ToString().Trim() != "C")
                            {
                                this.PhoneNumberToolStripTextBox.Text += e.Key.ToString();
                            }
                            else
                            {
                                this.PhoneNumberToolStripTextBox.Text = "";
                            }
                        }
                        break;
                }
                this.DisplayDialPadPlayDigits(e.Key.ToString());
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error occurred while playing digits." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void toolStripDropDownTransferDialPad_DropDownOpened(object sender, EventArgs e)
        {
            this.m_TransferDialpad.DialPadValue = this.TransferTxtDestination.Text;
        }

        private void DisplayDialPadPlayDigits(string sDigit)
        {
            //Sent dialpad value to txtphonenumbet
        }

        #endregion

        #region WebBrowser View Function

        private void WebBrowser_CanGoBackChanged(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::WebBrowser_CanGoBackChanged():: ";
            Tracing.TraceStatus(scope + "Starting.");
            GoBackToolStripButton.Enabled = this.MainWebBrowser.CanGoBack;
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void WebBrowser_CanGoForwardChanged(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::WebBrowser_CanGoForwardChanged():: ";
            Tracing.TraceStatus(scope + "Starting.");
            GoForwardToolStripButton.Enabled = this.MainWebBrowser.CanGoForward;
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void WebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            string scope = "CIC::frmMain::WebBrowser_Navigated():: ";
            Tracing.TraceStatus(scope + "Starting.");
            UrlToolStripTextBox.Text = this.MainWebBrowser.Url.AbsoluteUri;
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void WebBrowser_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
        {
            string scope = "CIC::frmMain::WebBrowser_ProgressChanged():: ";
            Tracing.TraceStatus(scope + "Starting.");
            WebBrowserProgressToolStripProgressBar.Maximum = (int)e.MaximumProgress;
            WebBrowserProgressToolStripProgressBar.Value = (int)e.CurrentProgress;
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void WebBrowser_StatusTextChanged(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::WebBrowser_StatusTextChanged():: ";
            Tracing.TraceStatus(scope + "Starting.");
            WebBrowserStatusToolStripStatusLabel.Text = this.MainWebBrowser.StatusText;
            Tracing.TraceStatus(scope + "Completed.");
        }

        #endregion

        #region Common Function

        private bool CheckEmptyPhoneNumber()
        {
            if (PhoneNumberToolStripTextBox.Text.Trim() == String.Empty)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string GetFilenameFromFilePath(string FilePath)
        {
            string sResult = "";
            string[] sTemp;

            if (FilePath.Substring(1, 1) != "\\")
            {
                if (System.IO.File.Exists(FilePath) == true)
                {
                    sTemp = FilePath.ToString().Split('\\');
                    if (sTemp.Length > 0)
                    {
                        sResult = sTemp[sTemp.Length - 1];
                    }
                }
            }
            else
            {
                //UNC
                sTemp = FilePath.ToString().Split('\\');
                if (sTemp.Length > 0)
                {
                    sResult = sTemp[sTemp.Length - 1];
                }
            }
            return sResult;
        }

        #endregion

        #region Common ToolStrip Function

        private void TransferPanelToolStripButton_Click(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::TransferPanelToolStripButton_Click():: ";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                switch (this.IsLoggedIntoDialer)
                {
                    case true:
                        MainSplitContainer.Panel2Collapsed = true;
                        break;
                    default:
                        if (MainSplitContainer.Panel2Collapsed == true)
                        {
                            MainSplitContainer.Panel2Collapsed = false;
                        }
                        else
                        {
                            MainSplitContainer.Panel2Collapsed = true;
                        }
                        this.ShowActiveCallInfo();
                        break;
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void CallToolStripSplitButton_ButtonClick(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::CallToolStripButton_ButtonClick()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                this.CallToolStripSplitButton.Enabled = false;
                switch (this.IsLoggedIntoDialer)
                {
                    case true:   //Log On to Dialer Server.
                        Tracing.TraceStatus(scope + "Call button clicked.Not Log On to Dialer Server.");
                        break;
                    default:     // Not Log On to Dialer Server.
                        if (this.CheckEmptyPhoneNumber() != true)
                        {
                            Tracing.TraceStatus(scope + "Call button clicked.Log On to Basic station.");
                            ININ.IceLib.Interactions.CallInteractionParameters callParams = new ININ.IceLib.Interactions.CallInteractionParameters(PhoneNumberToolStripTextBox.Text, CallMadeStage.Allocated);
                            ININ.IceLib.Connection.SessionSettings sessionSetting = this.IC_Session.GetSessionSettings();
                            callParams.AdditionalAttributes.Add("CallerHost", sessionSetting.MachineName.ToString());
                            this.IsManualDialing = true;
                            this.NormalInterationManager.MakeCallAsync(callParams, MakeCallCompleted, null);
                            this.SetCallHistory();
                        }
                        break;
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void MakeCallCompleted(object sender,InteractionCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                //
            }
            else
            {
                this.ActiveNormalInteraction = e.Interaction;
            }
        }

        private void PickupToolStripButton_Click(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::PickupToolStripButton_Click()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                switch (this.IsLoggedIntoDialer)
                {
                    case true:   //Log On to Dialer Server.
                        Tracing.TraceStatus(scope + "Pickup button clicked.Log on to Dialer Server.");
                        if (this.ActiveDialerInteraction != null)
                        {
                            this.ActiveDialerInteraction.Pickup();
                        }
                        if (this.ActiveNormalInteraction != null)
                        {
                            switch (this.ActiveNormalInteraction.InteractionType)
                            {
                                case InteractionType.Email:
                                    //Show Mail form
                                    this.ActiveNormalInteraction.Pickup();
                                    break;
                                case InteractionType.Chat:
                                    break;
                                case InteractionType.Call:
                                    this.ActiveNormalInteraction.Pickup();
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    default:     // Not Log On to Dialer Server.
                        Tracing.TraceStatus(scope + "Pickup button clicked[Basic station].");
                        if (this.ActiveNormalInteraction != null)
                        {
                            switch (this.ActiveNormalInteraction.InteractionType)
                            {
                                case InteractionType.Email:
                                    this.ViewEmailDetail(this.ActiveNormalInteraction);
                                    this.ActiveNormalInteraction.Pickup();
                                    break;
                                case InteractionType.Chat:
                                    break;
                                case InteractionType.Call:
                                    this.ActiveNormalInteraction.Pickup();
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus("Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void ViewEmailDetail(ININ.IceLib.Interactions.Interaction interaction)
        {
            string scope = "CIC::frmMain::ViewEmailDetail()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (this.EmailResponseList != null)
                {
                    if (this.EmailResponseList.Count > 0)
                    {
                        this.EmailDocView = new CIC.frmMailDocView(this.EmailResponseList, interaction);
                        EmailDocView.Show();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus("Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void HoldToolStripButton_Click(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::HoldToolStripButton_Click()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                switch (this.IsLoggedIntoDialer)
                {
                    case true:   //Log On to Dialer Server.
                        Tracing.TraceStatus(scope + "Hold button clicked.Log on to Dialer Server.");
                        if (this.ActiveDialerInteraction != null)
                        {
                            if (this.ActiveDialerInteraction.IsMuted)
                            {
                                this.ActiveDialerInteraction.Mute(false);
                            }
                            this.ActiveDialerInteraction.Hold(!this.ActiveDialerInteraction.IsHeld);
                            this.HoldToolStripButton.Checked = !this.HoldToolStripButton.Checked;
                            if (this.HoldToolStripButton.Checked == true)
                            {
                                this.MuteToolStripButton.Checked = false;
                                this.PickupToolStripButton.Checked = false;
                            }
                            this.IsMuted = this.ActiveDialerInteraction.IsMuted;
                        }
                        break;
                    default:     // Not Log On to Dialer Server.
                        Tracing.TraceStatus(scope + "Hold button clicked.Not log on to Dialer Server.");
                        if (this.ActiveNormalInteraction != null)
                        {
                            if (this.ActiveNormalInteraction.IsMuted)
                            {
                                this.ActiveNormalInteraction.Mute(false);
                            }
                            this.ActiveNormalInteraction.Hold(!this.ActiveNormalInteraction.IsHeld);
                            this.HoldToolStripButton.Checked = !this.HoldToolStripButton.Checked;
                            if (this.HoldToolStripButton.Checked == true)
                            {
                                this.MuteToolStripButton.Checked = false;
                                this.PickupToolStripButton.Checked = false;
                            }
                            this.IsMuted = this.ActiveNormalInteraction.IsMuted;
                        }

                        break;
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void MuteToolStripButton_Click(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::MuteToolStripButton_Click()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                switch (this.IsLoggedIntoDialer)
                {
                    case true:   //Log On to Dialer Server.
                        Tracing.TraceStatus(scope + "Mute button clicked.Log on to Dialer Server.");
                        if (this.ActiveDialerInteraction != null)
                        {
                            if (this.ActiveDialerInteraction.IsHeld)
                            {
                                this.ActiveDialerInteraction.Hold(false);
                            }
                            this.ActiveDialerInteraction.MuteAsync(!this.ActiveDialerInteraction.IsMuted, MuteCompleted, null);
                            if (this.MuteToolStripButton.Checked == true)
                            {
                                this.HoldToolStripButton.Checked = false;
                                this.PickupToolStripButton.Checked = false;
                            }
                        }
                        break;
                    default:     // Not Log On to Dialer Server.
                        if (this.ActiveNormalInteraction != null)
                        {
                            Tracing.TraceStatus(scope + "Mute button clicked.Not log on to Dialer Server.");
                            if (this.ActiveNormalInteraction.IsHeld)
                            {
                                this.ActiveNormalInteraction.Hold(false);
                            }
                            this.ActiveNormalInteraction.MuteAsync(!this.ActiveNormalInteraction.IsMuted, MuteCompleted, null);
                            this.MuteToolStripButton.Checked = !this.MuteToolStripButton.Checked;
                            if (this.MuteToolStripButton.Checked == true)
                            {
                                this.HoldToolStripButton.Checked = false;
                                this.PickupToolStripButton.Checked = false;
                            }
                        }
                        break;
                }
                this.IsMuted = this.MuteToolStripButton.Checked;
                if (this.IsMuted != true)
                {
                    this.UnMuteAll();
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void UnMuteAll()
        {
            string scope = "CIC::frmMain::UnMuteAll()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (this.InteractionList != null)
                {
                    for (int i = 0; i < this.InteractionList.Count; i++)
                    {
                        if (((ININ.IceLib.Interactions.Interaction)this.InteractionList[i]).IsMuted == true)
                        {
                            ((ININ.IceLib.Interactions.Interaction)this.InteractionList[i]).Mute(false);
                        }
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

        private void MuteCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //Reserve
        }

        private void DisconnectToolStripButton_Click(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::DisconnectToolStripButton_Click()::";
            Tracing.TraceStatus(scope + "Starting.");
            ININ.IceLib.Interactions.Interaction TempInteraction = null;
            try
            {
                switch (this.IsLoggedIntoDialer)
                {
                    case true:   //Log On to Dialer Server.
                        if (this.ActiveDialerInteraction == null) //&& (this.ActiveNormalInteraction != null))
                        {
                            if (this.ActiveNormalInteraction == null)
                            {
                                this.DisconnectToolStripButton.Enabled = false;
                            }
                            else
                            {
                                //continue enabled
                            }
                        }
                        else
                        {
                            this.ActiveDialerInteraction.Disconnect();
                            TempInteraction = this.ActiveNormalInteraction;
                            this.ActiveNormalInteraction.Disconnect();
                            this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
                            this.ShowActiveCallInfo();
                            this.CrmScreenPop();
                        }
                        break;
                    default:     // Not Log On to Dialer Server.
                        if (this.ActiveDialerInteraction != null)
                        {
                            this.ActiveDialerInteraction.Disconnect();
                            this.ActiveDialerInteraction = null;
                        }
                        if (this.ActiveNormalInteraction != null)
                        {
                            TempInteraction = this.ActiveNormalInteraction;
                            this.ActiveNormalInteraction.Disconnect();
                            this.RemoveNormalInteractionFromList(TempInteraction);
                            this.ActiveNormalInteraction = this.GetNormalInteractionFromList();
                        }
                        else
                        {
                            this.ActiveNormalInteraction = this.GetNormalInteractionFromList();
                            if (this.ActiveNormalInteraction != null)
                            {
                                this.ActiveNormalInteraction.Disconnect();
                            }
                        }
                        this.DisconnectToolStripButton.Enabled = false;
                        this.ShowActiveCallInfo();
                        if (this.InteractionList.Count <= 0)
                        {
                            this.ActiveConferenceInteraction = null;
                            this.ActiveConsultInteraction = null;
                        }
                        break;
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                if (this.ActiveNormalInteraction != null)
                {
                    this.ActiveNormalInteraction.Disconnect();
                }
                else
                {
                    this.ActiveNormalInteraction = this.GetNormalInteractionFromList();
                    if (this.ActiveNormalInteraction != null)
                    {
                        this.ActiveNormalInteraction.Disconnect();
                    }
                }
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void MakeConsultCallToolStripButton_Click(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::MakeConsultCallToolStripButton_Click()::";
            Tracing.TraceStatus(scope + "Starting.");
            ININ.IceLib.Interactions.CallInteractionParameters callParams = null;
            try
            {
                switch (this.IsLoggedIntoDialer)
                {
                    case true:   //Log On to Dialer Server.  use same normal to call before using dialer object to blind/consult transfer.
                        Tracing.TraceStatus(scope + "Call button clicked. Log On to Dialer Server.");
                        if (this.TransferTxtDestination.Text.Trim() != "")
                        {
                            Tracing.TraceStatus(scope + "Making consult call to " + this.TransferTxtDestination.Text);
                            callParams = new CallInteractionParameters(this.TransferTxtDestination.Text, CallMadeStage.Allocated);
                            if (this.NormalInterationManager != null)
                            {
                                this.NormalInterationManager.ConsultMakeCallAsync(callParams, MakeConsultCompleted, null);
                            }
                        }
                        break;
                    default:     // Not Log On to Dialer Server.
                        Tracing.TraceStatus(scope + "Call button clicked. Not log on to Dialer Server.");
                        if (this.TransferTxtDestination.Text.Trim() != "")
                        {
                            Tracing.TraceStatus(scope + "Making consult call to " + this.TransferTxtDestination.Text);
                            callParams = new CallInteractionParameters(this.TransferTxtDestination.Text, CallMadeStage.Allocated);
                        }
                        else
                        {
                            Tracing.TraceStatus(scope + "Making consult call to " + this.muserStatusGridView.SelectedItem.Extension);
                            if (this.muserStatusGridView.SelectedItem.Extension != null) 
                            {
                                if (this.muserStatusGridView.SelectedItem.Extension.ToString() != "")
                                {
                                    callParams = new CallInteractionParameters(this.muserStatusGridView.SelectedItem.Extension, CallMadeStage.Allocated);
                                }
                            }
                        }
                        if (callParams != null)
                        {
                            this.NormalInterationManager.ConsultMakeCallAsync(callParams,MakeConsultCompleted,null);
                        }
                        break;
                }
                this.EnabledTransferToolStripDisplayed();
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                this.ResetActiveCallInfo();
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void MakeConsultCompleted(object sender, InteractionCompletedEventArgs e)
        {
            this.ActiveConsultInteraction = e.Interaction;
        }

        private void LoginToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::LoginToolStripMenuItem_DropDownOpening()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (this.IsActiveConnection == true)
                {
                    Program.Initialize_dialingManager(this.IC_Session);
                    this.workflows = Program.DialingManager.GetAvailableWorkflows();
                    this.LoginToolStripMenuItem.DropDownItems.Clear();
                    if (workflows.Length > 0)
                    {
                        foreach (string workflow in this.workflows)
                        {
                            ToolStripMenuItem menu = new ToolStripMenuItem((string)workflow, null, WorkflowToolStripMenuItem_Click);
                            menu.Image = global::CIC.Properties.Resources.pin_green;
                            this.LoginToolStripMenuItem.DropDownItems.Add(menu);
                        }
                    }
                    else
                    {
                        ToolStripMenuItem menu = new ToolStripMenuItem("No Workflows Available");
                        menu.Enabled = false;
                        this.LoginToolStripMenuItem.DropDownItems.Add(menu);
                    }
                }
                else
                {
                    this.LoginToolStripMenuItem.DropDownItems.Clear();
                    this.workflows = null;
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (ININ.IceLib.IceLibException ex)
            {
                string output = String.Format("Cannot retrieving available workflows: {0}", ex.Message);
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                MessageBox.Show(output, "CIC Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void WorkflowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::WorkflowToolStripMenuItem_Click()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                this.PhoneNumberToolStripTextBox.Text = "";
                Tracing.TraceStatus(scope + "Logging into workflow. UserId=" + this.IC_Session.UserId + ", StationId=" + this.IC_Session.GetStationInfo().Id);
                this.IcWorkFlow = new CIC.ICWorkFlow(CIC.Program.DialingManager);
                this.DialerSession = IcWorkFlow.LogIn(((ToolStripMenuItem)sender).Text);
                this.IsLoggedIntoDialer = this.IcWorkFlow.LoginResult;
                if (this.IsLoggedIntoDialer == true)
                {
                    this.RegisterHandlers();
                    this.Initial_ActityCodes();
                    this.PlaceCallToolStripButton.Enabled = true;
                    this.SkipCallToolStripButton.Enabled = true;
                    this.CallActivityCodeToolStripComboBox.Enabled = true;
                    this.DispositionToolStripButton.Enabled = true;
                    this.RequestBreakToolStripButton.Enabled = true;
                    this.TransferPanelToolStripButton.Enabled = false;
                    MainSplitContainer.Panel2Collapsed = true;
                    this.imgcmbAgentStatus.Enabled = false;
                    this.LoginToolStripMenuItem.Enabled = false;
                    this.LogoutToolStripMenuItem.Enabled = true;
                    this.RequestBreakToolStripButton.Visible = true;
                    this.exitToolStripMenuItem.Enabled = false;
                    this.CallTypeToolStripStatusLabel.Text = "Campaign Call"; ;
                    this.CampaignIdToolStripStatusLabel.Text = ((ToolStripMenuItem)sender).Text;
                    this.InitializeStatusMessageDetails();
                    Tracing.TraceStatus(scope + "Completed.");
                }
                else
                {
                    this.PlaceCallToolStripButton.Enabled = false;
                    this.SkipCallToolStripButton.Enabled = false;
                    this.CallActivityCodeToolStripComboBox.Enabled = false;
                    this.DispositionToolStripButton.Enabled = false;
                    this.RequestBreakToolStripButton.Enabled = false;
                    this.TransferPanelToolStripButton.Enabled = true;
                    MainSplitContainer.Panel2Collapsed = true;
                    this.imgcmbAgentStatus.Enabled = true;
                    this.LoginToolStripMenuItem.Enabled = false;
                    this.LogoutToolStripMenuItem.Visible = false;
                    this.exitToolStripMenuItem.Enabled = true;
                    this.RequestBreakToolStripButton.Visible = false;
                    this.CallTypeToolStripStatusLabel.Text = "N/A";
                    this.CampaignIdToolStripStatusLabel.Text = "N/A";
                    Tracing.TraceStatus(scope + "WorkFlow [" + ((ToolStripMenuItem)sender).Text + "] logon Fail.Please try again.");
                }
                this.ShowActiveCallInfo();
            }
            catch (System.Exception ex)
            {
                this.PlaceCallToolStripButton.Enabled = false;
                this.SkipCallToolStripButton.Enabled = false;
                this.CallActivityCodeToolStripComboBox.Enabled = false;
                this.DispositionToolStripButton.Enabled = false;
                this.RequestBreakToolStripButton.Enabled = false;
                this.RequestBreakToolStripButton.Visible = false;
                this.imgcmbAgentStatus.Enabled = true;
                this.LoginToolStripMenuItem.Enabled = false;
                this.LogoutToolStripMenuItem.Visible = false;
                this.exitToolStripMenuItem.Enabled = true;
                this.CallTypeToolStripStatusLabel.Text = "N/A";
                this.CampaignIdToolStripStatusLabel.Text = "N/A";
                Tracing.TraceStatus(scope + "Error info.Logon to Workflow[" + ((ToolStripMenuItem)sender).Text + "] : " + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info.Logon to Workflow[" + ((ToolStripMenuItem)sender).Text + "] : " + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void RegisterHandlers()
        {
            string scope = "CIC::MainForm::RegisterHandlers()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                this.DialerSession.PreviewCallAdded += new EventHandler<ININ.IceLib.Dialer.PreviewCallAddedEventArgs>(PreviewCallAdded);
                this.DialerSession.DataPop += new EventHandler<ININ.IceLib.Dialer.DataPopEventArgs>(DataPop);
                this.DialerSession.CampaignTransition += new EventHandler<CampaignTransistionEventArgs>(CampaignTransition);
                this.DialerSession.BreakGranted += new EventHandler(BreakGranted);
                this.DialerSession.LogoutGranted += new EventHandler(LogoutGranted);
                Program.mDialingManager.WorkflowStopped += new EventHandler<WorkflowStoppedEventArgs>(WorkflowStopped);
                Program.mDialingManager.WorkflowStarted += new EventHandler<WorkflowStartedEventArgs>(WorkflowStarted);
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void LogoutGranted(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::LogoutGranted(): ";
            Tracing.TraceStatus(scope + "Starting.");
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
                            this.IcWorkFlow = null;
                            this.DialerSession = null;
                            this.LoginToolStripMenuItem.Enabled = true;
                            this.LogoutToolStripMenuItem.Enabled = false;
                            this.RequestBreakToolStripButton.Visible = false;
                            this.CallToolStripSplitButton.Enabled = true;
                            this.PickupToolStripButton.Enabled = false;
                            this.MuteToolStripButton.Enabled = false;
                            this.HoldToolStripButton.Enabled = false;
                            this.SkipCallToolStripButton.Enabled = false;
                            this.DisconnectToolStripButton.Enabled = false;
                            this.DialpadToolStripDropDownButton.Enabled = true;
                            this.exitToolStripMenuItem.Enabled = true;
                            this.IsLoggedIntoDialer = false;
                            this.imgcmbAgentStatus.Enabled = true;
                            this.InitializeStatusMessageDetails();
                            this.SetToDoNotDisturb_UserStatusMsg();
                            this.CallActivityCodeToolStripComboBox.Items.Clear();
                            this.ShowActiveCallInfo();
                            this.CrmScreenPop();
                            this.TransferPanelToolStripButton.Enabled = true;
                            MainSplitContainer.Panel2Collapsed = true;
                            this.WorkLogoutFlag = false;
                            System.Windows.Forms.MessageBox.Show(global::CIC.Properties.Settings.Default.CompletedWorkflowMsg, "System Info.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        default:
                            if (this.ActiveNormalInteraction != null)
                            {
                                this.ActiveNormalInteraction.Disconnect();
                                this.ActiveNormalInteraction = null;
                            }
                            if (this.IC_Session != null)
                            {
                                this.IC_Session.Disconnect();
                                this.IC_Session = null;
                            }
                            break;
                    }
                    Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        private void BreakGranted(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::BreakGranted(): ";
            Tracing.TraceStatus(scope + "Starting.");
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new EventHandler<EventArgs>(BreakGranted), new object[] { sender, e });
            }
            else
            {
                try
                {
                    switch (this.BreakStatus.Trim())
                    {
                        case "Request Break":
                            this.RequestBreakToolStripButton.Text = "End Break";
                            
                            this.SetToDoNotDisturb_UserStatusMsg();
                            if (this.WorkLogoutFlag != true)
                            {
                                // Shiw Break Status Message.
                                // System.Windows.Forms.MessageBox.Show(global::CIC.Properties.Settings.Default.CompletedBreak, "System Info.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            break;
                        case "End Break":
                            this.RequestBreakToolStripButton.Text = "Request Break";
                            break;
                        default:
                            this.RequestBreakToolStripButton.Text = "Request Break";
                            break;
                    }
                    if (this.RequestBreakToolStripButton.Text.Trim() == "Break Pending")
                    {
                        this.RequestBreakToolStripButton.Enabled = false;
                    }
                    else
                    {
                        this.RequestBreakToolStripButton.Enabled = true;
                    }
                    Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        private void SetToAvailable_UserStatusMsg()
        {
            ININ.IceLib.People.UserStatusUpdate statusUpdate = null;
            string scope = "CIC::MainForm::SetToAvailable_UserStatusMsg(): ";
            try
            {
            if (this.AvailableStatusMessageDetails != null)
            {
                if (this.mPeopleManager != null)
                {
                    statusUpdate = new UserStatusUpdate(this.mPeopleManager);
                    statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                    statusUpdate.UpdateRequest();
                }
            }
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void SetToDoNotDisturb_UserStatusMsg()
        {
            string scope = "CIC::MainForm::SetToDoNotDisturb_UserStatusMsg(): ";
            ININ.IceLib.People.UserStatusUpdate statusUpdate = null;
            try
            {
                Tracing.TraceStatus(scope + "Starting.");
                if (this.DoNotDisturbStatusMessageDetails != null)
                {
                    if (this.mPeopleManager != null)
                    {
                        statusUpdate = new UserStatusUpdate(this.mPeopleManager);
                        statusUpdate.StatusMessageDetails = this.DoNotDisturbStatusMessageDetails;
                        statusUpdate.UpdateRequest();
                    }
                }

                Tracing.TraceStatus(scope + "completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void CampaignTransition(object sender, CampaignTransistionEventArgs e)
        {
            //CampaignTransition
        }

        private void InitialContactData()
        {
            string scope = "CIC::MainForm::InitialContactData()::";
            Tracing.TraceStatus(scope + "Starting.");
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
                    i++;
                    this.mDialerData.Add(pair.Key.ToString().Trim(), pair.Value);
                    System.Data.DataRow dr = dt.NewRow();
                    dr["id"] = i.ToString();
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
            Tracing.TraceStatus(scope + "Completed.");
        }

        private string GetFullyScreenUrl()
        {
            string scope = "CIC::MainForm::GetFullyScreenUrl()::";
            Tracing.TraceStatus(scope + "Starting.");
            string sRet = "";
            string[] QueryATTRValueList = Properties.Settings.Default.ScreenPop_FieldList_ATTR.Split(';');
            string[] sTemp;
            int i = 0;
            try
            {
                if (this.mDialerData == null)
                {
                    if (Properties.Settings.Default.StartupUrl != null)
                    {
                        if (Properties.Settings.Default.AutoCRMSignOn == true)
                        {
                            sRet = System.String.Format(Properties.Settings.Default.StartupUrl.ToString(), global::CIC.Program.mLoginParam.UserId, global::CIC.Program.mLoginParam.Password);
                        }
                        else
                        {
                            sRet = Properties.Settings.Default.StartupUrl.ToString();
                        }
                    }
                }
                else
                {
                    sTemp = new string[QueryATTRValueList.Length];
                    for (i = 0; i < QueryATTRValueList.Length; i++)
                    {
                        sTemp[i] = this.mDialerData[QueryATTRValueList[i]];

                    }
                    sRet = System.String.Format(Properties.Settings.Default.PopUrl.ToString(), sTemp);
                }
            }
            catch (System.Exception ex)
            {
                sRet = Properties.Settings.Default.StartupUrl.ToString();
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
            }
            return sRet;
        }

        private void Initial_CallBack()
        {
            string scope = "CIC::MainForm::Initial_CallBack()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                switch (this.IsLoggedIntoDialer)
                {
                    case true:
                        if (this.ActiveDialerInteraction != null)
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
                        break;
                    default:
                        break;
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void CallBackInteraction_AttributesChanged(object sender, AttributesEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<AttributesEventArgs>(CallBackInteraction_AttributesChanged), new object[] { sender, e });
            }
            else
            {
                //
            }
        }

        private void CallBackInteration_StartWatchingCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string scope = "CIC::MainForm::CallBackInteration_StartWatchingCompleted()::";
            Tracing.TraceStatus(scope + "Starting.");
            if (e.Error != null)
            {
                Tracing.TraceStatus(scope + "Error info : " + e.Error.Message);
            }
            else
            {
                try
                {
                    if (this.ActiveDialerInteraction.WorkgroupQueueName.Length > 0)
                    {
                        this.ActiveWorkgroupDetails = new WorkgroupDetails(this.mPeopleManager, this.ActiveDialerInteraction.WorkgroupQueueName);
                        this.ActiveWorkgroupDetails.WatchedAttributesChanged += new EventHandler<WatchedAttributesEventArgs>(WorkGroup_WatchedAttributesChanged);
                        List<string> mWorkgroupDetailsAttributeNames = new List<string>();
                        mWorkgroupDetailsAttributeNames.Add(WorkgroupAttributes.WrapUpCodes);
                        mWorkgroupDetailsAttributeNames.Add(WorkgroupAttributes.HasMailbox);
                        mWorkgroupDetailsAttributeNames.Add(WorkgroupAttributes.HasQueue);
                        mWorkgroupDetailsAttributeNames.Add(WorkgroupAttributes.Extension);
                        mWorkgroupDetailsAttributeNames.Add(WorkgroupAttributes.ActiveMembers);
                        mWorkgroupDetailsAttributeNames.Add(WorkgroupAttributes.IsActive);
                        mWorkgroupDetailsAttributeNames.Add(WorkgroupAttributes.Members);
                        mWorkgroupDetailsAttributeNames.Add(WorkgroupAttributes.ActiveMembers);
                        mWorkgroupDetailsAttributeNames.Add(WorkgroupAttributes.Supervisors);
                        mWorkgroupDetailsAttributeNames.Add(WorkgroupAttributes.WrapUpClientTimeout);
                        this.ActiveWorkgroupDetails.StartWatchingAsync(mWorkgroupDetailsAttributeNames.ToArray(), WorkgroupDetailsStartWatchingComplete, null);
                    }
                    else
                    {
                        //wrapupCodesLabel.Visible = false;
                        //wrapupCodesComboBox.Visible = false;
                    }
                    Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        private void WorkgroupDetailsStartWatchingComplete(object sender, AsyncCompletedEventArgs e)
        {
            //
        }

        private void WorkGroup_WatchedAttributesChanged(Object sender, ININ.IceLib.People.WatchedAttributesEventArgs e)
        {
            //
        }

        private void DialerInteraction_AttributesChanged(object sender, AttributesEventArgs e)
        {
            string scope = "CIC::MainForm::DialerInteraction_AttributesChanged():: ";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (this.ActiveDialerInteraction != null)
                {
                    this.StrConnectionState = this.ActiveDialerInteraction.State.ToString();
                }
                else
                {
                    this.StrConnectionState = "None";
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch
            {
                //Get Connection State.
            }
        }

        private void PreviewCallAdded(object sender, ININ.IceLib.Dialer.PreviewCallAddedEventArgs e)
        {
            string scope = "CIC::MainForm::PreviewCallAdded()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (e.Interaction.IsWatching() != true)
                {
                    e.Interaction.AttributesChanged += new EventHandler<AttributesEventArgs>(DialerInteraction_AttributesChanged);
                    e.Interaction.StartWatching(this.InteractionAttributes);
                }
                this.ActiveDialerInteraction = e.Interaction;
                switch(e.Interaction.InteractionType)
                {
                    case InteractionType.Email:
                        break;
                    case InteractionType.Chat:
                        break;
                    case InteractionType.Callback:
                        this.Initial_CallBack();
                        this.InitialContactData();
                        this.ShowActiveCallInfo();
                        this.CrmScreenPop();
                        break;
                    case InteractionType.Call:
                        this.InitialContactData();
                        this.ShowActiveCallInfo();
                        this.CrmScreenPop();
                        break;
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void DataPop(object sender, ININ.IceLib.Dialer.DataPopEventArgs e)
        {
            string scope = "CIC::MainForm::DataPop()::";
            Tracing.TraceStatus(scope + "Starting.");
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
                        this.Initial_CallBack();
                        this.InitialContactData();
                        this.ShowActiveCallInfo();
                        this.CrmScreenPop();
                        break;
                    case InteractionType.Call:
                        this.InitialContactData();
                        this.ShowActiveCallInfo();
                        this.CrmScreenPop();
                        break;
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void WorkflowStarted(object sender, WorkflowStartedEventArgs args)
        {
            string scope = "CIC::MainForm::WorkflowStarted()::";
            Tracing.TraceStatus(scope + "Starting.");

            WorkLogoutFlag = false;
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void WorkflowStopped(object sender, WorkflowStoppedEventArgs args)
        {
            string scope = "CIC::MainForm::WorkflowStopped()::";
            Tracing.TraceStatus(scope + "Starting.");
            this.TransferPanelToolStripButton.Enabled = true;
            this.RequestBreakToolStripButton.Visible = false;
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void PlaceCallToolStripButton_Click(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::PlaceCallToolStripButton_Click(): ";
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new EventHandler<EventArgs>(PlaceCallToolStripButton_Click), new object[] { sender, e });
            }
            else
            {
                try
                {
                    Tracing.TraceStatus(scope + "Starting.[Place Call]");
                    if (this.ActiveDialerInteraction != null)
                    {
                        this.ActiveDialerInteraction.PlacePreviewCall();
                    }
                    this.DispositionToolStripButton.Enabled = true;
                    this.CallActivityCodeToolStripComboBox.Enabled = true;
                    this.PlaceCallToolStripButton.Enabled = false;
                    this.SkipCallToolStripButton.Enabled = false;
                    this.CallToolStripSplitButton.Enabled = false;
                    this.PickupToolStripButton.Enabled = false;
                    this.MuteToolStripButton.Enabled = false;
                    this.HoldToolStripButton.Enabled = false;
                    this.DisconnectToolStripButton.Enabled = false;
                    this.DialpadToolStripDropDownButton.Enabled = false;
                    Tracing.TraceStatus(scope + "Completed.[Place Call]");
                }
                catch (System.Exception ex)
                {
                    Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        private void SkipCallToolStripButton_Click(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::SkipCallToolStripButton_Click(): ";
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new EventHandler<EventArgs>(SkipCallToolStripButton_Click), new object[] { sender, e });
            }
            else
            {
                try
                {
                    Tracing.TraceStatus(scope + "Starting.[Skip Call]");
                    UserStatusUpdate statusUpdate = new UserStatusUpdate(this.mPeopleManager);
                    if (this.ActiveDialerInteraction != null)
                    {
                        CallCompletionParameters callCompletionParameters = new CallCompletionParameters(ReasonCode.Skipped, "Skipped");
                        this.ActiveDialerInteraction.CallComplete(callCompletionParameters);
                        if (this.AvailableStatusMessageDetails != null)
                        {
                            this.userManualStatusChangeFlag = true;
                        }
                    }
                    if (this.AvailableStatusMessageDetails != null)
                    {
                        statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                        statusUpdate.UpdateRequest();

                        this.imgcmbAgentStatus.SetMessage(this.AvailableStatusMessageDetails.MessageText);  //Set Available status for a new call.
                    }
                    Tracing.TraceStatus(scope + "Completed..[Skip Call]");
                }
                catch (System.Exception ex)
                {
                    Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
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

        private void DispositionToolStripButton_Click(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::DispositionToolStripButton_Click(): ";
            string sFinishcode = "";
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new EventHandler<EventArgs>(DispositionToolStripButton_Click), new object[] { sender, e });
            }
            else
            {
                Tracing.TraceStatus(scope + "Starting.[Disposition]");
                try
                {
                    switch (this.IsLoggedIntoDialer)
                    {
                        case true:
                            if (this.DispositionToolStripButton.Enabled == true)
                            {
                                if (this.ActiveDialerInteraction != null)
                                {
                                    ININ.IceLib.People.UserStatusUpdate statusUpdate = new UserStatusUpdate(this.mPeopleManager);
                                    if (this.ActiveDialerInteraction.DialingMode == DialingMode.Preview)
                                    {
                                        if (this.CallActivityCodeToolStripComboBox.SelectedIndex < 0)
                                        {
                                            sFinishcode = this.CallActivityCodeToolStripComboBox.Items[0].ToString();
                                        }
                                        else
                                        {
                                           sFinishcode = this.CallActivityCodeToolStripComboBox.SelectedItem.ToString();
                                        }
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
                                        if (this.BreakStatus.Trim() != "End Break")
                                        {
                                            if (this.AvailableStatusMessageDetails != null)
                                            {
                                                this.userManualStatusChangeFlag = true;
                                                statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                                                statusUpdate.UpdateRequest();
                                                this.imgcmbAgentStatus.SetMessage(this.AvailableStatusMessageDetails.MessageText);  //Set Available status for a new call.
                                            }
                                        }
                                        else
                                        {
                                            //this.SetToDoNotDisturb_UserStatusMsg();
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
                                    else if (this.ActiveDialerInteraction.DialingMode == DialingMode.OwnAgentCallback)
                                    {
                                        //
                                    }
                                    else if (this.ActiveDialerInteraction.DialingMode == DialingMode.OwnAgentCallback_Preview)
                                    {
                                        //
                                    }
                                    else if (this.ActiveDialerInteraction.DialingMode == DialingMode.Regular)
                                    {
                                        if (this.CallActivityCodeToolStripComboBox.SelectedIndex < 0)
                                        {
                                            sFinishcode = this.CallActivityCodeToolStripComboBox.Items[0].ToString();
                                        }
                                        else
                                        {
                                            sFinishcode = this.CallActivityCodeToolStripComboBox.SelectedItem.ToString();
                                        } 
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
                                            this.ActiveDialerInteraction.CallComplete(new ININ.IceLib.Dialer.CallCompletionParameters(sReasoncode, sFinishcode, this.CallBackDateTime, this.ScheduleAgent, false));
                                        }
                                        else
                                        {
                                            this.ActiveDialerInteraction.CallComplete(callCompletionParameters);
                                        }
                                        if (this.BreakStatus.Trim() != "End Break")
                                        {
                                            if (this.AvailableStatusMessageDetails != null)
                                            {
                                                this.userManualStatusChangeFlag = true;
                                                statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                                                statusUpdate.UpdateRequest();
                                                this.imgcmbAgentStatus.SetMessage(this.AvailableStatusMessageDetails.MessageText);  //Set Available status for a new call.
                                            }
                                        }
                                        else
                                        {
                                            //this.SetToDoNotDisturb_UserStatusMsg();
                                        }
                                    }
                                    else
                                    {
                                        //Other Mode!
                                    }
                                }
                            }
                            break;
                        default:
                            //Activity codes use in campaign mode only! This feature was disabled in normal mode.
                            break;
                    }
                    this.DispositionToolStripButton.Enabled = false;
                    this.CallActivityCodeToolStripComboBox.Enabled = false;
                    this.PlaceCallToolStripButton.Enabled = false;
                    this.SkipCallToolStripButton.Enabled = false;
                    this.CallToolStripSplitButton.Enabled = false;
                    this.PickupToolStripButton.Enabled = false;
                    this.MuteToolStripButton.Enabled = false;
                    this.HoldToolStripButton.Enabled = false;
                    this.DisconnectToolStripButton.Enabled = false;
                    this.DialpadToolStripDropDownButton.Enabled = false;
                    //this.CallActivityCodeToolStripComboBox.SelectedIndex = -1;  //Reset Reason Code.
                    Tracing.TraceStatus(scope + "Completed.[Disposition]");
                }
                catch (ININ.IceLib.IceLibException ex)
                {
                    Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
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

        private void RequestBreakToolStripButton_Click(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::RequestBreakToolStripButton_Click(): ";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (this.ActiveDialerInteraction == null)
                {
                    this.BreakStatus = "End Break";
                }
                else
                {
                    switch (this.RequestBreakToolStripButton.Text.Trim())
                    {
                        case "Request Break":
                            this.ActiveDialerInteraction.DialerSession.RequestBreak();
                            this.BreakStatus = "Request Break";
                            this.RequestBreakToolStripButton.Enabled = false;
                            this.RequestBreakToolStripButton.Text = "Break Pending";
                            if (this.WorkLogoutFlag == true)
                            {
                                System.Windows.Forms.MessageBox.Show(global::CIC.Properties.Settings.Default.IncompletedCall, "Error Info.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            break;
                        case "End Break":
                            this.SetToAvailable_UserStatusMsg();
                            this.ActiveDialerInteraction.DialerSession.EndBreak();
                            this.BreakStatus = "End Break";
                            this.RequestBreakToolStripButton.Enabled = true;
                            this.RequestBreakToolStripButton.Text = "Request Break";
                            break;
                        default:
                            this.RequestBreakToolStripButton.Enabled = false;
                            this.RequestBreakToolStripButton.Text = "Break Pending";
                            if (this.WorkLogoutFlag == true)
                            {
                                System.Windows.Forms.MessageBox.Show(global::CIC.Properties.Settings.Default.IncompletedCall, "Error Info.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            } 
                            break;
                    }
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                this.RequestBreakToolStripButton.Enabled = false;
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void mnuCamera_Click(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::mnuCamera_Click(): ";
            Tracing.TraceStatus(scope + "Starting.");
            string sipAddress = "";
            if (global::CIC.Properties.Settings.Default.CamEnable == true)
            {
                if (this.Camera == null)
                {
                    Camera = new CIC.frmCamera();
                    if (this.ActiveNormalInteraction != null)
                    {
                        if (this.ActiveNormalInteraction.IsConnected == true)
                        {
                            ININ.IceLib.Connection.SessionSettings session_setting = this.ActiveNormalInteraction.InteractionsManager.Session.GetSessionSettings();
                            sipAddress = this.ActiveNormalInteraction.GetStringAttribute("Eic_contactAddress");
                            if (sipAddress.Trim() != "")
                            {
                                Camera.RemoteHostIP = sipAddress;
                            }
                            else
                            {
                                Camera.RemoteHostIP = session_setting.MachineName;
                            }
                        }
                    }
                }
                Camera.Show();
            }
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::exitToolStripMenuItem_Click(): ";
            Tracing.TraceStatus(scope + "Starting.");
            Tracing.TraceStatus(scope + "Completed.");
            Application.Exit();
        }

        private void GoToolStripButton_Click(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::GoToolStripButton_Click(): ";
            Tracing.TraceStatus(scope + "Starting.");
            if (this.UrlToolStripTextBox.Text.Trim() != String.Empty)
            {
                try
                {
                    if (this.UrlToolStripTextBox.Text.Trim().Length >= 7)
                    {
                        if (this.UrlToolStripTextBox.Text.ToLower().Trim().Substring(0, 8) != "https://")
                        {
                            if (this.UrlToolStripTextBox.Text.ToLower().Trim().Substring(0, 7) != "ftp://")
                            {

                                if (this.UrlToolStripTextBox.Text.ToLower().Trim().Substring(0, 7) != "http://")
                                {
                                    if (this.UrlToolStripTextBox.Text.ToLower().Trim().Substring(0, 2) != "\\\\")
                                    {
                                        if (this.UrlToolStripTextBox.Text.ToLower().Trim().Substring(0, 7) != "file://")
                                        {
                                            this.UrlToolStripTextBox.Text = "http://" + this.UrlToolStripTextBox.Text.Trim();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    this.MainWebBrowser.Url = new System.Uri(UrlToolStripTextBox.Text, System.UriKind.Absolute);
                    Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void RefreshToolStripButton_Click(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::RefreshToolStripButton_Click(): ";
            Tracing.TraceStatus(scope + "Starting.");
            this.MainWebBrowser.Refresh();
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void GoBackToolStripButton_Click(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::GoBackToolStripButton_Click(): ";
            Tracing.TraceStatus(scope + "Starting.");
            if (this.MainWebBrowser.CanGoBack == true)
            {
                this.MainWebBrowser.GoBack();
            }
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void GoForwardToolStripButton_Click(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::GoForwardToolStripButton_Click(): ";
            Tracing.TraceStatus(scope + "Starting.");
            if (this.MainWebBrowser.CanGoForward == true)
            {
                this.MainWebBrowser.GoForward();
            }
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void StopToolStripButton_Click(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::StopToolStripButton_Click(): ";
            Tracing.TraceStatus(scope + "Starting.");
            this.MainWebBrowser.Stop();
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void PhoneNumberToolStripTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            string scope = "CIC::frmMain::PhoneNumberToolStripTextBox_KeyDown()::";
            Tracing.TraceStatus(scope + "Starting.");
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (this.PhoneNumberToolStripTextBox.Text.Trim() != String.Empty)
                    {
                        CallToolStripSplitButton_ButtonClick(sender, e);
                    }
                    break;
                default:
                    break;
            }
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void imgcmbAgentStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new EventHandler<EventArgs>(imgcmbAgentStatus_SelectedIndexChanged), new object[] { sender, e });
            }
            else
            {
                int i = 0;
                string scope = "CIC::frmMain::imgcmbAgentStatus_SelectedIndexChanged()::";
                Tracing.TraceStatus(scope + "Starting.");
                try
                {
                    UserStatusUpdate statusUpdate = new UserStatusUpdate(this.mPeopleManager);
                    if (this.imgcmbAgentStatus.Items.Count > 0)
                    {
                        string CurrentDisplayMessageText = this.imgcmbAgentStatus.GetMeaasageText(this.imgcmbAgentStatus.SelectedIndex);
                        switch (this.IsLoggedIntoDialer)
                        {
                            case true:
                                foreach (StatusMessageDetails status in this.AllStatusMessageList.GetList())
                                {
                                    if (status.MessageText.Equals(CurrentDisplayMessageText) == true)
                                    {
                                        if (this.CurrentUserStatus.StatusMessageDetails.MessageText.Trim() != CurrentDisplayMessageText.Trim())
                                        {
                                            statusUpdate.StatusMessageDetails = status;
                                            statusUpdate.UpdateRequest();
                                            this.userManualStatusChangeFlag = false;
                                        }
                                        break;
                                    }
                                    i++;
                                }
                                break;
                            default:
                                foreach (StatusMessageDetails status in this.AllStatusMessageList.GetList())
                                {
                                    if (status.IsSelectableStatus == true)
                                    {
                                        if (status.MessageText.Equals(CurrentDisplayMessageText) == true)
                                        {
                                            if (this.userManualStatusChangeFlag == true)
                                            {
                                                if (this.CurrentUserStatus.StatusMessageDetails.MessageText.Trim() != CurrentDisplayMessageText.Trim())
                                                {
                                                    statusUpdate.StatusMessageDetails = status;
                                                    statusUpdate.UpdateRequest();
                                                }
                                            }
                                            break;
                                        }
                                    }
                                    i++;
                                }
                                break;
                        }
                        this.userManualStatusChangeFlag = false;
                    }
                    Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception eMsg)
                {
                    this.userManualStatusChangeFlag = false;
                    Tracing.TraceStatus(scope + "Error info." + eMsg.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + eMsg.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        private void imgcmbAgentStatus_MouseClick(object sender, MouseEventArgs e)
        {
            string scope = "CIC::frmMain::imgcmbAgentStatus_MouseClick()::";
            Tracing.TraceStatus(scope + "Starting.");
            this.userManualStatusChangeFlag = true;
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void UrlToolStripTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            string scope = "CIC::frmMain::imgcmbAgentStatus_MouseClick()::";
            Tracing.TraceStatus(scope + "Starting.");
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    this.GoToolStripButton_Click(sender, e);
                    break;
                default:
                    break;
            }
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void CallToolStripSplitButton_Click(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::LogoutToolStripMenuItem_Click(): ";
            Tracing.TraceStatus(scope + "Starting.");
            KeyEventArgs keyEnter = new KeyEventArgs(Keys.Enter);
            this.PhoneNumberToolStripTextBox_KeyDown(this.PhoneNumberToolStripTextBox, keyEnter);
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void LogoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::LogoutToolStripMenuItem_Click(): ";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                switch (this.IsLoggedIntoDialer)
                {
                    case true:
                        if (this.CallStateToolStripStatusLabel.Text.ToLower().Trim() == "n/a")
                        {
                            this.LogoutGranted(sender, e);      //No call object from this campaign;permit to logging out.
                        }
                        else
                        {
                            if (this.ActiveDialerInteraction != null)
                            {
                                if (this.RequestBreakToolStripButton.Text.Trim() != "End Break")
                                {
                                    this.WorkLogoutFlag = true;      
                                    this.RequestBreakToolStripButton_Click(sender, e);               //wait for breakgrant
                                    this.ActiveDialerInteraction.DialerSession.RequestLogout();
                                }
                                else
                                {
                                    this.LogoutGranted(sender, e);     //already breakp;ermit to logging out.
                                }
                            }
                        }
                        break;
                    default:
                        if (this.ActiveNormalInteraction != null)
                        {
                            this.ActiveNormalInteraction.Disconnect();
                            this.ActiveNormalInteraction = null;
                        }
                        if (this.IC_Session != null)
                        {
                            this.IC_Session.Disconnect();
                            this.IC_Session = null;
                        }
                        break;
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void fullScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::fullScreenToolStripMenuItem_Click():: ";
            Tracing.TraceStatus(scope + "Starting.");
            if (this.FormBorderStyle != FormBorderStyle.None)
            {
                this.toolStripContainer1.BottomToolStripPanelVisible = false;
                this.FormBorderStyle = FormBorderStyle.None;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.toolStripContainer1.BottomToolStripPanelVisible = true;
            }

            //Refresh Screen Size.
            this.WindowState = FormWindowState.Normal;
            this.WindowState = FormWindowState.Maximized;
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void TransferPanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.TransferPanelToolStripButton_Click(sender, e);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox.ShowDialog();
        }

        private void TransferNowToolStripButton_Click(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::TransferNowToolStripButton_Click()::";
            Tracing.TraceStatus(scope + "Starting.");
            this.BlindTransferFlag = false;
            try
            {
                switch (this.IsLoggedIntoDialer)
                {
                    case true: //Log On to IC-Dialer.
                        if (this.ActiveNormalInteraction != null)
                        {
                            if (this.ActiveDialerInteraction != null)
                            {
                                if (this.ActiveConsultInteraction != null)
                                {
                                    Tracing.TraceNote(scope + "Performing consult transfer");
                                    this.ActiveNormalInteraction.ConsultTransferAsync(this.ActiveConsultInteraction.InteractionId, TransferCompleted, null);
                                }
                                else
                                {
                                    if (this.TransferTxtDestination.Text.Trim() != "")
                                    {
                                        if (this.ActiveNormalInteraction != null)
                                        {
                                            this.ActiveNormalInteraction.BlindTransfer(this.TransferTxtDestination.Text);
                                        }
                                        Tracing.TraceNote(scope + "Performing blind transfer");
                                    }
                                }
                                ININ.IceLib.People.UserStatusUpdate statusUpdate = new UserStatusUpdate(this.mPeopleManager);
                                string sFinishcode = global::CIC.Properties.Settings.Default.ReasonCode_Transfereded;
                                ININ.IceLib.Dialer.ReasonCode sReasoncode = ININ.IceLib.Dialer.ReasonCode.Transferred;
                                CallCompletionParameters callCompletionParameters = new CallCompletionParameters(sReasoncode, sFinishcode);
                                this.ActiveDialerInteraction.CallComplete(callCompletionParameters);
                                statusUpdate.StatusMessageDetails = this.AvailableStatusMessageDetails;
                                statusUpdate.UpdateRequest();
                                this.imgcmbAgentStatus.SetMessage(this.AvailableStatusMessageDetails.MessageText);  //Set Available status for a new call.
                            }
                        }
                        break;
                    default:
                        if (this.ActiveNormalInteraction != null)
                        {
                            if (this.ActiveConsultInteraction != null)
                            {
                                Tracing.TraceNote(scope + "Performing consult transfer");
                                if (this.ActiveConsultInteraction.InteractionId != this.ActiveNormalInteraction.InteractionId)
                                {
                                    this.ActiveNormalInteraction.ConsultTransferAsync(this.ActiveConsultInteraction.InteractionId, TransferCompleted, null);
                                    this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
                                    this.RemoveNormalInteractionFromList(this.ActiveConsultInteraction);
                                    this.BlindTransferFlag = true;
                                }
                                else
                                {
                                    this.ActiveConsultInteraction = null;
                                    if (this.InteractionList != null)
                                    {
                                        if (this.InteractionList.Count > 0)
                                        {
                                            foreach (ININ.IceLib.Interactions.Interaction CurrentInteraction in this.InteractionList)
                                            {
                                                if (CurrentInteraction.IsDisconnected != true)
                                                {
                                                    if (CurrentInteraction.InteractionId != this.ActiveNormalInteraction.InteractionId)
                                                    {
                                                        this.ActiveConsultInteraction = CurrentInteraction;  //Find Consult Call
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    //
                                                }
                                            }
                                            if (this.ActiveConsultInteraction != null)
                                            {
                                                this.ActiveNormalInteraction.ConsultTransferAsync(this.ActiveConsultInteraction.InteractionId, TransferCompleted, null);
                                                this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
                                                this.RemoveNormalInteractionFromList(this.ActiveConsultInteraction);
                                                this.BlindTransferFlag = true;
                                            }
                                        }
                                        else
                                        {
                                            if (this.TransferTxtDestination.Text.ToString().Trim() != "")
                                            {
                                                this.ActiveNormalInteraction.BlindTransfer(this.TransferTxtDestination.Text);
                                                this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
                                            }
                                            else
                                            {
                                                if (this.muserStatusGridView != null)
                                                {
                                                    if (this.muserStatusGridView.SelectedItem != null)
                                                    {
                                                        if (this.muserStatusGridView.SelectedItem.Extension != null)
                                                        {
                                                            if (this.muserStatusGridView.SelectedItem.Extension.ToString() != "")
                                                            {
                                                                this.ActiveNormalInteraction.BlindTransfer(this.muserStatusGridView.SelectedItem.Extension);
                                                                this.TransferTxtDestination.Text = this.muserStatusGridView.SelectedItem.Extension;
                                                                this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Tracing.TraceNote(scope + "Performing blind transfer");
                                if (this.TransferTxtDestination.Text.Trim() != "")
                                {
                                    this.ActiveNormalInteraction.BlindTransfer(this.TransferTxtDestination.Text.Trim());
                                    this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
                                }
                                else
                                {
                                    if (this.muserStatusGridView != null)
                                    {
                                        if (this.muserStatusGridView.SelectedItem != null)
                                        {

                                            if (this.muserStatusGridView.SelectedItem.Extension != null)
                                            {
                                                if (this.muserStatusGridView.SelectedItem.Extension.ToString() != "")
                                                {
                                                    this.ActiveNormalInteraction.BlindTransfer(this.muserStatusGridView.SelectedItem.Extension);
                                                    this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }
                this.ResetActiveCallInfo();
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                this.ResetActiveCallInfo();
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void TransferCompleted(Object sender, AsyncCompletedEventArgs e)
        {
            this.BlindTransferFlag = true;
            this.ShowActiveCallInfo();
            this.BlindTransferFlag = false;
            this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
            this.toolStripButtonClear_Click(sender, new System.EventArgs()); 
        }

        private void EnabledTransferToolStripDisplayed()
        {
            string scope = "CIC::frmMain::EnabledTransferToolStripDisplayed()::";
            Tracing.TraceStatus(scope + "Starting.");
            bool ConsultCallConected = true;
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new MethodInvoker(EnabledTransferToolStripDisplayed));
            }
            else
            {
                try
                {
                    if (this.InteractionList != null)
                    {
                        if (this.InteractionList.Count <= 0)
                        {
                            this.MakeConsultCallToolStripButton.Enabled = false;
                            this.TransferNowToolStripButton.Enabled = false;
                            this.SpeakToCallerToolStripButton.Enabled = false;
                            this.CancelTransferToolStripButton.Enabled = false;
                        }
                        else if (this.InteractionList.Count == 1)
                        {
                            if (this.TransferTxtDestination.Text.Trim() != "")
                            {
                                if (this.IsActiveConference_flag == true)
                                {
                                    this.MakeConsultCallToolStripButton.Enabled = false;
                                    this.TransferNowToolStripButton.Enabled = false;
                                }
                                else
                                {
                                    this.MakeConsultCallToolStripButton.Enabled = true;
                                    this.TransferNowToolStripButton.Enabled = true;
                                }
                            }
                            else
                            {
                                this.MakeConsultCallToolStripButton.Enabled = false;
                                this.TransferNowToolStripButton.Enabled = false;

                            }
                            this.SpeakToCallerToolStripButton.Enabled = false;
                            this.CancelTransferToolStripButton.Enabled = false;
                        }
                        else
                        {
                            for (int i = 0; i < this.InteractionList.Count; i++)
                            {
                                if (((ININ.IceLib.Interactions.Interaction)this.InteractionList[i]) != null)
                                {
                                    if (((ININ.IceLib.Interactions.Interaction)this.InteractionList[i]).State != InteractionState.Connected)
                                    {
                                        if (((ININ.IceLib.Interactions.Interaction)this.InteractionList[i]).State != InteractionState.Held)
                                        {
                                            ConsultCallConected = false;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (ConsultCallConected == true)
                            {
                                this.MakeConsultCallToolStripButton.Enabled = false;
                                this.SpeakToCallerToolStripButton.Enabled = true;
                                this.CancelTransferToolStripButton.Enabled = true;
                            }
                            else
                            {
                                //if (this.TransferTxtDestination.Text.Trim() != "")
                                //{
                                //    this.MakeConsultCallToolStripButton.Enabled = false;
                                //}
                                //else
                                //{
                                //    this.MakeConsultCallToolStripButton.Enabled = false;
                                //}
                                this.MakeConsultCallToolStripButton.Enabled = false;
                                this.TransferNowToolStripButton.Enabled = false;
                                this.SpeakToCallerToolStripButton.Enabled = false;
                                this.CancelTransferToolStripButton.Enabled = true;
                            }
                            if (this.TransferTxtDestination.Text.Trim() != "")
                            {
                                this.TransferNowToolStripButton.Enabled = true;
                            }
                            else
                            {
                                this.TransferNowToolStripButton.Enabled = false;
                            }
                        }
                    }
                    Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    this.MakeConsultCallToolStripButton.Enabled = false;
                    this.TransferNowToolStripButton.Enabled = false;
                    this.SpeakToCallerToolStripButton.Enabled = false;
                    this.CancelTransferToolStripButton.Enabled = false;
                    Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        private void SpeakToCallerToolStripButton_Click(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::SpeakToCallerToolStripButton_Click()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                switch (this.IsLoggedIntoDialer)
                {
                    case true: //Log On to IC-Dialer.
                        if (this.ActiveDialerInteraction != null)
                        {
                            if (this.ActiveNormalInteraction != null)
                            {
                                if (this.InteractionList != null)
                                {
                                    if (this.InteractionList.Count > 0)
                                    {
                                        foreach (ININ.IceLib.Interactions.Interaction CurrentInteraction in this.InteractionList)
                                        {
                                            if (CurrentInteraction.IsHeld == true)
                                            {
                                                this.ActiveNormalInteraction = CurrentInteraction;
                                                Tracing.TraceNote(scope + "Picking up active call");
                                                this.SwapPartyFlag = true;
                                                this.ActiveNormalInteraction.PickupAsync(Pickup_Completed, null);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        if (this.ActiveNormalInteraction != null)
                        {
                            if (this.InteractionList != null)
                            {
                                if (this.InteractionList.Count > 0)
                                {
                                    foreach (ININ.IceLib.Interactions.Interaction CurrentInteraction in this.InteractionList)
                                    {
                                        if (CurrentInteraction.IsHeld == true)
                                        {
                                            this.ActiveNormalInteraction = CurrentInteraction;
                                            Tracing.TraceNote(scope + "Picking up active call");
                                            this.SwapPartyFlag = true;
                                            this.ActiveNormalInteraction.PickupAsync(Pickup_Completed, null);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }
                this.EnabledTransferToolStripDisplayed();
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void Pickup_Completed(Object sender, AsyncCompletedEventArgs e)
        {
            this.ShowActiveCallInfo();
        }

        private void SetActiveCallInfo()
        {
            string scope = "CIC::frmMain::SetActiveCallInfo()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (this.InteractionList != null)
                {
                    if (this.InteractionList.Count > 0)
                    {
                        foreach (ININ.IceLib.Interactions.Interaction CurrentInteraction in this.InteractionList)
                        {
                            if (CurrentInteraction.State.ToString().ToLower().Trim() == "connected")
                            {
                                this.ActiveNormalInteraction = CurrentInteraction;
                                this.SwapPartyFlag = false;
                                break;
                            }
                        }
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

        private ININ.IceLib.Interactions.Interaction GetNormalInteractionFromList()
        {
            string scope = "CIC::frmMain::GetNormalInteractionFromList()::";
            Tracing.TraceStatus(scope + "Starting.");
            ININ.IceLib.Interactions.Interaction retInteraction = null;
            if (this.InteractionList != null)
            {
                if (InteractionList.Count > 0)
                {
                    foreach (ININ.IceLib.Interactions.Interaction CurrentInteraction in this.InteractionList)
                    {
                        retInteraction = CurrentInteraction;
                        break;
                    }
                }
            }
            Tracing.TraceStatus(scope + "Completed.");
            return retInteraction;
        }

        private ININ.IceLib.Interactions.Interaction GetAvailableInteractionFromList()
        {
            string scope = "CIC::frmMain::GetAvailableInteractionFromList()::";
            Tracing.TraceStatus(scope + "Starting.");
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
            Tracing.TraceStatus(scope + "Completed.");
            return retInteraction;
        }

        private void RemoveNormalInteractionFromList(ININ.IceLib.Interactions.InteractionId InteractionID)
        {
            string scope = "CIC::frmMain::RemoveNormalInteractionFromList(InteractionID)::";      //Over load I
            Tracing.TraceStatus(scope + "Starting.");
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
                            this.ActiveNormalInteraction = (ININ.IceLib.Interactions.Interaction)this.InteractionList[retIndex];
                            this.InteractionList.RemoveAt(retIndex);
                        }
                    }
                }
            }
            else
            {
                this.InteractionList.Clear();
                this.IsMuted = false;
            }
            this.Remove_ActiveCallSelPopMenu();
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void RemoveNormalInteractionFromList(ININ.IceLib.Interactions.Interaction Interaction_Object)
        {
            string scope = "CIC::frmMain::RemoveNormalInteractionFromList(Interaction_Object)::";      //Over load II
            Tracing.TraceStatus(scope + "Starting.");
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
                                        if (((ININ.IceLib.Interactions.Interaction)this.InteractionList[i]).IsDisconnected == true)
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
                                        this.CtmMenuIndex = retIndex;
                                        this.Remove_CtmContextMenuByIndex();
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
                                    this.ActiveNormalInteraction = (ININ.IceLib.Interactions.Interaction)this.InteractionList[retIndex];
                                    this.CtmMenuIndex = retIndex;
                                    this.Remove_CtmContextMenuByIndex();
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
                        this.ActiveConsultInteraction = null;
                        this.ActiveConferenceInteraction = null;
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
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                this.EnabledTransferToolStripDisplayed();
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void Remove_CtmContextMenuByIndex()
        {
            string scope = "CIC::frmMain::Remove_CtmContextMenuByIndex()::";
            Tracing.TraceStatus(scope + "Starting.");
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new MethodInvoker(Remove_CtmContextMenuByIndex));
            }
            else
            {
                try
                {
                    if ((this.CtmMenuIndex >= 0) && (this.CtmMenuIndex < this.ctmActiveCallSelection.Items.Count))
                    {
                        this.ctmActiveCallSelection.Items.RemoveAt(this.CtmMenuIndex);
                    }
                    this.CtmMenuIndex = -1;    //Reset
                }
                catch (System.Exception ex)
                {
                    Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        private void CancelTransferToolStripButton_Click(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::CancelTransferToolStripButton_Click()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                switch (this.IsLoggedIntoDialer)
                {
                    case true: //Log On to IC-Dialer.
                        if (this.ActiveDialerInteraction != null)
                        {
                            if (this.ActiveNormalInteraction != null)
                            {
                                if (this.InteractionList.Count > 0)
                                {
                                    if (this.ActiveConsultInteraction != null)
                                    {
                                        this.ActiveConsultInteraction.Disconnect();
                                        this.RemoveNormalInteractionFromList(this.ActiveConsultInteraction);
                                        this.ActiveConsultInteraction = null;
                                    }
                                    else
                                    {
                                        this.ActiveConsultInteraction = this.ActiveNormalInteraction;
                                        this.ActiveConsultInteraction.Disconnect();
                                        this.RemoveNormalInteractionFromList(this.ActiveConsultInteraction);
                                        this.ActiveConsultInteraction = null;
                                    }
                                    if (this.InteractionList != null)
                                    {
                                        foreach (ININ.IceLib.Interactions.Interaction CurrentInteraction in this.InteractionList)
                                        {
                                            if (CurrentInteraction.IsHeld == true)
                                            {
                                                this.ActiveNormalInteraction = CurrentInteraction;
                                                this.ActiveNormalInteraction.Pickup();
                                                break;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                        break;
                    default: //NOT Log On to IC-Dialer.
                        if (this.ActiveNormalInteraction != null)
                        {
                            if (this.InteractionList.Count > 0)
                            {
                                if (this.ActiveConsultInteraction != null)
                                {
                                    this.ActiveConsultInteraction.Disconnect();
                                    this.RemoveNormalInteractionFromList(this.ActiveConsultInteraction);
                                    this.ActiveConsultInteraction = null;
                                }
                                else
                                {
                                    this.ActiveConsultInteraction = this.ActiveNormalInteraction;
                                    this.ActiveConsultInteraction.Disconnect();
                                    this.RemoveNormalInteractionFromList(this.ActiveConsultInteraction);
                                    this.ActiveConsultInteraction = null;
                                }
                                if (this.InteractionList != null)
                                {
                                    foreach (ININ.IceLib.Interactions.Interaction CurrentInteraction in this.InteractionList)
                                    {
                                        if (CurrentInteraction.IsHeld == true)
                                        {
                                            this.ActiveNormalInteraction = CurrentInteraction;
                                            this.ActiveNormalInteraction.Pickup();
                                            break;
                                        }
                                    }
                                }
                            }

                        }
                        break;
                }
                this.ShowActiveCallInfo();
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                this.EnabledTransferToolStripDisplayed();
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void CreateConferenceToolStripButton_Click(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::CreateConferenceToolStripButton_Click()::";
            Tracing.TraceStatus(scope + "Starting.");
            int idx = 0;
            ININ.IceLib.Interactions.Interaction[] TmpInteraction;
            try
            {
                    switch (this.IsLoggedIntoDialer)
                    {
                        case true: //Log On to IC-Dialer.
                            if (this.ActiveDialerInteraction != null)
                            {
                                if (this.ActiveNormalInteraction != null)
                                {
                                    if (this.InteractionList != null)
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
                            }
                            break;
                        default:
                            if (this.ActiveNormalInteraction != null)
                            {
                                if (this.InteractionList != null)
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
                            break;
                    }
                    this.CreateConferenceToolStripButton.Enabled = false;
                    this.LeaveConferenceToolStripButton.Enabled = true;
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                this.CreateConferenceToolStripButton.Enabled = false;
                this.LeaveConferenceToolStripButton.Enabled = false;
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void MakeNewConferenceCompleted(Object sender, MakeNewConferenceCompletedEventArgs e)
        {
            string scope = "CIC::frmMain::MakeNewConferenceCompleted()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                //Conference variable
                this.ActiveConferenceInteraction = e.InteractionConference;
                bool ConferenceCancel = e.Cancelled;
                object ConferenceuserState = e.UserState;
                System.Exception ConferenceErrMsg = e.Error;
                this.ActiveConsultInteraction = null;
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void LeaveConferenceToolStripButton_Click(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::LeaveConferenceToolStripButton_Click()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (this.ActiveConsultInteraction != null)
                {
                    this.ActiveConsultInteraction.Disconnect();
                }
                switch (this.IsLoggedIntoDialer)
                {
                    case true: //Log On to IC-Dialer.
                        if (this.ActiveDialerInteraction != null)
                        {
                            this.ActiveDialerInteraction.Disconnect();
                        }
                        break;
                    default: //Not Log On to IC-Dialer.
                        if (this.ActiveNormalInteraction != null)
                        {
                            this.ActiveNormalInteraction.Disconnect();
                        }
                        break;
                }
                this.ActiveConferenceInteraction = null;
                this.EnabledTransferToolStripDisplayed();
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                this.EnabledTransferToolStripDisplayed();
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }      

        private void IVRMenu_Click(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::IVRMenu_Click()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (this.IVRMenuList != null)
                {
                    this.IVRMenuList.Close();
                    this.IVRMenuList = new frmIVRList(this.sCollectUserSelect);
                    this.IVRMenuList.Show();
                }
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void TransferTxtDestination_TextChanged(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::MonitoringTransferToolStrip()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                this.EnabledTransferToolStripDisplayed();
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void toolStripButtonClear_Click(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::MonitoringTransferToolStrip()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (this.ActiveConsultInteraction == null)
                {
                    this.TransferTxtDestination.Text = "";
                    //this.TransferTxtDestination
                }
                else
                {
                    this.TransferTxtDestination.Text = "";

                    //this.ResetActiveCallInfo();
                }
            }
            catch (System.Exception ex)
            {
                this.muserStatusGridView.ClearRowsSelection();
                this.MakeConsultCallToolStripButton.Enabled = false;
                this.TransferNowToolStripButton.Enabled = false;
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.MainWebBrowser.ShowSaveAsDialog();
        }

        private void PrintToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.MainWebBrowser.Print();
        }

        private void PrintPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.MainWebBrowser.ShowPrintPreviewDialog();
        }

        private void PageSetupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.MainWebBrowser.ShowPageSetupDialog();
        }

        private void PropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.MainWebBrowser.ShowPropertiesDialog();
        }

        private void mnuContents_Click(object sender, EventArgs e)
        {
            string HelpFilePath = CIC.Program.ApplicationPath + "\\Supports\\" + global::CIC.Properties.Settings.Default.CIC_Agent_Hlp;
            if (System.IO.File.Exists(HelpFilePath) == true)
            {
                Help.ShowHelp(this, HelpFilePath);
            }
        }

        private void PhoneNumberToolStripTextBox_TextChanged(object sender, EventArgs e)
        {
            if (this.PhoneNumberToolStripTextBox.Text.Trim() != "")
            {
                this.IsDialingEnabled = true;
            }
            else
            {
                this.IsDialingEnabled = false;
            }
            if (this.IsDialingEnabled == true)
            {
                this.CallToolStripSplitButton.Enabled = true;
            }
            else
            {
                this.CallToolStripSplitButton.Enabled = false;
            }
        }

        private void imgcmbAgentStatus_SelectedValueChanged(object sender, EventArgs e)
        {
            this.userManualStatusChangeFlag = true;
        }

        #endregion

        #region Initial Normal Interaction
    
        private void Initial_InteractionAttributes()
        {
            string scope = "CIC::MainForm::Initial_InteractionAttributes():: ";
            Tracing.TraceStatus(scope + "Starting.");
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
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void Initial_NormalInteraction()
        {
            string scope = "CIC::MainForm::Initial_NormalInteraction()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                Tracing.TraceStatus(scope + "Getting an instance of Normal InteractionsManager.");
                this.NormalInterationManager = InteractionsManager.GetInstance(this.IC_Session);
                if (this.InteractionList == null)
                {
                    this.InteractionList = new System.Collections.ArrayList();
                }
                else
                {
                    this.InteractionList.Clear();
                }
                Tracing.TraceStatus(scope + "Getting an instance of PeopleManager[Normal Interactions].");
                this.mPeopleManager = PeopleManager.GetInstance(this.IC_Session);
                this.IsLoggedIntoDialer = false;
                this.WebBrowserStatusToolStripStatusLabel.Text = "";
                //this.ActiveNormalEmailInteraction.
                if (this.sCollectUserSelect != null)
                {
                    if (this.sCollectUserSelect.Trim() != "")
                    {
                        this.IVRMenu.Enabled = true;
                    }
                    else
                    {
                        this.IVRMenu.Enabled = false;
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

        private void Dispose_QueueWatcher()
        {
            string scope = "CIC::MainForm::Dispose_QueueWatcher():: ";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                Tracing.TraceStatus(scope + "Creating instance of InteractionQueue");
                if (this.m_InteractionQueue != null)
                {
                    Tracing.TraceStatus(scope + "Attaching event handlers");
                    this.m_InteractionQueue.InteractionAdded -= new EventHandler<InteractionAttributesEventArgs>(this.m_InteractionQueue_InteractionAdded);
                    this.m_InteractionQueue.InteractionChanged -= new EventHandler<InteractionAttributesEventArgs>(m_InteractionQueue_InteractionChanged);
                    this.m_InteractionQueue.InteractionRemoved -= new EventHandler<InteractionEventArgs>(m_InteractionQueue_InteractionRemoved);
                    this.m_InteractionQueue.ConferenceInteractionAdded -= new EventHandler<ConferenceInteractionAttributesEventArgs>(m_InteractionQueue_ConferenceInteractionAdded);
                    this.m_InteractionQueue.ConferenceInteractionChanged -= new EventHandler<ConferenceInteractionAttributesEventArgs>(m_InteractionQueue_ConferenceInteractionChanged);
                    this.m_InteractionQueue.ConferenceInteractionRemoved -= new EventHandler<ConferenceInteractionEventArgs>(m_InteractionQueue_ConferenceInteractionRemoved);
                    this.m_InteractionQueue.StopWatchingAsync(null, null);
                    this.m_InteractionQueue = null;
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void InitializeQueueWatcher()
        {
            string scope = "CIC::MainForm::InitializeQueueWatcher():: ";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                Tracing.TraceStatus(scope + "Creating instance of InteractionQueue");
                if (this.NormalInterationManager != null)
                {
                    this.m_InteractionQueue = new ININ.IceLib.Interactions.InteractionQueue(this.NormalInterationManager, new QueueId(QueueType.MyInteractions, this.IC_Session.UserId));
                    Tracing.TraceStatus(scope + "Attaching event handlers");
                    this.m_InteractionQueue.InteractionAdded += new EventHandler<InteractionAttributesEventArgs>(this.m_InteractionQueue_InteractionAdded);
                    this.m_InteractionQueue.InteractionChanged += new EventHandler<InteractionAttributesEventArgs>(m_InteractionQueue_InteractionChanged);
                    this.m_InteractionQueue.InteractionRemoved += new EventHandler<InteractionEventArgs>(m_InteractionQueue_InteractionRemoved);
                    this.m_InteractionQueue.ConferenceInteractionAdded += new EventHandler<ConferenceInteractionAttributesEventArgs>(m_InteractionQueue_ConferenceInteractionAdded);
                    this.m_InteractionQueue.ConferenceInteractionChanged += new EventHandler<ConferenceInteractionAttributesEventArgs>(m_InteractionQueue_ConferenceInteractionChanged);
                    this.m_InteractionQueue.ConferenceInteractionRemoved += new EventHandler<ConferenceInteractionEventArgs>(m_InteractionQueue_ConferenceInteractionRemoved);
                    Tracing.TraceStatus(scope + "Start watching for queue events");
                    this.Initial_InteractionAttributes();
                    this.m_InteractionQueue.StartWatchingAsync(this.InteractionAttributes, null, null);
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void Set_ActiveCallSelPopMenu()
        {
            string scope = "CIC::MainForm::set_ActiveCallSelPopMenu():: ";
            Tracing.TraceStatus(scope + "Starting.");
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
                    if (this.ActiveNormalInteraction != null)
                    {
                        switch(this.ActiveNormalInteraction.Direction)
                        {
                            case InteractionDirection.Incoming:
                                MenuNameText = "{" + this.ActiveNormalInteraction.InteractionId.Id.ToString() + "} [" + this.ActiveNormalInteraction.LocalName + "] <- ["+this.ActiveNormalInteraction.RemoteDisplay+"]";
                                break;
                            case  InteractionDirection.Outgoing:
                                MenuNameText = "{" + this.ActiveNormalInteraction.InteractionId.Id.ToString() + "} [" + this.ActiveNormalInteraction.LocalName + "] -> [" + this.ActiveNormalInteraction.RemoteDisplay + "]";
                                break;
                            default:
                                MenuNameText = "{" + this.ActiveNormalInteraction.InteractionId.Id.ToString() + "} [" + this.ActiveNormalInteraction.LocalName + "] <?> [" + this.ActiveNormalInteraction.RemoteDisplay + "]";
                                break;
                        }
                        MenuNameValue = this.ActiveNormalInteraction.InteractionId.Id.ToString();
                        if (MenuNameValue != null)
                        {
                            if (this.ActiveNormalInteraction.ImmediateAccess == true)
                            {
                                System.Windows.Forms.ToolStripMenuItem menu = new System.Windows.Forms.ToolStripMenuItem(MenuNameValue, null, POPSelMenu_Clicked);
                                bool IsExist = false;
                                menu.Name = MenuNameValue;
                                menu.Text = MenuNameText;
                                switch (this.ActiveNormalInteraction.InteractionType)
                                {
                                    case InteractionType.Email:
                                        menu.Image = global::CIC.Properties.Resources.Exchange;
                                        break;
                                    case InteractionType.Chat:
                                        menu.Image = global::CIC.Properties.Resources.information;
                                        break;
                                    case InteractionType.Call:
                                        if (this.ActiveNormalInteraction.ConferenceId.Id.ToString().Trim() != "0")
                                        {
                                            menu.Image = global::CIC.Properties.Resources.Training;     //Conference Object
                                        }
                                        else
                                        {
                                            this.Set_ConferenceToolStrip();
                                            menu.Image = global::CIC.Properties.Resources.HmPhone1;
                                        }
                                        break;
                                    default:
                                        menu.Image = global::CIC.Properties.Resources.HmPhone1;
                                        break;
                                }
                                menu.Checked = false;
                                if (this.ctmActiveCallSelection != null)
                                {
                                    for (int i = 0; i < this.ctmActiveCallSelection.Items.Count; i++)
                                    {
                                        if (((System.Windows.Forms.ToolStripMenuItem)this.ctmActiveCallSelection.Items[i]).Text.Trim() == "Empty")
                                        {
                                            EmtyMenuIndex = i;
                                            break;
                                        }
                                    }
                                    if (EmtyMenuIndex >= 0)
                                    {
                                        this.ctmActiveCallSelection.Items.RemoveAt(EmtyMenuIndex);
                                    }
                                    for (int i = 0; i < this.ctmActiveCallSelection.Items.Count; i++)
                                    {
                                        if (((System.Windows.Forms.ToolStripMenuItem)this.ctmActiveCallSelection.Items[i]).Name.Trim() == MenuNameValue.Trim())
                                        {
                                            IsExist = true;
                                            break;
                                        }
                                    }
                                    if (IsExist != true)
                                    {
                                        this.ctmActiveCallSelection.Items.Add(menu);
                                    }
                                }
                            }
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
        }

        private void Set_ConferenceToolStrip()
        {
            string scope = "CIC::MainForm::Set_ConferenceToolStrip():: ";
            Tracing.TraceStatus(scope + "Starting.");
            bool AllConferencePartyConected = true;
            try
            {
                if (this.ctmActiveCallSelection.Items.Count >= 2)
                {
                    if (this.ActiveConferenceInteraction == null)
                    {
                        for (int i = 0; i < this.InteractionList.Count; i++)
                        {
                            if (((ININ.IceLib.Interactions.Interaction)this.InteractionList[i]) != null)
                            {
                                if (((ININ.IceLib.Interactions.Interaction)this.InteractionList[i]).State != InteractionState.Connected)
                                {
                                    if (((ININ.IceLib.Interactions.Interaction)this.InteractionList[i]).State != InteractionState.Held)
                                    {
                                        AllConferencePartyConected = false;
                                        break;
                                    }
                                }
                            }
                        }
                        if (AllConferencePartyConected == true)
                        {
                            this.CreateConferenceToolStripButton.Enabled = true;
                        }
                        else
                        {
                            this.CreateConferenceToolStripButton.Enabled = false;
                        }
                        this.LeaveConferenceToolStripButton.Enabled = false;
                    }
                    else
                    {
                        this.CreateConferenceToolStripButton.Enabled = false;
                        this.LeaveConferenceToolStripButton.Enabled = true;
                    }
                }
                else
                {
                    if (this.ActiveConferenceInteraction == null)
                    {
                        this.CreateConferenceToolStripButton.Enabled = false;
                        this.LeaveConferenceToolStripButton.Enabled = false;
                    }
                    else
                    {
                        this.CreateConferenceToolStripButton.Enabled = false;
                        this.LeaveConferenceToolStripButton.Enabled = true;
                    }
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                this.CreateConferenceToolStripButton.Enabled = false;
                this.LeaveConferenceToolStripButton.Enabled = false;
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
            }
        }

        private void Remove_ActiveCallSelPopMenu()
        {
            string scope = "CIC::MainForm::Remove_ActiveCallSelPopMenu():: ";
            Tracing.TraceStatus(scope + "Starting.");
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
        }

        private void Reset_ActiveCallSelPopMenu()
        {
            string scope = "CIC::MainForm::Reset_ActiveCallSelPopMenu():: ";
            Tracing.TraceStatus(scope + "Starting.");
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new MethodInvoker(Reset_ActiveCallSelPopMenu));
            }
            else
            {
                try
                {
                    if (this.ctmActiveCallSelection != null)
                    {
                        this.ctmActiveCallSelection.Items.Clear();
                    }
                    Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        private void POPSelMenu_Clicked(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::Reset_ActiveCallSelPopMenu():: ";
            Tracing.TraceStatus(scope + "Starting.");
            int i = 0;
            try
            {
                for (i = 0; i < this.ctmActiveCallSelection.Items.Count; i++)
                {
                    ((System.Windows.Forms.ToolStripMenuItem)this.ctmActiveCallSelection.Items[i]).Checked = false;
                }
                System.Windows.Forms.ToolStripMenuItem menuItem = (System.Windows.Forms.ToolStripMenuItem)sender;
                string sender_id = menuItem.Name;
                if (this.InteractionList != null)
                {
                    foreach (ININ.IceLib.Interactions.Interaction interact in this.InteractionList)
                    {
                        if (interact != null)
                        {
                            if (interact.InteractionId.Id.ToString().Trim() == sender_id.Trim())
                            {
                                this.ActiveNormalInteraction = interact;
                                menuItem.Checked = true;
                                switch (interact.InteractionType)
                                {
                                    case InteractionType.Email:
                                        this.ViewEmailDetail(interact);
                                        break;
                                    case InteractionType.Chat:
                                        break;
                                    case InteractionType.Callback:
                                        break;
                                    case InteractionType.Call:
                                        break;
                                    default:
                                        break;
                                }
                                if (this.ActiveNormalInteraction != null)
                                {
                                    this.SwapPartyFlag = true;
                                    this.ActiveNormalInteraction.Pickup();
                                }
                                break;
                            }
                        }
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
       
        private void UnifiedMessaging_StartWatching()
        {
            string scope = "CIC::frmMain::UnifiedMessaging_StartWatching()::";
            Tracing.TraceStatus(scope + "Starting");
            try
            {
                this.NormalUnifiedMessagingManager = ININ.IceLib.UnifiedMessaging.UnifiedMessagingManager.GetInstance(this.IC_Session);
                this.NormalUnifiedMessagingManager.VoicemailWaitingChanged += new System.EventHandler(UnifiedMessagingManager_VoicemailWaitingChanged);
                this.NormalUnifiedMessagingManager.StartWatchingVoicemailWaitingAsync(UnifiedMessagingManager_StartWatchingVoicemailWaitingCompleted, null);
                this.NormalUnifiedMessagingManager.RefreshVoicemailCacheAsync(-1, UnifiedMessagingManager_RefreshVoicemailCacheCompleted, null);
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void UnifiedMessagingManager_RefreshVoicemailCacheCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string scope = "CIC::frmMain::UnifiedMessagingManager_RefreshVoicemailCacheCompleted()::";
            Tracing.TraceStatus(scope + "Starting");
            try
            {
                //_VoicemailListAvailable = true;
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void UnifiedMessagingManager_StartWatchingVoicemailWaitingCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string scope = "CIC::frmMain::UnifiedMessagingManager_RefreshVoicemailCacheCompleted()::";
            Tracing.TraceStatus(scope + "Starting");
            try
            {
                //_WatchingVoicemailWaiting = true;
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void UnifiedMessagingManager_VoicemailWaitingChanged(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::UnifiedMessagingManager_RefreshVoicemailCacheCompleted()::";
            Tracing.TraceStatus(scope + "Starting");
            if (this.InvokeRequired)
            {
                this.Invoke(new EventHandler(UnifiedMessagingManager_VoicemailWaitingChanged));
            }
            else
            {
                try
                {
                    Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        private void EmailInteraction_ResponseUpdated(object sender, EventArgs e)
        {
            string scope = "CIC::MainForm::EmailInteraction_ResponseUpdated():: ";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (this.ActiveNormalEmailInteraction != null)
                {
                    if (!this.ActiveNormalEmailInteraction.Equals(this.emailResponse))
                    {
                        //
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

        private void SetActiveEmailQueue()
        {
            string scope = "CIC::MainForm::SetActiveEmailQueue():: ";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (this.ActiveNormalEmailInteraction != null)
                {
                    this.ActiveNormalEmailInteraction.ResponseUpdated += new EventHandler(EmailInteraction_ResponseUpdated);
                    this.ActiveNormalEmailInteraction.EmailStartWatchingAsync(EmailStartWatchingCompleted, null);
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void EmailStartWatchingCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string scope = "CIC::MainForm::EmailStartWatchingCompleted():: ";
            Tracing.TraceStatus(scope + "Starting.");
            bool IsExist = false;
            int i = 0;
            if (e.Error != null)
            {
                Tracing.TraceStatus(scope + "Error info.-->" + e.Error.StackTrace);
            }
            else
            {
                try
                {
                    this.emailResponse = this.ActiveNormalEmailInteraction.GetResponse();
                    if (this.EmailResponseList.Count > 0)
                    {
                        for (i = 0; i < this.EmailResponseList.Count; i++)
                        {
                            if(((ININ.IceLib.Interactions.EmailResponse)this.EmailResponseList[i]).InteractionId.Id == this.emailResponse.InteractionId.Id)
                            {
                                IsExist = true;
                            }
                        }
                    }
                    if (IsExist == true)
                    {
                        this.EmailResponseList.RemoveAt(i);
                    }
                    this.EmailResponseList.Add(this.emailResponse);
                    Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    Tracing.TraceStatus(scope + "Error info." + ex.Message + "-->" + e.Error.StackTrace);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message + "-->" + e.Error.StackTrace, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        private void Add_InteractionListObject(ININ.IceLib.Interactions.Interaction mInteraction)
        {
            int chk_idx = -1;
            string scope = "CIC::MainForm::Add_InteractionListObject():: ";
            Tracing.TraceStatus(scope + "Starting.");
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
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void m_InteractionQueue_InteractionAdded(object sender, InteractionAttributesEventArgs e)
        {
            string scope = "CIC::MainForm::m_InteractionQueue_InteractionAdded():: ";
            Tracing.TraceStatus(scope + "Starting.");
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
                        this.ActiveNormalEmailInteraction = new EmailInteraction(this.NormalInterationManager, e.Interaction.InteractionId);
                        this.SetActiveEmailQueue();
                        this.ActiveNormalInteraction = e.Interaction;
                        this.ShowActiveCallInfo();
                        break;
                    case InteractionType.Chat:
                        //
                        break;
                    case InteractionType.Callback:
                        this.ActiveCallbackInteraction = new CallbackInteraction(this.NormalInterationManager, e.Interaction.InteractionId);
                        this.ActiveNormalInteraction = e.Interaction;
                        this.StrConnectionState = this.ActiveNormalInteraction.State.ToString();
                        this.ShowActiveCallInfo();
                        break;
                    case InteractionType.Call:
                        if (e.Interaction.IsDisconnected != true)
                        {
                            this.Add_InteractionListObject(e.Interaction);
                            this.ActiveNormalInteraction = e.Interaction;
                            this.StrConnectionState = this.ActiveNormalInteraction.State.ToString();
                            if (this.ActiveNormalInteraction.GetStringAttribute("CallerHost") != null)
                            {
                                if (this.ActiveNormalInteraction.GetStringAttribute("CallerHost").ToString().Trim() != "")
                                {
                                    this.CallerHost = this.ActiveNormalInteraction.GetStringAttribute("CallerHost");
                                }
                                else
                                {
                                    ININ.IceLib.Connection.SessionSettings session_Setting = this.ActiveNormalInteraction.InteractionsManager.Session.GetSessionSettings();
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
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void NormalInteraction_AttributesChanged(object sender, AttributesEventArgs e)
        {
            string scope = "CIC::MainForm::NormalInteraction_AttributesChanged():: ";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (this.ActiveNormalInteraction != null)
                {
                    this.StrConnectionState = this.ActiveNormalInteraction.State.ToString();
                }
                else
                {
                    if (this.InteractionList != null)
                    {
                        if (this.InteractionList.Count <= 0)
                        {
                            this.StrConnectionState = "None";
                        }
                        else
                        {
                            //
                        }
                    }
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch
            {
                //Get Connection State.
                this.StrConnectionState = "None";
            }
        }

        private void m_InteractionQueue_InteractionChanged(object sender, InteractionAttributesEventArgs e)
        {
            string scope = "CIC::MainForm::m_InteractionQueue_InteractionChanged():: ";
            Tracing.TraceStatus(scope + "Starting.");
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
                        this.ActiveNormalInteraction = e.Interaction;
                        this.StrConnectionState = this.ActiveNormalInteraction.State.ToString();
                        if (this.ActiveNormalInteraction != null)
                        {
                            if (this.ActiveNormalInteraction.IsDisconnected == true)
                            {
                                this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
                                this.ActiveNormalInteraction = this.GetAvailableInteractionFromList();
                            }
                            else
                            {
                                if (this.BlindTransferFlag != true)
                                {
                                    this.Set_ActiveCallSelPopMenu();
                                }
                            }
                        }
                        if (this.BlindTransferFlag == true)
                        {
                            this.ResetActiveCallInfo();
                        }
                        this.ShowActiveCallInfo();
                        break;
                    default:
                        this.ActiveNormalInteraction = e.Interaction;
                        if (this.ActiveNormalInteraction != null)
                        {
                            if (this.ActiveNormalInteraction.IsDisconnected == true)
                            {
                                this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
                                this.ActiveNormalInteraction = this.GetAvailableInteractionFromList();
                            }
                        }
                        if (this.BlindTransferFlag == true)
                        {
                            this.StrConnectionState = "N/A";
                        }
                        this.ShowActiveCallInfo();
                        break;
                }

                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                if (this.ActiveNormalInteraction != null)
                {
                    this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
                    this.Remove_ActiveCallSelPopMenu();
                    this.ActiveNormalInteraction = this.GetAvailableInteractionFromList();
                    if (this.BlindTransferFlag == true)
                    {
                        this.BlindTransferFlag = false;
                        this.StrConnectionState = "N/A";
                    }
                    this.ShowActiveCallInfo();
                }
            }
        }

        private void m_InteractionQueue_InteractionRemoved(object sender, InteractionEventArgs e)
        {
            string scope = "CIC::MainForm::m_InteractionQueue_InteractionRemoved():: ";
            Tracing.TraceStatus(scope + "Starting.");
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
                        this.ActiveNormalInteraction = e.Interaction;
                        if (this.ActiveNormalInteraction != null)
                        {
                            if (this.ActiveNormalInteraction.IsDisconnected == true)
                            {
                                this.Remove_ActiveCallSelPopMenu();
                                this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
                                this.CallerHost = "";
                                this.ActiveNormalInteraction = this.GetAvailableInteractionFromList();
                                if (this.ActiveNormalInteraction.State != InteractionState.None)  //chk EIC_STATE
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
                        this.ActiveNormalInteraction = e.Interaction;
                        if (this.ActiveNormalInteraction != null)
                        {
                            if (this.ActiveNormalInteraction.IsDisconnected == true)
                            {
                                this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
                                this.CallerHost = "";
                                this.ActiveNormalInteraction = this.GetAvailableInteractionFromList();
                                if (this.ActiveNormalInteraction != null)
                                {
                                    this.StrConnectionState = this.ActiveNormalInteraction.State.ToString();
                                }
                                else
                                {
                                    this.StrConnectionState = "None";
                                }
                                if (this.ActiveNormalInteraction.State != InteractionState.None)  //chk EIC_STATE
                                {
                                    this.ShowActiveCallInfo();
                                }
                            }
                        }
                        break;
                    case InteractionType.Call:
                        this.ActiveNormalInteraction = e.Interaction;
                        if (this.ActiveNormalInteraction.IsConnected != true)
                        {
                            if (this.ActiveNormalInteraction != null)
                            {
                                this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
                                this.CallerHost = "";
                                this.ActiveNormalInteraction = this.GetAvailableInteractionFromList();
                                if (this.ActiveNormalInteraction != null)
                                {
                                    this.StrConnectionState = this.ActiveNormalInteraction.State.ToString();
                                }
                                else
                                {
                                    this.StrConnectionState = "None";
                                }
                                this.ShowActiveCallInfo();
                            }
                        }
                        break;
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                this.ResetActiveCallInfo();
                this.CallerHost = "";
                if (this.ActiveNormalInteraction != null)
                {
                    try
                    {
                        this.ActiveNormalInteraction.Disconnect();
                    }
                    catch
                    {
                        //Emty catch block
                    }
                this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
                }
                this.ActiveNormalInteraction = null;
                this.ShowActiveCallInfo();
            }
        }

        private void m_InteractionQueue_ConferenceInteractionAdded(object sender, ConferenceInteractionAttributesEventArgs e)
        {
            string scope = "CIC::MainForm::m_InteractionQueue_ConferenceInteractionAdded():: ";
            Tracing.TraceStatus(scope + "Starting.");
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
                    this.IsActiveConference_flag = true;
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
                                this.ActiveConferenceInteraction = new InteractionConference(this.NormalInterationManager, e.Interaction.InteractionType, e.ConferenceItem.ConferenceId);
                                this.ActiveNormalInteraction = e.Interaction;
                                this.ShowActiveCallInfo();
                            } 
                            break;
                        default:
                            break;
                    }
                    Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    Tracing.TraceStatus(scope + "Error info." + ex.Message);
                }
            }
        }

        private void m_InteractionQueue_ConferenceInteractionChanged(object sender, ConferenceInteractionAttributesEventArgs e)
        {
            string scope = "CIC::MainForm::m_InteractionQueue_ConferenceInteractionChanged():: ";
            Tracing.TraceStatus(scope + "Starting.");
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
                            this.ActiveNormalInteraction = e.Interaction;
                            if (e.ConferenceItem.IsDisconnected == true)
                            {
                                this.RemoveNormalInteractionFromList(this.ActiveNormalInteraction);
                            }
                            break;
                        default:
                            break;
                    }
                    this.Set_ConferenceToolStrip();
                    this.ShowActiveCallInfo();
                    Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                    this.ShowActiveCallInfo();
                }
            }
        }

        private void m_InteractionQueue_ConferenceInteractionRemoved(object sender, ConferenceInteractionEventArgs e)
        {
            string scope = "CIC::MainForm::m_InteractionQueue_ConferenceInteractionChanged():: ";
            Tracing.TraceStatus(scope + "Starting.");
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
                            this.ActiveNormalInteraction = e.Interaction;
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
                    Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        private void Start_AlertingWav()
        {
            string scope = "CIC::MainForm::Start_AlertingWav():: ";
            Tracing.TraceStatus(scope + "Starting.");
            string sPathWavPath = "";
            bool AlertFlag = false;
            bool Play_Looping = false;
            int RingCount = 0;
            int PlayCount = 0;
            switch (this.IsLoggedIntoDialer)
            {
                case true:
                    sPathWavPath = CIC.Program.ResourcePath + global::CIC.Properties.Settings.Default.DialerAlertSound;
                    AlertFlag = global::CIC.Properties.Settings.Default.DialerAlerting;
                    Play_Looping = global::CIC.Properties.Settings.Default.DialerLooping;
                    RingCount = global::CIC.Properties.Settings.Default.DialerRingCount;
                    break;
                default:
                    sPathWavPath = CIC.Program.ResourcePath + global::CIC.Properties.Settings.Default.NormalAlertSound;
                    AlertFlag = global::CIC.Properties.Settings.Default.NormalAlertting;
                    Play_Looping = global::CIC.Properties.Settings.Default.NormalLooping;
                    RingCount = global::CIC.Properties.Settings.Default.NormalRingCount;
                    break;
            }
            this.AlertSoundFileType = sPathWavPath.Trim().Substring(sPathWavPath.Trim().Length - 3, 3);
            if (AlertSoundFileType.ToLower().Trim() == "mp3")
            {
                this.cicMp3Player = new Locus.Control.MP3Player();
                this.cicMp3Player.Play(sPathWavPath,false);
            }
            else
            {
                if (AlertFlag == true)
                {
                    try
                    {
                        if (this.CurrentUserStatus != null)
                        {
                            if (this.CurrentUserStatus.StatusMessageDetails.IsDoNotDisturbStatus != true)
                            {
                                if (System.IO.File.Exists(sPathWavPath) == true)
                                {
                                    if (this.IsPlayAlerting != true)
                                    {
                                        this.soundPlayer = new System.Media.SoundPlayer(sPathWavPath);
                                        this.IsPlayAlerting = true;
                                        if (Play_Looping == true)
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
                                    Tracing.TraceStatus(scope + "Error info. : WAV File not found.");
                                }
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {

                        this.IsPlayAlerting = false;
                        Tracing.TraceStatus(scope + "Error info." + ex.Message);
                        System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                    }
                }
            }
        }

        private void Stop_AlertingWav()
        {
            string scope = "CIC::MainForm::Stop_AlertingWav():: ";
            Tracing.TraceStatus(scope + "Starting.");
            if (AlertSoundFileType.ToLower().Trim() == "mp3")
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
                    if (this.IsPlayAlerting == true)
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
                    Tracing.TraceStatus(scope + "Error info." + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
        }

        private void muserStatusGridView_ContextMenuEvent(object sender, UserDataEventArgs e)
        {
            string scope = "CIC::MainForm::muserStatusGridView_ContextMenuEvent()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                System.Windows.Forms.KeyEventArgs ke = new System.Windows.Forms.KeyEventArgs(Keys.Enter);
                if (e.MenuValue.Trim() != "")
                {
                    switch (e.MenuName)
                    {
                        case "callToolStripMenuItem":
                            this.PhoneNumberToolStripTextBox.Text = e.MenuValue;
                            this.PhoneNumberToolStripTextBox_KeyDown(sender, ke);
                            break;
                        case "mobiletoolStripMenuItem":
                            this.PhoneNumberToolStripTextBox.Text = e.MenuValue;
                            this.PhoneNumberToolStripTextBox_KeyDown(sender, ke);
                            break;
                        case "mailToToolStripMenuItem":
                            System.Diagnostics.Process.Start("mailto://" + e.MenuValue);
                            break;
                        default:
                            break;
                    }
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void muserStatusGridView_RowSelected(object sender, EventArgs e)
        {
            string scope = "CIC::frmMain::muserStatusGridView_RowSelected()::";
            Tracing.TraceStatus(scope + "Starting.");
            if (this.muserStatusGridView != null)
            {
                if (this.muserStatusGridView.SelectedItem != null)
                {
                    if (this.muserStatusGridView.SelectedItem.Extension != null)
                    {
                        if (this.muserStatusGridView.SelectedItem.Extension.ToString() != "")
                        {
                            this.TransferStatusToolStripLabel.Text = "Selected : " + this.muserStatusGridView.SelectedItem.UserId;
                            this.TransferTxtDestination.Text = this.muserStatusGridView.SelectedItem.Extension;
                        }
                    }
                }
            }
            Tracing.TraceStatus(scope + "Completed.");
        }

        #endregion

        #region Initial Dialer Interaction
     
        private void Initial_DialerInteraction()
        {
            string scope = "CIC::MainForm::Initial_DialerInteraction()::";
            Tracing.TraceStatus(scope + "Starting.");
            Tracing.TraceStatus(scope + "Getting an instance of PeopleManager[Dialer Interactions].");
            this.mPeopleManager = PeopleManager.GetInstance(Program.DialingManager.Session);
            this.IsLoggedIntoDialer = true;
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void Initial_ActityCodes()
        {
            string scope = "CIC::MainForm::Initial_ActityCodes()::";
            Tracing.TraceStatus(scope + "Starting.");
            CallActivityCodeToolStripComboBox.Items.Clear();
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
                            this.CallActivityCodeToolStripComboBox.Items.Add(this.DsReasonCode.Tables[0].Rows[i]["finish_code"].ToString());
                        }
                    }
                }
            }
            if (this.CallActivityCodeToolStripComboBox.Items.Count > 0)
            {
                this.CallActivityCodeToolStripComboBox.SelectedIndex = 0;
            }
            Tracing.TraceStatus(scope + "Completed.");
        }

        private ININ.IceLib.Dialer.ReasonCode GetReasonCode(string FinishCode)
        {
            ININ.IceLib.Dialer.ReasonCode sRet = 0;
            System.Data.DataRow[] Dr = this.DsReasonCode.Tables[0].Select("finish_code='" + FinishCode + "'");
            string sReason = "";
            if (Dr.Length > 0)
            {
                sReason = Dr[0]["reason_code"].ToString();
                switch (sReason.ToLower().Trim())
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
            }
            return sRet;
        }

        #endregion

        #region Agent Status Function

        private void InitializeStatusMessageDetails()
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
            Tracing.TraceStatus(scope + "Starting.");
            this.imgcmbAgentStatus.ImageList.Images.Clear();
            this.imgcmbAgentStatus.Items.Clear();
            try
            {
                switch (this.IsLoggedIntoDialer)
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
                            this.imgcmbAgentStatus.ImageList = this.imsLstServerStatus;
                            this.CmbImgAgentStatus.ImageList = this.imsLstServerStatus;
                            this.imsLstServerStatus.Images.Clear();
                            sIconPath = CIC.Program.ResourcePath;
                            this.AllStatusMessageList.StartWatching();
                            foreach (StatusMessageDetails status in this.AllStatusMessageList.GetList())
                            {
                                sIconName = this.GetFilenameFromFilePath(status.IconFileName.ToString());
                                sIconPath += sIconName;
                                if (System.IO.File.Exists(sIconPath) == true)
                                {
                                    Status_icon = new System.Drawing.Icon(sIconPath);
                                    this.imsLstServerStatus.Images.Add(status.MessageText, Status_icon);
                                }
                                else
                                {
                                    this.imsLstServerStatus.Images.Add(status.MessageText, status.Icon);
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
                                this.imgcmbAgentStatus.Items.Add(new ImageComboBoxItem(status.MessageText, iIndex));
                                iIndex++;
                                Tracing.TraceNote(scope + "Id=" + status.Id + ", MessageText=" + status.MessageText);
                            }
                        }
                        break;
                    default:    //Not Log On to Workflow
                        Tracing.TraceNote(scope + "Creating instance of StatusMessageList");
                        if (this.mPeopleManager != null)
                        {
                            string[] nusers = { this.IC_Session.UserId };   //Make value to array 
                            this.AllStatusMessageList = new StatusMessageList(this.mPeopleManager);
                            this.AllStatusMessageListOfUser = new UserStatusList(this.mPeopleManager);
                            this.AllStatusMessageListOfUser.WatchedObjectsChanged += new EventHandler<WatchedObjectsEventArgs<UserStatusProperty>>(AllStatusMessageListOfUser_WatchedObjectsChanged);
                            this.AllStatusMessageListOfUser.StartWatching(nusers);
                            this.CurrentUserStatus = this.AllStatusMessageListOfUser.GetUserStatus(this.IC_Session.UserId);
                            this.imgcmbAgentStatus.ImageList = this.imsLstServerStatus;
                            this.CmbImgAgentStatus.ImageList = this.imsLstServerStatus;
                            this.imsLstServerStatus.Images.Clear();
                            sIconPath = CIC.Program.ResourcePath;
                            this.AllStatusMessageList.StartWatching();
                            foreach (StatusMessageDetails status in this.AllStatusMessageList.GetList())
                            {
                                if (status.IsSelectableStatus == true)
                                {
                                    sIconName = this.GetFilenameFromFilePath(status.IconFileName.ToString());
                                    sIconPath += sIconName;
                                    if (System.IO.File.Exists(sIconPath) == true)
                                    {
                                        Status_icon = new System.Drawing.Icon(sIconPath);
                                        this.imsLstServerStatus.Images.Add(status.MessageText, Status_icon);
                                    }
                                    else
                                    {
                                        this.imsLstServerStatus.Images.Add(status.MessageText, status.Icon);
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
                                    imgcmbAgentStatus.Items.Add(new ImageComboBoxItem(status.MessageText, iIndex));
                                    iIndex++;
                                }
                                Tracing.TraceNote(scope + "Id=" + status.Id + ", MessageText=" + status.MessageText);
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
                            this.imgcmbAgentStatus.SetMessage(this.CurrentUserStatus.StatusMessageDetails.MessageText);
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
                            if (this.imgcmbAgentStatus.Items.Count > 0)
                            {
                                this.imgcmbAgentStatus.SetMessage(this.CurrentUserStatus.StatusMessageDetails.MessageText);
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info." + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void AllStatusMessageListOfUser_WatchedObjectsChanged(object sender, WatchedObjectsEventArgs<UserStatusProperty> e)
        {
            string scope = "CIC::frmMain::AllStatusMessageListOfUser_WatchedObjectsChanged()::";
            Tracing.TraceStatus(scope + "Starting.");
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
                        Tracing.TraceNote(scope + "Watched object changed");
                        ReadOnlyCollection<UserStatusProperty> userStatusProperties;
                        if (e.Changed.TryGetValue(this.IC_Session.UserId, out userStatusProperties))
                        {
                            foreach (ININ.IceLib.People.UserStatusProperty property in userStatusProperties)
                            {
                                switch(property)
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
                                                        if (this.imgcmbAgentStatus.Items.Count > 0)
                                                        {
                                                            if (this.imgcmbAgentStatus.SelectedIndex >= 0)
                                                            {
                                                                string DisplayStatus = this.imgcmbAgentStatus.GetMeaasageText(this.imgcmbAgentStatus.SelectedIndex);
                                                                if (this.CurrentUserStatus.StatusMessageDetails.MessageText.Trim() == DisplayStatus.Trim())
                                                                {
                                                                    statusUpdate.StatusMessageDetails = this.CurrentUserStatus.StatusMessageDetails;
                                                                    statusUpdate.UpdateRequest();
                                                                }
                                                            }
                                                        }
                                                        this.toolStriplblAdditionalStatus.Visible = false;
                                                    }
                                                    else
                                                    {
                                                        this.toolStriplblAdditionalStatus.Text = this.CurrentUserStatus.StatusMessageDetails.MessageText;
                                                        this.toolStriplblAdditionalStatus.Image = (Image)this.CurrentUserStatus.StatusMessageDetails.Icon.ToBitmap();
                                                        if (this.IsLoggedIntoDialer != true)
                                                        {
                                                            if (this.CurrentUserStatus.StatusMessageDetails.MessageText.ToLower().Trim() == global::CIC.Properties.Settings.Default.ACDAvailableTalkMsg.ToLower().Trim())
                                                            {
                                                                this.toolStriplblAdditionalStatus.Visible = true;
                                                            }
                                                            else if (this.CurrentUserStatus.StatusMessageDetails.MessageText.ToLower().Trim() == global::CIC.Properties.Settings.Default.ACDAgenNotAnsweringMsg.ToLower().Trim())
                                                            {
                                                                this.toolStriplblAdditionalStatus.Visible = true;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            this.toolStriplblAdditionalStatus.Visible = false;
                                                        }
                                                    }
                                                }
                                                bool bTemp = this.imgcmbAgentStatus.Enabled;
                                                this.imgcmbAgentStatus.Enabled = true;
                                                if (this.imgcmbAgentStatus.Items.Count > 0)
                                                {
                                                    this.imgcmbAgentStatus.SetMessage(this.CurrentUserStatus.StatusMessageDetails.MessageText);
                                                }
                                                this.imgcmbAgentStatus.Enabled = bTemp;
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
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
        }

        #endregion

        #region Call Status Display Function

        private string GetDialerNumber()
        {
            string DialerNumber = "";
            string AlternatePreview_ATTR = Properties.Settings.Default.AlternatePreviewNumbers;
            string[] AlternatePreviewNoATTRCollection;
            string scope = "CIC::frmMain::GetDialerNumber()::";
            Tracing.TraceStatus(scope + "Starting.");

            if (this.mDialerData[Properties.Settings.Default.Preview_Number_ATTR].ToString().Trim() == String.Empty)
            {
                if (AlternatePreview_ATTR != String.Empty)
                {
                    AlternatePreviewNoATTRCollection = AlternatePreview_ATTR.Split(';');
                    foreach (string PreviewNoATTR in AlternatePreviewNoATTRCollection)
                    {
                        if (PreviewNoATTR.Trim() != String.Empty)
                        {
                            if (this.mDialerData[PreviewNoATTR.Trim()].Trim() != String.Empty)
                            {
                                DialerNumber = this.mDialerData[PreviewNoATTR.Trim()];
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                DialerNumber = this.mDialerData[Properties.Settings.Default.Preview_Number_ATTR].ToString().Trim();
            }
            Tracing.TraceStatus(scope + "Completed.");
            return DialerNumber;
        }

        private void ResetActiveCallInfo()
        {
            string scope = "CIC::frmMain::ResetActiveCallInfo()::";
            Tracing.TraceStatus(scope + "Starting.");
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new MethodInvoker(ResetActiveCallInfo));
            }
            else
            {
                if (this.ActiveConsultInteraction != null)
                {
                    this.DirectiontoolStripStatus.Text = "None";
                    this.CallTypeToolStripStatusLabel.Text = "N/A";
                    this.CampaignIdToolStripStatusLabel.Text = "N/A";
                    this.QueueNameToolStripStatusLabel.Text = "N/A";
                    this.NumberToolStripStatusLabel.Text = "N/A";
                    this.CallStateToolStripStatusLabel.Text = "N/A";
                    this.SetInfoBarColor();
                    this.EnabledTransferToolStripDisplayed();
                    this.muserStatusGridView.ClearRowsSelection();
                }
                else
                {
                    if (this.BlindTransferFlag == true)
                    {
                        this.ctmActiveCallSelection.Items.Clear();
                        this.InteractionList.Clear();
                        this.StrConnectionState = "";
                        this.DirectiontoolStripStatus.Text = "None";
                        this.CallTypeToolStripStatusLabel.Text = "N/A";
                        this.CampaignIdToolStripStatusLabel.Text = "N/A";
                        this.QueueNameToolStripStatusLabel.Text = "N/A";
                        this.NumberToolStripStatusLabel.Text = "N/A";
                        this.CallStateToolStripStatusLabel.Text = "N/A";
                        this.SetInfoBarColor();
                        this.EnabledTransferToolStripDisplayed();
                        this.muserStatusGridView.ClearRowsSelection();
                    }
                    else
                    {
                        TransferTxtDestination.Text = "";
                    }
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
        }

        private void ShowActiveCallInfo()
        {
            string scope = "CIC::frmMain::ShowActiveCallInfo()::";
            Tracing.TraceStatus(scope + "Starting.");
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new MethodInvoker(ShowActiveCallInfo));
            }
            else
            {
                switch (this.IsLoggedIntoDialer)
                {
                    case true: //Log On to IC-Dialer.
                        if (this.ActiveDialerInteraction == null)
                        {
                            this.DirectiontoolStripStatus.Text = "None";
                            this.CallTypeToolStripStatusLabel.Text = "N/A";
                            this.CampaignIdToolStripStatusLabel.Text = "N/A";
                            this.QueueNameToolStripStatusLabel.Text = "N/A";
                            this.NumberToolStripStatusLabel.Text = "N/A";
                            this.CallStateToolStripStatusLabel.Text = "N/A";
                            this.ActiveConferenceInteraction = null;
                            this.Reset_ActiveCallSelPopMenu();
                            this.CallIdToolStripStatusLabel.Text = "None";
                        }
                        else
                        {
                            if (this.ActiveDialerInteraction.DialingMode == DialingMode.Regular)
                            {
                                if (global::CIC.Properties.Settings.Default.AutoAnswer == true)
                                {
                                    this.PickupToolStripButton_Click(this, new EventArgs());
                                }
                            }
                            this.CallIdToolStripStatusLabel.Text = this.ActiveDialerInteraction.CallIdKey.ToString().Trim();  
                            this.DirectiontoolStripStatus.Text = this.ActiveDialerInteraction.Direction.ToString();
                            this.CallTypeToolStripStatusLabel.Text = "Campaign Call";
                            this.CampaignIdToolStripStatusLabel.Text = this.mDialerData[Properties.Settings.Default.Preview_Campaign_ATTR];
                            this.QueueNameToolStripStatusLabel.Text = this.mDialerData[Properties.Settings.Default.Preview_QueueName_ATTR];
                            this.NumberToolStripStatusLabel.Text = this.GetDialerNumber();
                            this.CallStateToolStripStatusLabel.Text = this.ActiveDialerInteraction.StateDescription.ToString();
                        }
                        break;
                    default:
                        if (this.ActiveNormalInteraction == null)
                        {
                            this.DirectiontoolStripStatus.Text = "None";
                            this.CallTypeToolStripStatusLabel.Text = "N/A";
                            this.CampaignIdToolStripStatusLabel.Text = "N/A";
                            this.QueueNameToolStripStatusLabel.Text = "N/A";
                            this.NumberToolStripStatusLabel.Text = "N/A";
                            this.CallStateToolStripStatusLabel.Text = "N/A";
                            this.ActiveConferenceInteraction = null;
                            this.Reset_ActiveCallSelPopMenu();
                            this.CallIdToolStripStatusLabel.Text = "None";
                        }
                        else
                        {
                            if (this.BlindTransferFlag == true)
                            {
                                this.DirectiontoolStripStatus.Text = "None";
                                this.CallTypeToolStripStatusLabel.Text = "N/A";
                                this.CampaignIdToolStripStatusLabel.Text = "N/A";
                                this.QueueNameToolStripStatusLabel.Text = "N/A";
                                this.NumberToolStripStatusLabel.Text = "N/A";
                                this.CallStateToolStripStatusLabel.Text = "N/A";
                                this.ActiveConferenceInteraction = null;
                                this.CallIdToolStripStatusLabel.Text = "None";
                                this.Reset_ActiveCallSelPopMenu();
                            }
                            else
                            {
                                this.StrConnectionState = this.ActiveNormalInteraction.State.ToString();
                                switch (this.StrConnectionState.ToLower().Trim())
                                {
                                    case "n/a":
                                        this.DirectiontoolStripStatus.Text = "None";
                                        this.CallTypeToolStripStatusLabel.Text = "N/A";
                                        this.CampaignIdToolStripStatusLabel.Text = "N/A";
                                        this.QueueNameToolStripStatusLabel.Text = "N/A";
                                        this.NumberToolStripStatusLabel.Text = "N/A";
                                        this.CallStateToolStripStatusLabel.Text = "N/A";
                                        this.ActiveNormalInteraction = null;
                                        this.CallIdToolStripStatusLabel.Text = "None";
                                        break;
                                    case "held":
                                        if (this.SwapPartyFlag == true)
                                        {
                                            this.SetActiveCallInfo();
                                            this.ShowActiveCallInfo();
                                        }
                                        else
                                        {
                                            if (this.IsMuted == true)
                                            {
                                                this.StrConnectionState = "Mute";
                                            }
                                            this.DirectiontoolStripStatus.Text = this.ActiveNormalInteraction.Direction.ToString();
                                            this.CallTypeToolStripStatusLabel.Text = this.ActiveNormalInteraction.InteractionType.ToString();
                                            this.CampaignIdToolStripStatusLabel.Text = "Non-campaign Call";
                                            this.QueueNameToolStripStatusLabel.Text = this.ActiveNormalInteraction.WorkgroupQueueName.ToString();
                                            this.NumberToolStripStatusLabel.Text = this.ActiveNormalInteraction.RemoteDisplay.ToString();
                                            this.CallStateToolStripStatusLabel.Text = this.StrConnectionState;
                                        }
                                        this.CallIdToolStripStatusLabel.Text = this.ActiveNormalInteraction.CallIdKey.ToString().Trim();
                                        break; //IsMuted
                                    case "connected":
                                        if (this.IsMuted == true)
                                        {
                                            this.StrConnectionState = "Mute";
                                        }
                                        this.DirectiontoolStripStatus.Text = this.ActiveNormalInteraction.Direction.ToString();
                                        this.CallTypeToolStripStatusLabel.Text = this.ActiveNormalInteraction.InteractionType.ToString();
                                        this.CampaignIdToolStripStatusLabel.Text = "Non-campaign Call";
                                        this.QueueNameToolStripStatusLabel.Text = this.ActiveNormalInteraction.WorkgroupQueueName.ToString();
                                        this.NumberToolStripStatusLabel.Text = this.ActiveNormalInteraction.RemoteDisplay.ToString();
                                        this.CallStateToolStripStatusLabel.Text = this.StrConnectionState;
                                        this.CallIdToolStripStatusLabel.Text = this.ActiveNormalInteraction.CallIdKey.ToString().Trim();
                                        break;
                                    default:
                                        this.DirectiontoolStripStatus.Text = this.ActiveNormalInteraction.Direction.ToString();
                                        this.CallTypeToolStripStatusLabel.Text = this.ActiveNormalInteraction.InteractionType.ToString();
                                        this.CampaignIdToolStripStatusLabel.Text = "Non-campaign Call";
                                        this.QueueNameToolStripStatusLabel.Text = this.ActiveNormalInteraction.WorkgroupQueueName.ToString();
                                        this.NumberToolStripStatusLabel.Text = this.ActiveNormalInteraction.RemoteDisplay.ToString();
                                        this.CallStateToolStripStatusLabel.Text = this.StrConnectionState;
                                        this.CallIdToolStripStatusLabel.Text = this.ActiveNormalInteraction.CallIdKey.ToString().Trim();
                                        break;
                                }
                            }
                        }
                        break;
                }
                this.SetInfoBarColor();
                this.Set_ConferenceToolStrip();
                if (this.InteractionList != null)
                {
                    if (this.InteractionList.Count <= 0)
                    {
                        this.ActiveConsultInteraction = null;
                        this.IsActiveConference_flag = false;
                        this.ActiveConferenceInteraction = null;
                        this.ActiveNormalInteraction = null;
                        if (this.IsLoggedIntoDialer != true)
                        {
                            this.ActiveDialerInteraction = null;
                        }
                        this.toolStripButtonClear_Click(new Object(), new EventArgs());
                    }
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
        }

        private void SetCallHistory()
        {
            string scope = "CIC::frmMain::SetCallHistory()::";
            Tracing.TraceStatus(scope + "Starting.");
            if (this.PhoneNumberToolStripTextBox.Text.Trim() != String.Empty)
            {
                this.PhoneNumberToolStripTextBox.Items.Add(this.PhoneNumberToolStripTextBox.Text);
            }
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void SetInfoBarColor()
        {
            string scope = "CIC::frmMain::SetInfoBarColor()::";
            Tracing.TraceStatus(scope + "Starting.");
            switch (this.IsLoggedIntoDialer)
            {
                case true: //Log On to IC-Dialer.
                    switch (this.CallStateToolStripStatusLabel.Text.ToLower().Trim())
                    {
                        case "call":
                            this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["WorkFlowBar_Active"];
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            this.Stop_AlertingWav();
                            break;
                        case "alerting":
                            this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["WorkFlowBar_Info"];
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            this.Start_AlertingWav();
                            break;
                        case "offering":
                            this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["WorkFlowBar_Info"];
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            this.Start_AlertingWav();
                            break;
                        case "dialing":
                            this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["WorkFlowBar_Info"];
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            this.Stop_AlertingWav();
                            break;
                        case "proceeding":
                            this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["WorkFlowBar_Info"];
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            this.Stop_AlertingWav();
                            break;
                        case "messaging":
                            this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["WorkFlowBar_Info"];
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            this.Stop_AlertingWav();
                            break;
                        case "mute":
                            this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["WorkFlowBar_Active"];
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            break;
                        case "held":
                            this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["WorkFlowBar_Active"];
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            break;
                        case "connected":
                            this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["WorkFlowBar_Active"];
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            this.Stop_AlertingWav();
                            break;
                        case "initializing":
                            this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["WorkFlowBar_Initial"];
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            this.Stop_AlertingWav();
                            break;
                        default:
                            if (this.CallStateToolStripStatusLabel.Text.Substring(0, 3).ToUpper().Trim() == "ACD")
                            {
                                this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["WorkFlowBar_Active"];
                            }
                            else
                            {
                                this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["WorkFlowBar_Normal"];
                            }
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            this.Stop_AlertingWav();
                            break;
                    }
                    this.EnabledDialerCallTools();
                    break;
                default: //Not Log On to IC-Dialer.
                    switch (this.CallStateToolStripStatusLabel.Text.ToLower().Trim())
                    {
                        case "call":
                            this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["NormalBar_Active"];
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            this.Stop_AlertingWav();
                            break;
                        case "alerting":
                            this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["NormalBar_Info"];
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            this.Start_AlertingWav();
                            break;
                        case "offering":
                            this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["NormalBar_Info"];
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            this.Start_AlertingWav();
                            break;
                        case "proceeding":
                            this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["NormalBar_Info"];
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            this.Stop_AlertingWav();
                            break;
                        case "messaging":
                            this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["NormalBar_Info"];
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            this.Stop_AlertingWav();
                            break;
                        case "dialing":
                            this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["NormalBar_Info"];
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            this.Stop_AlertingWav();
                            break;
                        case "held":
                            this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["NormalBar_Active"];
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            this.Stop_AlertingWav();
                            break;
                        case "mute":
                            this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["NormalBar_Active"];
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            this.Stop_AlertingWav();
                            break;
                        case "connected":
                            this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["NormalBar_Active"];
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            this.Stop_AlertingWav();
                            break;
                        case "initializing":
                            this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["NormalBar_Normal"];
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            this.Stop_AlertingWav();
                            break;
                        default:
                            this.StatusBarStrip.BackgroundImage = global::CIC.Properties.Resources.bluebgBar;
                            if (this.CallStateToolStripStatusLabel.Text.Substring(0, 3).ToUpper().Trim() == "ACD")
                            {
                                this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["NormalBar_Active"];
                            }
                            else
                            {
                                this.CallInfoStatusStrip.BackgroundImage = global::CIC.Program.AppImageList.Images["NormalBar_Normal"];
                            }
                            this.CallInfoStatusStrip.BackgroundImageLayout = ImageLayout.Stretch;
                            this.Stop_AlertingWav();
                            break;
                    }
                    this.EnabledNormalCallTools();
                    break;
            }
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void EnabledNormalCallTools()
        {
            string scope = "CIC::frmMain::EnabledNormalCallTools()::";
            Tracing.TraceStatus(scope + "Starting.");
            Color OldColor = this.TelephonyToolStrip.BackColor;  //Save Original Trasparent Color
            this.DisconnectToolStripButton.Enabled = false;
            this.DispositionToolStripButton.Enabled = false;
            this.CallActivityCodeToolStripComboBox.SelectedIndex = -1;
            this.CallActivityCodeToolStripComboBox.Enabled = false;
            this.PlaceCallToolStripButton.Enabled = false;
            this.SkipCallToolStripButton.Enabled = false;
            this.RequestBreakToolStripButton.Text = "Request Break";
            this.RequestBreakToolStripButton.Enabled = false;
            this.EnabledTransferToolStripDisplayed();
            if (this.sCollectUserSelect.Trim() != "")
            {
                this.IVRMenu.Enabled = true;
            }
            else
            {
                this.IVRMenu.Enabled = false;
            }
            switch (this.CallStateToolStripStatusLabel.Text.ToLower().Trim())
            {
                case "initializing":
                    if (this.IsDialingEnabled == true)
                    {
                        this.CallToolStripSplitButton.Enabled = true;
                    }
                    else
                    {
                        this.CallToolStripSplitButton.Enabled = false;
                    }
                    this.PickupToolStripButton.Enabled = true;
                    this.MuteToolStripButton.Enabled = false;
                    this.HoldToolStripButton.Enabled = false;
                    this.PickupToolStripButton.Checked = false;
                    this.MuteToolStripButton.Checked = this.IsMuted;
                    this.HoldToolStripButton.Checked = false;
                    this.SkipCallToolStripButton.Enabled = false;
                    this.DisconnectToolStripButton.Enabled = false;
                    this.DialpadToolStripDropDownButton.Enabled = false;
                    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case "alerting":
                    if (this.IsDialingEnabled == true)
                    {
                        this.CallToolStripSplitButton.Enabled = true;
                    }
                    else
                    {
                        this.CallToolStripSplitButton.Enabled = false;
                    } 
                    this.PickupToolStripButton.Enabled = true;
                    this.MuteToolStripButton.Enabled = true;
                    this.HoldToolStripButton.Enabled = true;
                    this.PickupToolStripButton.Checked = false;
                    this.MuteToolStripButton.Checked = this.IsMuted;
                    this.HoldToolStripButton.Checked = false;
                    this.SkipCallToolStripButton.Enabled = true;
                    this.DisconnectToolStripButton.Enabled = true;
                    this.DialpadToolStripDropDownButton.Enabled = false;
                    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case "messaging":
                    if (this.IsDialingEnabled == true)
                    {
                        this.CallToolStripSplitButton.Enabled = true;
                    }
                    else
                    {
                        this.CallToolStripSplitButton.Enabled = false;
                    } 
                    this.PickupToolStripButton.Enabled = true;
                    this.MuteToolStripButton.Enabled = true;
                    this.HoldToolStripButton.Enabled = true;
                    this.PickupToolStripButton.Checked = false;
                    this.MuteToolStripButton.Checked = this.IsMuted;
                    this.HoldToolStripButton.Checked = false;
                    this.SkipCallToolStripButton.Enabled = true;
                    this.DisconnectToolStripButton.Enabled = true;
                    this.DialpadToolStripDropDownButton.Enabled = true;
                    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case "offering":
                    if (this.IsDialingEnabled == true)
                    {
                        this.CallToolStripSplitButton.Enabled = true;
                    }
                    else
                    {
                        this.CallToolStripSplitButton.Enabled = false;
                    } 
                    this.PickupToolStripButton.Enabled = true;
                    this.MuteToolStripButton.Enabled = true;
                    this.HoldToolStripButton.Enabled = true;
                    this.PickupToolStripButton.Checked = false;
                    this.MuteToolStripButton.Checked = this.IsMuted;
                    this.HoldToolStripButton.Checked = false;
                    this.DisconnectToolStripButton.Enabled = true;
                    this.DialpadToolStripDropDownButton.Enabled = true;
                    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case "dialing":
                    this.CallToolStripSplitButton.Enabled = false;
                    this.PickupToolStripButton.Enabled = false;
                    this.MuteToolStripButton.Enabled = false;
                    this.HoldToolStripButton.Enabled = false;
                    this.PickupToolStripButton.Checked = false;
                    this.MuteToolStripButton.Checked = this.IsMuted;
                    this.HoldToolStripButton.Checked = false;
                    this.DisconnectToolStripButton.Enabled = true;
                    this.DialpadToolStripDropDownButton.Enabled = true;
                    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case "proceeding":
                    this.CallToolStripSplitButton.Enabled = false;
                    this.PickupToolStripButton.Enabled = false;
                    this.MuteToolStripButton.Enabled = false;
                    this.HoldToolStripButton.Enabled = false;
                    this.PickupToolStripButton.Checked = false;
                    this.MuteToolStripButton.Checked = this.IsMuted;
                    this.HoldToolStripButton.Checked = false;
                    this.DisconnectToolStripButton.Enabled = true;
                    this.DialpadToolStripDropDownButton.Enabled = true;
                    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case "held":
                    if (this.IsDialingEnabled == true)
                    {
                        this.CallToolStripSplitButton.Enabled = true;
                    }
                    else
                    {
                        this.CallToolStripSplitButton.Enabled = false;
                    } 
                    this.PickupToolStripButton.Enabled = true;
                    this.MuteToolStripButton.Enabled = true;
                    this.HoldToolStripButton.Enabled = true;
                    this.DisconnectToolStripButton.Enabled = true;
                    this.DialpadToolStripDropDownButton.Enabled = true;
                    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case "mute":
                    if (this.IsDialingEnabled == true)
                    {
                        this.CallToolStripSplitButton.Enabled = true;
                    }
                    else
                    {
                        this.CallToolStripSplitButton.Enabled = false;
                    } 
                    this.PickupToolStripButton.Enabled = false;
                    this.MuteToolStripButton.Enabled = true;
                    this.HoldToolStripButton.Enabled = true;
                    this.DisconnectToolStripButton.Enabled = true;
                    this.DialpadToolStripDropDownButton.Enabled = true;
                    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case "connected":
                    if (this.IsDialingEnabled == true)
                    {
                        this.CallToolStripSplitButton.Enabled = true;
                    }
                    else
                    {
                        this.CallToolStripSplitButton.Enabled = false;
                    } 
                    this.PickupToolStripButton.Enabled = false;
                    this.MuteToolStripButton.Enabled = true;
                    this.HoldToolStripButton.Enabled = true;
                    this.PickupToolStripButton.Checked = false;
                    this.MuteToolStripButton.Checked = this.IsMuted;
                    this.HoldToolStripButton.Checked = false;
                    this.DisconnectToolStripButton.Enabled = true;
                    this.DialpadToolStripDropDownButton.Enabled = true;
                    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case "n/a":
                    if (this.IsDialingEnabled == true)
                    {
                        this.CallToolStripSplitButton.Enabled = true;
                    }
                    else
                    {
                        this.CallToolStripSplitButton.Enabled = false;
                    } 
                    this.PickupToolStripButton.Enabled = false;
                    this.MuteToolStripButton.Enabled = false;
                    this.HoldToolStripButton.Enabled = false;
                    this.PickupToolStripButton.Checked = false;
                    this.MuteToolStripButton.Checked = this.IsMuted;
                    this.HoldToolStripButton.Checked = false;
                    this.DisconnectToolStripButton.Enabled = false;
                    this.DialpadToolStripDropDownButton.Enabled = true;
                    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                default:
                    if (this.CallStateToolStripStatusLabel.Text.Substring(0, 3).ToUpper().Trim() == "ACD")
                    {
                        if (this.IsDialingEnabled == true)
                        {
                            this.CallToolStripSplitButton.Enabled = true;
                        }
                        else
                        {
                            this.CallToolStripSplitButton.Enabled = false;
                        } 
                        this.PickupToolStripButton.Enabled = false;
                        this.MuteToolStripButton.Enabled = true;
                        this.HoldToolStripButton.Enabled = true;
                        this.PickupToolStripButton.Checked = false;
                        this.MuteToolStripButton.Checked = this.IsMuted;
                        this.HoldToolStripButton.Checked = false;
                        this.DisconnectToolStripButton.Enabled = true;
                        this.DialpadToolStripDropDownButton.Enabled = true;
                        this.TelephonyToolStrip.BackColor = Color.Aqua;
                        this.TelephonyToolStrip.BackColor = OldColor;
                    }
                    else
                    {
                        if (this.IsDialingEnabled == true)
                        {
                            this.CallToolStripSplitButton.Enabled = true;
                        }
                        else
                        {
                            this.CallToolStripSplitButton.Enabled = false;
                        } 
                        this.PickupToolStripButton.Enabled = false;
                        this.MuteToolStripButton.Enabled = false;
                        this.HoldToolStripButton.Enabled = false;
                        this.PickupToolStripButton.Checked = false;
                        this.MuteToolStripButton.Checked = this.IsMuted;
                        this.HoldToolStripButton.Checked = false;
                        this.DisconnectToolStripButton.Enabled = false;
                        this.DialpadToolStripDropDownButton.Enabled = true;
                        this.TelephonyToolStrip.BackColor = Color.Aqua;
                        this.TelephonyToolStrip.BackColor = OldColor;
                    }
                    break;
            }
            Tracing.TraceStatus(scope + "Completed.");
        }

        private void EnabledDialerCallTools()
        {
            string scope = "CIC::frmMain::EnabledNormalCallTools()::";
            Tracing.TraceStatus(scope + "Starting.");
            Color OldColor = this.TelephonyToolStrip.BackColor;  //Save Original Trasparent Color
            if (this.ActiveConsultInteraction != null)
            {
                this.CancelTransferToolStripButton.Enabled = true;
            }
            else
            {
                this.CancelTransferToolStripButton.Enabled = false;
            }
            if (this.RequestBreakToolStripButton.Text.Trim() != "Break Pending")
            {
                this.RequestBreakToolStripButton.Enabled = true;
            }
            else
            {
                this.RequestBreakToolStripButton.Enabled = false;
            }
            switch (this.CallStateToolStripStatusLabel.Text.ToLower().Trim())
            {
                case "initializing":
                    this.DispositionToolStripButton.Enabled = false;
                    this.CallActivityCodeToolStripComboBox.Enabled = false;
                    this.PlaceCallToolStripButton.Enabled = true;
                    this.SkipCallToolStripButton.Enabled = true;
                    this.CallToolStripSplitButton.Enabled = false;
                    this.PickupToolStripButton.Enabled = false;
                    this.MuteToolStripButton.Enabled = false;
                    this.MuteToolStripButton.Checked = this.IsMuted;
                    this.HoldToolStripButton.Checked = false;
                    this.HoldToolStripButton.Enabled = false;
                    this.DisconnectToolStripButton.Enabled = false;
                    this.DialpadToolStripDropDownButton.Enabled = false;
                    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case "alerting":
                    this.DispositionToolStripButton.Enabled = true;
                    this.CallActivityCodeToolStripComboBox.Enabled = true;
                    this.PlaceCallToolStripButton.Enabled = false;
                    this.SkipCallToolStripButton.Enabled = false;
                    if (this.IsDialingEnabled == true)
                    {
                        this.CallToolStripSplitButton.Enabled = true;
                    }
                    else
                    {
                        this.CallToolStripSplitButton.Enabled = false;
                    }
                    this.PickupToolStripButton.Enabled = true;
                    this.MuteToolStripButton.Enabled = true;
                    this.HoldToolStripButton.Enabled = true;
                    this.MuteToolStripButton.Checked = this.IsMuted;
                    this.HoldToolStripButton.Checked = false;
                    this.SkipCallToolStripButton.Enabled = true;
                    this.DisconnectToolStripButton.Enabled = true;
                    this.DialpadToolStripDropDownButton.Enabled = true;
                    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case "messaging":
                    this.DispositionToolStripButton.Enabled = true;
                    //this.CallActivityCodeToolStripComboBox.SelectedIndex = -1;
                    this.CallActivityCodeToolStripComboBox.Enabled = true;
                    this.PlaceCallToolStripButton.Enabled = false;
                    this.SkipCallToolStripButton.Enabled = false;
                    if (this.IsDialingEnabled == true)
                    {
                        this.CallToolStripSplitButton.Enabled = true;
                    }
                    else
                    {
                        this.CallToolStripSplitButton.Enabled = false;
                    }
                    this.PickupToolStripButton.Enabled = true;
                    this.MuteToolStripButton.Enabled = true;
                    this.HoldToolStripButton.Enabled = true;
                    this.MuteToolStripButton.Checked = this.IsMuted;
                    this.HoldToolStripButton.Checked = false;
                    this.SkipCallToolStripButton.Enabled = true;
                    this.DisconnectToolStripButton.Enabled = true;
                    this.DialpadToolStripDropDownButton.Enabled = true;
                    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case "offering":
                    this.DispositionToolStripButton.Enabled = true;
                    this.CallActivityCodeToolStripComboBox.Enabled = true;
                    this.PlaceCallToolStripButton.Enabled = false;
                    this.SkipCallToolStripButton.Enabled = false;
                    if (this.IsDialingEnabled == true)
                    {
                        this.CallToolStripSplitButton.Enabled = true;
                    }
                    else
                    {
                        this.CallToolStripSplitButton.Enabled = false;
                    }
                    this.PickupToolStripButton.Enabled = true;
                    this.MuteToolStripButton.Enabled = true;
                    this.HoldToolStripButton.Enabled = true;
                    this.MuteToolStripButton.Checked = this.IsMuted;
                    this.HoldToolStripButton.Checked = false;
                    this.DisconnectToolStripButton.Enabled = true;
                    this.DialpadToolStripDropDownButton.Enabled = true;
                    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case "dialing":
                    this.DispositionToolStripButton.Enabled = true;
                    this.CallActivityCodeToolStripComboBox.Enabled = true;
                    this.PlaceCallToolStripButton.Enabled = false;
                    this.SkipCallToolStripButton.Enabled = false;
                    this.CallToolStripSplitButton.Enabled = false;
                    this.PickupToolStripButton.Enabled = false;
                    this.MuteToolStripButton.Enabled = false;
                    this.HoldToolStripButton.Enabled = false;
                    this.MuteToolStripButton.Checked = this.IsMuted;
                    this.HoldToolStripButton.Checked = false;
                    this.DisconnectToolStripButton.Enabled = true;
                    this.DialpadToolStripDropDownButton.Enabled = false;
                    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case "proceeding":
                    this.DispositionToolStripButton.Enabled = true;
                    this.CallActivityCodeToolStripComboBox.Enabled = true;
                    this.PlaceCallToolStripButton.Enabled = false;
                    this.SkipCallToolStripButton.Enabled = false;
                    this.CallToolStripSplitButton.Enabled = false;
                    this.PickupToolStripButton.Enabled = false;
                    this.MuteToolStripButton.Enabled = false;
                    this.HoldToolStripButton.Enabled = false;
                    this.MuteToolStripButton.Checked = this.IsMuted;
                    this.HoldToolStripButton.Checked = false;
                    this.DisconnectToolStripButton.Enabled = true;
                    this.DialpadToolStripDropDownButton.Enabled = false;
                    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case "held":
                    this.DispositionToolStripButton.Enabled = true;
                    this.CallActivityCodeToolStripComboBox.Enabled = true;
                    this.PlaceCallToolStripButton.Enabled = false;
                    this.SkipCallToolStripButton.Enabled = false;
                    if (this.IsDialingEnabled == true)
                    {
                        this.CallToolStripSplitButton.Enabled = true;
                    }
                    else
                    {
                        this.CallToolStripSplitButton.Enabled = false;
                    }
                    this.PickupToolStripButton.Enabled = true;
                    this.MuteToolStripButton.Enabled = true;
                    this.HoldToolStripButton.Enabled = true;
                    this.MuteToolStripButton.Checked = this.IsMuted;
                    this.HoldToolStripButton.Checked = true;
                    this.DisconnectToolStripButton.Enabled = true;
                    this.DialpadToolStripDropDownButton.Enabled = true;
                    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case "mute":
                    this.DispositionToolStripButton.Enabled = true;
                    this.CallActivityCodeToolStripComboBox.Enabled = true;
                    this.PlaceCallToolStripButton.Enabled = false;
                    this.SkipCallToolStripButton.Enabled = false;
                    if (this.IsDialingEnabled == true)
                    {
                        this.CallToolStripSplitButton.Enabled = true;
                    }
                    else
                    {
                        this.CallToolStripSplitButton.Enabled = false;
                    }
                    this.PickupToolStripButton.Enabled = true;
                    this.MuteToolStripButton.Enabled = true;
                    this.HoldToolStripButton.Enabled = true;
                    this.MuteToolStripButton.Checked = this.IsMuted;
                    this.HoldToolStripButton.Checked = false;
                    this.DisconnectToolStripButton.Enabled = true;
                    this.DialpadToolStripDropDownButton.Enabled = true;
                    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case "connected":
                    this.DispositionToolStripButton.Enabled = true;
                    this.CallActivityCodeToolStripComboBox.Enabled = true;
                    this.PlaceCallToolStripButton.Enabled = false;
                    this.SkipCallToolStripButton.Enabled = false;
                    if (this.IsDialingEnabled == true)
                    {
                        this.CallToolStripSplitButton.Enabled = true;
                    }
                    else
                    {
                        this.CallToolStripSplitButton.Enabled = false;
                    }
                    this.PickupToolStripButton.Enabled = false;
                    this.MuteToolStripButton.Enabled = true;
                    this.HoldToolStripButton.Enabled = true;
                    this.MuteToolStripButton.Checked = this.IsMuted;
                    this.HoldToolStripButton.Checked = false;
                    this.DisconnectToolStripButton.Enabled = true;
                    this.DialpadToolStripDropDownButton.Enabled = true;
                    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                case "n/a":
                    this.DispositionToolStripButton.Enabled = false;
                    this.CallActivityCodeToolStripComboBox.Enabled = false;
                    this.PlaceCallToolStripButton.Enabled = false;
                    this.SkipCallToolStripButton.Enabled = false;
                    this.CallToolStripSplitButton.Enabled = false;
                    this.PickupToolStripButton.Enabled = false;
                    this.MuteToolStripButton.Enabled = false;
                    this.HoldToolStripButton.Enabled = false;
                    this.MuteToolStripButton.Checked = this.IsMuted;
                    this.HoldToolStripButton.Checked = false;
                    this.DisconnectToolStripButton.Enabled = false;
                    this.DialpadToolStripDropDownButton.Enabled = false;
                    this.TelephonyToolStrip.BackColor = Color.Aqua;
                    this.TelephonyToolStrip.BackColor = OldColor;
                    break;
                default:
                    if (this.CallStateToolStripStatusLabel.Text.ToLower().Trim().Substring(0, 3).Equals("acd"))
                    {
                        this.DispositionToolStripButton.Enabled = true;
                        this.CallActivityCodeToolStripComboBox.Enabled = true;
                        this.PlaceCallToolStripButton.Enabled = false;
                        this.SkipCallToolStripButton.Enabled = false;
                        this.CallToolStripSplitButton.Enabled = false;
                        this.PickupToolStripButton.Enabled = false;
                        this.MuteToolStripButton.Enabled = false;
                        this.HoldToolStripButton.Enabled = false;
                        this.MuteToolStripButton.Checked = this.IsMuted;
                        this.HoldToolStripButton.Checked = false;
                        this.DisconnectToolStripButton.Enabled = true;
                        this.DialpadToolStripDropDownButton.Enabled = false;
                        this.TelephonyToolStrip.BackColor = Color.Aqua;
                        this.TelephonyToolStrip.BackColor = OldColor;
                    }
                    else if (this.CallStateToolStripStatusLabel.Text.ToLower().Trim().Substring(0, 10).Equals("disconnect"))
                    {
                        this.DispositionToolStripButton.Enabled = true;
                        this.CallActivityCodeToolStripComboBox.Enabled = true;
                        this.PlaceCallToolStripButton.Enabled = false;
                        this.SkipCallToolStripButton.Enabled = false;
                        this.CallToolStripSplitButton.Enabled = false;
                        this.PickupToolStripButton.Enabled = false;
                        this.MuteToolStripButton.Enabled = false;
                        this.HoldToolStripButton.Enabled = false;
                        this.MuteToolStripButton.Checked = this.IsMuted;
                        this.HoldToolStripButton.Checked = false;
                        this.DisconnectToolStripButton.Enabled = false;
                        this.DialpadToolStripDropDownButton.Enabled = false;
                        this.TelephonyToolStrip.BackColor = Color.Aqua;
                        this.TelephonyToolStrip.BackColor = OldColor;
                    }
                    else
                    {
                        this.DispositionToolStripButton.Enabled = false;
                        this.CallActivityCodeToolStripComboBox.Enabled = false;
                        this.PlaceCallToolStripButton.Enabled = false;
                        this.SkipCallToolStripButton.Enabled = false;
                        this.CallToolStripSplitButton.Enabled = false;
                        this.PickupToolStripButton.Enabled = false;
                        this.MuteToolStripButton.Enabled = false;
                        this.HoldToolStripButton.Enabled = false;
                        this.MuteToolStripButton.Checked = this.IsMuted;
                        this.HoldToolStripButton.Checked = false;
                        this.DisconnectToolStripButton.Enabled = false;
                        this.DialpadToolStripDropDownButton.Enabled = false;
                        this.TelephonyToolStrip.BackColor = Color.Aqua;
                        this.TelephonyToolStrip.BackColor = OldColor;
                    }
                    break;
            }
            Tracing.TraceStatus(scope + "Completed.");
        }

        private string GetStringSection(string allString, string startKeyString, string endKeyString)
        {
            string scope = "CIC::MainForm::CrmScreenPop()::";
            Tracing.TraceStatus(scope + "Starting.");
            string sRet = "";
            int KeyPoint = 0;
            try
            {
                if (allString != null)
                {
                    if (allString != String.Empty)
                    {
                        if ((startKeyString != null) && (endKeyString != null))
                        {
                            KeyPoint = allString.IndexOf(startKeyString);
                            if ((KeyPoint > 0) && (KeyPoint < allString.Length))
                            {
                                allString = allString.Substring(KeyPoint, allString.Length - KeyPoint);
                                KeyPoint = allString.IndexOf("&");
                                allString = allString.Substring(0, KeyPoint);
                                sRet = allString;
                            }
                        }
                    }
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
            return sRet;
        }

        private void CrmScreenPop_RefreshSession()
        {
            string scope = "CIC::MainForm::CrmScreenPop_RefreshSession()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (Properties.Settings.Default.PrePopUrl != null)
                {
                    if (global::CIC.Properties.Settings.Default.PrePopUrl.ToString().Trim() != "")
                    {
                        if (this.crmSID != null)
                        {
                            if (this.crmSID.Trim() != "")
                            {
                                string sLogOut = global::CIC.Properties.Settings.Default.PrePopUrl + this.crmSID;
                                sLogOut = sLogOut.Replace("??", "?");
                                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Url Info :" + sLogOut, System.Diagnostics.EventLogEntryType.Information); //Window Event Log
                                this.MainWebBrowser.Url = new System.Uri(sLogOut, System.UriKind.Absolute);
                            }
                            else
                            {
                                //Don't do any thing
                            }
                        }
                    }
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                this.MainWebBrowser.Url = new System.Uri(global::CIC.Properties.Settings.Default.StartupUrl, System.UriKind.Absolute);
            }
        }

        private void CrmScreenPop_SetMainEntry(string FullyUrl)
        {
            string scope = "CIC::MainForm::CrmScreenPop_SetMainEntry()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (FullyUrl != null)
                {
                    if (FullyUrl.Trim() != "")
                    {
                        this.MainWebBrowser.Url = new System.Uri(FullyUrl, System.UriKind.Absolute);
                    }
                }
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Url Info :" + FullyUrl, System.Diagnostics.EventLogEntryType.Information); //Window Event Log
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                this.MainWebBrowser.Url = new System.Uri(global::CIC.Properties.Settings.Default.StartupUrl, System.UriKind.Absolute);
            }
        }

        private void CrmScreenPop_RetriveLastSessionID()
        {
            string scope = "CIC::MainForm::CrmScreenPop_SetMainEntry()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (this.MainWebBrowser.DocumentText != null)
                {
                    if (this.MainWebBrowser.DocumentText != "")
                    {
                        string sDoc = this.MainWebBrowser.DocumentText;
                        this.crmSID = this.GetStringSection(sDoc, global::CIC.Properties.Settings.Default.startSearchKey, global::CIC.Properties.Settings.Default.endSearchKey);
                    }
                    else
                    {
                        this.crmSID = "";
                    }
                    Tracing.TraceStatus(scope + "Completed.");
                }
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                this.MainWebBrowser.Url = new System.Uri(global::CIC.Properties.Settings.Default.StartupUrl, System.UriKind.Absolute);
            }
        }

        private void CrmScreenPop_RetriveIVRValue()
        {
            string scope = "CIC::MainForm::CrmScreenPop_RetriveIVRValue()::";
            Tracing.TraceStatus(scope + "Starting.");
            try
            {
                if (this.ActiveNormalInteraction != null)
                {
                    //System.Uri Url = new System.Uri(global::CIC.Properties.Settings.Default.StartupUrl, System.UriKind.Absolute);
                    string sUsrSelect = this.ActiveNormalInteraction.GetStringAttribute("sUsrSelect");
                    //string Eic_RemoteTnRaw = this.ActiveNormalInteraction.GetStringAttribute("Eic_RemoteTnRaw");
                    //string sCustID = this.ActiveNormalInteraction.GetStringAttribute("sCustID");
                    //MessageBox.Show("Cus ID : "+sCustID);
                    this.sCollectUserSelect = this.ActiveNormalInteraction.GetStringAttribute("sCollectUserSelect");
                    if (global::CIC.Properties.Settings.Default.DisplayIVR_Screen == true)
                    {
                        if (this.sCollectUserSelect != null)
                        {
                            if (this.sCollectUserSelect.Trim() != "")
                            {
                                if (this.IVRMenuList != null)
                                {
                                    this.IVRMenuList.Close();
                                }
                                this.IVRMenuList = new frmIVRList(this.sCollectUserSelect);
                                this.IVRMenuList.Show();
                            }
                        }
                    }
                    if (sUsrSelect != null)
                    {
                        this.WebBrowserStatusToolStripStatusLabel.Text = sUsrSelect;
                    }
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
        }

        private void CrmScreenPop_SetACDEntry()
        {
            string scope = "CIC::MainForm::CrmScreenPop_SetACDEntry()::";
            Tracing.TraceStatus(scope + "Starting.");
            string FieldValue = "";
            string FieldATTR = global::CIC.Properties.Settings.Default.InbdScreenPop_FieldList_ATTR;
            try
            {
                if (this.ActiveNormalInteraction != null)
                {
                    if (FieldATTR.Trim() != "")
                    {
                        if (this.ActiveNormalInteraction.GetStringAttribute(FieldATTR) != "")
                        {
                            string sDoc1 = "";
                            string strCallId = "";
                            if (this.MainWebBrowser.DocumentText != null)
                            {
                                sDoc1 = this.MainWebBrowser.DocumentText;
                                if (sDoc1.Trim() != "")
                                {
                                    this.crmSID = this.GetStringSection(sDoc1, global::CIC.Properties.Settings.Default.startSearchKey, global::CIC.Properties.Settings.Default.endSearchKey);
                                    FieldValue = this.ActiveNormalInteraction.GetStringAttribute(FieldATTR);
                                    strCallId = this.ActiveNormalInteraction.CallIdKey.ToString().Trim(); 
                                    if (global::CIC.Properties.Settings.Default.PopUrl != null)
                                    {
                                        string sUrl = null;
                                        if (global::CIC.Properties.Settings.Default.CallerSupports == true)
                                        {
                                            sUrl = System.String.Format(global::CIC.Properties.Settings.Default.PopUrl.ToString(), strCallId, FieldValue);
                                        }
                                        else
                                        {
                                           sUrl = System.String.Format(global::CIC.Properties.Settings.Default.PopUrl.ToString(), FieldValue);
                                        }
                                        System.Uri popUrl = new System.Uri(sUrl, System.UriKind.Absolute);
                                        if (popUrl != null)
                                        {
                                            //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Url Info :" + sUrl, System.Diagnostics.EventLogEntryType.Information); //Window Event Log
                                            this.MainWebBrowser.Url = popUrl;
                                        }
                                        else
                                        {
                                            //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Url Info :" + sUrl, System.Diagnostics.EventLogEntryType.Information); //Window Event Log
                                            this.CrmScreenPop_SetStartupUrl();
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //this.CrmScreenPop_SetStartupUrl();
                        }

                    }
                    else
                    {
                        //this.CrmScreenPop_SetStartupUrl();
                    }
                }
                else
                {
                    //this.CrmScreenPop_SetStartupUrl();
                }
                Tracing.TraceStatus(scope + "Completed.");
            }
            catch (System.Exception ex)
            {
                Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                this.MainWebBrowser.Url = new System.Uri(global::CIC.Properties.Settings.Default.StartupUrl, System.UriKind.Absolute);
            }
        }

        private void CrmScreenPop_SetStartupUrl()
        {
            if (global::CIC.Properties.Settings.Default.StartupUrl != null)
            {
                if (Properties.Settings.Default.StartupUrl != null)
                {
                    if (Properties.Settings.Default.AutoCRMSignOn == true)
                    {
                        string sUrl = System.String.Format(Properties.Settings.Default.StartupUrl.ToString(), global::CIC.Program.mLoginParam.UserId, global::CIC.Program.mLoginParam.Password);
                        this.MainWebBrowser.Url = new System.Uri(sUrl, System.UriKind.Absolute);
                    }
                    else
                    {
                        this.MainWebBrowser.Url = new System.Uri(Properties.Settings.Default.StartupUrl, System.UriKind.Absolute);
                    }
                }
            }
        }

        private void CrmScreenPop_WriteUrlLog(string sUrl)
        {
            string scope = "CIC::MainForm::CrmScreenPop_WriteUrlLog()::";
            Tracing.TraceStatus(scope + "Starting.");
            if (sUrl != null)
            {
                if (sUrl.Trim() != "")
                {
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Url Info :" + sUrl, System.Diagnostics.EventLogEntryType.Information); //Window Event Log
                }
                else
                {
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Url Info :Null or Empty URL to popup.", System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                }
            }
            else
            {
                System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Url Info :Null or Empty URL to popup.", System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            }
            Tracing.TraceStatus(scope + "Completed.");

        }

        private void CrmScreenPop()
        {
            string scope = "CIC::MainForm::CrmScreenPop()::";
            Tracing.TraceStatus(scope + "Starting.");
            string FullyUrl = "";
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new MethodInvoker(CrmScreenPop));
            }
            else
            {
                try
                {
                    switch (this.IsLoggedIntoDialer)
                    {
                        case true:
                            FullyUrl = this.GetFullyScreenUrl();
                            this.CrmScreenPop_RefreshSession();
                            this.CrmScreenPop_SetMainEntry(FullyUrl);
                            this.CrmScreenPop_WriteUrlLog(FullyUrl);
                            this.CrmScreenPop_RetriveLastSessionID();
                            break;
                        default:
                            if (Properties.Settings.Default.StartupUrl != null)
                            {
                                this.CrmScreenPop_RetriveIVRValue();
                                this.CrmScreenPop_SetACDEntry();
                                if (this.MainWebBrowser.Url != null)
                                {
                                    this.CrmScreenPop_WriteUrlLog(this.MainWebBrowser.Url.ToString());
                                }
                                else
                                {
                                    this.CrmScreenPop_WriteUrlLog("");
                                }

                            }
                            break;
                    }
                    Tracing.TraceStatus(scope + "Completed.");
                }
                catch (System.Exception ex)
                {
                    Tracing.TraceStatus(scope + "Error info : " + ex.Message);
                    System.Diagnostics.EventLog.WriteEntry(Application.ProductName, scope + "Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                    this.MainWebBrowser.Url = new System.Uri(global::CIC.Properties.Settings.Default.StartupUrl, System.UriKind.Absolute);
                }
            }
        }

        #endregion

    }
}
