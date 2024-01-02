// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;

namespace Lemoine.Threading
{
  /// <summary>
  /// Extensions to interface <see cref="IThreadService"/>
  /// </summary>
  public static class ThreadServiceExtensions
  {
    static ILog log = LogManager.GetLogger (typeof (ThreadServiceExtensions).FullName);

#if NETSTANDARD
    /// <summary>
    /// BackgroundService implementation
    /// </summary>
    /// <param name="threadService"></param>
    /// <param name="stoppingToken"></param>
    /// <param name="exitIfException"></param>
    /// <returns></returns>
    public static async Task ExecuteAsync (this IThreadService threadService, CancellationToken stoppingToken, bool exitIfException = true)
    {
      try {
        await Task.Run (threadService.Initialize, stoppingToken);

        stoppingToken.WaitHandle.WaitOne ();

        await Task.Run (threadService.OnStop);
      }
      catch (Exception ex) {
        if (exitIfException) {
          LogManager.SetApplicationStopping ();
        }
        log.Error ($"ExecuteAsync: exception, exit={exitIfException}", ex);
        if (exitIfException) {
          Lemoine.Core.Environment.ForceExit ();
        }
        throw;
      }
    }

    /// <summary>
    /// BackgroundService implementation
    /// </summary>
    /// <param name="threadService">not null</param>
    /// <param name="stoppingToken"></param>
    /// <param name="exitIfException"></param>
    /// <returns></returns>
    public static async Task ExecuteAsync (this IThreadServiceAsync threadService, CancellationToken stoppingToken, bool exitIfException = true)
    {
      try {
        await threadService.InitializeAsync (stoppingToken);

        stoppingToken.WaitHandle.WaitOne ();

        await Task.Run (threadService.OnStop);
      }
      catch (Exception ex) {
        log.Error ($"ExecuteAsync: exception, exit={exitIfException}", ex);
        if (exitIfException) {
          Lemoine.Core.Environment.ForceExit ();
        }
        throw;
      }
    }
#endif // NETSTANDARD
  }
}
