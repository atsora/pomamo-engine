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

namespace Lemoine.Analysis
{
  /// <summary>
  /// Get the modifications that have not been completed yet and process them.
  /// 
  /// Try to get first in batch the past modifications and run them.
  /// Then run one by one the present modifications.
  /// </summary>
  public sealed class PendingGlobalMachineModificationAnalysis : PendingModificationAnalysis<IModification, Modification>, IThreadClass, IChecked
  {
    static readonly string PAST_MODIFICATIONS_BATCH_SIZE_KEY = "Analysis.Modification.Past.BatchSize";
    static readonly int PAST_MODIFICATIONS_BATCH_SIZE_DEFAULT = 8;

    static readonly string PAST_MODIFICATIONS_AGE_KEY = "Analysis.Modification.Past.Age";
    static readonly TimeSpan PAST_MODIFICATIONS_AGE_DEFAULT = TimeSpan.FromMinutes (20);

    static readonly string STEP_TIMEOUT_MARGIN_KEY = "Analysis.Modification.StepTimeoutMargin";
    static readonly TimeSpan STEP_TIMEOUT_MARGIN_DEFAULT = TimeSpan.FromSeconds (2); // Give 2 more seconds in the thread check

    static readonly string DELETE_NUMBER_BY_STEP_KEY = "Analysis.Modification.Delete.NumberByStep";
    static readonly int DELETE_NUMBER_BY_STEP_DEFAULT = 50;

    static readonly string DELETE_MAX_NUMBER_OF_MODIFICATIONS_KEY = "Analysis.Modification.Delete.MaxNumberOfModifications";
    static readonly int DELETE_MAX_NUMBER_OF_MODIFICATIONS_DEFAULT = 2000;

    static readonly string DELETE_NUMBER_MAX_DURATION_KEY = "Analysis.Modification.Delete.MaxDuration";
    static readonly TimeSpan DELETE_NUMBER_MAX_DURATION_DEFAULT = TimeSpan.FromSeconds (30);

    #region Members
    IList<IMachine> m_machines = null;
    #endregion // Members

    readonly ILog log = LogManager.GetLogger (typeof (PendingGlobalMachineModificationAnalysis).FullName);

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
    public PendingGlobalMachineModificationAnalysis ()
      : base ()
    {
    }

    /// <summary>
    /// Constructor
    /// 
    /// Run by default the analysis in a thread
    /// 
    /// Do not use this constructor in unit tests, because the Rollback hack does not work with it
    /// </summary>
    /// <param name="activityAnalysis"></param>
    public PendingGlobalMachineModificationAnalysis (ActivityAnalysis activityAnalysis)
      : base (activityAnalysis)
    {
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
    public PendingGlobalMachineModificationAnalysis (bool runInThread)
      : base (runInThread)
    {
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
    public PendingGlobalMachineModificationAnalysis (bool runInThread, ActivityAnalysis activityAnalysis)
      : base (runInThread, activityAnalysis)
    {
    }
    #endregion // Constructors

    #region Methods
    void LoadMachines ()
    {
      if (null == m_machines) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.PendingGlobalMachineModification.LoadMachines")) {
            m_machines = ModelDAOHelper.DAOFactory.MachineDAO
              .FindAll ();
          }
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
    protected override IEnumerable<ILockTableToPartition> CreateLockedTables (IDAOSession session)
    {
      // In the future, for performance reasons, tables could be locked
      // But not really required for the moment since this class is not used so often
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
    protected override IModification ReloadModification (IModification modification, bool lockTables = false)
    {
      IModification result;

      // Reload pastModification in case the status was updated
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ("Analysis.PendingGlobalMachineModification.ReloadPastModification",
                                                                     TransactionLevel.ReadCommitted))
    // Not read-only because a row in modificationstatus may be created
    {
        transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data
        result = ModelDAOHelper.DAOFactory.ModificationDAO
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
    protected override IEnumerable<IModification> GetPastPendingModifications (long lastModificationId,
                                                                               int lastPriority,
                                                                               DateTime before,
                                                                               int maxResults,
                                                                               int minPriority = 0)
    {
      IEnumerable<IModification> pastModifications;

      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = daoSession.BeginTransaction ("Analysis.PendingGlobalMachineModification.GetPast",
                                                                          TransactionLevel.ReadCommitted)) { // May be the read-only option is possible, but I am not sure
          // because I think the analysisstatus is initialized here
          transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data
          pastModifications =
            ModelDAOHelper.DAOFactory.GlobalModificationDAO
            .GetPastPendingModifications (lastModificationId, lastPriority,
                                          before, maxResults, minPriority)
            .Where (modification => (null == modification.ParentMachine))
            .Cast<IModification> ();
          transaction.Commit ();
        }

        LoadMachines ();

        foreach (var machine in m_machines) {
          using (IDAOTransaction transaction = daoSession.BeginReadOnlyTransaction ("Analysis.PendingMachineModification.GetPast",
                                                                                    TransactionLevel.ReadCommitted))
          // May be read-only because the row in modificationstatus is created by FillModificationStatus
          {
            transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data
            pastModifications = pastModifications
              .Concat (ModelDAOHelper.DAOFactory.MachineModificationDAO
                       .GetPastPendingModifications (machine,
                                                     lastModificationId, lastPriority,
                                                     before, maxResults, minPriority)
                       .Where (modification => (null == modification.ParentGlobal))
                       .Cast<IModification> ());
          }
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
    protected override IModification GetFirstPendingModification (long lastModificationId,
                                                                  int lastPriority,
                                                                  int minPriority = 0)
    {
      IModification modification;

      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = daoSession.BeginTransaction ("Analysis.PendingGlobalMachineModification.GetFirst",
                                                                          TransactionLevel.ReadCommitted)) { // May be the read-only option is possible, but I am not sure
          // because I think the analysisstatus is initialized here
          transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data
          modification = ModelDAOHelper.DAOFactory.GlobalModificationDAO
            .GetFirstPendingGlobalMachineModification (lastModificationId,
                                                       lastPriority,
                                                       minPriority);
          transaction.Commit ();
        }
        if (null != modification) {
          return modification;
        }

        LoadMachines ();

        foreach (IMachine machine in m_machines) {
          using (IDAOTransaction transaction = daoSession.BeginReadOnlyTransaction ("Analysis.PendingMachineModification.GetPast",
                                                                                    TransactionLevel.ReadCommitted))
          // May be read-only because the row in modificationstatus is created by FillModificationStatus
          {
            modification = ModelDAOHelper.DAOFactory.MachineModificationDAO
              .GetFirstPendingGlobalMachineModification (machine, lastModificationId, lastPriority, minPriority);
          } // Transaction
          if (null != modification) {
            return modification;
          }
        } // Loop on machines
      } // Session

      return null;
    }

    /// <summary>
    /// <see cref="PendingModificationAnalysis{I, T}"/>
    /// </summary>
    /// <param name="maxAnalysisDateTime"></param>
    /// <returns></returns>
    protected override bool DeleteFlaggedModifications (DateTime maxAnalysisDateTime)
    {
      var itemNumberByStep = Lemoine.Info.ConfigSet
        .LoadAndGet<int> (DELETE_NUMBER_BY_STEP_KEY, DELETE_NUMBER_BY_STEP_DEFAULT);
      var maxNumberOfModifications = Lemoine.Info.ConfigSet
        .LoadAndGet<int> (DELETE_MAX_NUMBER_OF_MODIFICATIONS_KEY, DELETE_MAX_NUMBER_OF_MODIFICATIONS_DEFAULT);
      var maxDuration = Lemoine.Info.ConfigSet
        .LoadAndGet<TimeSpan> (DELETE_NUMBER_MAX_DURATION_KEY, DELETE_NUMBER_MAX_DURATION_DEFAULT);
      var maxRunningDateTime = DateTime.UtcNow.Add (maxDuration);
      var maxDateTime = (maxAnalysisDateTime < maxRunningDateTime)
        ? maxRunningDateTime
        : maxAnalysisDateTime;

      ModelDAOHelper.DAOFactory.GlobalModificationDAO.Delete (AnalysisStatus.Delete);
      LoadMachines ();
      SetActive ();
      bool completed = true;
      foreach (IMachine machine in m_machines) {
        if (!ModelDAOHelper.DAOFactory.MachineModificationDAO.Delete (machine,
          AnalysisStatus.Delete,
          this,
          itemNumberByStep, maxNumberOfModifications, maxDateTime)) {
          log.WarnFormat ("DeleteFlaggedModifications: not completed (interrupted)");
          completed = false;
        }
        SetActive ();
      }
      return completed;
    }

    /// <summary>
    /// <see cref="PendingModificationAnalysis{I, T}"/>
    /// </summary>
    /// <param name="maxAnalysisDateTime"></param>
    /// <returns></returns>
    protected override bool DeleteDonePurgeModifications (DateTime maxAnalysisDateTime)
    {
      var itemNumberByStep = Lemoine.Info.ConfigSet
        .LoadAndGet<int> (DELETE_NUMBER_BY_STEP_KEY, DELETE_NUMBER_BY_STEP_DEFAULT);
      var maxNumberOfModifications = Lemoine.Info.ConfigSet
        .LoadAndGet<int> (DELETE_MAX_NUMBER_OF_MODIFICATIONS_KEY, DELETE_MAX_NUMBER_OF_MODIFICATIONS_DEFAULT);
      var maxDuration = Lemoine.Info.ConfigSet
        .LoadAndGet<TimeSpan> (DELETE_NUMBER_MAX_DURATION_KEY, DELETE_NUMBER_MAX_DURATION_DEFAULT);
      var maxRunningDateTime = DateTime.UtcNow.Add (maxDuration);
      var maxDateTime = (maxAnalysisDateTime < maxRunningDateTime)
        ? maxRunningDateTime
        : maxAnalysisDateTime;

      DateTime maxCompletionDateTime = DateTime.UtcNow.Subtract (AnalysisConfigHelper.AutoModificationPurgeDelay);
      ModelDAOHelper.DAOFactory.GlobalModificationDAO.Delete (AnalysisStatus.DonePurge, maxCompletionDateTime);
      LoadMachines ();
      SetActive ();
      bool completed = true;
      foreach (IMachine machine in m_machines) {
        if (!ModelDAOHelper.DAOFactory.MachineModificationDAO.Delete (machine, AnalysisStatus.DonePurge, maxCompletionDateTime,
         this,
         itemNumberByStep, maxNumberOfModifications, maxDateTime)) {
          log.WarnFormat ("DeleteDonePurgeModifications: not completed (interrupted)");
          completed = false;
        }
        SetActive ();
      }
      return completed;
    }

    /// <summary>
    /// Get the performance tracker key for a specific modification
    /// </summary>
    /// <returns></returns>
    protected override string GetPerfTrackerKey (IModification modification)
    {
      return "Analysis.Modification.GlobalMachine";
    }

    /// <summary>
    /// Process the sub-modifications of the specified modification
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="modification"></param>
    /// <param name="analysisStatus"></param>
    /// <param name="statusPriority"></param>
    protected override void ProcessSubModifications (CancellationToken cancellationToken, IModification modification, AnalysisStatus analysisStatus, int statusPriority)
    {
      bool notCompletedSubModifications = false;

      // - Get the sub-globalmodifications
      IEnumerable<IGlobalModification> subGlobalModifications;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("Analysis.PendingGlobalModification.GetSubModifications",
                                                                       TransactionLevel.ReadCommitted)) {
          transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data

          // Get the sub-modifications
          if (modification is IGlobalModification) {
            subGlobalModifications = ModelDAOHelper.DAOFactory.GlobalModificationDAO
              .GetNotCompletedSubGlobalModifications ((IGlobalModification)modification);
          }
          else if (modification is IMachineModification) {
            subGlobalModifications = ModelDAOHelper.DAOFactory.MachineModificationDAO
              .GetNotCompletedSubGlobalModifications ((IMachineModification)modification);
          }
          else {
            Debug.Assert (false);
            log.FatalFormat ("ProcessSubModifications: " +
                             "not supported/implemented modification type {0}",
                             modification);
            throw new Exception ("not supported/implemented modification type");
          }
          transaction.Commit ();
        }
      }

      // - Analyze the global sub-modifications
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
          notCompletedSubModifications = true;
        }
        log.DebugFormat ("MakeAnalysis: " +
                         "analysis of sub-modification {0} is completed",
                         subGlobalModification);
        SetActive ();
      }

      // - Get the machine sub-modifications
      IEnumerable<IMachineModification> subMachineModifications;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("Analysis.PendingGlobalModification.GetSubModifications",
                                                                       TransactionLevel.ReadCommitted)) {
          transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data

          // Get the sub-modifications
          if (modification is IGlobalModification) {
            subMachineModifications = ModelDAOHelper.DAOFactory.GlobalModificationDAO
              .GetNotCompletedSubMachineModifications ((IGlobalModification)modification);
          }
          else if (modification is IMachineModification) {
            subMachineModifications = ModelDAOHelper.DAOFactory.MachineModificationDAO
              .GetNotCompletedSubMachineModifications ((IMachineModification)modification);
          }
          else {
            Debug.Assert (false);
            log.FatalFormat ("ProcessSubModifications: " +
                             "not supported/implemented modification type {0}",
                             modification);
            throw new Exception ("not supported/implemented modification type");
          }
          transaction.Commit ();
        }
      }

      // - Analyze the machine sub-modifications
      foreach (IMachineModification subMachineModification in subMachineModifications) {
        log.DebugFormat ("MakeAnalysis: " +
                         "about to analyze sub-modification {0}",
                         subMachineModification);
        var subModificationAnalysis = new ModificationAnalysis (subMachineModification, this.ActivityAnalysis);
        while (!MakeAnalysis (cancellationToken, subMachineModification, subModificationAnalysis)) {
          log.InfoFormat ("MakeAnalysis: " +
                          "retry the analysis of sub-modification {0}",
                          subMachineModification);
          SetActive ();
        }
        // disable once ConvertIfToOrExpression
        if (subModificationAnalysis.AnalysisStatus.IsNotCompleted ()) {
          notCompletedSubModifications = true;
        }
        log.DebugFormat ("MakeAnalysis: " +
                         "analysis of sub-modification {0} is completed",
                         subMachineModification);
        SetActive ();
      }

      // - Update the status of the main modification if there is no pending sub-modification
      if (notCompletedSubModifications) {
        log.DebugFormat ("MakeAnalysis: " +
                         "one sub-modification has a pending status");
      }
      else { // !pendingGlobalSubModifications
        if (modification.AnalysisStatus.Equals (AnalysisStatus.Pending)) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("ProcessSubModifications: all sub-modifications completed, but current status is Pending, do nothing");
          }
          return;
        }
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
              log.InfoFormat ("ProcessSubModifications: done but keep the status {0} since it is not PendingSubModifications", analysisStatus);
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

    /// <summary>
    /// Get the maximum modificationId
    /// </summary>
    /// <returns></returns>
    protected override long? GetMaxModificationId ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.PendingGlobalMachineModification.GetMaxModificationId")) {
          long? maxModificationId = ModelDAOHelper.DAOFactory.GlobalModificationDAO
            .GetMaxModificationId ();
          LoadMachines ();
          foreach (IMachine machine in m_machines) {
            long? maxMachineModificationId = ModelDAOHelper.DAOFactory.MachineModificationDAO
              .GetMaxModificationId (machine);
            if (maxMachineModificationId.HasValue
                && (!maxModificationId.HasValue
                    || (maxModificationId.Value < maxMachineModificationId.Value))) {
              maxModificationId = maxMachineModificationId;
            }
          }
          return maxModificationId;
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
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.PendingGlobalMachineModification.GetMaxModificationId")) {
          long? maxModificationId = await ModelDAOHelper.DAOFactory.GlobalModificationDAO
            .GetMaxModificationIdAsync ();
          LoadMachines ();
          foreach (IMachine machine in m_machines) {
            long? maxMachineModificationId = await ModelDAOHelper.DAOFactory.MachineModificationDAO
              .GetMaxModificationIdAsync (machine);
            if (maxMachineModificationId.HasValue
                && (!maxModificationId.HasValue
                    || (maxModificationId.Value < maxMachineModificationId.Value))) {
              maxModificationId = maxMachineModificationId;
            }
          }
          return maxModificationId;
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override long GetLimitNewAnalysisStatus ()
    {
      // For the moment, do not set any limit
      return long.MaxValue;
    }

    /// <summary>
    /// Fill the analysis status
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    public override bool FillAnalysisStatus (CancellationToken cancellationToken, long limit)
    {
      bool limitReached = false;

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("Analysis.PendingGlobalMachineModification.FillAnalysisStatus", TransactionLevel.ReadCommitted)) {
          transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data
          LoadMachines ();
          foreach (var machine in m_machines) {
            if (cancellationToken.IsCancellationRequested) {
              if (GetLogger ().IsWarnEnabled) {
                GetLogger ().Warn ($"FillAnalysisStatus: cancellation requested");
              }
              transaction.Rollback ();
              return false;
            }
            bool machineLimitReached;
            ModelDAOHelper.DAOFactory.MachineModificationDAO
              .CreateNewAnalysisStatus (machine, false, 0, limit, out machineLimitReached);
            // false because ReadCommitted here
            limitReached |= machineLimitReached;
            // TODO: it could be optimized, setting a better value for minModificationId
          }
          transaction.Commit ();
        }
      }

      return !limitReached;
    }
    #endregion // Methods
  }
}
