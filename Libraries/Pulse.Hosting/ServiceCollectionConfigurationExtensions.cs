// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
#if NET6_0_OR_GREATER
using Lemoine.Core.Extensions.Cache;
using Microsoft.Extensions.Caching.Memory;
#endif // NET6_0_OR_GREATER
using Lemoine.Core.Hosting;
using Lemoine.Core.Hosting.LctrChecker;
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;
using Lemoine.Database.Migration;
using Lemoine.Extensions;
using Lemoine.Extensions.ExtensionsProvider;
using Lemoine.Extensions.Interfaces;
using Lemoine.Extensions.Plugin;
using Lemoine.FileRepository;
using Lemoine.GDBMigration;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.ModelDAO.Interfaces;
using Lemoine.ModelDAO.LctrChecker;
using Lemoine.WebDataAccess.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Pulse.Database.ConnectionInitializer;
using Pulse.Extensions;
using Pulse.Hosting.ApplicationInitializer;

namespace Pulse.Hosting
{
  /// <summary>
  /// Configuration extensions of <see cref="IServiceCollection"/>
  /// </summary>
  public static class ServiceCollectionConfigurationExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ServiceCollectionConfigurationExtensions).FullName);

    static readonly string CACHE_LRU_SIZE_KEY = "Cache.LRU.Size";
    const int CACHE_LRU_SIZE_DEFAULT = 1000;

#if NET6_0_OR_GREATER
    static readonly string CACHE_IMPLEMENTATION_KEY = "Cache.Implementation";
#endif // NET6_0_OR_GREATER
    const string CACHE_IMPLEMENTATION_DEFAULT = "default"; // "lru" or "memory" or "default"

    static readonly string DATA_ACCESS_KEY = "DataAccess";
    static readonly string DATA_ACCESS_WEB = "web";
    static readonly string DATA_ACCESS_DEFAULT = DATA_ACCESS_WEB;

    /// <summary>
    /// Configure a default <see cref="IFileRepoClientFactory"/>
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureFileRepoClientFactoryDefault (this IServiceCollection services)
    {
      services.AddSingleton<IFileRepoClientFactory, FileRepoClientFactoryNoCorba> ();

      return services;
    }

    /// <summary>
    /// Configure a <see cref="IFileRepoClientFactory"/> checking first if the server is lctr
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureFileRepoClientFactoryCheckLctr (this IServiceCollection services)
    {
      return services
        .AddSingleton<IFileRepoClientFactory, Pulse.Hosting.FileRepository.FileRepoClientFactoryCheckLctr> ();
    }

    /// <summary>
    /// Configure a <see cref="IFileRepoClientFactory"/> on lctr
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureFileRepoClientFactoryLctr (this IServiceCollection services)
    {
      return services
        .AddSingleton<IFileRepoClientFactory> ((IServiceProvider sp) => new FileRepoClientFactoryNoCorba (DefaultFileRepoClientMethod.PfrDataDir)
      );
    }

    /// <summary>
    /// Configure the database with:
    /// <item>NHibernate extensions</item>
    /// <item>an extensions provider</item>
    /// 
    /// For a computer that is not lctr
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection ConfigureDatabaseWithExtensionsDefaultInitializer (this IServiceCollection services, PluginFlag pluginFlag, IEnumerable<IExtensionInterfaceProvider> interfaceProviders, string applicationName, bool killOrphanedConnectionsFirst = false)
    {
      return services.ConfigureDatabaseWithExtensions<ApplicationInitializerWithExtensions> (pluginFlag, interfaceProviders, applicationName, killOrphanedConnectionsFirst);
    }

    /// <summary>
    /// Configure the database with:
    /// <item>NHibernate extensions</item>
    /// <item>an extensions provider</item>
    /// 
    /// For a computer that is not lctr
    /// 
    /// A FileRepoClientFactory must be defined before
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection ConfigureDatabaseWithExtensions (this IServiceCollection services, PluginFlag pluginFlag, IEnumerable<IExtensionInterfaceProvider> interfaceProviders, string applicationName, bool killOrphanedConnectionsFirst = false)
    {
      services.AddSingleton<IAssemblyLoader, Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader> ();

      services.AddSingleton<IMigrationHelper, MigrationHelper> ();
      services.AddSingleton<IPluginFilter> ((IServiceProvider sp) => new PluginFilterFromFlag (pluginFlag));
      services.AddSingleton<IMainPluginsLoader> ((IServiceProvider sp) => new PluginsLoader (sp.GetRequiredService<IAssemblyLoader> (), sp.GetService<IPluginSynchronizationTimeoutProvider> ()));
      services.AddSingleton<INHibernatePluginsLoader> ((IServiceProvider sp) => new NHibernatePluginsLoader (sp.GetRequiredService<IAssemblyLoader> (), sp.GetService<IPluginSynchronizationTimeoutProvider> ()));
      services.AddSingleton ((IServiceProvider sp) => new ConnectionInitializer (applicationName, sp.GetRequiredService<IMigrationHelper> ()) {
        KillOrphanedConnectionsFirst = killOrphanedConnectionsFirst
      });
      services.AddSingleton<IDatabaseConnectionStatus> ((IServiceProvider sp) => sp.GetRequiredService<ConnectionInitializer> ());
      services.AddSingleton<IExtensionsProvider> ((IServiceProvider sp) => new ExtensionsProvider (sp.GetRequiredService<IDatabaseConnectionStatus> (), sp.GetRequiredService<IPluginFilter> (), interfaceProviders, sp.GetRequiredService<IMainPluginsLoader> (), sp.GetRequiredService<INHibernatePluginsLoader> ()));
      services.AddSingleton<IDatabaseLctrChecker, DatabaseLctrChecker> ();
      services.AddSingleton<ILctrChecker, CheckDatabaseLctrChecker> ();
      services.AddSingleton<IConnectionInitializer> ((IServiceProvider sp) => new ConnectionInitializerWithNHibernateExtensions (sp.GetRequiredService<ConnectionInitializer> (), sp.GetRequiredService<IExtensionsProvider> (), sp.GetRequiredService<IFileRepoClientFactory> (), sp.GetRequiredService<ILctrChecker> ()));
      services.AddSingleton<IExtensionsLoader, ExtensionsLoader> ();

      return services;
    }

    /// <summary>
    /// Configure the database with:
    /// <item>NHibernate extensions</item>
    /// <item>an extensions provider</item>
    /// 
    /// For a computer that is not lctr
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection ConfigureDatabaseWithExtensions<T> (this IServiceCollection services, PluginFlag pluginFlag, IEnumerable<IExtensionInterfaceProvider> interfaceProviders, string applicationName, bool killOrphanedConnectionsFirst = false)
      where T : class, IApplicationInitializer
    {
      services.ConfigureDatabaseWithExtensions (pluginFlag, interfaceProviders, applicationName, killOrphanedConnectionsFirst: killOrphanedConnectionsFirst);
      services.AddSingleton<IApplicationInitializer, T> ();

      return services;
    }

    /// <summary>
    /// Configure the database with:
    /// <item>an extensions provider</item>
    /// <item>but no NHibernate extension</item>
    /// and the default initializer (forcing the database connection)
    /// 
    /// For a computer that is not lctr
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection ConfigureDatabaseWithNoNHibernateExtensionDefaultInitializer (this IServiceCollection services, PluginFlag pluginFlag, IEnumerable<IExtensionInterfaceProvider> interfaceProviders, string applicationName, bool killOrphanedConnectionsFirst = false)
    {
      return services.ConfigureDatabaseWithExtensions<ApplicationInitializerWithExtensions> (pluginFlag, interfaceProviders, applicationName, killOrphanedConnectionsFirst);
    }

    /// <summary>
    /// Configure the database with:
    /// <item>an extensions provider</item>
    /// <item>but no NHibernate extension</item>
    /// 
    /// For a computer that is not lctr
    /// 
    /// A FileRepoClientFactory must be defined before
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection ConfigureDatabaseWithNoNHibernateExtension (this IServiceCollection services, PluginFlag pluginFlag, IEnumerable<IExtensionInterfaceProvider> interfaceProviders, string applicationName, bool killOrphanedConnectionsFirst = false)
    {
      services.AddSingleton<IAssemblyLoader, Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader> ();

      services.AddSingleton<IMigrationHelper, MigrationHelper> ();
      services.AddSingleton<IPluginFilter> ((IServiceProvider sp) => new PluginFilterFromFlag (pluginFlag));
      services.AddSingleton<IMainPluginsLoader> ((IServiceProvider sp) => new PluginsLoader (sp.GetRequiredService<IAssemblyLoader> (), sp.GetService<IPluginSynchronizationTimeoutProvider> ()));
      services.AddSingleton<INHibernatePluginsLoader, DummyPluginsLoader> ();
      services.AddSingleton ((IServiceProvider sp) => new ConnectionInitializer (applicationName, sp.GetService<IMigrationHelper> ()) {
        KillOrphanedConnectionsFirst = killOrphanedConnectionsFirst
      });
      services.AddSingleton<IDatabaseConnectionStatus> ((IServiceProvider sp) => sp.GetRequiredService<ConnectionInitializer> ());
      services.AddSingleton<IConnectionInitializer> ((IServiceProvider sp) => sp.GetRequiredService<ConnectionInitializer> ());
      services.AddSingleton<IExtensionsProvider> ((IServiceProvider sp) => new ExtensionsProvider (sp.GetRequiredService<IDatabaseConnectionStatus> (), sp.GetRequiredService<IPluginFilter> (), interfaceProviders, sp.GetRequiredService<IMainPluginsLoader> (), sp.GetRequiredService<INHibernatePluginsLoader> ()));
      services.AddSingleton<IDatabaseLctrChecker, DatabaseLctrChecker> ();
      services.AddSingleton<ILctrChecker, CheckDatabaseLctrChecker> ();
      services.AddSingleton<IExtensionsLoader, ExtensionsLoader> ();

      return services;
    }

    /// <summary>
    /// Configure the database with:
    /// <item>an extensions provider</item>
    /// <item>but no NHibernate extension</item>
    /// 
    /// For a computer that is not lctr
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection ConfigureDatabaseWithNoNHibernateExtension<T> (this IServiceCollection services, PluginFlag pluginFlag, IEnumerable<IExtensionInterfaceProvider> interfaceProviders, string applicationName, bool killOrphanedConnectionsFirst = false)
      where T : class, IApplicationInitializer
    {
      return services
        .ConfigureDatabaseWithNoNHibernateExtension (pluginFlag, interfaceProviders, applicationName, killOrphanedConnectionsFirst)
        .AddSingleton<IApplicationInitializer, T> ();
    }

    /// <summary>
    /// Configure the database with no extension load and no file repo factory
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection ConfigureDatabaseWithNoExtension (this IServiceCollection services, string applicationName, bool killOrphanedConnectionsFirst = false)
    {
      services.AddSingleton<IMigrationHelper, MigrationHelper> ();
      services.AddSingleton ((IServiceProvider sp) => new ConnectionInitializer (applicationName, sp.GetRequiredService<IMigrationHelper> ()) {
        KillOrphanedConnectionsFirst = killOrphanedConnectionsFirst
      });
      services.AddSingleton<IConnectionInitializer> ((IServiceProvider sp) => sp.GetRequiredService<ConnectionInitializer> ());
      services.AddSingleton<IDatabaseConnectionStatus> ((IServiceProvider sp) => sp.GetRequiredService<ConnectionInitializer> ());
      return services;
    }


    /// <summary>
    /// Configure the database with no extension load and no file repo factory and a default initializer
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection ConfigureDatabaseWithNoExtensionDefaultInitializer (this IServiceCollection services, string applicationName, bool killOrphanedConnectionsFirst = false)
    {
      return services
        .ConfigureDatabaseWithNoExtension (applicationName, killOrphanedConnectionsFirst: killOrphanedConnectionsFirst)
        .AddSingleton<IApplicationInitializer, ApplicationInitializerWithDatabaseNoExtension> ();
    }

    /// <summary>
    /// Configure the database with a data access using the web service or the database
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection ConfigureDataAccessFromConfigSet (this IServiceCollection services, string applicationName = null)
    {
      var effectiveApplicationName = applicationName ?? System.Reflection.Assembly.GetCallingAssembly ().GetName ().Name;
      if (Lemoine.Info.ConfigSet.LoadAndGet<string> (DATA_ACCESS_KEY, DATA_ACCESS_DEFAULT).Equals (DATA_ACCESS_WEB)) {
        return services
          .AddSingleton<IMigrationHelper, MigrationHelper> ()
          .AddSingleton ((IServiceProvider sp) => new ConnectionInitializer (applicationName, sp.GetRequiredService<IMigrationHelper> ()))
          .ConfigureWebDataAccess<ConnectionInitializer> ();
      }
      else { // DATA_ACCESS_KEY not DATA_ACCESS_WEB
        return services.ConfigureDatabaseWithNoExtension (effectiveApplicationName);
      }
    }


    /// <summary>
    /// Configure the database with no extension load and no file repo factory and a default initializer
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection ConfigureDataAccessFromConfigSetDefaultInitializer (this IServiceCollection services, string applicationName = null)
    {
      return services
        .ConfigureDataAccessFromConfigSet (applicationName)
        .AddSingleton<IApplicationInitializer, ApplicationInitializerWithDatabaseNoExtension> ();
    }

    /// <summary>
    /// Configure the database with:
    /// <item>NHibernate extensions</item>
    /// <item>an extensions provider</item>
    /// for lctr
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection ConfigureDatabaseWithExtensionsLctrDefaultInitializer (this IServiceCollection services, PluginFlag pluginFlag, IEnumerable<IExtensionInterfaceProvider> interfaceProviders, string applicationName, bool killOrphanedConnectionsFirst = false)
    {
      return services.ConfigureDatabaseWithExtensionsLctr<ApplicationInitializerWithExtensions> (pluginFlag, interfaceProviders, applicationName, killOrphanedConnectionsFirst);
    }

    /// <summary>
    /// Configure the database with:
    /// <item>NHibernate extensions</item>
    /// <item>an extensions provider</item>
    /// for lctr
    /// 
    /// A FileRepoClientFactory must be defined before
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection ConfigureDatabaseWithExtensionsLctr (this IServiceCollection services, PluginFlag pluginFlag, IEnumerable<IExtensionInterfaceProvider> interfaceProviders, string applicationName, bool killOrphanedConnectionsFirst = false)
    {
      services.AddSingleton<IAssemblyLoader, Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader> ();

      services.AddSingleton<IMigrationHelper, MigrationHelper> ();
      services.AddSingleton<IPluginFilter> ((IServiceProvider sp) => new PluginFilterFromFlag (pluginFlag));
      services.AddSingleton<IMainPluginsLoader> ((IServiceProvider sp) => new PluginsLoader (sp.GetRequiredService<IAssemblyLoader> ()));
      services.AddSingleton<INHibernatePluginsLoader> ((IServiceProvider sp) => new NHibernatePluginsLoader (sp.GetRequiredService<IAssemblyLoader> ()));
      services.AddSingleton ((IServiceProvider sp) => new ConnectionInitializer (applicationName, sp.GetRequiredService<IMigrationHelper> ()) {
        KillOrphanedConnectionsFirst = killOrphanedConnectionsFirst
      });
      services.AddSingleton<IDatabaseConnectionStatus> ((IServiceProvider sp) => sp.GetRequiredService<ConnectionInitializer> ());
      services.AddSingleton<IExtensionsProvider> ((IServiceProvider sp) => new ExtensionsProvider (sp.GetRequiredService<IDatabaseConnectionStatus> (), sp.GetRequiredService<IPluginFilter> (), interfaceProviders, sp.GetRequiredService<IMainPluginsLoader> (), sp.GetRequiredService<INHibernatePluginsLoader> ()));
      services.AddSingleton<IConnectionInitializer> ((IServiceProvider sp) => new ConnectionInitializerExtensionsLctr (sp.GetRequiredService<ConnectionInitializer> (), sp.GetRequiredService<IExtensionsProvider> ()));
      services.AddSingleton<ILctrChecker, ForceLctr> ();
      services.AddSingleton<IExtensionsLoader, ExtensionsLoaderLctr> ();

      return services;
    }

    /// <summary>
    /// Configure the database with:
    /// <item>NHibernate extensions</item>
    /// <item>an extensions provider</item>
    /// for lctr
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection ConfigureDatabaseWithExtensionsLctr<T> (this IServiceCollection services, PluginFlag pluginFlag, IEnumerable<IExtensionInterfaceProvider> interfaceProviders, string applicationName, bool killOrphanedConnectionsFirst = false)
      where T : class, IApplicationInitializer
    {
      services.ConfigureDatabaseWithExtensionsLctr (pluginFlag, interfaceProviders, applicationName, killOrphanedConnectionsFirst: killOrphanedConnectionsFirst);
      services.AddSingleton<IApplicationInitializer, T> ();

      return services;
    }

    /// <summary>
    /// Configure the business layer with a LRU cache
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureBusinessLruCache (this IServiceCollection services, int lruCacheSizeDefault = CACHE_LRU_SIZE_DEFAULT)
    {
      int cacheLruSize = Lemoine.Info.ConfigSet.LoadAndGet<int> (CACHE_LRU_SIZE_KEY,
                                                                 lruCacheSizeDefault);
      services.AddSingleton<ICacheClientWithCleanExtension> ((IServiceProvider sp) => new Lemoine.Core.Cache.LruCacheClient (cacheLruSize));
      services.AddSingleton<ICacheClient> ((IServiceProvider sp) => sp.GetRequiredService<ICacheClientWithCleanExtension> ());
      services.AddSingleton<Lemoine.Business.IService> ((IServiceProvider sp) => new Lemoine.Business.CachedService (sp.GetRequiredService<ICacheClient> ()));
      return services;
    }

    /// <summary>
    /// Configure the business layer with a LRU cache
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureBusinessCache (this IServiceCollection services, string implementationDefault = CACHE_IMPLEMENTATION_DEFAULT, int lruCacheSizeDefault = CACHE_LRU_SIZE_DEFAULT)
    {
      services.AddSingleton<ICacheClientWithCleanExtension> ((IServiceProvider sp) => CreateCacheClient (implementationDefault, lruCacheSizeDefault));
      services.AddSingleton<ICacheClient> ((IServiceProvider sp) => sp.GetRequiredService<ICacheClientWithCleanExtension> ());
      services.AddSingleton<Lemoine.Business.IService> ((IServiceProvider sp) => new Lemoine.Business.CachedService (sp.GetRequiredService<ICacheClient> ()));
      return services;
    }

    static ICacheClientWithCleanExtension CreateCacheClient (string implementationDefault = CACHE_IMPLEMENTATION_DEFAULT, int lruCacheSizeDefault = CACHE_LRU_SIZE_DEFAULT)
    {
#if NET6_0_OR_GREATER
      string cacheImplementation = Lemoine.Info.ConfigSet
        .LoadAndGet<string> (CACHE_IMPLEMENTATION_KEY, implementationDefault);
      switch (cacheImplementation.ToLowerInvariant ()) {
      case "lru": {
        int lruSize = Lemoine.Info.ConfigSet.LoadAndGet<int> (CACHE_LRU_SIZE_KEY,
                                                              lruCacheSizeDefault);
        if (log.IsInfoEnabled) {
          log.Info ($"CreateCacheClient: use a LRU cache of size {lruSize}");
        }
        return new LruCacheClient (lruSize);
      }
      case "memory":
        if (log.IsInfoEnabled) {
          log.Info ($"CreateCacheClient: use the Microsoft Memory Cache");
        }
        var memoryCache = new MemoryCache (new MemoryCacheOptions ());
        return new CacheClientWithCleanExtensionCore (new MemoryCacheClient (memoryCache));
      default: {
        int lruSize = Lemoine.Info.ConfigSet.LoadAndGet<int> (CACHE_LRU_SIZE_KEY,
                                                              lruCacheSizeDefault);
        if (log.IsInfoEnabled) {
          log.InfoFormat ($"CreateCacheClient: use a (default) LRU cache of size {lruSize} with clean extension");
        }
        return new CacheClientWithCleanExtensionCore (new LruCacheClient (lruSize));
      }
      }
#else // !NET6_0_OR_GREATER
      int lruSize = Lemoine.Info.ConfigSet.LoadAndGet<int> (CACHE_LRU_SIZE_KEY,
                                                            lruCacheSizeDefault);
      if (log.IsInfoEnabled) {
        log.Info ($"CreateCacheClient: use a LRU cache of size {lruSize}");
      }
      return new LruCacheClient (lruSize);
#endif // !NET6_0_OR_GREATER
    }

    /// <summary>
    /// Configure the i18n catalog
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureCatalog (this IServiceCollection services, string applicationName = null)
    {
      var effectiveApplicationName = applicationName ?? System.Reflection.Assembly.GetCallingAssembly ().GetName ().Name;
      return services
        .AddSingleton<ICatalog> ((IServiceProvider sp) => CreateCatalog (effectiveApplicationName + "I18N"));
    }

    static ICatalog CreateCatalog (string textFileCatalogName)
    {
      var multiCatalog = new MultiCatalog ();
      multiCatalog.Add (new StorageCatalog (new Lemoine.ModelDAO.TranslationDAOCatalog (),
                                            new TextFileCatalog (textFileCatalogName,
                                                                 Lemoine.Info.PulseInfo.LocalConfigurationDirectory)));
      multiCatalog.Add (new DefaultTextFileCatalog ());
      return new CachedCatalog (multiCatalog);
    }

  }
}
