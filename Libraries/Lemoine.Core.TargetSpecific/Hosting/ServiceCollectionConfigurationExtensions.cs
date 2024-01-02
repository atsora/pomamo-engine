// Copyright (c) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

#if !NET40

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

namespace Lemoine.Core.TargetSpecific.Hosting
{
  /// <summary>
  /// Configuration extensions of <see cref="IServiceCollection"/>
  /// </summary>
  public static class ServiceCollectionConfigurationExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ServiceCollectionConfigurationExtensions).FullName);

    /// <summary>
    /// Configure lightly an application with:
    /// <item>no database connection</item>
    /// <item>dummy FileRepoClient</item>
    /// <item>dummy extensions loader</item>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureApplicationLight<T> (this IServiceCollection services)
      where T : class, IApplicationInitializer
    {
      return services
        .AddSingleton<IAssemblyLoader, Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader> ()
        .AddSingleton<IExtensionsLoader, ExtensionsLoaderDummy> ()
        .AddSingleton<IFileRepoClientFactory, FileRepoClientFactoryDummy> ()
        .AddSingleton<IApplicationInitializer, T> ();
    }
  }
}

#endif // !NET40