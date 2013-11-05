using System;
using System.Collections.Generic;
using System.Text;

namespace CIC
{
    class ICWorkFlow
    {
#region  Variable Declaration

        private ININ.IceLib.Dialer.DialerSession mDialerSession = null;
        private ININ.IceLib.Dialer.DialingManager mDialingManager = null;
        private ININ.IceLib.Dialer.AgentType mAgentType;
        private string mWorkFlow_Name;

#endregion

#region Properties And Method

        public ICWorkFlow(ININ.IceLib.Dialer.DialingManager objDialingManager)
        {
            this.mDialingManager = objDialingManager;
            this.SetDefaultAgentType();
        }

        private void SetDefaultAgentType()
        {
            this.mAgentType = new ININ.IceLib.Dialer.AgentType();
            this.mAgentType = ININ.IceLib.Dialer.AgentType.Regular;
        }

        public ININ.IceLib.Dialer.DialerSession DialerSession
        { 
            get
            { 
                return mDialerSession;
            }
        }

        public string CurrentWorkFlowName
        {
            get
            {
                return this.mWorkFlow_Name;
            }
            set
            {
                this.mWorkFlow_Name = value;
            }
        }

        public static string[] GetAvailableAWorkflowList(ININ.IceLib.Dialer.DialingManager mDialingManager)
        {
            string[] AvailableAWorkflowList;
            if(mDialingManager != null)
            {
                AvailableAWorkflowList = mDialingManager.GetAvailableWorkflows();
            }
            else
            {
                AvailableAWorkflowList = null;
            }
            return AvailableAWorkflowList;
        }

        private bool bDialerLogOnResult = false;

        public bool LoginResult
        {
            get
            {
                return this.bDialerLogOnResult;
            }
        }

        public ININ.IceLib.Dialer.DialerSession LogIn(string WorkFlowName)
        {
            if (WorkFlowName.Trim() ==  String.Empty)
            {
                if (this.mWorkFlow_Name == String.Empty)
                {
                    return this.mDialerSession;
                }
            }
            else
            {
                this.mWorkFlow_Name = WorkFlowName;
            }
            if(mDialingManager == null)
            {
                return this.mDialerSession;
            }
            try
              {
                ININ.IceLib.Dialer.LogonParameters parameters = new ININ.IceLib.Dialer.LogonParameters(this.mWorkFlow_Name);
                ININ.IceLib.Dialer.LogonResult result = this.mDialingManager.Logon(parameters);
              
                if (result.DialerSession != null)
                {
                    mDialerSession = result.DialerSession;
                    mDialerSession.SetAgentType(this.mAgentType);
                    mDialerSession.StartReceivingCalls();
                    if (Program.EstablishPersistentConnection)
                    {
                        mDialerSession.EstablishPersistentConnection();
                    }
                    this.bDialerLogOnResult = true;
                }
                else
                {
                    this.bDialerLogOnResult = false;
                }
              }
            catch (ININ.IceLib.IceLibException ex)
              {
                  this.bDialerLogOnResult = false;
                  throw ex;
              }
            return this.mDialerSession;
        }

#endregion

    }
}
