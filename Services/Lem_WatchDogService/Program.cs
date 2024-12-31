// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Extensions.Hosting;
using Lemoine.Core.Log;
using Lemoine.Core.Performance;
using Lemoine.I18N;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Lemoine.Service;
using Lemoine.ServiceTools;
using Lemoine.ServiceTools.ServicesProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace Lem_WatchDogService
{
  static class Program
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Program));

    static readonly string LOG4NET_TRACE_ENABLED_KEY = "Log4net.TraceEnabled";
    static readonly bool LOG4NET_TRACE_ENABLED_DEFAULT = false;

    const string SERVICE_SET_KEY = "WatchDog.ServiceSet";
    const string SERVICE_SET_DEFAULT = "Default"; // "TrackingConnector: Both Atsora Tracking and Connector

    /// <summary>
    /// Service name
    /// </summary>
#if NET45 || NET48
    internal static readonly string SERVICE_NAME = "Lemoine WatchDog";
#endif // NET45 || NET48

#if !NETCOREAPP
    static void Main (string[] args)
    {
      MainAsync (args).GetAwaiter ().GetResult ();
    }

#endif // NETCOREAPP

    /// <summary>
    /// Program entry point
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
#if NETCOREAPP
    static async Task Main (string[] args)
#else // !NETCOREAPP
    static async Task MainAsync (string[] args)
#endif // !NETCOREAPP
    {
      try {
        Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());
        ServiceOptions options = ServiceOptions.Parse (args);
        var log4netTraceEnabled = Lemoine.Info.ConfigSet.LoadAndGet (LOG4NET_TRACE_ENABLED_KEY, LOG4NET_TRACE_ENABLED_DEFAULT);
        LogManager.AddLog4net (traceEnabled: log4netTraceEnabled);
        bool interactive = options.Debug;
        if (options.Install) {
#if NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1)
          await Task.Run (ThreadServiceBase.Install);
#else // !(NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1))
          ThreadServiceBase.Install ();
#endif // NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1)
          return;
        }
        else if (options.Remove) {
#if NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1)
          await Task.Run (ThreadServiceBase.Remove);
#else // !(NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1))
          ThreadServiceBase.Remove ();
#endif // NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1)
          return;
        }

        var builder = Lemoine.Service.HostBuilder.CreateLemoineServiceHostBuilder6432<WatchDogService> (args, options, services => services.CreateServices ());
        if (options.Interactive) {
          await builder.RunConsoleAsync ();
        }
        else { // Or builder.RunAsServiceAsync () ?
          await builder.Build ().RunAsync ();
        }
      }
      catch (Exception) {
        Environment.Exit (1);
      }
    }

    static IServiceCollection CreateServices (this IServiceCollection services)
    {
      if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        AddWindowsServiceControllersProvider (services);
      }
      else if (RuntimeInformation.IsOSPlatform (OSPlatform.Linux)) {
        AddLinuxServiceControllersProvider (services);
      }
      else {
        throw new NotImplementedException ("No service controller on this platform yet");
      }

      return services
        .SetApplicationInitializer<Lemoine.Core.Hosting.ApplicationInitializer.BasicApplicationInitializer, SpecificConfigInitializer> ();
    }

    [SupportedOSPlatform ("windows")]
    static void AddWindowsServiceControllersProvider (IServiceCollection services)
    {
      services
        .AddSingleton<IServiceControllersProvider> (sp => GetServiceControllersProviderOnWindows ());
    }

    [SupportedOSPlatform ("windows")]
    static IServiceControllersProvider GetServiceControllersProviderOnWindows () =>
      Lemoine.Info.ConfigSet.LoadAndGet (SERVICE_SET_KEY, SERVICE_SET_DEFAULT) switch {
        SERVICE_SET_DEFAULT => new MultiServicesProvider (
#if CONNECTOR
          new ConnectorServicesProviderOnWindows (),
#else // !CONNECTOR
          new PomamoServicesProviderOnWindows (),
#endif // !CONNECTOR
          new ConfigServicesProviderOnWindows ()),
        _ => new MultiServicesProvider (new PomamoServicesProviderOnWindows (), new ConnectorServicesProviderOnWindows (), new ConfigServicesProviderOnWindows ())
      };

    [SupportedOSPlatform ("linux")]
    static void AddLinuxServiceControllersProvider (IServiceCollection services)
    {
      services
        .AddSingleton<IServiceControllersProvider> (sp => new MultiServicesProvider (new PomamoServicesProviderOnLinux (), new ConfigServicesProviderOnLinux ()));
    }
  }
}
