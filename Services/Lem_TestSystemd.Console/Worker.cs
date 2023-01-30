// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;
using Lemoine.ServiceTools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lem_TestSystemd.Console
{
  /// <summary>
  /// Worker
  /// </summary>
  public class Worker
    : IHostedService
  {
    readonly ILog log = LogManager.GetLogger (typeof (Worker).FullName);

    readonly IApplicationInitializer m_applicationInitializer;
    readonly IHostApplicationLifetime m_appLifeTime;
    readonly string m_serviceName;
    readonly bool m_start;

    /// <summary>
    /// Constructor
    /// </summary>
    public Worker (IApplicationInitializer applicationInitializer, IHostApplicationLifetime appLifeTime, string serviceName, bool start = false)
    {
      m_applicationInitializer = applicationInitializer;
      m_appLifeTime = appLifeTime;
      m_serviceName = serviceName;
      m_start = start;

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
        if (RuntimeInformation.IsOSPlatform (OSPlatform.Linux)) {
          await RunOnLinuxAsync (cancellationToken);
        }
      }
      catch (Exception ex) {
        log.Fatal ($"StartAsync: uncaught exception", ex);
        System.Console.WriteLine ($"Exception: {ex.Message}");
      }
      finally {
        m_appLifeTime.StopApplication ();
      }

      var duration = DateTime.UtcNow.Subtract (startDateTime);
      log.Info ($"StartAsync: process time is {duration}");
    }

    [SupportedOSPlatform ("linux")]
    async Task RunOnLinuxAsync (CancellationToken cancellationToken)
    {
      var serviceController = new SystemdServiceController (m_serviceName);
      await RunAsync (serviceController, cancellationToken);
    }

    async Task RunAsync (IServiceController serviceController, CancellationToken cancellationToken = default)
    {
      System.Console.WriteLine ($"IsInstalled={serviceController.IsInstalled}");
      System.Console.WriteLine ($"Running={serviceController.Running}");
      if (m_start) {
        await serviceController.StartServiceAsync (cancellationToken);
        System.Console.WriteLine ("Start requested");
        System.Console.WriteLine ($"Running={serviceController.Running}");
      }
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
