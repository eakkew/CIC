using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ININ.IceLib;
using ININ.IceLib.Connection;

namespace CIC
{
  public partial class frmICStation : Form
  {

#region  Variable Declaration

    private CIC.Utils.LoginResult mResult;
    private ININ.IceLib.Connection.Session mSession;
    private int SoftPhoneRegistryIndex = 0;
    private string StrSoftPhoneKey = "software\\Interactive Intelligence\\SIP Soft Phone";
    private string SoftPhoneStationNameKey = "Station";

#endregion

#region Properties And Method

    private void Initial_FormComponent()
    {
        this.StationTypeComboBox.Items.Clear();
        this.mResult = CIC.Utils.LoginResult.Cancelled;
        LoginButton.Enabled = false;
        LogInPreviewStatus.Visible = false;
        lblLogInStatusMsg.Visible = false;
        this.Text = Properties.Settings.Default.ApplicationTitle;
    }

    private string GetSIPSoftPhoneStationID
    {
        get
        {
            Microsoft.Win32.RegistryKey MainSoftphoneRegistry = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(StrSoftPhoneKey);
            if (MainSoftphoneRegistry != null)
            {
                string[] AllSoftPhoneRegistryKey = MainSoftphoneRegistry.GetSubKeyNames();
                if (AllSoftPhoneRegistryKey.Length > 0)
                {
                    Microsoft.Win32.RegistryKey CurrentSoftPhoneRegistry = MainSoftphoneRegistry.OpenSubKey(AllSoftPhoneRegistryKey[SoftPhoneRegistryIndex].ToString());
                    if (CurrentSoftPhoneRegistry != null)
                    {
                        return CurrentSoftPhoneRegistry.GetValue(SoftPhoneStationNameKey).ToString();
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }
    }

    private bool IsSoftPhoneRunning
    {
        get
        {
            bool RetValue = false;
            System.Diagnostics.Process[] AllRunningProcess = System.Diagnostics.Process.GetProcesses();
            if (AllRunningProcess != null)
            {
                for (int i = 0; i < AllRunningProcess.Length; i++)
                {
                    if (AllRunningProcess[i].ProcessName.ToLower().Trim() == "sipsoftphone")
                    {
                        RetValue = true;
                        break;
                    }
                }
            }
            return RetValue;
        }
    }

    private void InitialStationTypeList()
    {
        if (global::CIC.Properties.Settings.Default.StationType1Enabled == true)
        {
            this.StationTypeComboBox.Items.Add(global::CIC.Properties.Settings.Default.StationType1Name);
        }
        if (global::CIC.Properties.Settings.Default.StationType2Enabled == true)
        {
            this.StationTypeComboBox.Items.Add(global::CIC.Properties.Settings.Default.StationType2Name);
        }
        if (global::CIC.Properties.Settings.Default.StationType3Enabled == true)
        {
            this.StationTypeComboBox.Items.Add(global::CIC.Properties.Settings.Default.StationType3Name);
        }
        if (global::CIC.Properties.Settings.Default.StationType4Enabled == true)
        {
            this.StationTypeComboBox.Items.Add(global::CIC.Properties.Settings.Default.StationType4Name);
        }
        if (global::CIC.Properties.Settings.Default.StationType5Enabled == true)
        {
            if (this.IsSoftPhoneRunning == true)
            {
                this.StationTypeComboBox.Items.Add(global::CIC.Properties.Settings.Default.StationType5Name); //SoftPhone
            }
        }
    }

    private string GetStationTypeValue(string StationTypeName)
    {
        string RetValue = "";
        if (global::CIC.Properties.Settings.Default.StationType1Name.Trim() == StationTypeName.Trim())
        {
            RetValue = global::CIC.Properties.Settings.Default.StationType1Value;
        }
        if (global::CIC.Properties.Settings.Default.StationType2Name.Trim() == StationTypeName.Trim())
        {
            RetValue = global::CIC.Properties.Settings.Default.StationType2Value;
        }
        if (global::CIC.Properties.Settings.Default.StationType3Name.Trim() == StationTypeName.Trim())
        {
            RetValue = global::CIC.Properties.Settings.Default.StationType3Value;
        }
        if (global::CIC.Properties.Settings.Default.StationType4Name.Trim() == StationTypeName.Trim())
        {
            RetValue = global::CIC.Properties.Settings.Default.StationType4Value;
        }
        if (global::CIC.Properties.Settings.Default.StationType5Name.Trim() == StationTypeName.Trim())
        {
            RetValue = global::CIC.Properties.Settings.Default.StationType5Value;
        }
        return RetValue;
    }

    private void LoginForm_Load(object sender, EventArgs e)
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
        this.InitialStationTypeList();
        this.ServerTextBox.Text = global::CIC.Properties.Settings.Default.Server;
        this.PersistentCheckBox.Checked = global::CIC.Properties.Settings.Default.UsePersistentConnection;
        UpdateLoginControls();
        UpdateStationInfoControls();
        if (this.StationTypeComboBox.Items.Count >= global::CIC.Properties.Settings.Default.StationTypeId)
        {
            if (global::CIC.Properties.Settings.Default.StationTypeId > 0)
            {
                this.StationTypeComboBox.SelectedIndex = global::CIC.Properties.Settings.Default.StationTypeId - 1;
            }
            else
            {
                this.StationTypeComboBox.SelectedIndex = -1;
            }
        }
        else
        {
            this.StationTypeComboBox.SelectedIndex = -1;
        }

        EnableLoginButton();
        this.chkSavepassword.Checked = global::CIC.Properties.Settings.Default.savePassword;
        if (this.chkSavepassword.Checked == true)
        {
            locuslibk.RKey rKey = null;
            try
            {
                rKey = new locuslibk.RKey(global::CIC.Properties.Settings.Default.Server);
            }
            catch
            {
                //Not thrown All Error Message.
            }
            if (rKey != null)
            {
                rKey.LogInAgent = global::CIC.Properties.Settings.Default.UserId;
                try
                {
                    this.PasswordTextBox.Text = rKey.EncryptPassword;
                }
                catch
                {
                    this.PasswordTextBox.Text = "";
                }
            }
        }
        else
        {
            this.PasswordTextBox.Text = "";
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

        /*Get handle to desktop*/
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


            Native.BitBlt(hdc, curRect.Left, curRect.Top, (curRect.Right - curRect.Left), (curRect.Bottom - curRect.Top),
                          hdcCompatible, 0, 0, Native.SRCCOPY);

            Native.GdiFlush();

            System.Threading.Thread.Sleep(20);
        }


        Native.ReleaseDC(this.Handle, hdcForm);
        Native.DeleteDC(hdcCompatible);
        Native.DeleteObject(hbmForm);

        /*Release Desktop DC*/
        Native.ReleaseDC(IntPtr.Zero, hdc);
    }

    private void CancelLoginButton_Click(object sender, EventArgs e)
    {
        this.Close();
    }

    private void EnableLoginButton()
    {
      LoginButton.Enabled = false;
      if (UserIdTextBox.Enabled == true && (UserIdTextBox.Text == null || UserIdTextBox.Text.Trim() == ""))
      {
          return;
      }
      if (PasswordTextBox.Enabled == true && (PasswordTextBox.Text == null || PasswordTextBox.Text.Trim() == ""))
      {
          return;
      }
      if (ServerTextBox.Enabled == true && (ServerTextBox.Text == null || ServerTextBox.Text.Trim() == ""))
      {
          return;
      }
      if (StationIdTextBox.Enabled == true && (StationIdTextBox.Text == null || StationIdTextBox.Text.Trim() == ""))
      {
          return;
      }
      if (PhoneNumberTextBox.Enabled == true && (PhoneNumberTextBox.Text == null || PhoneNumberTextBox.Text.Trim() == ""))
      {
          return;
      }
      LoginButton.Enabled = true;
    }

    private void LogInProcessInfo()
    {
        Cursor.Current = Cursors.AppStarting;
        WindowsAuthenticationCheckBox.Enabled = false;
        UserIdTextBox.Enabled = false;
        PasswordTextBox.Enabled = false;
        ServerTextBox.Enabled = false;
        StationTypeComboBox.Enabled = false;
        StationIdTextBox.Enabled = false;
        PhoneNumberTextBox.Enabled = false;
        PersistentCheckBox.Enabled = false;
        LoginButton.Enabled = false;
        LogInPreviewStatus.Visible = true;
        lblLogInStatusMsg.Visible = false;
    }

    public frmICStation(ININ.IceLib.Connection.Session session)
    {
        Tracing.TraceStatus("CIC::frmICStation::frmICStation()::Show Station Login form.");
        if (session == null)
        {
            throw new ArgumentNullException("CIC::frmICStation::frmICStation()::Null reference session");
        }
        this.mSession = session;
        InitializeComponent();
        this.Load_ApplicationSkin();
        this.Initial_FormComponent();

    }

    private void Load_ApplicationSkin()
    {
        this.BackgroundImage = global::CIC.Program.AppImageList.Images["MainBackground"];
        this.BackgroundImageLayout = ImageLayout.Stretch;
        this.LoginButton.BackgroundImage = global::CIC.Program.AppImageList.Images["MainBackground"];
        this.LoginButton.BackgroundImageLayout = ImageLayout.Stretch;
        this.CancelLoginButton.BackgroundImage = global::CIC.Program.AppImageList.Images["MainBackground"];
        this.CancelLoginButton.BackgroundImageLayout = ImageLayout.Stretch;
    }

    public CIC.Utils.LoginResult LogInResult
    {
        get
        {
            return this.mResult;
        }
    }

    private void LoginButton_Click(object sender, EventArgs e)
    {
        this.LogInProcessInfo();
        this.SetLogInParam();
        global::CIC.Program.IcStation = new ICStation(this.mSession);
        global::CIC.Program.IcStation.LogIn(WindowsAuthenticationCheckBox.Checked, UserIdTextBox.Text, PasswordTextBox.Text, ServerTextBox.Text, StationTypeComboBox.SelectedItem.ToString(), StationIdTextBox.Text, PhoneNumberTextBox.Text, PersistentCheckBox.Checked, this.SessionConnectCompleted, null);
    }

    private void SetLogInParam()
    {
        global::CIC.Program.mLoginParam = new global::CIC.LoginParams();
        global::CIC.Program.mLoginParam.WindowsAuthentication = WindowsAuthenticationCheckBox.Checked;
        global::CIC.Program.mLoginParam.UserId = UserIdTextBox.Text;
        global::CIC.Program.mLoginParam.Password = PasswordTextBox.Text;
        global::CIC.Program.mLoginParam.Server = ServerTextBox.Text;
        global::CIC.Program.mLoginParam.StationType = StationTypeComboBox.SelectedItem.ToString();
        global::CIC.Program.mLoginParam.StationId = StationIdTextBox.Text;
        global::CIC.Program.mLoginParam.PhoneNumber = PhoneNumberTextBox.Text;
        global::CIC.Program.mLoginParam.Persistent = PersistentCheckBox.Checked;
        //---
        global::CIC.Properties.Settings.Default.StationTypeId = StationTypeComboBox.SelectedIndex;
        global::CIC.Properties.Settings.Default.StationId = StationIdTextBox.Text;
        global::CIC.Properties.Settings.Default.UserId= UserIdTextBox.Text;
        //global::CIC.Properties.Settings.Default.Password = PasswordTextBox.Text;
        global::CIC.Properties.Settings.Default.UseWindowsAuthentication = WindowsAuthenticationCheckBox.Checked;
        //global::CIC.Properties.Settings.Default.Server = ServerTextBox.Text;
        global::CIC.Properties.Settings.Default.Phone = PhoneNumberTextBox.Text;
        //global::CIC.Properties.Settings.Default.pers = PersistentCheckBox.Checked;
    }

    private void SessionConnectCompleted(object sender, AsyncCompletedEventArgs e)
    {
      string eDescription;
      this.LogInPreviewStatus.Visible = false;
      this.lblLogInStatusMsg.Visible = true;
      if (e.Error != null)
      {
        eDescription = "CIC::frmICStation::frmICStation()::Failed to connect to session manager. Error=" + e.Error.ToString();
        Tracing.TraceStatus(eDescription);
        Cursor.Current = Cursors.Default;
        this.mResult = CIC.Utils.LoginResult.Cancelled;
        this.WindowsAuthenticationCheckBox.Enabled = true;
        this.UpdateLoginControls();
        this.ServerTextBox.Enabled = true;
        this.StationTypeComboBox.Enabled = true;
        this.UpdateStationInfoControls();
        this.LoginButton.Enabled = true;
        this.LoginButton.Focus();
        this.lblLogInStatusMsg.Text = e.Error.Message.Trim();
      }
      else
      {
        this.lblLogInStatusMsg.Text = "Log In Completed!.";
        eDescription = "CIC::frmICStation::frmICStation()::Connected to session manager successfully";
        global::CIC.Properties.Settings.Default.UserId = this.UserIdTextBox.Text;
        global::CIC.Properties.Settings.Default.UseWindowsAuthentication = this.WindowsAuthenticationCheckBox.Checked;
        global::CIC.Properties.Settings.Default.StationId = this.StationIdTextBox.Text;
        global::CIC.Properties.Settings.Default.StationTypeId = this.StationTypeComboBox.SelectedIndex+1;
        global::CIC.Properties.Settings.Default.UsePersistentConnection = this.PersistentCheckBox.Checked;
        global::CIC.Properties.Settings.Default.savePassword = this.chkSavepassword.Checked;
        System.Net.IPHostEntry MainIpHostEntry = System.Net.Dns.GetHostEntry(this.ServerTextBox.Text);
        if (MainIpHostEntry != null)
        {
            System.Net.IPAddress[] MainIpAddrLst = MainIpHostEntry.AddressList;
            if (MainIpAddrLst.Length > 0)
            {
                global::CIC.Program.mLoginParam.Server = MainIpAddrLst[0].ToString();
            }
            else
            {
                global::CIC.Program.mLoginParam.Server = global::CIC.Properties.Settings.Default.Server;
            }
        }
        locuslibk.RKey rKey = new locuslibk.RKey(global::CIC.Program.mLoginParam.Server);
        try
        {
            if (this.UserIdTextBox.Text.Trim() != "")
            {
                rKey.LogInAgent = this.UserIdTextBox.Text;
            }
            rKey.EncryptPassword = this.PasswordTextBox.Text;
        }
        catch 
        {
            //Not thrown All Error Message.
        }
      
        //FailOver Server Checking
        if (global::CIC.Properties.Settings.Default.SwitchOverSupport == true)
        {
            if (global::CIC.Properties.Settings.Default.FailOverServer.Trim() != "")
            {
                if (global::CIC.Properties.Settings.Default.FailOverServer.Trim() != global::CIC.Properties.Settings.Default.Server.Trim())
                {
                    System.Net.IPHostEntry ipHostEntry = null;
                    try
                    {
                       ipHostEntry = System.Net.Dns.GetHostEntry(global::CIC.Properties.Settings.Default.FailOverServer);
                    }
                    catch
                    {
                        //Release All Error!{No such host is know}
                    }

                    if (ipHostEntry != null)
                    {
                        System.Net.IPAddress[] ipAddrLst = ipHostEntry.AddressList;
                        if (ipAddrLst.Length > 0)
                        {
                            global::CIC.Program.mLoginParam.FailOverServer = ipAddrLst[0].ToString();
                        }
                        else
                        {
                            global::CIC.Program.mLoginParam.FailOverServer = "";
                        }
                    }
                }
            }
        }
         if (global::CIC.Program.ReGenKeyEnabled == true)
        {
            try
            {
                if (global::CIC.Properties.Settings.Default.SwitchOverSupport == true)
                {
                    locuslibk.RKey rKeyA = new locuslibk.RKey(this.mSession.Endpoint.Host);
                    rKeyA.GenKeyAuthorize();
                    if (global::CIC.Properties.Settings.Default.FailOverServer.Trim() != "")
                    {
                        if (global::CIC.Properties.Settings.Default.FailOverServer.Trim() != global::CIC.Properties.Settings.Default.Server.Trim())
                        {
                            if (global::CIC.Program.mLoginParam.FailOverServer.Trim() != "")
                            {
                                locuslibk.RKey rKeyB = new locuslibk.RKey(global::CIC.Program.mLoginParam.FailOverServer);
                                rKeyB.GenKeyAuthorize();
                            }
                        }
                    }
                }
                else
                {
                    rKey.GenKeyAuthorize();
                }
            }
            catch 
            {
                //Not thrown All Error Message.
            }
        }
        try
        {
            if (rKey.ProcessKeyAuthorize() != true)
            {
                this.lblLogInStatusMsg.Text = "Please check your key file.";
                System.Windows.Forms.MessageBox.Show("Please check your key file.", "CIC-Agent error info.", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                this.mResult = CIC.Utils.LoginResult.Cancelled;
                //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, "KeyFile :: Error info.Please check your key file.", System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                Application.Exit();
            }
            else
            {
                this.mResult = CIC.Utils.LoginResult.Success;
                global::CIC.Program.mLoginParam.Password = this.PasswordTextBox.Text;
                global::CIC.Properties.Settings.Default.Server = this.ServerTextBox.Text;
                global::CIC.Properties.Settings.Default.Save();
            }
            global::CIC.Program.LogInFormClosed = true;
            this.Close();
        }
        catch (System.Exception ex)
        {
            this.lblLogInStatusMsg.Text = "Please check your key file.";
            System.Windows.Forms.MessageBox.Show("Please check your key file.", "CIC-Agent error info.", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            this.mResult = CIC.Utils.LoginResult.Cancelled;
            //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, "KeyFile :: Error info."+ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
            Application.Exit();
        }
      }
    }

    private void PasswordTextBox_TextChanged(object sender, EventArgs e)
    {
      EnableLoginButton();
    }

    private void PhoneNumberTextBox_TextChanged(object sender, EventArgs e)
    {
      EnableLoginButton();
    }

    private void ServerTextBox_TextChanged(object sender, EventArgs e)
    {
      EnableLoginButton();
    }

    private void UserIdTextBox_TextChanged(object sender, EventArgs e)
    {
      EnableLoginButton();
    }

    private void WindowsAuthenticationCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      UpdateLoginControls();
      EnableLoginButton();
    }

    private void UpdateLoginControls()
    {
      this.UserIdTextBox.Enabled = !this.WindowsAuthenticationCheckBox.Checked;
      this.PasswordTextBox.Enabled = !this.WindowsAuthenticationCheckBox.Checked;
      if (this.WindowsAuthenticationCheckBox.Checked == true)
      {
          this.UserIdTextBox.Text = Environment.UserName;
          this.PasswordTextBox.Text = "";
      }
      else
      {
          this.UserIdTextBox.Text = global::CIC.Properties.Settings.Default.UserId;
          if (global::CIC.Properties.Settings.Default.UserId.Trim() != "")
          {
              if (this.chkSavepassword.Checked == true)
              {
                  locuslibk.RKey rKey = new locuslibk.RKey(this.ServerTextBox.Text);
                  try
                  {
                      this.PasswordTextBox.Text = rKey.EncryptPassword;
                  }
                  catch { }
              }
          }
          else
          {
              this.PasswordTextBox.Text = "";
          }
      }
    }

    private void StationTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      UpdateStationInfoControls();
      EnableLoginButton();
    }

    private void UpdateStationInfoControls()
    {
        if (StationTypeComboBox.SelectedItem != null)
        {
            if (StationTypeComboBox.SelectedItem.ToString().ToLower() == global::CIC.Properties.Settings.Default.StationType1Name.ToLower().Trim())
            {
                StationIdTextBox.Enabled = true;
                StationIdTextBox.Text = global::CIC.Properties.Settings.Default.StationId;
                PhoneNumberTextBox.Enabled = false;
                PersistentCheckBox.Enabled = false;
            }
            else if (StationTypeComboBox.SelectedItem.ToString().ToLower() == global::CIC.Properties.Settings.Default.StationType2Name.ToLower().Trim())
            {
                StationIdTextBox.Enabled = false;
                StationIdTextBox.Text = Environment.MachineName;
                PhoneNumberTextBox.Enabled = false;
                PersistentCheckBox.Enabled = false;
            }
            else if (StationTypeComboBox.SelectedItem.ToString().ToLower() == global::CIC.Properties.Settings.Default.StationType3Name.ToLower().Trim())
            {
                StationIdTextBox.Enabled = true;
                StationIdTextBox.Text = global::CIC.Properties.Settings.Default.StationId;
                PhoneNumberTextBox.Enabled = false;
                PersistentCheckBox.Enabled = true;
            }
            else if (StationTypeComboBox.SelectedItem.ToString().ToLower() == global::CIC.Properties.Settings.Default.StationType4Name.ToLower().Trim())
            {
                StationIdTextBox.Enabled = false;
                StationIdTextBox.Text = Environment.MachineName;
                PhoneNumberTextBox.Enabled = true;
                PersistentCheckBox.Enabled = true;
            }
            else if (StationTypeComboBox.SelectedItem.ToString().ToLower() == global::CIC.Properties.Settings.Default.StationType5Name.ToLower().Trim())
            {
                StationIdTextBox.Text = this.GetSIPSoftPhoneStationID;
                StationIdTextBox.Enabled = false;
                PhoneNumberTextBox.Text = "";
                PhoneNumberTextBox.Enabled = false;
                PersistentCheckBox.Enabled = false;
            }
        }
    }

    private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      System.Configuration.ConfigurationManager.AppSettings["StationType"]= StationTypeComboBox.SelectedIndex.ToString();
      global::CIC.Program.LogInFormClosed = true;
      if (global::CIC.Properties.Settings.Default.Animate == true)
      {
          bool b = Native.AnimateWindow(this.Handle, 1000, Native.AW_BLEND | Native.AW_HIDE);
          base.OnClosing(e);
      }
    }

    private void chkSavepassword_CheckedChanged(object sender, EventArgs e)
    {
        if (this.chkSavepassword.Checked == true)
        {
            //Reserve
        }
        else
        {
            this.PasswordTextBox.Text = "";
        }
    }

#endregion

  }
}