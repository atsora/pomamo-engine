// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.ExceptionManagement;
using Lemoine.Info;
using Lemoine.ModelDAO;
using Lemoine.Threading;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
using System.Collections.Generic;
using Lemoine.Core.Performance;
using System.Threading;
using System.Linq;
using Lemoine.Extensions.Analysis.StateMachine;
using Lemoine.Core.StateMachine;

namespace Lemoine.Analysis
{
  /// <summary>
  /// Global analysis thread, that does not depend on machines
  /// </summary>
  public class GlobalAnalysis
    : ProcessOrThreadClass, IProcessOrThreadClass, IThreadStatusSupport
    , IGlobalAnalysis
    , IStateMachineAnalysis
  {
    /// <summary>
    /// App setting key for the restricted transaction level
    /// </summary>
    const string RESTRICTED_TRANSACTION_LEVEL_KEY = "Analysis.Global.RestrictedTransactionLevel";
    static readonly string RESTRICTED_TRANSLATION_LEVEL_DEFAULT = ""; // Default one

    /// <summary>
    /// Maximum number of time to spend in MakeAnalysis
    /// </summary>
    const string MAX_TIME_IN_ANALYSIS_KEY = "Analysis.Global.MaxTime";
    static readonly TimeSpan MAX_TIME_IN_ANALYSIS_DEFAULT = TimeSpan.FromMinutes (2);

    /// <summary>
    /// Maximum number of time to spend in ManageDayTemplates: key
    /// </summary>
    const string MAX_TIME_IN_DAY_TEMPLATES_KEY = "Analysis.Global.DayTemplate.MaxTime";
    /// <summary>
    /// Maximum number of time to spend in ManageDayTemplates: default value
    /// </summary>
    static readonly TimeSpan MAX_TIME_IN_DAY_TEMPLATES_DEFAULT = TimeSpan.FromSeconds (10);

    /// <summary>
    /// Maximum number of time to spend in ManageShiftTemplates: key
    /// </summary>
    const string MAX_TIME_IN_SHIFT_TEMPLATES_KEY = "Analysis.Global.ShiftTemplate.MaxTime";
    /// <summary>
    /// Maximum number of time to spend in ManageShiftTemplates: default value
    /// </summary>
    static readonly TimeSpan MAX_TIME_IN_SHIFT_TEMPLATES_DEFAULT = TimeSpan.FromSeconds (10);

    /// <summary>
    /// If the stored min activity analysis date/time is older than this age, refresh it: key
    /// </summary>
    const string CACHE_TIME_MIN_ACTIVITY_ANALYSIS_DATE_TIME_KEY = "Analysis.Global.CacheTimeMinActivityAnalysis";
    /// <summary>
    /// If the stored min activity analysis date/time is older than this age, refresh it: default value
    /// </summary>
    static readonly TimeSpan CACHE_TIME_MIN_ACTIVITY_ANALYSIS_DATE_TIME_DEFAULT = TimeSpan.FromMinutes (20);

    /// <summary>
    /// Default maximum time to spend in the analysis of the pending modifications
    /// </summary>
    const string MAX_TIME_PENDING_MODIFICATIONS_ANALYSIS_KEY =
      "Analysis.Global.PendingModifications.MaxTime";
    static readonly TimeSpan MAX_TIME_PENDING_MODIFICATIONS_ANALYSIS_DEFAULT = TimeSpan.FromMinutes (10);

    /// <summary>
    /// Default maximum time to spend in cleaning the flagged modifications
    /// </summary>
    const string MAX_TIME_CLEAN_FLAGGED_MODIFICATIONS_KEY =
      "Analysis.Global.CleanFlaggedModifications.MaxTime";
    static readonly TimeSpan MAX_TIME_CLEAN_FLAGGED_MODIFICATIONS_DEFAULT = TimeSpan.FromMinutes (10);

    #region Members
    readonly IActivityAnalysis m_activityAnalysis;
    bool m_initialized = false;
    readonly Lemoine.ModelDAO.TransactionLevel m_restrictedTransactionLevel = Lemoine.ModelDAO.TransactionLevel.Serializable;
    DayTemplateAnalysis m_dayTemplateAnalysis = null; // Initialized in method Initialize
    ShiftTemplateAnalysis m_shiftTemplateAnalysis = null; // Initialized in method Initialize
    PendingModificationAnalysis<Lemoine.Model.IGlobalModification, Lemoine.GDBPersistentClasses.GlobalModification> m_pendingModificationAnalysis = null; // Initialized in method Initialize
    DateTime? m_minActivityAnalysisDateTime;

    volatile bool m_firstRun = true;
    bool m_weekNumbersCompleted = false;
    DateTime? m_present = null;
    readonly SemaphoreSlim m_presentSemaphore = new SemaphoreSlim (1, 1);
    IStateMachine<IGlobalAnalysis> m_stateMachine;
    IList<Exception> m_stateExceptions;
    #endregion // Members

    readonly ILog log = LogManager.GetLogger (typeof (GlobalAnalysis).FullName);

    #region Getters / Setters
    /// <summary>
    /// <see cref="IGlobalAnalysis"/>
    /// </summary>
    public bool FirstRun
    {
      get { return m_firstRun; }
      set { m_firstRun = value; }
    }

    /// <summary>
    /// Return the state machine
    /// </summary>
    /// <returns></returns>
    protected virtual IContext GetStateMachine ()
    {
      return m_stateMachine;
    }

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

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="activityAnalysis">Can be null</param>
    public GlobalAnalysis (IActivityAnalysis activityAnalysis)
    {
      m_activityAnalysis = activityAnalysis;

      string restrictedTransactionLevelString = Lemoine.Info.ConfigSet
        .LoadAndGet (RESTRICTED_TRANSACTION_LEVEL_KEY, RESTRICTED_TRANSLATION_LEVEL_DEFAULT);
      if (!string.IsNullOrEmpty (restrictedTransactionLevelString)) {
        try {
          m_restrictedTransactionLevel = (Lemoine.ModelDAO.TransactionLevel)Enum
            .Parse (typeof (Lemoine.ModelDAO.TransactionLevel), restrictedTransactionLevelString);
          log.Info ($"GlobalAnalysis: got the restricted transaction level {m_restrictedTransactionLevel} from config");
        }
        catch (OperationCanceledException) {
          throw;
        }
        catch (Lemoine.Threading.AbortException) {
          throw;
        }
        catch (Exception ex) {
          log.Error ($"GlobalAnalysis: parsing the configuration {RESTRICTED_TRANSACTION_LEVEL_KEY}={restrictedTransactionLevelString} failed", ex);
        }
      }

      ConfigSet.Load (MAX_TIME_IN_DAY_TEMPLATES_KEY,
                      MAX_TIME_IN_DAY_TEMPLATES_DEFAULT);
      ConfigSet.Load (MAX_TIME_IN_SHIFT_TEMPLATES_KEY,
                      MAX_TIME_IN_SHIFT_TEMPLATES_DEFAULT);
      ConfigSet.Load (CACHE_TIME_MIN_ACTIVITY_ANALYSIS_DATE_TIME_KEY,
                      CACHE_TIME_MIN_ACTIVITY_ANALYSIS_DATE_TIME_DEFAULT);

      PauseCheck (); // Inactive until explicitely activated
    }
    #endregion // Constructors

    #region IProcessOrThreadClass implementation
    /// <summary>
    /// <see cref="IProcessOrThreadClass">ProcessClass</see> implementation
    /// </summary>
    /// <returns></returns>
    public override ILog GetLogger ()
    {
      return log;
    }

    /// <summary>
    /// <see cref="IProcessOrThreadClass">ProcessClass</see> implementation
    /// </summary>
    protected override void Run (CancellationToken cancellationToken)
    {
      MakeAnalysis (cancellationToken);
    }

    /// <summary>
    /// <see cref="IProcessOrThreadClass">IProcessOrThreadClass</see> implementation
    /// </summary>
    /// <returns></returns>
    public override string GetStampFileName ()
    {
      return "GlobalAnalysisStamp";
    }
    #endregion // IProcessOrThreadClass implementation

    #region IStateMachineAnalysis implementation
    /// <summary>
    /// <see cref="IStateMachineAnalysis"/>
    /// </summary>
    public string PerfSuffix => "";

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
    /// Check if a pause is requested for the activity analysis
    /// </summary>
    /// <returns></returns>
    public bool IsPauseRequested ()
    {
      return false;
    }

    /// <summary>
    /// Get the Id of the modification that triggered the pause
    /// </summary>
    /// <returns></returns>
    public Int64 GetPauseTriggeringModificationId ()
    {
      return 0;
    }
    #endregion // IStateMachineAnalysis implementation

    /// <summary>
    /// <see cref="IGlobalAnalysis"/>
    /// </summary>
    /// <returns></returns>
    public bool Initialize ()
    {
      if (!m_initialized) {
        log.Debug ("Initialize: initialize DayTemplateAnalysis");
        m_dayTemplateAnalysis =
          new DayTemplateAnalysis (m_restrictedTransactionLevel, this);
        log.Debug ("Initialize: initialize ShiftTemplateAnalysis");
        m_shiftTemplateAnalysis =
          new ShiftTemplateAnalysis (m_restrictedTransactionLevel, this);
        log.Debug ("Initialize: initialize PendingModificationAnalysis");
        m_pendingModificationAnalysis =
          new PendingGlobalModificationAnalysis (m_activityAnalysis) {
            CheckedParent = this
          };

        log.Debug ("Initialize /E");
        m_initialized = true;
      }

      return true;
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
          .GlobalExtensions<IGlobalAnalysisStateMachineExtension> (ext => ext.Initialize (this));
        var firstMatchingExtension = Lemoine.Business.ServiceProvider
          .Get (request)
          .OrderByDescending (ext => ext.Priority)
          .FirstOrDefault ();
        if (null == firstMatchingExtension) {
          GetLogger ().Error ("LoadStateMachine: no valid extension IGlobalAnalysisStateMachineExtension");
          return false;
        }
        m_stateMachine = new Lemoine.Analysis.StateMachine.AnalysisStateMachine<IGlobalAnalysis, GlobalAnalysis> (firstMatchingExtension.InitialState, this);
      }

      return true;
    }

    /// <summary>
    /// Make the analysis without checking the thread concurrency
    /// 
    /// Use the machine state
    /// 
    /// <see cref="IGlobalAnalysis"/>
    /// </summary>
    public virtual void MakeAnalysis (CancellationToken cancellationToken)
    {
      if (!LoadStateMachine ()) {
        GetLogger ().Error ("MakeAnalysis: no state machine, do nothing");
        return;
      }
      Debug.Assert (null != GetStateMachine ());

      InitializeNewAnalysis ();

      using (var perfTracker = new PerfTracker ("Analysis.GlobalAnalysis")) {
        RunStateMachine (cancellationToken, true);
      }
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
    /// <see cref="IGlobalAnalysis"/>
    /// </summary>
    /// <returns></returns>
    public bool ResetPresent ()
    {
      using (var semaphoreHolder = Lemoine.Threading.SemaphoreSlimHolder.Create (m_presentSemaphore)) {
        m_present = null;
      }
      return true;
    }

    DateTime GetPresent ()
    {
      var present = m_present;
      if (present.HasValue) {
        return present.Value;
      }

      using (var semaphoreHolder = Lemoine.Threading.SemaphoreSlimHolder.Create (m_presentSemaphore)) {
        TimeSpan cacheTimeMinActivityAnalysisDateTime = ConfigSet
          .LoadAndGet<TimeSpan> (CACHE_TIME_MIN_ACTIVITY_ANALYSIS_DATE_TIME_KEY,
                                 CACHE_TIME_MIN_ACTIVITY_ANALYSIS_DATE_TIME_DEFAULT);
        if (!m_minActivityAnalysisDateTime.HasValue
            || (m_minActivityAnalysisDateTime.Value.Add (cacheTimeMinActivityAnalysisDateTime) < DateTime.UtcNow)) {
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            // For performance reasons, 
            // it does not need to be done in a unique serializable transaction,
            // the data for each machine can be done one at a time
            // This may allow to save some locks
            // So this method should not be called in a transaction (for performance reason)
            var monitoredMachines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
              .FindAll ();
            IList<DateTime> activityAnalysisDateTimes = new List<DateTime> ();
            foreach (var monitoredMachine in monitoredMachines) {
              var monitoredMachineAnalysisStatus = ModelDAOHelper.DAOFactory
                .MonitoredMachineAnalysisStatusDAO.FindById (monitoredMachine.Id);
              if (null == monitoredMachineAnalysisStatus) {
                if (log.IsWarnEnabled) {
                  log.Warn ($"GetPresent: no MonitoredMachineAnalysisStatus data for machine id {monitoredMachine.Id}");
                }
              }
              else { // null != monitoredMachineAnalysisStatus
                activityAnalysisDateTimes.Add (monitoredMachineAnalysisStatus.ActivityAnalysisDateTime);
              }
            }
            if (activityAnalysisDateTimes.Any ()) {
              m_minActivityAnalysisDateTime = activityAnalysisDateTimes
                .Min ();
            }
            else { // !activityAnalysisDateTimes.Any
              m_minActivityAnalysisDateTime = null;
            }
          }
        }
        if (m_minActivityAnalysisDateTime.HasValue) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetPresent: min activity analysis date/time is {m_minActivityAnalysisDateTime}");
          }
        }
        else {
          log.Warn ("GetPresent: no monitoredMachineAnalysisStatus data, consider null");
        }
        SetActive ();

        m_present = m_minActivityAnalysisDateTime ?? DateTime.UtcNow;
        return m_present.Value;
      } // lock
    }

    /// <summary>
    /// <see cref="IGlobalAnalysis"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public bool ManageDayTemplates (CancellationToken cancellationToken)
    {
      TimeSpan maxProcessTime;
      if (this.FirstRun) { // No maximum process time
        maxProcessTime = TimeSpan.FromHours (1); // Note: TimeSpan.MaxValue causes some problems and a System.ArgumentOutOfRangeException
      }
      else {
        maxProcessTime = ConfigSet.LoadAndGet<TimeSpan> (MAX_TIME_IN_DAY_TEMPLATES_KEY,
                                                         MAX_TIME_IN_DAY_TEMPLATES_DEFAULT);
      }
      return ManageDayTemplates (cancellationToken, maxProcessTime);
    }

    /// <summary>
    /// <see cref="IGlobalAnalysis"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxTime"></param>
    /// <returns></returns>
    public bool ManageDayTemplates (CancellationToken cancellationToken, TimeSpan maxTime)
    {
      Debug.Assert (null != m_dayTemplateAnalysis);
      if (null == m_dayTemplateAnalysis) {
        log.FatalFormat ("ManageDayTemplates: " +
                         "m_dayTemplateAnalysis was not initialized " +
                         "although it should have been initialized by the Initialize method " +
                         "=> initialize it again");
        m_dayTemplateAnalysis = new DayTemplateAnalysis (m_restrictedTransactionLevel,
                                                         this);
      }

      TimeSpan untilFromNow = AnalysisConfigHelper.MaxDaySlotProcess;
      DateTime until = DateTime.UtcNow.Add (untilFromNow).Date; // Full day, to process a new day only once a day
      log.DebugFormat ("ManageDayTemplates: " +
                       "run the analysis until {0}",
                       until);
      var present = GetPresent ();
      var maxAnalysisDateTime = GetMaxAnalysisDateTime (maxTime);
      m_dayTemplateAnalysis.Run (cancellationToken, present,
                                 until,
                                 maxAnalysisDateTime);
      return true;
    }

    /// <summary>
    /// <see cref="IGlobalAnalysis"/>
    /// </summary>
    /// <returns></returns>
    public bool ManageShiftTemplates (CancellationToken cancellationToken)
    {
      TimeSpan maxProcessTime;
      if (this.FirstRun) {
        maxProcessTime = TimeSpan.FromHours (1); // Note: TimeSpan.MaxValue causes some problems and a System.ArgumentOutOfRangeException
      }
      else {
        maxProcessTime = ConfigSet.LoadAndGet<TimeSpan> (MAX_TIME_IN_SHIFT_TEMPLATES_KEY,
                                                         MAX_TIME_IN_SHIFT_TEMPLATES_DEFAULT);
      }
      return ManageShiftTemplates (cancellationToken, maxProcessTime);
    }

    /// <summary>
    /// <see cref="IGlobalAnalysis"/>
    /// </summary>
    /// <param name="maxTime"></param>
    /// <returns></returns>
    public bool ManageShiftTemplates (CancellationToken cancellationToken, TimeSpan maxTime)
    {
      Debug.Assert (null != m_shiftTemplateAnalysis);
      if (null == m_shiftTemplateAnalysis) {
        log.FatalFormat ("ManageShiftTemplates: " +
                         "m_shiftTemplateAnalysis was not initialized " +
                         "although it should have been initialized by the Initialize method " +
                         "=> initialize it again");
        m_shiftTemplateAnalysis = new ShiftTemplateAnalysis (m_restrictedTransactionLevel,
                                                             this);
      }

      TimeSpan untilFromNow = AnalysisConfigHelper.MaxShiftSlotProcess;
      DateTime until = DateTime.UtcNow.Add (untilFromNow).Date; // Full shift, to process a new shift only once a shift
      if (log.IsDebugEnabled) {
        log.Debug ($"ManageShiftTemplates: run the analysis until {until}");
      }
      var present = GetPresent ();
      var maxAnalysisDateTime = GetMaxAnalysisDateTime (maxTime);
      m_shiftTemplateAnalysis.Run (cancellationToken, present,
                                   until,
                                   maxAnalysisDateTime);
      return true;
    }


    /// <summary>
    /// <see cref="IGlobalAnalysis"/>
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
      return RunPendingModificationsAnalysis (cancellationToken, maxTime, minPastPriority, minPresentPriority);
    }

    /// <summary>
    /// <see cref="IGlobalAnalysis"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxTime"></param>
    /// <param name="minPastPriority"></param>
    /// <param name="minPresentPriority"></param>
    /// <returns></returns>
    public bool RunPendingModificationsAnalysis (CancellationToken cancellationToken, TimeSpan maxTime, int minPastPriority, int minPresentPriority)
    {
      Debug.Assert (null != m_pendingModificationAnalysis);
      if (null == m_pendingModificationAnalysis) {
        log.FatalFormat ("ManagePendingModificationAnalysis: " +
                         "m_pendingModificationAnalysis was not initialized " +
                         "although it should have been initialized by the Initialize method " +
                         "=> initialize it again");
        m_pendingModificationAnalysis = new PendingGlobalModificationAnalysis (m_activityAnalysis);
      }

      var maxAnalysisDateTime = GetMaxAnalysisDateTime (maxTime);
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
      if (null == m_pendingModificationAnalysis) {
        log.FatalFormat ("IsCleanFlaggedModificationsRequired: " +
                         "m_pendingModificationAnalysis was not initialized " +
                         "although it should have been initialized by the Initialize method " +
                         "=> initialize it again");
        m_pendingModificationAnalysis = new PendingGlobalModificationAnalysis (m_activityAnalysis);
      }
      return m_pendingModificationAnalysis.IsCleanRequired ();
    }

    /// <summary>
    /// <see cref="IGlobalAnalysis"/>
    /// </summary>
    /// <returns></returns>
    public bool CleanFlaggedModifications (CancellationToken cancellationToken)
    {
      var maxTime = Lemoine.Info.ConfigSet
  .LoadAndGet (MAX_TIME_CLEAN_FLAGGED_MODIFICATIONS_KEY,
    MAX_TIME_CLEAN_FLAGGED_MODIFICATIONS_DEFAULT);
      return CleanFlaggedModifications (cancellationToken, maxTime);
    }

    /// <summary>
    /// <see cref="IGlobalAnalysis"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxTime"></param>
    /// <returns></returns>
    public bool CleanFlaggedModifications (CancellationToken cancellationToken, TimeSpan maxTime)
    {
      Debug.Assert (null != m_pendingModificationAnalysis);
      if (null == m_pendingModificationAnalysis) {
        log.FatalFormat ("CleanFlaggedModifications: " +
                         "m_pendingModificationAnalysis was not initialized " +
                         "although it should have been initialized by the Initialize method " +
                         "=> initialize it again");
        m_pendingModificationAnalysis = new PendingGlobalModificationAnalysis (m_activityAnalysis);
      }

      var maxAnalysisDateTime = GetMaxAnalysisDateTime (maxTime);
      m_pendingModificationAnalysis.CleanFlaggedModifications (cancellationToken, maxAnalysisDateTime);
      return true;
    }

    /// <summary>
    /// <see cref="IGlobalAnalysis"/>
    /// </summary>
    /// <returns></returns>
    public bool ManageWeekNumbers (CancellationToken cancellationToken)
    {
      try {
        if (!m_weekNumbersCompleted) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ManageWeekNumbers: about to do it");
          }
          var weekNumberAnalysis = new WeekNumberAnalysis (m_restrictedTransactionLevel, this);
          weekNumberAnalysis.Run (cancellationToken);
          if (log.IsDebugEnabled) {
            log.Debug ($"ManageWeekNumbers: just completed");
          }
          m_weekNumbersCompleted = true;
        }
        else if (log.IsDebugEnabled) {
          log.Debug ($"ManageWeekNumbers: already completed");
        }
      }
      catch (OperationCanceledException) {
        throw;
      }
      catch (Exception ex) {
        log.Exception (ex, $"ManageWeekNumbers");
      }
      return true;
    }

    protected virtual DateTime GetMaxAnalysisDateTime (TimeSpan localMaxTime)
    {
      var maxAnalysisDateTime = this.StateMachineStartDateTime
        .Add (this.MaxTime);
      var maxDateTime = DateTime.UtcNow.Add (localMaxTime);
      if (maxAnalysisDateTime < maxDateTime) {
        return maxAnalysisDateTime;
      }
      else {
        return maxDateTime;
      }
    }

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
    public virtual void SwitchTo (IState<IGlobalAnalysis> state)
    {
      m_stateMachine.SwitchTo (state);
    }
    #endregion // Implementation of IContext

  }
}
