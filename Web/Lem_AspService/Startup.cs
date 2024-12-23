// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;
using Lemoine.WebMiddleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Lemoine.I18N;
using Lem_AspService.Services;
using Lemoine.WebMiddleware.Assemblies;
using Lemoine.Extensions.Interfaces;
using Lemoine.FileRepository;
using Lemoine.Model;
using Lemoine.Extensions;
using Lemoine.ModelDAO.Interfaces;
using Lemoine.WebMiddleware.Log;
using Microsoft.Extensions.Caching.Memory;
using Lemoine.Extensions.ExtensionsProvider;
using Pulse.Extensions;
using Lemoine.Database.Migration;
using Lemoine.GDBMigration;
using Lemoine.Core.Hosting;
using Lemoine.Core.Hosting.LctrChecker;
using Pulse.Hosting;
using Lemoine.ModelDAO;
using Pulse.Database.ConnectionInitializer;
using Lemoine.ModelDAO.LctrChecker;
using Lemoine.Hosting.AsyncInitialization;
using Lemoine.Core.Extensions.Logging;
using System.Net.Http;

namespace Lem_AspService
{
  public class Startup
  {
    readonly ILog log = Lemoine.Core.Log.LogManager.GetLogger (typeof (Startup));

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    // For CORS configuration, visit https://aspnetcore.readthedocs.io/en/stable/security/cors.html
    public void ConfigureServices (IServiceCollection services)
    {
      services.AddCors (options => {
        options.AddPolicy ("AllowAllOrigins",
          builder => {
            builder.AllowAnyOrigin ().AllowAnyMethod ().AllowAnyHeader ();
          });
      });

      var applicationName = System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Name;

      // Note: this requires Microsoft.Extensions.Logging to be installed
      services.AddLogging (loggingBuilder => {
        loggingBuilder.AddLemoineLog ();
      });

      services
        .Configure<KestrelServerOptions> (options => { options.AllowSynchronousIO = true; })
        .AddSingleton<IAssemblyLoader, Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader> ()
        .AddSingleton<IMigrationHelper, MigrationHelper> ()
        .AddSingleton<IPluginFilter> ((IServiceProvider sp) => new PluginFilterFromFlag (PluginFlag.Web))
        .AddSingleton<IMainPluginsLoader> ((IServiceProvider sp) => new PluginsLoader (sp.GetRequiredService<IAssemblyLoader> (), sp.GetService<IPluginSynchronizationTimeoutProvider> ()))
        .AddSingleton<INHibernatePluginsLoader> ((IServiceProvider sp) => new NHibernatePluginsLoader (sp.GetRequiredService<IAssemblyLoader> (), sp.GetService<IPluginSynchronizationTimeoutProvider> ()))
        .AddSingleton ((IServiceProvider sp) => new ConnectionInitializer (sp.GetRequiredService<IMigrationHelper> ()) {
          KillOrphanedConnectionsFirst = true
        })
        .AddSingleton<IDatabaseConnectionStatus> ((IServiceProvider sp) => sp.GetRequiredService<ConnectionInitializer> ())
        .AddSingleton<IExtensionsProvider> ((IServiceProvider sp) => new ExtensionsProvider (sp.GetRequiredService<IDatabaseConnectionStatus> (), sp.GetRequiredService<IPluginFilter> (), Pulse.Extensions.Web.ExtensionInterfaceProvider.GetInterfaceProviders (), sp.GetRequiredService<IMainPluginsLoader> (), sp.GetRequiredService<INHibernatePluginsLoader> ()))
        .AddSingleton<IDatabaseLctrChecker, DatabaseLctrChecker> ()
        .AddSingleton<ILctrChecker, CheckDatabaseLctrChecker> ()
        .AddSingleton<IConnectionInitializer> ((IServiceProvider sp) => new ConnectionInitializerWithNHibernateExtensions (sp.GetRequiredService<ConnectionInitializer> (), sp.GetRequiredService<IExtensionsProvider> (), sp.GetRequiredService<IFileRepoClientFactory> (), sp.GetRequiredService<ILctrChecker> ()))
        .AddSingleton<IExtensionsLoader, ExtensionsLoader> ()
        .AddSingleton<IFileRepoClientFactory, Services.FileRepoClientFactory> ()
        .AddSingleton<ICatalog, DefaultCatalog> ()
        .AddSingleton<Lemoine.Business.IBusinessServiceFactory, DefaultBusinessServiceFactory> ()
        .AddSingleton<System.Net.Http.HttpClient> ();

      var serviceAssemblies = new List<System.Reflection.Assembly> ()
      {
        typeof (Lemoine.Web.Cache.FlushCacheService).Assembly
        , typeof (Pulse.Web.Machine.GroupService).Assembly
        , typeof (Lemoine.WebService.ServiceHelper).Assembly
        , typeof (Lemoine.Web.ProductionMachiningStatus.CurrentMachineStateTemplateOperation.CurrentMachineStateTemplateOperationRequestDTO).Assembly
      };
      services.AddSingleton<IServiceAssembliesResolver> ((IServiceProvider sp) => new DefaultServiceAssemblies (serviceAssemblies, sp.GetService<IExtensionsLoader> ()));
      services.AddSingleton<HttpClient> ();

      services.AddMemoryCache ();
      WebMiddlewareServices.ConfigureServices (services);
      WebMiddlewareServices.AddAuthentication (services);
      WebMiddlewareServices.AddAuthorization (services);
#pragma warning disable ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
      var serviceProvider = services.BuildServiceProvider ();
#pragma warning restore ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
      WebMiddlewareServices.RegisterServices (services, serviceProvider);

      Pulse.Graphql.GraphQLMiddlewareServices.ConfigureServices (services);

      services.AddHostedService<FlushCacheWorker> ();

      services.AddAsyncInitializer<ConfigInitializer> ();
      services.AddAsyncInitializer<SingletonsInitializer> (); // They should be already initialized, but it is safer to add it here
      services.AddAsyncInitializer<CatalogInitializer> ();
      services.AddAsyncInitializer<BusinessInitializer> ();
      services.AddAsyncInitializer<LateInitializer> ();
      services.AddAsyncInitializer<OdbcInitializer> ();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure (IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment ()) {
        app.UseDeveloperExceptionPage ();
      }

      app.UseCors ("AllowAllOrigins");

      app.InjectPulseMiddleware ();

      app.Run (async (context) => {
        await context.Response.WriteAsync ("Hello World!");
      });
    }
  }
}
