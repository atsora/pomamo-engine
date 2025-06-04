// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.ExceptionManagement;
using Lemoine.Core.Log;
using Lemoine.Info.ConfigReader;

namespace Lemoine.Threading
{
  /// <summary>
  /// Base <see cref="IThreadClass">IThreadClass</see> implementation
  /// </summary>
  public abstract class ThreadClass : ILoggedThreadClass, IChecked, IThreadStatusSupport, IDisposable
  {
    /// <summary>
    /// Thread status to be able to use the ThreadPool: Available, Requested, Running
    /// </summary>
    public enum ThreadStatus
    {
      /// <summary>
      /// Available
      /// 
      /// This is also the status the thread gets once it ends in error but can be restarted
      /// </summary>
      Available = 0,
      /// <summary>
      /// The thread was requested
      /// </summary>
      Requested = 1,
      /// <summary>
      /// Running
      /// </summary>
      Running = 2,
      /// <summary>
      /// Cancelling
      /// </summary>
      Cancelling = 3,
      /// <summary>
      /// Aborted
      /// </summary>
      Aborted = 4,
    }

    static readonly TimeSpan DEFAULT_STOP_TIMEOUT = TimeSpan.FromMilliseconds (100);

    const string ACTIVE_STACK_TRACE_KEY = "ThreadClass.Active.StackTrace";
    const bool ACTIVE_STACK_TRACE_DEFAULT = false;

    static readonly string TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_KEY = "ThreadClass.TemporaryWithDelayException.Sleep";
    static readonly TimeSpan TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_DEFAULT = TimeSpan.FromSeconds (2);

    #region Members
    bool m_disposed = false;
    CancellationTokenSource m_exitRequestedSource = new CancellationTokenSource ();
    CancellationTokenSource m_cancelSource = new CancellationTokenSource ();
    CancellationTokenSource m_effectiveSource = null;
    TimeSpan? m_timeout = null;
    bool m_timeoutAbort = false;
    TimeSpan? m_notRespondingTimeout = null;
    TimeSpan? m_sleepBeforeRestart = null;
    readonly ReaderWriterLock m_runningParamLock = new ReaderWriterLock ();
    DateTime m_startDateTime; // To be accessed from the thread only
    DateTime m_lastExecution = DateTime.UtcNow;
    System.Threading.Thread m_thread = null;
    TimeSpan m_stopTimeout = DEFAULT_STOP_TIMEOUT;
    Int32 m_threadStatus = (Int32)ThreadStatus.Available;
    volatile bool m_pause = true;
    volatile bool m_error = false;
    ApartmentState m_apartmentState = ApartmentState.Unknown;
    readonly bool m_activeStackTrace = Lemoine.Info.ConfigSet.LoadAndGet (ACTIVE_STACK_TRACE_KEY, ACTIVE_STACK_TRACE_DEFAULT);
    volatile bool m_exitRequested = false;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ThreadClass).FullName);

    #region Getters / Setters
    /// <summary>
    /// When the main thread method ends in exception, should the application be stopped ?
    /// </summary>
    protected virtual bool ExitOnException => false;

    /// <summary>
    /// Current thread status
    /// </summary>
    protected ThreadStatus CurrentThreadStatus => (ThreadStatus)m_threadStatus;

    /// <summary>
    /// Start date/time of the thread
    /// </summary>
    protected DateTime StartDateTime
    {
      get { return m_startDateTime; }
    }

    /// <summary>
    /// Time after which the thread kills itself.
    /// 
    /// This is safer to make it thread safe although this is not necessary
    /// if Timeout is only set before it is run
    /// </summary>
    public TimeSpan? Timeout
    {
      get {
        using (ReadLockHolder holder = new ReadLockHolder (m_runningParamLock)) {
          return m_timeout;
        }
      }
      set {
        using (WriteLockHolder holder = new WriteLockHolder (m_runningParamLock)) {
          m_timeout = value;
        }
      }
    }

    /// <summary>
    /// Was the thread aborted because of a timeout ?
    /// 
    /// Thread safe
    /// </summary>
    public bool TimeoutAbort
    {
      get {
        using (ReadLockHolder holder = new ReadLockHolder (m_runningParamLock)) {
          return m_timeoutAbort;
        }
      }
      protected set {
        using (WriteLockHolder holder = new WriteLockHolder (m_runningParamLock)) {
          m_timeoutAbort = value;
        }
      }
    }

    /// <summary>
    /// Not responding timeout (null: unknown / not set)
    /// </summary>
    public TimeSpan? NotRespondingTimeout
    {
      get { return m_notRespondingTimeout; }
      set { m_notRespondingTimeout = value; }
    }

    /// <summary>
    /// Time to sleep before restarting a malfunctioning thread (null: unknown / not set)
    /// </summary>
    public TimeSpan? SleepBeforeRestart
    {
      get { return m_sleepBeforeRestart; }
      set { m_sleepBeforeRestart = value; }
    }

    /// <summary>
    /// Latest execution date/time of the method
    /// 
    /// Warning: This getter / setter must be thread safe !
    /// </summary>
    public virtual DateTime LastExecution
    {
      get {
        using (ReadLockHolder holder = new ReadLockHolder (m_runningParamLock)) {
          return m_lastExecution;
        }
      }
      set {
        using (WriteLockHolder holder = new WriteLockHolder (m_runningParamLock)) {
          m_lastExecution = value;
        }
      }
    }

    /// <summary>
    /// Was the thread completed but in error (an exception was raised) ?
    /// </summary>
    public bool Error => m_error;

    /// <summary>
    /// Associated thread
    /// 
    /// Null if it was not started in a thread
    /// </summary>
    public System.Threading.Thread Thread => m_thread;

    /// <summary>
    /// Was the thread started ?
    /// 
    /// In Requested or Running state
    /// </summary>
    public bool Started
    {
      get {
        switch (this.CurrentThreadStatus) {
        case ThreadStatus.Requested:
        case ThreadStatus.Running:
          return true;
        default:
          return false;
        }
      }
    }

    /// <summary>
    /// True if the thread must be launched in a single thread appartment
    /// </summary>
    public bool StaThread => m_apartmentState.Equals (ApartmentState.STA);

    /// <summary>
    /// Associated CancellationToken
    /// </summary>
    public CancellationToken CancellationToken
    {
      get {
        if (m_effectiveSource is null) {
          m_effectiveSource = CancellationTokenSource.CreateLinkedTokenSource (m_cancelSource?.Token ?? CancellationToken.None, ExitRequestedToken);
        }
        return m_effectiveSource.Token;
      }
    }

    /// <summary>
    /// CancellationToken which is associated to an 'Exit requested request'
    /// </summary>
    CancellationToken ExitRequestedToken => m_exitRequestedSource?.Token ?? CancellationToken.None;
    #endregion // Getters / Setters

    #region Methods
    /// <summary>
    /// Reset the start date/time
    /// </summary>
    protected void ResetStartDateTime ()
    {
      m_startDateTime = DateTime.UtcNow;
      m_error = false;
    }

    /// <summary>
    /// Update the last execution variable
    /// </summary>
    protected virtual void UpdateLastExecution ()
    {
      if (GetLogger ().IsTraceEnabled) {
        if (m_activeStackTrace) {
          GetLogger ().Trace ("UpdateLastExecution: thread active" + System.Environment.StackTrace.Replace (System.Environment.NewLine, " "));
        }
        else {
          GetLogger ().Trace ("UpdateLastExecution: thread active");
        }
      }
      this.LastExecution = DateTime.UtcNow;
    }

    /// <summary>
    /// Set the thread as active (update LastExecution to now)
    /// </summary>
    public virtual void SetActive ()
    {
      if (CancellationToken.IsCancellationRequested) {
        GetLogger ().Warn ("SetActive: cancellation was requested, throw OperationCancelledException");
        CancellationToken.ThrowIfCancellationRequested ();
      }

      UpdateLastExecution ();

      // Check if Timeout was reached
      if (this.Timeout.HasValue
          && (m_startDateTime.Add (this.Timeout.Value) < DateTime.UtcNow)) {
        if (GetLogger ().IsErrorEnabled) {
          GetLogger ().Error ($"SetActive: timeout detected after {DateTime.UtcNow.Subtract (m_startDateTime)} when timeout={this.Timeout.Value} at {System.Environment.StackTrace}");
        }
        this.TimeoutAbort = true;
        if (this.Cancel ()) {
          GetLogger ().Info ("SetActive: successfully cancelled");
        }
        CancellationToken.ThrowIfCancellationRequested ();

        GetLogger ().Fatal ("SetActive: abort since cancellation because of the Timeout failed");
        try {
          this.Abort (tryCancelFirst: false);
        }
        catch (PlatformNotSupportedException) {
          GetLogger ().Info ("SetActive: timeout reached, but this.Thread.Abort is not supported on this platform");
        }
        catch (Exception ex) {
          GetLogger ().Error ("SetActive: timeout reached, this.Thread.Abort failed", ex);
        }
        throw new AbortException ("Timeout was reached in SetActive.");
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void PauseCheck ()
    {
      m_pause = true;
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void ResumeCheck ()
    {
      m_pause = false;
    }

    /// <summary>
    /// Exit requested
    /// </summary>
    public bool ExitRequested => m_exitRequested;

    /// <summary>
    /// Set the exit is requested
    /// </summary>
    public void SetExitRequested ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"SetExitRequested");
      }
      m_exitRequested = true;
      m_exitRequestedSource?.Cancel ();
    }

    /// <summary>
    /// Register a call back in case the thread is cancelled
    /// </summary>
    /// <param name="cancelCallback"></param>
    public void RegisterCancelCallback (Action cancelCallback)
    {
      this.CancellationToken.Register (cancelCallback);
    }

    bool SwitchToCancelCompleted ()
    {
      if (null != m_effectiveSource) {
        var effectiveSource = m_effectiveSource;
        m_effectiveSource = null;
        effectiveSource.Dispose ();
      }
      var previousCancelSource = m_cancelSource;
      m_cancelSource = new CancellationTokenSource ();
      previousCancelSource?.Dispose ();

      if (!this.SwitchThreadStatusIfExpected (ThreadStatus.Available, ThreadStatus.Cancelling)) {
        GetLogger ().Fatal ($"SwitchToCancelCompleted: unexpected thread status {this.CurrentThreadStatus}, expected was Cancelling");
        this.SwitchThreadStatus (ThreadStatus.Available);
      }

      if (GetLogger ().IsInfoEnabled) {
        GetLogger ().Info ($"SwitchToCancelCompleted: successful");
      }
      return true;
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IThreadClass"/>
    /// </summary>
    public bool Cancel (TimeSpan? timeout = null)
    {
      try {
        if (GetLogger ().IsInfoEnabled) {
          GetLogger ().Info ("Cancel: cancel was requested");
        }
        PauseCheck ();

        var original = this.SwitchThreadStatus (ThreadStatus.Cancelling);
        if (original.Equals (ThreadStatus.Aborted)) {
          GetLogger ().Error ("Cancel: the thread status was already Aborted");
          return this.Abort (tryCancelFirst: false);
        }
        else if (original.Equals (ThreadStatus.Cancelling)) {
          GetLogger ().Info ($"Cancel: the thread status was already Cancelling, just wait...");
        }
        else {
          if (m_cancelSource is null) {
            GetLogger ().Error ($"Cancel: cancel source has already been disposed (null)");
          }
          else {
            try {
              m_cancelSource?.Cancel ();
            }
            catch (AggregateException ex) {
              GetLogger ().Error ("Cancel: aggregate exception in Cancel ()", ex);
              foreach (var ex1 in ex.InnerExceptions) {
                GetLogger ().Error ("Cancel: inner aggregate exception", ex1);
              }
            }
            catch (Exception ex) {
              GetLogger ().Error ("Cancel: exception in Cancel ()", ex);
            }
          }

          if (!original.Equals (ThreadStatus.Running)) { // && !original.Equals (ThreadStatus.Cancelling)
            if (Thread?.IsAlive ?? false) {
              GetLogger ().Fatal ($"Cancel: status was {original} while the thread is alive, thread state is {this.Thread.ThreadState} => Abort");
              return this.Abort (tryCancelFirst: false);
            }
            else {
              if (GetLogger ().IsInfoEnabled) {
                GetLogger ().Info ($"Cancel: the thread was not started, return at once");
              }
              return SwitchToCancelCompleted ();
            }
          }
        }

        // Wait the status is automatically back to Available (see SetCompleted)
        var endCheckDateTime = DateTime.UtcNow.Add (timeout ?? m_stopTimeout);
        while (DateTime.UtcNow < endCheckDateTime) {
          if (this.CurrentThreadStatus != ThreadStatus.Cancelling) {
            if (GetLogger ().IsInfoEnabled) {
              GetLogger ().Info ($"CancelAsync: completed");
            }
            return true;
          }
          System.Threading.Thread.Sleep (50);
        }

        if (GetLogger ().IsWarnEnabled) {
          GetLogger ().Warn ($"Cancel: not completed after {timeout ?? m_stopTimeout}");
        }
        return false;
      }
      catch (Exception ex) {
        GetLogger ().Fatal ($"Cancel: exception", ex);
        throw;
      }
    }

#if NETSTANDARD
    /// <summary>
    /// <see cref="Lemoine.Threading.IThreadClass" />
    /// </summary>
    public async Task<bool> CancelAsync (TimeSpan? timeout = null)
    {
      try {
        if (GetLogger ().IsInfoEnabled) {
          GetLogger ().Info ("CancelAsync: cancel was requested");
        }
        PauseCheck ();

        var original = this.SwitchThreadStatus (ThreadStatus.Cancelling);
        if (original.Equals (ThreadStatus.Aborted)) {
          GetLogger ().Error ("CancelAsync: the thread status was already Aborted");
          return this.Abort (tryCancelFirst: false);
        }
        else if (original.Equals (ThreadStatus.Cancelling)) {
          GetLogger ().Info ($"CancelAsync: the thread status was already Cancelling, just wait...");
        }
        else {
          if (m_cancelSource is null) {
            GetLogger ().Warn ($"CancelAsync: cancel source has already been disposed (null)");
          }
          else {
            try {
              m_cancelSource?.Cancel ();
            }
            catch (AggregateException ex) {
              GetLogger ().Error ("CancelAsync: aggregate exception in Cancel ()", ex);
              foreach (var ex1 in ex.InnerExceptions) {
                GetLogger ().Error ("CancelAsync: inner aggregate exception", ex1);
              }
            }
            catch (Exception ex) {
              GetLogger ().Error ("CancelAsync: exception in Cancel ()", ex);
            }
          }

          if (!original.Equals (ThreadStatus.Running)) { // && !original.Equals (ThreadStatus.Cancelling)
            if (Thread?.IsAlive ?? false) {
              GetLogger ().Fatal ($"CancelAsync: status was {original} while the thread is alive, thread state is {this.Thread.ThreadState} => Abort");
              return this.Abort (tryCancelFirst: false);
            }
            else {
              if (GetLogger ().IsInfoEnabled) {
                GetLogger ().Info ($"CancelAsync: the thread was not started, return at once");
              }
              return SwitchToCancelCompleted ();
            }
          }
        }

        // Wait the status is automatically back to Available (see SetCompleted)
        var endCheckDateTime = DateTime.UtcNow.Add (timeout ?? m_stopTimeout);
        while (DateTime.UtcNow < endCheckDateTime) {
          if (this.CurrentThreadStatus != ThreadStatus.Cancelling) {
            if (GetLogger ().IsInfoEnabled) {
              GetLogger ().Info ($"CancelAsync: completed");
            }
            return true;
          }
          await Task.Delay (50);
        }

        if (GetLogger ().IsWarnEnabled) {
          GetLogger ().Warn ($"CancelAsync: not completed after {timeout ?? m_stopTimeout}");
        }
        return false;
      }
      catch (Exception ex) {
        GetLogger ().Fatal ($"CancelAsync: exception", ex);
        throw;
      }
    }
#endif // NETSTANDARD

    /// <summary>
    /// <see cref="Lemoine.Threading.IThreadClass" />
    /// </summary>
    public void Interrupt ()
    {
      try {
        var result = Cancel (TimeSpan.FromTicks (0));
        if (result) {
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ($"Interrupt: Cancel returned {result}");
          }
        }
        else {
          GetLogger ().Error ($"Interrupt: Cancel retuned {result}");
        }
      }
      catch (Exception ex) {
        GetLogger ().Error ($"Interrupt: Cancel failed", ex);
      }
    }

    /// <summary>
    /// Main method of the tread:
    /// <item>initialize some properties</item>
    /// <item>run the Run method</item>
    /// </summary>
    /// <param name="stateInfo">cancellation token</param>
    protected virtual void RunMainThreadMethod (object stateInfo)
    {
      try {
        RunInThread (stateInfo);
      }
      catch (Exception ex) {
        if (this.ExitOnException) {
          GetLogger ().Fatal ("RunMainThreadMethod: exception in RunInThread => exit", ex);
          Lemoine.Core.Environment.ForceExit ();
        }
        else {
          GetLogger ().Error ("RunMainInThreadMethod: exception in RunInThread", ex);
        }
      }
    }

    /// <summary>
    /// Main method of the thread to override
    /// </summary>
    protected abstract void Run (CancellationToken cancellationToken);

    /// <summary>
    /// Run in pool with the default action
    /// </summary>
    /// <param name="stateInfo">cancellation token</param>
    public virtual void RunInPool (Object stateInfo)
    {
      RunInPool (stateInfo, Run);
    }

    /// <summary>
    /// Run in pool
    /// </summary>
    /// <param name="stateInfo">cancellation token</param>
    /// <param name="action">action</param>
    public virtual void RunInPool (Object stateInfo, Action<CancellationToken> action)
    {
      if (null != m_thread) {
        GetLogger ().Error ("RunInPool: thread was not null");
        m_thread = null;
      }
      RunInThread (stateInfo, action);
    }

    /// <summary>
    /// Request this class for execution
    /// 
    /// <see cref="IThreadStatusSupport"/>
    /// </summary>
    /// <returns>success</returns>
    public bool Request ()
    {
      return SwitchThreadStatusIfExpected (ThreadStatus.Requested, ThreadStatus.Available);
    }

    /// <summary>
    /// <see cref="IThreadStatusSupport"/>
    /// 
    /// Same as TryResetRequested, but log an error if the original status was not Requested
    /// </summary>
    /// <returns></returns>
    public bool ResetRequested ()
    {
      var available = (Int32)ThreadStatus.Available;
      var requested = (Int32)ThreadStatus.Requested;
      var original = Interlocked.CompareExchange (ref m_threadStatus, available, requested);
      if (original != requested) {
        GetLogger ().Error ($"ResetRequested: status was {original}, not Requested and was not reset to available");
        return false;
      }
      else {
        return true;
      }
    }

    /// <summary>
    /// <see cref="IThreadStatusSupport"/>
    /// 
    /// TODO: really ? What to do in case of exception...
    /// 
    /// ResetToCompleted rather...
    /// </summary>
    public void ResetToAvailable ()
    {
      try {
        var available = (Int32)ThreadStatus.Available;
        var original = Interlocked.CompareExchange (ref m_threadStatus, available, m_threadStatus);
        if (original != available) {
          GetLogger ().Warn ($"ResetToAvailable: state {original} was reset to available");
        }
      }
      catch (Exception ex) {
        GetLogger ().Fatal ($"ResetToAvailable: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Check if it can be run, if it is not currently running on an another thread
    /// 
    /// <see cref="IThreadStatusSupport"/>
    /// </summary>
    /// <returns></returns>
    public bool CanRun ()
    {
      var available = (Int32)ThreadStatus.Available;
      return (available == Interlocked.CompareExchange (ref m_threadStatus, available, available));
    }

    /// <summary>
    /// Switch to running only if already in Requested status
    /// 
    /// The Requested status is expected.
    /// 
    /// If the current status is Available, then the status is directly switched to Running
    /// </summary>
    /// <returns></returns>
    protected bool SwitchToRunning ()
    {
      var original = Interlocked.CompareExchange (ref m_threadStatus, (Int32)ThreadStatus.Running, (Int32)ThreadStatus.Requested);
      switch ((ThreadStatus)original) {
      case ThreadStatus.Requested:
        return true;
      case ThreadStatus.Available:
        if (SwitchThreadStatusIfExpected (ThreadStatus.Running, ThreadStatus.Available)) {
          if (GetLogger ().IsWarnEnabled) {
            GetLogger ().Warn ($"SwitchToRunning: direct switch from Available to Running");
          }
          return true;
        }
        else {
          GetLogger ().Error ($"SwitchToRunning: could not switch directly from Available to Running");
          return false;
        }
      case ThreadStatus.Cancelling:
        if (GetLogger ().IsInfoEnabled) {
          GetLogger ().Info ($"SwitchToRunning: the original status was Cancelling => return false");
        }
        return false;
      default:
        if (GetLogger ().IsWarnEnabled) {
          GetLogger ().Warn ($"SwitchToRunning: the original status {original} was not Requested, nor Available, nor Cancelling");
        }
        return false;
      }
    }

    /// <summary>
    /// Switch the state back to Available
    /// </summary>
    /// <returns>The state was Running or Cancelling</returns>
    protected bool SetCompleted ()
    {
      try {
        var original = Interlocked.CompareExchange (ref m_threadStatus, (Int32)ThreadStatus.Available, (Int32)ThreadStatus.Running);
        switch ((ThreadStatus)original) {
        case ThreadStatus.Running:
          return true;
        case ThreadStatus.Available:
          GetLogger ().Warn ($"SetCompleted: the status was already Available");
          return false;
        case ThreadStatus.Cancelling:
          GetLogger ().Info ($"SetCompleted: end of cancelling process");
          return SwitchToCancelCompleted ();
        default:
          GetLogger ().Fatal ($"SetCompleted: the status was {original}, not Running (or Available) or Cancelling");
          throw new Lemoine.Threading.AbortException ("Unexpected thread status.");
        }
      }
      catch (Exception ex) {
        GetLogger ().Fatal ($"SetCompleted: unexpected error", ex);
        throw;
      }
    }

    /// <summary>
    /// Switch the thread status
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    protected ThreadStatus SwitchThreadStatus (ThreadStatus target)
    {
      var original = Interlocked.CompareExchange (ref m_threadStatus, (Int32)target, (Int32)m_threadStatus);
      return (ThreadStatus)original;
    }

    /// <summary>
    /// Switch to a specficic state only if the expected state in parameter is the current one
    /// </summary>
    /// <param name="target"></param>
    /// <param name="expected"></param>
    /// <param name="logFailure"></param>
    /// <returns></returns>
    protected bool SwitchThreadStatusIfExpected (ThreadStatus target, ThreadStatus expected, bool logFailure = true)
    {
      var original = Interlocked.CompareExchange (ref m_threadStatus, (Int32)target, (Int32)expected);
      if ((Int32)expected != original) {
        if (logFailure && GetLogger ().IsWarnEnabled) {
          GetLogger ().Warn ($"CompareExchangeThreadStatus: the original status {original} was not the expected one {expected}");
        }
        return false;
      }
      return true;
    }

    /// <summary>
    /// Main thread method with the default action Run
    /// 
    /// The object must be in requested status first to be run
    /// </summary>
    /// <param name="stateInfo"></param>
    protected virtual void RunInThread (Object stateInfo)
    {
      RunInThread (stateInfo, Run);
    }

    /// <summary>
    /// Main thread method
    /// 
    /// The object must be in requested status first to be run
    /// </summary>
    /// <param name="stateInfo"></param>
    /// <param name="action"></param>
    protected virtual void RunInThread (Object stateInfo, Action<CancellationToken> action)
    {
      if (null == stateInfo) {
        GetLogger ().Error ("RunInThread: stateInfo was null, use m_cancel");
        RunInThread (this.CancellationToken);
        return;
      }

      CancellationToken cancellationToken;
      try {
        cancellationToken = (CancellationToken)stateInfo;
      }
      catch (Exception ex) {
        GetLogger ().Fatal ($"RunInThread: {stateInfo} is not a cancellation token", ex);
        RunInThread (this.CancellationToken);
        return;
      }

      if (!SwitchToRunning ()) {
        return;
      }

      bool allowRestart = true;
      try { // m_running has just been hold, else the block above would have returned
        if (null != m_effectiveSource) {
          var effectiveSource = m_effectiveSource;
          m_effectiveSource = null;
          effectiveSource.Dispose ();
        }
        m_effectiveSource = CancellationTokenSource
          .CreateLinkedTokenSource (m_cancelSource?.Token ?? CancellationToken.None, m_exitRequestedSource?.Token ?? CancellationToken.None, cancellationToken);
        var token = m_effectiveSource.Token;
        ResetStartDateTime ();
        this.TimeoutAbort = false;

        ResumeCheck ();

        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ("RunInThread: the lock m_running has just been acquired");
        }
        SetActive ();

        if (null != action) {
          action (token);
        }
        else { // null == action
          Run (token);
        }
        allowRestart = false;
      }
      catch (OperationCanceledException ex) {
        allowRestart = false;
        GetLogger ().Info ("RunInThread: operation canceled", ex);
      }
      catch (Lemoine.Threading.AbortException ex) {
        allowRestart = false;
        GetLogger ().Error ("RunInThread: AbortException => SetExitRequested", ex);
        SetExitRequested ();
      }
      catch (OutOfMemoryException ex) {
        allowRestart = false;
        GetLogger ().Error ("Run: OutOfMemoryException, give up", ex);
        SetExitRequested ();
      }
      catch (Exception ex) {
        if (ExceptionTest.RequiresExit (ex, log)) {
          allowRestart = false;
          // OutOfMemory or NHibernate.TransactionException which is not temporary
          // (TransactionSerializationFailure or TimeoutFailure)
          if (!(ex is OutOfMemoryException)) {
            GetLogger ().Error ($"RunInThread: Exception with inner exception {ex.InnerException} requires to exit, give up", ex);
          }
          SetExitRequested ();
        }
        else if (ExceptionTest.IsStale (ex, log)) {
          allowRestart = true;
          if (GetLogger ().IsWarnEnabled) {
            GetLogger ().Warn ("RunInThread: Stale exception", ex);
          }
        }
        else if (ExceptionTest.IsTemporaryWithDelay (ex, log)) {
          allowRestart = true;
          var temporaryWithDelayExceptionSleepTime = Lemoine.Info.ConfigSet
            .LoadAndGet (TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_KEY, TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_DEFAULT);
          GetLogger ().Warn ($"RunInThread: temporary with delay exception inner {ex.InnerException}, try again after sleeping {temporaryWithDelayExceptionSleepTime}", ex);
          this.Sleep (temporaryWithDelayExceptionSleepTime, this.CancellationToken);
        }
        else if (ExceptionTest.IsTemporary (ex, log)) {
          allowRestart = true;
          if (GetLogger ().IsWarnEnabled) {
            GetLogger ().Warn ("RunInThread: temporary (serialization, transaction aborted) failure => try again later", ex);
          }
        }
        else {
          allowRestart = true;
          GetLogger ().Error ("RunInThread: an uncaught exception occurred. The thread is interrupted", ex);
        }
      }
      finally { // Free any potential m_running lock
        PauseCheck ();
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ($"RunInThread: free the running lock, Run is completed, allowRestart={allowRestart}");
        }
        SetCompleted ();
        if (allowRestart) {
          m_error = true;
          ResumeCheck ();
        }
      }
    }

    /// <summary>
    /// Run directly the main execution method
    /// 
    /// The status must be Available
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <exception cref="InvalidOperationException">The status was not Available</exception>
    public void RunDirectly (CancellationToken? cancellationToken = null)
    {
      RunDirectly (Run, cancellationToken);
    }

    /// <summary>
    /// Run directly the main execution method
    /// 
    /// The status must be Available
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="InvalidOperationException">The status was not Available</exception>
    public void RunDirectly (Action<CancellationToken> action, CancellationToken? cancellationToken = null)
    {
      if (!Request ()) {
        GetLogger ().Error ($"RunDirectly: Request failed");
        throw new InvalidOperationException ("Thread status was not available.");
      }
      if (null != m_thread) {
        GetLogger ().Info ("RunDirectly: thread was not null");
        m_thread = null;
      }
      if (!SwitchToRunning ()) {
        GetLogger ().Fatal ($"RunDirectly: switch to Running status failed (unexpected)");
        throw new InvalidOperationException ("Thread status was not running.");
      }
      try {
        ResetStartDateTime ();
        this.TimeoutAbort = false;
        using (var localTokenSource = CancellationTokenSource
          .CreateLinkedTokenSource (this.CancellationToken, cancellationToken ?? CancellationToken.None)) {
          var token = localTokenSource.Token;
          action (token);
        }
      }
      finally {
        SetCompleted ();
      }
    }

#if NETSTANDARD
    /// <summary>
    /// Request the lock (check it is available) and run asynchronously
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">The status was not Available</exception>
    public async Task RequestAndRunAsync (CancellationToken? cancellationToken = null)
    {
      await RequestAndRunAsync (Run, cancellationToken);
    }

    /// <summary>
    /// Request the lock (check it is available) and run asynchronously
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">The status was not Available</exception>
    public async Task RequestAndRunAsync (Action<CancellationToken> action, CancellationToken? cancellationToken = null)
    {
      if (!Request ()) {
        GetLogger ().Error ($"RequestAndRunAsync: request failed");
        throw new InvalidOperationException ("Thread status was not available.");
      }
      await RunAsync (action, cancellationToken);
    }

    /// <summary>
    /// Run asynchronously
    /// 
    /// Run Request () first to make sure the status is Requested
    /// </summary>
    /// <param name="cancellationToken">Optional</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">The thread status was not Requested</exception>
    public async Task RunAsync (CancellationToken? cancellationToken = null)
    {
      await RunAsync (Run, cancellationToken);
    }

    /// <summary>
    /// Run asynchronously
    /// 
    /// Run Request () first to make sure the status is Requested
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellationToken">Optional</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">The thread status was not Requested</exception>
    public async Task RunAsync (Action<CancellationToken> action, CancellationToken? cancellationToken = null)
    {
      if (null != m_thread) {
        GetLogger ().Info ("RunAsync: thread was not null");
        m_thread = null;
      }
      if (!SwitchToRunning ()) {
        GetLogger ().Error ($"RunAsync: the status was not Requested => do nothing");
        throw new InvalidOperationException ("Thread status was not Requested.");
      }

      try {
        ResetStartDateTime ();
        this.TimeoutAbort = false;

        using (var localTokenSource = CancellationTokenSource
          .CreateLinkedTokenSource (this.CancellationToken, cancellationToken ?? CancellationToken.None)) {
          var token = localTokenSource.Token;
          await Task.Run (() => action (token), token);
        }
      }
      catch (Exception ex) {
        GetLogger ().Exception (ex, "RunAsync");
        throw;
      }
      finally {
        SetCompleted ();
      }
    }
#endif // NETSTANDARD

    /// <summary>
    /// Start an action in the thread pool
    /// </summary>
    /// <param name="action"></param>
    /// <param name="actionName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public bool StartInThreadPool (Action<CancellationToken> action, string actionName, CancellationToken cancellationToken)
    {
      if (!this.Request ()) {
        GetLogger ().Info ($"StartInThreadPool({actionName}): skip the execution since Request() returned false");
        return false;
      }
      if (null != m_thread) {
        GetLogger ().Info ("StartInThreadPool: thread was not null");
        m_thread = null;
      }

      void waitCallback (object x) => RunInPool (x, action);
      if (!ThreadPool.QueueUserWorkItem (new WaitCallback (waitCallback), cancellationToken)) {
        GetLogger ().Warn ($"StartInThreadPool({actionName}): task was not queued in the thread pool");
        this.ResetRequested ();
        return false;
      }
      else { // A thread was queued
        return true;
      }
    }

    /// <summary>
    /// Start the default action in the thread pool
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public bool StartInThreadPool (CancellationToken cancellationToken)
    {
      return StartInThreadPool (Run, "Run", cancellationToken);
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IThreadClass" />
    /// </summary>
    /// <returns></returns>
    public bool IsCheckInPause ()
    {
      return m_pause;
    }

    /// <summary>
    /// Get the logger
    /// </summary>
    /// <returns></returns>
    public abstract ILog GetLogger ();

    void SetApartmentState (ApartmentState apartmentState)
    {
      Debug.Assert (null != this.Thread);
      if (null == this.Thread) {
        GetLogger ().Fatal ($"SetApartmentState: Thread was null");
        return;
      }

      if (!apartmentState.Equals (ApartmentState.Unknown)) {
        if (m_apartmentState.Equals (apartmentState)) {
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ($"SetApartmentState: the apartment state was already set to {apartmentState}");
          }
        }
        else if (m_apartmentState.Equals (ApartmentState.Unknown)) {
          try {
            if (this.Thread.TrySetApartmentState (apartmentState)) {
              m_apartmentState = apartmentState;
            }
            else {
              GetLogger ().Error ($"SetApartmentState: the new apartment state {apartmentState} could not be set. Previously: {m_apartmentState}");
            }
          }
          catch (PlatformNotSupportedException ex) {
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ("SetApartmentState: SetApartmentState not supported on Linux and MacOs", ex);
            }
          }
          catch (ThreadStartException ex) {
            GetLogger ().Error ($"SetApartmentState: SetApartmentState failed with ThreadStateException: already started ? ThreadState is {this.Thread.ThreadState}", ex);
          }
          catch (Exception ex) {
            GetLogger ().Error ("SetApartmentState: SetApartmentState failed", ex);
          }
        }
        else {
          GetLogger ().Error ($"SetApartmentState: this is not possible to change the apartment state from {m_apartmentState} to {apartmentState}");
        }
      }
    }

    /// <summary>
    /// Start the thread
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="apartmentState">Default is Unknown (which is set to MTA when the thread is started)</param>
    public void Start (CancellationToken? cancellationToken = null, ApartmentState apartmentState = ApartmentState.Unknown)
    {
      if (!Request ()) {
        GetLogger ().Error ("Start: switch to request state failed, give up");
      }
      else {
        m_thread = new System.Threading.Thread (new ParameterizedThreadStart (this.RunMainThreadMethod));
        Debug.Assert (null != this.Thread);
        SetApartmentState (apartmentState);
        this.Thread.Start (cancellationToken ?? CancellationToken.None);
      }
    }

    /// <summary>
    /// Start the thread if it in Unstarted state
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="apartmentState"></param>
    public void StartIfUnstarted (CancellationToken? cancellationToken = null, ApartmentState apartmentState = ApartmentState.MTA)
    {
      if (null == this.Thread) {
        this.Start (cancellationToken, apartmentState);
      }
      else { // null != this.Thread
        switch (this.Thread.ThreadState) {
        case System.Threading.ThreadState.Background:
        case System.Threading.ThreadState.Running:
          // Nothing to do, already running
          break;
        case System.Threading.ThreadState.Stopped:
        case System.Threading.ThreadState.Aborted:
        case System.Threading.ThreadState.Suspended:
          GetLogger ().Warn ($"StartIfUnstarted: thread in unexpected state {this?.Thread.ThreadState}, {m_threadStatus}, restart it");
          SetCompleted ();
          goto case System.Threading.ThreadState.Unstarted;
        case System.Threading.ThreadState.Unstarted:
          this.Start (cancellationToken, apartmentState);
          break;
        case System.Threading.ThreadState.WaitSleepJoin:
          GetLogger ().Warn ("StartIfUnstarted: thread in state WaitSleepJoin, do nothing");
          break;
        case System.Threading.ThreadState.StopRequested:
        case System.Threading.ThreadState.AbortRequested:
          GetLogger ().Warn ($"StartIfUnstarted: thread in unexpected requested state {this?.Thread.ThreadState}");
          break;
        }
      }
    }

    /// <summary>
    /// <see cref="IThreadClass"/>
    /// </summary>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public bool Restart (TimeSpan? timeout = null)
    {
      PauseCheck ();

      try {
        if (!Cancel (timeout)) {
          GetLogger ().Error ($"Restart: Cancel failed to complete in {timeout ?? this.Timeout}");
          return false;
        }
      }
      catch (Exception ex) {
        GetLogger ().Error ($"Restart: exception in Cancel", ex);
        return false;
      }

      if (!Request ()) {
        GetLogger ().Error ($"Restart: new switch to Request state failed");
        return false;
      }
      if (GetLogger ().IsInfoEnabled) {
        GetLogger ().Info ($"Restart: state successfully switched back to Requested");
      }
      m_thread = new System.Threading.Thread (new ParameterizedThreadStart (this.RunMainThreadMethod));
      Debug.Assert (null != this.Thread);
      try {
        this.Thread.Start (this.CancellationToken);
      }
      catch (Exception ex) {
        GetLogger ().Error ($"Restart: restart of thread failed", ex);
        return false;
      }
      ResumeCheck ();
      return true;
    }

    /// <summary>
    /// Abort the thread
    /// </summary>
    /// <param name="tryCancelFirst"></param>
    /// <returns>success</returns>
    public virtual bool Abort (bool tryCancelFirst = true)
    {
      if (tryCancelFirst || (null == this.Thread)) {
        try {
          if (this.Cancel ()) {
            if (GetLogger ().IsInfoEnabled) {
              GetLogger ().Info ("Abort: Cancel was successful");
            }
            this.SwitchThreadStatus (ThreadStatus.Aborted);
            return true;
          }
          else {
            GetLogger ().Info ($"Abort: Cancel failed");
          }
        }
        catch (Exception ex) {
          GetLogger ().Error ($"Abort: Cancel failed", ex);
        }
      }
      else {
        this.SwitchThreadStatus (ThreadStatus.Cancelling);
      }

      if (null == this.Thread) {
        this.SwitchThreadStatus (ThreadStatus.Aborted);
        return false;
      }
      else { // null != this.Thread
        if (this?.Thread.ThreadState.Equals (System.Threading.ThreadState.Unstarted) ?? true) {
          this.SwitchThreadStatus (ThreadStatus.Aborted);
          return true;
        }
        try {
          this?.Thread.Abort ();
        }
        catch (PlatformNotSupportedException) { // on .NET Core
          if (!tryCancelFirst) {
            GetLogger ().Warn ("Abort: PlatformNotSupportedException: switch to cancel again");
            var result = Cancel ();
            GetLogger ().Info ($"Abort: PlatformNotSupportException and Cancel returned {result}");
            this.SwitchThreadStatus (ThreadStatus.Aborted);
            return result;
          }
        }
        catch (Exception ex) {
          GetLogger ().Error ("Abort: Abort exception", ex);
        }
        try {
          if (!this?.Thread.Join (m_stopTimeout) ?? false) {
            GetLogger ().Warn ($"Abort: thread still active after {m_stopTimeout}");
            this.SwitchThreadStatus (ThreadStatus.Aborted);
            return false;
          }
        }
        catch (ThreadStateException ex) {
          GetLogger ().Warn ("Abort: Join of thread failed because the thread has not been started", ex);
        }
        catch (Exception ex) {
          GetLogger ().Error ("Abort: Join of thread exception", ex);
        }

        if (GetLogger ().IsInfoEnabled) {
          GetLogger ().Info ($"Abort: success");
        }
        this.SwitchThreadStatus (ThreadStatus.Aborted);
        return true;
      }
    }

    /// <summary>
    /// Do nothing
    /// </summary>
    /// <param name="cancellationToken"></param>
    protected void DoNothing (CancellationToken cancellationToken)
    {
      GetLogger ().Error ("DoNothing");
      cancellationToken.WaitHandle.WaitOne ();
    }
    #endregion // Methods

    #region IDisposable implementation
    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    ~ThreadClass () => Dispose (false);

    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    public void Dispose ()
    {
      Dispose (true);
      GC.SuppressFinalize (this);
    }

    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose (bool disposing)
    {
      if (m_disposed) {
        return;
      }

      if (disposing) {
        var effectiveSource = m_effectiveSource;
        m_effectiveSource = null;
        effectiveSource?.Dispose ();

        var cancelSource = m_cancelSource;
        m_cancelSource = null;
        cancelSource?.Dispose ();

        var exitRequestedSource = m_exitRequestedSource;
        m_exitRequestedSource = null;
        exitRequestedSource?.Dispose ();
      }

      m_disposed = true;
    }
    #endregion // IDisposable implementation
  }
}
