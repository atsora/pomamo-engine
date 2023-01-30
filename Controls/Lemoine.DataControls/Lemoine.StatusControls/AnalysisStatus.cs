// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.I18N;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.StatusControls
{
  /// <summary>
  /// Description of AnalysisStatus.
  /// </summary>
  public partial class AnalysisStatus : UserControl
  {
    private class PendingStatus
    {
      public int numberOfPendingModification = 0;
      public DateTime? oldestAnalysisAppliedDateTime = null;

      public int NumberOfPendingModification
      {
        get { return numberOfPendingModification; }
        set { numberOfPendingModification = value; }
      }

      public Nullable<DateTime> OldestAnalysisAppliedDateTime
      {
        get { return oldestAnalysisAppliedDateTime; }
        set { oldestAnalysisAppliedDateTime = value; }
      }
    }

    #region Members
    Timer m_refreshTimer = new Timer ();
    int m_refreshPeriod = 1000; // in ms
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (AnalysisStatus).FullName);

    #region Getters / Setters
    /// <summary>
    /// refresh timer
    /// </summary>
    Timer RefreshTimer
    {
      get { return m_refreshTimer; }
    }

    /// <summary>
    /// refresh period (in ms)
    /// </summary>
    int RefreshPeriod
    {
      get { return m_refreshPeriod; }
      set { m_refreshPeriod = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public AnalysisStatus ()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent ();
    }
    #endregion // Constructors

    #region Methods
    private void refreshAnalysisStatus ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;

      if (daoFactory == null) {
        // to allow use in designer
        return;
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        //Count number of new modifications throught Nhibernate query
        int newModification = ModelDAOHelper.DAOFactory.ModificationDAO
          .GetNumber (Lemoine.Model.AnalysisStatus.New);

        countNewLbl.Text = newModification + GetModificationText (newModification);

        //Count number of error modifications throught Nhibernate query
        int errorModification = ModelDAOHelper.DAOFactory.ModificationDAO
          .GetNumber (Lemoine.Model.AnalysisStatus.Error);
        countErrorLbl.Text = errorModification + GetModificationText (errorModification);

        //Count number of timeout modifications throught Nhibernate query
        int timeoutModification = ModelDAOHelper.DAOFactory.ModificationDAO
          .GetNumber (Lemoine.Model.AnalysisStatus.Timeout);
        timeoutModification += ModelDAOHelper.DAOFactory.ModificationDAO
          .GetNumber (Lemoine.Model.AnalysisStatus.DatabaseTimeout);
        countTimeoutLabel.Text = timeoutModification + GetModificationText (timeoutModification);
      }
    }

    private String GetModificationText (int value)
    {
      if (value >= 1) {
        return " " + PulseCatalog.GetString ("Modifications");
      }
      else {
        return " " + PulseCatalog.GetString ("Modification");
      }
    }
    #endregion // Methods


    void RefreshBtnClick (object sender, EventArgs e)
    {
      refreshAnalysisStatus ();
    }

    void PeriodicRefreshCheckBoxCheckedChanged (object sender, EventArgs e)
    {
      if (this.periodicRefreshCheckBox.Checked) {
        this.RefreshTimer.Enabled = true;
        this.RefreshTimer.Interval = this.RefreshPeriod;
        this.RefreshTimer.Tick += new System.EventHandler (this.RefreshBtnClick);
      }
      else {
        this.RefreshTimer.Enabled = false;
      }
    }

    private void AnalysisStatus_Load (object sender, EventArgs e)
    {
      analysisStatusGrpBx.Text = PulseCatalog.GetString ("AnalysisStatus");
      newLbl.Text = PulseCatalog.GetString ("New") + " :";
      errorLbl.Text = PulseCatalog.GetString ("Error") + " :";

      refreshAnalysisStatus ();
    }
  }
}
