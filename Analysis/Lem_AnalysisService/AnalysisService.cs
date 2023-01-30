// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;

using Lemoine.Analysis;
using Lemoine.GDBPersistentClasses;
using Lemoine.Threading;
using Lemoine.Core.Log;
using Lemoine.Core.Hosting;
using System.Diagnostics;
using Lemoine.Core.Plugin;

namespace Lem_AnalysisService
{
  /// <summary>
  /// Main class of service Lem_AnalysisService
  /// </summary>
  public sealed class AnalysisService
    : Lemoine.Threading.IThreadServiceAsync, IDisposable
  {
    const string NOT_RESPONDING_TIMEOUT_KEY = "Analysis.CheckThreads.Timeout";
    static readonly TimeSpan NOT_RESPONDING_TIMEOUT_DEFAULT = TimeSpan.FromMinutes (5);

    readonly IApplicationInitializer m_applicationInitializer;
    readonly CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource ();
    bool m_disposed = false;
    readonly ActivityAnalysis m_activityAnalysis = new ActivityAnalysis ();
    readonly CheckThreadsAndProcesses m_checkThreads = new CheckThreadsAndProcesses ();

    readonly ILog log = LogManager.GetLogger (typeof (AnalysisService).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public AnalysisService (IApplicationInitializer applicationInitializer)
    {
      Debug.Assert (null != applicationInitializer);

      m_applicationInitializer = applicationInitializer;
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

        DaySlotCache.Activate ();
        MachineCache.Activate ();

        m_activityAnalysis.Initialize ();
        m_activityAnalysis.Start (linkedToken);

        // - Create the thread that checks the other threads
        m_checkThreads.AddThread (m_activityAnalysis);
        m_checkThreads.NotRespondingTimeout = Lemoine.Info.ConfigSet
          .LoadAndGet<TimeSpan> (NOT_RESPONDING_TIMEOUT_KEY, NOT_RESPONDING_TIMEOUT_DEFAULT);
        m_checkThreads.Start (linkedToken);

        // - Check no 'exit' was requested
        while (!m_activityAnalysis.ExitRequested
               && !m_checkThreads.ExitRequested) {
          await System.Threading.Tasks.Task.Delay (100, linkedToken);
          if (linkedToken.IsCancellationRequested
            && !m_activityAnalysis.ExitRequested && !m_checkThreads.ExitRequested) {
            // OnStop was called (not force exit), return
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
        if (!m_activityAnalysis.Abort ()) {
          log.Warn ("InitializeThreadsAsync: abort of activityAnalysis failed");
        }
        log.Fatal ("InitializeThreadsAsync: activityAnalysis aborted because Exit was requested");
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

      m_checkThreads?.Abort ();
      m_activityAnalysis?.Abort ();
    }
    #endregion // Methods

    #region IDisposable implementation
    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    ~AnalysisService () => Dispose (false);

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
        m_activityAnalysis?.Dispose ();
        m_checkThreads?.Dispose ();
        m_cancellationTokenSource?.Dispose ();
      }

      m_disposed = true;
    }
    #endregion // IDisposable implementation
  }
}
