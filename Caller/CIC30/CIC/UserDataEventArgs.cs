using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CIC
{
    public partial class UserDataEventArgs : EventArgs
    {
        public CIC.UserData userData;
        public string MenuName;
        public string MenuValue;

        public UserDataEventArgs()
        {
            //
        }


    }
}
