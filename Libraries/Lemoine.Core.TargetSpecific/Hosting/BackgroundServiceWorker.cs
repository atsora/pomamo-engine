// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NETCOREAPP

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Threading;
using Microsoft.Extensions.Hosting;

namespace Lemoine.Core.Hosting
{
  /// <summary>
  /// BackgroundServiceWorker
  /// </summary>
  public abstract class BackgroundServiceWorker
    : BackgroundService
    , IChecked
  {
    static readonly string ACTIVE_STACK_TRACE_KEY = "ThreadClass.Active.StackTrace";
    static readonly bool ACTIVE_STACK_TRACE_DEFAULT = false;

    static readonly TimeSpan SLEEP_TIME_DEFAULT = TimeSpan.FromSeconds (10);

    #region Members
    bool m_disposed = false;
    readonly CancellationTokenSource m_cancelTokenSource = new CancellationTokenSource ();
    DateTime m_startDateTime; // To be accessed from here only
    ReaderWriterLock m_runningParamLock = new ReaderWriterLock ();
    DateTime m_lastExecution = DateTime.UtcNow;
    volatile bool m_pause = true;
    bool m_activeStackTrace = Lemoine.Info.ConfigSet.LoadAndGet (ACTIVE_STACK_TRACE_KEY, ACTIVE_STACK_TRACE_DEFAULT);
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Start date/time of the thread
    /// </summary>
    protected DateTime StartDateTime
    {
      get { return m_startDateTime; }
    }

    /// <summary>
    /// Was the worker requested to be interrupted ?
    /// </summary>
    public bool Interrupted
    {
      get { return m_cancelTokenSource.IsCancellationRequested; }
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
    /// Sleep time
    /// </summary>
    protected TimeSpan SleepTime
    {
      get; set;
    }
     = SLEEP_TIME_DEFAULT;
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public BackgroundServiceWorker ()
    {
    }
    #endregion // Constructors

    #region BackgroundService implementation
    /// <summary>
    /// Background service implementation
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync (CancellationToken stoppingToken)
    {
      try {
        m_startDateTime = DateTime.UtcNow;
        await RunAsync (stoppingToken);
      }
      catch (Exception ex) {
        try {
          GetLogger ().Fatal ("Run: exception", ex);
        }
        catch (Exception) { }

        Exit (ex);
      }
    }

    /// <summary>
    /// Default implementation with the default sleep time
    /// 
    /// Unless there is serious problem, this method should not throw any exception
    /// when the stoppingToken is cancelled
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected virtual async Task RunAsync (CancellationToken stoppingToken)
    {
      await RunAsync (this.SleepTime, stoppingToken);
    }

    /// <summary>
    /// Default implementation with a specified sleep time
    /// 
    /// Unless there is serious problem, this method should not throw any exception
    /// when the stoppingToken is cancelled
    /// </summary>
    /// <param name="sleepTime"></param>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected virtual async Task RunAsync (TimeSpan sleepTime, CancellationToken stoppingToken)
    {
      using (var tokenSource = CancellationTokenSource.CreateLinkedTokenSource (m_cancelTokenSource.Token, stoppingToken)) {
        var token = tokenSource.Token;
        while (!tokenSource.IsCancellationRequested) {
          await RunOnceAsync (token);
          SetActive ();
          token.WaitHandle.WaitOne (sleepTime);
        }
      }
    }

    /// <summary>
    /// Method to override for a single execution
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected virtual async Task RunOnceAsync (CancellationToken stoppingToken)
    {
      await Task.Delay (0);
    }

    /// <summary>
    /// Default method to sleep with the default sleep time
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected virtual async Task Sleep (CancellationToken stoppingToken)
    {
      await this.Sleep (this.SleepTime, stoppingToken);
    }

    /// <summary>
    /// Default method to sleep with the specified sleep time
    /// </summary>
    /// <param name="s"></param>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected virtual async Task Sleep (TimeSpan s, CancellationToken stoppingToken)
    {
      await Task.Run (() => stoppingToken.WaitHandle.WaitOne (s));
    }
    #endregion // BackgroundService implementation

    #region IChecked implementation
    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked"/>
    /// </summary>
    public void PauseCheck ()
    {
      m_pause = true;
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked"/>
    /// </summary>
    public void ResumeCheck ()
    {
      m_pause = false;
    }

    /// <summary>
    /// Is the worker check in pause ?
    /// </summary>
    /// <returns></returns>
    public bool IsCheckInPause ()
    {
      return m_pause;
    }

    /// <summary>
    /// Interrupt the background service
    /// 
    /// Cancel it
    /// </summary>
    public void Interrupt ()
    {
      Cancel ();
    }

    /// <summary>
    /// Cancel the background service
    /// </summary>
    public void Cancel ()
    {
      m_cancelTokenSource.Cancel ();
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked"/>
    /// </summary>
    public void SetActive ()
    {
      if (m_cancelTokenSource.IsCancellationRequested) {
        GetLogger ().Warn ("SetActive: canellation was requested");
      }
      m_cancelTokenSource.Token.ThrowIfCancellationRequested ();

      UpdateLastExecution ();
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
    #endregion // IChecked implementation

    /// <summary>
    /// Get the logger
    /// </summary>
    /// <returns></returns>
    public abstract ILog GetLogger ();

    /// <summary>
    /// Force exit
    /// </summary>
    /// <param name="ex"></param>
    protected void Exit (Exception ex)
    {
      // TODO: is it possible to do something softer ?
      Lemoine.Core.Environment.LogAndForceExit (ex, GetLogger ());
    }

    /// <summary>
    /// Force exit
    /// </summary>
    protected void Exit ()
    {
      // TODO: is it possible to do something softer ?
      Lemoine.Core.Environment.LogAndForceExit (GetLogger ());
    }

    #region IDisposable implementation
    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    ~BackgroundServiceWorker ()
    {
      Dispose (false);
    }

    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    public override void Dispose ()
    {
      Dispose (true);
      base.Dispose ();
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
        m_cancelTokenSource.Dispose ();
      }

      m_disposed = true;
    }
    #endregion // IDisposable implementation
  }
}

#endif // NETCOREAPP
