// Copyright (C) 2023 Atsora Solutions

using System;
using System.Threading;
using Lemoine.Core.Log;
using Lemoine.Threading;
using Pomamo.Stamping.FileDetection;

namespace Lemoine.Stamping.Lem_StampingService
{
  /// <summary>
  /// Main class of service Lem_StampFileWatchService
  /// </summary>
  public sealed class StampingService
    : Lemoine.Threading.IThreadServiceAsync, IDisposable
  {
    static readonly string NOT_RESPONDING_TIMEOUT_KEY = "StampingService.CheckThreads.Timeout";
    static readonly TimeSpan NOT_RESPONDING_TIMEOUT_DEFAULT = TimeSpan.FromMinutes (5);

    readonly CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource ();
    bool m_disposed = false;
    IndexFileDetection m_indexFileDetection;
    readonly CheckThreadsAndProcesses m_check = new CheckThreadsAndProcesses ();

    static readonly ILog log = LogManager.GetLogger (typeof (StampingService).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public StampingService ()
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
        m_indexFileDetection = new IndexFileDetection ();
        m_indexFileDetection.Start (linkedToken);

        // - Create the thread that checks the other threads
        m_check.AddThread (m_indexFileDetection);
        m_check.NotRespondingTimeout = Lemoine.Info.ConfigSet
          .LoadAndGet<TimeSpan> (NOT_RESPONDING_TIMEOUT_KEY, NOT_RESPONDING_TIMEOUT_DEFAULT);
        m_check.Start (linkedToken);

        // - Check no 'exit' was requested
        while (!m_indexFileDetection.ExitRequested && !m_check.ExitRequested) {
          await System.Threading.Tasks.Task.Delay (100, linkedToken);
          if (linkedToken.IsCancellationRequested
            && !m_indexFileDetection.ExitRequested && !m_check.ExitRequested) {
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
        m_indexFileDetection.Abort ();
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
      m_indexFileDetection?.Abort ();
    }

    #region IDisposable implementation
    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    ~StampingService () => Dispose (false);

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
