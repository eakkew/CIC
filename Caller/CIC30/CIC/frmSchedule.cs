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
    public partial class frmSchedule : Form
    {
        private static frmSchedule instance = null;
        private DateTime scheduled;
        private bool isCanceled;

        public static frmSchedule getInstance(string dialNumber)
        {
            if (instance == null || instance.IsDisposed)
            {
                instance = new frmSchedule(dialNumber);
            }
            return instance;
        }

        private frmSchedule()
        {
            InitializeComponent();
        }

        private frmSchedule(string dialNumber)
        {
            InitializeComponent();
            initializeComboboxInfo();
            phone_box.Text = dialNumber;
            isCanceled = false;
        }

        private void initializeComboboxInfo()
        {
            hour_combobox.Items.Clear();
            minute_combobox.Items.Clear();
            for (int hour = 0; hour < 24; ++hour)
            {
                hour_combobox.Items.Add(hour);
            }
            for (int min = 0; min < 60; ++min)
            {
                minute_combobox.Items.Add(min);
            }
            hour_combobox.SelectedIndex = 0;
            minute_combobox.SelectedIndex = 0;
        }

        private void save_button_Click(object sender, EventArgs e)
        {
            isCanceled = false;
            if (!validateTime())
            {
                minute_combobox.ForeColor = Color.Red;
                hour_combobox.ForeColor = Color.Red;
            }
            else
            {
                minute_combobox.ForeColor = Color.Black;
                hour_combobox.ForeColor = Color.Black;
                this.Close();
            }

        }

        private void hour_combobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DateTime today = DateTime.Now;
            int hour = (hour_combobox.Text != "") ? int.Parse(hour_combobox.Text) : 0;
            int minute = (minute_combobox.Text != "") ? int.Parse(minute_combobox.Text) : 0;
            // incase you want incremental time
            //scheduled = DateTime.Now;
            //scheduled = scheduled.AddHours(hour);
            //scheduled = scheduled.AddMinutes(minute);
            scheduled = new DateTime(today.Year, today.Month, today.Day, hour, minute, 0);

            if (validateTime())
            {
                minute_combobox.ForeColor = Color.Black;
                hour_combobox.ForeColor = Color.Black;
            }
            else
            {
                minute_combobox.ForeColor = Color.Red;
                hour_combobox.ForeColor = Color.Red;
            }
        }

        private void minute_combobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DateTime today = DateTime.Now;
            int hour = (hour_combobox.Text != "") ? int.Parse(hour_combobox.Text) : 0;
            int minute = (minute_combobox.Text != "") ? int.Parse(minute_combobox.Text) : 0;
            // incase you want incremental time
            //scheduled = DateTime.Now;
            //scheduled = scheduled.AddHours(hour);
            //scheduled = scheduled.AddMinutes(minute);
            scheduled = new DateTime(today.Year, today.Month, today.Day, hour, minute, 0);

            if (validateTime())
            {
                minute_combobox.ForeColor = Color.Black;
                hour_combobox.ForeColor = Color.Black;
            }
            else
            {
                minute_combobox.ForeColor = Color.Red;
                hour_combobox.ForeColor = Color.Red;
            }
        }

        public bool validateTime()
        {
            DateTime today = DateTime.Now;
            return (scheduled > today && !isCanceled);
        }

        private void frmSchedule_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            this.isCanceled = true;
            this.Close();
        }

        public DateTime getScheduledTime()
        {
            return scheduled;
        }

    }
}
