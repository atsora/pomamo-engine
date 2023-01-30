// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml;

using Lemoine.Core.Log;
using Lemoine.Threading;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System.Linq;
using System.Diagnostics;
using Lemoine.Core.ExceptionManagement;
using System.Threading;
using Lemoine.Business.Config;
using Lemoine.FileRepository;

namespace Lemoine.AutoReason
{
  /// <summary>
  /// Class that checks all the alert data to process and process them if applicable
  /// </summary>
  public class AutoReasonEngine
    : ThreadClass
    , IThreadClass
    , IChecked
  {
    /// <summary>
    /// Not responding timeout in CheckThreads
    /// </summary>
    static readonly string NOT_RESPONDING_TIMEOUT_KEY = "AutoReason.Engine.NotRespondingTimeout";
    static readonly TimeSpan NOT_RESPONDING_TIMEOUT_DEFAULT = TimeSpan.FromMinutes (7);

    /// <summary>
    /// This value indicates what is the maximum number of threads to use in the thread pool
    /// </summary>
    static readonly string MAX_THREADS_IN_POOL_KEY = "AutoReason.Engine.MaxThreadsInPool";
    static readonly int MAX_THREADS_IN_POOL_DEFAULT = 20;

    /// <summary>
    /// This value indicates what is the maximum number of monitored machine threads
    /// that can be run simultaneously
    /// </summary>
    static readonly string MAX_RUNNING_MACHINE_THREADS_KEY = "AutoReason.Engine.MaxRunningMachineThreads";
    static readonly int MAX_RUNNING_MACHINE_THREADS_DEFAULT = 4;

    /// <summary>
    /// Percentage of the used memory after which the service is stopped
    /// </summary>
    static readonly string MEMORY_PERCENTAGE_EXIT_KEY = "AutoReason.Engine.MemoryPercentageExit";
    static readonly int MEMORY_PERCENTAGE_EXIT_DEFAULT = 40;

    /// <summary>
    /// Percentage of the used memory after which no new thread is created
    /// </summary>
    static readonly string MEMORY_PERCENTAGE_STOP_NEW_THREADS_KEY = "AutoReason.Engine.MemoryPercentageStopNewThreads";
    static readonly int MEMORY_PERCENTAGE_STOP_NEW_THREADS_DEFAULT = 30;

    /// <summary>
    /// Time to wait when the maximum number of running monitored machine analysis threads is reached
    /// </summary>
    static readonly string SLEEP_MAX_RUNNING_MACHINE_THREADS_KEY = "AutoReason.Machine.SleepMaxRunningThreads";
    static readonly TimeSpan SLEEP_MAX_RUNNING_MACHINE_THREADS_DEFAULT = TimeSpan.FromMilliseconds (100);

    /// <summary>
    /// Maximum time to wait for a free running monitored analysis thread
    /// </summary>
    static readonly string MAX_WAIT_MACHINE_THREAD_KEY = "AutoReason.Machine.MaxWaitThread";
    static readonly TimeSpan MAX_WAIT_MACHINE_THREAD_DEFAULT = TimeSpan.FromMinutes (3);

    /// <summary>
    /// Time to wait when the memory limit is reached before adding more threads
    /// </summary>
    static readonly string SLEEP_FREE_MEMORY_KEY = "AutoReason.Machine.SleepFreeMemory";
    static readonly TimeSpan SLEEP_FREE_MEMORY_DEFAULT = TimeSpan.FromMilliseconds (100);

    /// <summary>
    /// Maximum time to wait after a memory limit is detected before adding more threads
    /// </summary>
    static readonly string MAX_WAIT_FREE_MEMORY_KEY = "AutoReason.Machine.MaxWaitFreeMemory";
    static readonly TimeSpan MAX_WAIT_FREE_MEMORY_DEFAULT = TimeSpan.FromMinutes (3);

    static readonly string FREQUENCY_KEY = "AutoReason.Engine.Sleep";
    static readonly TimeSpan FREQUENCY_DEFAULT = TimeSpan.FromSeconds (2);

    static readonly string TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_KEY = "AutoReason.TemporaryWithDelayException.Sleep";
    static readonly TimeSpan TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_DEFAULT = TimeSpan.FromSeconds (5);

    #region Members
    bool m_disposed = false;
    readonly IConfigUpdateChecker m_configUpdateChecker;
    readonly IDictionary<int, MachineAutoReasonAnalysis> m_machineAnalysisList =
      new Dictionary<int, MachineAutoReasonAnalysis> ();
    readonly CheckThreadsAndProcesses m_checkThreads = new CheckThreadsAndProcesses ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (AutoReasonEngine).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonEngine (bool isLctr)
    {
      if (isLctr) {
        m_configUpdateChecker = new ConfigUpdateCheckerWithAvailableFileRepository ();
        m_checkThreads.AddAdditionalCheckers (m_configUpdateChecker, new FileRepoChecker ());
      }
      else {
        m_configUpdateChecker = new ConfigUpdateChecker ();
        m_checkThreads.AddAdditionalCheckers (m_configUpdateChecker);
      }

      // MachineActivityAnalysis
      string machinesOption = Lemoine.Info.OptionsFile.GetOption ("Machines");
      ICollection<int> machineIdsOption = new HashSet<int> ();
      if (!string.IsNullOrEmpty (machinesOption)) {
        log.Info ($"AutoReasonEngine: Option Machines is set with {machinesOption}");
        string[] machineIds = machinesOption.Split (new char[] { ',', ';', ' ' });
        foreach (string machineIdString in machineIds) {
          int machineId;
          if (int.TryParse (machineIdString, out machineId)) {
            log.Debug ($"AutoReasonEngine: consider machine Id {machineId} from option");
            machineIdsOption.Add (machineId);
          }
        }
      }

      // m_analysisList, by monitored machine
      IEnumerable<IMonitoredMachine> monitoredMachines;
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        monitoredMachines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindAllWithMachineModule ()
          .Where (m => m.MonitoringType.Id != (int)Lemoine.Model.MachineMonitoringTypeId.Obsolete); // Exclude the obsolete machines
      }
      log.InfoFormat ("AutoReasonEngine: " +
                      "start AutoReasonAnalysis with {0} monitored machines",
                      monitoredMachines.Count ());
      foreach (IMonitoredMachine monitoredMachine in monitoredMachines) {
        if (string.IsNullOrEmpty (machinesOption)
            || machineIdsOption.Contains (monitoredMachine.Id)) {
          log.DebugFormat ("AutoReasonEngine: " +
                           "initialize machine {0}",
                           monitoredMachine);
          var monitoredMachineActivityAnalysis =
            new MachineAutoReasonAnalysis (monitoredMachine);
          m_machineAnalysisList.Add (monitoredMachine.Id, monitoredMachineActivityAnalysis);
          m_checkThreads.AddThread (monitoredMachineActivityAnalysis);
        }
      }
    }
    #endregion // Constructors

    #region Methods
    MachineAutoReasonAnalysis GetMachineActivityAnalysis (IMachine machine)
    {
      MachineAutoReasonAnalysis machineAutoReasonAnalysis = null;
      if (!m_machineAnalysisList.TryGetValue (machine.Id, out machineAutoReasonAnalysis)) {
        log.FatalFormat ("RequestPause: " +
                         "monitored machine activity analysis {0} could not be found",
                         machine);
        Debug.Assert (false);
        throw new InvalidOperationException ();
      }
      Debug.Assert (null != machineAutoReasonAnalysis);
      return machineAutoReasonAnalysis;
    }

    /// <summary>
    /// Main method:
    /// <item>get all the data to process</item>
    /// <item>if applicable, process them</item>
    /// </summary>
    public void RunOnePass ()
    {

      log.Debug ("RunOnePass: completed");
    }

    #endregion // Methods

    #region IThreadClass implementation
    /// <summary>
    /// Run the activity analysis and loop
    /// </summary>
    protected override void Run (CancellationToken cancellationToken)
    {
      m_checkThreads.InitializeAdditionalCheckers ();

      TimeSpan notRespondingTimeout = Lemoine.Info.ConfigSet
            .LoadAndGet<TimeSpan> (NOT_RESPONDING_TIMEOUT_KEY,
                                   NOT_RESPONDING_TIMEOUT_DEFAULT);

      while (!cancellationToken.IsCancellationRequested) {
        try {
          m_checkThreads.NotRespondingTimeout = notRespondingTimeout;
          m_checkThreads.StartIfUnstarted (cancellationToken);
          RunAnalysis (cancellationToken);
          log.Fatal ("Run: RunAnalysis should loop and never return ! => try again in a few seconds");
        }
        catch (ThreadAbortException) {
          // Try to make the thread abort faster, interrupting all the threads in the thread pool earlier
          log.Info ("Run: thread abort exception");
          try {
            m_checkThreads.Abort ();
          }
          catch (Exception ex1) {
            log.Error ("Run: abort of m_checkThreads failed", ex1);
          }

          foreach (var machineAnalysis in m_machineAnalysisList.Values) {
            machineAnalysis.Interrupt ();
          }

          throw;
        }
        catch (OutOfMemoryException ex) {
          log.Error ("Run: OutOfMemoryException, give up", ex);
          this.SetExitRequested ();
          throw new OutOfMemoryException ("OutOfMemoryException raised by RunAnalysis", ex);
        }
        catch (OperationCanceledException ex) {
          log.Error ("Run: operationCanceledException", ex);
          cancellationToken.ThrowIfCancellationRequested ();
          break;
        }
        catch (Exception ex) {
          if (ExceptionTest.IsStale (ex, log)) {
            log.Info ($"Run: Stale exception => try again", ex);
          }
          else if (ExceptionTest.IsTransactionSerializationFailure (ex, log)) {
            log.Info ($"Run: transaction serialization failure => try again", ex);
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
            log.Error ($"Run: exception with an invalid request, inner {ex.InnerException} but continue", ex);
          }
          else if (ExceptionTest.RequiresExit (ex, log)) {
            // OutOfMemory or NHibernate.TransactionException which is not temporary
            // (TransactionSerializationFailure or TimeoutFailure)
            log.Error ($"Run: Exception with inner exception {ex.InnerException} requires to exit, give up", ex);
            this.SetExitRequested ();
            throw new Lemoine.Threading.AbortException ("Exception requires to exit in MakeAnalysis", ex);
          }
          else {
            log.Fatal ("Run: unexpected error => try again in a few seconds", ex);
          }
        }
        this.Sleep (TimeSpan.FromSeconds (30), cancellationToken, () => this.ExitRequested);
      }

      log.Error ("Run: loop end because of a cancellation was requested");
    }

    /// <summary>
    /// Run the Activity Analysis on all the machine modules once
    /// </summary>
    public void RunAnalysis (CancellationToken cancellationToken)
    {
      TimeSpan frequency = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (FREQUENCY_KEY, FREQUENCY_DEFAULT);
      int workerThreads;
      int completionPortThreads;

      ThreadPool.GetMaxThreads (out workerThreads, out completionPortThreads);
      log.InfoFormat ("RunAnalysis: " +
                      "default workThreads={0} completionPortThreads={1}",
                      workerThreads, completionPortThreads);
      int maxThreads = Lemoine.Info.ConfigSet.LoadAndGet<int> (MAX_THREADS_IN_POOL_KEY, MAX_THREADS_IN_POOL_DEFAULT);
      if (maxThreads < workerThreads) {
        log.InfoFormat ("RunAnalysis: " +
                        "decrease the number of workThreads from {0} to {1}",
                        workerThreads, maxThreads);
        workerThreads = maxThreads;
      }
      if (maxThreads < completionPortThreads) {
        log.InfoFormat ("RunAnalysis: " +
                        "decrease the number of completionPortThreads from {0} to {1}",
                        completionPortThreads, maxThreads);
        completionPortThreads = maxThreads;
      }
      if (!ThreadPool.SetMaxThreads (workerThreads, completionPortThreads)) {
        log.Error ("RunAnalysis: SetMaxThreads failed");
      }

      int memoryPercentageExit = Lemoine.Info.ConfigSet.LoadAndGet<int> (MEMORY_PERCENTAGE_EXIT_KEY, MEMORY_PERCENTAGE_EXIT_DEFAULT);
      long memoryLimitExit = (long)(Lemoine.Info.ComputerInfo.TotalPhysicalMemory / 100)
        * memoryPercentageExit;
      while (!cancellationToken.IsCancellationRequested) {
        DateTime loopBegin = DateTime.UtcNow;

        if (m_checkThreads.ExitRequested) {
          log.Fatal ("RunAnalysis: exit requested from checkThreads");
          this.SetExitRequested ();
          throw new Lemoine.Threading.AbortException ("Exit requested");
        }

        if (memoryLimitExit < Lemoine.Info.ProgramInfo.GetPhysicalMemory ()) {
          log.ErrorFormat ("RunAnalysis: " +
                           "memory limit {0} is reached " +
                           "=> exit",
                           memoryLimitExit);
          this.SetExitRequested ();
          throw new OutOfMemoryException ("Memory limit reached, exit requested");
        }

        SetActive ();
        CheckNoConfigUpdate ();
        SetActive ();

        // m_machineAutoReaons, by monitored machine
        foreach (var machineAnalysis in m_machineAnalysisList.Values) {
          if (cancellationToken.IsCancellationRequested) {
            log.Error ("RunAnalysis: cancellation requested in machine analysis loop");
            cancellationToken.ThrowIfCancellationRequested ();
            break;
          }

          if (memoryLimitExit < Lemoine.Info.ProgramInfo.GetPhysicalMemory ()) {
            log.ErrorFormat ("RunAnalysis: " +
                             "memory limit {0} is reached " +
                             "=> exit",
                             memoryLimitExit);
            this.SetExitRequested ();
            throw new OutOfMemoryException ("Memory limit reached, exit requested");
          }

          if (machineAnalysis.ExitRequested) {
            log.FatalFormat ("RunAnalysis: " +
                             "exit requested from MachineActivityAnalysis machineId={0}",
                             machineAnalysis.Machine.Id);
            this.SetExitRequested ();
            throw new Exception ("Exit requested");
          }

          if (machineAnalysis.CanRun ()) {
            log.DebugFormat ("RunAnalysis: " +
                             "run a new analysis on machine {0}",
                             machineAnalysis.Machine);
            SetActive ();
            WaitFreeThreads (cancellationToken);
            SetActive ();
            WaitFreeMemory (cancellationToken);
            SetActive ();

            if (!machineAnalysis.StartInThreadPool (machineAnalysis.RunAnalysis, "RunAnalysis", cancellationToken)) {
              log.Warn ("RunAnalysis: analysis.RunAnalysis task was not queued in the thread pool");
            }
          }
        }

        DateTime loopEnd = loopBegin.Add (frequency);
        this.SleepUntil (loopEnd, cancellationToken);

        SetActive ();
        if (m_checkThreads.ExitRequested) {
          log.Fatal ("RunAnalysis: exit requested from checkThreads");
          this.SetExitRequested ();
          throw new Lemoine.Threading.AbortException ("Exit requested");
        }

        foreach (var analysis in m_machineAnalysisList.Values) {
          cancellationToken.ThrowIfCancellationRequested ();
          if (analysis.ExitRequested) {
            log.Fatal ($"RunAnalysis: exit requested from MachineActivityAnalysis machineId={analysis.Machine.Id}");
            this.SetExitRequested ();
            throw new Lemoine.Threading.AbortException ("Exit requested");
          }
          if (!analysis.CanRun ()) {
            log.Warn ($"RunAnalysis: analysis for machine {analysis.Machine.Id} is still running");
          }
        }
        log.Debug ("RunAnalysis: " +
                   "run once again the analysis");
      } // Loop

      log.Error ("RunAnalysis: cancellation was requested, end of RunAnalysis method");
      cancellationToken.ThrowIfCancellationRequested ();
    }

    /// <summary>
    /// Check the configuration was not updated
    /// </summary>
    void CheckNoConfigUpdate ()
    {
      if (!m_configUpdateChecker.CheckNoConfigUpdate ()) {
        log.Error ("CheckNoConfigUpdate: the configuration was updated => exit");
        this.SetExitRequested ();
        throw new Exception ("Configuration updated, exit");
      }
    }

    void WaitFreeThreads (CancellationToken cancellationToken)
    {
      DateTime loopBeginDateTime = DateTime.UtcNow;
      int maxRunningMachineThreads = Lemoine.Info.ConfigSet.LoadAndGet<int> (MAX_RUNNING_MACHINE_THREADS_KEY, MAX_RUNNING_MACHINE_THREADS_DEFAULT);
      while ( (maxRunningMachineThreads <=
             GetNumberRunningMachineAnalysisThreads ()) && !cancellationToken.IsCancellationRequested) {
        TimeSpan waitTime = DateTime.UtcNow.Subtract (loopBeginDateTime);
        TimeSpan sleepTime = (TimeSpan)Lemoine.Info.ConfigSet.LoadAndGet (SLEEP_MAX_RUNNING_MACHINE_THREADS_KEY,
                                                                          SLEEP_MAX_RUNNING_MACHINE_THREADS_DEFAULT);
        log.DebugFormat ("WaitFreeThreads: " +
                         "the maximum number of running machine threads {0} is still reached after {1} " +
                         "=> wait one running thread is finished during {2}",
                         maxRunningMachineThreads,
                         waitTime,
                         sleepTime);
        TimeSpan maxWaitTime = (TimeSpan)Lemoine.Info.ConfigSet.LoadAndGet (MAX_WAIT_MACHINE_THREAD_KEY,
                                                                            MAX_WAIT_MACHINE_THREAD_DEFAULT);
        if (maxWaitTime <= waitTime) {
          log.ErrorFormat ("WaitFreeThreads: " +
                           "the maximum wait time {0} to wait for a machine analysis thread is reached. " +
                           "Wait time is {1} " +
                           "=> do not wait for more time",
                           maxWaitTime,
                           waitTime);
          break;
        }
        this.Sleep (sleepTime, cancellationToken, () => this.ExitRequested);
      }
    }

    void WaitFreeMemory (CancellationToken cancellationToken)
    {
      DateTime loopBeginDateTime = DateTime.UtcNow;
      int memoryPercentageStopNewThreads = Lemoine.Info.ConfigSet.LoadAndGet<int> (MEMORY_PERCENTAGE_STOP_NEW_THREADS_KEY, MEMORY_PERCENTAGE_STOP_NEW_THREADS_DEFAULT);
      long memoryLimit = (long)(Lemoine.Info.ComputerInfo.TotalPhysicalMemory / 100)
        * memoryPercentageStopNewThreads;
      while ( (memoryLimit < Lemoine.Info.ProgramInfo.GetPhysicalMemory ()) && !cancellationToken.IsCancellationRequested) {
        TimeSpan waitTime = DateTime.UtcNow.Subtract (loopBeginDateTime);
        TimeSpan sleepTime = (TimeSpan)Lemoine.Info.ConfigSet.LoadAndGet (SLEEP_FREE_MEMORY_KEY,
                                                                          SLEEP_FREE_MEMORY_DEFAULT);
        log.InfoFormat ("WaitFreeMemory: " +
                        "the memory limit {0} is reached still after {1} " +
                        "=> wait some more memory is free during {2}",
                        memoryPercentageStopNewThreads,
                        waitTime,
                        sleepTime);
        TimeSpan maxWaitTime = (TimeSpan)Lemoine.Info.ConfigSet.LoadAndGet (MAX_WAIT_FREE_MEMORY_KEY,
                                                                            MAX_WAIT_FREE_MEMORY_DEFAULT);
        if (maxWaitTime <= waitTime) {
          log.ErrorFormat ("WaitFreeMemory: " +
                           "the maximum wait time {0} to wait for some free memory is reached. " +
                           "Wait time is {1} " +
                           "=> do not wait for more time",
                           maxWaitTime,
                           waitTime);
          break;
        }
        this.Sleep (sleepTime, cancellationToken, () => this.ExitRequested);
      }
    }

    /// <summary>
    /// Return the number of running monitored machine analysis threads
    /// </summary>
    /// <returns></returns>
    int GetNumberRunningMachineAnalysisThreads ()
    {
      int result = 0;
      foreach (var analysis in m_machineAnalysisList.Values) {
        if (!analysis.CanRun ()) {
          ++result;
        }
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"RunAnalysis: the current number of running monitored machine analysis threads is {result}");
      }
      return result;
    }

    /// <summary>
    /// Logger
    /// </summary>
    /// <returns></returns>
    public override ILog GetLogger ()
    {
      return log;
    }
    #endregion // IThreadClass implementation

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
        m_checkThreads?.Dispose ();
      }

      m_disposed = true;

      base.Dispose (disposing);
    }
    #endregion // IDisposable implementation
  }
}
