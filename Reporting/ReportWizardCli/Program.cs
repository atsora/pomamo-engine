// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using CommandLine;
using Lemoine.Core.Extensions.Hosting;
using Lemoine.Core.Log;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pulse.Hosting;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ReportWizardCli
{
  internal class Program
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Program).FullName);

    static async Task Main (string[] args)
    {
      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());

      Options? options = null;
      try {
        var result = CommandLine.Parser.Default.ParseArguments<Options> (args);

        result.WithNotParsed<Options> (errors => {
          Environment.ExitCode = 1;
          options = null;
        });

        result.WithParsed<Options> (opt => {
          options = opt;
        });
      }
      catch (Exception ex) {
        System.Console.Error.WriteLine ($"Exception {ex} raised");
        Environment.Exit (1);
        return;
      }

      if (options is null) {
        System.Console.Error.WriteLine ($"Invalid arguments");
        Environment.Exit (1);
        return;
      }

      var parameters = options.Parameters;
      if (null != parameters) {
        Lemoine.Info.ConfigSet.AddCommandLineParameters (parameters);
      }
      LogManager.AddLog4net (traceEnabled: options.TraceEnabled);

      var hostBuilder = CreateHostBuilder (args, options);

      try {
        await hostBuilder.Build ().RunAsync ();
      }
      catch (Exception ex) {
        log.Error ($"Main: exception {ex} raised", ex);
        Environment.Exit (1);
        return;
      }
    }

    static IHostBuilder CreateHostBuilder (string[] args, Options options)
    {
      var applicationName = System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Name;
      return Host.CreateDefaultBuilder (args)
        .ConfigureConsoleAppConfiguration (options)
        .ConfigureServices ((_, services) =>
        {
          services
          .AddHostedService<Worker> ()
          .AddSingleton<Options> (options)
          .AddSingleton (new IniConfigInitializer (options.FilePath))
          .AddSingleton<HttpClient> ()
          .ConfigureDataAccessFromConfigSetDefaultInitializer ();
        });
    }
  }
}
