// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using CommandLine;
using Lemoine.Core.Log;
using Lemoine.Info.ConfigReader.TargetSpecific;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Pulse.Hosting;
using Lemoine.Cnc.Engine;
using Microsoft.Extensions.DependencyInjection;
using Lemoine.Core.Hosting;
using Lemoine.ModelDAO.Interfaces;
using Lemoine.Info.ApplicationNameProvider;
using Lemoine.Info;

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
        log.Error ("Parse: exception raised", ex);
        System.Console.Error.WriteLine ($"Exception {ex} raised");
        Environment.Exit (1);
        return;
      }

      var applicationName = "Lem_CncConsole-" + options.CncAcquisitionId;
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
        .AddSingleton<IApplicationNameProvider> ((IServiceProvider sp) => new ApplicationNameProviderFromString (applicationName))
        .ConfigureDatabaseWithNoNHibernateExtension<ApplicationInitializerCncAcquisition> (Lemoine.Model.PluginFlag.Cnc, Lemoine.Extensions.Cnc.ExtensionInterfaceProvider.GetInterfaceProviders (), applicationName, killOrphanedConnectionsFirst: true)
        .AddSingleton<IConsoleRunner<Options>, ConsoleRunner> ();
    }
  }
}
