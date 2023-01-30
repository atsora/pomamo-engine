// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Lemoine.ModelDAO;
using Lemoine.Model;
using Lemoine.StatusControls;

using Lemoine.Core.Log;
using System.Linq;

namespace Lem_ManualActivity
{
  /// <summary>
  /// Description of ManualActivityProgressForm.
  /// </summary>
  public partial class ManualActivityProgressForm : Form
  {
    #region Members
    IMonitoredMachine m_monitoredMachine;
    IMachineMode m_machineMode;
    DateTime m_beginTime;
    UpperBound<DateTime> m_endTime;
    bool m_noEndDate;
    ICollection<IActivityManual> m_activityManualAssociations = new List<IActivityManual> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ManualActivityProgressForm).FullName);

    #region Getters / Setters

    /// <summary>
    /// Monitored machine
    /// </summary>
    public IMonitoredMachine MonitoredMachine
    {
      get { return m_monitoredMachine; }
      set { m_monitoredMachine = value; }
    }

    /// <summary>
    /// Machine mode
    /// </summary>
    public IMachineMode MachineMode
    {
      get { return m_machineMode; }
      set { m_machineMode = value; }
    }

    /// <summary>
    /// Begin of Manual activity association
    /// </summary>
    public DateTime BeginTime
    {
      get { return m_beginTime; }
      set { m_beginTime = value; }
    }

    /// <summary>
    /// End of Manual activity associations
    /// </summary>
    public UpperBound<DateTime> EndTime
    {
      get { return m_endTime; }
      set { m_endTime = value; }
    }

    /// <summary>
    /// Do we have and end date
    /// </summary>
    public bool IsEndDate
    {
      get { return m_noEndDate; }
      set { m_noEndDate = value; }
    }

    /// <summary>
    /// List of all persistents Manual Activity
    /// </summary>
    public ICollection<IActivityManual> ActivityManualAssociations
    {
      get { return m_activityManualAssociations; }
      set { m_activityManualAssociations = value; }
    }

    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ManualActivityProgressForm ()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent ();

      this.Load += new System.EventHandler (this.ManualActivityProgressFormLoad);
    }

    #endregion // Constructors

    #region Methods

    #endregion // Methods

    void ManualActivityProgressFormLoad (object sender, EventArgs e)
    {
      IActivityManual activityManual;

      using (var daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        IRevision revision = ModelDAOHelper.ModelFactory.CreateRevision ();
        revision.Application = Lemoine.Info.ProgramInfo.Name;
        revision.IPAddress = Lemoine.Info.ComputerInfo.GetIPAddresses ().FirstOrDefault ();

        activityManual = ModelDAOHelper.ModelFactory.CreateActivityManual (this.MonitoredMachine,
                                                                           this.MachineMode,
                                                                           new UtcDateTimeRange (BeginTime, EndTime));

        activityManual.Revision = revision;
        this.ActivityManualAssociations.Add (activityManual);

        ModelDAOHelper.DAOFactory.RevisionDAO.MakePersistent (revision);
        ModelDAOHelper.DAOFactory.ActivityManualDAO.MakePersistent (activityManual);

        transaction.Commit ();
      }

      this.modificationProgressBar1.AddModifications ((System.Collections.ICollection)this.ActivityManualAssociations);
      this.modificationProgressBar1.AnalysisProgressLoad ();
    }

    void ModificationProgressBar1Disposed (object sender, EventArgs e)
    {
      if (100 <= this.modificationProgressBar1.Progress) {
        MessageBox.Show ("Manual activity processed with success", "Lem_ManualActivity", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
      this.Dispose ();
    }

    private void ManualActivityProgressForm_FormClosed (object sender, FormClosedEventArgs e)
    {
      if ((this.modificationProgressBar1?.Progress ?? 100) < 100) {
        MessageBox.Show ("Analysis still in progress", "Lem_ManualActivity", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
    }
  }
}
