// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Threading;
using CommandLine;
using Lemoine.Core.Log;
using CommandLine.Text;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Microsoft.Extensions.DependencyInjection;
using Pulse.Hosting.ApplicationInitializer;
using Lemoine.Core.Hosting;
using Lemoine.Core.Extensions.Hosting;
using Pulse.Hosting;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Lem_TestDynamicTime.Console
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

        result.WithNotParsed (errors =>
        {
          Environment.ExitCode = 1;
          options = null;
          return;
        });

        result.WithParsed (opt =>
        {
          var parameters = opt.Parameters;
          if (null != parameters) {
            Lemoine.Info.ConfigSet.AddCommandLineParameters (parameters);
          }
          LogManager.AddLog4net ();

          options = opt;
        });

        if (options is null) {
          log.Error ($"Main: options error in {args}");
          Environment.ExitCode = 1;
          return;
        }

        Debug.Assert (null != options);
      }
      catch (Exception ex) {
        log.Error ("Main: options parsing  exception", ex);
        System.Console.Error.WriteLine ($"Options parsing exception {ex} raised");
        Environment.ExitCode = 1;
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
        .ConfigureFileRepoClientFactoryLctr ()
        .ConfigureDatabaseWithExtensionsLctr (Lemoine.Model.PluginFlag.Analysis, Lemoine.Extensions.Analysis.ExtensionInterfaceProvider.GetInterfaceProviders (), applicationName)
        .ConfigureBusinessLruCache ()
        .SetApplicationInitializer<ApplicationInitializerWithExtensions, BusinessApplicationInitializer> ()
        .AddSingleton<IConsoleRunner<Options>, ConsoleRunner> ();
    }

  }
}
