// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;

using CommandLine;
using Lemoine.Core.Log;
using Lemoine.Info.ConfigReader.TargetSpecific;
using System.Threading.Tasks;
using Lemoine.Core.Plugin;
using Lemoine.Extensions.Interfaces;
using Lemoine.Core.Hosting;
using Lemoine.Core.Extensions.Hosting;
using Lemoine.Extensions;
using Lemoine.Core.Hosting.LctrChecker;
using Microsoft.Extensions.DependencyInjection;
using Lemoine.Extensions.ExtensionsProvider;
using Lemoine.ModelDAO;
using Pulse.Hosting;
using Pulse.Extensions;
using Lemoine.Model;
using Lemoine.Extensions.Plugin;
using Microsoft.Extensions.Hosting;
using Pulse.Hosting.ApplicationInitializer;
using System.Collections.Generic;

namespace Lem_PackageManager.Console
{
  static class Program
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Program).FullName);

    /// <summary>
    /// Program entry point.
    /// </summary>
    public static async Task Main (string[] args)
    {
      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());

      Options options = null;

      try {
        var result = CommandLine.Parser.Default.ParseArguments<Options> (args);

        result.WithNotParsed (errors => {
          log.Info ("Main: invalid arguments");
          Environment.ExitCode = 1;
          return;
        });

        result.WithParsed (opt => {
          var parameters = opt.Parameters;
          if (null != parameters) {
            Lemoine.Info.ConfigSet.AddCommandLineParameters (parameters);
          }
          if (opt.Check) {
            LogManager.AddLog4netDefaultConfigurationFile (logFileSuffix: "-c");
          }
          else {
            LogManager.AddLog4net ();
          }
          options = opt;
        });
      }
      catch (Exception ex) {
        log.Error ("Main: options parsing  exception", ex);
        System.Console.Error.WriteLine ($"Options parsing exception {ex} raised");
        Environment.ExitCode = 1;
      }


      if (options is null) {
        return;
      }

      var applicationName = System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Name;
      var builder = Pulse.Hosting.HostBuilder.CreatePulseConsoleHostBuilder (args, options, services => CreateServices (services, applicationName));
      var host = builder.Build ();

      var serviceProvider = host.Services;
      var consoleRunner = serviceProvider.GetRequiredService<IConsoleRunner<Options>> ();
      consoleRunner.SetOptions (options);
      await consoleRunner.RunConsoleAsync ();
    }

    static IServiceCollection CreateServices (IServiceCollection services, string applicationName)
    {
      return services
        .ConfigureDatabaseWithNoExtension (applicationName)
        .AddSingleton<IAssemblyLoader, Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader> ()
        .AddSingleton<IPluginFilter> ((IServiceProvider sp) => new PluginFilterFromFlag (PluginFlag.Config))
        .AddSingleton<IMainPluginsLoader> ((IServiceProvider sp) => new PluginsLoader (sp.GetRequiredService<IAssemblyLoader> ()))
        .AddSingleton<INHibernatePluginsLoader, DummyPluginsLoader> ()
        .AddSingleton<IExtensionsProvider> ((IServiceProvider sp) => new ExtensionsProvider (sp.GetRequiredService<IDatabaseConnectionStatus> (), sp.GetRequiredService<IPluginFilter> (), GetInterfaceProviders (), sp.GetRequiredService<IMainPluginsLoader> (), sp.GetRequiredService<INHibernatePluginsLoader> ()))
        .AddSingleton<ILctrChecker, ForceLctr> ()
        .AddSingleton<IExtensionsLoader, ExtensionsLoaderLctr> ()
        .AddSingleton<IConsoleRunner<Options>, ConsoleRunner> ()
        .SetApplicationInitializer<ApplicationInitializerWithDatabaseNoExtension> ();
    }

    static IEnumerable<IExtensionInterfaceProvider> GetInterfaceProviders ()
    {
      return new List<IExtensionInterfaceProvider> {
        new Lemoine.Extensions.Alert.ExtensionInterfaceProvider (),
        new Lemoine.Extensions.Analysis.ExtensionInterfaceProvider (),
        new Lemoine.Extensions.AutoReason.ExtensionInterfaceProvider (),
        new Lemoine.Extensions.Business.ExtensionInterfaceProvider (),
        new Lemoine.Extensions.Cnc.ExtensionInterfaceProvider (),
        new Lemoine.Extensions.Database.ExtensionInterfaceProvider (),
        new Lemoine.Extensions.Web.ExtensionInterfaceProvider (),
        new Pulse.Extensions.Business.ExtensionInterfaceProvider (),
        new Pulse.Extensions.Database.ExtensionInterfaceProvider (),
        new Pulse.Extensions.Web.ExtensionInterfaceProvider (),
      };
    }
  }
}
