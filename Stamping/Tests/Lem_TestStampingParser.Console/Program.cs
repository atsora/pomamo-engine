// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Lemoine.Stamping;
using Lemoine.Core.Plugin;
using Lemoine.Stamping.Impl;
using Lemoine.Stamping.StampingEventHandlersProviders;
using Lemoine.Conversion;
using Lemoine.Core.Log;

namespace Lem_TestStampingParser.Console
{
  class Program
  {
    static async Task Main (string[] args)
    {
      LogManager.AddLog4net ();
      var build = CreateHostBuilder (args).Build ();
      await build.RunAsync ();
    }

    public static IHostBuilder CreateHostBuilder (string[] args) =>
        Host.CreateDefaultBuilder (args)
            .ConfigureServices ((_, services) => {
              services
              .AddHostedService<Worker> ()
              .AddScoped<FullStampingProcess> ()
              .AddSingleton<IAssemblyLoader, Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader> ()
              .AddSingleton<TypeLoader> ()
              .AddSingleton<IAutoConverter, DefaultAutoConverter> ()
              .AddSingleton<IStampingEventHandlersProvider> (sp => CreateStampingEventHandlersProvider ())
              .AddSingleton<Lemoine.StandardGCodesParser.Configuration> () // Required for StampingParser
              .AddScoped<IStampingParser, Lemoine.StandardGCodesParser.StampingParser> ()
              .AddScoped<IStampingApplicationBuilder, StampingApplicationBuilder> ()
              .AddSingleton<IMilestoneStampLineCreator, TestStampLineCreator> ()
              .AddScoped<Lemoine.Stamping.StampingEventHandlers.LogEvents> ()
              .AddScoped<Lemoine.Stamping.StampingEventHandlers.SequenceTimeRecorder> ()
              .AddScoped<Lemoine.Stamping.StampingEventHandlers.SequenceAtToolChange> ()
              .AddScoped<Lemoine.Stamping.StampingEventHandlers.DelayMachiningSequence> ()
              .AddScoped<Lemoine.Stamping.StampingEventHandlers.SequenceTagWriter> ()
              .AddScoped<StampingData> ()
              .AddScoped<IStamper, TestStamperFromString> ()
              .AddScoped<IStampingFileFlow, Lemoine.Stamping.StampingFileFlows.NoFileFlow> ();
            });

    static IStampingEventHandlersProvider CreateStampingEventHandlersProvider () =>
      new EventHandlersProviderBuilder ()
        .Add<Lemoine.Stamping.StampingEventHandlers.LogEvents> ()
        .Add<Lemoine.Stamping.StampingEventHandlers.SequenceAtToolChange> ()
        .Add<Lemoine.Stamping.StampingEventHandlers.DelayMachiningSequence> ()
        .Add<Lemoine.Stamping.StampingEventHandlers.SequenceTimeRecorder> ()
        .Add<Lemoine.Stamping.StampingEventHandlers.SequenceTagWriter> ();

  }
}
