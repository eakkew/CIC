using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ININ.IceLib;
using ININ.IceLib.Connection;
using ININ.IceLib.Directories;
using ININ.IceLib.People;

namespace CIC
{
    public class UserData : System.ComponentModel.INotifyPropertyChanged
    {
        ININ.IceLib.Directories.ContactEntry m_ContactEntry;
        System.Windows.Forms.ImageList m_ImageList;
        ININ.IceLib.People.UserStatus m_UserStatus;
        ININ.IceLib.People.UserStatusList m_UserStatusList;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public UserData(ContactEntry contactEntry, PeopleManager peopleManager, ImageList imageList)
        {
            string scope = "UserData::UserData : ";

            Tracing.TraceVerbose(scope + "Initializing new instance of UserData");

            if (contactEntry == null) throw new ArgumentNullException("contactEntry");
            if (imageList == null) throw new ArgumentNullException("imageList");

            m_ContactEntry = contactEntry;
            m_ImageList = imageList;

            Tracing.TraceVerbose(scope + "Creating a new instance of UserStatusList");
            m_UserStatusList = new UserStatusList(peopleManager);

            Tracing.TraceVerbose(scope + "Attaching event watchers to watch for changes in user");
            m_UserStatusList.WatchedObjectsChanged += new EventHandler<WatchedObjectsEventArgs<UserStatusProperty>>(m_UserStatusList_WatchedObjectsChanged);

            Tracing.TraceVerbose(scope + "Start watching changes in status for user=" + contactEntry.UserId);
            m_UserStatusList.StartWatching(new string[] { contactEntry.UserId });
        }

        public Image AvailabilityIcon
        {
            get
            {
                Tracing.TraceVerbose("UserData::AvailabilityIcon : Property get. UserData instance=" + m_ContactEntry.UserId);
                if (m_UserStatus == null) m_UserStatus = m_UserStatusList[m_ContactEntry.UserId];

                if (m_UserStatus.OnPhone == true)
                    return m_ImageList.Images["OnPhone"];
                else if (m_UserStatus.LoggedIn == false)
                    return m_ImageList.Images["Offline"];
                else if (m_UserStatus.StatusMessageDetails.IsDoNotDisturbStatus == true)
                    return m_ImageList.Images["NotAvailable"];
                else
                    return m_ImageList.Images["Available"];
            }
        }

        public bool AvailableForCall
        {
            get
            {
                string scope = "UserData::AvailableForCall : ";

                Tracing.TraceVerbose(scope + "Property get. UserData instance=" + m_ContactEntry.UserId);
                if (m_UserStatus == null) m_UserStatus = m_UserStatusList[m_ContactEntry.UserId];

                if (m_UserStatus.OnPhone == true)
                {
                    Tracing.TraceVerbose(scope + "User is on the phone, not available for call");
                    return false;
                }
                else if (m_UserStatus.LoggedIn == false)
                {
                    Tracing.TraceVerbose(scope + "User is not logged in, not available for call");
                    return false;
                }
                else if (m_UserStatus.StatusMessageDetails.IsDoNotDisturbStatus == true)
                {
                    Tracing.TraceVerbose(scope + "User is in DND status, not available for call");
                    return false;
                }
                else
                {
                    Tracing.TraceVerbose(scope + "User is available for call");
                    return true;
                }
            }
        }

        public string Extension
        {
            get
            {
                Tracing.TraceVerbose("UserData::Extension : Property get. UserData instance=" + m_ContactEntry.UserId + ", result=" + m_ContactEntry.Extension);
                return m_ContactEntry.Extension;
            }
        }

        private void m_UserStatusList_WatchedObjectsChanged(object sender, WatchedObjectsEventArgs<UserStatusProperty> e)
        {
            string scope = "UserData::m_UserStatusList_WatchedObjectsChanged : ";
            Tracing.TraceVerbose(scope + "Received event for user=" + m_ContactEntry.UserId);
            if (e.Changed.Count > 0)
            {
                Tracing.TraceVerbose(scope + "Event is an ObjectChanged event. Event count=" + e.Changed.Count.ToString());

                // Only interested in Object Changed event for the user in m_ContactEntry
                ReadOnlyCollection<UserStatusProperty> userStatusProperties;

                Tracing.TraceVerbose(scope + "Retrieving list of ObjectChanged events for user=" + m_ContactEntry.UserId);
                if (e.Changed.TryGetValue(m_ContactEntry.UserId, out userStatusProperties) == true)
                {
                    foreach (UserStatusProperty property in userStatusProperties)
                    {
                        Tracing.TraceVerbose(scope + "Event=" + property.ToString());
                        switch (property)
                        {
                            case UserStatusProperty.StatusMessageDetails:
                                Tracing.TraceVerbose(scope + "Raising OnPropertyChanged event");
                                OnPropertyChanged(new PropertyChangedEventArgs("Status"));
                                OnPropertyChanged(new PropertyChangedEventArgs("AvailabilityIcon"));
                                break;
                            case UserStatusProperty.OnPhone:
                                Tracing.TraceVerbose(scope + "Raising OnPropertyChanged event");
                                OnPropertyChanged(new PropertyChangedEventArgs("AvailabilityIcon"));
                                break;
                            case UserStatusProperty.Stations:
                                Tracing.TraceVerbose(scope + "Raising OnPropertyChanged event");
                                OnPropertyChanged(new PropertyChangedEventArgs("AvailabilityIcon"));
                                break;
                            default:
                                Tracing.TraceVerbose(scope + "Not interested in this event. Event=" + property.ToString());
                                break;
                        }
                    }
                }
            }
            else
                Tracing.TraceVerbose(scope + "Not an ObjectChanged event. Not interested");
        }

        public string Name
        {
            get
            {
                Tracing.TraceVerbose("UserData::Name : Property get. UserData instance=" + m_ContactEntry.UserId + ", result=" + m_ContactEntry.GetDisplayableIdentifier());
                return m_ContactEntry.GetDisplayableIdentifier();
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null) PropertyChanged(this, e);
        }

        public string Status
        {
            get
            {
                Tracing.TraceVerbose("UserData::Status : Property get. UserData instance=" + m_ContactEntry.UserId + ", result=" + m_UserStatusList[m_ContactEntry.UserId].StatusMessageDetails.MessageText);
                return m_UserStatusList[m_ContactEntry.UserId].StatusMessageDetails.MessageText;
            }
        }

        public string UserId
        {
            get
            {
                Tracing.TraceVerbose("UserData::UserId : Property get. UserData instance=" + m_ContactEntry.UserId + ", result=" + m_ContactEntry.UserId);
                return m_ContactEntry.UserId;
            }
        }
    }
}
