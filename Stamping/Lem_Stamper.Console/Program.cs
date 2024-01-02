// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using CommandLine;

using Lemoine.Stamping;
using Lemoine.Stamping.Config;
using Lemoine.Stamping.Impl;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Lemoine.Core.Log;
using Lemoine.Core.Extensions.Hosting;
using System.IO;
using Lemoine.Core.Plugin;
using Pulse.Hosting;
using Pulse.Hosting.ApplicationInitializer;
using Lemoine.Conversion;
using Lemoine.Model;
using System.Collections.Generic;

namespace Lem_Stamper.Console
{
  class Program
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Program).FullName);

    static readonly string CONFIG_ERROR_FILE_FLOW_KEY = "Lem_Stamper.Console.ConfigErrorFileFlow";
    static readonly string CONFIG_ERROR_FILE_FLOW_DEFAULT = "Lemoine.Stamping.ConfigErrorFileFlows.MoveOnConfigError, Lemoine.Stamping";

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

      IDictionary<string, object> initialStampingData = new Dictionary<string, object> ();
      foreach (var data in options.Data) {
        var keyValue = data.Split (new char[] { '=' }, 2);
        if (2 != keyValue.Length) {
          log.Error ($"Main: invalid stamping data {data}, probably no separator '=' in it");
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"Main: add stamping data {keyValue[0]} = {keyValue[1]}");
          }
          initialStampingData[keyValue[0]] = keyValue[1];
        }
      }
      if (!string.IsNullOrEmpty (options.ConfigName)) { 
        initialStampingData["StampingConfigName"] = options.ConfigName;
      }

      var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();

      StampingConfig stampingConfig;
      if (!Directory.Exists (options.Output)) {
        log.Info ($"Main: create directory {options.Output}");
        Directory.CreateDirectory (options.Output);
      }
      var outputFilePath = Path.Combine (options.Output, Path.GetFileName (options.Input));
      try {
        var stampingConfigFactory = GetStampingConfigFactory (options);
        stampingConfig = stampingConfigFactory.CreateStampingConfig ();
      }
      catch (Exception ex) {
        log.Error ($"Main: error while getting the stamping config", ex);
        await System.Console.Error.WriteLineAsync ($"Stamping config error: {ex.Message}");
        await OnConfigErrorAsync (assemblyLoader, options.Input, outputFilePath);
        return;
      }
      var hostBuilder = CreateHostBuilder (args, options, assemblyLoader, stampingConfig, options.Input, outputFilePath, initialStampingData);
      await hostBuilder.Build ().RunAsync ();
    }

    static async Task OnConfigErrorAsync (IAssemblyLoader assemblyLoader, string inputFilePath, string outputFilePath)
    {
      try {
        var configErrorFileFlowTypeName = Lemoine.Info.ConfigSet
          .LoadAndGet (CONFIG_ERROR_FILE_FLOW_KEY, CONFIG_ERROR_FILE_FLOW_DEFAULT);
        var typeLoader = new TypeLoader (assemblyLoader);
        var configErrorFileFlow = typeLoader.Load<IConfigErrorFileFlow> (configErrorFileFlowTypeName);
        if (configErrorFileFlow is null) {
          log.Fatal ($"Main: no config error flow with type {configErrorFileFlowTypeName} => do nothing");
          return;
        }
        await configErrorFileFlow.OnConfigError (inputFilePath, outputFilePath);
      }
      catch (Exception ex) {
        log.Fatal ($"OnConfigError: exception", ex);
        throw;
      }
    }

    static IStampingConfigFactory GetStampingConfigFactory (Options options)
    {
      if (!string.IsNullOrEmpty (options.ConfigFilePath)) {
        if (!string.IsNullOrEmpty (options.ConfigName)) {
          log.Error ($"GetStampingConfig: both config name and config file path are set => use config file path {options.ConfigFilePath}, name={options.ConfigName} is skipped");
        }
        return new Lemoine.Stamping.Config.StampingConfigFromFile (options.ConfigFilePath);
      }
      else {
        if (string.IsNullOrEmpty (options.ConfigName)) {
          log.Fatal ("GetStampingConfig: Nor config file path nor config name is set, which is unexpected");
          throw new ArgumentException ("Nor config file path nor config name is set", "options");
        }
        return new Lemoine.Stamping.Config.StampingConfigFromName (options.ConfigName);
      }
    }

    static IHostBuilder CreateHostBuilder (string[] args, Options options, IAssemblyLoader assemblyLoader, StampingConfig stampingConfig, string inputFilePath, string outputFilePath, IEnumerable<KeyValuePair<string, object>> initialStampingData)
    {
      var applicationName = System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Name;
      var typeLoader = new TypeLoader (assemblyLoader);
      return Host.CreateDefaultBuilder (args)
        .ConfigureConsoleAppConfiguration (options)
        .ConfigureServices ((_, services) => {
          services
          .AddHostedService<Worker> ()
          .AddSingleton<ILogFactory> (LogManager.LoggerFactory)
          .AddScoped<FullStampingProcess> ()
          .AddSingleton<StampingConfig> (stampingConfig)
          .AddSingleton<IAssemblyLoader, Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader> ()
          .AddSingleton<TypeLoader> (typeLoader)
          .AddSingleton<IAutoConverter, DefaultAutoConverter> ()
          .AddSingleton<IStampingEventHandlersProvider, Lemoine.Stamping.StampingEventHandlersProviders.EventHandlersProviderFromStampingConfig> ()
          .AddSingleton<IStamperParametersProvider> (sp => new StamperParametersProvider (inputFilePath, outputFilePath))
          .AddSingleton<IStampingApplicationBuilder, StampingApplicationBuilder> ()
          .AddStampingConfig (typeLoader, stampingConfig)
          .AddSingleton<StampingData> (sp => new StampingData (initialStampingData))
          .ConfigureDatabaseWithNoExtension (applicationName)
          .SetApplicationInitializer<ApplicationInitializerWithDatabaseNoExtension> ();
        });
    }
  }
}
