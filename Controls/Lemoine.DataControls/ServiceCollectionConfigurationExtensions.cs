// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.BaseControls;
using Lemoine.Core.Log;
using Lemoine.Extensions;
using Lemoine.Model;
using Microsoft.Extensions.DependencyInjection;
using Pulse.Hosting;

namespace Lemoine.DataControls
{
  /// <summary>
  /// ServiceCollectionConfigurationExtensions
  /// </summary>
  public static class ServiceCollectionConfigurationExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ServiceCollectionConfigurationExtensions).FullName);

    /// <summary>
    /// Extension to <see cref="IServiceCollection"/>
    /// 
    /// Create the Gui services when a database access with no extension is required
    /// </summary>
    /// <param name="services"></param>
    /// <param name="applicationName"></param>
    /// <returns></returns>
    public static IServiceCollection CreateGuiServicesDatabaseNoExtension (this IServiceCollection services, string applicationName = null)
    {
      var effectiveApplicationName = applicationName ?? System.Reflection.Assembly.GetCallingAssembly ().GetName ().Name;
      return services
        .ConfigureFileRepoClientFactoryDefault ()
        .ConfigureDatabaseWithNoExtension (effectiveApplicationName)
        .ConfigureCatalog (effectiveApplicationName)
        .AddSingleton<IGuiInitializer, GuiInitializer.GuiInitializer> ();
    }

    /// <summary>
    /// Extension to <see cref="IServiceCollection"/>
    /// 
    /// Create the Gui services when a database access and all extensions (including NHibernate extensions)
    /// </summary>
    /// <param name="services"></param>
    /// <param name="applicationName"></param>
    /// <returns></returns>
    public static IServiceCollection CreateGuiServicesDatabaseWithExtensions (this IServiceCollection services, PluginFlag pluginFlag, IEnumerable<IExtensionInterfaceProvider> interfaceProviders, string applicationName = null)
    {
      var effectiveApplicationName = applicationName ?? System.Reflection.Assembly.GetCallingAssembly ().GetName ().Name;
      return services
        .ConfigureFileRepoClientFactoryDefault ()
        .ConfigureDatabaseWithExtensions (pluginFlag, interfaceProviders, effectiveApplicationName)
        .ConfigureCatalog (effectiveApplicationName)
        .AddSingleton<IGuiInitializer, GuiInitializer.GuiInitializer> ();
    }

    /// <summary>
    /// Extension to <see cref="IServiceCollection"/>
    /// 
    /// Create the Gui services when a database access and with the extensions but not the NHibernate extensions
    /// </summary>
    /// <param name="services"></param>
    /// <param name="applicationName"></param>
    /// <returns></returns>
    public static IServiceCollection CreateGuiServicesDatabaseWithNoNHibernateExtension (this IServiceCollection services, PluginFlag pluginFlag, IEnumerable<IExtensionInterfaceProvider> interfaceProviders, string applicationName = null, TimeSpan? pluginSynchronizationTimeoutDefault = null)
    {
      var effectiveApplicationName = applicationName ?? System.Reflection.Assembly.GetCallingAssembly ().GetName ().Name;
      return services
        .ConfigureFileRepoClientFactoryDefault ()
        .ConfigureDatabaseWithNoNHibernateExtension (pluginFlag, interfaceProviders, effectiveApplicationName)
        .ConfigureCatalog (effectiveApplicationName)
        .AddSingleton<IGuiInitializer, GuiInitializer.GuiInitializer> ();
    }

    /// <summary>
    /// Extension to <see cref="IServiceCollection"/>
    /// 
    /// Create the Gui services with a data access from config set
    /// </summary>
    /// <param name="services"></param>
    /// <param name="applicationName"></param>
    /// <returns></returns>
    public static IServiceCollection CreateGuiServicesDataAccessFromConfigSet (this IServiceCollection services, string applicationName = null)
    {
      var effectiveApplicationName = applicationName ?? System.Reflection.Assembly.GetCallingAssembly ().GetName ().Name;
      return services
        .ConfigureFileRepoClientFactoryDefault ()
        .ConfigureDataAccessFromConfigSet (effectiveApplicationName)
        .ConfigureCatalog (effectiveApplicationName)
        .AddSingleton<IGuiInitializer, GuiInitializer.GuiInitializer> ();
    }

  }
}
