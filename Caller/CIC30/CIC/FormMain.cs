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
    public enum FormMainState
    {
        Preview,
        Calling,
        Disconnect,
        Hold,
        Mute,
        Break,
        ManualCall,
        None,

    };
    //private bool IsLoggedIntoDialer = false;

    public partial class FormMain : Form
    {
        private bool break_requested;
        
        private FormMainState prev_state = FormMainState.Preview;
        private static Interaction ActiveNormalInteration { get; set; }

        private float timer;
        private string calling_phone = "0881149998";

        public bool transfer_complete = false;
 
        public FormMainState req_state_change = FormMainState.None;
       
        public FormMain()
        {
            InitializeComponent();
        }

        private void reset_timer()
        {
            timer1.Stop();
            timer = 10.0f;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer = 10.0f;
            state_info_label.Text = "Next Calling Number: " + calling_phone;
        }

        private void workflow_button_Click(object sender, EventArgs e)
        {
            frmWorkflow workflow = new CIC.frmWorkflow();
            workflow.Show();
        }

        private void call_button_Click(object sender, EventArgs e)
        {
            //change state
            name1_panel.BackColor = Color.Yellow;
            
            reset_timer();
            state_info_label.Text = "Calling: " + calling_phone;

            state_change(FormMainState.Calling);
        }

        private void disconnect_button_Click(object sender, EventArgs e)
        {
            state_info_label.Text = "Disconnected from: " + calling_phone;
            frmDisposition disposition = new frmDisposition();
            disposition.Show();
            state_change(FormMainState.Disconnect);
        }

        private void hold_button_Click(object sender, EventArgs e)
        {
            state_change(FormMainState.Hold);
        }

        private void mute_button_Click(object sender, EventArgs e)
        {
            state_change(FormMainState.Mute);
        }

        private void transfer_button_Click(object sender, EventArgs e)
        {
            transfer_complete = false;
            frmTransfer transfer = new frmTransfer();
            transfer.Show();
            // check if there is still a connection or if transfer complete.
            if (transfer_complete)
            {
                state_change(FormMainState.Disconnect);
            }
        }

        private void conference_button_Click(object sender, EventArgs e)
        {

        }

        private void manual_call_button_Click(object sender, EventArgs e)
        {
            frmManualCall manualCall = new frmManualCall();
            manualCall.FormClosed += manual_call_closed;
            manualCall.Show();
            
            state_change(FormMainState.ManualCall);
        }

        private void manual_call_closed(object sender, EventArgs e)
        {
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
            }
            prev_state = state;
            req_state_change = FormMainState.None;
        }

        private void reset_state()
        {
            workflow_button.Enabled = false;
            call_button.Enabled = false;
            disconnect_button.Enabled = false;
            hold_button.Enabled = false;
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
            transfer_button.Enabled = true;
            conference_button.Enabled = true;
            manual_call_button.Enabled = true;
            break_button.Enabled = true;
            endbreak_button.Enabled = true;
            logout_workflow_button.Enabled = true;
            exit_button.Enabled = true;
        }

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
            enable_all_button();
            disconnect_button.Enabled = false;

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

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer -= (float)timer1.Interval / 1000;
            timer_info.Text = "Time until call: " + timer.ToString("F1");
            if (timer <= 0)
            {
                reset_timer();
                state_change(FormMainState.Calling);
                state_info_label.Text = "Calling: " + calling_phone;
            }
        }

        public static void MakeCallCompleted(object sender,InteractionCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                //
            }
            else
            {
                ActiveNormalInteration = e.Interaction;
            }
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
    }
}
