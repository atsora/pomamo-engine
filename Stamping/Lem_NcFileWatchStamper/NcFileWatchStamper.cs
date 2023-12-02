// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using Lemoine.Core.Log;
using Lemoine.Threading;
using Pomamo.Stamping.FileDetection;

namespace Lemoine.Stamping.Lem_NcFileWatchStamper
{
  /// <summary>
  /// Main class of service Lem_NcFileWatchStamper
  /// </summary>
  public sealed class NcFileWatchStamper
    : Lemoine.Threading.IThreadServiceAsync, IDisposable
  {
    static readonly string NOT_RESPONDING_TIMEOUT_KEY = "NcFileWatchStamper.CheckThreads.Timeout";
    static readonly TimeSpan NOT_RESPONDING_TIMEOUT_DEFAULT = TimeSpan.FromMinutes (5);

    readonly CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource ();
    bool m_disposed = false;
    NcFileDetection m_ncFileDetection;
    readonly CheckThreadsAndProcesses m_check = new CheckThreadsAndProcesses ();

    static readonly ILog log = LogManager.GetLogger (typeof (NcFileWatchStamper).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public NcFileWatchStamper ()
    {
    }

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

        // thread to monitor the index files directory
        m_ncFileDetection = new NcFileDetection ();
        m_ncFileDetection.Start (linkedToken);

        // - Create the thread that checks the other threads
        m_check.AddThread (m_ncFileDetection);
        m_check.NotRespondingTimeout = Lemoine.Info.ConfigSet
          .LoadAndGet<TimeSpan> (NOT_RESPONDING_TIMEOUT_KEY, NOT_RESPONDING_TIMEOUT_DEFAULT);
        m_check.Start (linkedToken);

        // - Check no 'exit' was requested
        while (!m_ncFileDetection.ExitRequested && !m_check.ExitRequested) {
          await System.Threading.Tasks.Task.Delay (100, linkedToken);
          if (linkedToken.IsCancellationRequested
            && !m_ncFileDetection.ExitRequested && !m_check.ExitRequested) {
            // OnStop was called, return
            LogManager.SetApplicationStopping ();
            log.Info ($"InitializeThreadsAsync: cancellation requested (OnStop called), return");
            return;
          }
        }
      }

      LogManager.SetApplicationStopping ();
      log.Fatal ("InitializeThreadsAsync: exit was requested");
      try {
        m_check.Abort ();
        log.Fatal ("InitializeThreadsAsync: checkThreads aborted because Exit was requested");
        m_ncFileDetection.Abort ();
        log.Fatal ("InitializeThreadsAsync: m_directoryManager aborted because Exit was requested");
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

      m_check?.Abort ();
      m_ncFileDetection?.Abort ();
    }

    #region IDisposable implementation
    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    ~NcFileWatchStamper () => Dispose (false);

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
        m_check?.Dispose ();
        m_cancellationTokenSource?.Dispose ();
      }

      m_disposed = true;
    }
    #endregion // IDisposable implementation
  }
}
