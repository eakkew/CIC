using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace CIC
{
  public static class Utils
  {
    public static string NameValueCollectionToString(NameValueCollection collection)
    {
      if (collection == null) return "";

      string result = "";
      for (int i = 0; i < collection.Count; i++)
      {
        result = result + collection.Keys[i].ToString() + "=" + collection[i].ToString() + " ";
      }
      return result;
    }

    public enum LoginResult
    {
        Success, Cancelled
    }

    public enum ICServerConnectResult
    {
        SuccessfulConnection = 0,
        AlreadyConnected = 1,
        ServerNotResponding = 2,
        ConnectingToAnInactiveServer = 3,
        CookieOutOfDate = 4,
        UsernameOrPasswordIncorrect = 5,
        UnspecifiedError = 6,
        CSRequestRejected = 1001,
        CSRequestTimeout = 1002,
        CSNoNotifierConnection = 1003,
        CSBadNotifierConnection = 1004,
        CSRequestAbandoned = 1005,
        CSRequestCancelled = 1006,
        CSAsyncOperationPending = 1009,
        CSUnexpectedError = 1011,
        CSFunctionalityNotActive = 1012,
        CSOperationCancelled = 1013,
        CSInvalidParameters = 1014,
        CSUnknownUser = 1015,
        CSUnknownStation = 1016,
        CSUnknownGroup = 1017,
        CSIncompatibleVersion = 1100,
        CSInvalidUserStatus = 1102,
        CSClientServicesDown = 1103,
        CSNotInitialized = 1104,
        CSSystemNotFullyUp = 1105,
        CSInvalidUserName = 1111,
        CSInvalidStationName = 1113,
        CSGroupCacheNotActive = 1116,
        CSInvalidStatus = 1117,
        CSOutOfStationLicenses = 1200,
        CSCannotRemoteLoginLocalStation = 1201,
        CSUmMailBoxOnlyUserNotAllowedToLogOnAnyStation = 1202,
        CSUmUserNotAllowedToLogOnAnyWorkstation = 1203,
        CSOnlyStationLoginsAllowedOnThisStation = 1204,
        CSStationAlreadyExisted = 1300,
        CSAgentIsNotLoggedIn = 1301,
        CSStationDoesNotHaveRights = 1302,
        CSStationRightsAlreadyInUse = 1303,
        CSStationNotActive = 1304,
        CSUserDoesNotHaveRights = 1305,
        CSCannotLocalLoginOnRemoteStation = 1306,
        CSUserDoesNotHaveAllowTnChangeRights = 1307,
        CSNoClientLicense = 1308,
        CSUserConnectedOnAnotherStation = 1400,
        CSStationSpecifiedFromAnotherComputer = 1401,
        CSStationInUseByAnotherUser = 1402,
        CSRequiredLicenseNotAvailable = 1403,
        NoServerInformationCouldBeRetrieved = 32000,
        NotifierResponseTimeout = 32001,
        InternalExceptionOccurred = 32002,
        LoginFailed = 32003,
        InvalidClientServicesVersion = 32004,
        ClientServicesLoginFailed = 32005
    }

    public enum CallType
    {
        None, Normal, Dialer, Previewing
    }

    public enum CallbackType
    {
        CampaignWide, OwnAgent
    }

  }
}
