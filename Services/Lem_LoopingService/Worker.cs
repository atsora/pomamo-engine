// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Lemoine.Service;
using Lemoine.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lem_LoopingService
{
  /// <summary>
  /// BackgroundService worker for a IThreadService
  /// 
  /// <see cref="IThreadService"/>
  /// 
  /// Fully available in .NET Core &Ge; 3.0
  /// Partially available in .NET Core &Ge; 2.1
  /// </summary>
  public class Worker : BackgroundService
  {
    readonly ILog log = LogManager.GetLogger (typeof (Worker).FullName);

    readonly IThreadService m_service;

#region Getters / Setters
#endregion // Getters / Setters

#region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="service"></param>
    public Worker (IConfiguration configuration, IThreadService service)
    {
      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader (configuration));
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

    /// <summary>
    /// Create the host builder
    /// 
    /// Register it as a Windows or Systemd service only with .NET Core &ge; 3.0
    /// </summary>
    /// <param name="args"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder CreateHostBuilder (string[] args, ServiceOptions options)
    {
      var defaultBuilder = Host
        .CreateDefaultBuilder (args);

      if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        defaultBuilder = defaultBuilder
          .UseWindowsService ();
      }
      else if (RuntimeInformation.IsOSPlatform (OSPlatform.Linux)) {
        defaultBuilder = defaultBuilder
          .UseSystemd ();
      }

      return defaultBuilder
        .ConfigureAppConfiguration ((hostingContext, config) =>
        {
          config.AddCommandLine (options.MicrosoftParameters.ToArray ());
          config.AddEnvironmentVariables ("POMAMO_");
        })
        .ConfigureServices ((hostContext, services) =>
        {
          services.AddSingleton<IThreadService, LoopingService> ();
          services.AddHostedService<Worker> ();
        });
    }
  }
}

#endif // NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2)
