// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NETCOREAPP && !NETCOREAPP2_0
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Threading;
using Microsoft.Extensions.Hosting;

namespace Lemoine.Service
{
  /// <summary>
  /// BackgroundService worker for a IThreadService
  /// 
  /// <see cref="IThreadService"/>
  /// </summary>
  public class ThreadServiceWorker : BackgroundService
  {
    readonly ILog log = LogManager.GetLogger (typeof (ThreadServiceWorker).FullName);

    readonly IThreadService m_service;

#region Getters / Setters
#endregion // Getters / Setters

#region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="service"></param>
    public ThreadServiceWorker (IThreadService service)
    {
      m_service = service;
    }
#endregion // Constructors

    /// <summary>
    /// BackgroundService implementation
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync (CancellationToken stoppingToken)
    {
      await m_service.ExecuteAsync (stoppingToken);
    }
  }
}
#endif // NETCOREAPP && !NETCOREAPP2_0
