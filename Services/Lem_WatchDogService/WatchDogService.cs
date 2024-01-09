// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.ServiceTools;
using Lemoine.Threading;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Lem_WatchDogService
{
  /// <summary>
  /// Main class of service Lem_WatchDogService
  /// </summary>
  public class WatchDogService
    : Lemoine.Threading.IThreadServiceAsync, IDisposable
  {
    static readonly string CHECK_EVERY_KEY = "WatchDogService.CheckEvery";
    static readonly TimeSpan CHECK_EVERY_DEFAULT = TimeSpan.FromSeconds (5);

    readonly IApplicationInitializer m_applicationInitializer;
    readonly IServiceControllersProvider m_serviceControllersProvider;
    readonly CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource ();
    bool m_disposed = false;

    static readonly ILog log = LogManager.GetLogger (typeof (WatchDogService).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public WatchDogService (IApplicationInitializer applicationInitializer, IServiceControllersProvider serviceControllersProvider)
    {
      Debug.Assert (null != applicationInitializer);

      m_applicationInitializer = applicationInitializer;
      m_serviceControllersProvider = serviceControllersProvider;
    }

    /// <summary>
    /// <see cref="IThreadService"/>
    /// </summary>
    public void Initialize ()
    {
      System.Threading.Tasks.Task.Run (() => InitializeAsync (CancellationToken.None));
    }

    /// <summary>
    /// Use the default OnStart method
    /// </summary>
    public async System.Threading.Tasks.Task InitializeAsync (CancellationToken cancellationToken)
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
          log.Error ("InitializeAsync: InitializeApplication failed", ex);
          throw;
        }

        if (linkedToken.IsCancellationRequested) {
          return;
        }

        try {
          var serviceControllers = m_serviceControllersProvider
            .GetServiceControllers ()
            .ToList ();

          if (linkedToken.IsCancellationRequested) {
            return;
          }

          var every = Lemoine.Info.ConfigSet
            .LoadAndGet (CHECK_EVERY_KEY, CHECK_EVERY_DEFAULT);

          while (!linkedToken.IsCancellationRequested) {
            foreach (var serviceController in serviceControllers) {
              if (log.IsDebugEnabled) {
                log.Debug ($"InitializeAsync: check {serviceController.ServiceName}");
              }
              try {
                if (!serviceController.Running) {
                  log.Info ($"InitializeAsync: start {serviceController.ServiceName}");
                  linkedToken.ThrowIfCancellationRequested ();
                  await serviceController.StartServiceAsync (linkedToken);
                }
              }
              catch (OperationCanceledException ex) {
                if (linkedToken.IsCancellationRequested) {
                  LogManager.SetApplicationStopping ();
                  log.Info ("InitializeAsync: cancellation requested (OnStop called), return", ex);
                  return;
                }
                else {
                  log.Fatal ($"InitializeAsync: internal request of cancellation for {serviceController.ServiceName}, this should not happen, but continue", ex);
                }
              }
              catch (Exception ex) {
                log.Error ($"InitializeAsync: unexpected error in StartServiceAsync for service {serviceController.ServiceName}, but continue", ex);
              }

              if (linkedToken.IsCancellationRequested) {
                LogManager.SetApplicationStopping ();
                log.Info ("InitializeAsync: cancellation requested (OnStop called), return");
                return;
              }
            }

            await System.Threading.Tasks.Task.Delay (every, linkedToken); // Every 5s by default
            if (linkedToken.IsCancellationRequested) {
              LogManager.SetApplicationStopping ();
              log.Info ("InitializeAsync: cancellation requested (OnStop called), return");
              return;
            }
          }
        }
        catch (OperationCanceledException) {
          return;
        }
        catch (Exception ex) {
          log.Error ("InitializeAsync: exception", ex);
          throw;
        }

      }
    }

    /// <summary>
    /// Stop this service.
    /// </summary>
    public void OnStop ()
    {
      LogManager.SetApplicationStopping ();

      m_cancellationTokenSource?.Cancel ();
    }

    #region IDisposable implementation
    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    ~WatchDogService () => Dispose (false);

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
        m_cancellationTokenSource?.Dispose ();
      }

      m_disposed = true;
    }
    #endregion // IDisposable implementation
  }
}
