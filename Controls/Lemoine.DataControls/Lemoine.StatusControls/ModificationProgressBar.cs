// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.StatusControls
{
  /// <summary>
  /// Description of ModificationProgressBar.
  /// </summary>
  public partial class ModificationProgressBar : UserControl
  {
    readonly ILog log = LogManager.GetLogger<ModificationProgressBar> ();

    #region Events
    /// <summary>
    /// Event emitted when the progress is 100%
    /// </summary>
    public event Action Finished;
    
    /// <summary>
    /// Event emitted when an operation took a long time
    /// </summary>
    public event Action TimeOut;
    #endregion // Events

    #region Getters / Setters
    /// <summary>
    /// Timeout duration in seconds
    /// By default 0: no timeout
    /// </summary>
    [DefaultValue(0)]
    [Description("Timeout duration in seconds. If 0: no timeout.")]
    public int TimeOutDuration { get; set; }
    
    /// <summary>
    /// Current progress
    /// </summary>
    public int Progress
    {
      get { return this.analysisProgressBar.Value; }
    }
    
    /// <summary>
    /// list of modifications for which progress is tracked
    /// </summary>
    ICollection<IModification> Modifications { get; set; }
    
    /// <summary>
    /// BackgroundWorker for measuring analysis progress
    /// </summary>
    BackgroundWorker BackgroundWorker { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ModificationProgressBar()
    {
      TimeOutDuration = 0;
      BackgroundWorker = null;
      Modifications = new List<IModification>();
      InitializeComponent();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Launch analysis progress tracking
    /// </summary>
    public void AnalysisProgressLoad()
    {
      BackgroundWorker = new BackgroundWorker();
      BackgroundWorker.WorkerReportsProgress = true;
      BackgroundWorker.DoWork += new DoWorkEventHandler(BackgroundWorkerCheckAnalysisStatus);
      BackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundProgressCompleted);
      BackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(BackgroundProgressChanged);
      BackgroundWorkerStart(this, null);
    }
    
    /// <summary>
    /// Add collection of (would-be) modifications to track
    /// </summary>
    public void AddModifications (ICollection modifications)
    {
      foreach(object obj in modifications) {
        IModification modification = obj as IModification;
        if (modification != null) {
          this.Modifications.Add(modification);
        }
      }
    }
    
    /// <summary>
    /// Add collection of (would-be) modifications to track
    /// </summary>
    public void AddModifications (IEnumerable<IModification> modifications)
    {
      foreach (IModification modification in modifications) {
        this.Modifications.Add(modification);
      }
    }
    
    /// <summary>
    /// Launch background worker
    /// </summary>
    private void BackgroundWorkerStart(object sender, EventArgs e)
    {
      if (this.BackgroundWorker.IsBusy != true) {
        // Start the asynchronous operation.
        Cursor = Cursors.AppStarting;
        this.BackgroundWorker.RunWorkerAsync(this.Modifications);
      }
    }

    /// <summary>
    /// return the number of modifications not yet treated
    /// lower than a given id <paramref name="maxId"/>
    /// </summary>
    /// <param name="maxId"></param>
    /// <returns></returns>
    private double GetNumberOfRemainingModifications(long maxId)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        return ModelDAOHelper.DAOFactory.ModificationDAO.GetNumberOfRemainingModifications(maxId);
      }
    }
    
    /// <summary>
    /// Background worker for checking status of shift changes analysis
    /// </summary>
    private void BackgroundWorkerCheckAnalysisStatus(object sender, DoWorkEventArgs e)
    {
      BackgroundWorker bw = sender as BackgroundWorker;
      ICollection<IModification> allModifications = (ICollection<IModification>) e.Argument;
      
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      
      long maxId = allModifications
        .Aggregate<IModification, long> (-1, (max, next) => (max < ((Lemoine.Collections.IDataWithId<long>)next).Id)
                                         ? ((Lemoine.Collections.IDataWithId<long>)next).Id
                                         : max);
      
      double initialModificationCount;
      try {
        initialModificationCount = this.GetNumberOfRemainingModifications (maxId);
      }
      catch (Exception ex) {
        log.Error ("BackgroundWorkerCheckAnalysis: exception", ex);
        throw;
      }
      double currentModificationCount = initialModificationCount;
      double percentProgress = 0;
      
      bw.ReportProgress(0);
      
      int msWaited = 0;
      int msPause = 1000;
      while ((currentModificationCount > 0) && (percentProgress < 100)) {
        bool isProgress = false;
        
        double newModificationCount = this.GetNumberOfRemainingModifications(maxId);
        
        if (newModificationCount != currentModificationCount) {
          isProgress = true;
          currentModificationCount = newModificationCount;
          if (currentModificationCount == 0) {
            percentProgress = 100;
          }
          else {
            double newProgress = ((initialModificationCount - currentModificationCount) * 100) / initialModificationCount;
            if (newProgress > percentProgress) {
              percentProgress = newProgress;
            }
          }
        }
        
        if (isProgress) {
          msWaited = 0;
          bw.ReportProgress((int)percentProgress);
        }
        else if (TimeOutDuration > 0) {
          msWaited += msPause;
          if (msWaited > 1000 * TimeOutDuration) {
            if (TimeOut != null) {
              TimeOut ();
            }

            return;
          }
        }
        
        if (percentProgress < 100) {
          System.Threading.Thread.Sleep(msPause);
        }
      }
    }
    
    /// <summary>
    /// Report progress on background work
    /// </summary>
    private void BackgroundProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      this.analysisProgressBar.Value = e.ProgressPercentage;
    }

    /// <summary>
    /// called when background work is completed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BackgroundProgressCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      Cursor = Cursors.Default;
      if (Finished != null) {
        Finished ();
      }

      Dispose ();
    }
    #endregion // Methods
  }
}
