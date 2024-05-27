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
using Lemoine.Extensions.Interfaces;
using Lemoine.Extensions.Plugin;
using Lemoine.I18N;
using Microsoft.Extensions.DependencyInjection;
using Pulse.Hosting;
using Pulse.Hosting.ApplicationInitializer;

namespace Lem_OperationExplorer
{
  /// <summary>
  /// ServiceCollectionExtensions
  /// </summary>
  public static class ServiceCollectionExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ServiceCollectionExtensions).FullName);

    static readonly TimeSpan? PLUGIN_SYNCHRONIZATION_TIMEOUT_DEFAULT = TimeSpan.FromSeconds (1);

    public static IServiceCollection CreateLemOperationExplorerServices (this IServiceCollection services)
    {
      return services
        .AddSingleton<IPluginSynchronizationTimeoutProvider> ((IServiceProvider sp) => new PluginSynchronizationTimeoutProviderFromConfigSet (PLUGIN_SYNCHRONIZATION_TIMEOUT_DEFAULT))
        .CreateGuiServicesDatabaseWithNoNHibernateExtension (Lemoine.Model.PluginFlag.OperationExplorer, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders ())
        .ConfigureBusinessLruCache ()
        .SetApplicationInitializer<ApplicationInitializerWithExtensions, BusinessApplicationInitializer, PulseCatalogInitializer> ()
        .AddTransient<MainForm> ();
    }

  }
}
