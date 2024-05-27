// Copyright (C) 2024 Atsora Solutions

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.BaseControls;
using Lemoine.Core.Extensions.Hosting;
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;
using Lemoine.DataControls;
using Lemoine.DataControls.GuiInitializer;
using Lemoine.Extensions;
using Lemoine.Extensions.DummyImplementations;
using Lemoine.Extensions.Interfaces;
using Lemoine.Extensions.Plugin;
using Lemoine.I18N;
using Microsoft.Extensions.DependencyInjection;
using Pulse.Hosting;
using Pulse.Hosting.ApplicationInitializer;

namespace Lem_ExcelDataImport
{
  /// <summary>
  /// ServiceCollectionExtensions
  /// </summary>
  public static class ServiceCollectionExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ServiceCollectionExtensions).FullName);

    static readonly TimeSpan? PLUGIN_SYNCHRONIZATION_TIMEOUT_DEFAULT = TimeSpan.FromSeconds (1);

    public static IServiceCollection CreateExcelDataImportServices (this IServiceCollection services)
    {
      return services
        .AddSingleton<IPluginSynchronizationTimeoutProvider> ((IServiceProvider sp) => new PluginSynchronizationTimeoutProviderFromConfigSet (PLUGIN_SYNCHRONIZATION_TIMEOUT_DEFAULT))
        .CreateGuiServicesDatabaseWithNoNHibernateExtension (Lemoine.Model.PluginFlag.OperationExplorer, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders ())
        .ConfigureBusinessLruCache ()
        .SetApplicationInitializer<ApplicationInitializerWithExtensions, BusinessApplicationInitializer, PulseCatalogInitializer> ()
        .AddTransient<ExcelViewForm> ();
    }

  }
}
