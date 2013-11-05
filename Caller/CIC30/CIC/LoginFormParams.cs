using System;
using System.Collections.Generic;
using System.Text;
using ININ.IceLib;
using ININ.IceLib.Connection;
using ININ.IceLib.Dialer;

namespace CIC
{
    class LoginParams
    {
        private CIC.Utils.LoginResult m_LoginResult;
        private ININ.IceLib.Connection.Session m_Session;

        public LoginParams(ININ.IceLib.Connection.Session session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }
            this.m_Session = session;
        }

        public CIC.Utils.LoginResult Result
        {
            get
            { 
                return m_LoginResult;
            }
            set
            { 
                m_LoginResult = value;
            }
        }

        public Session Session
        {
            get 
            { 
                return m_Session;
            }
        }
    }
}
