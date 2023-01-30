// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Cache;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;

namespace Pulse.Hosting.ApplicationInitializer
{
  /// <summary>
  /// Application initializer <see cref="IApplicationInitializer"/> for the business layer
  /// </summary>
  public class BusinessApplicationInitializer: IApplicationInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (BusinessApplicationInitializer).FullName);

    readonly ICacheClient m_cacheClient;
    readonly Lemoine.Business.IService m_service;

    /// <summary>
    /// Constructor
    /// </summary>
    public BusinessApplicationInitializer (ICacheClient cacheClient, Lemoine.Business.IService service)
    {
      m_cacheClient = cacheClient;
      m_service = service;
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    public void InitializeApplication (CancellationToken cancellationToken = default)
    {
      Lemoine.Business.ServiceProvider.Service = m_service;
      Lemoine.Core.Cache.CacheManager.CacheClient = m_cacheClient;
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      await Task.Delay (0);
      Lemoine.Business.ServiceProvider.Service = m_service;
      Lemoine.Core.Cache.CacheManager.CacheClient = m_cacheClient;
    }
  }
}
