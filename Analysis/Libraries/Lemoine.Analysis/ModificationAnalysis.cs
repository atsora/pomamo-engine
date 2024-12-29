// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Core.ExceptionManagement;
using Lemoine.GDBPersistentClasses;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Threading;
using System.Threading;
using Lemoine.Collections;
using Lemoine.Database.Persistent;

namespace Lemoine.Analysis
{
  /// <summary>
  /// Class to run a single modification
  /// </summary>
  public class ModificationAnalysis
    : ThreadClass, IThreadClass, IChecked
  {
    /// <summary>
    /// App setting key for the restricted transaction level
    /// </summary>
    static readonly string RESTRICTED_TRANSACTION_LEVEL_KEY = "Analysis.Modification.RestrictedTransactionLevel";
    static readonly string RESTRICTED_TRANSACTION_LEVEL_DEFAULT = "";

    static readonly string MAX_PAUSE_REQUEST_TRY_ATTEMPT_KEY = "Analysis.Modification.MaxPauseRequestTryAttempt";
    static readonly int MAX_PAUSE_REQUEST_TRY_ATTEMPT_DEFAULT = 5;

    static readonly string LOCK_TABLE_TO_PARTITION_KEY = "Analysis.Modification.LockTableToPartition";
    static readonly bool LOCK_TABLE_TO_PARTITION_VALUE = true;

    static readonly string TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_KEY = "Analysis.Modification.TemporaryWithDelayException.Sleep";
    static readonly TimeSpan TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_DEFAULT = TimeSpan.FromSeconds (2);

    #region Members
    volatile bool m_timeoutInterruption = false;
    readonly IModification m_modification;
    readonly IActivityAnalysis m_activityAnalysis = null;
    Exception m_exceptionStatus = null;
    volatile bool m_retry = false;
    volatile AnalysisStatus m_analysisStatus = AnalysisStatus.New;
    volatile int m_statusPriority = -1; // Not set: -1
    int m_requestPauseAttempt = 0;
    TransactionLevel m_restrictedTransactionLevel = TransactionLevel.Serializable;
    volatile int m_analysisConnectionId = -1;
    readonly IMachine m_machine;
    #endregion // Members

    readonly ILog log = LogManager.GetLogger (typeof (ModificationAnalysis).FullName);

    #region Getters / Setters
    /// <summary>
    /// Timeout interruption
    /// </summary>
    public bool TimeoutInterruption
    {
      get { return m_timeoutInterruption; }
      private set { m_timeoutInterruption = value; }
    }

    /// <summary>
    /// Connection ID of the analysis.
    /// 
    /// -1 is returned if not applicable
    /// </summary>
    public int AnalysisConnectionId
    {
      get { return m_analysisConnectionId; }
    }

    /// <summary>
    /// Exception status once Run () completed.
    /// 
    /// This is not thread safe
    /// </summary>
    public Exception ExceptionStatus
    {
      get { return m_exceptionStatus; }
    }

    /// <summary>
    /// Should the modification be retried ?
    /// For example in case of serialization failure
    /// </summary>
    public bool Retry
    {
      get { return m_retry; }
    }

    /// <summary>
    /// Analysis status of the modification
    /// </summary>
    public AnalysisStatus AnalysisStatus
    {
      get { return m_analysisStatus; }
    }

    /// <summary>
    /// Status priority of the modification
    /// </summary>
    public int StatusPriority
    {
      get
      {
        Debug.Assert (-1 != m_statusPriority); // Preivously set
        return m_statusPriority;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="modification">Not null</param>
    /// <param name="activityAnalysis"></param>
    /// <param name="machine">analysis on a specific machine (nullable)</param>
    public ModificationAnalysis (IModification modification, IActivityAnalysis activityAnalysis = null, IMachine machine = null)
    {
      Debug.Assert (null != modification);

      m_machine = machine;
      if (null != m_machine) {
        log = LogManager.GetLogger (typeof (ModificationAnalysis).FullName + "." + m_machine.Id + "." + ((IDataWithId<long>)modification).Id);
      }
      else {
        log = LogManager.GetLogger (typeof (ModificationAnalysis).FullName + "." + ((IDataWithId<long>)modification).Id);
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"ModificationAnalysis: initial status={modification.AnalysisStatus} priority={modification.StatusPriority}");
      }

      m_modification = modification;
      m_analysisStatus = modification.AnalysisStatus;
      m_statusPriority = modification.StatusPriority;
      m_activityAnalysis = activityAnalysis;
      LoadConfigurations ();
    }

    void LoadConfigurations ()
    {
      string restrictedTransactionLevelString = Lemoine.Info.ConfigSet
        .LoadAndGet (RESTRICTED_TRANSACTION_LEVEL_KEY, RESTRICTED_TRANSACTION_LEVEL_DEFAULT);
      if (!string.IsNullOrEmpty (restrictedTransactionLevelString)) {
        try {
          m_restrictedTransactionLevel = (TransactionLevel)Enum
            .Parse (typeof (TransactionLevel), restrictedTransactionLevelString);
          if (log.IsInfoEnabled) {
            log.Info ($"LoadConfigurations: got the transaction level {m_restrictedTransactionLevel} from configuration");
          }
        }
        catch (OperationCanceledException) {
          throw;
        }
        catch (Lemoine.Threading.AbortException) {
          throw;
        }
        catch (Exception ex) {
          log.Error ($"LoadConfigurations: parsing the configuration {RESTRICTED_TRANSACTION_LEVEL_KEY}={restrictedTransactionLevelString} failed", ex);
        }
      }
    }
    #endregion // Constructors

    /// <summary>
    /// Flag it with a timeout interruption
    /// </summary>
    public void FlagTimeoutInterruption ()
    {
      this.TimeoutInterruption = true;
    }

    /// <summary>
    /// <see cref="ThreadClass"></see>
    /// </summary>
    /// <returns></returns>
    public override ILog GetLogger ()
    {
      return log;
    }

    #region Methods
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

    string BuildTransactionName (string prefix)
    {
      var transactionName = prefix;
      var transactionNameSuffix = m_modification.GetTransactionNameSuffix ();
      if (!string.IsNullOrEmpty (transactionNameSuffix)) {
        transactionName += transactionNameSuffix;
      }
      return transactionName;
    }

    /// <summary>
    /// Analyze the modification
    /// </summary>
    protected override void Run (CancellationToken cancellationToken)
    {
      m_exceptionStatus = null;
      m_retry = false;
      IList<IMachine> impactedMachines = null;

      if (log.IsDebugEnabled) {
        log.DebugFormat ("Run: start");
      }

      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          if (null != m_activityAnalysis) { // Test if a pause in a monitored machine activity analysis is required
            // Re-associate m_modification to this session, else we can't get the property of the machine
            ModelDAOHelper.DAOFactory.ModificationDAO.Lock (m_modification);
            impactedMachines =
              m_modification.GetImpactedActivityAnalysis ();
            if (null != impactedMachines) {
              if (log.IsDebugEnabled) {
                log.Debug ($"Run: {impactedMachines.Count} impacted machines");
              }
              foreach (IMachine impactedMachine in impactedMachines) {
                if (cancellationToken.IsCancellationRequested) {
                  if (log.IsInfoEnabled) {
                    log.Info ($"Run: cancellation was requested");
                  }
                  m_retry = true;
                  return;
                }
                if (!m_activityAnalysis.RequestPause (impactedMachine, ((Lemoine.Collections.IDataWithId<long>)m_modification).Id)) {
                  log.Warn ($"Run: the pause on machine Id {impactedMachine.Id} is already set on another modification => sleep a while and start again later");
                  // Note: m_requestPauseAttempt is here to limit the causes of deadlocks
                  //       if by chance in the future several modifications may be run in the same time
                  this.Sleep (TimeSpan.FromMilliseconds (100), cancellationToken);
                  ++m_requestPauseAttempt;
                  m_retry = true;
                  return;
                }
              } // foreach impactedMachine in impactedMachines

              if (!m_restrictedTransactionLevel.Equals (TransactionLevel.Serializable)) {
                // If the transaction is not serializable, make the analysis only once the activity analysis is really in pause
                foreach (IMachine impactedMachine in impactedMachines) {
                  if (log.IsDebugEnabled) {
                    log.Debug ($"Run: about to wait the analysis of machine {impactedMachine.Id} is in pause for modification");
                  }
                  m_activityAnalysis.WaitPause (impactedMachine, ((Lemoine.Collections.IDataWithId<long>)m_modification).Id, cancellationToken);
                  if (log.IsDebugEnabled) {
                    log.Debug ($"Run: analysis of machine {impactedMachine.Id} is now in pause for modification");
                  }
                } // foreach impactedMachine in impactedMachines
                log.Info ($"Run: the analysis of all impacted machines for modification are now in pause");
              }
            } // null != impactedMachines
          }

          // Clean m_requestPauseAttempt because the all the required pauses were acquired
          m_requestPauseAttempt = 0;

          IEnumerable<ILockTableToPartition> lockedTables;
          if (null != impactedMachines) {
            if (1 == impactedMachines.Count) {
              lockedTables = CreateLockedTables (session, impactedMachines[0]);
            }
            else {
              lockedTables = null;
            }
          }
          else {
            if (null != m_machine) {
              lockedTables = CreateLockedTables (session, m_machine);
            }
            else {
              lockedTables = null;
            }
          }

          using (IDAOTransaction transaction = session
            .BeginTransaction (BuildTransactionName ("Analysis.Modification.Run"),
                               m_restrictedTransactionLevel,
                               lockedTables: lockedTables)) {
            // Restrict the transaction level if possible because there can be (rarely) some concurrent updates
            transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data

            m_analysisConnectionId = ModelDAOHelper.DAOFactory.GetCurrentConnectionId ();

            if (m_restrictedTransactionLevel.Equals (TransactionLevel.Serializable)) {
              // For serializable transactions, do not use an UpgradeLock
              ModelDAOHelper.DAOFactory.ModificationDAO.Lock (m_modification);
            }
            else {
              ModelDAOHelper.DAOFactory.ModificationDAO.UpgradeLock (m_modification);
            }
            try {
              if (log.IsDebugEnabled) {
                log.Debug ($"Run: run MakeAnalysis");
              }
              MakeAnalysis ();
              if (log.IsDebugEnabled) {
                log.Debug ($"Run: MakeAnalysis is completed");
              }
            }
            catch (System.Threading.ThreadAbortException ex) {
              if (this.TimeoutInterruption) {
                if (log.IsDebugEnabled) {
                  log.Debug ("Run: thread abord exception was requested by a time out => rollback excplicitely the transaction", ex);
                }
                transaction.Rollback ();
              }
              throw;
            }
            catch (Lemoine.Threading.AbortException ex) {
              if (this.TimeoutInterruption) {
                if (log.IsDebugEnabled) {
                  log.Debug ("Run: thread abord exception was requested by a time out => rollback excplicitely the transaction", ex);
                }
                transaction.Rollback ();
              }
              throw;
            }
            catch (OperationCanceledException ex) {
              if (this.TimeoutInterruption) {
                if (log.IsDebugEnabled) {
                  log.Debug ("Run: operation canceled exception was requested by a time out => rollback excplicitely the transaction", ex);
                }
                transaction.Rollback ();
              }
              throw;
            }
            catch (Exception ex) {
              if (ExceptionTest.IsTransactionSerializationFailure (ex)) {
                transaction.FlagSerializationFailure ();
              }
              if (IsStepTimeoutFailure (ex, log)) {
                if (log.IsDebugEnabled) {
                  log.Debug ("Run: step timeout failure => rollback explicitely the transaction", ex);
                }
                try {
                  transaction.Rollback ();
                }
                catch (OperationCanceledException) {
                  throw;
                }
                catch (Lemoine.Threading.AbortException) {
                  throw;
                }
                catch (Exception rollbackException) {
                  if (log.IsErrorEnabled) {
                    log.Error ("Run: exception in Rollback ()", rollbackException);
                  }
                }
              }
              else { // Else implicit rollback
                if (log.IsDebugEnabled) {
                  log.Debug ("Run: not a timeout exception => implicit rollback", ex);
                }
              }
              throw;
            }
            if (log.IsDebugEnabled) {
              log.Debug ($"Run: Analysis is completed (before commit)");
            }
            transaction.Commit ();
          }
          // Commit is ok => you can update the analysis status
          if (log.IsDebugEnabled) {
            log.Debug ($"Run: after commit, new Status={m_modification?.AnalysisStatus} statusPriority={m_modification?.StatusPriority}");
          }
          m_analysisStatus = m_modification.AnalysisStatus;
          m_statusPriority = m_modification.StatusPriority;
          if (m_analysisStatus.Equals (AnalysisStatus.InProgress)
              || m_analysisStatus.Equals (AnalysisStatus.StepTimeout)
              || m_analysisStatus.Equals (AnalysisStatus.Timeout)
              || m_analysisStatus.Equals (AnalysisStatus.DatabaseTimeout)) {
            m_retry = true;
            if (log.IsInfoEnabled) {
              log.Info ($"Run: analysis status {m_modification?.AnalysisStatus} in progress or step timeout in {((IDataWithId<long>)m_modification)?.Id}, set retry to true");
            }
          }
          if (ModelDAOHelper.DAOFactory.IsTransactionActive ()) {
            log.Fatal ("Run: a transaction wass still active when the analysis status was updated");
          }
        }
      }
      catch (System.Threading.ThreadAbortException ex) {
        ((Modification)m_modification).RestoreStatus (m_analysisStatus, m_statusPriority);
        if (this.TimeoutInterruption) {
          if (log.IsInfoEnabled) {
            log.Info ("Run: thread abort exception because it was requested by a timeout", ex);
          }
          return;
        }
        else { // !this.TimeoutInterruption
          if (log.IsErrorEnabled) {
            log.Error ("Run: thread abort exception although it was not requested by a timeout", ex);
          }
          m_exceptionStatus = ex;
          return;
        }
      }
      catch (Lemoine.Threading.AbortException ex) {
        ((Modification)m_modification).RestoreStatus (m_analysisStatus, m_statusPriority);
        if (this.TimeoutInterruption) {
          if (log.IsInfoEnabled) {
            log.Info ("Run: AbortException because it was requested by a timeout", ex);
          }
          return;
        }
        else { // !this.TimeoutInterruption
          if (log.IsErrorEnabled) {
            log.Error ("Run: AbortException although it was not requested by a timeout", ex);
          }
          m_exceptionStatus = ex;
          return;
        }
      }
      catch (OperationCanceledException ex) {
        ((Modification)m_modification).RestoreStatus (m_analysisStatus, m_statusPriority);
        if (this.TimeoutInterruption) {
          if (log.IsInfoEnabled) {
            log.Info ("Run: OperationCanceledException because it was requested by a timeout", ex);
          }
          return;
        }
        else { // !this.TimeoutInterruption
          if (log.IsErrorEnabled) {
            log.Error ("Run: OperationCanceledException was not requested by a timeout", ex);
          }
          m_exceptionStatus = ex;
          return;
        }
      }
      catch (OutOfMemoryException ex) {
        log.Error ("Run: OutOfMemoryException, give up", ex);
        SetExitRequested ();
        throw new OutOfMemoryException ("OutOfMemoryException raised in MakeAnalysis", ex);
      }
      catch (Exception ex) {
        ((Modification)m_modification).RestoreStatus (m_analysisStatus, m_statusPriority);
        if (ExceptionTest.IsStale (ex, log)) {
          log.Warn ($"Run: Analysis failed with a Stale exception", ex);
          m_retry = true;
          Debug.Assert (!ModelDAOHelper.DAOFactory.IsTransactionActive ());
          if (ModelDAOHelper.DAOFactory.IsTransactionActive ()) {
            log.Fatal ("Run: after a stale exception, the transaction is still active");
            SetExitRequested ();
            throw new Exception ("Transaction active with inner StaleObjectException raised in MakeAnalysis", ex);
          }
          return;
        }
        else if (ExceptionTest.IsTransactionSerializationFailure (ex, log)) { // Serialization only
          log.Warn ("Run: Analysis failed with a serialization failure", ex);
          m_retry = true;
          Debug.Assert (!ModelDAOHelper.DAOFactory.IsTransactionActive ());
          if (ModelDAOHelper.DAOFactory.IsTransactionActive ()) {
            log.Fatal ($"Run: after a serialization failure, the transaction is still active");
            SetExitRequested ();
            throw new Lemoine.Threading.AbortException ("Transaction active with inner serialization failure raised in MakeAnalysis.", ex);
          }
          return;
        }
        else if (IsStepTimeoutFailure (ex, log)) {
          HandleStepTimeoutException (ex);
          return;
        }
        else if (ExceptionTest.IsTimeoutFailure (ex, log)) {
          HandleDatabaseTimeoutException (ex);
          return;
        }
        else if (ExceptionTest.IsIntegrityConstraintViolation (ex, log)) {
          HandleIntegrityConstraintViolation (ex);
          return;
        }
        else if (ExceptionTest.IsTemporaryWithDelay (ex, log)) {
          var temporaryWithDelayExceptionSleepTime = Lemoine.Info.ConfigSet
            .LoadAndGet (TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_KEY, TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_DEFAULT);
          log.Warn ($"Run: temporary with delay exception inner {ex.InnerException}, try again after sleeping {temporaryWithDelayExceptionSleepTime}", ex);
          this.Sleep (temporaryWithDelayExceptionSleepTime, this.CancellationToken);
          m_exceptionStatus = ex;
          return;
        }
        else if (ExceptionTest.RequiresExit (ex, log)) {
          log.Error ("Run: exception requires to exit, give up", ex);
          SetExitRequested ();
          throw new Lemoine.Threading.AbortException ("Exception that requires to exit", ex);
        }
        else if (ex is NullReferenceException) {
          log.Error ("Run: NullReferenceException", ex);
          m_exceptionStatus = ex;
          return;
        }
        else {
          log.Exception (ex, "Run");
          m_exceptionStatus = ex;
          return;
        }
      }
      finally {
        // release a potential pause in the activity analysis
        // except if the modification is aimed at being restarted
        if ((null != m_activityAnalysis) && (null != impactedMachines)) {
          int maxTryAttempt = ConfigSet.LoadAndGet<int> (MAX_PAUSE_REQUEST_TRY_ATTEMPT_KEY,
                                                         MAX_PAUSE_REQUEST_TRY_ATTEMPT_DEFAULT);
          if (m_retry && (m_requestPauseAttempt < maxTryAttempt)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Run: do not release the pause because the modification will be retried and the number of request pause attempt {m_requestPauseAttempt} is low enough");
            }
          }
          else {
            if (m_retry) {
              log.Warn ($"Run: the modification will be retried but release the pause anyway because the number of attempt to get the pause {m_requestPauseAttempt} reached the limit {maxTryAttempt}");
            }
            m_requestPauseAttempt = 0;
            foreach (IMachine impactedMachine in impactedMachines) {
              if (log.IsDebugEnabled) {
                log.Debug ($"Run: release the pause and machineId {impactedMachine.Id}");
              }
              m_activityAnalysis.ReleasePause (impactedMachine, ((Lemoine.Collections.IDataWithId<long>)m_modification).Id);
            } // foreach impactedMachine in impactedMachines
          }
        }
      }
    }

    /// <summary>
    /// Check if the raised exception is a StepTimeoutException
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static bool IsStepTimeoutFailure (Exception ex, ILog logger)
    {
      if (ex is StepTimeoutException) {
        logger.Info ("IsStepTimeoutFailure: step timeout exception", ex);
        return true;
      }

      if (null != ex.InnerException) {
        if (logger.IsDebugEnabled) {
          logger.Debug ("IsStepTimeoutFailure: inspect inner exception", ex);
        }
        return IsStepTimeoutFailure (ex.InnerException, logger);
      }

      return false;
    }

    void MakeAnalysis ()
    {
      DateTime begin = DateTime.UtcNow;
      if (!m_modification.AnalysisBegin.HasValue) {
        m_modification.AnalysisBegin = begin;
      }
      ++m_modification.AnalysisIterations;
      ((Modification)m_modification).RunAnalysis ();
      ModelDAOHelper.DAOFactory.FlushData (); // cf. massive updates
      DateTime end = DateTime.UtcNow;
      TimeSpan duration = end.Subtract (begin);
      m_modification.AnalysisTotalDuration = m_modification.AnalysisTotalDuration.Add (duration);
      m_modification.AnalysisLastDuration = duration;
      m_modification.AnalysisEnd = end;
      ModelDAOHelper.DAOFactory.ModificationDAO.MakePersistent (m_modification);
    }

    /// <summary>
    /// In case of command timeout exception, flag the modification as databasetimeout,
    /// else record the exception status
    /// </summary>
    /// <param name="ex"></param>
    void HandleDatabaseTimeoutException (Exception ex)
    {
      log.Warn ("HandleTimeoutException: timeout in request => try to set the modification status to timeout");

      try {
        // Command timeout => set the status of the modification to timeout
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction (BuildTransactionName ("Analysis.Modification.HandleTimeoutException"),
                                                                       TransactionLevel.ReadCommitted)) {
          transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data

          m_modification.MarkAsDatabaseTimeout ();
          ModelDAOHelper.DAOFactory.ModificationDAO.MakePersistent (m_modification);
          ModelDAOHelper.DAOFactory.AnalysisLogDAO.Add (m_modification,
                                                        LogLevel.ERROR,
                                                        "Npgsql command timeout");
          transaction.Commit ();
        }
        // Commit is ok, you can update the analysis status
        if (log.IsDebugEnabled) {
          log.Debug ($"HandleDatabaseTimeoutException: after commit, new Status={m_modification.AnalysisStatus} statusPriority={m_modification.StatusPriority}");
        }
        m_analysisStatus = m_modification.AnalysisStatus;
        m_statusPriority = m_modification.StatusPriority;
        if (ModelDAOHelper.DAOFactory.IsTransactionActive ()) {
          log.Fatal ("HandleTimeoutException: a transaction was still active when the analysis status was updated");
        }
      }
      catch (OperationCanceledException timeoutStatusException) {
        ((Modification)m_modification).RestoreStatus (m_analysisStatus, m_statusPriority);
        log.Error ("HandleTimeoutException: exception while trying to record a timeout status",
          timeoutStatusException);
        m_exceptionStatus = ex;
        throw;
      }
      catch (Lemoine.Threading.AbortException timeoutStatusException) {
        ((Modification)m_modification).RestoreStatus (m_analysisStatus, m_statusPriority);
        log.Error ("HandleTimeoutException: exception while trying to record a timeout status",
          timeoutStatusException);
        m_exceptionStatus = ex;
        throw;
      }
      catch (Exception timeoutStatusException) {
        ((Modification)m_modification).RestoreStatus (m_analysisStatus, m_statusPriority);
        log.Error ("HandleTimeoutException: exception while trying to record a timeout status",
          timeoutStatusException);
        m_exceptionStatus = ex;
        return;
      }
    }

    /// <summary>
    /// In case of step timeout exception, flag the modification as steptimeout,
    /// else record the exception status
    /// </summary>
    /// <param name="ex"></param>
    void HandleStepTimeoutException (Exception ex)
    {
      log.Warn ("HandleStepTimeoutException: step timeout => try to set the modification status to steptimeout");

      Debug.Assert (!ModelDAOHelper.DAOFactory.IsTransactionActive ());
      if (ModelDAOHelper.DAOFactory.IsTransactionActive ()) {
        log.Fatal ("HandleStepTimeoutException: after a steptimeout failure, the transaction is still active");
      }

      try {
        // steptimeout => set the status of the modification to timeout
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction (BuildTransactionName ("Analysis.Modification.HandleStepTimeoutException"),
          TransactionLevel.ReadCommitted)) {
          transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data

          m_modification.MarkAsStepTimeout ();
          ModelDAOHelper.DAOFactory.ModificationDAO.MakePersistent (m_modification);
          transaction.Commit ();
        }
        // Commit is ok, you can update the analysis status
        if (log.IsDebugEnabled) {
          log.Debug ($"HandleStepTimeoutException: after commit, new Status={m_modification.AnalysisStatus} statusPriority={m_modification.StatusPriority}");
        }
        m_analysisStatus = m_modification.AnalysisStatus;
        m_statusPriority = m_modification.StatusPriority;
        if (ModelDAOHelper.DAOFactory.IsTransactionActive ()) {
          log.Fatal ("HandleStepTimeoutException: a transaction was still active when the analysis status was updated");
        }
      }
      catch (Lemoine.Threading.AbortException stepTimeoutStatusException) {
        ((Modification)m_modification).RestoreStatus (m_analysisStatus, m_statusPriority);
        log.Error ("HandleStepTimeoutException: " +
                   "exception while trying to record a timeout status",
                   stepTimeoutStatusException);
        m_exceptionStatus = ex;
        throw;
      }
      catch (OperationCanceledException stepTimeoutStatusException) {
        ((Modification)m_modification).RestoreStatus (m_analysisStatus, m_statusPriority);
        log.Error ("HandleStepTimeoutException: " +
                   "exception while trying to record a timeout status",
                   stepTimeoutStatusException);
        m_exceptionStatus = ex;
        throw;
      }
      catch (Exception stepTimeoutStatusException) {
        ((Modification)m_modification).RestoreStatus (m_analysisStatus, m_statusPriority);
        log.Error ("HandleStepTimeoutException: " +
                   "exception while trying to record a timeout status",
                   stepTimeoutStatusException);
        m_exceptionStatus = ex;
        return;
      }

      m_retry = true;
    }

    /// <summary>
    /// In case of Integrity Constraint Violation, flag the modification
    /// </summary>
    /// <param name="ex"></param>
    void HandleIntegrityConstraintViolation (Exception ex)
    {
      IDatabaseExceptionDetails databaseExceptionDetails;
      ExceptionTest.IsDatabaseException (ex, log, out databaseExceptionDetails);
      if (log.IsDebugEnabled) {
        log.Debug ($"HandleIntegrityConstraintViolation: database exception details is {databaseExceptionDetails}");
      }

      // Temporary solution
      // Consider integrity constraint violations on reasonslot as being temporary for:
      // 23514 check_violation (_posduration)
      // 23P01 exclusion_violation (_nooverlap)
      if ((databaseExceptionDetails.Code.Equals ("23514") || databaseExceptionDetails.Equals ("23P01"))
        && databaseExceptionDetails.BaseMessage.Contains ("reasonslot")) {
        log.Error ($"HandleIntegrityConstraintViolation: Analysis of {m_modification} failed with a constraint integrity violation, try to retry because on reasonslot table", ex);
        m_retry = true;
        Debug.Assert (!ModelDAOHelper.DAOFactory.IsTransactionActive ());
        if (ModelDAOHelper.DAOFactory.IsTransactionActive ()) {
          log.Fatal ("HandleIntegrityConstraintViolation: after an integrity constraint violation, the transaction is still active");
          SetExitRequested ();
          throw new Lemoine.Threading.AbortException ("Transaction active with inner IntegrityConstraintViolation raised in MakeAnalysis.", ex);
        }
        return;
      }

      log.Error ($"HandleIntegrityConstraintViolation: integrity constraint violation => try to set the modification status to ConstraintIntegrityViolation, details={databaseExceptionDetails}");

      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction (BuildTransactionName ("Analysis.Modification.HandleIntegrityConstraintViolation"),
                                                                       TransactionLevel.ReadCommitted)) {
          transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data

          m_modification.MarkAsConstraintIntegrityViolation ();
          ModelDAOHelper.DAOFactory.ModificationDAO.MakePersistent (m_modification);
          var message = "Constraint integrity violation: " + databaseExceptionDetails;
          ModelDAOHelper.DAOFactory.AnalysisLogDAO.Add (m_modification,
                                                        LogLevel.CRIT,
                                                        message);
          transaction.Commit ();
        }
        // Commit is ok, you can update the analysis status
        if (log.IsDebugEnabled) {
          log.Debug ($"HandleIntegrityConstraintViolation: after commit, new Status={m_modification.AnalysisStatus} statusPriority={m_modification.StatusPriority}");
        }
        m_analysisStatus = m_modification.AnalysisStatus;
        m_statusPriority = m_modification.StatusPriority;
        if (ModelDAOHelper.DAOFactory.IsTransactionActive ()) {
          log.Fatal ("HandleIntegrityConstraintViolation: a transaction was still active when the analysis status was updated");
        }
      }
      catch (Lemoine.Threading.AbortException integrityConstraintViolationStatusException) {
        ((Modification)m_modification).RestoreStatus (m_analysisStatus, m_statusPriority);
        log.Fatal ("HandleIntegrityConstraintViolation: exception while trying to record an integrity constraint violation status", integrityConstraintViolationStatusException);
        m_exceptionStatus = ex;
        throw;
      }
      catch (OperationCanceledException integrityConstraintViolationStatusException) {
        ((Modification)m_modification).RestoreStatus (m_analysisStatus, m_statusPriority);
        log.Fatal ("HandleIntegrityConstraintViolation: exception while trying to record an integrity constraint violation status", integrityConstraintViolationStatusException);
        m_exceptionStatus = ex;
        throw;
      }
      catch (Exception integrityConstraintViolationStatusException) {
        ((Modification)m_modification).RestoreStatus (m_analysisStatus, m_statusPriority);
        log.Fatal ("HandleIntegrityConstraintViolation: exception while trying to record an integrity constraint violation status", integrityConstraintViolationStatusException);
        m_exceptionStatus = ex;
        return;
      }
    }
    #endregion // Methods
  }
}
