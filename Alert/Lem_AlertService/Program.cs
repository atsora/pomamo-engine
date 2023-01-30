// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.FileRepository;
using Lemoine.I18N;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Lemoine.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pulse.Business.Computer;
using Pulse.Hosting;
using Pulse.Hosting.ApplicationInitializer;
using System;
using System.Threading.Tasks;

namespace Lem_AlertService
{
  static class Program
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Program).FullName);

    /// <summary>
    /// Service name
    /// </summary>
#if NET45 || NET48
    internal static readonly string SERVICE_NAME = "Lemoine Alert";
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
        LogManager.AddLog4net ();
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
        var builder = Pulse.Hosting.HostBuilder.CreatePulseServiceHostBuilder<AlertService> (args, options, services => services.CreateServices (applicationName));
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
        .AddSingleton<ILctrChecker> ((IServiceProvider sp) => sp.GetRequiredService<ServerChecker> ())
        .AddSingleton<IValidServerChecker> ((IServiceProvider sp) => sp.GetRequiredService<ServerChecker> ())
        .ConfigureFileRepoClientFactoryCheckLctr ()
        .ConfigureDatabaseWithExtensionsDefaultInitializer (Lemoine.Model.PluginFlag.Alert, Lemoine.Extensions.Alert.ExtensionInterfaceProvider.GetInterfaceProviders (), applicationName, killOrphanedConnectionsFirst: true)
        .ConfigureBusinessLruCache ()
        .ConfigureCatalog (applicationName)
        .SetApplicationInitializer<ApplicationInitializerWithExtensions, ServerChecker, BusinessApplicationInitializer, PulseCatalogInitializer> ();
    }
  }
}
