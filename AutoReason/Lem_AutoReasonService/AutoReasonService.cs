// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using System.Diagnostics;

using Lemoine.AutoReason;
using Lemoine.Core.Log;
using Lemoine.Threading;
using Lemoine.Core.Hosting;
using Pulse.Business.Computer;
using Atsora.FsCore;

namespace Lem_AutoReasonService
{
  /// <summary>
  /// Main class of service Lem_AutoReasonService
  /// </summary>
  public sealed class AutoReasonService
    : Lemoine.Threading.IThreadServiceAsync, IDisposable
  {
    static readonly string NOT_RESPONDING_TIMEOUT_KEY = "AutoReason.CheckThreads.Timeout";
    static readonly TimeSpan NOT_RESPONDING_TIMEOUT_DEFAULT = TimeSpan.FromMinutes (5);

    #region Members
    readonly IApplicationInitializer m_applicationInitializer;
    readonly ILctrChecker m_lctrChecker;
    readonly CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource ();
    bool m_disposed = false;
    AutoReasonEngine m_autoReasonEngine;
    readonly CheckThreadsAndProcesses m_checkThreads = new CheckThreadsAndProcesses ();
    #endregion

    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonService).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonService (IApplicationInitializer applicationInitializer, ILctrChecker lctrChecker)
    {
      Debug.Assert (null != applicationInitializer);
      Debug.Assert (null != lctrChecker);

      SystemKeyChecker.CheckFeature ("AutoReason");

      m_applicationInitializer = applicationInitializer;
      m_lctrChecker = lctrChecker;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// <see cref="IThreadService"/>
    /// </summary>
    public void Initialize ()
    {
      System.Threading.Tasks.Task.Run (() => InitializeAsync (CancellationToken.None)).Wait ();
    }

    /// <summary>
    /// Use the default OnStart method
    /// </summary>
    public async System.Threading.Tasks.Task InitializeAsync (CancellationToken cancellationToken)
    {
      await InitializeThreadsAsync (cancellationToken);
    }

    async System.Threading.Tasks.Task InitializeThreadsAsync (CancellationToken cancellationToken)
    {
      using (var linkedCancellationTokenSource = CancellationTokenSource
        .CreateLinkedTokenSource (m_cancellationTokenSource.Token, cancellationToken)) {
        var linkedToken = linkedCancellationTokenSource.Token;

        try {
          await m_applicationInitializer.InitializeApplicationAsync (linkedToken);
        }
        catch (OperationCanceledException) {
          return;
        }
        catch (Exception ex) {
          log.Error ("InitializeThreadsAsync: InitializeApplication failed", ex);
          throw;
        }

        if (linkedToken.IsCancellationRequested) {
          return;
        }

        log.Info ("InitializeThreadsAsync: activate DaySlotCache");
        Lemoine.GDBPersistentClasses.DaySlotCache.Activate ();

        if (null == m_autoReasonEngine) {
          var isLctr = m_lctrChecker.IsLctr ();
          m_autoReasonEngine = new AutoReasonEngine (isLctr);
        }

        if (linkedToken.IsCancellationRequested) {
          // In case Stop is run before the service is fully initialized
          return;
        }

        m_autoReasonEngine.Start (linkedToken);

        // - Create the thread that checks the other threads
        m_checkThreads.AddThread (m_autoReasonEngine);
        m_checkThreads.NotRespondingTimeout = Lemoine.Info.ConfigSet
          .LoadAndGet<TimeSpan> (NOT_RESPONDING_TIMEOUT_KEY, NOT_RESPONDING_TIMEOUT_DEFAULT);
        m_checkThreads.Start (linkedToken);

        // - Check no 'exit' was requested
        while (!m_autoReasonEngine.ExitRequested && !m_checkThreads.ExitRequested) {
          await System.Threading.Tasks.Task.Delay (100, linkedToken);
          if (linkedToken.IsCancellationRequested
            && !m_autoReasonEngine.ExitRequested && !m_checkThreads.ExitRequested) {
            // OnStop was called, return
            LogManager.SetApplicationStopping ();
            log.Info ("InitializeThreadsAsync: cancellation requested (OnStop called), return");
            return;
          }
        }
      }

      LogManager.SetApplicationStopping ();
      log.Fatal ("InitializeThreadsAsync: exit was requested by one of the two main threads");
      try {
        if (!m_checkThreads.Abort ()) {
          log.Warn ("InitializeThreadsAsync: abort of checkThreads failed");
        }
        log.Fatal ("InitializeThreadsAsync: checkThreads aborted because Exit was requested");
        if (!m_autoReasonEngine.Abort ()) {
          log.Warn ("InitializeThreadsAsync: abort of autoReasonEngine failed");
        }
        log.Fatal ("InitializeThreadsAsync: autoReasonEngine aborted because Exit was requested");
      }
      finally {
        Lemoine.Core.Environment.LogAndForceExit (log);
      }
    }

    /// <summary>
    /// Stop this service.
    /// </summary>
    public void OnStop ()
    {
      LogManager.SetApplicationStopping ();

      m_cancellationTokenSource?.Cancel ();

      // - Stop the checking thread
      m_checkThreads?.Abort ();
      m_autoReasonEngine?.Abort ();
    }
    #endregion // Methods

    #region IDisposable implementation
    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    ~AutoReasonService () => Dispose (false);

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
    void Dispose (bool disposing)
    {
      if (m_disposed) {
        return;
      }

      if (disposing) {
        m_autoReasonEngine?.Dispose ();
        m_cancellationTokenSource?.Dispose ();
      }

      m_disposed = true;
    }
    #endregion // IDisposable implementation
  }
}
