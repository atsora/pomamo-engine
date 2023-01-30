// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pulse.Hosting;

namespace Lem_CncDataConsole
{
  static public class Program
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

        result.WithNotParsed<Options> (errors =>
        {
          Environment.ExitCode = 1;
          return;
        });

        result.WithParsed<Options> (opt =>
        {
          var parameters = opt.Parameters;
          if (null != parameters) {
            Lemoine.Info.ConfigSet.AddCommandLineParameters (parameters);
          }

          var logFileSuffix = "";
          if (opt.MachineModuleId != int.MinValue) {
            logFileSuffix += "-" + opt.MachineModuleId;
          }
          LogManager.AddLog4netDefaultConfigurationFile (logFileSuffix);

          if (opt.MachineModuleId == int.MinValue) {
            log.ErrorFormat ("Main: no machine module Id was set");
            System.Console.Error.WriteLine ("No machine module was set");
            Environment.Exit (1);
          }

          options = opt;
        });
      }
      catch (Exception ex) {
        log.Error ($"Main: Exception raised", ex);
        System.Console.Error.WriteLine ($"Exception {ex} raised");
        Environment.Exit (1);
      }

      if (options is null) {
        return;
      }

      var applicationName = System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Name;
      if (options.MachineModuleId != int.MinValue) {
        applicationName += "-" + options.MachineModuleId;
      }
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
        .ConfigureFileRepoClientFactoryDefault ()
        .ConfigureDatabaseWithNoNHibernateExtensionDefaultInitializer (Lemoine.Model.PluginFlag.CncData, Lemoine.Extensions.Cnc.ExtensionInterfaceProvider.GetInterfaceProviders (), applicationName, killOrphanedConnectionsFirst: true)
        .AddSingleton<IConsoleRunner<Options>, CncDataConsole> ();
    }
  }
}
