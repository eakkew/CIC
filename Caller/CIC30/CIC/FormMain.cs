using ININ.IceLib.Interactions;
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
    private enum CallerType
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
        private bool IsLoggedIntoDialer { get; set; }
        private bool IsActiveConnection { get; set; }

        private ICWorkFlow IcWorkFlow = null;
        private ININ.IceLib.Connection.Session IC_Session = null;
        private ININ.IceLib.Dialer.DialerCallInteraction ActiveDialerInteraction = null;
        private ININ.IceLib.Interactions.Interaction ActiveNormalInteraction = null;
        private ININ.IceLib.Dialer.DialerSession DialerSession = null;
        private ININ.IceLib.Interactions.InteractionsManager NormalInterationManager = null;
        private FormMainState prev_state = FormMainState.Preview;
        private FormMainState current_state = FormMainState.Preview;
        private static Interaction ActiveNormalInteration { get; set; }

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
            switch (current_state)
            {   
                // case calling state -> change to hold state
                case FormMainState.Calling:
                    state_info_label.Text = "Hold call from: " + calling_phone;
                    state_change(FormMainState.Hold);
                    break;
                // case Mute state -> change to hold state.
                case FormMainState.Mute:
                    state_info_label.Text = "Hold call from: " + calling_phone;
                    state_change(FormMainState.Hold);
                    break;
                // case Hold state -> change to calling state
                case FormMainState.Hold:
                    state_info_label.Text = "Continue call from: " + calling_phone;
                    state_change(FormMainState.Calling);
                    break;
            }
                
        }

        private void mute_button_Click(object sender, EventArgs e)
        {
            switch (current_state)
            {
                // case calling state -> change to mute state
                case FormMainState.Calling:
                    state_info_label.Text = "Mute call from: " + calling_phone;
                    state_change(FormMainState.Mute);
                    break;
                // case Mute state -> change to mute state.
                case FormMainState.Hold:
                    state_info_label.Text = "Mute call from: " + calling_phone;
                    state_change(FormMainState.Mute);
                    break;
                // case Hold state -> change back to calling state
                case FormMainState.Mute:
                    state_info_label.Text = "Continue call from: " + calling_phone;
                    state_change(FormMainState.Calling);
                    break;
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
            if (ActiveNormalInteration != null)
            {
                ActiveNormalInteration.Disconnect();
                ActiveNormalInteration = null;
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
                    this.Initial_ActityCodes();

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
            // TODO: copy function from frmMain to here
        }

        private void InitializeStatusMessageDetails()
        {
            // TODO: copy function from frmMain to here
        }

        private void Initial_ActityCodes()
        {
            // TODO: copy function from frmMain to here
        }

        private void RegisterHandlers()
        {
            // TODO: copy function from frmMain to here
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
                    hold_state();
                    break;
                case FormMainState.Mute:
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
                placecall();
                
                state_change(FormMainState.Calling);
                state_info_label.Text = "Calling: " + calling_phone;
            }
        }

        public static void MakeCallCompleted(object sender,InteractionCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                ActiveNormalInteration = e.Interaction;
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
            
            if (IsDialerInteractionAvailableForPickup())
            {
                pickup();
                return;
            }
            else placecall();
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
                            if (ActiveNormalInteration != null)
                            {
                                ActiveNormalInteration.Disconnect();
                                ActiveNormalInteration = null;
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
        
        /****************************************************
        *****************************************************
        ******************* The Logic Part ******************
        *****************************************************
        ****************************************************/
        private void placecall()
        {
            // Src: PlaceCallToolStripButton_Click()
            // string scope = "CIC::MainForm::PlaceCallToolStripButton_Click(): ";
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new EventHandler<EventArgs>(PlaceCallToolStripButton_Click), new object[] { sender, e });
            }
            else
            {
                try
                {
                    // Tracing.TraceStatus(scope + "Starting.[Place Call]");
                    if (this.ActiveDialerInteration != null)
                    {
                        this.ActiveDialerInteration.PlacePreviewCall();
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
            //TODO to be implemented.
            // PickupToolStripButton_Click()
        }
    }
}
