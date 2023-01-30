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
using Lemoine.Core.Log;
using Lemoine.Stamping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lem_TestHeidenhainParser.Console
{
  /// <summary>
  /// Worker
  /// </summary>
  public class Worker
    : IHostedService
  {
    readonly ILog log = LogManager.GetLogger (typeof (Worker).FullName);

    readonly IHostApplicationLifetime m_appLifeTime;
    readonly IServiceProvider m_serviceProvider;

    /// <summary>
    /// Constructor
    /// </summary>
    public Worker (IHostApplicationLifetime appLifeTime, IServiceProvider serviceProvider)
    {
      m_appLifeTime = appLifeTime;
      m_serviceProvider = serviceProvider;

      appLifeTime.ApplicationStarted.Register (OnStarted);
      appLifeTime.ApplicationStopping.Register (OnStopping);
      appLifeTime.ApplicationStopped.Register (OnStopped);
    }

    void Configure (IStampingApplicationBuilder stampingApplicationBuilder)
    {
      stampingApplicationBuilder
        .UseEventHandler<Lemoine.Stamping.StampingEventHandlers.LogEvents> ()
        .UseEventHandler<Lemoine.Stamping.StampingEventHandlers.SequenceAtToolChange> ()
        .UseEventHandler<Lemoine.Stamping.StampingEventHandlers.DelayMachiningSequence> ()
        .UseEventHandler<Lemoine.Stamping.StampingEventHandlers.SequenceTimeRecorder> ()
        .UseEventHandler<Lemoine.Stamping.StampingEventHandlers.SequenceTagWriter> ();
    }

    public async Task StartAsync (CancellationToken cancellationToken)
    {
      log.Debug ("StartAsync");

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
