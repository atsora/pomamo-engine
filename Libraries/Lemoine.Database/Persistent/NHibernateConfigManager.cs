// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using NHibernate.Cfg;
using Lemoine.Extensions.Database;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Reflection;
using Lemoine.Extensions.Interfaces;
using Lemoine.Info;

namespace Lemoine.Database.Persistent
{
  /// <summary>
  /// NHibernateConfigManager
  /// </summary>
  public class NHibernateConfigManager
  {
    static readonly string NHIBERNATE_CONFIG_PATH_KEY = "NHibernateConfig.Path";
    static readonly string NHIBERNATE_CONFIG_PATH_DEFAULT = ""; // Default: empty: consider application + suffix

    static readonly string NHIBERNATE_CONFIG_SUFFIX_KEY = "NHibernateConfig.Suffix";
    static readonly string NHIBERNATE_CONFIG_SUFFIX_DEFAULT = ".nh.cfg.xml"; // If empty, use the default hibernate.cfg.xml

    static readonly string CORE_MEMORY_CACHE_EXPIRATION_SCAN_FREQUENCY_KEY = "nhibernate.corememorycache.ExpirationScanFrequency";
    static readonly TimeSpan CORE_MEMORY_CACHE_EXPIRATION_SCAN_FREQUENCY_DEFAULT = TimeSpan.FromMinutes (5);

    readonly static ILog log = LogManager.GetLogger (typeof (NHibernateConfigManager).FullName);

    Configuration m_configuration;

    #region Getters / Setters
    /// <summary>
    /// NHibernate configuration
    /// </summary>
    public Configuration Configuration
    {
      get { return m_configuration; }
    }
    #endregion // Getters / Setters

    #region Constructors
    NHibernateConfigManager (params Assembly[] assemblies)
      : this (CreateInitializeNHibernateConfig (assemblies))
    {
    }

    NHibernateConfigManager (Configuration configuration)
    {
      m_configuration = configuration;
    }
    #endregion // Constructors

    static string GetDefaultConnectionString ()
    {
      // Note that default MaxPoolSize (for the Npgsql connection pool) is
      // 100 since version 3.1 of Npgsql (20 previously)
      return $"Server=localhost;Database=${Constants.DEFAULT_DATABASE_NAME};User ID=${Constants.DEFAULT_DATABASE_USER};Password=${Constants.DEFAULT_DATABASE_PASSWORD};";
    }

    static Configuration CreateInitializeNHibernateConfig (params Assembly[] assemblies)
    {
      var configuration = new Configuration ();

      // 1. Set default values
      // i> PostgreSQL
      configuration.SetProperty ("connection.provider",
                                 "NHibernate.Connection.DriverConnectionProvider");
      configuration.SetProperty ("connection.driver_class",
                                 "NHibernate.Driver.NpgsqlDriver");
      configuration.SetProperty ("connection.connection_string", GetDefaultConnectionString ());
      configuration.SetProperty ("dialect",
                                 "NHibernate.Dialect.PostgreSQLPulseDialect, Lemoine.NHibernate");
      // iii> Context
      configuration.SetProperty ("current_session_context_class",
                                 "async_local");
      // iv> Cache
      configuration.SetProperty ("cache.provider_class",
                                 "NHibernate.Caches.CoreMemoryCache.CoreMemoryCacheProvider, NHibernate.Caches.CoreMemoryCache");
      // To use SysCache: (or set it in the .exe.config)
      // configuration.SetProperty ("cache.provider_class", "NHibernate.Caches.SysCache.SysCacheProvider, NHibernate.Caches.SysCache");
      // To use Bamboo Prevalence: (or set it in the .exe.config)
      // configuration.SetProperty ("cache.provider_class", "NHibernate.Caches.Prevalence.PrevalenceCacheProvider, NHibernate.Caches.Prevalence");
      // For Prevalence only:
      var prevalenceBase = System.IO.Path.Combine (Lemoine.Info.PulseInfo.LocalConfigurationDirectory, "PrevalenceCacheStorage");
      configuration.SetProperty ("cache.prevalenceBase", prevalenceBase);
      configuration.SetProperty ("cache.use_query_cache", "false");//that makes the unit tests crash for the moment
      configuration.SetProperty ("cache.use_second_level_cache", "false");
      configuration.SetProperty ("cache.default_expiration", "180"); // 3 minutes
                                                                     // configuration.SetProperty ("show_sql", "true"); // for debug only !
                                                                     // v> Load the .hbm.xml files
      configuration.SetProperty ("cache.use_sliding_expiration", "false"); // Default: false. For CoreMemoryCache
      foreach (var assembly in assemblies) {
        configuration.AddAssembly (assembly);
      }

      // CoreMemoryCache configuration
      NHibernate.Caches.CoreMemoryCache.CoreMemoryCacheProvider.ExpirationScanFrequency = Lemoine.Info.ConfigSet
        .LoadAndGet (CORE_MEMORY_CACHE_EXPIRATION_SCAN_FREQUENCY_KEY, CORE_MEMORY_CACHE_EXPIRATION_SCAN_FREQUENCY_DEFAULT);
      // To configure regions:
      // NHibernate.Caches.CoreMemoryCache.CoreMemoryCacheProvider.SetRegionConfiguration (new NHibernate.Caches.CoreMemoryCache.RegionConfig (region, expiration, sliding));

      return configuration;
    }

    /// <summary>
    /// Create a new NHibernate config manager with cache and extensions
    /// </summary>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static NHibernateConfigManager CreateNoCacheNoExtensions (params Assembly[] assemblies)
    {
      var nhibernateConfigManager = new NHibernateConfigManager (assemblies);
      return nhibernateConfigManager;
    }

    /// <summary>
    /// Create a new NHibernate config manager (full capabilities)
    /// </summary>
    /// <param name="extensionsProvider"></param>
    /// <param name="applicationName"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static NHibernateConfigManager Create (IExtensionsProvider extensionsProvider, string applicationName, params Assembly[] assemblies)
    {
      if (log.IsDebugEnabled) {
        log.Debug ("Create");
      }

      IEnumerable<INHibernateExtension> extensions;
      if (extensionsProvider is null) {
        if (log.IsDebugEnabled) {
          log.Debug ("Create: extensionsProvider is null");
        }
        extensions = new List<INHibernateExtension> ();
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ("Create: load and get the NHibernate extensions");
        }
        extensions = extensionsProvider
          .LoadAndGetNHibernateExtensions<Lemoine.Extensions.Database.INHibernateExtension> ();
      }

      // 1. Try to pre-load the configuration to make the process faster
      var cache = new NHibernateConfigCache (applicationName);
      var configuration = cache.TryLoad (assemblies, extensions);
      if (null != configuration) {
        if (log.IsDebugEnabled) {
          log.Debug ("Create: return configuration from cache");
        }
        return new NHibernateConfigManager (configuration);
      }
      else { // null == configuration
        var nhibernateConfigManager = CreateNoCacheNoExtensions (assemblies);
        nhibernateConfigManager.AddExtensions (extensions);

        if (log.IsDebugEnabled) {
          log.Debug ("Create: a new configuration was created");
        }

        configuration = nhibernateConfigManager.Configuration;
        cache.TryStore (configuration, extensions);

        return nhibernateConfigManager;
      }
    }

    /// <summary>
    /// Create a new NHibernate config manager (full capabilities)
    /// </summary>
    /// <param name="extensionsProvider"></param>
    /// <param name="applicationName"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static async System.Threading.Tasks.Task<NHibernateConfigManager> CreateAsync (IExtensionsProvider extensionsProvider, string applicationName, params Assembly[] assemblies)
    {
      if (log.IsDebugEnabled) {
        log.Debug ("CreateAsync");
      }

      IEnumerable<INHibernateExtension> extensions;
      if (extensionsProvider is null) {
        if (log.IsDebugEnabled) {
          log.Debug ("Create: extensionsProvider is null");
        }
        extensions = new List<INHibernateExtension> ();
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ("Create: load and get the NHibernate extensions");
        }
        extensions = extensionsProvider
          .LoadAndGetNHibernateExtensions<Lemoine.Extensions.Database.INHibernateExtension> ();
      }

      // 1. Try to pre-load the configuration to make the process faster
      var cache = new NHibernateConfigCache (applicationName);
      var configuration = await cache.TryLoadAsync (assemblies, extensions);
      if (null != configuration) {
        if (log.IsDebugEnabled) {
          log.Debug ("Create: return configuration from cache");
        }
        return new NHibernateConfigManager (configuration);
      }
      else { // null == configuration
        var nhibernateConfigManager = CreateNoCacheNoExtensions (assemblies);
        nhibernateConfigManager.AddExtensions (extensions);

        if (log.IsDebugEnabled) {
          log.Debug ("Create: a new configuration was created");
        }

        configuration = nhibernateConfigManager.Configuration;
        await cache.TryStoreAsync (configuration, extensions);

        return nhibernateConfigManager;
      }
    }

    void AddExtensions (IEnumerable<INHibernateExtension> extensions)
    {
      // - Extensions
      //   Note: this needs to be included in the mapping cache, I am not sure why yet,
      //         else the new assemblies are not loaded correctly
      try {
        foreach (var extension in extensions) {
          if (extension.ContainsMapping ()) {
            try {
              var assembly = extension.GetType ().Assembly;
              m_configuration.AddAssembly (assembly);
            }
            catch (Exception ex) {
              log.Error ($"AddExtensions: error while trying to add extension assembly {extension.GetType ().Assembly} to NHibernate", ex);
            }
          }
          try {
            extension.UpdateConfiguration (ref m_configuration);
          }
          catch (Exception ex) {
            log.Error ($"AddExtensions: exception in UpdateConfiguration", ex);
          }
        }
      }
      catch (Exception ex) {
        log.Error ("AddExtensions: " +
                   "error when loading all the extensions",
                   ex);
      }
    }

    /// <summary>
    /// Configure NHibernate with the specified connection string
    /// </summary>
    /// <param name="connectionString"></param>
    public void Configure (string connectionString)
    {
      SetConnectionString (connectionString);
      TryConfigure ();
      if (log.IsInfoEnabled) {
        log.InfoFormat ("Configure: " +
                        "the connection string is <{0}>",
                        this.Configuration.GetProperty ("connection.connection_string"));
      }
    }

    /// <summary>
    /// Configure NHibernate with the specified connection string
    /// </summary>
    /// <param name="connectionString"></param>
    public async System.Threading.Tasks.Task ConfigureAsync (string connectionString)
    {
      SetConnectionString (connectionString);
      await TryConfigureAsync ();
      if (log.IsInfoEnabled) {
        log.InfoFormat ("Configure: " +
                        "the connection string is <{0}>",
                        this.Configuration.GetProperty ("connection.connection_string"));
      }
    }

    void SetConnectionString (string connectionString)
    {
      m_configuration.SetProperty ("connection.connection_string",
                                   connectionString);
    }

    string GetConfigurationFilePath ()
    {
      var path = Lemoine.Info.ConfigSet.LoadAndGet (NHIBERNATE_CONFIG_PATH_KEY, NHIBERNATE_CONFIG_PATH_DEFAULT);
      if (!string.IsNullOrEmpty (path) && File.Exists (path)) {
        return path;
      }

      var suffix = Lemoine.Info.ConfigSet.LoadAndGet (NHIBERNATE_CONFIG_SUFFIX_KEY, NHIBERNATE_CONFIG_SUFFIX_DEFAULT);
      if (!string.IsNullOrEmpty (suffix)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetConfigurationFilePath: consider suffix {suffix}");
        }
        string programAbsolutePath = Lemoine.Info.ProgramInfo.AbsolutePath;
        if (string.IsNullOrEmpty (programAbsolutePath) || !File.Exists (programAbsolutePath)) {
          log.Error ($"GetConfigurationFilePath: program path {programAbsolutePath} is null or does not exist");
        }
        else {
          if (!programAbsolutePath.EndsWith (".exe") && !programAbsolutePath.EndsWith (".dll")) {
            log.Error ($"GetConfigurationFilePath: program path {programAbsolutePath} with a wrong extension");
          }
          var configurationFilePath = System.IO.Path.ChangeExtension (programAbsolutePath, suffix);
          if (File.Exists (configurationFilePath)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetConfigurationFilePath: file {configurationFilePath} exists");
            }
            return configurationFilePath;
          }
          else {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetConfigurationFilePath: file {configurationFilePath} does not exist");
            }
          }
        }
      }

      if (log.IsInfoEnabled) {
        log.Info ($"GetConfigurationFilePath: no configuration file path could be determined, use the default one");
      }
      return "";
    }

    [DebuggerNonUserCode]
    void TryConfigure ()
    {
      var configurationFilePath = GetConfigurationFilePath ();
      try {
        if (!string.IsNullOrEmpty (configurationFilePath)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"TryConfigure: consider config file {configurationFilePath}");
          }
          m_configuration.Configure (configurationFilePath);
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"TryConfigure: no config file");
          }
          m_configuration.Configure ();// default: hibernate.cfg.xml
        }
        if (log.IsInfoEnabled) {
          log.Info ("TryConfigure: use the nhibernate configuration from " +
                    "hibernate.cfg.xml or the configuration file of the " +
                    "application");
        }
      }
      catch (Exception ex) {
        if (log.IsInfoEnabled) {
          log.Info ("TryConfigure: " +
                    "configuration.Configure failed",
                    ex);
        }
      }
    }

    [DebuggerNonUserCode]
    async System.Threading.Tasks.Task TryConfigureAsync ()
    {
      var configurationFilePath = GetConfigurationFilePath ();
      try {
        if (!string.IsNullOrEmpty (configurationFilePath)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"TryConfigure: consider config file {configurationFilePath}");
          }
          await System.Threading.Tasks.Task.Run (() => m_configuration.Configure (configurationFilePath));// default: hibernate.cfg.xml
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"TryConfigure: no config file");
          }
          await System.Threading.Tasks.Task.Run (() => m_configuration.Configure ());// default: hibernate.cfg.xml
        }
        if (log.IsInfoEnabled) {
          log.Info ("TryConfigure: use the nhibernate configuration from " +
                    "hibernate.cfg.xml or the configuration file of the " +
                    "application");
        }
      }
      catch (Exception ex) {
        if (log.IsInfoEnabled) {
          log.Info ("TryConfigure: " +
                    "configuration.Configure failed",
                    ex);
        }
      }
    }
  }
}
