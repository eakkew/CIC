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
  public partial class UserStatusGridView : UserControl
  {
    private BindingSource m_BindingSource = null;
    private ContactDirectory m_ContactDirectory;
    private DirectoryConfiguration m_DirectoryConfiguration;
    private DirectoriesManager m_DirectoryManager;
    private PeopleManager m_PeopleManager;
    private int idx = 0;
    private ReadOnlyCollection<ContactEntry> contacts;
    private System.Drawing.Point MousePoint = new Point();
    public delegate void RowSelectedHandler(object sender, EventArgs e);
    public event RowSelectedHandler RowSelected;
    public delegate void EventHandler(object sender, CIC.UserDataEventArgs e);
    public event EventHandler ContextMenuEvent;

    public void ClearItem()
    {
        UserStatusDataGridView.DataSource = null;
        ctnUserMenu.Items.Clear();
    }

    public UserStatusGridView()
    {
      InitializeComponent();
    }

    public void Initialize(Session session)
    {
      string scope = this.Name + "::Initialize : ";
      Tracing.TraceStatus(scope + "Initializing UserStatusGridView");
      if (session == null)
      {
          throw new ArgumentNullException("session");
      }
      try
      {
        UserStatusDataGridView.Rows.Clear();
        m_BindingSource = new BindingSource();
        // Initialize column bindings
        UserStatusDataGridView.AutoGenerateColumns = false;
        UserStatusDataGridView.DataSource = m_BindingSource;
        UserStatusDataGridView.Columns["AvailabilityColumn"].DataPropertyName = "AvailabilityIcon";
        UserStatusDataGridView.Columns["UserColumn"].DataPropertyName = "Name";
        UserStatusDataGridView.Columns["StatusColumn"].DataPropertyName = "Status";

        // Initialize DirectoryManager and PeopleManager
        Tracing.TraceNote(scope + "Getting instance of ININ.IceLib.Directories.DirectoryManager");
        m_DirectoryManager = DirectoriesManager.GetInstance(session);

        Tracing.TraceNote(scope + "Getting instance of ININ.IceLib.People.PeopleManager");
        m_PeopleManager = PeopleManager.GetInstance(session);

        // Get list of directories from server
        Tracing.TraceNote(scope + "Creating a new instance of ININ.IceLib.Directories.DirectoryConfiguration");
        m_DirectoryConfiguration = new DirectoryConfiguration(m_DirectoryManager);

        Tracing.TraceNote(scope + "Start watch on DirectoryConfiguration to retrieve list of directories from server");
        m_DirectoryConfiguration.StartWatching();
        ReadOnlyCollection<DirectoryMetadata> directories = m_DirectoryConfiguration.GetList();
        m_DirectoryConfiguration.StopWatching();

        // Look for Company Directory directory entry
        Tracing.TraceNote(scope + "Looking through results of watch to find Company Directory");
        foreach (DirectoryMetadata directoryEntry in directories)
        {
          Tracing.TraceNote(scope + "Directory Id=" + directoryEntry.Id + ", Category=" + directoryEntry.Category);
          if (directoryEntry.Category == DirectoryMetadataCategory.Company)
          {
            Tracing.TraceNote(scope + "Found Company Directory");

            // Get list of contacts for the Company Directory
            Tracing.TraceNote(scope + "Getting list of contacts in Company Directory");
            Tracing.TraceNote(scope + "Creating new instance of ININ.IceLib.Directories.ContactDirectory for Company Directory");
            m_ContactDirectory = new ContactDirectory(m_DirectoryManager, directoryEntry);

            Tracing.TraceNote(scope + "Start watch on ContactDirectory to retrieve list of contacts in Company Directory");
            m_ContactDirectory.StartWatching();
            //ReadOnlyCollection<ContactEntry> contacts = m_ContactDirectory.GetList();
            contacts = m_ContactDirectory.GetList();
            m_ContactDirectory.StopWatching();

            Tracing.TraceNote(scope + "Found " + contacts.Count.ToString() + " contact(s) in Company Directory. Populating internal list");

            // Add each contact in Company Directory into a BindingSource for the DataGridView
            for (int i = 0; i < contacts.Count; i++)
            {
              m_BindingSource.Add(new UserData(contacts[i], m_PeopleManager, IconsImageList));
            }
            break;
          }
        }
      }
      catch (Exception ex)
      {
        Tracing.TraceException(ex, "Exception occurred while initializing UserStatusGridView");
        throw ex;
      }
    }

    protected virtual void OnRowSelected(EventArgs e)
    {
       //
    }

    public UserData SelectedItem
    {
      get
      {
          if (UserStatusDataGridView.SelectedRows.Count <= 0)
          {
              return null;
          }
          else
          {
              return (UserData)UserStatusDataGridView.SelectedRows[0].DataBoundItem;
          }
      }
    }

    private void UserStatusDataGridView_RowEnter(object sender, DataGridViewCellEventArgs e)
    {
        //
    }

    private void UserStatusDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
        if (this.UserStatusDataGridView.Rows.Count > 0)
        {
            CIC.UserData TmpUsrData = (UserData)UserStatusDataGridView.SelectedRows[0].DataBoundItem;
            if (TmpUsrData != null)
            {
                this.setUserDataMenu(TmpUsrData);
            }
        }
    }
    
    private void setUserDataMenu(CIC.UserData mUsrData)
    {
        try
        {
            for (idx = 0; idx < contacts.Count; idx++)
            {
                if (contacts[idx].UserId == mUsrData.UserId)
                {
                    if (contacts[idx].Extension.ToString().Trim() != "")
                    {
                        this.callToolStripMenuItem.Text = "Phone : " + contacts[idx].Extension.ToString();
                    }
                    else
                    {
                        this.callToolStripMenuItem.Text = "Phone : <Empty>";
                    }
                    if (contacts[idx].MobilePhone.ToString().Trim() != "")
                    {
                        this.mobiletoolStripMenuItem.Text = "Mobile : " + contacts[idx].MobilePhone.ToString();
                    }
                    else
                    {
                        this.mobiletoolStripMenuItem.Text = "Mobile : <Empty>";
                    }
                    if (contacts[idx].BusinessEmail.ToString().Trim() != "")
                    {
                        this.mailToToolStripMenuItem.Text = "Email : " + contacts[idx].BusinessEmail.ToString();
                    }
                    else
                    {
                        this.mailToToolStripMenuItem.Text = "Email : <Empty>";
                    }
                    break;
                }
            }
        }
        catch (System.Exception ex)
        {
            Tracing.TraceException(ex, "Exception occurred while initializing UserStatusGridView");
            throw ex;
        }
    }

    public void ClearRowsSelection()
    {
        if (this.UserStatusDataGridView.Rows.Count > 0)
        {
            if (UserStatusDataGridView.SelectedRows.Count > 0)
            {
                UserStatusDataGridView.ClearSelection();
            }
        }
    }

    public bool IsRowsSelected()
    {
        if(UserStatusDataGridView.SelectedRows.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void UserStatusDataGridView_MouseEnter(object sender, EventArgs e)
    {
        //
    }
    
    private void UserStatusDataGridView_MouseMove(object sender, MouseEventArgs e)
    {
        //get Mouse point
        MousePoint.X = e.X;
        MousePoint.Y = e.Y;
    }
   
    private void callToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (ContextMenuEvent != null)
        {
            CIC.UserDataEventArgs objArgs = new UserDataEventArgs();
            objArgs.userData = this.SelectedItem;
            objArgs.MenuName = "callToolStripMenuItem";
            objArgs.MenuValue = contacts[idx].Extension.ToString();
            ContextMenuEvent(this.callToolStripMenuItem, objArgs);
        }
    }

    private void mobiletoolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (ContextMenuEvent != null)
        {
            CIC.UserDataEventArgs objArgs = new UserDataEventArgs();
            objArgs.userData = this.SelectedItem;
            objArgs.MenuName = "mobiletoolStripMenuItem";
            objArgs.MenuValue = contacts[idx].MobilePhone.ToString();
            ContextMenuEvent(this.callToolStripMenuItem, objArgs);
        }
    }

    private void mailToToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (ContextMenuEvent != null)
        {
            CIC.UserDataEventArgs objArgs = new UserDataEventArgs();
            objArgs.userData = this.SelectedItem;
            objArgs.MenuName = "mailToToolStripMenuItem";
            objArgs.MenuValue = contacts[idx].BusinessEmail.ToString();
            ContextMenuEvent(this.callToolStripMenuItem, objArgs);
        }
    }

    private void UserStatusDataGridView_SelectionChanged(object sender, EventArgs e)
    {
        try
        {
            if (this.UserStatusDataGridView.Rows.Count > 0)
            {
                CIC.UserData TmpUsrData = (UserData)UserStatusDataGridView.SelectedRows[0].DataBoundItem;
                if (TmpUsrData != null)
                {
                    this.setUserDataMenu(TmpUsrData);
                }
                if (RowSelected != null)
                {
                    RowSelected(this, e);
                }
            }
        }
        catch
        {
            //
        }
    }

    private void UserStatusDataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
    {
        try
        {
            if (this.UserStatusDataGridView.Rows.Count > 0)
            {
                CIC.UserData TmpUsrData = (UserData)UserStatusDataGridView.Rows[e.RowIndex].DataBoundItem;
                if (TmpUsrData != null)
                {
                    this.setUserDataMenu(TmpUsrData);
                }
                UserStatusDataGridView.Rows[e.RowIndex].Selected = true;
                if (RowSelected != null)
                {
                    RowSelected(this, e);
                }
            }
        }
        catch
        {
            //
        }
    }

  }
}