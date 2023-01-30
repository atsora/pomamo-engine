// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;
using Lemoine.Stamping;
using Lemoine.Stamping.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lem_Stamper.Console
{
  /// <summary>
  /// Worker
  /// </summary>
  public class Worker
    : IHostedService
  {
    readonly ILog log = LogManager.GetLogger (typeof (Worker).FullName);

    readonly IApplicationInitializer m_applicationInitializer;
    readonly IStampingEventHandlersProvider m_stampingConfig;
    readonly TypeLoader m_typeLoader;
    readonly IHostApplicationLifetime m_appLifeTime;
    readonly IServiceProvider m_serviceProvider;
    readonly IStampingApplicationBuilder m_stampingApplicationBuilder;

    /// <summary>
    /// Constructor
    /// </summary>
    public Worker (IApplicationInitializer applicationInitializer, IHostApplicationLifetime appLifeTime, IServiceProvider serviceProvider, IStampingEventHandlersProvider stampingConfig, TypeLoader typeLoader, IStampingApplicationBuilder stampingApplicationBuilder)
    {
      m_applicationInitializer = applicationInitializer;
      m_stampingConfig = stampingConfig;
      m_typeLoader = typeLoader;
      m_appLifeTime = appLifeTime;
      m_serviceProvider = serviceProvider;
      m_stampingApplicationBuilder = stampingApplicationBuilder;

      appLifeTime.ApplicationStarted.Register (OnStarted);
      appLifeTime.ApplicationStopping.Register (OnStopping);
      appLifeTime.ApplicationStopped.Register (OnStopped);
    }

    public async Task StartAsync (CancellationToken cancellationToken)
    {
      log.Debug ("StartAsync");

      var startDateTime = DateTime.UtcNow;

      await m_applicationInitializer.InitializeApplicationAsync (cancellationToken);

      cancellationToken.ThrowIfCancellationRequested ();

      try {
        using (var scope = m_serviceProvider.CreateScope ()) {
          var provider = scope.ServiceProvider;
          var fullStampingProcess = provider.GetRequiredService<FullStampingProcess> ();
          await fullStampingProcess.StampAsync (cancellationToken);
        }
      }
      catch (Exception ex) {
        log.Fatal ($"StartAsync: uncaught exception", ex);
      }
      finally {
        m_appLifeTime.StopApplication ();
      }

      var duration = DateTime.UtcNow.Subtract (startDateTime);
      log.Info ($"StartAsync: process time is {duration}");
    }

    public Task StopAsync (CancellationToken cancellationToken)
    {
      log.Debug ("StopAsync");

      return Task.CompletedTask;
    }

    private void OnStarted ()
    {
      log.Debug ("OnStarted");
    }

    private void OnStopping ()
    {
      log.Debug ("OnStopping");
    }

    private void OnStopped ()
    {
      log.Debug ("OnStopped");
    }
  }
}
