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
        private string dialerNumber;

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
            dialerNumber = dialNumber;
            this.ActiveControl = phone_box;
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
            if (!validateTime() && !validatePhone())
            {
                minute_combobox.ForeColor = Color.Red;
                hour_combobox.ForeColor = Color.Red;
                phone_box.ForeColor = Color.Red;
            }
            else
            {
                minute_combobox.ForeColor = Color.Black;
                hour_combobox.ForeColor = Color.Black;
                phone_box.ForeColor = Color.Black;
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

            if (validateTime() && validatePhone())
            {
                minute_combobox.ForeColor = Color.Black;
                hour_combobox.ForeColor = Color.Black;
                phone_box.ForeColor = Color.Black;
                this.save_button.Enabled = true;
            }
            else
            {
                minute_combobox.ForeColor = Color.Red;
                hour_combobox.ForeColor = Color.Red;
                phone_box.ForeColor = Color.Red;
                this.save_button.Enabled = false;
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

            if (validateTime() && validatePhone())
            {
                minute_combobox.ForeColor = Color.Black;
                hour_combobox.ForeColor = Color.Black;
                phone_box.ForeColor = Color.Black;
                this.save_button.Enabled = true;
            }
            else
            {
                minute_combobox.ForeColor = Color.Red;
                hour_combobox.ForeColor = Color.Red;
                phone_box.ForeColor = Color.Red;
                this.save_button.Enabled = false;
            }
        }

        public bool validateTime()
        {
            DateTime today = DateTime.Now;
            return (scheduled > today && !isCanceled);
        }

        public bool validatePhone()
        {
            return Util.form_validation_telephone_number(phone_box.Text);
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
            return scheduled.ToUniversalTime();
        }

        public string getNumber()
        {
            return this.phone_box.Text;
        }

        private void phone_box_TextChanged(object sender, EventArgs e)
        {
            if (validateTime() && validatePhone())
            {
                minute_combobox.ForeColor = Color.Black;
                hour_combobox.ForeColor = Color.Black;
                phone_box.ForeColor = Color.Black;
                this.save_button.Enabled = true;
            }
            else
            {
                minute_combobox.ForeColor = Color.Red;
                hour_combobox.ForeColor = Color.Red;
                phone_box.ForeColor = Color.Red;
                this.save_button.Enabled = false;
            }
        }

        public void updateDialerNumber(string dialingNumber)
        {
            this.dialerNumber = dialingNumber;
        }

        private void frmSchedule_Load(object sender, EventArgs e)
        {
            this.isCanceled = false;
            this.phone_box.Text = this.dialerNumber;
            hour_combobox.SelectedIndex = 0;
            minute_combobox.SelectedIndex = 0;
        }

    }
}
