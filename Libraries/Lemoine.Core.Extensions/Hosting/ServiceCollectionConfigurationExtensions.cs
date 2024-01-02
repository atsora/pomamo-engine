// Copyright (c) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.FileRepository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Lemoine.Core.Plugin;
using Lemoine.Extensions.Interfaces;
using Lemoine.Core.Hosting;
using Lemoine.Extensions.DummyImplementations;

namespace Lemoine.Core.Extensions.Hosting
{
  /// <summary>
  /// Configuration extensions of <see cref="IServiceCollection"/>
  /// </summary>
  public static class ServiceCollectionConfigurationExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ServiceCollectionConfigurationExtensions).FullName);

    /// <summary>
    /// Configure a <see cref="IFileRepoClientFactory"/> on lctr
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureFileRepoClientFactoryLctr (this IServiceCollection services)
    {
      return services
        .AddSingleton<IFileRepoClientFactory> ((IServiceProvider sp) => new FileRepoClientFactoryNoCorba (DefaultFileRepoClientMethod.PfrDataDir)
      );
    }
  }
}
