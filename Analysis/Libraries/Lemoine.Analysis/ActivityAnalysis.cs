// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Lemoine.Core.ExceptionManagement;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Threading;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
using Lemoine.Collections;
using Lemoine.Core.Performance;

namespace Lemoine.Analysis
{
  /// <summary>
  /// This class takes care of the activity analysis
  /// for all the machine modules
  /// 
  /// It may be run on a dedicated thread
  /// </summary>
  public class ActivityAnalysis : ThreadClass, IThreadClass, IActivityAnalysis
  {
    /// <summary>
    /// Run the global analysis in a thread pool: key
    /// </summary>
    static readonly string RUN_GLOBAL_ANALYSIS_IN_THREAD_POOL_KEY = "Analysis.Global.RunInThreadPool";
    /// <summary>
    /// Run the global analysis in a thread pool: default value
    /// </summary>
    static readonly bool RUN_GLOBAL_ANALYSIS_IN_THREAD_POOL_DEFAULT = false;
    /// <summary>
    /// Time to wait for the machine analysis completion: key
    /// </summary>
    static readonly string GLOBAL_ANALYSIS_MACHINE_ANALYSIS_COMPLETION_WAIT_KEY = "Analysis.Global.MachineAnalysisCompletionWait";
    /// <summary>
    /// Time to wait for the machine analysis completion: default value
    /// </summary>
    static readonly TimeSpan GLOBAL_ANALYSIS_MACHINE_ANALYSIS_COMPLETION_WAIT_DEFAULT = TimeSpan.FromSeconds (45);
    /// <summary>
    /// Time to wait when the maximum number of running monitored machine analysis threads is reached: key
    /// </summary>
    static readonly string SLEEP_MAX_RUNNING_MACHINE_THREADS_KEY = "Analysis.Machine.SleepMaxRunningThreads";
    /// <summary>
    /// Time to wait when the maximum number of running monitored machine analysis threads is reached: default value
    /// </summary>
    static readonly TimeSpan SLEEP_MAX_RUNNING_MACHINE_THREADS_DEFAULT = TimeSpan.FromMilliseconds (100);
    /// <summary>
    /// Maximum time to wait for a free running monitored analysis thread: key
    /// </summary>
    static readonly string MAX_WAIT_MACHINE_THREAD_KEY = "Analysis.Machine.MaxWaitThread";
    /// <summary>
    /// Maximum time to wait for a free running monitored analysis thread: default value
    /// </summary>
    static readonly TimeSpan MAX_WAIT_MACHINE_THREAD_DEFAULT = TimeSpan.FromMinutes (3);
    /// <summary>
    /// Time to wait when the memory limit is reached before adding more threads: key
    /// </summary>
    static readonly string SLEEP_FREE_MEMORY_KEY = "Analysis.Machine.SleepFreeMemory";
    /// <summary>
    /// Time to wait when the memory limit is reached before adding more threads: default value
    /// </summary>
    static readonly TimeSpan SLEEP_FREE_MEMORY_DEFAULT = TimeSpan.FromMilliseconds (100);
    /// <summary>
    /// Maximum time to wait after a memory limit is detected before adding more threads: key
    /// </summary>
    static readonly string MAX_WAIT_FREE_MEMORY_KEY = "Analysis.Machine.MaxWaitFreeMemory";
    /// <summary>
    /// Maximum time to wait after a memory limit is detected before adding more threads: default value
    /// </summary>
    static readonly TimeSpan MAX_WAIT_FREE_MEMORY_DEFAULT = TimeSpan.FromMinutes (3);

    /// <summary>
    /// Not responding timeout in CheckThreads
    /// </summary>
    static readonly string NOT_RESPONDING_TIMEOUT_KEY = "Analysis.Activity.NotRespondingTimeout";
    static readonly TimeSpan NOT_RESPONDING_TIMEOUT_DEFAULT = TimeSpan.FromHours (1);
    // Note: in some cases, 7 minutes is too short. There must be processes when SetActive is not set as often as it should

    static readonly string FILL_ANALYSIS_STATUS_IN_THREAD_POOL_KEY = "Analysis.FillAnalysisStatus.InThreadPool";
    static readonly bool FILL_ANALYSIS_STATUS_IN_THREAD_POOL_DEFAULT = true;

    static readonly string FILL_ANALYSIS_STATUS_COMPLETION_WAIT_KEY =
      "Analysis.FillAnalysisStatus.CompletionWait";
    static readonly TimeSpan FILL_ANALYSIS_STATUS_COMPLETION_WAIT_DEFAULT = TimeSpan.FromSeconds (10);

    static readonly string TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_KEY = "Analysis.Activity.TemporaryWithDelayException.Sleep";
    static readonly TimeSpan TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_DEFAULT = TimeSpan.FromSeconds (2);

    #region Members
    bool m_disposed = false;
    bool m_initialized = false;
    readonly ConfigUpdateChecker m_configUpdateChecker = new ConfigUpdateChecker ();
    readonly GlobalAnalysis m_globalAnalysis;
    bool m_machineAnalysisCompleted = true;
    readonly IDictionary<int, MachineActivityAnalysis> m_machineAnalysisList =
      new Dictionary<int, MachineActivityAnalysis> ();
    readonly CheckThreadsAndProcesses m_checkThreads = new CheckThreadsAndProcesses ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ActivityAnalysis).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ActivityAnalysis ()
    {
      m_globalAnalysis = new GlobalAnalysis (this);
    }

    void Initialize ()
    {
      RegisterCancelCallback (m_globalAnalysis.Interrupt);

      // m_globalAnalysis
      m_checkThreads.AddThread (m_globalAnalysis);
      RegisterCancelCallback (m_checkThreads.Interrupt);

      // Options
      var includeMachines = Lemoine.Info.ConfigSet.LoadAndGet ($"Analysis.IncludeMachines", "");
      var includeMachineIds = Lemoine.Collections.EnumerableString.ParseListString<int> (includeMachines);
      var excludeMachines = Lemoine.Info.ConfigSet.LoadAndGet ($"Analysis.ExcludeMachines", "");
      var excludeMachineIds = Lemoine.Collections.EnumerableString.ParseListString<int> (excludeMachines);

      // MachineActivityAnalysis
      // m_analysisList, by monitored machine
      IEnumerable<IMonitoredMachine> monitoredMachines;
      IEnumerable<IMachine> notMonitoredMachines;
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        monitoredMachines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindAllWithMachineModule ()
          .Where (m => m.MonitoringType.Id != (int)Lemoine.Model.MachineMonitoringTypeId.Obsolete); // Exclude the obsolete machines
        IEnumerable<IMachine> allMachines = ModelDAOHelper.DAOFactory.MachineDAO
          .FindAll ()
          .Where (m => m.MonitoringType.Id != (int)Lemoine.Model.MachineMonitoringTypeId.Obsolete); // Exclude the obsolete machines
        IEnumerable<int> monitoredMachineIds =
          monitoredMachines.Select<IMonitoredMachine, int> (monitoredMachine => monitoredMachine.Id);
        notMonitoredMachines = allMachines.Where (machine => !monitoredMachineIds.Contains (machine.Id));
      }
      if (includeMachineIds.Any ()) {
        monitoredMachines = monitoredMachines
          .Where (m => includeMachineIds.Contains (m.Id));
        notMonitoredMachines = notMonitoredMachines
          .Where (m => includeMachineIds.Contains (m.Id));
      }
      if (excludeMachineIds.Any ()) {
        monitoredMachines = monitoredMachines
          .Where (m => !excludeMachineIds.Contains (m.Id));
        notMonitoredMachines = notMonitoredMachines
          .Where (m => !excludeMachineIds.Contains (m.Id));
      }
      log.Info ($"ActivityAnalysis: start ActivityAnalysis with {monitoredMachines.Count ()} monitored machines");
      foreach (IMonitoredMachine monitoredMachine in monitoredMachines) {
        if (log.IsDebugEnabled) {
          log.Debug ($"ActivityAnalysis: initialize machine {monitoredMachine.Id}");
        }
        var monitoredMachineActivityAnalysis =
          new MonitoredMachineActivityAnalysis (monitoredMachine);
        m_machineAnalysisList.Add (monitoredMachine.Id, monitoredMachineActivityAnalysis);
        RegisterCancelCallback (monitoredMachineActivityAnalysis.Interrupt);
        m_checkThreads.AddThread (monitoredMachineActivityAnalysis);
      }
      foreach (IMachine notMonitoredMachine in notMonitoredMachines) {
        if (log.IsDebugEnabled) {
          log.Debug ($"ActivityAnalysis: initialize machine {notMonitoredMachine.Id}");
        }
        var machineActivityAnalysis =
          new MachineActivityAnalysis (notMonitoredMachine);
        m_machineAnalysisList.Add (notMonitoredMachine.Id, machineActivityAnalysis);
        RegisterCancelCallback (machineActivityAnalysis.Interrupt);
        m_checkThreads.AddThread (machineActivityAnalysis);
      }

      m_initialized = true;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Request a pause in the activity analysis
    /// </summary>
    /// <param name="machine">Not null</param>
    /// <param name="modificationId"></param>
    /// <returns>The pause could be set</returns>
    public bool RequestPause (IMachine machine, long modificationId)
    {
      Debug.Assert (null != machine);
      if (!machine.IsMonitored ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"RequestPause: the machine id {machine.Id} is not monitored, return true at once");
        }
        return true;
      }
      MachineActivityAnalysis machineActivityAnalysis;
      try {
        machineActivityAnalysis =
          GetMachineActivityAnalysis (machine);
      }
      catch (OperationCanceledException ex) {
        log.Error ("RequestPause: OperationCanceledException", ex);
        throw;
      }
      catch (Lemoine.Threading.AbortException ex) {
        log.Error ("RequestPause: AbortException", ex);
        throw;
      }
      catch (Exception ex) {
        log.Error ($"RequestPause: could not get the activity analysis that corresponds to machine id {machine.Id} => return false", ex);
        return false;
      }
      return machineActivityAnalysis.RequestPause (modificationId);
    }

    /// <summary>
    /// Release a pause in the activity analysis
    /// </summary>
    /// <param name="machine">Not null</param>
    /// <param name="modificationId"></param>
    public void ReleasePause (IMachine machine, long modificationId)
    {
      Debug.Assert (null != machine);
      if (!machine.IsMonitored ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"ReleasePause: the machine id {machine.Id} is not monitored, return at once");
        }
        return;
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"ReleasePause: about to release the pause for machine id {machine.Id}");
      }
      MachineActivityAnalysis machineActivityAnalysis =
        GetMachineActivityAnalysis (machine);
      machineActivityAnalysis.ReleasePause (modificationId);
    }

    /// <summary>
    /// Check if the activity analysis is already in pause
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="modificationId"></param>
    /// <returns></returns>
    public bool IsInPause (IMachine machine, int modificationId)
    {
      Debug.Assert (null != machine);
      if (!machine.IsMonitored ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsInPause: the machine id {machine.Id} is not monitored, return at once true");
        }
        return true;
      }
      MachineActivityAnalysis machineActivityAnalysis =
        GetMachineActivityAnalysis (machine);
      return machineActivityAnalysis.IsInPause (modificationId);
    }

    /// <summary>
    /// Wait for the pause of the activity analysis
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="modificationId"></param>
    /// <param name="cancellationToken"></param>
    public void WaitPause (IMachine machine, long modificationId, CancellationToken cancellationToken)
    {
      Debug.Assert (null != machine);
      if (!machine.IsMonitored ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"WaitPause: the machine id {machine.Id} is not monitored, return at once");
        }
        return;
      }
      MachineActivityAnalysis machineActivityAnalysis =
        GetMachineActivityAnalysis (machine);
      DateTime now = DateTime.UtcNow;
      while (!machineActivityAnalysis.IsInPause (modificationId)) {
        SetActive ();
        if (log.IsDebugEnabled) {
          log.Debug ("WaitPause: the pause has not been set yet");
        }
        if (TimeSpan.FromSeconds (2) <= DateTime.UtcNow.Subtract (now)) {
          // After 2 seconds, raise a warning
          log.Warn ($"WaitPause: waiting pause for already {DateTime.UtcNow.Subtract (now)}");
        }
        cancellationToken.WaitHandle.WaitOne (100); // 100ms
      }
    }

    MachineActivityAnalysis GetMachineActivityAnalysis (IMachine machine)
    {
      MachineActivityAnalysis machineActivityAnalysis = null;
      if (!m_machineAnalysisList.TryGetValue (machine.Id, out machineActivityAnalysis)) {
        log.Fatal ($"GetMachineActivityAnalysis: monitored machine activity analysis {machine.Id} could not be found");
        Debug.Assert (false);
        throw new InvalidOperationException ();
      }
      Debug.Assert (null != machineActivityAnalysis);
      return machineActivityAnalysis;
    }

    /// <summary>
    /// Run the activity analysis and loop
    /// </summary>
    protected override void Run (CancellationToken cancellationToken)
    {
      if (!m_initialized) {
        this.Initialize ();
      }

      m_configUpdateChecker.Initialize ();

      TimeSpan notRespondingTimeout = Lemoine.Info.ConfigSet
            .LoadAndGet<TimeSpan> (NOT_RESPONDING_TIMEOUT_KEY,
                                   NOT_RESPONDING_TIMEOUT_DEFAULT);
      try {
        while (!cancellationToken.IsCancellationRequested) {
          SetActive ();
          try {
            m_checkThreads.NotRespondingTimeout = notRespondingTimeout;
            m_checkThreads.StartIfUnstarted (cancellationToken);
            RunAnalysis (cancellationToken);
            log.Fatal ("Run: RunAnalysis should loop and never return ! => try again in a few seconds");
          }
          catch (OperationCanceledException ex) {
            log.Info ("Run: OperationCanceledException", ex);
            throw;
          }
          catch (ThreadAbortException ex) {
            // Try to make the thread abort faster, interrupting all the threads in the thread pool earlier
            log.Info ("Run: thread abort exception", ex);
            throw new Lemoine.Threading.AbortException ("ThreadAbortException raised.", ex);
          }
          catch (Lemoine.Threading.AbortException ex) {
            // Try to make the thread abort faster, interrupting all the threads in the thread pool earlier
            log.Info ("Run: AbortException", ex);
            SetExitRequested ();
            throw;
          }
          catch (OutOfMemoryException ex) {
            log.Error ("Run: OutOfMemoryException, give up", ex);
            SetExitRequested ();
            return; // Do not throw an exception here because it is the main method of a thread, it looks to be safer
          }
          catch (Exception ex) {
            if (ExceptionTest.IsStale (ex, log)) {
              log.Info ("Run: Stale exception => try again", ex);
            }
            else if (ExceptionTest.IsTransactionSerializationFailure (ex, log)) {
              log.Info ("Run: transaction serialization failure => try again", ex);
            }
            else if (ExceptionTest.IsTemporaryWithDelay (ex, log)) {
              var temporaryWithDelayExceptionSleepTime = Lemoine.Info.ConfigSet
                .LoadAndGet (TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_KEY, TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_DEFAULT);
              log.Warn ($"Run: temporary with delay exception inner {ex.InnerException}, try again after sleeping {temporaryWithDelayExceptionSleepTime}", ex);
              this.Sleep (temporaryWithDelayExceptionSleepTime, cancellationToken);
            }
            else if (ExceptionTest.IsTemporary (ex, log)) { // Usually timeout
              log.Warn ($"Run: exception with inner exception {ex.InnerException} is temporary => try again", ex);
            }
            else if (ExceptionTest.IsInvalid (ex, log)) {
              log.Error ($"Run: exception with an invalid request inner {ex.InnerException} but continue", ex);
            }
            else if (ExceptionTest.RequiresExit (ex, log)) {
              // OutOfMemory or NHibernate.TransactionException which is not temporary
              // (TransactionSerializationFailure or TimeoutFailure)
              if (!(ex is OutOfMemoryException)) {
                log.Error ($"Run: Exception with inner exception {ex.InnerException} requires to exit, give up", ex);
              }
              SetExitRequested ();
              return;
            }
            else {
              log.Fatal ("Run: unexpected error => try again in a few seconds", ex);
            }
          }
          cancellationToken.WaitHandle.WaitOne (TimeSpan.FromSeconds (30));
        } // While
      }
      catch (OperationCanceledException ex) {
        log.Warn ("Run: operation canceled exception", ex);
      }
      catch (Lemoine.Threading.AbortException ex) {
        // Try to make the thread abort faster, interrupting all the threads in the thread pool earlier
        log.Error ("Run: abort exception => exit requested", ex);
        SetExitRequested ();
        try {
          if (!m_checkThreads.Abort ()) {
            log.Error ("Run: abort of checkThreads failed");
          }
        }
        catch (Exception ex1) {
          log.Error ("Run: abort of m_checkThreads failed", ex1);
        }

        try {
          if (!m_globalAnalysis.Abort ()) {
            log.Error ("Run: abort of global analysis failed");
          }
        }
        catch (Exception ex1) {
          log.Error ($"Run: abort of global analysis failed", ex1);
        }
        foreach (var machineAnalysis in m_machineAnalysisList.Values) {
          try {
            if (!machineAnalysis.Abort ()) {
              log.Error ("Run: abort of machine analysis failed");
            }
          }
          catch (Exception ex1) {
            log.Error ("Run: abort of one machine analysis failed", ex1);
          }
        }
        if (!this.Abort ()) {
          log.Error ("Run: final abort failed");
        }
      }
      catch (OutOfMemoryException ex) {
        log.Error ("Run: OutOfMemoryException, give up", ex);
        SetExitRequested ();
        return; // Do not throw an exception here because it is the main method of a thread, it looks to be safer
      }
      catch (Exception ex) {
        log.Exception (ex, "Run: unother exception => Cancel");
        if (!this.Cancel ()) {
          log.Error ("Run: Cancel failed");
        }
      }

      log.Info ("Run: cancellation requested");
      cancellationToken.ThrowIfCancellationRequested ();
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
    /// Run the Activity Analysis on all the machine modules once
    /// </summary>
    void RunAnalysis (CancellationToken cancellationToken)
    {
      int workerThreads;
      int completionPortThreads;

      ThreadPool.GetMaxThreads (out workerThreads, out completionPortThreads);
      if (log.IsInfoEnabled) {
        log.InfoFormat ("RunAnalysis: " +
                        "default workThreads={0} completionPortThreads={1}",
                        workerThreads, completionPortThreads);
      }
      int maxThreads = AnalysisConfigHelper.MaxThreadsInPool;
      if (maxThreads < workerThreads) {
        if (log.IsInfoEnabled) {
          log.InfoFormat ("RunAnalysis: " +
                          "decrease the number of workThreads from {0} to {1}",
                          workerThreads, maxThreads);
        }
        workerThreads = maxThreads;
      }
      if (maxThreads < completionPortThreads) {
        if (log.IsInfoEnabled) {
          log.InfoFormat ("RunAnalysis: " +
                          "decrease the number of completionPortThreads from {0} to {1}",
                          completionPortThreads, maxThreads);
        }
        completionPortThreads = maxThreads;
      }
      if (!ThreadPool.SetMaxThreads (workerThreads, completionPortThreads)) {
        log.Error ("RunAnalysis: SetMaxThreads failed");
      }

      m_globalAnalysis.FirstRun = true;
      long memoryLimitExit = (long)(Lemoine.Info.ComputerInfo.TotalPhysicalMemory / 100)
        * AnalysisConfigHelper.MemoryPercentageExit;
      while (!cancellationToken.IsCancellationRequested) {
        RunAnalysisOnce (cancellationToken, memoryLimitExit);
      }
    }

    void RunAnalysisOnce (CancellationToken cancellationToken, long memoryLimitExit)
    {
      DateTime startDateTime = DateTime.UtcNow;

      if (m_checkThreads.ExitRequested) {
        log.Fatal ("RunAnalysisOnce: exit requested from checkThreads");
        SetExitRequested ();
        throw new Lemoine.Threading.AbortException ("Exit was requested by CheckThreads.");
      }

      if (memoryLimitExit < Lemoine.Info.ProgramInfo.GetPhysicalMemory ()) {
        log.Error ($"RunAnalysisOnce: memory limit {memoryLimitExit} is reached => exit");
        SetExitRequested ();
        throw new OutOfMemoryException ("Memory limit reached, exit requested");
      }

      if (cancellationToken.IsCancellationRequested) {
        log.Error ($"RunAnalysisOnce: cancellation requested => return");
        return;
      }

      SetActive ();
      CheckNoConfigUpdate ();
      SetActive ();

      RunGlobalAnalysis (cancellationToken);
      SetActive ();

      if (cancellationToken.IsCancellationRequested) {
        log.Error ($"RunAnalysisOnce: cancellation requested => return");
        return;
      }

      if (m_checkThreads.ExitRequested || m_globalAnalysis.ExitRequested) {
        log.Fatal ("RunAnalysisOnce: exit requested from checkThreads or global analysis");
        SetExitRequested ();
        throw new Lemoine.Threading.AbortException ("Exit was requested by CheckThreads or Global Analysis.");
      }

      RunMachineAnalysis (memoryLimitExit, cancellationToken);

      if (cancellationToken.IsCancellationRequested) {
        log.Error ($"RunAnalysisOnce: cancellation requested after RunMachineAnalysis => return");
        return;
      }

      DateTime endDateTime = startDateTime.Add (AnalysisConfigHelper.ActivityAnalysisFrequency);
      this.SleepUntil (endDateTime, cancellationToken);

      SetActive ();
      if (m_checkThreads.ExitRequested) {
        log.Fatal ("RunAnalysisOnce: exit requested from checkThreads");
        SetExitRequested ();
        throw new Lemoine.Threading.AbortException ("Exit was requested by CheckThreads.");
      }

      if (!m_globalAnalysis.CanRun ()) {
        log.Warn ("RunAnalysisOnce: global analysis is still running");
      }
      CheckExitRequestedByMachineAnalysis ();
      if (log.IsDebugEnabled) {
        log.Debug ("RunAnalysisOnce: completed (run it once again)");
      }
    }

    void RunGlobalAnalysis (CancellationToken cancellationToken)
    {
      if (m_globalAnalysis.FirstRun) {
        // Run once globalAnalysis without any timeout management directly
        if (log.IsInfoEnabled) {
          log.Info ("RunGlobalAnalysis: run first globalAnalysis completely");
        }
        m_globalAnalysis.RunDirectly (cancellationToken);
        FillAnalysisStatus (cancellationToken);
        if (log.IsDebugEnabled) {
          log.Debug ($"RunGlobalAnalysis: first time completed");
        }
        m_globalAnalysis.FirstRun = false;
      }
      else { // Global analysis has already been run once completely
        if (log.IsDebugEnabled) {
          log.Debug ($"RunGlobalAnalysis: not the first time");
        }

        if (m_globalAnalysis.ExitRequested) {
          log.Fatal ("RunGlobalAnalysis: exit requested from Global Analysis");
          SetExitRequested ();
          throw new Lemoine.Threading.AbortException ("Exit is requested.");
        }

        if (m_globalAnalysis.CanRun ()) {
          // Check the machine analysis is completed before running the global analysis
          // to avoid some locks
          if (log.IsDebugEnabled) {
            log.Debug ("RunGlobalAnalysis: before running the global analysis check all the machine analysis is completed again");
          }
          if (!WaitForMachineAnalysisCompletionBeforeGlobalAnalysis (cancellationToken)) {
            if (log.IsWarnEnabled) {
              log.Warn ("RunGlobalAnalysis: the machine analysis is not completed, but the timeout was reached, continue with the global analysis");
            }
          }

          bool runInThreadPool = ConfigSet.LoadAndGet<bool> (RUN_GLOBAL_ANALYSIS_IN_THREAD_POOL_KEY,
                                                             RUN_GLOBAL_ANALYSIS_IN_THREAD_POOL_DEFAULT);
          if (log.IsDebugEnabled) {
            log.Debug ($"RunGlobalAnalysis: run a new global analysis InThreadPool={runInThreadPool}");
          }
          SetActive ();
          if (runInThreadPool) {
            m_globalAnalysis.StartInThreadPool (cancellationToken);
          }
          else {
            m_globalAnalysis.Start (cancellationToken);
          }

          FillAnalysisStatus (cancellationToken);
        } // End m_globalAnalysis.CanRun ()
      } // if(firstGlobalAnalysisRun) else
    }

    void FillAnalysisStatus (CancellationToken cancellationToken)
    {
      using (new PerfTracker ("Analysis.FillAnalysisStatus")) {
        var runInThreadPool = ConfigSet.LoadAndGet<bool> (FILL_ANALYSIS_STATUS_IN_THREAD_POOL_KEY,
                                                          FILL_ANALYSIS_STATUS_IN_THREAD_POOL_DEFAULT);
        if (log.IsDebugEnabled) {
          log.Debug ($"FillAnalysisStatus: run InThreadPool={runInThreadPool}");
        }
        foreach (MachineActivityAnalysis machineAnalysis in m_machineAnalysisList.Values) {
          SetActive ();
          if (cancellationToken.IsCancellationRequested) {
            log.Warn ($"FillAnalysisStatus: cancellation requested => return");
            return;
          }

          // To avoid as much as possible a serialization failure during the machine analysis
          if (runInThreadPool) {
            if (machineAnalysis.StartInThreadPool (machineAnalysis.FillAnalysisStatus, "FillAnalysisStatus", cancellationToken)) {
              m_machineAnalysisCompleted = false;
            }
          }
          else { // !runInThreadPool
            machineAnalysis.RunDirectly (FillAnalysisStatus, cancellationToken);
          }
        }
        SetActive ();

        if (runInThreadPool) {
          if (cancellationToken.IsCancellationRequested) {
            log.Warn ($"FillAnalysisStatus: cancellation requested => return");
            return;
          }
          if (!WaitForFillAnalysisStatusCompletion (cancellationToken)) {
            if (log.IsWarnEnabled) {
              log.WarnFormat ("FillAnalysisStatus: this is not completed, but continue");
            }
          }
        }
      }
    }

    void RunMachineAnalysis (long memoryLimitExit, CancellationToken cancellationToken)
    {
      // m_analysisList, by monitored machine
      var random = new Random ();
      var randomMachineAnalysis = m_machineAnalysisList.Values
        .OrderBy (x => random.Next ()); // Not to run the machine analysis of the same machines all the time in priority
      foreach (var machineAnalysis in randomMachineAnalysis) {
        if (memoryLimitExit < Lemoine.Info.ProgramInfo.GetPhysicalMemory ()) {
          log.Error ($"RunMachineAnalysis: memory limit {memoryLimitExit} is reached => exit");
          SetExitRequested ();
          throw new OutOfMemoryException ("Memory limit reached, exit requested");
        }

        if (machineAnalysis.ExitRequested) {
          log.Fatal ($"RunMachineAnalysis: exit requested from MachineActivityAnalysis machineId={machineAnalysis.Machine.Id}");
          SetExitRequested ();
          throw new Lemoine.Threading.AbortException ("Exit is requested.");
        }

        if (cancellationToken.IsCancellationRequested) {
          log.Warn ($"RunMachineAnalysis: cancellation requested => return");
          return;
        }

        if (machineAnalysis.CanRun ()) {
          if (log.IsDebugEnabled) {
            log.Debug ($"RunMachineAnalysis: run a new analysis on machine {machineAnalysis.Machine.Id}");
          }
          SetActive ();
          WaitFreeThreads (machineAnalysis, cancellationToken);
          SetActive ();
          WaitFreeMemory (machineAnalysis, cancellationToken);
          SetActive ();
          if (cancellationToken.IsCancellationRequested) {
            log.Warn ($"RunMachineAnalysis: cancellation requested after CanRun => return");
            return;
          }
          if (machineAnalysis.StartInThreadPool (machineAnalysis.MakeAnalysis, "MakeAnalysis", cancellationToken)) {
            m_machineAnalysisCompleted = false;
          }
        }
      }
    }

    bool CheckExitRequested (MachineActivityAnalysis machineAnalysis)
    {
      if (machineAnalysis.ExitRequested) {
        log.Fatal ($"CheckExitRequested: exit requested from MachineActivityAnalysis machineId={machineAnalysis.Machine.Id}");
        SetExitRequested ();
        throw new Lemoine.Threading.AbortException ("Exit is requested.");
      }

      return false;
    }

    /// <summary>
    /// Check the configuration was not updated
    /// </summary>
    void CheckNoConfigUpdate ()
    {
      if (!m_configUpdateChecker.Check ()) {
        log.Error ($"CheckNoConfigUpdate: the configuration was updated => exit");
        SetExitRequested ();
        throw new Lemoine.Threading.AbortException ("Configuration updated, exit");
      }
    }

    void WaitFreeThreads (MachineActivityAnalysis machineAnalysis, CancellationToken cancellationToken)
    {
      var maxRunningMachineThreads = AnalysisConfigHelper.MaxRunningMachineThreads;
      if ((maxRunningMachineThreads <= GetNumberRunningMachineAnalysisThreads ())
        && !cancellationToken.IsCancellationRequested) {
        var maxWaitTime = Lemoine.Info.ConfigSet
          .LoadAndGet<TimeSpan> (MAX_WAIT_MACHINE_THREAD_KEY, MAX_WAIT_MACHINE_THREAD_DEFAULT);
        var frequencyCheck = Lemoine.Info.ConfigSet
          .LoadAndGet<TimeSpan> (SLEEP_MAX_RUNNING_MACHINE_THREADS_KEY, SLEEP_MAX_RUNNING_MACHINE_THREADS_DEFAULT);
        if (log.IsDebugEnabled) {
          log.Debug ($"WaitFreeThreads: wait free machine threads, Max={maxRunningMachineThreads}, MaxWaitTime={maxWaitTime} => wait one running thread is finished every {frequencyCheck}");
        }
        if (this.Sleep (maxWaitTime, cancellationToken, () => CheckExitRequested (machineAnalysis) || (GetNumberRunningMachineAnalysisThreads () < maxRunningMachineThreads), frequencyCheck)) {
          log.Error ($"WaitFreeThreads: the maximum wait time {maxWaitTime} to wait for a machine analysis thread is reached");
        }
      }
    }

    void WaitFreeMemory (MachineActivityAnalysis machineAnalysis, CancellationToken cancellationToken)
    {
      long memoryLimit = (long)(Lemoine.Info.ComputerInfo.TotalPhysicalMemory / 100)
        * AnalysisConfigHelper.MemoryPercentageStopNewThreads;
      if (memoryLimit < Lemoine.Info.ProgramInfo.GetPhysicalMemory ()
        && !cancellationToken.IsCancellationRequested) {
        var maxWaitTime = Lemoine.Info.ConfigSet
          .LoadAndGet<TimeSpan> (MAX_WAIT_FREE_MEMORY_KEY, MAX_WAIT_FREE_MEMORY_DEFAULT);
        var frequencyCheck = Lemoine.Info.ConfigSet
          .LoadAndGet<TimeSpan> (SLEEP_FREE_MEMORY_KEY, SLEEP_FREE_MEMORY_DEFAULT);
        if (log.IsInfoEnabled) {
          log.Info ($"WaitFreeMemory: wait more free memory, %={AnalysisConfigHelper.MemoryPercentageStopNewThreads}, MaxWaitTime={maxWaitTime} => wait some more memory is free every {frequencyCheck}");
        }
        if (this.Sleep (maxWaitTime, cancellationToken, () => CheckExitRequested (machineAnalysis) || (Lemoine.Info.ProgramInfo.GetPhysicalMemory () <= memoryLimit), frequencyCheck)) {
          log.Error ($"WaitFreeMemory: the maximum wait time {maxWaitTime} to wait for some free memory is reached");
          if (memoryLimit < Lemoine.Info.ProgramInfo.GetPhysicalMemory ()) {
            log.Error ($"WaitFreeMemory: physical memory higher than {memoryLimit} after {maxWaitTime} => exit");
            SetExitRequested ();
            throw new OutOfMemoryException ("No enough memory for a new thread, exit requested");
          }
        }
      }
    }

    /// <summary>
    /// Return the number of running monitored machine analysis threads
    /// </summary>
    /// <returns></returns>
    int GetNumberRunningMachineAnalysisThreads ()
    {
      int result = 0;
      foreach (MachineActivityAnalysis analysis in m_machineAnalysisList.Values) {
        if (!analysis.CanRun ()) {
          ++result;
        }
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"GetNumberRunningMachineAnalysisThreads: the current number of running monitored machine analysis threads is {result}");
      }
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="waitTime"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>the process is fully completed on all the machines</returns>
    bool WaitForMachineAnalysisCompletion (TimeSpan waitTime, CancellationToken cancellationToken)
    {
      if (m_machineAnalysisCompleted) {
        if (log.IsDebugEnabled) {
          log.Debug ("WaitForMachineAnalysisCompletion: already completed");
        }
        return true;
      }

      if (log.IsDebugEnabled) {
        log.Debug ("WaitForMachineAnalysisCompletion: check all the machine analysis is completed again");
      }
      DateTime waitEnd = DateTime.UtcNow.Add (waitTime);
      while (!cancellationToken.IsCancellationRequested) {
        SetActive ();
        if (waitEnd < DateTime.UtcNow) {
          if (log.IsWarnEnabled) {
            log.Warn ($"WaitForMachineAnalysisCompletion: run global analysis without being sure all the machine analysis are completed because {waitTime} is reached");
          }
          return false;
        }
        CheckExitRequestedByMachineAnalysis ();
        if (CheckForMachineAnalysisCompletion ()) {
          log.Debug ("WaitForMachineAnalysisCompletion: all the machine analysis are completed");
          return true;
        }
        this.Sleep (TimeSpan.FromMilliseconds (100), cancellationToken);
        if (m_checkThreads.ExitRequested) {
          log.Fatal ("WaitForMachineAnalysisCompletion: exit requested from checkThreads");
          SetExitRequested ();
          throw new Lemoine.Threading.AbortException ("Exit was requested by CheckThreads.");
        }
      } // while
      cancellationToken.ThrowIfCancellationRequested ();
      return false;
    }

    void CheckExitRequestedByMachineAnalysis ()
    {
      foreach (var machineAnalysis in m_machineAnalysisList.Values) {
        if (machineAnalysis.ExitRequested) {
          log.Fatal ($"CheckExitRequestedByMachineAnalysis: exit requested from MachineActivityAnalysis machineId={machineAnalysis.Machine.Id}");
          SetExitRequested ();
          throw new Lemoine.Threading.AbortException ("Exit is requested.");
        }
      }
    }

    bool CheckForMachineAnalysisCompletion ()
    {
      bool allCompleted = true;
      foreach (MachineActivityAnalysis machineAnalysis in m_machineAnalysisList.Values) {
        if (!machineAnalysis.CanRun ()) {
          if (log.IsInfoEnabled) {
            log.Info ($"CheckForMachineAnalysisCompletion: analysis for machine {machineAnalysis.Machine.Id} is still running => allCompleted=false");
          }
          allCompleted = false;
        }
      }
      if (allCompleted) {
        log.Debug ("CheckForMachineAnalysisCompletion: all the machine analysis are completed");
        m_machineAnalysisCompleted = true;
        return true;
      }
      else {
        return false;
      }
    }

    bool WaitForMachineAnalysisCompletionBeforeGlobalAnalysis (CancellationToken cancellationToken)
    {
      TimeSpan globalAnalysisMachineAnalysisCompletionWait = ConfigSet
        .LoadAndGet<TimeSpan> (GLOBAL_ANALYSIS_MACHINE_ANALYSIS_COMPLETION_WAIT_KEY,
                               GLOBAL_ANALYSIS_MACHINE_ANALYSIS_COMPLETION_WAIT_DEFAULT);
      return WaitForMachineAnalysisCompletion (globalAnalysisMachineAnalysisCompletionWait, cancellationToken);
    }

    bool WaitForFillAnalysisStatusCompletion (CancellationToken cancellationToken)
    {
      TimeSpan fillAnalysisStatusCompletionWait = ConfigSet
        .LoadAndGet<TimeSpan> (FILL_ANALYSIS_STATUS_COMPLETION_WAIT_KEY,
                               FILL_ANALYSIS_STATUS_COMPLETION_WAIT_DEFAULT);
      return WaitForMachineAnalysisCompletion (fillAnalysisStatusCompletionWait, cancellationToken);
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
        m_globalAnalysis?.Dispose ();
        m_checkThreads?.Dispose ();
      }

      m_disposed = true;

      base.Dispose (disposing);
    }
    #endregion // IDisposable implementation
  }
}
