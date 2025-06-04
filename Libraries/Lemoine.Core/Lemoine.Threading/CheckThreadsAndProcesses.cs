// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

using Lemoine.Info;
using Lemoine.Core.Log;

namespace Lemoine.Threading
{
  /// <summary>
  /// Check the threads and processes are still running
  /// </summary>
  public class CheckThreadsAndProcesses
    : ThreadClass
  {
    const string RESTART_THREAD_KEY = "CheckThreadsAndProcesses.RestartThread";
    const bool RESTART_THREAD_DEFAULT = true;

    const string RESTART_THREAD_TIMEOUT_KEY = "CheckThreadsAndProcesses.RestartThreadTimeout";
    static readonly TimeSpan RESTART_THREAD_TIMEOUT_DEFAULT = TimeSpan.FromSeconds (10);

    static readonly string FREQUENCY_KEY = "CheckThreadsAndProcesses.Frequency";
    static readonly TimeSpan FREQUENCY_DEFAULT = TimeSpan.FromMinutes (2);

    static readonly string NOT_RESPONDING_TIMEOUT_KEY = "CheckThreadsAndProcesses.NotRespondingTimeout";
    static readonly TimeSpan NOT_RESPONDING_TIMEOUT_DEFAULT = TimeSpan.FromMinutes (2);

    static readonly string SLEEP_BEFORE_RESTART_KEY = "CheckThreadsAndProcesses.SleepBeforeRestart";
    static readonly TimeSpan SLEEP_BEFORE_RESTART_DEFAULT = TimeSpan.FromSeconds (30);

    static readonly string SLEEP_BEFORE_FORCE_EXIT_KEY = "CheckThreadsAndProcesses.SleepBeforeForceExit";
    static readonly TimeSpan SLEEP_BEFORE_FORCE_EXIT_DEFAULT = TimeSpan.FromSeconds (10);

    #region Members
    bool m_disposed = false;
    readonly IList<ProcessClassExecution> m_processClassExecutions = new List<ProcessClassExecution> ();
    readonly IList<ILoggedThreadClass> m_threadClasses = new List<ILoggedThreadClass> ();
    readonly IList<Func<bool>> m_additionalChecks = new List<Func<bool>> ();
    readonly IList<IAdditionalChecker> m_additionalCheckers = new List<IAdditionalChecker> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (CheckThreadsAndProcesses).FullName);

    #region Getters / Setters
    /// <summary>
    /// When the main thread method ends in exception, should the application be stopped ?
    /// </summary>
    override protected bool ExitOnException => true;

    /// <summary>
    /// Thread classes to check
    /// </summary>
    protected IList<ILoggedThreadClass> ThreadClasses => m_threadClasses;

    /// <summary>
    /// Process classes to check
    /// </summary>
    protected IList<ProcessClassExecution> ProcessClassExecutions => m_processClassExecutions;

    /// <summary>
    /// Global cancellation token to check if the application was really been requested to exit
    /// </summary>
    public CancellationToken GlobalCancellationToken { get; set; } = CancellationToken.None;
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CheckThreadsAndProcesses ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add an additional check
    /// </summary>
    /// <param name="additionalCheck"></param>
    public void AddAdditionalCheck (Func<bool> additionalCheck)
    {
      m_additionalChecks.Add (additionalCheck);
    }

    /// <summary>
    /// Add additional checkers
    /// </summary>
    /// <param name="additionalCheckers"></param>
    public void AddAdditionalCheckers (params IAdditionalChecker[] additionalCheckers)
    {
      foreach (var additionalChecker in additionalCheckers) {
        m_additionalCheckers.Add (additionalChecker);
      }
    }

    /// <summary>
    /// Initialize additional checkers
    /// </summary>
    /// <param name="additionalCheckers"></param>
    public void InitializeAdditionalCheckers (CancellationToken cancellationToken = default)
    {
      foreach (var additionalChecker in m_additionalCheckers) {
        if (cancellationToken.IsCancellationRequested) {
          return;
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"InitializeAdditionalCheckers: about to initialize {additionalChecker}");
        }
        try {
          additionalChecker.Initialize ();
        }
        catch (Exception ex) {
          log.Error ($"InitializeAdditionalCheckers: Initialize of {additionalChecker} failed", ex);
        }
      }
    }

    /// <summary>
    /// Add a <see cref="IThreadClass">Thread class</see> to check
    /// </summary>
    /// <param name="threadClass"></param>
    public void AddThread (ILoggedThreadClass threadClass)
    {
      m_threadClasses.Add (threadClass);
    }

    /// <summary>
    /// Add a <see cref="ProcessClassExecution">Process class exeuction</see> with the <see cref="IProcessClass">Process class</see> to check
    /// </summary>
    /// <param name="processClassExecution"></param>
    public void AddProcess (ProcessClassExecution processClassExecution)
    {
      m_processClassExecutions.Add (processClassExecution);
    }

    /// <summary>
    /// Override SetActive here to do nothing
    /// since it is called by ThreadClass.RunInThread
    /// and since it may prevent the application from exiting
    /// </summary>
    public override void SetActive ()
    {
    }

    /// <summary>
    /// Main thread method
    /// </summary>
    protected override void Run (CancellationToken cancellationToken)
    {
      try {
        var frequency = Lemoine.Info.ConfigSet
          .LoadAndGet (FREQUENCY_KEY, FREQUENCY_DEFAULT);
        while (!cancellationToken.IsCancellationRequested) {
          this.Sleep (frequency, cancellationToken);
          if (cancellationToken.IsCancellationRequested) {
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"Run: cancellation requested");
            }
            return;
          }
          try {
            CheckAllThreads (cancellationToken);
          }
          catch (Exception ex) {
            GetLogger ().Fatal ("Run: CheckAllThreads returned an unexpected exception, but continue", ex);
          }
          if (cancellationToken.IsCancellationRequested) {
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"Run: cancellation requested");
            }
            return;
          }
          try {
            CheckAllProcesses (cancellationToken);
          }
          catch (Exception ex) {
            GetLogger ().Fatal ("Run: CheckAllProcesses returned an unexpected exception, but continue", ex);
          }
          if (cancellationToken.IsCancellationRequested) {
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"Run: cancellation requested");
            }
            return;
          }
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ($"Run: additional checks");
          }
          foreach (Func<bool> additionalCheck in m_additionalChecks) {
            if (cancellationToken.IsCancellationRequested) {
              if (GetLogger ().IsDebugEnabled) {
                GetLogger ().Debug ($"Run: cancellation requested");
              }
              return;
            }
            try {
              if (!additionalCheck ()) {
                GetLogger ().Error ("Run: this additional check failed, exit");
                Exit (GetLogger ());
              }
            }
            catch (Exception ex) {
              GetLogger ().Error ($"Run: one additional check returned an exception, but continue", ex);
            }
          }
          foreach (var additionalChecker in m_additionalCheckers) {
            if (cancellationToken.IsCancellationRequested) {
              if (GetLogger ().IsDebugEnabled) {
                GetLogger ().Debug ($"Run: cancellation requested");
              }
              return;
            }
            try {
              if (!additionalChecker.Check ()) {
                GetLogger ().Error ($"Run: additional checker {additionalChecker} failed, exit");
                Exit (GetLogger ());
              }
            }
            catch (Exception ex) {
              GetLogger ().Error ($"Run: additional checker {additionalChecker} returned an exception {ex.Message}, but continue", ex);
            }
          }

          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ($"Run: all checks are ok");
          }
        }

        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ($"Run: cancellation requested in loop condition");
        }
      }
      catch (Exception uncaughtException) {
        GetLogger ().Fatal ($"Run: uncaught exception", uncaughtException);
        throw;
      }
    }

    void Exit (ILog logger)
    {
      try {
        SetExitRequested ();
        // - Sleep a little before executing exit to try the exitRequested method first
        TimeSpan sleepBeforeForceExit = Lemoine.Info.ConfigSet
          .LoadAndGet<TimeSpan> (SLEEP_BEFORE_FORCE_EXIT_KEY,
                                 SLEEP_BEFORE_FORCE_EXIT_DEFAULT);
        this.Sleep (sleepBeforeForceExit, this.GlobalCancellationToken);
        if (!this.GlobalCancellationToken.IsCancellationRequested) {
          // - exitRequested was not enough, force exit !
          logger.Warn ("Exit: force exit because no global cancellation token was cancelled");
          ForceExit (logger);
        }
      }
      catch (Exception ex) {
        GetLogger ().Error ($"Exit: exception {ex.Message} => force exit", ex);
        ForceExit (logger);
        throw;
      }
    }

    void ForceExit (ILog logger)
    {
      Lemoine.Core.Environment.LogAndForceExit (logger);
    }

    /// <summary>
    /// Logger
    /// </summary>
    /// <returns></returns>
    public override ILog GetLogger ()
    {
      return log;
    }

    void CheckAllThreads (CancellationToken cancellationToken)
    {
      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().Debug ($"CheckAllThreads");
      }
      foreach (var threadClass in m_threadClasses) {
        if (cancellationToken.IsCancellationRequested) {
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ($"CheckAllThreads: cancellation requested");
          }
          return;
        }
        try {
          CheckThread (threadClass, cancellationToken);
        }
        catch (Exception ex) {
          GetLogger ().Error ($"CheckAllThreads: CheckThread of {threadClass} returned an exception, but continue", ex);
        }
      }
    }

    /// <summary>
    /// Check a thread with its associated main class
    /// </summary>
    /// <param name="threadClass"></param>
    /// <param name="cancellationToken"></param>
    protected void CheckThread<T> (T threadClass, CancellationToken cancellationToken)
      where T: IThreadClass, ILogged
    {
      try {
        if (threadClass.ExitRequested) {
          GetLogger ().Fatal ($"CheckThread: exit was requested by {threadClass} => exit");
          Exit (GetLogger ());
          return;
        }

        if (threadClass.IsCheckInPause ()) {
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ("CheckThread: in pause");
          }
          return;
        }

        DateTime lastExecution = threadClass.LastExecution;
        TimeSpan elapsedTime = DateTime.UtcNow - lastExecution;
        if (threadClass.Error || (GetNotRespondingTimeout (threadClass) < elapsedTime)) { // 5 minutes
          ILog threadClassLogger = threadClass.GetLogger ();

          if (threadClass.Error) {
            GetLogger ().Error ("CheckThread: the thread ended in error (an exception was raised)");
            threadClassLogger.Error ("the thread ended in error (an exception was raised)");
          }
          else { // !threadClass.Error
            GetLogger ().Fatal ("CheckThread: the thread is not ok");
            threadClassLogger.Fatal ("the CheckThreads.CheckThread method detected the thread was not ok");
          }

          if (Lemoine.Info.ConfigSet.LoadAndGet (RESTART_THREAD_KEY, RESTART_THREAD_DEFAULT)) {
            GetLogger ().Warn ("CheckThread: try to restart the thread");
            threadClassLogger.Warn ("the CheckThreads.CheckThread method detected the thread was not ok => try to restart it");
            var restartTimeout = Lemoine.Info.ConfigSet
              .LoadAndGet (RESTART_THREAD_TIMEOUT_KEY, RESTART_THREAD_TIMEOUT_DEFAULT);
            if (threadClass.Restart (restartTimeout)) {
              GetLogger ().Info ($"CheckThread: thread successfully restarted in {restartTimeout}");
              threadClassLogger.Info ($"the thread was successfully restarted in {restartTimeout}");
              return;
            }
          }

          GetLogger ().Fatal ("CheckThread: the thread is not ok and was not restarted successfully => stop the program");
          threadClassLogger.Fatal ("the CheckThreads.CheckThread method detected the thread was not ok and it was not restarted successfully => stop the program");
          SetExitRequested ();
          // - Sleep a little before executing exit to try the exitRequested method first
          TimeSpan sleepBeforeForceExit = Lemoine.Info.ConfigSet
            .LoadAndGet<TimeSpan> (SLEEP_BEFORE_FORCE_EXIT_KEY,
                                   SLEEP_BEFORE_FORCE_EXIT_DEFAULT);
          this.Sleep (sleepBeforeForceExit, cancellationToken);
          if (!lastExecution.Equals (threadClass.LastExecution)) {
            GetLogger ().Fatal ("CheckThread: LastExecution was finally updated after the initial sleep but this is too late");
          }
          // - exitRequested was not enough, force exit !
          ForceExit (threadClassLogger);
        }
      }
      catch (Exception ex) {
        GetLogger ().Fatal ("CheckThread: unexpected exception", ex);
        throw;
      }
    }

    TimeSpan GetNotRespondingTimeout (IThreadClass threadClass)
    {
      if (threadClass.NotRespondingTimeout.HasValue) {
        return threadClass.NotRespondingTimeout.Value;
      }
      else if (this.NotRespondingTimeout.HasValue) {
        return this.NotRespondingTimeout.Value;
      }
      else {
        return Lemoine.Info.ConfigSet
          .LoadAndGet<TimeSpan> (NOT_RESPONDING_TIMEOUT_KEY,
                                 NOT_RESPONDING_TIMEOUT_DEFAULT);
      }
    }

    /// <summary>
    /// Check once the processes do not hang
    /// </summary>
    void CheckAllProcesses (CancellationToken cancellationToken)
    {
      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().Debug ($"CheckAllProcesses");
      }
      string programDirectory = Directory.GetParent (ProgramInfo.AbsolutePath).FullName;
      Directory.SetCurrentDirectory (programDirectory);
      foreach (ProcessClassExecution processClassExecution in m_processClassExecutions) {
        if (cancellationToken.IsCancellationRequested) {
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ($"CheckAllProcesses: cancellation requested");
          }
          return;
        }
        try {
          CheckProcess (processClassExecution, cancellationToken);
        }
        catch (Exception ex) {
          GetLogger ().Error ($"CheckAllProcesses: checkProcess of {processClassExecution.GetProcessName ()} failed but continue", ex);
        }
      }
    }

    void CheckProcess (ProcessClassExecution processClassExecution, CancellationToken cancellationToken)
    {
      try {
        string processName = processClassExecution.GetProcessName ();
        Process[] processes = Process.GetProcessesByName (processName);
        ILog checkProcessLog = LogManager.GetLogger ($"{processClassExecution.GetLogger ().Name}.CheckProcess");
        if (0 == processes.Length) {
          checkProcessLog.Warn ("The process was not running => restart it");
          processClassExecution.Start ();
          return;
        }
        else if (1 < processes.Length) {
          checkProcessLog.Fatal ($"More than one processes with name {processName} is running => kill both of them and restart it");
          Debug.Assert (1 == processes.Length);
          bool allStopped = true;
          foreach (Process process in processes) {
            if (cancellationToken.IsCancellationRequested) {
              if (log.IsDebugEnabled) {
                log.Debug ($"CheckAllProcesses: cancellation requested");
              }
              return;
            }
            if (!ProcessClassExecution.KillProcess (process.Id, checkProcessLog)) {
              checkProcessLog.Error ($"Process {processName} could not be stopped");
              allStopped = false;
            }
          }
          if (allStopped) {
            TimeSpan sleepBeforeRestart;
            if (processClassExecution.ProcessClass.SleepBeforeRestart.HasValue) {
              sleepBeforeRestart = processClassExecution.ProcessClass.SleepBeforeRestart.Value;
            }
            else if (this.SleepBeforeRestart.HasValue) {
              sleepBeforeRestart = this.SleepBeforeRestart.Value;
            }
            else {
              sleepBeforeRestart = Lemoine.Info.ConfigSet
                .LoadAndGet<TimeSpan> (SLEEP_BEFORE_RESTART_KEY,
                                       SLEEP_BEFORE_RESTART_DEFAULT);
            }
            checkProcessLog.Info ($"All the processes were stopped => restart a new one in {sleepBeforeRestart}");
            this.Sleep (sleepBeforeRestart, cancellationToken);
            processClassExecution.Start ();
          }
          return;
        }
        else { // Only one process
          var stampFilePath = processClassExecution.ProcessClass.GetStampFilePath ();
          if (!File.Exists (stampFilePath)) {
            checkProcessLog.Warn ($"CheckProcesses: stampFilePath {stampFilePath} does not exist");
          }
          else {
            TimeSpan elapsedTime =
              DateTime.UtcNow - File.GetLastWriteTimeUtc (stampFilePath);
            TimeSpan notRespondingTimeout;
            if (processClassExecution.ProcessClass.NotRespondingTimeout.HasValue) {
              notRespondingTimeout = processClassExecution.ProcessClass.NotRespondingTimeout.Value;
            }
            else if (this.NotRespondingTimeout.HasValue) {
              notRespondingTimeout = this.NotRespondingTimeout.Value;
            }
            else {
              notRespondingTimeout = Lemoine.Info.ConfigSet
                .LoadAndGet<TimeSpan> (NOT_RESPONDING_TIMEOUT_KEY,
                                       NOT_RESPONDING_TIMEOUT_DEFAULT);
            }
            if (notRespondingTimeout < elapsedTime) {
              Process process = processes[0];
              checkProcessLog.Warn ($"Process {processName} PID {process?.Id} is not ok (the stamp file is too old) => restart it");
              if (ProcessClassExecution.KillProcess (process.Id, checkProcessLog)) {
                TimeSpan sleepBeforeRestart;
                if (processClassExecution.ProcessClass.SleepBeforeRestart.HasValue) {
                  sleepBeforeRestart = processClassExecution.ProcessClass.SleepBeforeRestart.Value;
                }
                else if (this.SleepBeforeRestart.HasValue) {
                  sleepBeforeRestart = this.SleepBeforeRestart.Value;
                }
                else {
                  sleepBeforeRestart = Lemoine.Info.ConfigSet
                    .LoadAndGet<TimeSpan> (SLEEP_BEFORE_RESTART_KEY,
                                           SLEEP_BEFORE_RESTART_DEFAULT);
                }
                checkProcessLog.Info ($"Process {processName} was successfully stopped => restart it in {sleepBeforeRestart}");
                this.Sleep (sleepBeforeRestart, cancellationToken);
                processClassExecution.Start ();
              }
              else {
                checkProcessLog.Error ($"Process {processName} PID {process?.Id} could not be stopped");
              }
            }
          }
        }
      }
      catch (Exception ex) {
        GetLogger ().Fatal ("CheckProcess: unexpected exception", ex);
        throw;
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
        // Dispose your objects here
      }

      m_disposed = true;

      base.Dispose (disposing);
    }
    #endregion // IDisposable implementation
  }
}
