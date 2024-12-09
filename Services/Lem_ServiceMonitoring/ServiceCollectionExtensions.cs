// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Extensions.Hosting;
using Lemoine.Core.Log;
using Lemoine.DataControls;
using Lemoine.I18N;
using LemoineServiceMonitoring;
using Microsoft.Extensions.DependencyInjection;
using Pulse.Hosting.ApplicationInitializer;
#if CONNECTOR
using Atsora.JsonFileData;
using Lemoine.Core.Hosting;
using Lemoine.BaseControls;
using Lemoine.DataControls.GuiInitializer;
using Lemoine.Extensions.DummyImplementations;
using Lemoine.Extensions.Interfaces;
using Lemoine.FileRepository;
using Lemoine.WebDataAccess.Hosting;
using Pulse.Hosting;
#endif // CONNECTOR

namespace Lem_ServiceMonitoring
{
  /// <summary>
  /// ServiceCollectionExtensions
  /// </summary>
  public static class ServiceCollectionExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ServiceCollectionExtensions).FullName);

#if CONNECTOR
    static readonly string DATA_ACCESS_KEY = "DataAccess";
    static readonly string DATA_ACCESS_WEB = "web";
    static readonly string DATA_ACCESS_DEFAULT = DATA_ACCESS_WEB;
#endif // CONNECTOR

    public static IServiceCollection CreateServiceMonitoringServices (this IServiceCollection services)
    {
#if CONNECTOR
      services
        .ConfigureFileRepoClientFactoryLctr ();
      if (Lemoine.Info.ConfigSet.LoadAndGet<string> (DATA_ACCESS_KEY, DATA_ACCESS_DEFAULT).Equals (DATA_ACCESS_WEB)) {
        services
          .AddSingleton ((IServiceProvider sp) => new JsonFileDataConnectionInitializer ())
          .ConfigureWebDataAccess<JsonFileDataConnectionInitializer> ();
      }
      else {
        services
          .ConfigureJsonFileDataOnly ();
      }
      return services
        .AddSingleton<IGuiInitializer> ((IServiceProvider sp) => new GuiInitializer (sp.GetRequiredService<IApplicationInitializer> (), sp.GetService<IFileRepoClientFactory> ()))
        .AddSingleton<IExtensionsLoader, ExtensionsLoaderDummy> ()
        .AddSingleton<IApplicationInitializer, ApplicationInitializerConnector> ()
#else // !CONNECTOR
      return services
        .CreateGuiServicesDataAccessFromConfigSet ()
        .SetApplicationInitializer<ApplicationInitializerWithDatabaseNoExtension, PulseCatalogInitializer> ()
#endif // !CONNECTOR
        .AddTransient<MainForm> ();

    }

  }
}
