// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Lemoine.I18N;
using Lem_CncCoreService.Services;
using Lemoine.Extensions.Interfaces;
using Lemoine.FileRepository;
using Lemoine.Model;
using Lemoine.Extensions;
using Lemoine.ModelDAO.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Lemoine.CncEngine;
using Lem_CncCoreService.Asp;
using Lemoine.Business.Config;
using Lemoine.Threading;
using Pulse.Extensions;
using Lemoine.Extensions.ExtensionsProvider;
using Pulse.Database.ConnectionInitializer;
using Lemoine.Hosting.AsyncInitialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pulse.Hosting;

namespace Lem_CncCoreService
{
  public class Startup
  {
    static readonly string LOCAL_CNC_CONFIGURATION_KEY = "CncCore.LocalCncConfiguration";
    static readonly string LOCAL_CNC_CONFIGURATION_DEFAULT = "";

    readonly ILog log = Lemoine.Core.Log.LogManager.GetLogger (typeof (Startup));

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    // For CORS configuration, visit https://aspnetcore.readthedocs.io/en/stable/security/cors.html
    public void ConfigureServices (IServiceCollection services)
    {
      services.AddCors (options => {
        options.AddPolicy ("AllowAllOrigins",
          builder => {
            builder.AllowAnyOrigin ().AllowAnyMethod ();
          });
      });

      services.Configure<KestrelServerOptions> (options => { options.AllowSynchronousIO = true; });

      var localCncConfiguration = Lemoine.Info.ConfigSet
        .LoadAndGet (LOCAL_CNC_CONFIGURATION_KEY, LOCAL_CNC_CONFIGURATION_DEFAULT);

      var applicationName = System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Name;
      services
        .ConfigureFileRepoClientFactoryDefault ()
        .ConfigureDatabaseWithNoNHibernateExtension (Lemoine.Model.PluginFlag.Cnc, GetInterfaceProviders (), applicationName, killOrphanedConnectionsFirst: true);
      services.AddSingleton<ICncEngineConfig> (new CncEngineConfig (true, true) {
        ConsoleProgramName = "",
      });

      if (string.IsNullOrEmpty (localCncConfiguration)) {
        services.AddTransient<IEnumerable<IAdditionalChecker>> ((IServiceProvider sp) => new IAdditionalChecker[] { new ConfigUpdateCheckerWithAvailableFileRepository (), new FileRepoChecker () });
        services.AddSingleton<ICncAcquisitionInitializer, LpostCncAcquisitionInitializer<Lemoine.GDBPersistentClasses.MachineModule>> ();
        services.AddSingleton<IAcquisitionSet, AcquisitionsFromCncAcquisitions> ();
        services.AddSingleton<IAcquisitionFinder, AcquisitionFinderById> (); // TODO: use another one later ?
      }
      else {
        services.AddTransient<IEnumerable<IAdditionalChecker>> ((IServiceProvider sp) => new IAdditionalChecker[] { });
        services.AddSingleton<IAcquisitionSet> ((IServiceProvider sp) => new AcquisitionsFromLocalFile (sp.GetRequiredService<ICncEngineConfig> (), localCncConfiguration, sp.GetService<IAssemblyLoader> (), sp.GetService<IFileRepoClientFactory> (), sp.GetService<IExtensionsLoader> ()));
        services.AddSingleton<IAcquisitionFinder, AcquisitionFinderUnique> ();
      }

      services.AddSingleton<IMemoryCache, MemoryCache> ();
      Asp.WebMiddlewareServices.ConfigureServices (services);

      services.AddHostedService<CncEngineWorker> ();

      if (string.IsNullOrEmpty (localCncConfiguration)) {
        services.AddAsyncInitializer<FullConfigInitializer> ();
        services.AddAsyncInitializer<SingletonsInitializer> ();
        services.AddAsyncInitializer<AssemblyLoaderInitializer> ();
        services.AddAsyncInitializer<EarlyAssemblyLoader> ();
        services.AddAsyncInitializer<LateInitializer> ();
      }
      else { // No database access or FileRepo implementation is required
        services.AddAsyncInitializer<LightConfigInitializer> ();
        services.AddAsyncInitializer<AssemblyLoaderInitializer> ();
        services.AddAsyncInitializer<EarlyAssemblyLoader> ();
      }
    }

    static IEnumerable<IExtensionInterfaceProvider> GetInterfaceProviders () => Lemoine.Extensions.Cnc.ExtensionInterfaceProvider.GetInterfaceProviders ().Union (new List<IExtensionInterfaceProvider> { new Pulse.Extensions.Database.ExtensionInterfaceProvider () });

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure (IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment ()) {
        app.UseDeveloperExceptionPage ();
      }

      app.UseCors ("AllowAllOrigins");

      app.InjectRoutingLogic ();

      app.Run (async (context) => {
        await context.Response.WriteAsync ("Hello World!");
      });
    }
  }
}
