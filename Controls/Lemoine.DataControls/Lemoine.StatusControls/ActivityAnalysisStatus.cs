// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.StatusControls
{
  /// <summary>
  /// Description of ActivityAnalysisStatus.
  /// </summary>
  public partial class ActivityAnalysisStatus : UserControl
  {
    #region Members
    Timer m_refreshTimer = new Timer();
    int m_refreshPeriod = 1000; // in ms
    IMonitoredMachine m_currentMonitoredMachine = null;
    DateTime m_initialDateTime;
    int m_initialCount;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ActivityAnalysisStatus).FullName);

    #region Getters / Setters
    
    /// <summary>
    /// refresh timer
    /// </summary>
    Timer RefreshTimer {
      get { return m_refreshTimer; }
    }
    
    /// <summary>
    /// refresh period (in ms)
    /// </summary>
    int RefreshPeriod {
      get { return m_refreshPeriod; }
      set { m_refreshPeriod = value; }
    }
    
    /// <summary>
    /// current monitored machine
    /// </summary>
    IMonitoredMachine CurrentMonitoredMachine {
      get { return m_currentMonitoredMachine; }
      set { m_currentMonitoredMachine = value; }
    }
    
    
    /// <summary>
    /// date time when current monitored machine was first selected
    /// </summary>
    DateTime InitialDateTime {
      get { return m_initialDateTime; }
      set { m_initialDateTime = value; }
    }
    
    /// <summary>
    /// analysis count when current monitored machine was first selected
    /// </summary>
    int InitialCount {
      get { return m_initialCount; }
      set { m_initialCount = value; }
    }
    
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ActivityAnalysisStatus()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      this.monitoredMachineSelection1.Nullable = true;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// refresh the analysis status
    /// </summary>
    void refreshAnalysisStatus ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if (daoFactory == null) {
        // to allow use in designer
        return;
      }
      
      if (null == m_currentMonitoredMachine) {
        return;
      }

      lock (m_currentMonitoredMachine)
      {
        using (IDAOSession daoSession = daoFactory.OpenSession ()) {
          bool IsNewMachine = false;
          
          IMonitoredMachineAnalysisStatus lastAnalysisStatus;
          
          if (this.monitoredMachineSelection1.SelectedMonitoredMachines.Count > 0) {
            IMonitoredMachine selectedMonitoredMachine =
              this.monitoredMachineSelection1.SelectedMonitoredMachines[0];
            lastAnalysisStatus = ModelDAOHelper.DAOFactory.MonitoredMachineAnalysisStatusDAO
              .FindById (selectedMonitoredMachine.Id);
            if (CurrentMonitoredMachine != selectedMonitoredMachine) {
              CurrentMonitoredMachine = selectedMonitoredMachine;
              IsNewMachine = true;
            }
          }
          else {
            lastAnalysisStatus = ModelDAOHelper.DAOFactory.MonitoredMachineAnalysisStatusDAO
              .GetLast ();
            CurrentMonitoredMachine = null;
          }

          DateTime lastActivityAnalysisDateTime =
            lastAnalysisStatus.ActivityAnalysisDateTime;
          
          IMonitoredMachine lastMachine = lastAnalysisStatus.MonitoredMachine;
          
          this.lastActivityAnalysisDateTimeTextBox.Text =
            lastActivityAnalysisDateTime.ToString();
          
          this.lastActivityAnalysisMachineTextBox.Text =
            lastMachine.Name;
          
          this.lastActivityAnalysisCountBox.Text =
            lastAnalysisStatus.ActivityAnalysisCount.ToString();
          
          bool IsAverageAvailable = false;
          if (IsNewMachine) {
            InitialDateTime = lastActivityAnalysisDateTime;
            InitialCount = lastAnalysisStatus.ActivityAnalysisCount;
          } else {
            if (CurrentMonitoredMachine != null) {
              TimeSpan currentTimeSpan = lastActivityAnalysisDateTime - InitialDateTime;
              double nbSeconds = currentTimeSpan.TotalSeconds;
              if (nbSeconds >= 1.0) {
                this.averageTextBox.Text =
                  String.Format("{0:0.00}",
                                ((lastAnalysisStatus.ActivityAnalysisCount - InitialCount) * 10.0)/ nbSeconds);
                IsAverageAvailable = true;
              }
            }
          }
          
          if (!IsAverageAvailable) {
            this.averageTextBox.Text = "N/A";
          }
        }
      }
      
      
    }
    #endregion // Methods
    
    void RefreshBtnClick(object sender, EventArgs e)
    {
      refreshAnalysisStatus();
    }
    
    
    void PeriodicRefreshCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      if (this.periodicRefreshCheckBox.Checked) {
        this.RefreshTimer.Enabled = true;
        this.RefreshTimer.Interval = this.RefreshPeriod;
        this.RefreshTimer.Tick += new System.EventHandler(this.RefreshBtnClick);
      } else {
        this.RefreshTimer.Enabled = false;
      }
    }
    
    void DoResetCount() {
      if (this.monitoredMachineSelection1.SelectedMonitoredMachines.Count > 0) {
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        IMonitoredMachine selectedMonitoredMachine =
          this.monitoredMachineSelection1.SelectedMonitoredMachines[0];
        using (IDAOSession daoSession = daoFactory.OpenSession ())
          using(IDAOTransaction transaction = daoSession.BeginTransaction("ActivityAnalysisStatus.ResetCount"))
        {
          IMonitoredMachineAnalysisStatus lastAnalysisStatus = ModelDAOHelper.DAOFactory.MonitoredMachineAnalysisStatusDAO
            .FindById (selectedMonitoredMachine.Id);

          lastAnalysisStatus.ActivityAnalysisCount = 0;
                    
          ModelDAOHelper.DAOFactory
            .MonitoredMachineAnalysisStatusDAO.MakePersistent(lastAnalysisStatus);
          
          transaction.Commit();
        }
      }
      
      // in order to reset current machine information
      this.CurrentMonitoredMachine = null;
      refreshAnalysisStatus();
    }

    private void ActivityAnalysisStatus_Load (object sender, EventArgs e)
    {
      refreshAnalysisStatus ();
    }
  }
}
