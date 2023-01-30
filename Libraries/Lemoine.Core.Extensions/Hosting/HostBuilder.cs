// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Lemoine.Core.Extensions.Logging;
using Lemoine.Core.Log;
using Lemoine.Core.Options;
using Lemoine.Service;
using Lemoine.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lemoine.Core.Extensions.Hosting
{
  /// <summary>
  /// HostBuilder
  /// </summary>
  public static class HostBuilder
  {
    static readonly ILog log = LogManager.GetLogger (typeof (HostBuilder).FullName);

    /// <summary>
    /// Create the host builder
    /// 
    /// Register it as a Windows or Systemd service only with .NET Core &ge; 3.0
    /// </summary>
    /// <param name="args"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder CreateLemoineServiceHostBuilder<T> (string[] args, ServiceOptions options, Action<IServiceCollection> configureServices)
      where T : class, IThreadServiceAsync
    {
      var defaultBuilder = Host
        .CreateDefaultBuilder (args);

      if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        defaultBuilder = defaultBuilder
          .UseWindowsService ();
      }
#if NETCOREAPP
      else if (RuntimeInformation.IsOSPlatform (OSPlatform.Linux)) {
        defaultBuilder = defaultBuilder
          .UseSystemd ();
      }
#endif // NETCOREAPP

      return defaultBuilder
        .ConfigureLemoineServiceAppConfiguration (options)
        .ConfigureServices ((hostContext, services) => {
          configureServices (services);
#if NET6_0_OR_GREATER
          services.AddLogging (loggingBuilder => {
            loggingBuilder.AddLemoineLog ();
          });
#endif // NET6_0_OR_GREATER
          services.AddSingleton<IThreadServiceAsync, T> ();
          services.AddSingleton<IThreadService> ((IServiceProvider sp) => sp.GetRequiredService<IThreadServiceAsync> ());
          services.AddHostedService<AsyncThreadServiceWorker> ();
        });
    }

    /// <summary>
    /// Create the host builder for a Lemoine console application
    /// </summary>
    /// <param name="args"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder CreateLemoineConsoleHostBuilder (string[] args, IMicrosoftParameters options, Action<IServiceCollection> configureServices)
    {
      var defaultBuilder = Host
        .CreateDefaultBuilder (args);

      return defaultBuilder
        .ConfigureLemoineConsoleAppConfiguration (options)
        .ConfigureServices ((hostContext, services) => {
#if NET6_0_OR_GREATER
          services.AddLogging (loggingBuilder => {
            loggingBuilder.AddLemoineLog ();
          });
#endif // NET6_0_OR_GREATER
          configureServices (services);
        });
    }

    /// <summary>
    /// Create the host builder for a Lemoine Gui application
    /// </summary>
    /// <param name="args"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder CreateLemoineGuiHostBuilder (string[] args, IMicrosoftParameters options, Action<IServiceCollection> configureServices)
    {
      var defaultBuilder = Host
        .CreateDefaultBuilder (args);

      return defaultBuilder
        .ConfigureLemoineGuiAppConfiguration (options)
        .ConfigureServices ((hostContext, services) => {
#if NET6_0_OR_GREATER
          services.AddLogging (loggingBuilder => {
            loggingBuilder.AddLemoineLog ();
          });
#endif // NET6_0_OR_GREATER
          configureServices (services);
        });
    }

    /// <summary>
    /// Create the host builder for a Lemoine Gui application
    /// </summary>
    /// <param name="args"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder CreateLemoineGuiHostBuilder (string[] args, Action<IServiceCollection> configureServices)
    {
      var defaultBuilder = Host
        .CreateDefaultBuilder (args);

      return defaultBuilder
        .ConfigureServices ((hostContext, services) => {
#if NET6_0_OR_GREATER
          services.AddLogging (loggingBuilder => {
            loggingBuilder.AddLemoineLog ();
          });
#endif // NET6_0_OR_GREATER
          configureServices (services);
        });
    }
  }
}
