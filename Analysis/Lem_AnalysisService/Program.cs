// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Hosting;
using Lemoine.Core.Hosting.LctrChecker;
using Lemoine.Core.Log;
using Lemoine.Core.Performance;
using Lemoine.I18N;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Lemoine.ModelDAO;
using Lemoine.ModelDAO.LctrChecker;
using Lemoine.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pulse.Business.Computer;
using Pulse.Hosting;
using Pulse.Hosting.ApplicationInitializer;
using System;
using System.Threading.Tasks;

namespace Lem_AnalysisService
{
  static class Program
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Program));

    static readonly string LOG4NET_TRACE_ENABLED_KEY = "Log4net.TraceEnabled";
    static readonly bool LOG4NET_TRACE_ENABLED_DEFAULT = false;

    const string CACHE_IMPLEMENTATION_DEFAULT = "default"; // "lru" or "memory" or "default"
    const int CACHE_LRU_SIZE_DEFAULT = 10000;

    /// <summary>
    /// Service name
    /// </summary>
#if NET45 || NET48
    internal static readonly string SERVICE_NAME = "Lemoine Analysis";
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

        var applicationName = System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Name;
        var builder = Pulse.Hosting.HostBuilder.CreatePulseServiceHostBuilder<AnalysisService> (args, options, services => services.CreateServices (applicationName));
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

    static IServiceCollection CreateServices (this IServiceCollection services, string applicationName)
    {
      return services
        .AddSingleton ((IServiceProvider sp) => new ValidLctrChecker (new CheckDatabaseLctrChecker (new DatabaseLctrChecker (), sp.GetService<IDatabaseConnectionStatus> ())))
        .AddSingleton<IValidServerChecker> ((IServiceProvider sp) => sp.GetRequiredService<ValidLctrChecker> ())
        .ConfigureFileRepoClientFactoryLctr ()
        .ConfigureDatabaseWithExtensionsLctr (Lemoine.Model.PluginFlag.Analysis, Lemoine.Extensions.Analysis.ExtensionInterfaceProvider.GetInterfaceProviders (), applicationName, killOrphanedConnectionsFirst: true)
        .ConfigureBusinessCache (implementationDefault: CACHE_IMPLEMENTATION_DEFAULT, lruCacheSizeDefault: CACHE_LRU_SIZE_DEFAULT)
        .AddSingleton<IPerfRecorder, Lemoine.Business.Performance.PerfRecorderFromExtensions> ()
        .AddSingleton<ApplicationInitializerWithExtensions> ()
        .AddSingleton<BusinessApplicationInitializer> ()
        .AddSingleton<PerfRecorderInitializer> ()
        .SetApplicationInitializerCollection<ApplicationInitializerWithExtensions, BusinessApplicationInitializer, ValidLctrChecker, PerfRecorderInitializer> ();
    }
  }
}
