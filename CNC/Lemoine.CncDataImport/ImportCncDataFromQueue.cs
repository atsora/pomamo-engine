// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Lemoine.Cnc.Data;
using Lemoine.CncDataImport.Cache;
using Lemoine.Collections;
using Lemoine.Core.ExceptionManagement;
using Lemoine.Info;
using Lemoine.Info.ConfigReader;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Threading;
using Lemoine.Core.Log;
using System.Data;
using System.Linq;

namespace Lemoine.CncDataImport
{
  /// <summary>
  /// Description of ImportCncDataFromQueue.
  /// 
  /// Note on transactions: because Fact, CncValue and MachineModuleActivity are only updated by this service,
  ///                       read-committed transaction are sufficient
  /// </summary>
  public partial class ImportCncDataFromQueue
    : ProcessOrThreadClass
    , IProcessOrThreadClass, IChecked
  {
    static readonly TimeSpan DEFAULT_SLEEP = TimeSpan.FromSeconds (2); // Default sleep time, once all the work is done
    const int DEFAULT_FETCH_DATA_NUMBER = 60; // 60 datas ~= 2 minutes
    static readonly TimeSpan DEFAULT_WHICHEVER_NB_OF_DATA_PROCESS_AFTER = TimeSpan.FromMinutes (1);
    static readonly TimeSpan DEFAULT_BREAK_FREQUENCY = TimeSpan.FromSeconds (2); // Every 2 seconds by default, take a small break
    static readonly TimeSpan DEFAULT_BREAK_TIME = TimeSpan.FromMilliseconds (100); // The break is by default 100ms
    static readonly TimeSpan DEFAULT_CLEAN_DETECTIONS_FREQUENCY = TimeSpan.FromDays (1); // Clean the detections only every day
    static readonly TimeSpan DEFAULT_CLEAN_DETECTIONS_PARAMETER = TimeSpan.FromDays (30); // Keep 30 days of detections
    static readonly TimeSpan DEFAULT_VISIT_MACHINE_MODES_EVERY = TimeSpan.FromSeconds (8); // At least every 8s, in case of MultiXXXCncDataQueue, visit the machine modes
    const int DEFAULT_MIN_NUMBER_OF_DATA_TO_PROCESS = 1;

    static readonly string TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_KEY = "CncData.TemporaryWithDelayException.Sleep";
    static readonly TimeSpan TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_DEFAULT = TimeSpan.FromSeconds (5);

    static readonly string MAX_TRY_ATTEMPT_KEY = "Cnc.DataImport.MachineMode.MaxTryAttempt";
    static readonly int MAX_TRY_ATTEMPT_DEFAULT = 3;

    #region Members
    bool m_disposed = false;
    int? m_notEnoughDataFirstQueueIndex = null; // For MultiXXXCncDataQueue only
    internal ICncDataQueue m_cncDataQueue = null;
    readonly CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource ();
    DateTime m_lastBreakDateTime = DateTime.UtcNow;
    DateTime m_lastCleanDetections = new DateTime (0, DateTimeKind.Utc);
    readonly IDictionary<ExchangeDataCommand, IImportData> m_importData = new Dictionary<ExchangeDataCommand, IImportData> ();
    #endregion // Members

    readonly ILog log; // Defined in constructor

    #region Getters / Setters
    /// <summary>
    /// Sleep time once all the work is done
    /// Default is 2s
    /// </summary>
    public TimeSpan SleepTime { get; set; }

    /// <summary>
    /// Frequency at which a break is done
    /// Default is 2s
    /// </summary>
    public TimeSpan BreakFrequency { get; set; }

    /// <summary>
    /// Break time
    /// Default is 100ms
    /// </summary>
    public TimeSpan BreakTime { get; set; }

    /// <summary>
    /// Number of data to fetch in the same time
    /// Default is 60
    /// </summary>
    public int FetchDataNumber { get; set; }

    /// <summary>
    /// Minimal number of data to process in the same time
    /// Default is 1
    /// </summary>
    public int MinNbOfDataToProcess { get; set; }

    /// <summary>
    /// Time after which a data must be processed whichever number of data there is in the queue
    /// Default is 1 min
    /// </summary>
    public TimeSpan WhicheverNbOfDataProcessAfter { get; set; }

    /// <summary>
    /// Duration after which machine mode data must be computed again
    /// Default is 8s
    /// </summary>
    public TimeSpan VisitMachineModesEvery { get; set; }

    /// <summary>
    /// Machine module (read only)
    /// </summary>
    public IMachineModule MachineModule { get; private set; }

    /// <summary>
    /// Use it only to force the use of a kind of Queue (for test purposes)
    /// </summary>
    public string Type { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule"></param>
    public ImportCncDataFromQueue (IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != machineModule.MonitoredMachine);

      Type = null;
      MachineModule = machineModule;

      // Load configuration
      SleepTime = ConfigSet.LoadAndGet<TimeSpan> ("Sleep", DEFAULT_SLEEP);
      BreakFrequency = ConfigSet.LoadAndGet<TimeSpan> ("BreakFrequency", DEFAULT_BREAK_FREQUENCY);
      FetchDataNumber = ConfigSet.LoadAndGet<int> ("FetchDataNumber", DEFAULT_FETCH_DATA_NUMBER);
      VisitMachineModesEvery = ConfigSet.LoadAndGet<TimeSpan> ("VisitMachineModesEvery", DEFAULT_VISIT_MACHINE_MODES_EVERY);
      MinNbOfDataToProcess = ConfigSet.LoadAndGet<int> ("MinNbOfDataToProcess", DEFAULT_MIN_NUMBER_OF_DATA_TO_PROCESS);
      WhicheverNbOfDataProcessAfter = ConfigSet.LoadAndGet<TimeSpan> ("WhicheverNbOfDataProcessAfter", DEFAULT_WHICHEVER_NB_OF_DATA_PROCESS_AFTER);
      BreakTime = ConfigSet.LoadAndGet<TimeSpan> ("BreakTime", DEFAULT_BREAK_TIME);

      log = LogManager.GetLogger (string.Format ("{0}.{1}.{2}",
                                                 typeof (ImportCncDataFromQueue).FullName,
                                                 machineModule.MonitoredMachine.Id,
                                                 machineModule.Id));

      // Set the parent process Id
      try {
        base.ParentProcessId = Process.GetCurrentProcess ().Id;
      }
      catch (Exception ex) {
        log.Error ($"ImportCncDataFromQueue: error while getting the parent process Id", ex);
      }

      // Data import
      m_importData[ExchangeDataCommand.MachineMode] = new ImportDataMachineMode (machineModule.MonitoredMachine);
      m_importData[ExchangeDataCommand.Stamp] = new ImportDataStamp (machineModule);
      m_importData[ExchangeDataCommand.SequenceMilestone] = new ImportDataSequenceMilestone (machineModule);
      m_importData[ExchangeDataCommand.Action] = new ImportDataAction (machineModule);
      m_importData[ExchangeDataCommand.CncVariableSet] = new ImportDataCncVariables (machineModule);
      m_importData[ExchangeDataCommand.CncAlarm] = new ImportDataCncAlarm (machineModule);
      m_importData[ExchangeDataCommand.DetectionTimeStamp] = new ImportDataDetectionTimestamp (machineModule);
      m_importData[ExchangeDataCommand.MachineModuleActivity] = new ImportDataMachineModule (machineModule);

      // CncValue and StopCncValue share the same cache
      var cache = new CacheCncValue (machineModule);
      m_importData[ExchangeDataCommand.CncValue] = new ImportDataCncValues (machineModule, cache, this);
      m_importData[ExchangeDataCommand.StopCncValue] = new ImportDataStopCncValues (machineModule, cache);

      foreach (var importData in m_importData.Values) {
        importData.LastVisitDateTime = DateTime.UtcNow;
      }
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get the stamp file
    /// </summary>
    /// <returns></returns>
    public override string GetStampFileName ()
    {
      return string.Format ("CncDataStamp-{0}", MachineModule.Id);
    }

    /// <summary>
    /// Stop the execution of the thread
    /// </summary>
    public void Stop ()
    {
      m_cancellationTokenSource.Cancel ();
    }

    /// <summary>
    /// Run the importation
    /// </summary>
    protected override void Run (CancellationToken cancellationToken)
    {
      try {
        using (var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource (cancellationToken, m_cancellationTokenSource.Token)) {
          var linkedToken = linkedTokenSource.Token;

          while (!linkedToken.IsCancellationRequested) {
            try {
              SetActive ();
              ImportAllInQueue (linkedToken);
            }
            #region Exception management for ImportAllInQueue
            catch (OutOfMemoryException ex) {
              log.Fatal ("Run: OutOfMemoryException, give up");
              throw new OutOfMemoryException ("OutOfMemoryException in ImportAllInQueue", ex);
            }
            catch (Exception ex) {
              if (ExceptionTest.RequiresExit (ex, log)) {
                log.Fatal ($"Run: exception inner {ex.InnerException} requires to exit", ex);
                Lemoine.Core.Environment.LogAndForceExit (ex, log);
                throw new Exception ("Exception requires to exit", ex);
              }
              else if (ExceptionTest.IsUnauthorized (ex, log)) {
                log.Fatal ($"Run: read-only SQLite database, you may not run the program with the sufficient priviledge => exit", ex);
                Lemoine.Core.Environment.LogAndForceExit (ex, log);
                throw new Exception ("Read-only SQLite database", ex);
              }
              else if (ExceptionTest.IsTemporaryWithDelay (ex, log)) {
                var temporaryWithDelayExceptionSleepTime = Lemoine.Info.ConfigSet
                  .LoadAndGet (TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_KEY, TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_DEFAULT);
                log.Warn ($"Run: temporary with delay exception inner {ex.InnerException}, try again after sleeping {temporaryWithDelayExceptionSleepTime}", ex);
                this.Sleep (temporaryWithDelayExceptionSleepTime, linkedToken);
              }
              else if (ExceptionTest.IsTemporary (ex, log)) {
                log.Warn ($"Run: temporary exception inner {ex.InnerException}, try again", ex);
              }
              else if (ExceptionTest.IsStale (ex, log)) {
                log.Warn ($"Run: ImportAllInQueue with stale, try again", ex);
              }
              else if (ExceptionTest.IsTransactionSerializationFailure (ex, log)) {
                log.Info ($"Run: transaction serialization failure exception, try again");
              }
              else if (ExceptionTest.IsInvalid (ex, log)) {
                log.Error ($"Run: invalid request error inner {ex.InnerException} => give up", ex);
                throw new Exception ("Invalid exception", ex);
              }
              else {
                log.Error ("Run: ImportAllInQueue failed", ex);
              }
            }
            #endregion // Exception management for ImportAllInQueue

            // Sleep / Vaccuum (if required)
            DateTime pauseUntil = DateTime.UtcNow.Add (SleepTime);
            try {
              SetActive ();
              if (null != m_cncDataQueue && m_cncDataQueue.VacuumIfNeeded ()) {
                log.Info ("Run: Vacuum was performed");
                SetActive ();
              }
              else { // No vaccuum => check if some old detections may be removed
                try {
                  CleanDetections ();
                }
                catch (Exception ex) {
                  log.Error ("Run: CleanDetections failed but continue", ex);
                }
              }
            }
            catch (Exception ex) {
              SetActive ();
              log.Error ("Run: Vacuum failed but continue", ex);
            }

            this.SleepUntil (pauseUntil, linkedToken);
          }
        }
      }
      catch (Exception uncaughtException) {
        log.Error ($"Run: uncaught exception", uncaughtException);
        throw;
      }

      log.Info ($"Run: cancellation requested");
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
    /// Initialize queue and import all the data in the queue
    /// </summary>
    internal void ImportAllInQueue (CancellationToken cancellationToken)
    {
      while ((null == m_cncDataQueue) && !cancellationToken.IsCancellationRequested) {
        InitializeQueue (cancellationToken);
        if (null == m_cncDataQueue) {
          log.Error ("ImportAllInQueue: the queue could not be initialized, try again in a few seconds");
          this.Sleep (TimeSpan.FromSeconds (2), cancellationToken);
        }
      }
      ImportAllInInitializedQueue (cancellationToken);
    }

    /// <summary>
    /// Initialize the queue, if it has not been initialized yet
    /// </summary>
    void InitializeQueue (CancellationToken cancellationToken)
    {
      if (null == m_cncDataQueue) {
        // Configuration of the queue
        var defaultConfigReader = new MemoryConfigReader ();
        defaultConfigReader.Add ("ReceiveOnly", true);

        try {
          var localSubDirectory = "CncData." + this.MachineModule.MonitoredMachine.Id + "." + this.MachineModule.Id;
          var queueConfFull = new QueueConfigurationFull (Lemoine.Extensions.ExtensionManager.ExtensionsProvider, this.MachineModule.MonitoredMachine.Id, this.MachineModule.Id, localSubDirectory, true, true);
          m_cncDataQueue = queueConfFull.CreateQueue (this.MachineModule.MonitoredMachine.Id,
                                                      this.MachineModule.Id,
                                                      defaultConfigReader);
        }
        catch (Exception ex) {
          log.Error ("InitializeQueue: the queue could not be created probably the configuration could not be loaded", ex);
          Debug.Assert (null == m_cncDataQueue);
          return;
        }
        if (m_cncDataQueue is ICheckedCaller checkedCaller) {
          checkedCaller.SetCheckedCaller (this);
        }
      }
    }

    /// <summary>
    /// Import all the data in the initialized queue
    /// </summary>
    void ImportAllInInitializedQueue (CancellationToken cancellationToken)
    {

      m_lastBreakDateTime = DateTime.UtcNow;

      while (!cancellationToken.IsCancellationRequested) {
        SetActive ();
        TakeBreak ();
        IList<ExchangeData> queueDatas;
        try {
          queueDatas = m_cncDataQueue.Peek (FetchDataNumber);
        }
        catch (Exception ex) {
          log.Error ("ImportAllInInitializedQueue: exception occurred", ex);
          throw;
        }
        SetActive ();

        if (0 == queueDatas.Count) {
          log.Debug ("ImportAllInInitializedQueue: no data was retrieved from the queue");
          return;
        }

        if ((queueDatas.Count < MinNbOfDataToProcess)
            && (queueDatas[0].DateTime <= DateTime.UtcNow)
            && (DateTime.UtcNow.Subtract (queueDatas[0].DateTime) < WhicheverNbOfDataProcessAfter)) {
          log.InfoFormat ("ImportAllInInitializedQueue: " +
                          "because the minimum of data {0} is not reached ({1} datas) " +
                          "and the oldest date {2} is not old enough (limit age is {3}) " +
                          "=> do not process it right now",
                          MinNbOfDataToProcess, queueDatas.Count,
                          queueDatas[0].DateTime, WhicheverNbOfDataProcessAfter);
          if (m_cncDataQueue is IMultiCncDataQueue multiQueue) {
            Debug.Assert (null != multiQueue);
            log.DebugFormat ("ImportAllInInitializedQueue: " +
                             "firstQueueIndex={0} currentQueueIndex={1}",
                             m_notEnoughDataFirstQueueIndex, multiQueue.CurrentQueueIndex);
            if (!m_notEnoughDataFirstQueueIndex.HasValue) {
              m_notEnoughDataFirstQueueIndex = multiQueue.CurrentQueueIndex;
            }
            else if (m_notEnoughDataFirstQueueIndex.Value == multiQueue.CurrentQueueIndex) {
              // Back to the beginning: no data to process, sleep
              m_notEnoughDataFirstQueueIndex = null;
              return;
            }
            multiQueue.MoveNextQueue ();
            continue;
          }
          else {
            return;
          }
        }
        m_notEnoughDataFirstQueueIndex = null;

        IList<ExchangeData> datas = new List<ExchangeData> ();
        foreach (ExchangeData data in queueDatas) {
          SetActive ();
          // Check the data is compatible with datas
          if (IsDataCompatible (datas, data)) {
            datas.Add (data);
          }
          else { // There is enough data in the list, let's process them
            Debug.Assert (0 < datas.Count);
            break;
          }
        }

        // Note because there is at least one data in queueDatas and one data compatible with an empty list
        // datas can't be empty
        Debug.Assert (0 < datas.Count);
        if (0 == datas.Count) { // No data
          log.Fatal ("ImportAllInInitializedQueue: the data list should not be empty, never");
          throw new Exception ("empty data list");
        }
        else {
          ExchangeData firstData = datas[0];
          log.DebugFormat ("ImportDatas: command={0}", firstData.Command);

          // Import data
          TryImportDatas (datas, cancellationToken);
          m_importData[firstData.Command].LastVisitDateTime = DateTime.UtcNow;

          // Dequeue
          try {
            m_cncDataQueue.UnsafeDequeue (datas.Count);
          }
          catch (Exception ex) {
            log.Error ($"ImportAllInInitializedQueue: UnsafeDequeue failed count={datas.Count} => try to fallback with a normal Dequeue", ex);
            for (int i = 0; i < datas.Count; ++i) {
              var dequeued = m_cncDataQueue.Dequeue ();
              if (!object.Equals (dequeued, datas[i])) {
                log.FatalFormat ("ImportAllInQueueInitialized: " +
                                 "the dequeued data {0} does not correspond to the previously peeked data {1} " +
                                 "=> stop dequeuing the queue",
                                 dequeued, datas[i]);
                break;
              }
            }
          }
          datas.Clear ();
        }

        if (m_cncDataQueue is IMultiCncDataQueue multiQueue1) {
          TimeSpan lastMachineModeVisitAge =
            DateTime.UtcNow.Subtract (m_importData[ExchangeDataCommand.MachineMode].LastVisitDateTime);
          if (VisitMachineModesEvery < lastMachineModeVisitAge) {
            log.InfoFormat ("ImportAllInInitializedQueue: " +
                            "last visit of machine modes was {0} ago, " +
                            "which is more than limit {1} " +
                            "=> give the priority to the machine mode " +
                            "and reset the queue in case of IMultiCncDataQueue",
                            lastMachineModeVisitAge,
                            VisitMachineModesEvery);
            m_importData[ExchangeDataCommand.MachineMode].LastVisitDateTime = DateTime.UtcNow;
            Debug.Assert (null != multiQueue1);
            multiQueue1.Reset ();
          }
        }
      }
    }

    void TryImportDatas (IList<ExchangeData> datas, CancellationToken cancellationToken = default, int attempt = 0)
    {
      try {
        var firstData = datas.First ();
        m_importData[firstData.Command].ImportDatas (datas, cancellationToken);
      }
      catch (Exception ex) {
        if (ExceptionTest.IsTemporaryWithDelay (ex)) {
          if (log.IsInfoEnabled) {
            log.Info ($"TryImportMachineMode: temporary with delay exception", ex);
          }
          var waitTime = Lemoine.Info.ConfigSet
            .LoadAndGet (TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_KEY, TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_DEFAULT);
          this.Sleep (waitTime, cancellationToken);
        }
        else if (ExceptionTest.IsTemporary (ex)) {
          if (log.IsInfoEnabled) {
            log.Info ($"TryImportMachineMode: temporary exception", ex);
          }
        }
        else {
          log.Error ($"TryImportMachineMode: not a temporary exception, throw it", ex);
          throw;
        }
        cancellationToken.ThrowIfCancellationRequested ();
        var newAttempt = attempt + 1;
        var maxAttempt = Lemoine.Info.ConfigSet
          .LoadAndGet (MAX_TRY_ATTEMPT_KEY, MAX_TRY_ATTEMPT_DEFAULT);
        if (newAttempt <= maxAttempt) {
          if (log.IsInfoEnabled) {
            log.Info ($"TryImportMachineMode: attempt={newAttempt} did not reach {maxAttempt} => try again");
            TryImportDatas (datas, cancellationToken, newAttempt);
          }
        }
        else {
          if (log.IsWarnEnabled) {
            log.Warn ($"TryImportMachineMode: maxAttempt={maxAttempt} reached => give up");
          }
          throw;
        }
      }
    }

    /// <summary>
    /// Sleep from time to time to leave some resources to the Cnc Service
    /// </summary>
    void TakeBreak ()
    {
      // Every two seconds, sleep 100 ms
      if (BreakFrequency <= DateTime.UtcNow.Subtract (m_lastBreakDateTime)) {
        this.Sleep (BreakTime);
        m_lastBreakDateTime = DateTime.UtcNow;
        SetActive ();
      }
    }

    /// <summary>
    /// Check whether the new data is compatible with an existing list of datas
    /// </summary>
    /// <param name="datas"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    bool IsDataCompatible (IList<ExchangeData> datas, ExchangeData data)
    {
      // Empty list => compatible
      if (datas.Count == 0) {
        return true;
      }

      ExchangeData lastData = datas[datas.Count - 1];

      // The targeted machine module must be the same so that the data is compatible
      if (!object.Equals (lastData.MachineModuleId, data.MachineModuleId)) {
        log.Debug ("IsDataCompatible: the targeted machine module is different => not compatible");
        return false;
      }

      // Check it is the same command
      if (!lastData.Command.Equals (data.Command)) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("IsDataCompatible: " +
                           "the new data {0} does not contain the same command than {1} " +
                           "=> not compatible",
                           data, lastData);
        }
        return false;
      }

      // Check it concerns the same kind of data, and check the gap if necessary
      return m_importData[data.Command].IsMergeable (data, lastData);
    }

    void CleanDetections ()
    {
      if (DEFAULT_CLEAN_DETECTIONS_FREQUENCY < DateTime.UtcNow.Subtract (m_lastCleanDetections)) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("CleanDetections: " +
                           "clean the detections because the last process was at {0} (older than {1})",
                           m_lastCleanDetections, DEFAULT_CLEAN_DETECTIONS_FREQUENCY);
        }
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          // - Get MachineModuleAnalysisStatus
          IMachineModuleAnalysisStatus analysisStatus;
          using (IDAOTransaction transaction = session.BeginTransaction ("CncData.CleanDetections.GetMachineModuleAnalysisStatus",
                                                                         TransactionLevel.ReadCommitted)) {
            analysisStatus = ModelDAOHelper.DAOFactory.MachineModuleAnalysisStatusDAO
              .FindById (MachineModule.Id);
            transaction.Commit ();
          }
          if (null == analysisStatus) {
            log.WarnFormat ("CleanDetections: " +
                            "no analysis status for machine module id={0}",
                            MachineModule.Id);
          }
          else {
            // - Delete the machine module detections that are older than 30 days and that are not processed yet
            using (IDAOTransaction transaction = session.BeginTransaction ("CncData.CleanDetections.Clean",
                                                                           TransactionLevel.ReadUncommitted))
            // Note: In current PostgreSQL implementation, same as ReadCommitted
            {
              transaction.SynchronousCommitOption = SynchronousCommit.Off;
              ModelDAOHelper.DAOFactory.MachineModuleDetectionDAO.Clean (MachineModule,
                                                                         analysisStatus.LastMachineModuleDetectionId,
                                                                         DEFAULT_CLEAN_DETECTIONS_PARAMETER);
              transaction.Commit ();
            }
          }
        }
        m_lastCleanDetections = DateTime.UtcNow;
      }
    }
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
        m_cncDataQueue?.Dispose ();
        m_cancellationTokenSource.Dispose ();
      }

      m_disposed = true;

      base.Dispose (disposing);
    }
    #endregion // IDisposable implementation
  }
}
