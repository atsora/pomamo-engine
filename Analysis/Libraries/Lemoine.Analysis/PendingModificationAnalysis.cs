// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Lemoine.Core.ExceptionManagement;
using Lemoine.GDBPersistentClasses;
using Lemoine.Info;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Threading;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
using Lemoine.Core.Performance;
using System.Threading.Tasks;
using Lemoine.Collections;

namespace Lemoine.Analysis
{
  /// <summary>
  /// Get the modifications that have not been completed yet and process them.
  /// 
  /// Try to get first in batch the past modifications and run them.
  /// Then run one by one the present modifications.
  /// </summary>
  public abstract class PendingModificationAnalysis<I, T> : ThreadClass, IThreadClass, IChecked
    where I : IModification
    where T : Modification, I //, Lemoine.Threading.IChecked, Lemoine.Collections.IDataWithId<long>
  {
    /// <summary>
    /// Pending modification progress
    /// </summary>
    public struct PendingModificationsProgress
    {
      /// <summary>
      /// Create
      /// </summary>
      /// <param name="lastModificationId"></param>
      /// <param name="lastPriority"></param>
      /// <returns></returns>
      public static PendingModificationsProgress CreateDefault ()
      {
        return new PendingModificationsProgress (0, int.MaxValue, 0);
      }

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="lastModificationId"></param>
      /// <param name="lastPriority"></param>
      /// <param name="minPriority"></param>
      /// <returns></returns>
      PendingModificationsProgress (long lastModificationId, int lastPriority, int minPriority)
      {
        this.LastModificationId = lastModificationId;
        this.LastPriority = lastPriority;
        this.MinPriority = minPriority;
      }

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="modification"></param>
      /// <param name="minPriority"></param>
      public PendingModificationsProgress (IModification modification, int minPriority)
        : this (((IDataWithId<long>)modification).Id, modification.StatusPriority, minPriority)
      {
      }

      /// <summary>
      /// Last modification id
      /// </summary>
      public long LastModificationId { get; private set; }

      /// <summary>
      /// Last priority
      /// </summary>
      public int LastPriority { get; private set; }

      /// <summary>
      /// Reference to the minimum priority that was used during the analysis
      /// </summary>
      public int MinPriority { get; private set; }

      /// <summary>
      /// To string method
      /// </summary>
      /// <returns></returns>
      public override string ToString ()
      {
        return $"PendingModificationsProcess LastPriority={this.LastPriority} LastModificationId={this.LastModificationId} MinPriority={MinPriority}";
      }
    }

    static readonly string EXIT_IF_THREAD_ABORT_KEY = "Analysis.Modification.ExitIfThreadAbort";
    static readonly bool EXIT_IF_THREAD_ABORT_DEFAULT = true;

    /// <summary>
    /// Default maximum time to spend in the analysis of the pending modifications
    /// </summary>
    static readonly string MAX_TIME_PENDING_MODIFICATIONS_RUN_KEY =
      "Analysis.Activity.PendingModifications.MaxTime.Run";
    static readonly TimeSpan MAX_TIME_PENDING_MODIFICATIONS_RUN_DEFAULT = TimeSpan.FromDays (1);

    static readonly string TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_KEY = "Analysis.PendingModificationAnalysis.TemporaryWithDelayException.Sleep";
    static readonly TimeSpan TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_DEFAULT = TimeSpan.FromSeconds (2);

    #region Members
    bool m_disposed = false;
    bool m_threadExecution = true;
    TimeSpan m_modificationTimeout = TimeSpan.FromMinutes (2);
    TimeSpan m_modificationStepTimeout = TimeSpan.FromSeconds (40);
    volatile bool m_exitRequested = false;

    readonly ActivityAnalysis m_activityAnalysis = null;

    // Properties for optimization
    bool m_cleanRequired = true; // From a previous partial execution
    long? m_maxModificationIdAllCompleted;
    long m_limitNewAnalysisStatus = -1; // Not set
    bool m_fillAnalysisStatusCompleted = false;

    PendingModificationsProgress m_progress = PendingModificationsProgress.CreateDefault ();

    /// <summary>
    /// Extensions
    /// </summary>
    protected IEnumerable<Lemoine.Extensions.Analysis.IPendingModificationAnalysisExtension> m_extensions;

    IChecked m_checkedParent = null;
    #endregion // Members

    readonly ILog log = LogManager.GetLogger (typeof (PendingModificationAnalysis<I, T>).FullName);

    #region Getters / Setters
    /// <summary>
    /// Run the analysis in a thread (default, for debugging purpose, you can turn this to false)
    /// </summary>
    internal protected bool ThreadExecution
    {
      get { return m_threadExecution; }
      set { m_threadExecution = value; }
    }

    /// <summary>
    /// Activity analysis
    /// </summary>
    protected ActivityAnalysis ActivityAnalysis
    {
      get { return m_activityAnalysis; }
    }

    /// <summary>
    /// Reference to the checked parent
    /// </summary>
    public IChecked CheckedParent
    {
      get { return m_checkedParent; }
      set { m_checkedParent = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// 
    /// Run by default the analysis in a thread
    /// 
    /// Do not use this constructor in unit tests, because the Rollback hack does not work with it
    /// </summary>
    protected PendingModificationAnalysis ()
      : this (null)
    {
      GetExtensions ();
    }

    /// <summary>
    /// Constructor
    /// 
    /// Run by default the analysis in a thread
    /// 
    /// Do not use this constructor in unit tests, because the Rollback hack does not work with it
    /// </summary>
    /// <param name="activityAnalysis"></param>
    protected PendingModificationAnalysis (ActivityAnalysis activityAnalysis)
    {
      m_modificationTimeout = AnalysisConfigHelper.ModificationTimeout;
      m_modificationStepTimeout = AnalysisConfigHelper.ModificationStepTimeout;
      m_activityAnalysis = activityAnalysis;

      GetExtensions ();
    }

    /// <summary>
    /// Constructor
    /// 
    /// Parameter to select if the analysis must be run in a specific thread so that a timeout may be tracked,
    /// or in the current thread
    /// 
    /// To make some tests and be able to use the Rollback functionality,
    /// runInThread = false must be used
    /// </summary>
    /// <param name="runInThread"></param>
    protected PendingModificationAnalysis (bool runInThread)
    {
      m_threadExecution = runInThread;
      m_modificationTimeout = AnalysisConfigHelper.ModificationTimeout;
      m_modificationStepTimeout = AnalysisConfigHelper.ModificationStepTimeout;

      GetExtensions ();
    }

    /// <summary>
    /// Constructor
    /// 
    /// Parameter to select if the analysis must be run in a specific thread so that a timeout may be tracked,
    /// or in the current thread
    /// 
    /// To make some tests and be able to use the Rollback functionality,
    /// runInThread = false must be used
    /// </summary>
    /// <param name="runInThread"></param>
    /// <param name="activityAnalysis"></param>
    protected PendingModificationAnalysis (bool runInThread, ActivityAnalysis activityAnalysis)
    {
      m_threadExecution = runInThread;
      m_modificationTimeout = AnalysisConfigHelper.ModificationTimeout;
      m_modificationStepTimeout = AnalysisConfigHelper.ModificationStepTimeout;
      m_activityAnalysis = activityAnalysis;

      GetExtensions ();
    }

    void GetExtensions ()
    {
      m_extensions = Lemoine.Extensions.ExtensionManager
        .GetExtensions<Lemoine.Extensions.Analysis.IPendingModificationAnalysisExtension> (checkedThread: this)
        .ToList (); // ToList is mandatory else the result of the Linq command is not cached
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// 
    /// </summary>
    public override void SetActive ()
    {
      if (null != m_checkedParent) {
        m_checkedParent.SetActive ();
      }
      else {
        base.SetActive ();
      }
    }

    /// <summary>
    /// Run the analysis and loop
    /// </summary>
    protected override void Run (CancellationToken cancellationToken)
    {
      while (!m_exitRequested && !cancellationToken.IsCancellationRequested) {
        try {
          var maxTime = Lemoine.Info.ConfigSet
            .LoadAndGet (MAX_TIME_PENDING_MODIFICATIONS_RUN_KEY, MAX_TIME_PENDING_MODIFICATIONS_RUN_DEFAULT);
          var maxAnalysisDateTime = DateTime.UtcNow.Add (maxTime);
          RunOnce (cancellationToken, maxAnalysisDateTime, 0, 0);

          // - Sleep if needed
          TimeSpan every = AnalysisConfigHelper.Every;
          cancellationToken.WaitHandle.WaitOne (every);
          SetActive ();
        }
        catch (OperationCanceledException ex) {
          GetLogger ().Error ("Run: OperationCanceledException", ex);
          throw;
        }
        catch (Lemoine.Threading.AbortException ex) {
          GetLogger ().Error ("Run: AbortException", ex);
          throw;
        }
        catch (Exception ex) {
          SetActive ();
          if (ExceptionTest.IsStale (ex)) {
            GetLogger ().Info ("Run: StaleObjectStateException => try again", ex);
          }
          else if (ExceptionTest.IsTransactionSerializationFailure (ex, GetLogger ())) {
            GetLogger ().Info ("Run: serialization failure => try again", ex);
          }
          else if (ExceptionTest.IsTimeoutFailure (ex, GetLogger ())) {
            GetLogger ().Warn ("Run: timeout failure => try again", ex);
          }
          else if (ExceptionTest.IsTemporaryWithDelay (ex, log)) {
            var temporaryWithDelayExceptionSleepTime = Lemoine.Info.ConfigSet
              .LoadAndGet (TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_KEY, TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_DEFAULT);
            log.Warn ($"Run: temporary with delay exception inner {ex.InnerException}, try again after sleeping {temporaryWithDelayExceptionSleepTime}", ex);
            this.Sleep (temporaryWithDelayExceptionSleepTime, cancellationToken);
          }
          else if (ExceptionTest.IsTemporary (ex, GetLogger ())) {
            GetLogger ().Warn ("Run: temporary failure => try again", ex);
          }
          else if (ExceptionTest.RequiresExit (ex, GetLogger ())) {
            GetLogger ().Error ($"Run: exception with inner {ex.InnerException} requires to exit, give up", ex);
            m_exitRequested = true;
            throw new Exception ("Exception requires to exit", ex);
          }
          else {
            GetLogger ().Error ("Run: exception but try again", ex);
          }
        }
      }
    }

    /// <summary>
    /// Run the analysis once
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxAnalysisDateTime"></param>
    /// <param name="minPastPriority"></param>
    /// <param name="minPresentPriority"></param>
    /// <returns>Completed</returns>
    public virtual bool RunOnce (CancellationToken cancellationToken, DateTime maxAnalysisDateTime, int minPastPriority, int minPresentPriority)
    {
      try {
        SetActive ();
        var result = MakeAnalysis (cancellationToken, maxAnalysisDateTime, minPastPriority, minPresentPriority);
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ($"RunOnce: completed result={result}");
        }
        return result;
      }
      catch (OperationCanceledException ex) {
        GetLogger ().Error ("RunOnce: OperationCanceledException", ex);
        throw;
      }
      catch (Lemoine.Threading.AbortException ex) {
        GetLogger ().Error ("RunOnce: AbortException", ex);
        throw;
      }
      catch (Exception ex) {
        SetActive ();
        if (ExceptionTest.IsStale (ex, GetLogger ())) {
          GetLogger ().Info ("RunOnce: StaleObjectStateException", ex);
          throw;
        }
        else if (ExceptionTest.IsTransactionSerializationFailure (ex, GetLogger ())) {
          GetLogger ().Info ("RunOnce: serialization failure", ex);
          throw;
        }
        else if (ExceptionTest.IsTimeoutFailure (ex, GetLogger ())) {
          GetLogger ().Warn ("RunOnce: timeout failure", ex);
          throw;
        }
        else if (ExceptionTest.IsTemporaryWithDelay (ex, GetLogger ())) {
          GetLogger ().Warn ("RunOnce: temporary with delay failure", ex);
          throw;
        }
        else if (ExceptionTest.IsTemporary (ex, GetLogger ())) {
          GetLogger ().Warn ("RunOnce: temporary failure", ex);
          throw;
        }
        else if (ExceptionTest.RequiresExit (ex, GetLogger ())) {
          GetLogger ().Error ($"RunOnce: Exception inner {ex.InnerException} requires to exit", ex);
          throw;
        }
        else {
          GetLogger ().Exception (ex, "RunOnce");
          throw;
        }
      }
    }

    /// <summary>
    /// Logger
    /// </summary>
    /// <returns></returns>
    public override ILog GetLogger ()
    {
      return log;
    }

    /// <summary>
    /// Create locked tables
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    protected abstract IEnumerable<ILockTableToPartition> CreateLockedTables (IDAOSession session);

    /// <summary>
    /// Get the batch size for the past modifications
    /// </summary>
    /// <returns></returns>
    protected abstract int GetPastModificationsBatchSize ();

    /// <summary>
    /// Get the maximum age for a past modification
    /// </summary>
    /// <returns></returns>
    protected abstract TimeSpan GetPastModificationsAge ();

    /// <summary>
    /// Get the step timeout margin
    /// </summary>
    /// <returns></returns>
    protected abstract TimeSpan GetStepTimeoutMargin ();

    /// <summary>
    /// Reload a modification
    /// </summary>
    /// <param name="modification"></param>
    /// <param name="lockTables">Lock some tables. Set it to true if no transaction is opened</param>
    /// <returns></returns>
    protected abstract I ReloadModification (I modification, bool lockTables = false);

    /// <summary>
    /// Reset the internal progress values:
    /// <item>LastModificationId</item>
    /// <item>LastPriority</item>
    /// </summary>
    public virtual void ResetProgress ()
    {
      m_progress = PendingModificationsProgress.CreateDefault ();
    }

    /// <summary>
    /// Update the progress
    /// </summary>
    /// <param name="modification"></param>
    /// <param name="minPriority"></param>
    protected virtual PendingModificationsProgress UpdateProgress (IModification modification, int minPriority)
    {
      m_progress = new PendingModificationsProgress (modification, minPriority);
      return m_progress;
    }

    protected virtual PendingModificationsProgress GetProgress (int minPriority)
    {
      if (minPriority < m_progress.MinPriority) {
        // The cache can't be used and a new progress cache must be returned
        return PendingModificationsProgress.CreateDefault ();
      }
      else {
        return m_progress; // ok
      }
    }

    /// <summary>
    /// process all pending modifications at at given DateTime
    /// proceeds batch per batch
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxAnalysisDateTime"></param>
    /// <param name="minPastPriority"></param>
    /// <param name="minPresentPriority">greater or equal minPastPriority</param>
    /// <returns>Completed</returns>
    public virtual bool MakeAnalysis (CancellationToken cancellationToken, DateTime maxAnalysisDateTime, int minPastPriority, int minPresentPriority)
    {
      try {
        int correctedMinPastPriority = minPastPriority;
        if (minPresentPriority < minPastPriority) {
          GetLogger ().Fatal ($"MakeAnalysis: wrong min priorities, past {minPastPriority} greater than present {minPresentPriority} => correct the past priority");
          correctedMinPastPriority = minPresentPriority;
        }

        // Get the maximum modification ID
        long? maxModificationId = GetMaxModificationId ();
        if (object.Equals (maxModificationId, m_maxModificationIdAllCompleted)) { // maxModificationId has already been processed
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ($"MakeAnalysis: Max ModificationId {maxModificationId} has already been flagged as completed (no change) => nothing to do (except cleaning the modifications)");
          }
          return true;
        }

        m_fillAnalysisStatusCompleted = false;

        // - Loop on all the pending modifications and process them
        ResetProgress ();
        bool pendingModifications = false;

        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ($"MakeAnalysis: about to process past modifications");
        }

        var completed = ProcessPastPendingModifications (cancellationToken, ref pendingModifications, maxAnalysisDateTime, correctedMinPastPriority);

        if (cancellationToken.IsCancellationRequested) {
          GetLogger ().Error ("MakeAnalysis: cancellation is requested after ProcessPastPendingModifications => return false");
          return false;
        }

        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ($"MakeAnalysis: about to process present modifications");
        }

        completed &= ProcessPresentPendingModifications (cancellationToken, ref pendingModifications, maxAnalysisDateTime, minPresentPriority);

        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ($"MakeAnalysis: present modifications completed");
        }

        if (!pendingModifications && completed && (0 == correctedMinPastPriority) && (0 == minPresentPriority)) {
          // There was no pending modifications with maxModificationId
          // => update m_maxModificationId
          // This is only valid the second time the modifications are processed
          // to be sure there is no modification any progress any more
          m_maxModificationIdAllCompleted = maxModificationId;
        }

        return completed;
      }
      catch (OperationCanceledException ex) {
        GetLogger ().Error ("MakeAnalysis: OperationCanceledException detected => return false", ex);
        return false;
      }
      catch (Exception ex) {
        GetLogger ().Exception (ex, "MakeAnalysis");
        throw;
      }
    }

    /// <summary>
    /// Process the past pending modifications by batch
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="pendingModifications"></param>
    /// <param name="maxAnalysisDateTime"></param>
    /// <param name="minPriority"></param>
    /// <returns>completed</returns>
    bool ProcessPastPendingModifications (CancellationToken cancellationToken, ref bool pendingModifications, DateTime maxAnalysisDateTime, int minPriority = 0)
    {
      var before = DateTime.UtcNow.Subtract (GetPastModificationsAge ());

      var progress = GetProgress (minPriority);

      int pastModificationsBatchSize = GetPastModificationsBatchSize ();
      bool allRetrieved = false;

      try {
        while (!allRetrieved && !cancellationToken.IsCancellationRequested) {
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ($"ProcessPastPendingModifications: about to get the first {pastModificationsBatchSize} past modifications with {progress}");
          }
          SetActive ();
          CheckNewAnalysisStatus (cancellationToken);
          SetActive ();
          IList<I> pastModifications =
            GetPastPendingModifications (progress.LastModificationId, progress.LastPriority,
                                         before,
                                         pastModificationsBatchSize,
                                         minPriority)
            .ToList ();
          if (!pastModifications.Any ()) {
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().DebugFormat ("ProcessPastPendingModifications: " +
                "no more past modifications found");
            }
            return true;
          }
          allRetrieved = pastModifications.Count < pastModificationsBatchSize;
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ("ProcessPastPendingModifications: " +
              $"{pastModifications.Count} past modifications retrieved, allRetrieved={allRetrieved}");
          }
          for (int i = 0; i < pastModifications.Count;) {
            var pastModification = (T)pastModifications[i];
            SetActive ();
            pastModification.Caller = this;
            progress = UpdateProgress (pastModification, minPriority);
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"ProcessPastPendingModifications: Begin on past modification id {pastModification.Id}");
              GetLogger ().Debug ($"ProcessPastPendingModifications: new progress is {progress}");
            }
            if (MakeAnalysis (cancellationToken, pastModification)) {
              if (GetLogger ().IsDebugEnabled) {
                GetLogger ().Debug ($"ProcessPastPendingModifications: End on past modification id {pastModification.Id}");
              }
              if (pastModification.AnalysisStatus.IsInProgress ()
                  || pastModification.AnalysisStatus.Equals (AnalysisStatus.Pending)) {
                pendingModifications = true;
              }
              if (pastModification.AnalysisStatus.Equals (AnalysisStatus.Delete)
                || pastModification.AnalysisStatus.Equals (AnalysisStatus.DonePurge)) {
                SetCleanRequired ();
              }
              ++i;
            }
            else {
              if (GetLogger ().IsInfoEnabled) {
                GetLogger ().InfoFormat ("ProcessPastPendingModifications: " +
                  "past modification id={0} is retried or continued (because in progress)",
                  pastModification.Id);
              }
              // Reload pastModification in case the status was updated
              pastModifications[i] = ReloadModification (pastModifications[i], lockTables: true);
            }

            if (cancellationToken.IsCancellationRequested) {
              if (GetLogger ().IsErrorEnabled) {
                GetLogger ().Error ($"ProcessPastPendingModifications: cancel requested");
              }
              return false;
            }

            if (maxAnalysisDateTime < DateTime.UtcNow) {
              if (GetLogger ().IsInfoEnabled) {
                GetLogger ().Info ($"ProcessPastPendingModifications: {maxAnalysisDateTime} reached => return false at once");
              }
              return false;
            }

          } // Loop on pastModifications
        } // Main loop

        if (allRetrieved) {
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ($"ProcessPastPendingModifications: last past modifications already retrieved, return true");
          }
          return true;
        }
        else {
          if (GetLogger ().IsErrorEnabled) {
            GetLogger ().Error ($"ProcessPastPendingModifications: cancel requested");
          }
          return false;
        }
      }
      catch (OperationCanceledException ex) {
        GetLogger ().Error ("ProcessPastPendingModifications: OperationCanceledException detected", ex);
        return false;
      }
      catch (Exception ex) {
        GetLogger ().Exception (ex, "ProcessPastPendingModifications");
        throw;
      }
    }

    /// <summary>
    /// Process the past present modifications one by one
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="pendingModifications"></param>
    /// <param name="maxAnalysisDateTime"></param>
    /// <param name="minPriority"></param>
    /// <returns>completed</returns>
    bool ProcessPresentPendingModifications (CancellationToken cancellationToken, ref bool pendingModifications, DateTime maxAnalysisDateTime, int minPriority = 0)
    {
      var progress = GetProgress (minPriority);

      try {
        while (!cancellationToken.IsCancellationRequested) {
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ($"ProcessPresentPendingModifications: about to get the present pending modifications with {progress}");
          }
          SetActive ();
          CheckNewAnalysisStatus (cancellationToken);
          SetActive ();

          T firstPendingModification =
            (T)GetFirstPendingModification (progress.LastModificationId,
                                            progress.LastPriority,
                                            minPriority);

          if (firstPendingModification == default (T)) { // Job done
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ("ProcessPresentPendingModifications: no pending modifications, return true (completed)");
            }
            return true;
          }

          firstPendingModification.Caller = this;
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().DebugFormat ("ProcessPresentPendingModifications: " +
              "Begin on present modification id {0}",
              firstPendingModification.Id);
          }
          if (MakeAnalysis (cancellationToken, firstPendingModification)) {
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().DebugFormat ("ProcessPresentPendingModifications: " +
                "End on present modification id {0}",
                firstPendingModification.Id);
            }
            progress = UpdateProgress (firstPendingModification, minPriority);
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"ProcessPresentPendingModifications: new progress is {progress}");
            }
            if (firstPendingModification.AnalysisStatus.IsInProgress ()
                || firstPendingModification.AnalysisStatus.Equals (AnalysisStatus.Pending)) {
              pendingModifications = true;
            }
            if (firstPendingModification.AnalysisStatus.Equals (AnalysisStatus.Delete)
              || firstPendingModification.AnalysisStatus.Equals (AnalysisStatus.DonePurge)) {
              SetCleanRequired ();
            }
          }
          else {
            if (GetLogger ().IsInfoEnabled) {
              GetLogger ().InfoFormat ("ProcessPresentPendingModifications: " +
                "Present Modification id {0} must be retried or continued",
                firstPendingModification.Id);
            }
          }

          if (cancellationToken.IsCancellationRequested) {
            if (GetLogger ().IsInfoEnabled) {
              GetLogger ().Info ($"ProcessPresentPendingModifications: cancellation requested => return false at once");
            }
            return false;
          }

          if (maxAnalysisDateTime < DateTime.UtcNow) {
            if (GetLogger ().IsInfoEnabled) {
              GetLogger ().Info ($"ProcessPresentPendingModifications: {maxAnalysisDateTime} reached => return false at once");
            }
            return false;
          }
        } // Loop present modifications


        GetLogger ().Error ($"ProcessPresentPendingModifications: cancellation requested => return false");
        return false;
      }
      catch (OperationCanceledException ex) {
        GetLogger ().Error ("ProcessPresentPendingModifications: operation canceled exception detected => return false", ex);
        return false;
      }
      catch (Exception ex) {
        GetLogger ().Exception (ex, "ProcessPresentPendingModifications");
        throw;
      }
    }

    /// <summary>
    /// Get the past pending modifications
    /// </summary>
    /// <returns></returns>
    /// <param name="lastModificationId"></param>
    /// <param name="lastPriority"></param>
    /// <param name="before"></param>
    /// <param name="maxResults"></param>
    /// <param name="minPriority"></param>
    /// <returns></returns>
    protected abstract IEnumerable<I> GetPastPendingModifications (long lastModificationId,
                                                                   int lastPriority,
                                                                   DateTime before,
                                                                   int maxResults,
                                                                   int minPriority = 0);

    /// <summary>
    /// Get the first pending modification
    /// </summary>
    /// <param name="lastModificationId"></param>
    /// <param name="lastPriority"></param>
    /// <param name="minPriority"></param>
    /// <returns></returns>
    protected abstract I GetFirstPendingModification (long lastModificationId,
                                                      int lastPriority,
                                                      int minPriority = 0);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsCleanRequired ()
    {
      return m_cleanRequired && AnalysisConfigHelper.CleanDeletedModifications;
    }

    protected void SetCleanRequired ()
    {
      m_cleanRequired = true;
    }

    /// <summary>
    /// Clean the modifications that were flagged to be deleted
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxAnalysisDateTime"></param>
    public virtual void CleanFlaggedModifications (CancellationToken cancellationToken, DateTime maxAnalysisDateTime)
    {
      if (!IsCleanRequired ()) {
        log.Warn ($"CleanFlaggedModifications: clean is not required, but do it anyway");
      }

      SetActive ();
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = daoSession.BeginTransaction ("Analysis.PendingModification.Delete",
                                                                          TransactionLevel.ReadCommitted)) {
          transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data
          DeleteFlaggedModifications (maxAnalysisDateTime);
          transaction.Commit ();
        }
        SetActive ();
        using (IDAOTransaction transaction = daoSession.BeginTransaction ("Analysis.PendingModification.DeleteDonePurge",
                                                                          TransactionLevel.ReadCommitted)) {
          transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data
          DeleteDonePurgeModifications (maxAnalysisDateTime);
          transaction.Commit ();
        }
      }
      SetActive ();

      m_cleanRequired = false;
    }

    /// <summary>
    /// Delete all the modifications that are flagged to be deleted
    /// 
    /// This must be run inside a transaction
    /// </summary>
    /// <param name="maxAnalysisDateTime"></param>
    /// <returns>Completed (else interrupted by maxAnalysisDateTime)</returns>
    protected abstract bool DeleteFlaggedModifications (DateTime maxAnalysisDateTime);

    /// <summary>
    /// Delete all the modifications that are flagged DonePurge
    /// 
    /// This must be run inside a transaction
    /// </summary>
    /// <param name="maxAnalysisDateTime"></param>
    /// <returns>Completed (else interrupted by maxAnalysisDateTime)</returns>
    protected abstract bool DeleteDonePurgeModifications (DateTime maxAnalysisDateTime);

    /// <summary>
    /// Make the analysis of the specified modification
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="modification"></param>
    /// <returns>If true, completed, else retry it</returns>
    protected virtual bool MakeAnalysis (CancellationToken cancellationToken, I modification)
    {
      var modificationAnalysis = new ModificationAnalysis (modification, m_activityAnalysis);
      return MakeAnalysis (cancellationToken, modification, modificationAnalysis);
    }

    /// <summary>
    /// Get the performance tracker key for a specific modification
    /// </summary>
    /// <returns></returns>
    protected abstract string GetPerfTrackerKey (I modification);

    /// <summary>
    /// Make the analysis of the specified modification with the extension management
    /// </summary>
    /// <param name="modification"></param>
    /// <param name="modificationAnalysis"></param>
    /// <returns>If true, completed, else retry it</returns>
    protected virtual bool MakeAnalysis (CancellationToken cancellationToken, I modification, ModificationAnalysis modificationAnalysis)
    {
      GetLogger ().DebugFormat ("MakeAnalysis: modification id={0} type={1}",
        ((IDataWithId<long>)modification).Id,
        modification.GetType ());

      foreach (var extension in m_extensions) {
        cancellationToken.ThrowIfCancellationRequested ();
        extension.BeforeMakeAnalysis (modification);
      }
      bool completed;
      try {
        cancellationToken.ThrowIfCancellationRequested ();
        LogModification (modification, Lemoine.Core.Log.Level.Debug, "Start");
        using (var perfTracker = new PerfTracker (GetPerfTrackerKey (modification))) {
          completed = MakeAnalysisNoExtension (cancellationToken, modification, modificationAnalysis);
        }
        LogModification (modification, Lemoine.Core.Log.Level.Info, "End");
        cancellationToken.ThrowIfCancellationRequested ();
      }
      catch (OperationCanceledException ex) {
        GetLogger ().Error ("MakeAnalysis: OperationCanceledException", ex);
        throw;
      }
      catch (Lemoine.Threading.AbortException ex) {
        GetLogger ().Error ("MakeAnalysis: AbortException", ex);
        throw;
      }
      catch (Exception ex) {
        LogModification (modification, Lemoine.Core.Log.Level.Error, "Exception");
        try {
          foreach (var extension in m_extensions) {
            cancellationToken.ThrowIfCancellationRequested ();
            extension.MakeAnalysisException (modification, ex);
          }
        }
        catch (Exception ex1) {
          GetLogger ().Error ("MakeAnalysis: exception in MakeAnalysisException extension", ex1);
        }
        throw;
      }
      foreach (var extension in m_extensions) {
        cancellationToken.ThrowIfCancellationRequested ();
        extension.AfterMakeAnalysis (modification, completed);
      }
      return completed;
    }

    void LogModification (I modification, Lemoine.Core.Log.Level level, string message)
    {
      try {
        var perfTrackerKey = GetPerfTrackerKey (modification);
        var modificationLog = LogManager.GetLogger (perfTrackerKey + "." + ((IDataWithId<long>)modification).Id);
        modificationLog.Log (level, message);
      }
      catch (OperationCanceledException ex) {
        GetLogger ().Error ("LogModification: OperationCanceledException", ex);
        throw;
      }
      catch (Lemoine.Threading.AbortException ex) {
        GetLogger ().Error ("LogModification: AbortException", ex);
        throw;
      }
      catch (Exception ex) {
        GetLogger ().Error ("LogModification: exception in log exception", ex);
      }
    }

    /// <summary>
    /// Notify a modification with sub-modifications has just been flagged as completed
    /// </summary>
    /// <param name="modification"></param>
    protected virtual void NotifyAllSubModificationsCompleted (I modification)
    {
      GetLogger ().DebugFormat ("NotifyAllSubModificationsCompleted: modification id={0} type={1}",
        ((IDataWithId<long>)modification).Id,
        modification.GetType ());

      foreach (var extension in m_extensions) {
        extension.NotifyAllSubModificationsCompleted (modification);
      }
    }

    /// <summary>
    /// Make the analysis of the specified modification without the extension management
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="modification">not null</param>
    /// <param name="modificationAnalysis">not null</param>
    /// <returns>If true, completed, else retry it</returns>
    protected virtual bool MakeAnalysisNoExtension (CancellationToken cancellationToken, I modification, ModificationAnalysis modificationAnalysis)
    {
      if (null == modification) {
        GetLogger ().Fatal ("MakeAnalysisNoExtension: modification is null");
      }
      Debug.Assert (null != modification);
      Debug.Assert (null != modificationAnalysis);

      DateTime startDateTime = DateTime.UtcNow;

      if (modification.AnalysisStatus.Equals (AnalysisStatus.PendingSubModifications)) {
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().DebugFormat ("MakeAnalysisNoExtension: " +
            "modification with pending sub-modifications: " +
            "process them, and return true. Modification={0}",
            ((IDataWithId<long>)modification).Id);
        }
        ProcessSubModifications (cancellationToken, modification, modification.AnalysisStatus, modification.StatusPriority);
        return true;
      }
      else { // Not PendingSubModifications
        Debug.Assert ((modification.AnalysisStatus.Equals (AnalysisStatus.New))
                      || (modification.AnalysisStatus.Equals (AnalysisStatus.Pending))
                      || (modification.AnalysisStatus.Equals (AnalysisStatus.InProgress))
                      || (modification.AnalysisStatus.Equals (AnalysisStatus.StepTimeout))
                      || (modification.AnalysisStatus.Equals (AnalysisStatus.Timeout))
                      || (modification.AnalysisStatus.Equals (AnalysisStatus.DatabaseTimeout)));

        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().DebugFormat ("MakeAnalysisNoExtension: " +
            "about to process modification {0} status={1}",
            ((IDataWithId<long>)modification).Id, modification.AnalysisStatus);
        }

        if (!m_threadExecution) {
          modificationAnalysis.RunDirectly (cancellationToken);
        }
        else {
          modificationAnalysis.Start (cancellationToken);
          var thread = modificationAnalysis.Thread;
          TimeSpan stepTimeoutMargin = GetStepTimeoutMargin ();
          while (!thread.Join (TimeSpan.FromMilliseconds (100))) {
            SetActive ();
            if (modificationAnalysis.ExitRequested) {
              GetLogger ().Error ("MakeAnalysisNoExtension: exit requested by modification analysis");
              SetExitRequested ();
              throw new Lemoine.Threading.AbortException ("Exit was requested by the modification analysis.");
            }
            if (m_modificationTimeout < DateTime.UtcNow.Subtract (startDateTime)) {
              // Time is over: give up
              if (GetLogger ().IsErrorEnabled) {
                GetLogger ().Error ($"MakeAnalysisNoExtension: the modification {((IDataWithId<long>)modification).Id} analysis run took more than {m_modificationTimeout}, give up (abort the modification analysis thread)");
              }

              modificationAnalysis.FlagTimeoutInterruption ();
              bool cancelSuccessful;
              try {
                cancelSuccessful = modificationAnalysis.Cancel ();
                if (!cancelSuccessful) {
                  GetLogger ().Warn ("MakeAnalysisNoExtension: timeout in Cancel");
                }
              }
              catch (Exception ex) {
                GetLogger ().Error ("MakeAnalysisNoExtension: exception in Cancel", ex);
                cancelSuccessful = false;
              }
              if (!cancelSuccessful) {
                try {
                  if (!this.Abort (tryCancelFirst: false)) {
                    GetLogger ().Error ("MakeAnalysisNoExtension: abort failed after Cancel failed");
                  }
                }
                catch (PlatformNotSupportedException ex) {
                  GetLogger ().Info ("MakeAnalysisNoExtension: abort not supported on this platform", ex);
                }
                catch (Exception ex) {
                  GetLogger ().Error ("MakeAnalysisNoExtension: error when trying to abort the modification thread", ex);
                }
              }
              // Set the status of the modification to timeout
              using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
                var lockedTables = CreateLockedTables (session);
                using (IDAOTransaction transaction = session
                  .BeginTransaction ("Analysis.PendingModification.Timeout",
                                     TransactionLevel.ReadCommitted,
                                     lockedTables: lockedTables)) {
                  transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data

                  // Re-load again modification because modification may be already in used in the session
                  // that is in the thread that were aborted.
                  modification = ReloadModification (modification, lockTables: false); // Not the top transaction: lockTables: false
                  if (GetLogger ().IsErrorEnabled) {
                    if (modification.AnalysisStatus.Equals (AnalysisStatus.Timeout)) {
                      GetLogger ().ErrorFormat ("MakeAnalysisNoExtension: trying to set again modification id {0} in status Timeout", ((IDataWithId<long>)modification).Id);
                    }
                  }
                  if (!modification.AnalysisBegin.HasValue) {
                    modification.AnalysisBegin = startDateTime;
                  }
                  modification.MarkAsTimeout (startDateTime);
                  ModelDAOHelper.DAOFactory.ModificationDAO.MakePersistent (modification);
                  ModelDAOHelper.DAOFactory.AnalysisLogDAO.Add (modification,
                                                                LogLevel.ERROR,
                                                                "Analysis timeout");
                  transaction.Commit ();
                }
              }
              // thread abort occurred: this may leave connections pending.
              // to be on the safe side, kill the connection or exit the application
              var exitIfThreadAbort = Lemoine.Info.ConfigSet
                .LoadAndGet (EXIT_IF_THREAD_ABORT_KEY, EXIT_IF_THREAD_ABORT_DEFAULT);
              if (exitIfThreadAbort && !cancelSuccessful) {
                GetLogger ().Error ("MakeAnalysisNoExtension: cancel was not successful, exit not to leave open database connections");
                SetExitRequested ();
                throw new Lemoine.Threading.AbortException ("Exit was requested.");
              }
              else if (-1 < modificationAnalysis.AnalysisConnectionId) {
                try {
                  var killResult = ModelDAOHelper.DAOFactory.KillActiveConnection (modificationAnalysis.AnalysisConnectionId);
                  if (GetLogger ().IsInfoEnabled) {
                    GetLogger ().InfoFormat ("MakeAnalysisNoExtension: kill active connection id {0} - result {1}", modificationAnalysis.AnalysisConnectionId, killResult);
                  }
                  return false;
                }
                catch (Exception ex) {
                  GetLogger ().Error ("MakeAnalysisNoExtension: KillActiveConnection returned an exception => exit", ex);
                  SetExitRequested ();
                  throw new Lemoine.Threading.AbortException ("Exit is requested.");
                }
              }
              else {
                GetLogger ().Fatal ("MakeAnalysisNoExtension: " +
                  "no analysis connection Id, exit not to leave open database connections, safer");
                this.SetExitRequested ();
                throw new Lemoine.Threading.AbortException ("Exit is requested.");
              }
            } // End if timeout
            // Note: do not check if the stepTimeout was reached, because the rules to determine
            //       if the step process is really active are quite complex
          } // End while
        }
      } // Not SubModifications => analysis
      SetActive ();

      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().Debug ($"MakeAnalysisNoExtension: Analysis of modification {((IDataWithId<long>)modification).Id} was completed in about {DateTime.UtcNow.Subtract (startDateTime)}");
      }
      if (modificationAnalysis.ExitRequested) {
        GetLogger ().Fatal ("MakeAnalysis: ExitRequested in modificationAnalysis");
        SetExitRequested ();
        throw new Lemoine.Threading.AbortException ("Exit is requested.");
      }
      else if (null != modificationAnalysis.ExceptionStatus) {
        // The Run method of the thread ended with an exception
        GetLogger ().Exception (modificationAnalysis.ExceptionStatus, "MakeAnalysisNoExtension: modificationAnalysis ended with exception");
        throw modificationAnalysis.ExceptionStatus;
      }
      else if (modificationAnalysis.Retry) {
        if (GetLogger ().IsInfoEnabled) {
          GetLogger ().Info ($"MakeAnalysisNoExtension: about to retry/continue the modification {((IDataWithId<long>)modification).Id}");
        }
        return false;
      }
      else if (modificationAnalysis.AnalysisStatus.Equals (AnalysisStatus.PendingSubModifications)) { // Sub-modifications to process
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ($"MakeAnalysisNoExtension: process now the sub-modifications of {((IDataWithId<long>)modification).Id}");
        }
        ProcessSubModifications (cancellationToken, modification, modificationAnalysis.AnalysisStatus, modificationAnalysis.StatusPriority);
        return true;
      }
      else {
        return true;
      }
    }

    /// <summary>
    /// Process the sub-modifications of the specified modification
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="modification"></param>
    /// <param name="analysisStatus">Analysis status of the modification (because ModificationAnalysis may be used)</param>
    /// <param name="statusPriority"></param>
    protected abstract void ProcessSubModifications (CancellationToken cancellationToken, I modification, AnalysisStatus analysisStatus, int statusPriority);

    /// <summary>
    /// Get the maximum modificationId
    /// </summary>
    /// <returns></returns>
    protected abstract long? GetMaxModificationId ();

    /// <summary>
    /// Get the maximum modificationId asynchronously
    /// </summary>
    /// <returns></returns>
    protected abstract Task<long?> GetMaxModificationIdAsync ();

    void CheckNewAnalysisStatus (CancellationToken cancellationToken)
    {
      if (m_fillAnalysisStatusCompleted) {
        return;
      }

      long limit;
      if (-1 != m_limitNewAnalysisStatus) {
        limit = m_limitNewAnalysisStatus;
      }
      else {
        limit = GetLimitNewAnalysisStatus ();
      }

      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().Debug ($"CheckNewAnalysisStatus: FillAnalysisStatus with limit {limit}");
      }
      m_fillAnalysisStatusCompleted = FillAnalysisStatus (cancellationToken, limit);
    }

    /// <summary>
    /// Fill in the analysis status table for the 'New' modifications.
    /// 
    /// Not to be run in a parent transaction
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="limit">Maximum number of items to retrieve</param>
    /// <returns>Completed (the limit was not reached)</returns>
    public abstract bool FillAnalysisStatus (CancellationToken cancellationToken, long limit);

    /// <summary>
    /// Get the default limit for the creation of the new analysis status
    /// </summary>
    /// <returns></returns>
    protected abstract long GetLimitNewAnalysisStatus ();
    #endregion // Methods

    #region IDisposable implementation
    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose (bool disposing)
    {
      if (m_disposed) {
        return;
      }

      if (disposing) {
        // Dispose any managed object here
      }

      m_disposed = true;

      base.Dispose (disposing);
    }

    #endregion // IDisposable implementation
  }
}
