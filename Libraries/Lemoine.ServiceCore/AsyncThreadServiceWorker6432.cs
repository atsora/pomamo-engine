// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

#if !NET40

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Lemoine.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Lemoine.Service
{
  /// <summary>
  /// BackgroundService worker for a IThreadService for a mixed 64/32-bit system
  /// 
  /// <see cref="IThreadService"/>
  /// </summary>
  public class AsyncThreadServiceWorker6432 : BackgroundService
  {
    readonly ILog log = LogManager.GetLogger (typeof (AsyncThreadServiceWorker).FullName);

    readonly IThreadServiceAsync m_service;

    /// <summary>
    /// Constructor
    /// </summary>
    public AsyncThreadServiceWorker6432 (IConfiguration configuration, IThreadServiceAsync service)
    {
      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader (configuration, mixed6432: true));
      m_service = service;
    }

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
#endif // !NET40
