using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CIC
{
  public partial class ScheduleCallbackForm : Form
  {
    private CIC.Utils.CallbackType m_CallbackType;
    private DateTime m_CallbackDateTime;
    private string m_ScheduledNumber;
    private bool ScheduleCallbackFormResult = false;

    public bool ScheduleCallbackResult
    {
        get
        {
            return this.ScheduleCallbackFormResult;
        }
    }

    public ScheduleCallbackForm(string defaultNumber)
    {
      InitializeComponent();
      this.m_ScheduledNumber = defaultNumber;
      this.ScheduleNumberTextBox.Text = defaultNumber;
      this.OffsetTimeComboBox.SelectedIndex = 0;
      this.ShowScheduledCallbackDateTime();
      if (this.MyselfRadioButton.Checked == true)
      {
          this.m_CallbackType = CIC.Utils.CallbackType.OwnAgent;
      }
      if (this.GroupRadioButton.Checked == true)
      {
          this.m_CallbackType = CIC.Utils.CallbackType.CampaignWide;
      }
    }

    private void AnotherDayRadioButton_CheckedChanged(object sender, EventArgs e)
    {
      this.AnotherDayDatePicker.Enabled = this.AnotherDayRadioButton.Checked;
      this.AnotherDayTimePicker.Enabled = this.AnotherDayRadioButton.Checked;
      this.ShowScheduledCallbackDateTime();
      this.AnotherDayDatePicker.Focus();
    }

    public DateTime CallbackDateTime
    {
      get 
      {
          return this.m_CallbackDateTime;
      }
    }

    public CIC.Utils.CallbackType CallbackType
    {
      get 
      { 
          return this.m_CallbackType; 
      }
    }

    public string ScheduledNumber
    {
      get
      {
          return this.m_ScheduledNumber;
      }
    }

    private void OffsetTimeRadioButton_CheckedChanged(object sender, EventArgs e)
    {
      this.OffsetTimeComboBox.Enabled = this.OffsetTimeRadioButton.Checked;
      this.ShowScheduledCallbackDateTime();
      this.OffsetTimeComboBox.Focus();
    }

    private void SpecificTimeRadioButton_CheckedChanged(object sender, EventArgs e)
    {
      this.SpecificTimeDateTimePicker.Enabled = SpecificTimeRadioButton.Checked;
      this.ShowScheduledCallbackDateTime();
      this.SpecificTimeDateTimePicker.Focus();
    }

    private void TodayRadioButton_CheckedChanged(object sender, EventArgs e)
    {
      if (this.TodayRadioButton.Checked == true)
      {
        this.OffsetTimeRadioButton.Enabled = true;
        this.SpecificTimeRadioButton.Enabled = true;
        this.OffsetTimeComboBox.Enabled = this.OffsetTimeRadioButton.Checked;
        this.SpecificTimeDateTimePicker.Enabled = this.SpecificTimeRadioButton.Checked;
        this.ShowScheduledCallbackDateTime();
        if (this.OffsetTimeRadioButton.Checked == true)
        {
            this.OffsetTimeComboBox.Focus();
        }
        if (this.SpecificTimeRadioButton.Checked == true)
        {
            this.SpecificTimeDateTimePicker.Focus();
        }
      }
      else
      {
        OffsetTimeRadioButton.Enabled = false;
        OffsetTimeComboBox.Enabled = false;
        SpecificTimeRadioButton.Enabled = false;
        SpecificTimeDateTimePicker.Enabled = false;
      }
    }

    private void ShowScheduledCallbackDateTime()
    {
        if (this.TodayRadioButton.Checked == true)
        {
            if (this.OffsetTimeRadioButton.Checked == true)
            {
                switch (this.OffsetTimeComboBox.SelectedItem.ToString().ToLower())
                {
                    case "15 minutes later":
                        this.m_CallbackDateTime = DateTime.Now.AddMinutes(15);
                        break;
                    case "30 minutes later":
                        this.m_CallbackDateTime = DateTime.Now.AddMinutes(30);
                        break;
                    case "1 hour later":
                        this.m_CallbackDateTime = DateTime.Now.AddHours(1);
                        break;
                    case "2 hours later":
                        this.m_CallbackDateTime = DateTime.Now.AddHours(2);
                        break;
                    case "3 hours later":
                        this.m_CallbackDateTime = DateTime.Now.AddHours(3);
                        break;
                    case "4 hours later":
                        this.m_CallbackDateTime = DateTime.Now.AddHours(4);
                        break;
                    default:
                        this.m_CallbackDateTime = DateTime.Now;
                        break;
                }
            }
            else
            {
                DateTime today = DateTime.Today;
                this.m_CallbackDateTime = new DateTime(today.Year, today.Month, today.Day, this.SpecificTimeDateTimePicker.Value.Hour, this.SpecificTimeDateTimePicker.Value.Minute, this.SpecificTimeDateTimePicker.Value.Second);
            }
        }
        else
        {
            this.m_CallbackDateTime = new DateTime(this.AnotherDayDatePicker.Value.Year, this.AnotherDayDatePicker.Value.Month, this.AnotherDayDatePicker.Value.Day, this.AnotherDayTimePicker.Value.Hour, this.AnotherDayTimePicker.Value.Minute, this.AnotherDayTimePicker.Value.Second);
        }
      this.CallbackScheduledDateTimeLabel.Text = this.m_CallbackDateTime.ToString("dddd, MMMM d HH:mm");
    }

    private void AnotherDayDatePicker_ValueChanged(object sender, EventArgs e)
    {
      this.ShowScheduledCallbackDateTime();
    }

    private void AnotherDayTimePicker_ValueChanged(object sender, EventArgs e)
    {
      this.ShowScheduledCallbackDateTime();
    }

    private void OffsetTimeComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      this.ShowScheduledCallbackDateTime();
    }

    private void SpecificTimeDateTimePicker_ValueChanged(object sender, EventArgs e)
    {
      this.ShowScheduledCallbackDateTime();
    }

    private void MyselfRadioButton_CheckedChanged(object sender, EventArgs e)
    {
        if (this.MyselfRadioButton.Checked == true)
        {
            this.m_CallbackType = CIC.Utils.CallbackType.OwnAgent;
        }
    }

    private void GroupRadioButton_CheckedChanged(object sender, EventArgs e)
    {
        if (this.GroupRadioButton.Checked == true)
        {
            this.m_CallbackType = CIC.Utils.CallbackType.CampaignWide;
        }
    }

    private void ScheduleNumberTextBox_TextChanged(object sender, EventArgs e)
    {
      this.m_ScheduledNumber = this.ScheduleNumberTextBox.Text;
    }

    private void ScheduleCallbackForm_Load(object sender, EventArgs e)
    {
        //
    }

    private void OKButton_Click(object sender, EventArgs e)
    {
        this.ScheduleCallbackFormResult = true;
    }

    private void CancelScheduleButton_Click(object sender, EventArgs e)
    {
        this.ScheduleCallbackFormResult = false;
    }
  }


}