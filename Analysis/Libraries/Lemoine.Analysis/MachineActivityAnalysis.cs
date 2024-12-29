// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lemoine.Core.ExceptionManagement;
using Lemoine.Extensions;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Threading;
using Lemoine.Core.Log;
using Lemoine.Core.Performance;
using Lemoine.Core.StateMachine;
using Lemoine.Extensions.Analysis.StateMachine;
using Lemoine.Extensions.Analysis;

namespace Lemoine.Analysis
{
  /// <summary>
  /// This class takes care of the activity analysis
  /// of a unique not monitored machine
  /// </summary>
  public class MachineActivityAnalysis
    : ProcessOrThreadClass, IProcessOrThreadClass, IChecked, IThreadStatusSupport
    , IMachineActivityAnalysis
    , IStateMachineAnalysis
  {
    /// <summary>
    /// Maximum number of time to spend in ManageMachineStateTemplates
    /// </summary>
    static readonly string MAX_TIME_IN_MACHINE_STATE_TEMPLATES_KEY = "Analysis.Activity.MachineStateTemplates.MaxTime";
    static readonly TimeSpan MAX_TIME_IN_MACHINE_STATE_TEMPLATES_DEFAULT = TimeSpan.FromSeconds (100); // Previously 1s

    /// <summary>
    /// Minimum time before checking the maximum time is reached in ManageMachineStateTemplates
    /// </summary>
    static readonly string MIN_TIME_IN_MACHINE_STATE_TEMPLATES_KEY = "Analysis.Activity.MachineStateTemplates.MinTime";
    static readonly TimeSpan MIN_TIME_IN_MACHINE_STATE_TEMPLATES_DEFAULT = TimeSpan.FromSeconds (20); // Previously 1s

    /// <summary>
    /// Maximum number of time to spend in MakeAnalysis
    /// </summary>
    static readonly string MAX_TIME_IN_ANALYSIS_KEY = "Analysis.Activity.MaxTime";
    static readonly TimeSpan MAX_TIME_IN_ANALYSIS_DEFAULT = TimeSpan.FromSeconds (40);

    /// <summary>
    /// Default maximum time to spend in the analysis of the pending modifications
    /// </summary>
    static readonly string MAX_TIME_PENDING_MODIFICATIONS_ANALYSIS_KEY =
      "Analysis.Activity.PendingModifications.MaxTime";
    static readonly TimeSpan MAX_TIME_PENDING_MODIFICATIONS_ANALYSIS_DEFAULT = TimeSpan.FromMinutes (2);

    /// <summary>
    /// Default minimum time before checking the maximum time is reached in the analysis of the pending modifications
    /// </summary>
    static readonly string MIN_TIME_PENDING_MODIFICATIONS_ANALYSIS_KEY =
      "Analysis.Activity.PendingModifications.MinTime";
    static readonly TimeSpan MIN_TIME_PENDING_MODIFICATIONS_ANALYSIS_DEFAULT = TimeSpan.FromSeconds (20);

    /// <summary>
    /// Default maximum time to spend in cleaning the flagged modifications
    /// </summary>
    static readonly string MAX_TIME_CLEAN_FLAGGED_MODIFICATIONS_KEY =
      "Analysis.Activity.CleanFlaggedModifications.MaxTime";
    static readonly TimeSpan MAX_TIME_CLEAN_FLAGGED_MODIFICATIONS_DEFAULT = TimeSpan.FromMinutes (10);

    /// <summary>
    /// Default minimum time before checking the maximum time is reached in cleaning the flagged modifications
    /// </summary>
    static readonly string MIN_TIME_CLEAN_FLAGGED_MODIFICATIONS_KEY =
      "Analysis.Activity.CleanFlaggedModifications.MinTime";
    static readonly TimeSpan MIN_TIME_CLEAN_FLAGGED_MODIFICATIONS_DEFAULT = TimeSpan.FromSeconds (0);

    /// <summary>
    /// Default maximum time for production analysis
    /// </summary>
    static readonly string MAX_TIME_PRODUCTION_ANALYSIS_KEY = "Analysis.Activity.Production.MaxTime";
    static readonly TimeSpan MAX_TIME_PRODUCTION_ANALYSIS_DEFAULT = TimeSpan.FromMinutes (2);

    /// <summary>
    /// Default minimum time before checking the maximum time for production analysis
    /// </summary>
    static readonly string MIN_TIME_PRODUCTION_ANALYSIS_KEY = "Analysis.Activity.Production.MinTime";
    static readonly TimeSpan MIN_TIME_PRODUCTION_ANALYSIS_DEFAULT = TimeSpan.FromSeconds (0);

    /// <summary>
    /// Default maximum time for operation slot split analysis
    /// </summary>
    static readonly string MAX_TIME_OPERATION_SLOT_SPLIT_ANALYSIS_KEY = "Analysis.Activity.OperationSlotSplit.MaxTime";
    static readonly TimeSpan MAX_TIME_OPERATION_SLOT_SPLIT_ANALYSIS_DEFAULT = TimeSpan.FromMinutes (2);

    /// <summary>
    /// Default minimum time before checking the maximum time for operation slot split analysis is reached
    /// </summary>
    static readonly string MIN_TIME_OPERATION_SLOT_SPLIT_ANALYSIS_KEY = "Analysis.Activity.OperationSlotSplit.MinTime";
    static readonly TimeSpan MIN_TIME_OPERATION_SLOT_SPLIT_ANALYSIS_DEFAULT = TimeSpan.FromSeconds (20);

    /// <summary>
    /// App setting key for the restricted transaction level
    /// </summary>
    static readonly string RESTRICTED_TRANSACTION_LEVEL_KEY = "Analysis.Activity.RestrictedTransactionLevel";
    static readonly string RESTRICTED_TRANSLATION_LEVEL_DEFAULT = ""; // Default one

    #region Members
    bool m_initialized = false;
    bool m_threadExecution = true;
    readonly IMachine m_machine;
    Int64 m_pause = 0;
    readonly Object m_lock = new Object ();
    readonly TransactionLevel m_restrictedTransactionLevel = TransactionLevel.Serializable;

    readonly ISingleAnalysis m_productionAnalysis;
    readonly ISingleAnalysis m_operationSlotSplitAnalysis;
    MachineStateTemplateAnalysis m_machineStateTemplateAnalysis = null; // Initialized in method Initialize
    PendingModificationAnalysis<Lemoine.Model.IMachineModification, Lemoine.GDBPersistentClasses.MachineModification> m_pendingModificationAnalysis = null; // Initialized in method Initialize

    IEnumerable<Lemoine.Extensions.Analysis.ISingleMachineAnalysisExtension> m_extensions = null;

    IStateMachine<IMachineActivityAnalysis> m_stateMachine;
    IList<Exception> m_stateExceptions;
    #endregion // Members

    ILog log = LogManager.GetLogger (typeof (MachineActivityAnalysis).FullName);

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
    /// Associated not monitored machine
    /// </summary>
    public IMachine Machine => m_machine;

    /// <summary>
    /// Reference to PendingModificationAnalysis
    /// </summary>
    protected PendingModificationAnalysis<Lemoine.Model.IMachineModification, Lemoine.GDBPersistentClasses.MachineModification> PendingModificationAnalysis => m_pendingModificationAnalysis;

    /// <summary>
    /// Restricted transaction level
    /// </summary>
    public TransactionLevel RestrictedTransactionLevel => m_restrictedTransactionLevel;

    /// <summary>
    /// Return the state machine
    /// </summary>
    /// <returns></returns>
    protected virtual IContext GetStateMachine () => m_stateMachine;

    /// <summary>
    /// <see cref="IContext"/>
    /// </summary>
    public DateTime StateMachineStartDateTime => GetStateMachine ()?.StateMachineStartDateTime ?? DateTime.UtcNow;

    /// <summary>
    /// <see cref="IContext"/>
    /// </summary>
    public virtual TimeSpan MaxTime => (TimeSpan)ConfigSet.LoadAndGet<TimeSpan> (MAX_TIME_IN_ANALYSIS_KEY, MAX_TIME_IN_ANALYSIS_DEFAULT);

    /// <summary>
    /// <see cref="IStateMachineAnalysis"/>
    /// </summary>
    public IEnumerable<Exception> StateExceptions => m_stateExceptions;
    #endregion // Getters / Setters

    #region IStateMachineAnalysis implementation
    /// <summary>
    /// <see cref="IStateMachineAnalysis"/>
    /// </summary>
    public string PerfSuffix => $".{m_machine.Id}";
    #endregion // IStateMachineAnalysis implementation

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected MachineActivityAnalysis ()
    {
      Debug.Assert (false);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    public MachineActivityAnalysis (IMachine machine)
    {
      Debug.Assert (null != machine);

      m_machine = machine;
      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                this.GetType ().FullName,
                                                machine.Id));

      m_productionAnalysis = new ProductionAnalysis (this);
      m_operationSlotSplitAnalysis = new OperationSlotSplitAnalysis (this);

      string restrictedTransactionLevelString = Lemoine.Info.ConfigSet
        .LoadAndGet (RESTRICTED_TRANSACTION_LEVEL_KEY, RESTRICTED_TRANSLATION_LEVEL_DEFAULT);
      if (!string.IsNullOrEmpty (restrictedTransactionLevelString)) {
        try {
          m_restrictedTransactionLevel = (Lemoine.ModelDAO.TransactionLevel)Enum
            .Parse (typeof (Lemoine.ModelDAO.TransactionLevel), restrictedTransactionLevelString);
          log.Info ($"MonitoredMachineActivityAnalysis: got the restricted transaction level {m_restrictedTransactionLevel} from config");
        }
        catch (OperationCanceledException) {
          throw;
        }
        catch (Lemoine.Threading.AbortException) {
          throw;
        }
        catch (Exception ex) {
          log.Error ($"MonitoredMachineActivityAnalysis: parsing the configuration {RESTRICTED_TRANSACTION_LEVEL_KEY}={restrictedTransactionLevelString} failed", ex);
        }
      }
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Check if an exception requires to exit
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public bool IsExitRequired (Exception ex)
    {
      var exitRequired = ExceptionTest.RequiresExit (ex, log);
      if (exitRequired) {
        log.Error ("IsExitRequired: exception requires to exit, give up", ex);
      }
      return exitRequired;
    }

    /// <summary>
    /// Request a pause in the activity analysis
    /// </summary>
    /// <param name="modificationId">ID of the modification that requests the pause</param>
    /// <returns>The pause could be set</returns>
    public bool RequestPause (long modificationId)
    {
      long initialValue = Interlocked.CompareExchange (ref m_pause, modificationId, (long)0);
      if (0 == initialValue) {
        if (log.IsInfoEnabled) {
          log.InfoFormat ("RequestPause: " +
                          "the pause has been correctly set to modificationId {0}",
                          modificationId);
        }
        return true;
      }
      else if (modificationId == initialValue) {
        if (log.IsInfoEnabled) {
          log.InfoFormat ("RequestPause: " +
                          "the pause is already hold by modificationId {0}",
                          modificationId);
        }
        return true;
      }
      else {
        if (log.IsInfoEnabled) {
          log.InfoFormat ("RequestPause: " +
                          "the pause is already set by modificationId {0}, " +
                          "exit without updating m_pause",
                          initialValue);
        }
        return false;
      }
    }

    /// <summary>
    /// Release a pause in the activity analysis
    /// </summary>
    /// <param name="modificationId">ID</param>
    public void ReleasePause (long modificationId)
    {
      if (modificationId == Interlocked.CompareExchange (ref m_pause, 0, modificationId)) {
        if (log.IsInfoEnabled) {
          log.InfoFormat ("ReleasePause: " +
                          "the pause has been released for modificationId {0}",
                          modificationId);
        }
      }
      else {
        log.ErrorFormat ("ReleasePause: " +
                         "trying to release a pause with the wrong modificationId {0} " +
                         "=> this should not happen",
                         modificationId);
        throw new InvalidOperationException ("ReleasePause");
      }
    }

    /// <summary>
    /// Check if the activity analysis is already in pause
    /// </summary>
    /// <param name="modificationId">ID</param>
    /// <returns></returns>
    public bool IsInPause (long modificationId)
    {
      return modificationId == Interlocked.CompareExchange (ref m_pause, modificationId, modificationId);
    }

    /// <summary>
    /// Check if a pause is requested for the activity analysis
    /// </summary>
    /// <returns></returns>
    public bool IsPauseRequested ()
    {
      long pauseValue = Interlocked.CompareExchange (ref m_pause, 0, 0);
      if (0 != pauseValue) {
        log.InfoFormat ("IsPauseRequested: " +
                        "the value is {0} " +
                        "=> a pause was requested",
                        pauseValue);
        return true;
      }
      else {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("IsPauseRequested: " +
                           "pause value was {0}. " +
                           "No pause was requested",
                           pauseValue);
        }
        return false;
      }
    }

    /// <summary>
    /// Get the Id of the modification that triggered the pause
    /// </summary>
    /// <returns></returns>
    public Int64 GetPauseTriggeringModificationId ()
    {
      long pauseValue = Interlocked.CompareExchange (ref m_pause, 0, 0);
      return pauseValue;
    }

    /// <summary>
    /// Initialize
    /// </summary>
    public virtual bool Initialize ()
    {
      Debug.Assert (null != m_machine);

      if (!m_initialized) {
        log.Debug ("Initialize /B");

        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          m_productionAnalysis.Initialize ();

          SetActive ();
          if (IsPauseRequested ()) {
            if (log.IsInfoEnabled) {
              log.Info ($"Initialize: interrupt the initialization after getting the production analysis status because the modification analysis Id {m_pause} is run on the same monitored machine");
            }
            return false;
          }

          m_operationSlotSplitAnalysis.Initialize ();

          SetActive ();
          if (IsPauseRequested ()) {
            if (log.IsInfoEnabled) {
              log.Info ($"Initialize: interrupt the initialization after initializing the operation split analysis because the modification analysis Id {m_pause} is run on the same monitored machine");
            }
            return false;
          }
        }

        log.Debug ("Initialize: initialize MachineStateTemplateAnalysis");
        m_machineStateTemplateAnalysis =
          new MachineStateTemplateAnalysis (m_machine, m_restrictedTransactionLevel, this);

        InitializePendingModificationAnalysis ();

        log.Debug ("Initialize /E");
        m_initialized = true;
      }

      return true;
    }

    void InitializePendingModificationAnalysis ()
    {
      log.Debug ("InitializePendingModificationAnalysis");
      if (null == m_pendingModificationAnalysis) {
        m_pendingModificationAnalysis = new PendingMachineModificationAnalysis (m_machine);
        m_pendingModificationAnalysis.ThreadExecution = m_threadExecution;
        m_pendingModificationAnalysis.CheckedParent = this;
      }
    }

    /// <summary>
    /// Fill analysis status of the pending machine modifications.
    /// 
    /// Not to be run in a parent transaction.
    /// 
    /// To be run by GlobalAnalysis
    /// </summary>
    public virtual void FillAnalysisStatus (CancellationToken cancellationToken)
    {
      InitializePendingModificationAnalysis ();

      m_pendingModificationAnalysis.FillAnalysisStatus (cancellationToken, long.MaxValue);
    }

    /// <summary>
    /// <see cref="IStateMachineAnalysis"/>
    /// </summary>
    /// <param name="ex"></param>
    public void AddStateException (Exception ex)
    {
      m_stateExceptions.Add (ex);
    }

    /// <summary>
    /// Initialize some properties before a new analysis
    /// </summary>
    protected virtual void InitializeNewAnalysis ()
    {
      m_stateExceptions = new List<Exception> ();
    }

    /// <summary>
    /// Load the state machine
    /// 
    /// To be overridden by a super class
    /// </summary>
    /// <returns></returns>
    protected virtual bool LoadStateMachine ()
    {
      if (null == m_stateMachine) {
        var request = new Lemoine.Business.Extension
          .MachineExtensions<IMachineActivityAnalysisStateMachineExtension> (m_machine, (ext, m) => ext.Initialize (this));
        var firstMatchingExtension = Lemoine.Business.ServiceProvider
          .Get (request)
          .OrderByDescending (ext => ext.Priority)
          .FirstOrDefault ();
        if (null == firstMatchingExtension) {
          GetLogger ().Error ("LoadStateMachine: no extension IMachineActivityAnalysisStateMachineExtension");
          return false;
        }
        m_stateMachine = new Lemoine.Analysis.StateMachine.AnalysisStateMachine<IMachineActivityAnalysis, MachineActivityAnalysis> (firstMatchingExtension.InitialState, this);
      }

      return true;
    }

    /// <summary>
    /// Make the analysis without checking the thread concurrency
    /// 
    /// Use the machine state
    /// </summary>
    public virtual void MakeAnalysis (CancellationToken cancellationToken)
    {
      Debug.Assert (null != m_machine);

      if (!LoadStateMachine ()) {
        GetLogger ().Error ("MakeAnalysis: no state machine, do nothing");
        return;
      }
      Debug.Assert (null != GetStateMachine ());

      InitializeNewAnalysis ();

      RunStateMachine (cancellationToken, true);
    }

    /// <summary>
    /// Run the state machine
    /// 
    /// Suppose the internal properties are already correctly initialized
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="throwStateException">throw an exception if one of the state returned an exception, else return true</param>
    /// <returns>if true is returned, any other process can continue (kind of completed)</returns>
    protected virtual bool RunStateMachine (CancellationToken cancellationToken, bool throwStateException)
    {
      var result = m_stateMachine.Run (cancellationToken);
      if (throwStateException && this.StateExceptions.Any ()) {
        log.Error ($"RunStateMachine: {this.StateExceptions.Count ()} exceptions in state machine execution => throw an exception");
        throw new Exception ("Exception in MakeAnalysis (inner is first)", this.StateExceptions.First ());
      }
      return result;
    }

    /// <summary>
    /// <see cref="IMachineActivityAnalysis"/>
    /// </summary>
    /// <returns></returns>
    public bool ManageMachineStateTemplates (CancellationToken cancellationToken)
    {
      TimeSpan maxTimeInMachineStateTemplates = ConfigSet.LoadAndGet<TimeSpan> (MAX_TIME_IN_MACHINE_STATE_TEMPLATES_KEY,
                                                                                MAX_TIME_IN_MACHINE_STATE_TEMPLATES_DEFAULT);
      TimeSpan minTimeInMachineStateTemplates = ConfigSet.LoadAndGet<TimeSpan> (MIN_TIME_IN_MACHINE_STATE_TEMPLATES_KEY,
                                                                                MIN_TIME_IN_MACHINE_STATE_TEMPLATES_DEFAULT);
      return ManageMachineStateTemplates (cancellationToken, maxTimeInMachineStateTemplates, minTimeInMachineStateTemplates);
    }

    /// <summary>
    /// <see cref="IMachineActivityAnalysis"/>
    /// </summary>
    /// <returns></returns>
    public bool ManageMachineStateTemplates (CancellationToken cancellationToken, TimeSpan maxTime, TimeSpan minTime)
    {
      Debug.Assert (null != m_machineStateTemplateAnalysis);
      if (null == m_machineStateTemplateAnalysis) {
        log.Fatal ("ManageMachineStateTemplates: " +
                   "m_machineStateTemplateAnalysis was not initialized " +
                   "although it should have been initialized by the Initialize method " +
                   "=> initialize it again");
        m_machineStateTemplateAnalysis = new MachineStateTemplateAnalysis (m_machine,
                                                                           m_restrictedTransactionLevel,
                                                                           this);
      }

      // 7 days from now
      DateTime until = DateTime.UtcNow.AddDays (7).Date; // Full day, to process a new day only once a day
      var present = GetPresent ();
      if (log.IsDebugEnabled) {
        log.Debug ($"ManageMachineStateTemplates: run the analysis until {until}, present={present}");
      }
      var maxMachineStateTemplateAnalysisDateTime = GetMaxAnalysisDateTime (maxTime, minTime);
      m_machineStateTemplateAnalysis.Run (cancellationToken,
        present,
        until,
        maxMachineStateTemplateAnalysisDateTime);
      return true;
    }

    /// <summary>
    /// Get the present UTC date/time
    /// </summary>
    /// <returns></returns>
    protected virtual DateTime GetPresent ()
    {
      return DateTime.UtcNow;
    }

    /// <summary>
    /// Return the maximum date/time for a specific analysis
    /// </summary>
    /// <param name="localMaxTime"></param>
    /// <param name="localMinTime">minimum time before checking the maximum global time</param>
    /// <returns></returns>
    protected virtual DateTime GetMaxAnalysisDateTime (TimeSpan localMaxTime, TimeSpan localMinTime)
    {
      if (localMaxTime < localMinTime) {
        if (log.IsDebugEnabled) {
          log.Info ($"GetMaxAnalysisDateTime: max={localMaxTime} < min={localMinTime} {System.Environment.StackTrace}");
        }
        else if (log.IsInfoEnabled) {
          log.Info ($"GetMaxAnalysisDateTime: max={localMaxTime} < min={localMinTime}");
        }
      }

      var maxAnalysisDateTime = this.StateMachineStartDateTime
        .Add (this.MaxTime);
      var maxDateTime = DateTime.UtcNow.Add (localMaxTime);
      var minDateTime = DateTime.UtcNow.Add (localMinTime);
      // Return the max of the three times
      if (maxAnalysisDateTime < maxDateTime) {
        if (minDateTime < maxAnalysisDateTime) {
          return maxAnalysisDateTime;
        }
        else {
          return minDateTime;
        }
      }
      else {
        return maxDateTime;
      }
    }

    /// <summary>
    /// Return the minimum date/time before checking the maximum date/time is reached
    /// for a specific analysis
    /// </summary>
    /// <param name="localMinTime"></param>
    /// <returns></returns>
    protected virtual DateTime GetMinAnalysisDateTime (TimeSpan localMinTime)
    {
      var minAnalysisDateTime = DateTime.UtcNow
        .Add (localMinTime);
      return minAnalysisDateTime;
    }

    /// <summary>
    /// <see cref="IMachineActivityAnalysis"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="minPastPriority"></param>
    /// <param name="minPresentPriority"></param>
    /// <returns></returns>
    public bool RunPendingModificationsAnalysis (CancellationToken cancellationToken, int minPastPriority, int minPresentPriority)
    {
      var maxTime = Lemoine.Info.ConfigSet
        .LoadAndGet (MAX_TIME_PENDING_MODIFICATIONS_ANALYSIS_KEY,
          MAX_TIME_PENDING_MODIFICATIONS_ANALYSIS_DEFAULT);
      var minTime = Lemoine.Info.ConfigSet
        .LoadAndGet (MIN_TIME_PENDING_MODIFICATIONS_ANALYSIS_KEY,
          MIN_TIME_PENDING_MODIFICATIONS_ANALYSIS_DEFAULT);
      return RunPendingModificationsAnalysis (cancellationToken, maxTime, minTime, minPastPriority, minPresentPriority);
    }

    /// <summary>
    /// <see cref="IMachineActivityAnalysis"/>
    /// </summary>
    public bool RunPendingModificationsAnalysis (CancellationToken cancellationToken, TimeSpan maxTime, TimeSpan minTime, int minPastPriority, int minPresentPriority)
    {
      var maxAnalysisDateTime = GetMaxAnalysisDateTime (maxTime, minTime);
      m_pendingModificationAnalysis.RunOnce (cancellationToken, maxAnalysisDateTime, minPastPriority, minPresentPriority);
      return true;
    }

    /// <summary>
    /// <see cref="IGlobalAnalysis"/>
    /// </summary>
    /// <returns></returns>
    public bool IsCleanFlaggedModificationsRequired ()
    {
      Debug.Assert (null != m_pendingModificationAnalysis);
      return m_pendingModificationAnalysis.IsCleanRequired ();
    }

    /// <summary>
    /// <see cref="IGlobalAnalysis"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public bool CleanFlaggedModifications (CancellationToken cancellationToken)
    {
      var maxTime = Lemoine.Info.ConfigSet
  .LoadAndGet (MAX_TIME_CLEAN_FLAGGED_MODIFICATIONS_KEY,
    MAX_TIME_CLEAN_FLAGGED_MODIFICATIONS_DEFAULT);
      var minTime = Lemoine.Info.ConfigSet
  .LoadAndGet (MIN_TIME_CLEAN_FLAGGED_MODIFICATIONS_KEY,
    MIN_TIME_CLEAN_FLAGGED_MODIFICATIONS_DEFAULT);
      return CleanFlaggedModifications (cancellationToken, maxTime, minTime);
    }

    /// <summary>
    /// <see cref="IGlobalAnalysis"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="minTime"></param>
    /// <param name="maxTime"></param>
    /// <returns></returns>
    public bool CleanFlaggedModifications (CancellationToken cancellationToken, TimeSpan maxTime, TimeSpan minTime)
    {
      Debug.Assert (null != m_pendingModificationAnalysis);
      var maxAnalysisDateTime = GetMaxAnalysisDateTime (maxTime, minTime);
      m_pendingModificationAnalysis.CleanFlaggedModifications (cancellationToken, maxAnalysisDateTime);
      return true;
    }

    /// <summary>
    /// <see cref="IMachineActivityAnalysis"/>
    /// </summary>
    /// <returns></returns>
    public bool RunProductionAnalysis (CancellationToken cancellationToken)
    {
      var maxTimeProductionAnalysis = Lemoine.Info.ConfigSet
        .LoadAndGet (MAX_TIME_PRODUCTION_ANALYSIS_KEY,
        MAX_TIME_PRODUCTION_ANALYSIS_DEFAULT);
      var minTimeProductionAnalysis = Lemoine.Info.ConfigSet
        .LoadAndGet (MIN_TIME_PRODUCTION_ANALYSIS_KEY,
        MIN_TIME_PRODUCTION_ANALYSIS_DEFAULT);
      return RunProductionAnalysis (cancellationToken, maxTimeProductionAnalysis, minTimeProductionAnalysis);
    }

    /// <summary>
    /// <see cref="IMachineActivityAnalysis"/>
    /// </summary>
    public bool RunProductionAnalysis (CancellationToken cancellationToken, TimeSpan maxTime, TimeSpan minTime)
    {
      var maxDateTime = GetMaxAnalysisDateTime (maxTime, minTime);
      return m_productionAnalysis.RunOnce (cancellationToken, maxDateTime, minTime);
    }

    /// <summary>
    /// <see cref="IMachineActivityAnalysis"/>
    /// </summary>
    /// <returns></returns>
    public bool RunOperationSlotSplitAnalysis (CancellationToken cancellationToken)
    {
      var maxTimeOperationSlotSplitAnalysis = Lemoine.Info.ConfigSet
        .LoadAndGet (MAX_TIME_OPERATION_SLOT_SPLIT_ANALYSIS_KEY,
        MAX_TIME_OPERATION_SLOT_SPLIT_ANALYSIS_DEFAULT);
      var minTimeOperationSlotSplitAnalysis = Lemoine.Info.ConfigSet
        .LoadAndGet (MIN_TIME_OPERATION_SLOT_SPLIT_ANALYSIS_KEY,
        MIN_TIME_OPERATION_SLOT_SPLIT_ANALYSIS_DEFAULT);
      return RunOperationSlotSplitAnalysis (cancellationToken, maxTimeOperationSlotSplitAnalysis, minTimeOperationSlotSplitAnalysis);
    }

    /// <summary>
    /// <see cref="IMachineActivityAnalysis"/>
    /// </summary>
    public bool RunOperationSlotSplitAnalysis (CancellationToken cancellationToken, TimeSpan maxTime, TimeSpan minTime, TimeSpan? period = null)
    {
      var maxDateTime = GetMaxAnalysisDateTime (maxTime, minTime);
      return m_operationSlotSplitAnalysis.RunOnce (cancellationToken, maxDateTime, minTime, period.HasValue ? (int?)period.Value.TotalSeconds : (int?)null);
    }

    public IEnumerable<IState<T>> GetExtensionAnalysisStates<T> ()
      where T : IMachineActivityAnalysis, IContext<T>
    {
      if (null == m_extensions) {
        m_extensions = Lemoine.Extensions.ExtensionManager
          .GetExtensions<Lemoine.Extensions.Analysis.ISingleMachineAnalysisExtension> (checkedThread: this)
          .Where (extension => extension.Initialize (m_machine))
          .ToList (); // ToList is mandatory else the result of the Linq command is not cached
      }
      Debug.Assert (null != m_extensions);
      return m_extensions
        .Select (ext => new SingleMachineAnalysisExtensionState<T> (ext));
    }
    #endregion // Methods

    #region Implementation of IContext
    /// <summary>
    /// <see cref="IContext"/>
    /// </summary>
    public virtual void SwitchToEndState ()
    {
      GetStateMachine ().SwitchToEndState ();
    }

    /// <summary>
    /// <see cref="IContext{TContext}"/>
    /// </summary>
    /// <param name="state"></param>
    public virtual void SwitchTo (IState<IMachineActivityAnalysis> state)
    {
      m_stateMachine.SwitchTo (state);
    }
    #endregion // Implementation of IContext

    #region Implementation of ProcessClass
    /// <summary>
    /// <see cref="ProcessClass">ProcessClass</see> implementation
    /// </summary>
    /// <returns></returns>
    public override string GetStampFileName ()
    {
      return "ActivityAnalysisStamp-" + m_machine.Id;
    }

    /// <summary>
    /// <see cref="ProcessClass">ProcessClass</see> implementation
    /// </summary>
    /// <param name="cancellationToken"></param>
    protected override void Run (CancellationToken cancellationToken)
    {
      MakeAnalysis (cancellationToken);
    }

    /// <summary>
    /// <see cref="ProcessClass">ProcessClass</see> implementation
    /// </summary>
    /// <returns></returns>
    public override ILog GetLogger ()
    {
      return log;
    }
    #endregion // Implementation of ProcessClass
  }
}
