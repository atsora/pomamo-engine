// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using CommandLine;
using Lemoine.Core.Log;
using Lemoine.Info.ConfigReader.TargetSpecific;
using System.Threading.Tasks;
using Pulse.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Lemoine.Core.Hosting;
using Lemoine.Info.ApplicationNameProvider;
using Lemoine.Info;
using Lemoine.Extensions;
using System.Collections.Generic;
using System.Linq;
using Lemoine.CncEngine;
using Lemoine.Core.TargetSpecific.Hosting;

namespace Lem_CncConsole
{
  class Program
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Program).FullName);

    /// <summary>
    /// Program entry point.
    /// </summary>
    [STAThread]
    public static async Task Main (string[] args)
    {
      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());

      Options options = null;

      try {
        var result = CommandLine.Parser.Default.ParseArguments<Options> (args);

        result.WithNotParsed<Options> (errors => {
          Environment.ExitCode = 1;
          return;
        });

        result.WithParsed<Options> (opt => {
          var parameters = opt.Parameters;
          if (null != parameters) {
            Lemoine.Info.ConfigSet.AddCommandLineParameters (parameters);
          }

          var logFileSuffix = "";
          if (opt.CncAcquisitionId != int.MinValue) {
            logFileSuffix += "-" + opt.CncAcquisitionId;
          }
          LogManager.AddLog4netDefaultConfigurationFile (logFileSuffix);

          if ((opt.CncAcquisitionId == int.MinValue) && string.IsNullOrEmpty (opt.File)) {
            log.Error ("Main: no cncAcquisition Id or file was set");
            System.Console.Error.WriteLine ("No cnc acquisition id or file was set");
            Environment.Exit (1);
          }
          options = opt;
        });
      }
      catch (Exception ex) {
        log.Error ("Main: exception raised in arguments parsing", ex);
        System.Console.Error.WriteLine ($"Exception {ex} raised");
        Environment.Exit (1);
        return;
      }

      if (options is null) {
        log.Info ("Main: options null");
        Environment.Exit (1);
        return;
      }

      var applicationName = "Lem_CncConsole-" + options.CncAcquisitionId;
      var builder = Lemoine.Core.TargetSpecific.Hosting.HostBuilder.CreateConsoleHostBuilder (args, options, services => CreateServices (services, applicationName, options));
      var host = builder.Build ();

      var serviceProvider = host.Services;
      var consoleRunner = serviceProvider.GetRequiredService<IConsoleRunner<Options>> ();
      consoleRunner.SetOptions (options);
      await consoleRunner.RunConsoleAsync ();
    }

    static IServiceCollection CreateServices (IServiceCollection services, string applicationName, Options options)
    {
      var result = services
        .AddSingleton<IApplicationNameProvider> ((IServiceProvider sp) => new ApplicationNameProviderFromString (applicationName))
        .AddSingleton<ICncEngineConfig> ((IServiceProvider sp) => new CncEngineConfig (false, true) {
          ConsoleProgramName = "",
        });
      if (options.Light) {
        result = result
          .ConfigureApplicationLight<ApplicationInitializerCncNoDatabase> ();
      }
      else {
        result = result
          .ConfigureFileRepoClientFactoryDefault ()
          .ConfigureDatabaseWithNoNHibernateExtension<ApplicationInitializerCncAcquisition> (Lemoine.Model.PluginFlag.Cnc, GetInterfaceProviders (), applicationName, killOrphanedConnectionsFirst: true);
      }
      return result
        .AddSingleton<IConsoleRunner<Options>, ConsoleRunner> ();
    }

    static IEnumerable<IExtensionInterfaceProvider> GetInterfaceProviders () => Lemoine.Extensions.Cnc.ExtensionInterfaceProvider.GetInterfaceProviders ().Union (new List<IExtensionInterfaceProvider> { new Pulse.Extensions.Database.ExtensionInterfaceProvider () });
  }
}
