// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using CommandLine;
using Lemoine.Core.Log;
using Lemoine.Core.Extensions.Hosting;
using Lemoine.Info.ConfigReader.TargetSpecific;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pulse.Business.Computer;
using Pulse.Hosting.ApplicationInitializer;
using Lemoine.Core.Performance;
using Lemoine.Core.Hosting;
using Pulse.Hosting;

namespace Lem_ActivityAnalysis.Console
{
  class Program
  {
    const string CACHE_IMPLEMENTATION_DEFAULT = "default"; // "lru" or "memory" or "default"
    const int CACHE_LRU_SIZE_DEFAULT = 1000;

    static readonly ILog log = LogManager.GetLogger (typeof (Program).FullName);

    /// <summary>
    /// Program entry point.
    /// </summary>
    public static async Task Main (string[] args)
    {
      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());

      Options options = null;
      bool global = false;
      int machineId = -1;
      int sleepTime;

      try {
        var result = CommandLine.Parser.Default.ParseArguments<Options> (args);

        result.WithNotParsed (errors => {
          Environment.ExitCode = 1;
          return;
        });

        result.WithParsed (opt => {
          var parameters = opt.Parameters;
          if (null != parameters) {
            Lemoine.Info.ConfigSet.AddCommandLineParameters (parameters);
          }
          global = opt.Global;
          machineId = opt.MachineId;
          sleepTime = opt.SleepTime;

          var logFileSuffix = "";
          if (global) {
            logFileSuffix += "-g";
          }
          else if (int.MinValue != machineId) {
            logFileSuffix += "-" + machineId;
          }
          LogManager.AddLog4netDefaultConfigurationFile (logFileSuffix);
          options = opt;
        });
      }
      catch (Exception ex) {
        log.Error ("Main: Exception raised", ex);
        System.Console.Error.WriteLine ($"Exception {ex} raised");
        Environment.ExitCode = 1;
        return;
      }

      if (options is null) {
        return;
      }

      string applicationName = "Lem_ActivityAnalysis.Console";
      if (global) {
        applicationName += "-global";
      }
      else if (int.MinValue != machineId) {
        applicationName += "-" + machineId;
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
        .ConfigureFileRepoClientFactoryLctr ()
        .ConfigureDatabaseWithExtensionsLctr (Lemoine.Model.PluginFlag.Analysis, Lemoine.Extensions.Analysis.ExtensionInterfaceProvider.GetInterfaceProviders (), applicationName, killOrphanedConnectionsFirst: true)
        .ConfigureBusinessCache (implementationDefault: CACHE_IMPLEMENTATION_DEFAULT, lruCacheSizeDefault: CACHE_LRU_SIZE_DEFAULT)
        .AddSingleton<IPerfRecorder, Lemoine.Business.Performance.PerfRecorderFromExtensions> ()
        .SetApplicationInitializer<ApplicationInitializerWithExtensions, BusinessApplicationInitializer, PerfRecorderInitializer> ()
        .AddSingleton<IConsoleRunner<Options>, ConsoleRunner> ();
    }
  }
}
