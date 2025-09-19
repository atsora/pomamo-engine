// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Linq;
using Lemoine.GDBPersistentClasses;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Threading;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
using System.Threading.Tasks;
using System.Threading;
using Lemoine.Collections;

namespace Lemoine.Analysis
{
  /// <summary>
  /// Get the modifications that have not been completed yet and process them.
  /// 
  /// Try to get first in batch the past modifications and run them.
  /// Then run one by one the present modifications.
  /// </summary>
  public sealed class PendingMachineModificationAnalysis : PendingModificationAnalysis<IMachineModification, MachineModification>, IThreadClass, IChecked
  {
    static readonly string PAST_MODIFICATIONS_BATCH_SIZE_KEY = "Analysis.MachineModification.Past.BatchSize";
    static readonly int PAST_MODIFICATIONS_BATCH_SIZE_DEFAULT = 8;

    static readonly string PAST_MODIFICATIONS_AGE_KEY = "Analysis.MachineModification.Past.Age";
    static readonly TimeSpan PAST_MODIFICATIONS_AGE_DEFAULT = TimeSpan.FromMinutes (20);

    static readonly string LIMIT_NEW_ANALYSIS_BATCH_MULTIPLICATOR_KEY = "Analysis.MachineModification.LimitNewAnalysisBatchMultiplicator";
    static readonly int LIMIT_NEW_ANALYSIS_BATCH_MULTIPLICATOR_DEFAULT = 10;

    static readonly string STEP_TIMEOUT_MARGIN_KEY = "Analysis.MachineModification.StepTimeoutMargin";
    static readonly TimeSpan STEP_TIMEOUT_MARGIN_DEFAULT = TimeSpan.FromSeconds (2); // Give 2 more seconds in the thread check

    static readonly string DELETE_NUMBER_BY_STEP_KEY = "Analysis.Modification.Delete.NumberByStep";
    static readonly int DELETE_NUMBER_BY_STEP_DEFAULT = 50;

    static readonly string DELETE_MAX_NUMBER_OF_MODIFICATIONS_KEY = "Analysis.Modification.Delete.MaxNumberOfModifications";
    static readonly int DELETE_MAX_NUMBER_OF_MODIFICATIONS_DEFAULT = 500;

    static readonly string DELETE_NUMBER_MAX_DURATION_KEY = "Analysis.Modification.Delete.MaxDuration";
    static readonly TimeSpan DELETE_NUMBER_MAX_DURATION_DEFAULT = TimeSpan.FromSeconds (10);

    static readonly string FILL_ANALYSIS_STATUS_SERIALIZABLE_KEY = "Analysis.Modification.FillAnalysisStatusSerializable";
    static readonly bool FILL_ANALYSIS_STATUS_SERIALIZABLE_DEFAULT = true;

    static readonly string LOCK_TABLE_TO_PARTITION_KEY = "Analysis.Modification.LockTableToPartition";
    static readonly bool LOCK_TABLE_TO_PARTITION_VALUE = true;

    static readonly string LOCK_TABLE_TO_PARTITION_FILL_ANALYSIS_STATUS_KEY = "Analysis.Modification.LockTableToPartition.FillAnalysisStatus";
    static readonly bool LOCK_TABLE_TO_PARTITION_FILL_ANALYSIS_STATUS_VALUE = true;

    static readonly int SUB_MODIFICATION_ATTEMPT_WARNING = 100;
    static readonly int SUB_MODIFICATION_ATTEMPT_FATAL = 10000;

    readonly IMachine m_machine;
    long m_lastModificationStatusId = 0;
 
    readonly ILog log = LogManager.GetLogger (typeof (PendingMachineModificationAnalysis).FullName);

    /// <summary>
    /// Constructor
    /// 
    /// Run by default the analysis in a thread
    /// 
    /// Do not use this constructor in unit tests, because the Rollback hack does not work with it
    /// </summary>
    /// <param name="machine">not null</param>
    public PendingMachineModificationAnalysis (IMachine machine)
      : base ()
    {
      Debug.Assert (null != machine);

      m_machine = machine;
      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                this.GetType ().FullName,
                                                machine.Id));

      foreach (var extension in m_extensions) {
        extension.Initialize (machine);
      }
    }

    /// <summary>
    /// Constructor
    /// 
    /// Run by default the analysis in a thread
    /// 
    /// Do not use this constructor in unit tests, because the Rollback hack does not work with it
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="activityAnalysis"></param>
    public PendingMachineModificationAnalysis (IMachine machine, ActivityAnalysis activityAnalysis)
      : base (activityAnalysis)
    {
      Debug.Assert (null != machine);

      m_machine = machine;
      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                this.GetType ().FullName,
                                                machine.Id));

      foreach (var extension in m_extensions) {
        extension.Initialize (machine);
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
    /// <param name="machine"></param>
    /// <param name="runInThread"></param>
    public PendingMachineModificationAnalysis (IMachine machine, bool runInThread)
      : base (runInThread)
    {
      Debug.Assert (null != machine);

      m_machine = machine;
      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                this.GetType ().FullName,
                                                machine.Id));

      foreach (var extension in m_extensions) {
        extension.Initialize (machine);
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
    /// <param name="machine"></param>
    /// <param name="runInThread"></param>
    /// <param name="activityAnalysis"></param>
    public PendingMachineModificationAnalysis (IMachine machine, bool runInThread, ActivityAnalysis activityAnalysis)
      : base (runInThread, activityAnalysis)
    {
      Debug.Assert (null != machine);

      m_machine = machine;
      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                this.GetType ().FullName,
                                                machine.Id));

      foreach (var extension in m_extensions) {
        extension.Initialize (machine);
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
      return CreateLockedTables (session, m_machine);
    }

    IEnumerable<ILockTableToPartition> CreateLockedTables (IDAOSession session, IMachine machine)
    {
      Debug.Assert (null != machine);

      var lockTableToPartition = Lemoine.Info.ConfigSet
        .LoadAndGet (LOCK_TABLE_TO_PARTITION_KEY, LOCK_TABLE_TO_PARTITION_VALUE);
      if (lockTableToPartition) {
        return session.CreateLockTableToPartition (machine.Id, new string[] {
        "machinemodification",
        "reasonmachineassociation"
      });
      }
      else {
        return new List<ILockTableToPartition> ();
      }
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
    /// 
    /// This method contains its own read-only transaction
    /// </summary>
    /// <param name="modification"></param>
    /// <param name="lockTables"></param>
    /// <returns></returns>
    protected override IMachineModification ReloadModification (IMachineModification modification, bool lockTables = false)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IEnumerable<ILockTableToPartition> lockedTables = lockTables
          ? CreateLockedTables (session)
          : null;
        using (IDAOTransaction transaction = session
          .BeginReadOnlyTransaction ("Analysis.PendingMachineModification.ReloadPastModification",
                                     TransactionLevel.ReadCommitted,
                                     lockedTables: lockedTables))
        // May be read-only because the row in modificationstatus is created by FillModificationStatus
        {
          return ModelDAOHelper.DAOFactory.MachineModificationDAO
            .FindById (((Lemoine.Collections.IDataWithId<long>)modification).Id, modification.Machine);
        }
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
    protected override IEnumerable<IMachineModification> GetPastPendingModifications (long lastModificationId,
                                                                                      int lastPriority,
                                                                                      DateTime before,
                                                                                      int maxResults,
                                                                                      int minPriority = 0)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var lockedTables = CreateLockedTables (session);
        using (IDAOTransaction transaction = session
          .BeginReadOnlyTransaction ("Analysis.PendingMachineModification.GetPast",
                                     TransactionLevel.ReadCommitted,
                                     lockedTables: lockedTables))
        // It may be read-only because the row in modificationstatus is created by FillModificationStatus
        {
          return ModelDAOHelper.DAOFactory.MachineModificationDAO
            .GetPastPendingModifications (m_machine,
                                          lastModificationId, lastPriority,
                                          before, maxResults, minPriority);
        }
      }
    }

    /// <summary>
    /// Get the first pending modification
    /// </summary>
    /// <param name="lastModificationId"></param>
    /// <param name="lastPriority"></param>
    /// <param name="minPriority"></param>
    /// <returns></returns>
    protected override IMachineModification GetFirstPendingModification (long lastModificationId,
                                                                         int lastPriority,
                                                                         int minPriority)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var lockedTables = CreateLockedTables (session);
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.PendingMachineModification.GetFirst",
                                                                                  TransactionLevel.ReadCommitted, lockedTables: lockedTables))
        // It may be read-only because the row in modificationstatus is created by FillModificationStatus
        {
          return ModelDAOHelper.DAOFactory.MachineModificationDAO
            .GetFirstPendingModification (m_machine,
                                          lastModificationId,
                                          lastPriority,
                                          minPriority);
        }
      }
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

      if (!ModelDAOHelper.DAOFactory.MachineModificationDAO.Delete (m_machine, AnalysisStatus.Delete, null,
        itemNumberByStep, maxNumberOfModifications, maxDateTime)) {
        log.WarnFormat ("DeleteFlaggedModifications: not completed (interrupted)");
        return false;
      }
      return true;
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

      if (!ModelDAOHelper.DAOFactory.MachineModificationDAO.Delete (m_machine, AnalysisStatus.DonePurge, maxCompletionDateTime, null,
        itemNumberByStep, maxNumberOfModifications, maxDateTime)) {
        log.WarnFormat ("DeleteDonePurgeModifications: not completed (interrupted)");
        return false;
      }
      return true;
    }

    /// <summary>
    /// Get the performance tracker key for a specific modification
    /// </summary>
    /// <returns></returns>
    protected override string GetPerfTrackerKey (IMachineModification modification)
    {
      return "Analysis.Modification." + modification.Machine.Id;
    }

    /// <summary>
    /// Process the sub-modifications of the specified modification
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="modification"></param>
    /// <param name="analysisStatus"></param>
    /// <param name="statusPriority"></param>
    protected override void ProcessSubModifications (CancellationToken cancellationToken, IMachineModification modification, AnalysisStatus analysisStatus, int statusPriority)
    {
      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().Debug ($"ProcessSubModifications: id={((IDataWithId<long>)modification).Id} status={analysisStatus} statusPriority={statusPriority}");
      }

      // - Get the machine sub-modifications
      IEnumerable<IMachineModification> subSameMachineModifications;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var lockedTables = CreateLockedTables (session);
        using (IDAOTransaction transaction = session.BeginTransaction ("Analysis.PendingGlobalModification.GetSubModifications",
          TransactionLevel.ReadCommitted, lockedTables: lockedTables)) {
          transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data

          // Get the sub-modifications
          subSameMachineModifications = ModelDAOHelper.DAOFactory.MachineModificationDAO
            .GetNotCompletedSubMachineModifications (m_machine, modification);
          transaction.Commit ();
        }
      }

      // - Analyze the machine sub-modifications
      bool notCompletedSameMachineSubModifications = false;
      int? maxSameMachineSubStatusPriority = null;
      foreach (IMachineModification subSameMachineModification in subSameMachineModifications) {
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().DebugFormat ("ProcessSubModifications: " +
            "about to analyze sub-modification {0}",
            ((IDataWithId<long>)subSameMachineModification).Id);
        }
        var subModificationAnalysis = new ModificationAnalysis (subSameMachineModification, this.ActivityAnalysis, m_machine);
        {
          int i = 0;
          while (!MakeAnalysis (cancellationToken, subSameMachineModification, subModificationAnalysis)) {
            if (SUB_MODIFICATION_ATTEMPT_WARNING < i++) {
              if (GetLogger ().IsWarnEnabled) {
                GetLogger ().WarnFormat ("ProcessSubModifications: attempt {0} to complete sub-modification {1}",
                  i, ((IDataWithId<long>)subSameMachineModification).Id);
              }
              if (SUB_MODIFICATION_ATTEMPT_FATAL < i) {
                GetLogger ().FatalFormat ("ProcessSubModifications: attempt {0} to complete sub-modification {1}, throw an exception",
                  i, ((IDataWithId<long>)subSameMachineModification).Id);
                throw new Exception ("Attempt limit reached for sub-modification");
              }
            }
            if (GetLogger ().IsInfoEnabled) {
              GetLogger ().InfoFormat ("ProcessSubModifications: " +
                "retry the analysis of sub-modification {0}",
                ((IDataWithId<long>)subSameMachineModification).Id);
            }
            SetActive ();
          }
        }
        // disable once ConvertIfToOrExpression
        if (subModificationAnalysis.AnalysisStatus.IsNotCompleted ()) {
          notCompletedSameMachineSubModifications = true;
          if (!maxSameMachineSubStatusPriority.HasValue) {
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"ProcessSubModifications: sub-modification {((IDataWithId<long>)subSameMachineModification).Id} with status priority {subModificationAnalysis.StatusPriority}");
            }
            maxSameMachineSubStatusPriority = subModificationAnalysis.StatusPriority;
          }
          else if (maxSameMachineSubStatusPriority.Value < subModificationAnalysis.StatusPriority) {
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"ProcessSubModifications: sub-modification {((IDataWithId<long>)subSameMachineModification).Id} with a greater status priority {subModificationAnalysis.StatusPriority}");
            }
            maxSameMachineSubStatusPriority = subModificationAnalysis.StatusPriority;
          }
        }
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().DebugFormat ("ProcessSubModifications: " +
            "analysis of sub-modification {0} is completed",
            ((IDataWithId<long>)subSameMachineModification).Id);
        }
        SetActive ();
      }

      // - Update the status of the main modification if there is no pending sub-modification
      if (notCompletedSameMachineSubModifications) {
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ($"ProcessSubModifications: " +
            $"one sub-modification has a not completed status, new priority is {maxSameMachineSubStatusPriority} while current status priority is {statusPriority}");
        }
        if (maxSameMachineSubStatusPriority.HasValue
          && (statusPriority != maxSameMachineSubStatusPriority.Value)) {
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (IDAOTransaction transaction = session.BeginTransaction ("Analysis.PendingModification.SubModificationNewPriority",
                                                                           TransactionLevel.ReadCommitted)) {
              transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data

              // Reload the modification
              modification = ModelDAOHelper.DAOFactory.MachineModificationDAO
                .FindById (((Lemoine.Collections.IDataWithId<long>)modification).Id, modification.Machine);

              if (GetLogger ().IsDebugEnabled) {
                GetLogger ().Debug ($"ProcessSubModifications: change status priority from {modification.StatusPriority} to {maxSameMachineSubStatusPriority.Value} for modification id {((IDataWithId<long>)modification).Id}");
              }

              modification.StatusPriority = maxSameMachineSubStatusPriority.Value;
              ModelDAOHelper.DAOFactory.ModificationDAO.MakePersistent (modification);

              transaction.Commit ();
            } // transaction
          } // session
        }
      }
      else if (AnalysisStatus.PendingSubModifications != analysisStatus) {
        if (GetLogger ().IsErrorEnabled) {
          GetLogger ().ErrorFormat ("ProcessSubModifications: all sub-modifications were processed but keep the status {0} of modification id={1} unchanged since it is not PendingSubModifications",
            modification.AnalysisStatus, ((IDataWithId<long>)modification).Id);
        }
      }
      else { // !notCompletedSameMachineSubModifications
        if (!CheckNotCompletedSubModifications (modification)) {
          var allSubModificationsCompleted = false;
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (IDAOTransaction transaction = session.BeginTransaction ("Analysis.PendingModification.SubModificationDone",
                                                                           TransactionLevel.ReadCommitted)) {
              transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data

              // Reload the modification
              modification = ModelDAOHelper.DAOFactory.MachineModificationDAO
                .FindById (((Lemoine.Collections.IDataWithId<long>)modification).Id, modification.Machine);

              if (GetLogger ().IsDebugEnabled) {
                GetLogger ().DebugFormat ("ProcessSubModifications: " +
                  "the modification {0} with sub-modifications is now completed",
                  ((IDataWithId<long>)modification).Id);
              }
              if (modification.AnalysisStatus.Equals (AnalysisStatus.PendingSubModifications)) {
                modification.MarkAllSubModificationsCompleted ();
                allSubModificationsCompleted = true;
              }
              else {
                if (GetLogger ().IsInfoEnabled) {
                  GetLogger ().InfoFormat ("ProcessSubModifications: done but keep the status {0} since it is not PendingSubModifications", analysisStatus);
                }
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

    bool CheckNotCompletedSubModifications (IMachineModification modification)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        if (modification.AnalysisSubMachineModifications) {
          IList<IMachine> machines;
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.CheckPendingSubModifications.Machines")) {
            machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAll ();
          }

          foreach (var machine in machines) {
            var minModificationId = ((IDataWithId<long>)modification).Id;
            using (IDAOTransaction transaction = session.BeginTransaction ("Analysis.CheckPendingSubModifications.FillAnalysisStatus")) {
              transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data
              ModelDAOHelper.DAOFactory.MachineModificationDAO
                .CreateNewAnalysisStatusNoLimit (m_machine, false, minModificationId);
              transaction.Commit ();
            }
            var lockedTables = CreateLockedTables (session, machine);
            using (IDAOTransaction transaction = session
              .BeginReadOnlyTransaction ("Analysis.CheckPendingSubMachineModificationsByMachine",
                                         lockedTables: lockedTables)) {
              bool pendingMachineSubModifications = ModelDAOHelper.DAOFactory.MachineModificationDAO
                .HasNotCompletedSubMachineModifications (modification, machine, false);
              if (pendingMachineSubModifications) {
                if (GetLogger ().IsDebugEnabled) {
                  GetLogger ().Debug ("CheckPendingSubModifications: " +
                    "there is at least one pending machine sub-modification");
                }
                return true;
              }
            }
          } // modification.AnalysisSubMachineModifications
        }

        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.CheckPendingSubGlobalModifications")) {
          bool pendingGlobalSubModifications = ModelDAOHelper.DAOFactory.MachineModificationDAO
            .HasNotCompletedGlobalModifications (modification);
          if (pendingGlobalSubModifications) {
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ("CheckPendingSubModifications: " +
                "there is at least one pending global sub-modification");
            }
            return true;
          }
        }

        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ("CheckPendingSubModifications: " +
            "there is no pending sub-modification");
        }
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
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.PendingMachineModification.GetMaxModificationId")) {
          return ModelDAOHelper.DAOFactory.MachineModificationDAO
            .GetMaxModificationId (m_machine);
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
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.PendingMachineModification.GetMaxModificationId")) {
          return await ModelDAOHelper.DAOFactory.MachineModificationDAO
            .GetMaxModificationIdAsync (m_machine);
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override long GetLimitNewAnalysisStatus ()
    {
      var limitNewAnalysisBatchMultiplicator = Lemoine.Info.ConfigSet
        .LoadAndGet<int> (LIMIT_NEW_ANALYSIS_BATCH_MULTIPLICATOR_KEY,
        LIMIT_NEW_ANALYSIS_BATCH_MULTIPLICATOR_DEFAULT);
      return GetPastModificationsBatchSize () * limitNewAnalysisBatchMultiplicator;
    }

    /// <summary>
    /// Not to run in a parent transaction
    /// </summary>
    /// <param name="limit"></param>
    /// <returns></returns>
    public override bool FillAnalysisStatus (CancellationToken cancellationToken, long limit)
    {
      bool serializable = Lemoine.Info.ConfigSet
        .LoadAndGet (FILL_ANALYSIS_STATUS_SERIALIZABLE_KEY, FILL_ANALYSIS_STATUS_SERIALIZABLE_DEFAULT);
      var transactionLevel = serializable ? TransactionLevel.Serializable : TransactionLevel.Default;

      bool limitReached;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IEnumerable<ILockTableToPartition> lockedTables;
        var lockTable = Lemoine.Info.ConfigSet
          .LoadAndGet (LOCK_TABLE_TO_PARTITION_FILL_ANALYSIS_STATUS_KEY,
          LOCK_TABLE_TO_PARTITION_FILL_ANALYSIS_STATUS_VALUE);
        if (lockTable) {
          lockedTables = session.CreateLockTableToPartition (m_machine.Id, new string[] { "machinemodification", "machinemodificationstatus" });
        }
        else {
          lockedTables = new List<ILockTableToPartition> ();
        }
        if (cancellationToken.IsCancellationRequested) {
          if (GetLogger ().IsWarnEnabled) {
            GetLogger ().Warn ("FillAnalysisStatus: cancellation requested");
          }
          return false;
        }
        using (IDAOTransaction transaction = session
          .BeginTransaction ("Analysis.PendingMachineModification.FillAnalysisStatus",
                             transactionLevel,
                             lockedTables: lockedTables)) {
          transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data
          // Note: this must be the top serializable transaction
          m_lastModificationStatusId = ModelDAOHelper.DAOFactory.MachineModificationDAO
            .CreateNewAnalysisStatus (m_machine, serializable, m_lastModificationStatusId, limit, out limitReached);
          transaction.Commit ();
        }
      }
      return !limitReached;
    }
  }
}
