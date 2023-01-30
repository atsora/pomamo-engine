// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Lemoine.GDBPersistentClasses;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Threading;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
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
  public sealed class PendingGlobalModificationAnalysis : PendingModificationAnalysis<IGlobalModification, GlobalModification>, IThreadClass, IChecked
  {
    static readonly string PAST_MODIFICATIONS_BATCH_SIZE_KEY = "Analysis.GlobalModification.Past.BatchSize";
    static readonly int PAST_MODIFICATIONS_BATCH_SIZE_DEFAULT = 8;

    static readonly string PAST_MODIFICATIONS_AGE_KEY = "Analysis.GlobalModification.Past.Age";
    static readonly TimeSpan PAST_MODIFICATIONS_AGE_DEFAULT = TimeSpan.FromMinutes (20);

    static readonly string STEP_TIMEOUT_MARGIN_KEY = "Analysis.GlobalModification.StepTimeoutMargin";
    static readonly TimeSpan STEP_TIMEOUT_MARGIN_DEFAULT = TimeSpan.FromSeconds (2); // Give 2 more seconds in the thread check

    #region Members
    #endregion // Members

    readonly ILog log = LogManager.GetLogger (typeof (PendingGlobalModificationAnalysis).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// 
    /// Run by default the analysis in a thread
    /// 
    /// Do not use this constructor in unit tests, because the Rollback hack does not work with it
    /// </summary>
    public PendingGlobalModificationAnalysis ()
      : base ()
    {
      foreach (var extension in m_extensions) {
        extension.Initialize (null);
      }
    }

    /// <summary>
    /// Constructor
    /// 
    /// Run by default the analysis in a thread
    /// 
    /// Do not use this constructor in unit tests, because the Rollback hack does not work with it
    /// </summary>
    /// <param name="activityAnalysis"></param>
    public PendingGlobalModificationAnalysis (ActivityAnalysis activityAnalysis)
      : base (activityAnalysis)
    {
      foreach (var extension in m_extensions) {
        extension.Initialize (null);
      }
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
    public PendingGlobalModificationAnalysis (bool runInThread)
      : base (runInThread)
    {
      foreach (var extension in m_extensions) {
        extension.Initialize (null);
      }
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
    public PendingGlobalModificationAnalysis (bool runInThread, ActivityAnalysis activityAnalysis)
      : base (runInThread, activityAnalysis)
    {
      foreach (var extension in m_extensions) {
        extension.Initialize (null);
      }
    }
    #endregion // Constructors

    #region Methods
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
    protected override IEnumerable<ILockTableToPartition> CreateLockedTables (IDAOSession session)
    {
      return null;
    }

    /// <summary>
    /// Get the batch size for the past modifications
    /// </summary>
    /// <returns></returns>
    protected override int GetPastModificationsBatchSize ()
    {
      return ConfigSet.LoadAndGet<int> (PAST_MODIFICATIONS_BATCH_SIZE_KEY,
                                        PAST_MODIFICATIONS_BATCH_SIZE_DEFAULT);
    }

    /// <summary>
    /// Get the maximum age for a past modification
    /// </summary>
    /// <returns></returns>
    protected override TimeSpan GetPastModificationsAge ()
    {
      return ConfigSet.LoadAndGet<TimeSpan> (PAST_MODIFICATIONS_AGE_KEY,
                                             PAST_MODIFICATIONS_AGE_DEFAULT);
    }

    /// <summary>
    /// Get the step timeout margin
    /// </summary>
    /// <returns></returns>
    protected override TimeSpan GetStepTimeoutMargin ()
    {
      return ConfigSet.LoadAndGet<TimeSpan> (STEP_TIMEOUT_MARGIN_KEY,
                                             STEP_TIMEOUT_MARGIN_DEFAULT);
    }

    /// <summary>
    /// Reload a modification (after a transaction failure for example)
    /// </summary>
    /// <param name="modification"></param>
    /// <param name="lockTables"></param>
    /// <returns></returns>
    protected override IGlobalModification ReloadModification (IGlobalModification modification, bool lockTables = false)
    {
      IGlobalModification result;

      // Reload pastModification in case the status was updated
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ("Analysis.PendingGlobalModification.ReloadPastModification",
                                                                     TransactionLevel.ReadCommitted))
    // Not read-only because a row in modificationstatus may be created
    {
        transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data
        result = ModelDAOHelper.DAOFactory.GlobalModificationDAO
          .FindById (((Lemoine.Collections.IDataWithId<long>)modification).Id);
        transaction.Commit ();
      }

      return result;
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
    protected override IEnumerable<IGlobalModification> GetPastPendingModifications (long lastModificationId,
                                                                                     int lastPriority,
                                                                                     DateTime before,
                                                                                     int maxResults,
                                                                                     int minPriority = 0)
    {
      IEnumerable<IGlobalModification> pastModifications;

      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = daoSession.BeginTransaction ("Analysis.PendingGlobalModification.GetPast",
                                                                          TransactionLevel.ReadCommitted)) { // May be the read-only option is possible, but I am not sure
          // because I think the analysisstatus is initialized here
          transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data
          pastModifications =
            ModelDAOHelper.DAOFactory.GlobalModificationDAO
            .GetPastPendingModifications (lastModificationId, lastPriority,
                                          before, maxResults, minPriority);
          transaction.Commit ();
        }
      }

      return pastModifications;
    }

    /// <summary>
    /// Get the first pending modification
    /// </summary>
    /// <param name="lastModificationId"></param>
    /// <param name="lastPriority"></param>
    /// <param name="minPriority"></param>
    /// <returns></returns>
    protected override IGlobalModification GetFirstPendingModification (long lastModificationId,
                                                                        int lastPriority,
                                                                        int minPriority = 0)
    {
      IGlobalModification result;

      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        // Note: a transaction with a Commit is created here
        //       else the new row in modificationstatus is not saved in the database
        using (IDAOTransaction transaction = daoSession.BeginTransaction ("Analysis.PendingGlobalModification.GetFirst")) { // May be the read-only option is possible, but I am not sure
          // because I think the analysisstatus is initialized here
          transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data
          result = ModelDAOHelper.DAOFactory.GlobalModificationDAO
            .GetFirstPendingModification (lastModificationId,
                                          lastPriority,
                                          minPriority);
          transaction.Commit ();
        }
      }

      return result;
    }

    /// <summary>
    /// <see cref="PendingModificationAnalysis{I, T}"/>
    /// </summary>
    /// <param name="maxAnalysisDateTime"></param>
    /// <returns></returns>
    protected override bool DeleteFlaggedModifications (DateTime maxAnalysisDateTime)
    {
      ModelDAOHelper.DAOFactory.GlobalModificationDAO.Delete (AnalysisStatus.Delete);
      return true;
    }

    /// <summary>
    /// <see cref="PendingModificationAnalysis{I, T}"/>
    /// </summary>
    /// <param name="maxAnalysisDateTime"></param>
    /// <returns></returns>
    protected override bool DeleteDonePurgeModifications (DateTime maxAnalysisDateTime)
    {
      DateTime maxCompletionDateTime = DateTime.UtcNow.Subtract (AnalysisConfigHelper.AutoModificationPurgeDelay);
      ModelDAOHelper.DAOFactory.GlobalModificationDAO.Delete (AnalysisStatus.DonePurge, maxCompletionDateTime);
      return true;
    }

    /// <summary>
    /// Get the performance tracker key for a specific modification
    /// </summary>
    /// <returns></returns>
    protected override string GetPerfTrackerKey (IGlobalModification modification)
    {
      return "Analysis.Modification.Global";
    }

    /// <summary>
    /// Process the sub-modifications of the specified modification
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="modification"></param>
    /// <param name="analysisStatus"></param>
    /// <param name="statusPriority"></param>
    protected override void ProcessSubModifications (CancellationToken cancellationToken, IGlobalModification modification, AnalysisStatus analysisStatus, int statusPriority)
    {
      // - Get the sub-globalmodifications
      IEnumerable<IGlobalModification> subGlobalModifications;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("Analysis.PendingGlobalModification.GetSubModifications",
                                                                       TransactionLevel.ReadCommitted)) {
          transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data

          // Get the sub-modifications
          subGlobalModifications = ModelDAOHelper.DAOFactory.GlobalModificationDAO
            .GetNotCompletedSubGlobalModifications (modification);
          transaction.Commit ();
        }
      }

      // - Analyze them
      bool notCompletedGlobalSubModifications = false;
      foreach (IGlobalModification subGlobalModification in subGlobalModifications) {
        log.DebugFormat ("MakeAnalysis: " +
                         "about to analyze sub-modification {0}",
                         subGlobalModification);
        var subModificationAnalysis = new ModificationAnalysis (subGlobalModification, this.ActivityAnalysis);
        while (!MakeAnalysis (cancellationToken, subGlobalModification, subModificationAnalysis)) {
          log.InfoFormat ("MakeAnalysis: " +
                          "retry the analysis of sub-modification {0}",
                          subGlobalModification);
          SetActive ();
        }
        // disable once ConvertIfToOrExpression
        if (subModificationAnalysis.AnalysisStatus.IsNotCompleted ()) {
          notCompletedGlobalSubModifications = true;
        }
        log.DebugFormat ("MakeAnalysis: " +
                         "analysis of sub-modification {0} is completed",
                         subGlobalModification);
        SetActive ();
      }

      // - Update the status of the main modification if there is no pending sub-modification
      if (notCompletedGlobalSubModifications) {
        log.DebugFormat ("MakeAnalysis: " +
                         "one sub-modification has a pending status");
      }
      else { // !pendingGlobalSubModifications
        if (!CheckPendingSubModifications (modification)) {
          var allSubModificationsCompleted = false;
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            var lockedTables = CreateLockedTables (session);
            using (IDAOTransaction transaction = session
              .BeginTransaction ("Analysis.PendingModification.SubModificationDone",
                                 TransactionLevel.ReadCommitted,
                                 lockedTables: lockedTables)) {
              transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data

              modification = ReloadModification (modification);

              log.DebugFormat ("MakeAnalysis: " +
                               "the modification {0} with sub-modifications is now completed",
                               modification);
              if (modification.AnalysisStatus.Equals (AnalysisStatus.PendingSubModifications)) {
                modification.MarkAllSubModificationsCompleted ();
                allSubModificationsCompleted = true;
              }
              else {
                log.InfoFormat ("ProcessSubModifications: done but keep the status {0} since it is not PendingSubModifications", modification.AnalysisStatus);
              }
              ModelDAOHelper.DAOFactory.ModificationDAO.MakePersistent (modification);

              transaction.Commit ();
            } // transaction
          } // session
          if (allSubModificationsCompleted) {
            NotifyAllSubModificationsCompleted (modification);
          }
        }
      }
    }

    bool CheckPendingSubModifications (IGlobalModification modification)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        if (modification.AnalysisSubMachineModifications) {
          IList<IMachine> machines;
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.CheckPendingSubModifications.Machines")) {
            machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAll ();
          }

          foreach (var machine in machines) {
            using (IDAOTransaction transaction = session.BeginTransaction ("Analysis.CheckPendingSubModifications.CreateNewAnalysisStatus")) {
              ModelDAOHelper.DAOFactory.MachineModificationDAO
                .CreateNewAnalysisStatusNoLimit (machine, false, ((IDataWithId<long>)modification).Id);
              // TODO: it could be optimized, setting a better value for LastModificationId
              transaction.Commit ();
            }
            using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.CheckPendingSubMachineModificationsByMachine")) {
              bool pendingMachineSubModifications = ModelDAOHelper.DAOFactory.GlobalModificationDAO
                .HasNotCompletedSubMachineModifications (modification, machine, false);
              if (pendingMachineSubModifications) {
                log.DebugFormat ("CheckPendingSubModifications: " +
                                 "there is at least one pending machine sub-modification");
                return true;
              }
            }
          } // modification.AnalysisSubMachineModifications
        }

        log.DebugFormat ("CheckPendingSubModifications: " +
                         "there is no pending sub-modification");
        return false;
      }
    }

    /// <summary>
    /// Get the maximum modificationId
    /// </summary>
    /// <returns></returns>
    protected override long? GetMaxModificationId ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.PendingGlobalModification.GetMaxModificationId")) {
          return ModelDAOHelper.DAOFactory.GlobalModificationDAO
            .GetMaxModificationId ();
        }
      }
    }

    /// <summary>
    /// Get the maximum modificationId asynchronously
    /// </summary>
    /// <returns></returns>
    protected override async Task<long?> GetMaxModificationIdAsync ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.PendingGlobalModification.GetMaxModificationId")) {
          return await ModelDAOHelper.DAOFactory.GlobalModificationDAO
            .GetMaxModificationIdAsync ();
        }
      }
    }

    /// <summary>
    /// Not really useful here because not used
    /// </summary>
    /// <returns></returns>
    protected override long GetLimitNewAnalysisStatus ()
    {
      return long.MaxValue;
    }

    /// <summary>
    /// Fill the analysis status
    /// 
    /// Not required to do anything here
    /// </summary>
    /// <param name="limit"></param>
    /// <returns></returns>
    public override bool FillAnalysisStatus (CancellationToken cancellationToken, long limit)
    {
      // Do nothing here because it is not required for the global modifications that are not partitioned by machine
      return true;
    }
    #endregion // Methods
  }
}
