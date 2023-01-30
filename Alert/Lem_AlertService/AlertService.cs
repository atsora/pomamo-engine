// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;

using Lemoine.Alert;
using Lemoine.I18N;
using Lemoine.Core.Log;
using Lemoine.Threading;
using Lemoine.Model;
using Lemoine.Extensions.Alert;
using System.Linq;
using Pulse.Hosting;
using Lemoine.Business.Config;
using Lemoine.Core.Hosting;
using System.Diagnostics;

namespace Lem_AlertService
{
  /// <summary>
  /// Main class of service Lem_AlertService
  /// </summary>
  public sealed class AlertService
    : Lemoine.Threading.IThreadServiceAsync, IDisposable
  {
    static readonly string NOT_RESPONDING_TIMEOUT_KEY = "Alert.CheckThreads.Timeout";
    static readonly TimeSpan NOT_RESPONDING_TIMEOUT_DEFAULT = TimeSpan.FromMinutes (5);

    #region Members
    readonly IApplicationInitializer m_applicationInitializer;
    readonly CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource ();
    bool m_disposed = false;
    AlertEngine m_alertEngine;
    readonly CheckThreadsAndProcesses m_check = new CheckThreadsAndProcesses ();
    #endregion

    static readonly ILog log = LogManager.GetLogger (typeof (AlertService).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public AlertService (IApplicationInitializer applicationInitializer)
    {
      Debug.Assert (null != applicationInitializer);

      m_applicationInitializer = applicationInitializer;

      var configUpdateChecker = new ConfigUpdateChecker ();
      m_check.AddAdditionalCheckers (configUpdateChecker);
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

        var alertConfigExtensionRequest = new Lemoine.Business.Extension.GlobalExtensions<IAlertConfigExtension> (ext => ext.Initialize ());
        var alertConfigExtensions = Lemoine.Business.ServiceProvider
          .Get (alertConfigExtensionRequest);
        var listeners = alertConfigExtensions
          .Where (x => null != x.Listeners)
          .SelectMany (x => x.Listeners);
        var triggeredActions = alertConfigExtensions
          .Where (x => null != x.TriggeredActions)
          .SelectMany (x => x.TriggeredActions);

        if (linkedToken.IsCancellationRequested) {
          // In case Stop is run before the service is fully initialized
          return;
        }

        if (log.IsDebugEnabled) {
          log.Debug ($"InitializeThreadsAsync: {listeners.Count ()} listeners and {triggeredActions.Count ()} triggered actions");
        }
        m_alertEngine = new AlertEngine (listeners, triggeredActions);
        m_alertEngine.Start (linkedToken);

        // - Create the thread that checks the other threads
        m_check.AddThread (m_alertEngine);
        m_check.NotRespondingTimeout = Lemoine.Info.ConfigSet
          .LoadAndGet<TimeSpan> (NOT_RESPONDING_TIMEOUT_KEY, NOT_RESPONDING_TIMEOUT_DEFAULT);
        m_check.Start (linkedToken);

        // - Check no 'exit' was requested
        while (!m_alertEngine.ExitRequested && !m_check.ExitRequested) {
          await System.Threading.Tasks.Task.Delay (100, linkedToken);
          if (linkedToken.IsCancellationRequested
            && !m_alertEngine.ExitRequested && !m_check.ExitRequested) {
            // OnStop was called, return
            LogManager.SetApplicationStopping ();
            log.Info ($"InitializeThreadsAsync: cancellation requested (OnStop called), return");
            return;
          }
        }
      }

      LogManager.SetApplicationStopping ();
      log.Fatal ("InitializeThreadsAsync: exit was requested by one of the two main threads");
      try {
        m_check.Abort ();
        log.Fatal ("InitializeThreadsAsync: checkThreads aborted because Exit was requested");
        m_alertEngine.Abort ();
        log.Fatal ("InitializeThreadsAsync: alertEngine aborted because Exit was requested");
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
      m_alertEngine?.Abort ();
    }
    #endregion // Methods

    #region IDisposable implementation
    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    ~AlertService () => Dispose (false);

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
