// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using CommandLine;

using Lemoine.Info.ConfigReader.TargetSpecific;
using Lemoine.Core.Log;
using Lemoine.Core.Extensions.Hosting;
using Lemoine.Core.Hosting.ApplicationInitializer;
using Lemoine.Core.Hosting;

namespace Lem_TestSystemd.Console
{
  class Program
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
      await hostBuilder.Build ().RunAsync ();
    }

    static IHostBuilder CreateHostBuilder (string[] args, Options options)
    {
      var applicationName = System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Name;
      return Host.CreateDefaultBuilder (args)
        .ConfigureConsoleAppConfiguration (options)
        .ConfigureServices ((_, services) => {
          services
          .AddHostedService<Worker> ((IServiceProvider sp) => new Worker (sp.GetRequiredService<IApplicationInitializer> (), sp.GetRequiredService<IHostApplicationLifetime> (), options.ServiceName, options.Start))
          .SetApplicationInitializer<BasicApplicationInitializer> ();
        });
    }
  }
}
