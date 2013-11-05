using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
//using System.Threading;
using System.Windows.Forms;

namespace CIC
{
  public partial class CallbackSummaryForm : Form
  {
  //  private delegate void WorkerEventHandler(QueryParameters queryParams, AsyncOperation asyncOp);
  //  private WorkerEventHandler m_WorkerDelegate;

  //  private SendOrPostCallback SendOrPostCompletionMethodDelegate;

  //  private string m_Workflow;
  //  private string m_UserId;

  //  public CallbackSummaryForm(string workflow, string userId)
  //  {
  //    InitializeComponent();

  //    m_Workflow = workflow;
  //    m_UserId = userId;

  //    SendOrPostCompletionMethodDelegate = new SendOrPostCallback(SendOrPostCompletionMethod);
  //  }

  //  public void BeginQuery(QueryParameters queryParams)
  //  {
  //    AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(queryParams);
  //    m_WorkerDelegate = new WorkerEventHandler(QueryWorker);
  //    m_WorkerDelegate.BeginInvoke(queryParams, asyncOp, null, null);
  //  }

  //  private void QueryWorker(QueryParameters queryParams, AsyncOperation asyncOp)
  //  {
  //    SqlConnection sqlConn = null;
  //    SqlCommand sqlCmd = null;
  //    int callbacksToday = 0;
  //    int callbacksTomorrow = 0;
  //    bool success = false;
  //    Exception savedException = null;

  //    try
  //    {
  //      sqlConn = new SqlConnection("Server=172.24.68.54;Database=I3_Dialer;User ID=sa;Password=locus123;");
  //      sqlConn.Open();

  //      string tablename;
  //      switch (queryParams.WorkflowId.Trim().ToLower())
  //      {
  //        case "poc - predictive":
  //          tablename = "I3_POCPREDICTIVE_CS0";
  //          break;
  //        case "poc - preview":
  //          tablename = "I3_POCPREVIEW_CS0";
  //          break;
  //        default:
  //          tablename = "";
  //          break;
  //      }

  //      sqlCmd = new SqlCommand("SELECT COUNT(*) FROM " + tablename + " " + 
  //        "WHERE agentid = '" + queryParams.AgentId + "' AND " +
  //        "schedtime BETWEEN '" + DateTime.Today.ToString("yyyy-MM-dd") + "' AND '" + DateTime.Today.AddDays(1).ToString("yyyy-MM-dd") + "'", sqlConn);
  //      callbacksToday = (int)sqlCmd.ExecuteScalar();

  //      sqlCmd.CommandText = "SELECT COUNT(*) FROM " + tablename + " " +
  //        "WHERE agentid = '" + queryParams.AgentId + "' AND " +
  //        "schedtime BETWEEN '" + DateTime.Today.AddDays(1).ToString("yyyy-MM-dd") + "' AND '" + DateTime.Today.AddDays(2).ToString("yyyy-MM-dd") + "'";
  //      callbacksTomorrow = (int)sqlCmd.ExecuteScalar();

  //      success = true;
  //    }
  //    catch (Exception ex)
  //    {
  //      savedException = ex;
  //    }
  //    finally
  //    {
  //      if (sqlConn != null) sqlConn.Close();
  //    }

  //    QueryCompletedEventArgs e;
  //    if (success == true)
  //      e = new QueryCompletedEventArgs(QueryStatus.Success, "Completed successfully.", callbacksToday, callbacksTomorrow);
  //    else
  //      e = new QueryCompletedEventArgs(QueryStatus.Failed, savedException.ToString(), 0, 0);
  //    SignalCompletion(e, asyncOp);
  //  }

  //  private void SignalCompletion(QueryCompletedEventArgs e, AsyncOperation asyncOp)
  //  {
  //    asyncOp.PostOperationCompleted(SendOrPostCompletionMethodDelegate, e);
  //  }

  //  private void SendOrPostCompletionMethod(object operationState)
  //  {
  //    QueryCompletedEventArgs e = (QueryCompletedEventArgs)operationState;
  //    if (e.Status == QueryStatus.Success)
  //    {
  //      CallbacksTodayTextBox.Text = e.CallbacksToday.ToString();
  //      CallbacksTomorowTextBox.Text = e.CallbacksTomorrow.ToString();
  //      StatusLabel.Text = "";
  //    }
  //    else
  //    {
  //      StatusLabel.Text = e.Message;
  //    }
  //    QueryProgressBar.Hide();
  //  }

  //  private void CallbackSummaryForm_Load(object sender, EventArgs e)
  //  {
  //    BeginQuery(new QueryParameters(m_Workflow, m_UserId));
  //  }

  //  private void CloseButton_Click(object sender, EventArgs e)
  //  {
  //    Close();
  //  }
  //}

  //public enum QueryStatus
  //{
  //  Success, Failed
  //}

  //public class QueryCompletedEventArgs : EventArgs
  //{
  //  int m_CallbacksToday;
  //  int m_CallbacksTomorrow;
  //  QueryStatus m_QueryStatus;
  //  string m_Message;

  //  public QueryCompletedEventArgs(QueryStatus status, string message, int callbacksToday, int callbacksTomorrow)
  //  {
  //    m_QueryStatus = status;
  //    m_Message = message;
  //    m_CallbacksToday = callbacksToday;
  //    m_CallbacksTomorrow = callbacksTomorrow;
  //  }

  //  public QueryStatus Status
  //  {
  //    get { return m_QueryStatus; }
  //  }

  //  public string Message
  //  {
  //    get { return m_Message; }
  //  }

  //  public int CallbacksToday
  //  {
  //    get { return m_CallbacksToday; }
  //  }

  //  public int CallbacksTomorrow
  //  {
  //    get { return m_CallbacksTomorrow; }
  //  }
  //}

  //public class QueryParameters
  //{
  //  private string m_WorkflowId;
  //  private string m_AgentId;

  //  public QueryParameters(string workflowId, string agentId)
  //  {
  //    m_WorkflowId = workflowId;
  //    m_AgentId = agentId;
  //  }

  //  public string WorkflowId
  //  {
  //    get { return m_WorkflowId; }
  //  }

  //  public string AgentId
  //  {
  //    get { return m_AgentId; }
  //  }
      public string LocalImagePath;
      public String ApplicationPath;
      public string SoundWavPath;
  }
}