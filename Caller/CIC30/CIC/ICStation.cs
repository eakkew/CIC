using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ININ.IceLib;
using ININ.IceLib.Connection;

namespace CIC
{
    public class ICStation
    {

#region  Variable Declaration

        private bool IsFirstLogIn = false;
        private ININ.IceLib.Connection.Session mSession;
        private ININ.IceLib.Connection.SessionSettings session_setting = null;
        private string ServerA = null;
        private string ServerB = null;
        private ININ.IceLib.Connection.HostSettings host = null;
        private ININ.IceLib.Connection.AuthSettings auth = null;
        public int mConnectionTimes = 0;
        private ININ.IceLib.Connection.StationSettings station;
        private string mAppName;
        private System.Object mUserState;

#endregion

#region Properties And Method
       
        public string SessionAppName
        {
            get
            {
                return this.mAppName;
            }
            set
            {
                this.mAppName = value;
            }
        }

        public int ConnectionTimes
        {
            get
            {
                return this.mConnectionTimes;
            }
            set
            {
                this.mConnectionTimes = value;
            }
        }

        public System.Object userState
        {
            get
            {
                return this.mUserState;
            }
        }

        public ICStation(ININ.IceLib.Connection.Session session)
        {
            this.IsFirstLogIn = true;
            this.mSession = session;
        }

        public ININ.IceLib.Connection.Session CurrentSession
        {
            get
            {
                return this.mSession; 
            }
            set
            {
                this.mSession = value;
            }
        }

        public void LogIn(bool WindowsAuthenticationCheck, string user, string password, string ServerName,
                          string StationType, string stationId, string phoneNumber, bool Persistent,
                          System.ComponentModel.AsyncCompletedEventHandler completedCallBack, System.Object UserState)
        {
            this.ServerA = ServerName;
            this.session_setting = new ININ.IceLib.Connection.SessionSettings();
            this.session_setting.ClassOfService = ININ.IceLib.Connection.ClassOfService.General;
            this.session_setting.DeviceType = ININ.IceLib.Connection.DeviceType.Win32;
            if(this.mAppName == null)
            {
                this.session_setting.ApplicationName = "CIC Agent Application";
            }
            else
            {
                this.session_setting.ApplicationName = this.mAppName;
            }
            this.host = new ININ.IceLib.Connection.HostSettings(new HostEndpoint(ServerName));
            if(WindowsAuthenticationCheck == true)
            {
                this.auth = new WindowsAuthSettings();
            }
            else
            {
                this.auth = new ICAuthSettings(user, password);
            }

            if(StationType.ToString().ToLower() == global::CIC.Properties.Settings.Default.StationType1Name.ToLower().Trim())
            {
                this.station = new WorkstationSettings(stationId, SupportedMedia.Call);
            }
            else if(StationType.ToString().ToLower() == global::CIC.Properties.Settings.Default.StationType2Name.ToLower().Trim())
            {
                this.station = new WorkstationSettings(stationId, SupportedMedia.Call);
            }
            else if(StationType.ToString().ToLower() == global::CIC.Properties.Settings.Default.StationType3Name.ToLower().Trim())
            {
                this.station = new RemoteStationSettings(stationId, SupportedMedia.Call, "", Persistent);
            }
            else if(StationType.ToString().ToLower() == global::CIC.Properties.Settings.Default.StationType4Name.ToLower().Trim())
            {
                this.station = new RemoteNumberSettings(SupportedMedia.Call, phoneNumber, Persistent);
            }
            else if(StationType.ToString().ToLower() == global::CIC.Properties.Settings.Default.StationType5Name.ToLower().Trim())
            {
                this.station = new WorkstationSettings(stationId, SupportedMedia.Call);
            }
            else
            {
                throw new InvalidOperationException("Unrecognized station type selected in combobox");
            }
            string eDescription = "CIC::frmICStation::frmICStation()::Connecting to session manager. host=" + host.ToString() + ", auth=" + auth.ToString() + ", station=" + station.ToString();
            // TODO: add log event here
            Tracing.TraceStatus(eDescription);
            session_setting.MachineName = this.GetActiveLocalAddress(host.HostEndpoint.Host.ToString());
            if (this.ConnectionTimes <= 0)
            {
                if (this.host == null)
                {
                    this.host = new ININ.IceLib.Connection.HostSettings(new HostEndpoint(this.ServerA));
                }
                this.ConnectionTimes++;
            }
            else
            {
                this.ServerB = global::CIC.Properties.Settings.Default.FailOverServer;
                this.host = new ININ.IceLib.Connection.HostSettings(new HostEndpoint(this.ServerB));
            }
            this.mSession.ConnectAsync(session_setting, host, auth, station, completedCallBack, this.mUserState);
            this.mUserState = UserState;
        }

        public void ICConnect()
        {
            if (this.mConnectionTimes <= 0)
            {
                if (this.ServerA == null)
                {
                    this.ServerA = global::CIC.Program.mLoginParam.Server;
                }
                this.host = new ININ.IceLib.Connection.HostSettings(new HostEndpoint(this.ServerA));
                this.mConnectionTimes++;
            }
            else
            {
                if (global::CIC.Properties.Settings.Default.SwitchOverSupport == true)
                {
                    if (this.ServerB == null)
                    {
                        if (global::CIC.Program.mLoginParam.FailOverServer.Trim() != "")
                        {
                            this.ServerB = global::CIC.Program.mLoginParam.FailOverServer.Trim();
                        }
                        else
                        {
                            this.ServerB = this.ServerA;
                        }
                    }
                }
                else
                {
                    if (this.ServerA == null)
                    {
                        this.ServerA = global::CIC.Program.mLoginParam.Server;
                    }
                    this.ServerB = this.ServerA;
                }
                this.host = new ININ.IceLib.Connection.HostSettings(new HostEndpoint(this.ServerB));
                this.mConnectionTimes = 0;
            }
            locuslibk.RKey rKey = new locuslibk.RKey(this.host.HostEndpoint.Host);
            if (rKey.ProcessKeyAuthorize() == true)
            {
                this.mSession.ConnectAsync(this.session_setting, this.host, this.auth, this.station, completedCallBack, this.mUserState);
            }
        }

        private void completedCallBack(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (this.IsFirstLogIn == true)
            {
                this.IsFirstLogIn = false;
                this.mConnectionTimes = 0;
            }
            global::CIC.Program.m_Session = this.mSession;
        }

        private void mSession_Changed(System.Object Sender, ININ.IceLib.Connection.ConnectionStateChangedEventArgs e)
        {
            switch (e.State)
            {
                case ININ.IceLib.Connection.ConnectionState.Attempting:
                    break;
                case ININ.IceLib.Connection.ConnectionState.Up:
                    this.ConnectionTimes = 0;
                    break;
                case ININ.IceLib.Connection.ConnectionState.Down:
                    this.ICConnect();
                    break;
                case ININ.IceLib.Connection.ConnectionState.None:
                    this.ICConnect();
                    break;
                default:
                    break;
            }
        }

        private string GetActiveLocalAddress(string SipServerAddress)
        {
            System.Net.IPAddress[] IP_Addr;
            string mLocalIP = "";
            string HostName = System.Net.Dns.GetHostName();
            IP_Addr = System.Net.Dns.GetHostAddresses(HostName);
            if(IP_Addr != null)
            {
                foreach (System.Net.IPAddress IP in IP_Addr)
                {
                    if(this.GetNetworkAddress(SipServerAddress) == this.GetNetworkAddress(IP.ToString()))
                    {
                        mLocalIP = IP.ToString();      //check for same network of sip server
                        break;
                    }
                }
                if (mLocalIP == "")
                {
                    if(IP_Addr.Length>0)
                    {
                        mLocalIP = IP_Addr[0].ToString();
                    }
                    else
                    {
                        mLocalIP = "127.0.0.1";
                    }
                }
            }
            return mLocalIP;
        }

        private string GetNetworkAddress(string sIPAddress)
        {
            string[] sTempIP = sIPAddress.Split('.');
            string sNetAddress = "";
            if (sTempIP.Length > 3)
            {
                sNetAddress = sTempIP[0].ToString() + "." + sTempIP[1].ToString() +"."+ sTempIP[2].ToString();
            }
            return sNetAddress;
        }

#endregion

    }
}
