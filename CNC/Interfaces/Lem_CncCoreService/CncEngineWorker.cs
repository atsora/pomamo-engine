// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Business.Config;
using Lemoine.CncEngine;
using Lemoine.Core.Cache;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.Threading;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lem_CncCoreService
{
  /// <summary>
  /// Background service worker to flush regularly the cache
  /// </summary>
  public class CncEngineWorker : BackgroundServiceWorker
  {
    readonly ILog log = LogManager.GetLogger<CncEngineWorker> ();

    readonly IAcquisitionSet m_acquisitionListBuilder;
    readonly CheckThreadsAndProcesses m_checkThreadsAndProcesses = new CheckThreadsAndProcesses ();

    readonly IList<Acquisition> m_acquisitions = new List<Acquisition> ();

    public CncEngineWorker (IAcquisitionSet acquisitionListBuilder, IEnumerable<IAdditionalChecker> additionalCheckers)
    {
      m_acquisitionListBuilder = acquisitionListBuilder;

      m_checkThreadsAndProcesses.AddAdditionalCheckers (additionalCheckers.ToArray ());
    }

    protected override async Task RunAsync (CancellationToken stoppingToken)
    {
      try {
        await Task.Run (() => m_checkThreadsAndProcesses.InitializeAdditionalCheckers (stoppingToken));
        if (stoppingToken.IsCancellationRequested) {
          log.Warn ("RunAsync: cancellation of stoppingToken requested");
          return;
        }

        var acquisitions = m_acquisitionListBuilder.GetAcquisitions (stoppingToken);
        // Note: ToList () to initialize first all the threads before starting them,
        //       else there are some CORBA problems
        //       while trying to download the configuration files

        foreach (var acquisition in acquisitions) {
          stoppingToken.ThrowIfCancellationRequested ();
          acquisition.StartThreadOrProcess (stoppingToken);
          // TODO: or RunAsync ?
        }

        foreach (var acquisition in acquisitions) {
          stoppingToken.ThrowIfCancellationRequested ();
          acquisition.AddToThreadsAndProcessesChecker (m_checkThreadsAndProcesses);
        }
        m_checkThreadsAndProcesses.Start (stoppingToken);
        if (log.IsDebugEnabled) {
          log.Debug ("RunAsync: check threads and processes started");
        }
      }
      catch (Exception ex) {
        try {
          if (stoppingToken.IsCancellationRequested) {
            LogManager.SetApplicationStopping ();
            log.Warn ($"RunAsync: cancellation requested for stopping token, return");
          }
          else {
            log.Fatal ("RunAsync: unexpected exception", ex);
          }
        }
        catch (Exception) { }
        throw;
      }
    }

    public IEnumerable<Acquisition> GetThreadAcquisitions ()
    {
      return m_acquisitions.Where (a => !a.UseProcess);
    }

    public override ILog GetLogger ()
    {
      return log;
    }

  }
}
