// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Lemoine.Core.ExceptionManagement;
using Lemoine.Extensions;
using Lemoine.GDBPersistentClasses;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
using Lemoine.Core.Performance;
using Lemoine.Core.StateMachine;
using Lemoine.Extensions.Analysis.StateMachine;
using System.Threading;

namespace Lemoine.Analysis
{
  /// <summary>
  /// This class takes care of the activity analysis
  /// of a unique monitored machine
  /// </summary>
  public class MonitoredMachineActivityAnalysis
    : MachineActivityAnalysis
    , IMonitoredMachineActivityAnalysis
  {
    /// <summary>
    /// Maximum number of facts that are processed in the same time
    /// </summary>
    static readonly string MAX_NUMBER_OF_FACTS_KEY = "Analysis.Activity.Facts.MaxNumber";
    static readonly int MAX_NUMBER_OF_FACTS_DEFAULT = 50;

    /// <summary>
    /// Number of attempt to complete a transaction in case of a serialization failure
    /// </summary>
    static readonly string NB_ATTEMPT_SERIALIZATION_FAILURE_KEY = "Analysis.Activity.NbAttemptSerializationFailure";
    static readonly int NB_ATTEMPT_SERIALIZATION_FAILURE_DEFAULT = 2;

    /// <summary>
    /// Maximum time to process machine state templates (else a new transaction is run to complete it)
    /// </summary>
    static readonly string MAX_TIME_PROCESS_MACHINE_STATE_TEMPLATE_KEY = "Analysis.Activity.MaxTimeProcessMachineStateTemplate";
    static readonly TimeSpan MAX_TIME_PROCESS_MACHINE_STATE_TEMPLATE_DEFAULT = TimeSpan.FromMinutes (3);

    /// <summary>
    /// Minimumm time before checking the maximum time is reached in machine state template analysis
    /// </summary>
    static readonly string MIN_TIME_PROCESS_MACHINE_STATE_TEMPLATE_KEY = "Analysis.Activity.MinTimeProcessMachineStateTemplate";
    static readonly TimeSpan MIN_TIME_PROCESS_MACHINE_STATE_TEMPLATE_DEFAULT = TimeSpan.FromMinutes (1);

    /// <summary>
    /// Maximum time to spend to each call to ProcessingReasonSlotAnalysis
    /// </summary>
    static readonly string MAX_TIME_PROCESSING_REASON_SLOTS_ANALYSIS_KEY =
      "Analysis.Activity.ProcessingReasonSlots.MaxTime";
    static readonly TimeSpan MAX_TIME_PROCESSING_REASON_SLOTS_ANALYSIS_DEFAULT = TimeSpan.FromSeconds (30);

    /// <summary>
    /// Minimum time before checking the maximum time is reached in ProcessingReasonSlotAnalysis
    /// </summary>
    static readonly string MIN_TIME_PROCESSING_REASON_SLOTS_ANALYSIS_KEY =
      "Analysis.Activity.ProcessingReasonSlots.MinTime";
    static readonly TimeSpan MIN_TIME_PROCESSING_REASON_SLOTS_ANALYSIS_DEFAULT = TimeSpan.FromSeconds (30);

    /// <summary>
    /// Maximum time to spend in detection analysis
    /// </summary>
    static readonly string MAX_TIME_DETECTION_ANALYSIS_KEY =
      "Analysis.Activity.Detection.MaxTime";
    static readonly TimeSpan MAX_TIME_DETECTION_ANALYSIS_DEFAULT = TimeSpan.FromSeconds (40);

    /// <summary>
    /// Minimum time before checking the maximum time is reached in detection analysis.
    /// Time per machine module
    /// </summary>
    static readonly string MIN_TIME_DETECTION_ANALYSIS_KEY =
      "Analysis.Activity.Detection.MinTime";
    static readonly TimeSpan MIN_TIME_DETECTION_ANALYSIS_DEFAULT = TimeSpan.FromSeconds (20);

    /// <summary>
    /// Maximum time to spend in AutoSequence analysis
    /// </summary>
    static readonly string MAX_TIME_AUTO_SEQUENCE_ANALYSIS_KEY =
      "Analysis.Activity.AutoSequences.MaxTime";
    static readonly TimeSpan MAX_TIME_AUTO_SEQUENCE_ANALYSIS_DEFAULT = TimeSpan.FromSeconds (20);

    /// <summary>
    /// Minimum time before checking the maximum time is reeached in AutoSequence analysis.
    /// Time per machine module
    /// </summary>
    static readonly string MIN_TIME_AUTO_SEQUENCE_ANALYSIS_KEY =
      "Analysis.Activity.AutoSequences.MinTime";
    static readonly TimeSpan MIN_TIME_AUTO_SEQUENCE_ANALYSIS_DEFAULT = TimeSpan.FromSeconds (10);

    #region Members
    readonly IMonitoredMachine m_monitoredMachine;
    IMonitoredMachineAnalysisStatus m_monitoredMachineAnalysisStatus = null;

    bool m_initialized = false;
    ProcessingReasonSlotsAnalysis m_processingReasonSlotsAnalysis;
    AutoSequenceAnalysis m_autoSequenceAnalysis;
    readonly IDetectionAnalysis m_detectionAnalysis;

    readonly IEnumerable<Lemoine.Extensions.Analysis.IAnalysisExtension> m_analysisExtensions;

    IStateMachine<IMonitoredMachineActivityAnalysis> m_stateMachine;
    IList<IFact> m_facts;
    #endregion // Members

    readonly ILog log = LogManager.GetLogger (typeof (MonitoredMachineActivityAnalysis).FullName);

    #region Getters / Setters
    /// <summary>
    /// Associated monitored machine
    /// </summary>
    public IMonitoredMachine MonitoredMachine => m_monitoredMachine;

    /// <summary>
    /// Reference to DetectionAnalysis
    /// </summary>
    public IDetectionAnalysis DetectionAnalysis => m_detectionAnalysis;

    /// <summary>
    /// <see cref="IMonitoredMachineActivityAnalysis"/>
    /// </summary>
    public IList<IFact> Facts => m_facts;
    #endregion // Getters / Setters

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="monitoredMachine">not null</param>
    public MonitoredMachineActivityAnalysis (IMonitoredMachine monitoredMachine)
      : base (monitoredMachine)
    {
      Debug.Assert (null != monitoredMachine);

      m_monitoredMachine = monitoredMachine;
      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                this.GetType ().FullName,
                                                monitoredMachine.Id));

      // Extension initialization
      m_analysisExtensions = Lemoine.Extensions.ExtensionManager
        .GetExtensions<Lemoine.Extensions.Analysis.IAnalysisExtension> (checkedThread: this)
        .Where (extension => extension.Initialize (monitoredMachine))
        .ToList (); // ToList is mandatory else the result of the Linq command is not cached

      m_processingReasonSlotsAnalysis = new ProcessingReasonSlotsAnalysis (monitoredMachine, this);
      m_autoSequenceAnalysis = new AutoSequenceAnalysis (this);
      m_detectionAnalysis = new DetectionAnalysis (this, m_analysisExtensions.OfType<Lemoine.Extensions.Analysis.IDetectionExtension> ());
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="monitoredMachine">not null</param>
    public MonitoredMachineActivityAnalysis (IMonitoredMachine monitoredMachine, IDetectionAnalysis detectionAnalysis)
      : base (monitoredMachine)
    {
      Debug.Assert (null != monitoredMachine);

      m_monitoredMachine = monitoredMachine;
      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                this.GetType ().FullName,
                                                monitoredMachine.Id));

      // Extension initialization
      m_analysisExtensions = Lemoine.Extensions.ExtensionManager
        .GetExtensions<Lemoine.Extensions.Analysis.IAnalysisExtension> (checkedThread: this)
        .Where (extension => extension.Initialize (monitoredMachine))
        .ToList (); // ToList is mandatory else the result of the Linq command is not cached

      m_processingReasonSlotsAnalysis = new ProcessingReasonSlotsAnalysis (monitoredMachine, this);
      m_autoSequenceAnalysis = new AutoSequenceAnalysis (this);
      m_detectionAnalysis = detectionAnalysis;
    }

    #region Methods
    IEnumerable<Lemoine.Extensions.Analysis.IActivityAnalysisExtension> GetActivityAnalysisExtensions ()
    {
      return m_analysisExtensions.OfType<Lemoine.Extensions.Analysis.IActivityAnalysisExtension> ();
    }

    /// <summary>
    /// Initialize
    /// </summary>
    public override bool Initialize ()
    {
      Debug.Assert (null != m_monitoredMachine);

      if (!m_initialized) {
        if (log.IsDebugEnabled) {
          log.Debug ("Initialize /B");
        }

        if (!base.Initialize ()) {
          if (log.IsInfoEnabled) {
            log.Info ($"Initialize: base.Initialize returned false => not completed");
          }
          return false;
        }

        if (IsPauseRequested ()) {
          if (log.IsInfoEnabled) {
            log.InfoFormat ($"Initialize: interrupt the initialization after the initialization of MachineActivityAnalysis because the modification analysis Id {GetPauseTriggeringModificationId ()} is run on the same monitored machine");
          }
          return false;
        }

        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          // - Initialize the single analysis
          m_detectionAnalysis.Initialize ();

          SetActive ();
          if (IsPauseRequested ()) {
            if (log.IsInfoEnabled) {
              log.Info ($"Initialize: interrupt the initialization after initializing the detection analysis because the modification analysis Id {GetPauseTriggeringModificationId ()} is run on the same monitored machine");
            }
            return false;
          }

          // - Initialize m_monitoredMachineAnalysisStatus with the time of the latest analysis
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.MonitoredMachineStatusInitialization",
                                                                                 TransactionLevel.ReadCommitted)) {
            m_monitoredMachineAnalysisStatus =
              ModelDAOHelper.DAOFactory.MonitoredMachineAnalysisStatusDAO
              .FindById (m_monitoredMachine.Id);
          }
          if (null == m_monitoredMachineAnalysisStatus) {
            m_monitoredMachineAnalysisStatus = ModelDAOHelper.ModelFactory
              .CreateMonitoredMachineAnalysisStatus (m_monitoredMachine);
            m_monitoredMachineAnalysisStatus.ActivityAnalysisDateTime = DateTime.UtcNow;
            using (IDAOTransaction transaction = session.BeginTransaction ("Analysis.MonitoredMachineStatusCreation",
                                                                           TransactionLevel.ReadCommitted)) { // Transaction: Initiate MonitoredMachineAnalysisStatus
              ModelDAOHelper.DAOFactory.MonitoredMachineAnalysisStatusDAO
                .MakePersistent (m_monitoredMachineAnalysisStatus);
              transaction.Commit ();
            }
          }
          Debug.Assert (null != m_monitoredMachineAnalysisStatus);
          if (log.IsDebugEnabled) {
            log.DebugFormat ("MakeActivityAnalysis: " +
                             "machine module status {0} loaded " +
                             "for monitored machine {1}",
                             m_monitoredMachineAnalysisStatus, m_monitoredMachine.Id);
          }

          SetActive ();
          if (IsPauseRequested ()) {
            if (log.IsInfoEnabled) {
              log.Info ($"Initialize: interrupt the initialization after getting the machine analysis status because the modification analysis Id {GetPauseTriggeringModificationId ()} is run on the same monitored machine");
            }
            return false;
          }

          SetActive ();
          if (IsPauseRequested ()) {
            if (log.IsInfoEnabled) {
              log.Info ($"Initialize: interrupt the initialization after getting the production analysis status because the modification analysis Id {GetPauseTriggeringModificationId ()} is run on the same monitored machine");
            }
            return false;
          }

          // - Note: there has already been an eager fetch of the machine modules

          // - Initialize the MachineModuleAnalysisStatus
          foreach (IMachineModule machineModule in m_monitoredMachine.MachineModules) {
            IMachineModuleAnalysisStatus machineModuleAnalysisStatus;
            using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.MachineModuleStatusInitialization",
                                                                                   TransactionLevel.ReadCommitted)) {
              machineModuleAnalysisStatus = ModelDAOHelper.DAOFactory.MachineModuleAnalysisStatusDAO
                .FindById (machineModule.Id);
            }
            if (null == machineModuleAnalysisStatus) {
              machineModuleAnalysisStatus = ModelDAOHelper.ModelFactory
                .CreateMachineModuleAnalysisStatus (machineModule);
              using (IDAOTransaction transaction = session.BeginTransaction ("Analysis.MachineModuleStatusCreation",
                                                                             TransactionLevel.ReadCommitted)) { // Transaction: Initiate MachineModuleAnalysisStatus
                ModelDAOHelper.DAOFactory.MachineModuleAnalysisStatusDAO
                  .MakePersistent (machineModuleAnalysisStatus);
                transaction.Commit ();
              }
            }
            SetActive ();
            if (IsPauseRequested ()) {
              if (log.IsInfoEnabled) {
                log.Info ($"Initialize: interrupt the initialization after getting one machine module analysis status because the modification analysis Id {GetPauseTriggeringModificationId ()} is run on the same monitored machine");
              }
              return false;
            }
          }
        }

        log.Debug ("Initialize /E");
        m_initialized = true;
      }

      return true;
    }

    /// <summary>
    /// Return the state machine
    /// </summary>
    /// <returns></returns>
    protected override IContext GetStateMachine ()
    {
      return m_stateMachine;
    }

    /// <summary>
    /// Load the state machine
    /// </summary>
    /// <returns></returns>
    protected override bool LoadStateMachine ()
    {
      if (null == m_stateMachine) {
        var request = new Lemoine.Business.Extension
          .MonitoredMachineExtensions<IMonitoredMachineActivityAnalysisStateMachineExtension> (m_monitoredMachine, (ext, m) => ext.Initialize (this));
        var firstMatchingExtension = Lemoine.Business.ServiceProvider
          .Get (request)
          .OrderByDescending (ext => ext.Priority)
          .FirstOrDefault ();
        if (null == firstMatchingExtension) {
          GetLogger ().Error ("LoadStateMachine: no extension IMonitoredMachineActivityAnalysisStateMachineExtension");
          return false;
        }
        m_stateMachine = new Lemoine.Analysis.StateMachine.AnalysisStateMachine<IMonitoredMachineActivityAnalysis, MonitoredMachineActivityAnalysis> (firstMatchingExtension.InitialState, this);
      }

      return true;
    }

    /// <summary>
    /// Initialize some properties before a new analysis
    /// </summary>
    protected override void InitializeNewAnalysis ()
    {
      base.InitializeNewAnalysis ();
      m_facts = new List<IFact> ();
    }

    /// <summary>
    /// Run the state machine
    /// 
    /// Suppose the internal properties are already correctly initialized
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="throwStateException">throw an exception if one of the state returned an exception, else return true</param>
    /// <returns>if true is returned, any other process can continue (kind of completed)</returns>
    protected override bool RunStateMachine (CancellationToken cancellationToken, bool throwStateException)
    {
      var result = m_stateMachine.Run (cancellationToken);
      if (throwStateException && this.StateExceptions.Any ()) {
        log.Error ($"RunStateMachine: {this.StateExceptions.Count ()} exceptions in state machine execution => throw an exception");
        throw new Exception ("Exception in MakeAnalysis (inner is first)", this.StateExceptions.First ());
      }
      return result;
    }

    /// <summary>
    /// Run the analysis of this MachineActivityAnalysis class
    /// without initializing the properties of a new analysis
    /// 
    /// <see cref="IMonitoredMachineActivityAnalysis"/>
    /// </summary>
    /// <returns></returns>
    public bool RunMachineActivityAnalysis (CancellationToken cancellationToken)
    {
      if (!base.LoadStateMachine ()) {
        GetLogger ().Error ("MakeAnalysis: no state machine, do nothing");
        return false;
      }

      return base.RunStateMachine (cancellationToken, false);
    }

    /// <summary>
    /// <see cref="IMonitoredMachineActivityAnalysis"/>
    /// </summary>
    /// <returns>if false, interrupt the state machine</returns>
    public bool RunProcessingReasonSlotsAnalysisLastPeriod (CancellationToken cancellationToken, TimeSpan lastPeriod)
    {
      var range = new UtcDateTimeRange (DateTime.UtcNow.Subtract (lastPeriod));
      return RunProcessingReasonSlotsAnalysis (cancellationToken, range: range);
    }

    /// <summary>
    /// <see cref="IMonitoredMachineActivityAnalysis"/>
    /// </summary>
    /// <returns>if false, interrupt the state machine</returns>
    public bool RunProcessingReasonSlotsAnalysisLastPeriod (CancellationToken cancellationToken, TimeSpan maxTime, TimeSpan minTime, TimeSpan lastPeriod)
    {
      var range = new UtcDateTimeRange (DateTime.UtcNow.Subtract (lastPeriod));
      return RunProcessingReasonSlotsAnalysis (cancellationToken, maxTime, minTime, range: range);
    }

    /// <summary>
    /// <see cref="IMonitoredMachineActivityAnalysis"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="range">Optional range</param>
    /// <returns>if false, interrupt the state machine</returns>
    public bool RunProcessingReasonSlotsAnalysis (CancellationToken cancellationToken, UtcDateTimeRange range = null)
    {
      var maxTimeProcessingReasonSlotsAnalysis = Lemoine.Info.ConfigSet
        .LoadAndGet (MAX_TIME_PROCESSING_REASON_SLOTS_ANALYSIS_KEY,
          MAX_TIME_PROCESSING_REASON_SLOTS_ANALYSIS_DEFAULT);
      var minTimeProcessingReasonSlotsAnalysis = Lemoine.Info.ConfigSet
        .LoadAndGet (MIN_TIME_PROCESSING_REASON_SLOTS_ANALYSIS_KEY,
          MIN_TIME_PROCESSING_REASON_SLOTS_ANALYSIS_DEFAULT);
      return RunProcessingReasonSlotsAnalysis (cancellationToken, maxTimeProcessingReasonSlotsAnalysis, minTimeProcessingReasonSlotsAnalysis, range);
    }

    /// <summary>
    /// <see cref="IMonitoredMachineActivityAnalysis"/>
    /// </summary>
    /// <param name="maxTime">Max time</param>
    /// <param name="minTime">Minimum time before checking the maximum time</param>
    /// <param name="range">Optional range</param>
    /// <param name="maxLoopNumber"></param>
    /// <returns>if false, interrupt the state machine</returns>
    public bool RunProcessingReasonSlotsAnalysis (CancellationToken cancellationToken, TimeSpan maxTime, TimeSpan minTime, UtcDateTimeRange range = null, int? maxLoopNumber = null)
    {
      var maxProcessingReasonSlotsAnalysisDateTime =
        GetMaxAnalysisDateTime (maxTime, minTime);
      m_processingReasonSlotsAnalysis.RunOnce (cancellationToken, maxProcessingReasonSlotsAnalysisDateTime, range, maxLoopNumber);
      return true;
    }

    /// <summary>
    /// <see cref="IMonitoredMachineActivityAnalysis"/>
    /// </summary>
    /// <returns>if false, interrupt the state machine</returns>
    public bool RunDetectionAnalysis (CancellationToken cancellationToken)
    {
      var maxTimeDetectionAnalysis = Lemoine.Info.ConfigSet
        .LoadAndGet (MAX_TIME_DETECTION_ANALYSIS_KEY,
          MAX_TIME_DETECTION_ANALYSIS_DEFAULT);
      var minTimeDetectionAnalysis = Lemoine.Info.ConfigSet
        .LoadAndGet (MIN_TIME_DETECTION_ANALYSIS_KEY,
          MIN_TIME_DETECTION_ANALYSIS_DEFAULT);
      return RunDetectionAnalysis (cancellationToken, maxTimeDetectionAnalysis, minTimeDetectionAnalysis);
    }

    /// <summary>
    /// <see cref="IMonitoredMachineActivityAnalysis"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxTime">Max time</param>
    /// <param name="minTime">Minimum time before checking the maximum time is reached</param>
    /// <param name="maxDetectionNumber">max number of detection number to consider. If null, consider a configuration key</param>
    /// <returns>if false, interrupt the state machine</returns>
    public bool RunDetectionAnalysis (CancellationToken cancellationToken, TimeSpan maxTime, TimeSpan minTime, int? maxDetectionNumber = null)
    {
      var maxDetectionAnalysisDateTime = GetMaxAnalysisDateTime (maxTime, minTime);
      m_detectionAnalysis.RunOnce (cancellationToken, maxDetectionAnalysisDateTime, minTime, maxDetectionNumber);
      return true;
    }

    /// <summary>
    /// <see cref="IMonitoredMachineActivityAnalysis"/>
    /// </summary>
    /// <returns></returns>
    public bool RunAutoSequenceAnalysis (CancellationToken cancellationToken)
    {
      var maxTimeAutoSequenceAnalysis = Lemoine.Info.ConfigSet
        .LoadAndGet (MAX_TIME_AUTO_SEQUENCE_ANALYSIS_KEY,
          MAX_TIME_AUTO_SEQUENCE_ANALYSIS_DEFAULT);
      var minTimeAutoSequenceAnalysis = Lemoine.Info.ConfigSet
        .LoadAndGet (MIN_TIME_AUTO_SEQUENCE_ANALYSIS_KEY,
          MIN_TIME_AUTO_SEQUENCE_ANALYSIS_DEFAULT);
      return RunAutoSequenceAnalysis (cancellationToken, maxTimeAutoSequenceAnalysis, minTimeAutoSequenceAnalysis);
    }

    /// <summary>
    /// <see cref="IMonitoredMachineActivityAnalysis"/>
    /// </summary>
    public bool RunAutoSequenceAnalysis (CancellationToken cancellationToken, TimeSpan maxTime, TimeSpan minTime, int? numberOfItems = null)
    {
      var maxAutoSequenceAnalysisDateTime = GetMaxAnalysisDateTime (maxTime, minTime);
      m_autoSequenceAnalysis.ManageAutoSequencePeriods (cancellationToken, m_facts, maxAutoSequenceAnalysisDateTime, minTime, numberOfItems);
      return true;
    }

    IList<IFact> GetFacts (DateTime after, int? maxNumberOfFacts = null)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.GetFacts",
                                                                               TransactionLevel.ReadCommitted)) {
          int effectiveMaxNumberOfFacts = maxNumberOfFacts ?? ConfigSet.LoadAndGet<int> (MAX_NUMBER_OF_FACTS_KEY,
                                                                                         MAX_NUMBER_OF_FACTS_DEFAULT);
          return
            ModelDAOHelper.DAOFactory.FactDAO.FindAllAfter (m_monitoredMachine,
                                                            after,
                                                            effectiveMaxNumberOfFacts);
        } // auto commit because read-only
      }
    }

    public bool RunActivityAnalysis (CancellationToken cancellationToken, int? maxNumberOfFacts = null)
    {
      try {
        DateTime lastActivityAnalysisDateTime = m_monitoredMachineAnalysisStatus.ActivityAnalysisDateTime;
        m_facts = GetFacts (lastActivityAnalysisDateTime, maxNumberOfFacts);
        SetActive ();

        if (IsPauseRequested ()) {
          GetLogger ().Info ($"RunActivityAnalysis: interrupt the activity analysis after getting the facts because the modification analysis Id {GetPauseTriggeringModificationId ()} is run on the same monitored machine");
          throw new InterruptException ("Modification analysis on the same monitored machine");
        }

        foreach (var extension in GetActivityAnalysisExtensions ()) {
          try {
            extension.BeforeProcessingActivities (lastActivityAnalysisDateTime, m_facts);
          }
          catch (Exception ex1) {
            GetLogger ().Error ($"RunActivityAnalysis: BeforeProcessingActivities failed for {extension}", ex1);
            throw;
          }
        }

        if (0 == m_facts.Count) {
          // UNDONE: process the machine off
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ($"RunActivityAnalysis: no fact after {m_monitoredMachineAnalysisStatus.ActivityAnalysisDateTime}");
          }
        }
        else { // 0 < facts.Count => ManageActivities
          int nbAttemptSerializationFailure = ConfigSet.LoadAndGet<int> (NB_ATTEMPT_SERIALIZATION_FAILURE_KEY,
                                                                         NB_ATTEMPT_SERIALIZATION_FAILURE_DEFAULT);
          for (int attempt = 0; attempt < nbAttemptSerializationFailure; attempt++) { // Limit the number of attempts in case of serializable failure
            SetActive ();
            if (log.IsDebugEnabled) {
              GetLogger ().Debug ($"RunActivityAnalysis: attempt={attempt} lastActivityAnalysisDateTime={lastActivityAnalysisDateTime}");
            }
            try {
              ProcessActivities (cancellationToken, lastActivityAnalysisDateTime, m_facts);
              break; // Transaction ok
            }
            catch (OperationCanceledException ex) {
              GetLogger ().Warn ($"RunActivityAnalysis: cancelled", ex);
              throw;
            }
            catch (Exception ex) {
              // Reload m_monitoredMachineAnalysisStatus that may have been updated
              try {
                // Extension point
                foreach (var extension in GetActivityAnalysisExtensions ()) {
                  SetActive ();
                  extension.AfterActivitiesRollback ();
                }
                SetActive ();

                using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
                  using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.MonitoredMachineStatusReload")) {
                    m_monitoredMachineAnalysisStatus = ModelDAOHelper.DAOFactory.MonitoredMachineAnalysisStatusDAO
                      .FindById (m_monitoredMachine.Id); // Reload may return a Stale exception
                  } // auto commit because read-only
                }
                if (null == m_monitoredMachineAnalysisStatus) {
                  GetLogger ().Fatal ("RunActivityAnalysis: null MonitoredMachineAnalysisStatus");
                  Debug.Assert (null != m_monitoredMachineAnalysisStatus);
                  throw new Exception ("null MonitoredMachineAnalysisStatus");
                }
                else if (!lastActivityAnalysisDateTime.Equals (m_monitoredMachineAnalysisStatus.ActivityAnalysisDateTime)) {
                  GetLogger ().Info ($"RunActivityAnalysis: Activity Analysis DateTime changed from {lastActivityAnalysisDateTime} to {m_monitoredMachineAnalysisStatus.ActivityAnalysisDateTime} after the serialization failure, reset lastActivityAnalydidDateTime and continue");
                  lastActivityAnalysisDateTime = m_monitoredMachineAnalysisStatus.ActivityAnalysisDateTime;
                  Debug.Assert (m_facts.All (f => lastActivityAnalysisDateTime < f.End));
                }
              }
              catch (OperationCanceledException ex1) {
                GetLogger ().Warn ($"RunActivityAnalysis: cancelled", ex1);
                throw;
              }
              catch (Exception ex1) {
                GetLogger ().Fatal ($"RunActivityAnalysis: re-loading monitoredMachineAnalysisStatus after {ex} failed", ex1);
                throw new Exception ("Reload error of MonitoredMachineAnalysisStatus", ex);
              }

              // Try again if it is a serialization/stale failure and if there is no pause, else exit
              if (!IsPauseRequested ()
                  && (ExceptionTest.IsStale (ex, GetLogger ()) || ExceptionTest.IsTemporary (ex, GetLogger ()))) {
                GetLogger ().Warn ($"RunActivityAnalysis: temporary (serialization) or stale failure with attempt {attempt} ", ex);
                // Note: There is no need to clear the pending saves here
                //       because there is no open session
              }
              else {
                GetLogger ().Error ("RunActivityAnalysis: exception in ManageActivities", ex);
                throw;
              }
            }
          } // End attempt loop
          SetActive ();
        } // End 0 < facts.Count
      }
      catch (Exception ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"RunActivityAnalysis: exception {ex}", ex);
        }
        m_facts = new List<IFact> ();
        throw;
      }

      return true;
    }

    /// <summary>
    /// Get the present UTC date/time
    /// </summary>
    /// <returns></returns>
    protected override DateTime GetPresent ()
    {
      return m_monitoredMachineAnalysisStatus.ActivityAnalysisDateTime;
    }

    #region ManageActivities part
    /// <summary>
    /// Process the activities in transactions
    /// </summary>
    /// <param name="lastActivityAnalysisDateTime"></param>
    /// <param name="facts"></param>
    void ProcessActivities (CancellationToken cancellationToken, DateTime lastActivityAnalysisDateTime, IList<IFact> facts)
    {
      Debug.Assert (0 < facts.Count);
      if (facts.Count <= 0) {
        log.Fatal ("ProcessActivities: no fact to process, this should not happen because of the pre-condition");
        return;
      }

      Debug.Assert (m_monitoredMachineAnalysisStatus.ActivityAnalysisDateTime < facts[facts.Count - 1].End);
      if (facts[facts.Count - 1].End <= m_monitoredMachineAnalysisStatus.ActivityAnalysisDateTime) {
        log.FatalFormat ("ProcessActivities: " +
                         "last fact {0} before ActivityAnalysisDateTime {1}, " +
                         "this should not happen because of the pre-condition",
                         facts[facts.Count - 1], m_monitoredMachineAnalysisStatus.ActivityAnalysisDateTime);
        return;
      }

      // Get the full period to process
      Debug.Assert (0 != facts.Count);
      DateTime fullActivityBegin = m_monitoredMachineAnalysisStatus.ActivityAnalysisDateTime;
      DateTime fullActivityEnd = facts[facts.Count - 1].End;
      if (fullActivityEnd <= fullActivityBegin) {
        log.Warn ($"ProcessActivities: no fact after activity analysis date time {fullActivityBegin}");
        return;
      }

      while (true) {
        SetActive ();
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ($"Analysis.ManageActivities.{this.Machine.Id}.{fullActivityBegin}-{fullActivityEnd}",
                                                                       this.RestrictedTransactionLevel)) {
          transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data

          IList<IObservationStateSlot> observationStateSlots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .GetListInRange (m_monitoredMachine,
                             fullActivityBegin,
                             fullActivityEnd);
          Debug.Assert (0 != observationStateSlots.Count); // At least one observation state slot must always exist
          SetActive ();

          // Check there is no null machine observation state in ObservationStateSlots
          if (false == CheckObservationStateSlotsProcessed (cancellationToken, observationStateSlots,
                                                            fullActivityEnd)) {
            log.Debug ("ProcessActivities: some observation state slots were processed, continue in another transaction");
            ModelDAOHelper.DAOFactory.Flush (); // For the unit tests
            transaction.Commit ();
            continue;
          }

          ManageActivities (lastActivityAnalysisDateTime, facts,
                            fullActivityBegin, fullActivityEnd,
                            observationStateSlots);

          // Extension point
          foreach (var extension in GetActivityAnalysisExtensions ()) {
            SetActive ();
            extension.BeforeActivitiesCommit ();
          }
          SetActive ();

          transaction.Commit ();
        }
        break;
      }
    }

    bool CheckObservationStateSlotsProcessed (CancellationToken cancellationToken, IEnumerable<IObservationStateSlot> observationStateSlots,
                                              DateTime until)
    {
      // TODO: cancellationToken
      TimeSpan maxTimeInProcessMachineStateTemplate = Lemoine.Info.ConfigSet
        .LoadAndGet<TimeSpan> (MAX_TIME_PROCESS_MACHINE_STATE_TEMPLATE_KEY,
                               MAX_TIME_PROCESS_MACHINE_STATE_TEMPLATE_DEFAULT);
      TimeSpan minTimeInProcessMachineStateTemplate = Lemoine.Info.ConfigSet
        .LoadAndGet<TimeSpan> (MIN_TIME_PROCESS_MACHINE_STATE_TEMPLATE_KEY,
                               MIN_TIME_PROCESS_MACHINE_STATE_TEMPLATE_DEFAULT);
      var maxAnalysisDateTime = GetMaxAnalysisDateTime (maxTimeInProcessMachineStateTemplate, minTimeInProcessMachineStateTemplate);
      var observationStateSlotsToProcess = observationStateSlots
        .Where (oss => (null == oss.MachineObservationState));
      if (!observationStateSlotsToProcess.Any ()) {
        return true;
      }
      else { // Any ()
        foreach (var observationStateSlot in observationStateSlotsToProcess) {
          SetActive ();
          if (log.IsDebugEnabled) {
            log.Debug ($"CheckObservationStateSlotsProcessed: Process template between {observationStateSlot.BeginDateTime} and {until}");
          }
          if (cancellationToken.IsCancellationRequested) {
            log.Warn ($"CheckObservationStateSlotsProcessed: cancellation requested => return false");
            return false;
          }
          // - Process the template between observationStateSlot.BeginDateTime and fullActivityEnd
          if (false ==
              ((ObservationStateSlot)observationStateSlot)
              .ProcessTemplate (cancellationToken, new UtcDateTimeRange (observationStateSlot.BeginDateTime, until),
                                null, true, this, maxAnalysisDateTime)) {
            log.Warn ("CheckObservationStateSlotsProcessed: ProcessTemplate interrupted because maxAnalysisDateTime was reached");
            return false;
          }
          if (maxAnalysisDateTime < DateTime.UtcNow) {
            log.Warn ($"CheckObservationStateSlotsProcessed: do not process more observation state slots because {maxAnalysisDateTime} was reached");
            return false;
          }
        }
        // - Return false because the observation state slots needs to be reloaded
        //   and it is better to open a new transaction for that
        log.Debug ("CheckObservationStateSlotsProcessed: some machine state templates were processed => return false");
        return false;
      }
    }

    void ManageActivities (DateTime lastActivityAnalysisDateTime, IList<IFact> facts,
                           DateTime fullActivityBegin, DateTime fullActivityEnd,
                           IList<IObservationStateSlot> observationStateSlots)
    {
      Debug.Assert (fullActivityBegin < fullActivityEnd);
      Debug.Assert (observationStateSlots.Any ());

      // Reload m_monitoredMachineAnalysisStatus
      try {
        ModelDAOHelper.DAOFactory.MonitoredMachineAnalysisStatusDAO
          .Lock (m_monitoredMachineAnalysisStatus);
      }
      catch (Exception ex) {
        log.Fatal ("ManageActivities: Lock of MonitoredMachineAnalysisStatus failed", ex);
        m_monitoredMachineAnalysisStatus = ModelDAOHelper.DAOFactory.MonitoredMachineAnalysisStatusDAO
          .Reload (m_monitoredMachineAnalysisStatus);
        throw;
      }

      if (!lastActivityAnalysisDateTime.Equals (m_monitoredMachineAnalysisStatus.ActivityAnalysisDateTime)) {
        log.Fatal ($"ManageActivities: activity analysis datetime changed from {lastActivityAnalysisDateTime} to {m_monitoredMachineAnalysisStatus.ActivityAnalysisDateTime} !");
        Debug.Assert (false);
        throw new Exception ("ActivityAnalysisDateTime incoherent");
      }

      // Get the corresponding OperationSlots in the [minRunning, maxRunning] period
      var operationSlots = GetOperationSlots (fullActivityBegin, facts);
      SetActive ();

      int factIndex = 0;
      int observationStateSlotIndex = 0;
      int operationSlotIndex = 0;
      // Loop on the facts and on the machine observation states
      while (factIndex < facts.Count) {
        SetActive ();

        // There should be an Observation State Slot until the end
        Debug.Assert (observationStateSlotIndex < observationStateSlots.Count);

        if (facts[factIndex].End <= fullActivityBegin) {
          // This fact is out of range
          ++factIndex;
          continue;
        }

        // Get the facts properties
        DateTime factBegin = facts[factIndex].Begin;
        DateTime factEnd = facts[factIndex].End;
        IMachineMode machineMode = facts[factIndex].CncMachineMode;
        if (log.IsDebugEnabled) {
          log.Debug ($"ManageActivities: process fact {factBegin}-{factEnd} machineMode={machineMode}");
        }

        // Skip the observation state slots that occur before factBegin
        // This may happen in case of a "no data" period
        while ((observationStateSlotIndex + 1 < observationStateSlots.Count)
               && (NullableDateTime.Compare (observationStateSlots[observationStateSlotIndex].EndDateTime,
                                             factBegin) <= 0)) {
          ++observationStateSlotIndex;
        }

        // Skip the observation state slots that occur before factBegin
        // This may happen in case of a "no data" period
        while ((operationSlotIndex + 1 < operationSlots.Count)
               && (NullableDateTime.Compare (operationSlots[operationSlotIndex].EndDateTime,
                                             factBegin) <= 0)) {
          ++operationSlotIndex;
        }

        // See if the next facts can be merged with this one
        while ((factIndex + 1 < facts.Count)
               && (facts[factIndex + 1].Begin.Equals (factEnd))
               && (facts[factIndex + 1].CncMachineMode.Equals (machineMode))
               && (!observationStateSlots[observationStateSlotIndex].EndDateTime.HasValue
                   || (facts[factIndex + 1].Begin < observationStateSlots[observationStateSlotIndex].EndDateTime.Value))) {
          ++factIndex;
          factEnd = facts[factIndex].End;
        }

        Debug.Assert (Bound.Compare<DateTime> (observationStateSlots[observationStateSlotIndex].BeginDateTime, factEnd) <= 0);
        Debug.Assert (NullableDateTime.Compare (factBegin,
                                                observationStateSlots[observationStateSlotIndex].EndDateTime)
                      <= 0);

        if ((fullActivityBegin <= factEnd)
            || (factBegin <= fullActivityEnd)) {
          // fact period intersects fullActivity period
          // => there is some activity to process

          // Correct factBegin if needed
          if (factBegin < fullActivityBegin) {
            factBegin = fullActivityBegin;
          }

          {
            // Loop on the observation state slots
            // that intersect this fact
            // to process the activity
            DateTime activityObservationPeriodBegin = factBegin;
            while (observationStateSlotIndex < observationStateSlots.Count) {
              SetActive ();
              IObservationStateSlot observationStateSlot =
                observationStateSlots[observationStateSlotIndex];
              DateTime activityObservationPeriodEnd =
                Bound.Compare<DateTime> (factEnd,
                                         observationStateSlot.EndDateTime) < 0
                ? factEnd
                : observationStateSlot.EndDateTime.Value;
              Debug.Assert (activityObservationPeriodBegin <= activityObservationPeriodEnd);

              // activityObservationPeriodEnd is strictly after ActivityAnalysisDateTime
              // because the retrieved observationStateSlots are after fullActivityBegin
              Debug.Assert (m_monitoredMachineAnalysisStatus.ActivityAnalysisDateTime < activityObservationPeriodEnd);
              if (activityObservationPeriodEnd <= m_monitoredMachineAnalysisStatus.ActivityAnalysisDateTime) {
                log.Fatal ($"ManageActivities: activityObservationPeriodEnd {activityObservationPeriodEnd} before ActivityAnalysisDateTime {m_monitoredMachineAnalysisStatus.ActivityAnalysisDateTime}");
                throw new InvalidOperationException ("Bad activityObservationPeriodEnd");
              }

              // observationStateSlot.MachineObservationState is not null
              Debug.Assert (null != observationStateSlot.MachineObservationState);
              if (null == observationStateSlot.MachineObservationState) {
                log.Error ($"ManageActivities: null machine observation state in slot {observationStateSlot}, may be because the transaction was not serializable");
                throw new Lemoine.GDBUtils.TransientAnalysisException ("ManageActivities: null machine observation state in slot");
              }

              // Process the new activity period
              if (false == ProcessNewActivityPeriod (new UtcDateTimeRange (activityObservationPeriodBegin,
                                                                           activityObservationPeriodEnd),
                                                     machineMode,
                                                     observationStateSlot.MachineStateTemplate,
                                                     observationStateSlot.MachineObservationState,
                                                     observationStateSlot.Shift)) {
                // The observation state slots must be reloaded before continuing
                log.Debug ("ManageActivities: exit because the observation state slots must be reloaded first");
                return;
              }

              // Flag it as done
              if (log.IsDebugEnabled) {
                log.Debug ($"ManageActivities: set ActivityAnalysisDateTime={activityObservationPeriodEnd}");
              }
              Debug.Assert (m_monitoredMachineAnalysisStatus.ActivityAnalysisDateTime < activityObservationPeriodEnd);
              m_monitoredMachineAnalysisStatus.ActivityAnalysisDateTime = activityObservationPeriodEnd;
              m_monitoredMachineAnalysisStatus.ActivityAnalysisCount++;
              ModelDAOHelper.DAOFactory.MonitoredMachineAnalysisStatusDAO
                .MakePersistent (m_monitoredMachineAnalysisStatus);

              if (NullableDateTime.Compare (factEnd,
                                            observationStateSlot.EndDateTime)
                  <= 0) {
                if (factEnd.Equals (observationStateSlot.EndDateTime)) {
                  ++observationStateSlotIndex;
                }
                if (log.IsDebugEnabled) {
                  log.Debug ($"ManageActivities: factEnd={factEnd} is before observationStateSlot={observationStateSlot.EndDateTime} => visit the next fact");
                }
                break;
              }
              else {
                // Take the next observation state slot
                Debug.Assert (observationStateSlot.EndDateTime.HasValue);
                activityObservationPeriodBegin = observationStateSlot.EndDateTime.Value;
                ++observationStateSlotIndex;
                Debug.Assert ((observationStateSlotIndex == observationStateSlots.Count)
                              || observationStateSlot.EndDateTime.Equals (observationStateSlots[observationStateSlotIndex].BeginDateTime));
                if (log.IsDebugEnabled) {
                  log.Debug ($"ManageActivities: take the next observation state slot begin={activityObservationPeriodBegin}");
              }
              }
            } // End loop on ObservationStateSlots
          }

          if (machineMode.Running.HasValue && machineMode.Running.Value
              && AnalysisConfigHelper.OperationSlotRunTime) { // Activity period
            // Loop on the operation slots
            // that intersect this fact
            // to process the activity in case the machine is running
            while (operationSlotIndex < operationSlots.Count) {
              SetActive ();
              IOperationSlot operationSlot =
                operationSlots[operationSlotIndex];

              if (log.IsDebugEnabled) {
                log.Debug ($"ManageActivities: operation slot part: current operation slot is {operationSlot} {operationSlot.DateTimeRange}");
              }

              if (Bound.Compare<DateTime> (factEnd, operationSlot.BeginDateTime) <= 0) {
                // fact before the operation slot
                // => operation slots must be checked with the next fact
                if (log.IsDebugEnabled) {
                  log.Debug ($"ManageActivities: operation slot part: operation slot begin={operationSlot.BeginDateTime} is after the fact end={factEnd}, get the next fact");
                }
                break;
              }
              if (Bound.Compare<DateTime> (operationSlot.EndDateTime, factBegin) <= 0) {// just in case
                // operation slot before the fact
                // => visit the next operation slot
                if (log.IsDebugEnabled) {
                  log.Debug ($"ManageActivities: operation slot part: operationSlot.End={operationSlot.EndDateTime} before factBegin={factBegin} => visit the next operation slot");
                }
                ++operationSlotIndex;
                continue;
              }

              // Intersection ok
              Debug.Assert (operationSlot.DateTimeRange.Overlaps (new UtcDateTimeRange (factBegin, factEnd)));

              UtcDateTimeRange activityOperationPeriod =
                new UtcDateTimeRange (operationSlot.DateTimeRange.Intersects (new UtcDateTimeRange (factBegin, factEnd)));
              Debug.Assert (!activityOperationPeriod.IsEmpty ()); // Because Overlaps, see above

              if (log.IsDebugEnabled) {
                log.Debug ($"ManageActivities: operation slot part: the intersection period is: {activityOperationPeriod} on operation slot={operationSlot?.Id}");
              }

              // Process the new activity period
              Debug.Assert (machineMode.Running.HasValue);
              Debug.Assert (machineMode.Running.Value);
              Debug.Assert (activityOperationPeriod.Duration.HasValue);
              TimeSpan activityDuration = activityOperationPeriod.Duration.Value;
              if (operationSlot.RunTime.HasValue) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"ManageActivities: operation slot part: add duration {activityDuration} to old run time {operationSlot.RunTime} into operation slot {operationSlot?.Id}");
                }
                operationSlot.RunTime += activityDuration;
              }
              else {
                if (log.IsDebugEnabled) {
                  log.Debug ($"ManageActivities: operation slot part: initialize operation slot {operationSlot?.Id} with run time {activityDuration}");
                }
                operationSlot.RunTime = activityDuration;
              }
              ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (operationSlot);

              if (NullableDateTime.Compare (factEnd,
                                            operationSlot.EndDateTime)
                  < 0) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"ManageActivities: operation slot part: factEnd={factEnd} is before operationslot end={operationSlot.EndDateTime} => visit the next fact");
                }
                break;
              }
              else {
                // Take the next operation slot
                Debug.Assert (operationSlot.EndDateTime.HasValue);
                ++operationSlotIndex;
                if (log.IsDebugEnabled) {
                  log.Debug ($"ManageActivities: operation slot part: take the next operation slot begin={activityOperationPeriod.Lower}");
              }
              }
            } // End loop on OperationSlots
          }
        } // if fact in activity period

        // Take the next fact
        factIndex++;

      } // End first loop on facts
    }

    IList<IOperationSlot> GetOperationSlots (DateTime fullActivityBegin, IList<IFact> facts)
    {
      DateTime minRunningPeriod = DateTime.MaxValue;
      DateTime maxRunningPeriod = DateTime.MaxValue;
      foreach (IFact fact in facts) {
        // Update the running properties if applicable
        if (fact.CncMachineMode.Running.HasValue && fact.CncMachineMode.Running.Value) {
          if (minRunningPeriod.Equals (DateTime.MaxValue)) {
            minRunningPeriod = fact.Begin;
          }
          maxRunningPeriod = fact.End;
        }
      }
      if (minRunningPeriod < fullActivityBegin) {
        minRunningPeriod = fullActivityBegin;
      }
      return ModelDAOHelper.DAOFactory.OperationSlotDAO
        .FindOverlapsRange (m_monitoredMachine,
                            new UtcDateTimeRange (minRunningPeriod, maxRunningPeriod));
    }

    /// <summary>
    /// if false is returned, it was only partially applied and the observation state slots must be reloaded
    /// </summary>
    /// <param name="activityRange"></param>
    /// <param name="machineMode"></param>
    /// <param name="machineStateTemplate"></param>
    /// <param name="machineObservationState">not null</param>
    /// <param name="shift"></param>
    /// <returns></returns>
    bool ProcessNewActivityPeriod (UtcDateTimeRange activityRange,
                                   IMachineMode machineMode,
                                   IMachineStateTemplate machineStateTemplate,
                                   IMachineObservationState machineObservationState,
                                   IShift shift)
    {
      SetActive ();
      Debug.Assert (null != machineObservationState);

      if (activityRange.IsEmpty ()) {
        log.Debug ("ProcessNewActivityPeriod: empty period");
        return true;
      }
      else {
        Debug.Assert (null != ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (this.Machine.Id));

        // Udate the ReasonSlots through CncActivityMachineAssociation
        var activityModification =
          new CncActivityMachineAssociation (this.MonitoredMachine,
                                             machineMode,
                                             machineStateTemplate,
                                             machineObservationState,
                                             activityRange);
        activityModification.Caller = this;
        activityModification.Shift = shift;

        IMachineStatus machineStatus =
          ModelDAOHelper.DAOFactory.MachineStatusDAO
          .FindById (this.Machine.Id);

        foreach (var extension in GetActivityAnalysisExtensions ()) {
          SetActive ();
          extension.BeforeProcessingNewActivityPeriod (machineStatus);
        }

        if (false == activityModification.ProcessAssociation (machineStatus)) {
          SetActive ();
          if (log.IsDebugEnabled) {
            log.Debug ("ProcessNewActivityPeriod: it was partially processed and the observation state slots must be reloaded");
          }
          // Note: when this note was written,
          //       if false is returned, only a NoData period until activityBegin was processed
          return false;
        }

        foreach (var extension in GetActivityAnalysisExtensions ()) {
          SetActive ();
          extension.AfterProcessingNewActivityPeriod (activityRange,
                                                      machineMode,
                                                      machineStateTemplate, machineObservationState,
                                                      shift);
        }
        SetActive ();

        if (log.IsDebugEnabled) {
          log.Debug ($"ProcessNewActivityPeriod: ok for {machineMode?.Id} in {activityRange}");
        }

        return true;
      }
    }
    #endregion // ManageActivities part

    #endregion // Methods

    #region Implementation of IContext
    /// <summary>
    /// <see cref="IContext{TContext}"/>
    /// </summary>
    /// <param name="state"></param>
    public void SwitchTo (IState<IMonitoredMachineActivityAnalysis> state)
    {
      m_stateMachine.SwitchTo (state);
    }
    #endregion // Implementation of IContext
  }
}
