
//Application : Client Application for I3 system 
//            : Supported I3 version 3.0 or higher SU1 and 
//            : Ice library version 3.0.204.10107 .Ice library(Dialer library) version 3.0.11.10414
//Generated by Mr.Suthee Wangsawasd
//Project Starting on 14 October 2008.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using ININ.IceLib;
using ININ.IceLib.Connection;
using ININ.IceLib.Dialer;

namespace CIC
{
  static class Program
  {

#region Global Variable Definition 
      private static string wildcardKey = "/i3reset";
      private static bool _Initialized = false;
      private static bool _EstablishPersistentConnection = false;
      private static int SoftPhoneRegistryIndex = 0;
      private static string StrSoftPhoneKey = "software\\Interactive Intelligence\\SIP Soft Phone";
      private static string SoftPhoneStationNameKey = "Station";
      public static ININ.IceLib.Connection.Session m_Session = null;
      public static bool LogInFormClosed = true;
      public static global::CIC.LoginParams mLoginParam = null;
      public static CIC.ICStation IcStation = null;
      public static CIC.frmICStation mICStation = null;
      public static ININ.IceLib.Dialer.DialingManager mDialingManager = null;
      public static CIC.frmMain MainApp = null;
      public static CIC.FormMain MainDashboard = null;
      public static CIC.frmWorkflow Workflow = null;
      public static ININ.IceLib.Dialer.DialingManager DialingManager
      {
          get
          {
              if (!_Initialized)
              {
                  _Initialized = true;
              }
              return mDialingManager;
          }
          set
          {
              mDialingManager = value;
          }
      }
      public static void Initialize_dialingManager(Session session)
      {
          if (!_Initialized)
          {
              mDialingManager = null;
              mDialingManager = new ININ.IceLib.Dialer.DialingManager(session);
              _Initialized = true;
          }
      }
      public static bool EstablishPersistentConnection
      {
          get
          { 
              return _EstablishPersistentConnection;
          }
          set
          {
              _EstablishPersistentConnection = value;
          }
      }
      public static System.Windows.Forms.ImageList AppImageList = null;
      public static CIC.Utils.LoginResult SessionLogInResult;
      public static string AppError;
      public static string ApplicationPath;
      public static string ResourcePath;
      public static string ApplicationImagePath;
      // TODO: disable regen key
      public static bool ReGenKeyEnabled = true;

#endregion

#region Starting Application Section
      [STAThread]

      public static void Main(string[] args)
      {
          if (global::CIC.Program.CheckExistLib() == true)
          {
              try
              {
                  Application.EnableVisualStyles();
                  Application.SetCompatibleTextRenderingDefault(false);
                  global::CIC.Program.Initial_ApplicationPath();
                  global::CIC.Program.InitialSkin();
                  if (args != null)
                  {
                      if (args.Length > 0)
                      {
                          if (args[0].ToLower().Trim() == wildcardKey.ToLower().Trim())
                          {
                              //Reset All Registry
                              global::CIC.Properties.Settings.Default.Reset();
                              global::CIC.Properties.Settings.Default.Reload();
                              global::CIC.Program.ReGenKeyEnabled = true;
                          }
                      }
                  }
                  m_Session = new Session();
                  if (global::CIC.Properties.Settings.Default.SingleSignOn != true)
                  {
                      mICStation = new CIC.frmICStation(m_Session);
                      LogInFormClosed = false;
                      Application.Run(mICStation);
                      // TODO: undo bypass the server login
                      global::CIC.Program.SessionLogInResult = Utils.LoginResult.Success;
                      //global::CIC.Program.SessionLogInResult = mICStation.LogInResult;
                  }
                  else
                  {
                      global::CIC.Program.GetWindowsLogInParam();
                  }

                  //login and run app
                  while (!global::CIC.Program.LogInFormClosed)
                  {
                      //Waitting for LogIn Dialog Closed.
                      Application.DoEvents();
                  }
                  if (global::CIC.Program.SessionLogInResult == CIC.Utils.LoginResult.Success)
                  {
                      m_Session.Disconnect();
                      m_Session = null;
                      //MainApp = new CIC.frmMain();
                      MainDashboard = new CIC.FormMain();
                      Application.Run(MainDashboard);
                  }
                  if (m_Session != null)
                  {
                      m_Session.Disconnect();
                  }
                  Tracing.TraceNote("CIC::Program::Main()::Disconnecting from Session Manager.");
              }
              catch (System.Exception e)
              {
                  //Reset All Registry
                  string eDescription = "CIC::Program::Main()::" + e.Message;
                  global::CIC.Properties.Settings.Default.Reset();
                  global::CIC.Properties.Settings.Default.Reload();
                  //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, "CIC::Program::Main()::Error info." + e.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                  Tracing.TraceNote(eDescription);
              }
              global::CIC.Properties.Settings.Default.Save();
              Tracing.TraceNote("CIC::Program::Main()::Application end.");
          }
          else
          {
              //Reset All Registry
              global::CIC.Properties.Settings.Default.Reset();
              global::CIC.Properties.Settings.Default.Reload();
              //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, AppError, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
              MessageBox.Show(AppError,"Application Error!", MessageBoxButtons.OK, MessageBoxIcon.Information);
          }
          Application.Exit();
      }

      private static void GetWindowsLogInParam()
      {
          global::CIC.Program.mLoginParam = new global::CIC.LoginParams();
          locuslibk.RKey rKey = new locuslibk.RKey(global::CIC.Properties.Settings.Default.Server);
          try
          {
              if (rKey.ProcessKeyAuthorize() != true)
              {
                  System.Windows.Forms.MessageBox.Show("Please check your key file.", "Error Information!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                  //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, "KeyFile :: Error info.Please check your key file.", System.Diagnostics.EventLogEntryType.Error); //Window Event Log
                  Application.Exit();
              }
          }
          catch (System.Exception ex)
          {
              System.Windows.Forms.MessageBox.Show("Please check your key file.", "Error Information!", MessageBoxButtons.OK, MessageBoxIcon.Error);
              //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, "KeyFile :: Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
              Application.Exit();
          }
          global::CIC.Program.mLoginParam.WindowsAuthentication = true;  // Aways true on this mode
          global::CIC.Program.mLoginParam.UserId = Environment.UserName;
          global::CIC.Program.mLoginParam.Password = ""; //Not used
          global::CIC.Program.mLoginParam.Server = global::CIC.Properties.Settings.Default.Server;
          switch (global::CIC.Properties.Settings.Default.StationTypeId)
          {
            case 1:
              global::CIC.Program.mLoginParam.StationType = global::CIC.Properties.Settings.Default.StationType1Value;
              global::CIC.Program.mLoginParam.StationId = global::CIC.Properties.Settings.Default.StationId;
              global::CIC.Program.mLoginParam.PhoneNumber = "";
              global::CIC.Program.mLoginParam.Persistent = false;
              break;
            case 2:
              global::CIC.Program.mLoginParam.StationType = global::CIC.Properties.Settings.Default.StationType2Value;
              global::CIC.Program.mLoginParam.StationId = Environment.MachineName;
              global::CIC.Program.mLoginParam.PhoneNumber = "";
              global::CIC.Program.mLoginParam.Persistent = false;
              break;
            case 3:
              global::CIC.Program.mLoginParam.StationType = global::CIC.Properties.Settings.Default.StationType3Value;
              global::CIC.Program.mLoginParam.StationId = global::CIC.Properties.Settings.Default.StationId;
              global::CIC.Program.mLoginParam.PhoneNumber = "";
              global::CIC.Program.mLoginParam.Persistent = true;
              break;
            case 4:
              global::CIC.Program.mLoginParam.StationType = global::CIC.Properties.Settings.Default.StationType4Value;
              global::CIC.Program.mLoginParam.StationId = "";
              global::CIC.Program.mLoginParam.PhoneNumber = global::CIC.Properties.Settings.Default.Phone;
              global::CIC.Program.mLoginParam.Persistent = true;
              break;
            case 5:
              global::CIC.Program.mLoginParam.StationType = global::CIC.Properties.Settings.Default.StationType5Value;
              if (global::CIC.Program.GetSIPSoftPhoneStationID().Trim() != "")
              {
                  global::CIC.Program.mLoginParam.StationId = global::CIC.Program.GetSIPSoftPhoneStationID();
              }
              else
              {
                  System.Windows.Forms.MessageBox.Show("Please open SIP Soft Phone before.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                  Application.Exit();
              }
              global::CIC.Program.mLoginParam.PhoneNumber = "";
              global::CIC.Program.mLoginParam.Persistent = false;
              break;
            default:
              global::CIC.Program.mLoginParam.StationType = global::CIC.Properties.Settings.Default.StationType1Value;
              global::CIC.Program.mLoginParam.StationId = global::CIC.Properties.Settings.Default.StationId;
              global::CIC.Program.mLoginParam.PhoneNumber = "";
              global::CIC.Program.mLoginParam.Persistent = false;
              break;
          }

          //Pre-login
          global::CIC.Program.IcStation = new ICStation(global::CIC.Program.m_Session);
          global::CIC.Program.IcStation.LogIn(global::CIC.Program.mLoginParam.WindowsAuthentication, global::CIC.Program.mLoginParam.UserId, global::CIC.Program.mLoginParam.Password, global::CIC.Program.mLoginParam.Server, global::CIC.Program.mLoginParam.StationType, global::CIC.Program.mLoginParam.StationId, global::CIC.Program.mLoginParam.PhoneNumber, global::CIC.Program.mLoginParam.Persistent, SessionConnectCompleted, null);       
      }

      private static string GetSIPSoftPhoneStationID()
      {
          Microsoft.Win32.RegistryKey MainSoftphoneRegistry = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(StrSoftPhoneKey);
          string[] AllSoftPhoneRegistryKey = MainSoftphoneRegistry.GetSubKeyNames();
          if (AllSoftPhoneRegistryKey.Length > 0)
          {
              Microsoft.Win32.RegistryKey CurrentSoftPhoneRegistry = MainSoftphoneRegistry.OpenSubKey(AllSoftPhoneRegistryKey[SoftPhoneRegistryIndex].ToString());
              return CurrentSoftPhoneRegistry.GetValue(SoftPhoneStationNameKey).ToString();
          }
          else
          {
              return "";
          }
      }

      private static void SessionConnectCompleted(object sender, AsyncCompletedEventArgs e)
      {
          if (e.Error != null)
          {
              global::CIC.Program.SessionLogInResult = CIC.Utils.LoginResult.Cancelled;
              //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, "CIC::Program::SessionConnectCompleted [Single sign on]::Error info." + e.Error, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
              System.Windows.Forms.MessageBox.Show("CIC::Program::SessionConnectCompleted [Single sign on]::Error info." + e.Error, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
              Application.Exit();
          }
          else
          {
              global::CIC.Program.SessionLogInResult = CIC.Utils.LoginResult.Success;
          }
      }

      private static bool CheckExistLib()
      {
           bool ChkININLib = false;
           try
            {
                Tracing.TraceNote("CIC::Program::Main()::Application started.");
                ChkININLib = true;
            }
           catch(System.Exception e)
            {
                AppError = "ICE Info : " + e.Message+". Please Check ICE-Lib installation.";
                ChkININLib = false;
            }
            return ChkININLib;
      }

      private static void Initial_ApplicationPath()
      {
          string scope = "CIC::Program::Initisl_ApplicationPath()::";
          Tracing.TraceStatus(scope + "Initializing status messages");
          string sAppPath = Environment.CurrentDirectory;
          if (System.IO.Directory.Exists(sAppPath) == true)
          {
              CIC.Program.ApplicationPath = sAppPath;
              CIC.Program.ResourcePath = sAppPath + "\\Resources\\";
              CIC.Program.ApplicationImagePath = sAppPath + "\\Resources\\";
              if (System.IO.Directory.Exists(CIC.Program.ResourcePath) == true)
              {
                  Tracing.TraceStatus(scope + "Initializing resource path completed.");
              }
              else
              {
                  Tracing.TraceStatus(scope + "Initializing resource path fail.");
              }
          }
          else
          {
              Tracing.TraceStatus(scope + "Initializing resource path fail.");
          }
      }

      private static void InitialSkin()
      {
          if (global::CIC.Program.AppImageList == null)
          {
              global::CIC.Program.AppImageList = new System.Windows.Forms.ImageList();
          }
          else
          {
              global::CIC.Program.AppImageList.Images.Clear();
          }
          string ImgPath = "";
          try
          {
              ImgPath = global::CIC.Program.ResourcePath  + global::CIC.Properties.Settings.Default.NormalBar_Normal;
              if (System.IO.File.Exists(ImgPath) == true)
              {
                  global::CIC.Program.AppImageList.Images.Add("NormalBar_Normal", System.Drawing.Image.FromFile(ImgPath));
              }
              ImgPath = global::CIC.Program.ResourcePath + global::CIC.Properties.Settings.Default.NormalBar_Info;
              if (System.IO.File.Exists(ImgPath) == true)
              {
                  global::CIC.Program.AppImageList.Images.Add("NormalBar_Info", System.Drawing.Image.FromFile(ImgPath));
              }
              ImgPath = global::CIC.Program.ResourcePath + global::CIC.Properties.Settings.Default.NormalBar_Active;
              if (System.IO.File.Exists(ImgPath) == true)
              {
                  global::CIC.Program.AppImageList.Images.Add("NormalBar_Active", System.Drawing.Image.FromFile(ImgPath));
              }

              ImgPath = global::CIC.Program.ResourcePath + global::CIC.Properties.Settings.Default.WorkFlowBar_Normal;
              if (System.IO.File.Exists(ImgPath) == true)
              {
                  global::CIC.Program.AppImageList.Images.Add("WorkFlowBar_Normal", System.Drawing.Image.FromFile(ImgPath));
              }
              ImgPath = global::CIC.Program.ResourcePath + global::CIC.Properties.Settings.Default.WorkFlowBar_Info;
              if (System.IO.File.Exists(ImgPath) == true)
              {
                  global::CIC.Program.AppImageList.Images.Add("WorkFlowBar_Info", System.Drawing.Image.FromFile(ImgPath));
              }
              ImgPath = global::CIC.Program.ResourcePath + global::CIC.Properties.Settings.Default.WorkFlowBar_Active;
              if (System.IO.File.Exists(ImgPath) == true)
              {
                  global::CIC.Program.AppImageList.Images.Add("WorkFlowBar_Active", System.Drawing.Image.FromFile(ImgPath));
              }
              ImgPath = global::CIC.Program.ResourcePath + global::CIC.Properties.Settings.Default.WorkFlowBar_Initial;
              if (System.IO.File.Exists(ImgPath) == true)
              {
                  global::CIC.Program.AppImageList.Images.Add("WorkFlowBar_Initial", System.Drawing.Image.FromFile(ImgPath));
              }
              ImgPath = global::CIC.Program.ResourcePath + global::CIC.Properties.Settings.Default.TransferBar;
              if (System.IO.File.Exists(ImgPath) == true)
              {
                  global::CIC.Program.AppImageList.Images.Add("TransferBar", System.Drawing.Image.FromFile(ImgPath));
              }
              ImgPath = global::CIC.Program.ResourcePath + global::CIC.Properties.Settings.Default.MainBackground;
              if (System.IO.File.Exists(ImgPath) == true)
              {
                  global::CIC.Program.AppImageList.Images.Add("MainBackground", System.Drawing.Image.FromFile(ImgPath));
              }
          }
          catch (System.Exception ex)
          {
              //System.Diagnostics.EventLog.WriteEntry(Application.ProductName, "CIC::Program::InitialSkin()::Error info." + ex.Message, System.Diagnostics.EventLogEntryType.Error); //Window Event Log
          }
      }

#endregion

  }
}