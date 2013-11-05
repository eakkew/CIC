using System;
using System.Collections.Generic;
using System.Text;

namespace CIC
{
    public class LoginParams
    {
        public CIC.Utils.LoginResult LoginResult;
        public string UserId;
        public string Password;
        public string Server;
        public string FailOverServer;
        public string StationType;
        public string StationId;
        public string PhoneNumber;
        public bool Persistent;
        public bool WindowsAuthentication;

        public LoginParams()
        {
            this.LoginResult = Utils.LoginResult.Cancelled;
            this.UserId = "";
            this.Password = "";
            this.Server = "";
            this.FailOverServer = "";
            this.StationType = "";
            this.StationId = "";
            this.PhoneNumber = "";
            this.Persistent = false;
            this.WindowsAuthentication = false;
        }


    }
}
