// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using MessagePack;
using System.Reflection;
using System.IO;
using Lemoine.Info;
using System.Runtime.Serialization.Formatters.Binary;
using NHibernate.Cfg;
using Lemoine.Extensions.Database;
using MessagePack.Resolvers;

namespace Lemoine.Database.Persistent
{
  /// <summary>
  /// NHibernateConfigCache
  /// 
  /// Note: this does not work with .NET Core since System.Type is not serializable in .NET Core
  /// See https://github.com/dotnet/runtime/issues/23169
  /// and https://docs.microsoft.com/en-us/dotnet/standard/serialization/binary-serialization
  /// TODO: find a work around
  /// 
  /// For the moment Database.UseMappingCache and Database.StoreMappingCache are disallowed
  /// in DefaultCoreConfigReader
  /// </summary>
  public class NHibernateConfigCache
  {
    readonly static ILog log = LogManager.GetLogger (typeof (NHibernateConfigCache).FullName);

    const string USE_MAPPING_CACHE_KEY = "Database.UseMappingCache";
    const bool USE_MAPPING_CACHE_DEFAULT = true;

    const string FORCE_USING_MAPPING_CACHE_KEY = "Database.ForceUsingMappingCache";
    const bool FORCE_USING_MAPPING_CACHE_DEFAULT = false;

    static readonly string STORE_MAPPING_CACHE_KEY = "Database.StoreMappingCache";
    static readonly bool STORE_MAPPING_CACHE_DEFAULT = true;

    static readonly string SERIALIZER_KEY = "Database.MappingCache.Serializer"; // MessagePack / BinaryFormatter
    static readonly string SERIALIZER_MESSAGEPACK = "MessagePack";
    static readonly string SERIALIZER_BINARYFORMATTER = "BinaryFormatter"; // Soon obsolete
    static readonly string SERIALIZER_DEFAULT = SERIALIZER_BINARYFORMATTER;

    const string DEFAULT_GDB_CONFIG_CACHE_FILE = "GDBConfigCache";

    string m_applicationName;
    Configuration m_configuration = null;

    #region Getters / Setters
    /// <summary>
    /// Is the cache valid ?
    /// </summary>
    public bool Valid
    {
      get { return null != m_configuration; }
    }

    /// <summary>
    /// Loaded configuration
    /// </summary>
    public Configuration Configuration
    {
      get { return m_configuration; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="applicationName"></param>
    public NHibernateConfigCache (string applicationName)
    {
      m_applicationName = applicationName;
    }
    #endregion // Constructors

    Configuration Load ()
    {
      var configCachePath = GetConfigCachePath ();

      try {
        var serializer = Lemoine.Info.ConfigSet.LoadAndGet (SERIALIZER_KEY, SERIALIZER_DEFAULT);
        if (serializer.Equals (SERIALIZER_MESSAGEPACK)) {
          using (Stream configCacheFile = File.Open (configCachePath, FileMode.Open)) {
            m_configuration = MessagePackSerializer.Deserialize<NHibernate.Cfg.Configuration> (configCacheFile, ContractlessStandardResolver.Options);
          }
        }
        else {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
          BinaryFormatter binaryFormatter = new BinaryFormatter ();
#pragma warning restore SYSLIB0011 // Type or member is obsolete
          // Note: binary serialization is obsolete
          // This code is still active but not really required. If absolutely needed this could be removed in the future
          using (Stream configCacheFile = File.Open (configCachePath, FileMode.Open)) {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
            m_configuration = binaryFormatter.Deserialize (configCacheFile)
#pragma warning restore SYSLIB0011 // Type or member is obsolete
              as NHibernate.Cfg.Configuration;
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"Load: exception while trying to deserialize {configCachePath} => delete it", ex);
        try {
          File.Delete (configCachePath);
        }
        catch (Exception ex2) {
          log.Error ($"Load: {configCachePath} could not be deleted", ex2);
        }
        throw;
      }

      if (log.IsInfoEnabled) {
        log.Info ($"Load: pre-load the configuration with file {configCachePath}");
      }
      return m_configuration;
    }

    async System.Threading.Tasks.Task<Configuration> LoadAsync ()
    {
      var configCachePath = GetConfigCachePath ();

      var serializer = Lemoine.Info.ConfigSet.LoadAndGet (SERIALIZER_KEY, SERIALIZER_DEFAULT);
      if (serializer.Equals (SERIALIZER_MESSAGEPACK)) {
        using (Stream configCacheFile = File.Open (configCachePath, FileMode.Open)) {
          m_configuration = await MessagePackSerializer.DeserializeAsync<NHibernate.Cfg.Configuration> (configCacheFile, ContractlessStandardResolver.Options);
        }
      }
      else {
        // Note: binary serialization is obsolete
        // This code is still active but not really required. If absolutely needed this could be removed in the future
#pragma warning disable SYSLIB0011 // Type or member is obsolete
        BinaryFormatter binaryFormatter = new BinaryFormatter ();
#pragma warning restore SYSLIB0011 // Type or member is obsolete
        // TODO: Async deserialization using a memory stream
        using (Stream configCacheFile = File.Open (configCachePath, FileMode.Open)) {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
          m_configuration = await System.Threading.Tasks.Task.Run<Configuration> (() => binaryFormatter.Deserialize (configCacheFile)
#pragma warning restore SYSLIB0011 // Type or member is obsolete
            as NHibernate.Cfg.Configuration);
        }
      }

      if (log.IsInfoEnabled) {
        log.Info ($"LoadAsync: pre-load the configuration with file {configCachePath}");
      }
      return m_configuration;
    }

    /// <summary>
    /// Load the Configuration from cache if it is enabled and valid
    /// </summary>
    /// <param name="extensions"></param>
    /// <returns>null if not applicable</returns>
    public Configuration TryLoad (IEnumerable<Assembly> assemblies, IEnumerable<INHibernateExtension> extensions)
    {
      if ((Lemoine.Info.ConfigSet.LoadAndGet<bool> (USE_MAPPING_CACHE_KEY,
                                                     USE_MAPPING_CACHE_DEFAULT)
            && IsConfigurationFileValid (assemblies, extensions))
          || Lemoine.Info.ConfigSet.LoadAndGet<bool> (FORCE_USING_MAPPING_CACHE_KEY,
                                                      FORCE_USING_MAPPING_CACHE_DEFAULT)) {
        try {
          return Load ();
        }
        catch (Exception ex) {
          log.Error ($"TryLoad: exception occured while trying to deserialize the file {GetConfigCachePath ()}", ex);
          return null;
        }
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ("TryLoad: cache is not valid or not enabled");
        }
        return null;
      }
    }

    /// <summary>
    /// Load the Configuration from cache if it is enabled and valid
    /// </summary>
    /// <param name="extensions"></param>
    /// <returns>null if not applicable</returns>
    public async System.Threading.Tasks.Task<Configuration> TryLoadAsync (IEnumerable<Assembly> assemblies, IEnumerable<INHibernateExtension> extensions)
    {
      if ((Lemoine.Info.ConfigSet.LoadAndGet<bool> (USE_MAPPING_CACHE_KEY,
                                                     USE_MAPPING_CACHE_DEFAULT)
            && await IsConfigurationFileValidAsync (assemblies, extensions))
          || Lemoine.Info.ConfigSet.LoadAndGet<bool> (FORCE_USING_MAPPING_CACHE_KEY,
                                                      FORCE_USING_MAPPING_CACHE_DEFAULT)) {
        try {
          return await LoadAsync ();
        }
        catch (Exception ex) {
          log.Error ($"TryLoad: exception occured while trying to deserialize the file {GetConfigCachePath ()}", ex);
          return null;
        }
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ("TryLoad: cache is not valid or not enabled");
        }
        return null;
      }
    }

    void Store (Configuration configuration, IEnumerable<INHibernateExtension> extensions)
    {
      var serializer = Lemoine.Info.ConfigSet.LoadAndGet (SERIALIZER_KEY, SERIALIZER_DEFAULT);
      if (serializer.Equals (SERIALIZER_MESSAGEPACK)) {
        using (Stream configCacheFile = File.Open (GetConfigCachePath (), FileMode.Create)) {
          MessagePackSerializer.Serialize (configCacheFile, configuration, ContractlessStandardResolver.Options);
        }
      }
      else {
        // Note: binary serialization is obsolete
        // This code is still active but not really required. If absolutely needed this could be removed in the future
#pragma warning disable SYSLIB0011 // Type or member is obsolete
        BinaryFormatter binaryFormatter = new BinaryFormatter ();
#pragma warning restore SYSLIB0011 // Type or member is obsolete
        using (Stream configCacheFile = File.Open (GetConfigCachePath (), FileMode.Create)) {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
          binaryFormatter.Serialize (configCacheFile, configuration);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
        }
      }
      // Extensions
      var extensionString = GetExtensionsString (extensions);
      File.WriteAllText (GetExtensionsCachePath (), extensionString);
      m_configuration = configuration;
    }

    async System.Threading.Tasks.Task StoreAsync (Configuration configuration, IEnumerable<INHibernateExtension> extensions)
    {
      var serializer = Lemoine.Info.ConfigSet.LoadAndGet (SERIALIZER_KEY, SERIALIZER_DEFAULT);
      if (serializer.Equals (SERIALIZER_MESSAGEPACK)) {
        using (Stream configCacheFile = File.Open (GetConfigCachePath (), FileMode.Create)) {
          await MessagePackSerializer.SerializeAsync (configCacheFile, configuration, ContractlessStandardResolver.Options);
        }
      }
      else {
        // Note: binary serialization is obsolete
        // This code is still active but not really required. If absolutely needed this could be removed in the future
#pragma warning disable SYSLIB0011 // Type or member is obsolete
        BinaryFormatter binaryFormatter = new BinaryFormatter ();
#pragma warning restore SYSLIB0011 // Type or member is obsolete
        // TODO: async serialize with a MemoryStream
        using (Stream configCacheFile = File.Open (GetConfigCachePath (), FileMode.Create)) {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
          binaryFormatter.Serialize (configCacheFile, configuration);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
        }
      }
      // Extensions
      var extensionString = GetExtensionsString (extensions);
      // Note: File.WriteAllTextAsync is only available in .NET Core, not in .NET Standard
#if NETCOREAPP
      await File.WriteAllTextAsync (GetExtensionsCachePath (), extensionString);
#else // !NETCOREAPP
      using (var sw = new StreamWriter (GetExtensionsCachePath ())) {
        await sw.WriteAsync (extensionString);
      }
#endif
      m_configuration = configuration;
    }

    /// <summary>
    /// Try to record a cache of the NHibernate configuration
    /// </summary>
    /// <param name="configuration">not null</param>
    /// <param name="extensions"></param>
    public void TryStore (Configuration configuration, IEnumerable<INHibernateExtension> extensions)
    {
      Debug.Assert (null != configuration);

      if (Lemoine.Info.ConfigSet.LoadAndGet<bool> (STORE_MAPPING_CACHE_KEY,
                                                   STORE_MAPPING_CACHE_DEFAULT)) {
#pragma warning disable SYSLIB0050 // Type or member is obsolete
        if (!typeof (Configuration).IsSerializable) {
          log.Error ($"TryStore: configuration type is not serializable, give up");
          return;
        }
#pragma warning restore SYSLIB0050 // Type or member is obsolete
        else if (log.IsInfoEnabled) {
          log.Info ($"TryStore: configuration type is serializable, try to serialize it");
        }

        try { // Try to serialize the configuration
              // Cache itself
          Store (configuration, extensions);
        }
        catch (Exception ex) {
          log.Error ("TryStore: serializing the configuration failed", ex);
          try {
            File.Delete (GetConfigCachePath ());
          }
          catch (Exception ex1) {
            log.Error ($"TryStore: removing {GetConfigCachePath ()} failed", ex1);
          }
        }
      }
    }

    /// <summary>
    /// Try to record a cache of the NHibernate configuration
    /// </summary>
    /// <param name="configuration">not null</param>
    /// <param name="extensions"></param>
    public async System.Threading.Tasks.Task TryStoreAsync (Configuration configuration, IEnumerable<INHibernateExtension> extensions)
    {
      Debug.Assert (null != configuration);

      if (Lemoine.Info.ConfigSet.LoadAndGet<bool> (STORE_MAPPING_CACHE_KEY,
                                                   STORE_MAPPING_CACHE_DEFAULT)) {
#pragma warning disable SYSLIB0050 // Type or member is obsolete
        if (!typeof (Configuration).IsSerializable) {
          log.Error ($"TryStoreAsync: configuration type is not serializable, give up");
          return;
        }
#pragma warning restore SYSLIB0050 // Type or member is obsolete
        else if (log.IsInfoEnabled) {
          log.Info ($"TryStoreAsync: configuration type is serializable, try to serialize it");
        }

        try { // Try to serialize the configuration
              // Cache itself
          await StoreAsync (configuration, extensions);
        }
        catch (Exception ex) {
          log.Error ("TryStore: serializing the configuration failed", ex);
          try {
            File.Delete (GetConfigCachePath ());
          }
          catch (Exception ex1) {
            log.Error ($"TryStore: removing {GetConfigCachePath ()} failed", ex1);
          }
        }
      }
    }

    /// <summary>
    /// Check the cache configuration file is valid
    /// </summary>
    /// <param name="extensions"></param>
    /// <returns></returns>
    bool IsConfigurationFileValid (IEnumerable<Assembly> assemblies, IEnumerable<INHibernateExtension> extensions)
    {
      string configCachePath = GetConfigCachePath ();
      string extensionsCachePath = GetExtensionsCachePath ();

      if (!File.Exists (configCachePath)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsConfigurationFileValid: the file {configCachePath} does not exist");
        }
        return false;
      }

      if (!File.Exists (extensionsCachePath)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsConfigurationFileValid: the file {extensionsCachePath} does not exist");
        }
        return false;
      }

      FileInfo configInfo = new FileInfo (configCachePath);
      var configInfoTimeUtc = configInfo.LastWriteTimeUtc;
      { // Calling assembly
        Assembly ass = Assembly.GetCallingAssembly ();
        if (false == CheckAssemblyTime (ass, configInfoTimeUtc)) {
          log.InfoFormat ("IsConfigurationFileValid: " +
                          "config file is not ok compared to {0}",
                          ass.FullName);
          return false;
        }
      }
      foreach (var assembly in assemblies) {
        if (false == CheckAssemblyTime (assembly, configInfoTimeUtc)) {
          log.Info ($"IsConfigurationFileValid: config file is not ok compared to {assembly.FullName}");
          return false;
        }
      }

      foreach (var extension in extensions) {
        var ass = extension.GetType ().Assembly;
        if (false == CheckAssemblyTime (ass, configInfoTimeUtc)) {
          log.InfoFormat ("IsConfigurationFileValid: " +
                          "config file is not ok compared to {0}",
                          ass.FullName);
          return false;
        }
      }

      string extensionsString = GetExtensionsString (extensions);
      try {
        string cacheExtensionsString = File.ReadAllText (extensionsCachePath);
        if (!string.Equals (extensionsString, cacheExtensionsString)) {
          log.InfoFormat ("IsConfigurationFileValid: " +
                          "extensions strings differ {0} VS {1}",
                          extensionsString, cacheExtensionsString);
          return false;
        }
      }
      catch (Exception ex) {
        log.Error ($"IsConfigurationFileValid: error reading {extensionsCachePath}", ex);
        return false;
      }


      return true;
    }

    /// <summary>
    /// Check the cache configuration file is valid
    /// </summary>
    /// <returns></returns>
    async System.Threading.Tasks.Task<bool> IsConfigurationFileValidAsync (IEnumerable<Assembly> assemblies, IEnumerable<INHibernateExtension> extensions)
    {
      string configCachePath = GetConfigCachePath ();
      string extensionsCachePath = GetExtensionsCachePath ();

      if (!File.Exists (configCachePath)) {
        log.DebugFormat ("IsConfigurationFileValid: " +
                         "the file {0} does not exist",
                         configCachePath);
        return false;
      }

      if (!File.Exists (extensionsCachePath)) {
        log.DebugFormat ("IsConfigurationFileValid: " +
                         "the file {0} does not exist",
                         extensionsCachePath);
        return false;
      }

      FileInfo configInfo = new FileInfo (configCachePath);
      var configInfoTimeUtc = configInfo.LastWriteTimeUtc;
      { // Calling assembly
        Assembly ass = Assembly.GetCallingAssembly ();
        if (false == CheckAssemblyTime (ass, configInfoTimeUtc)) {
          log.InfoFormat ("IsConfigurationFileValid: " +
                          "config file is not ok compared to {0}",
                          ass.FullName);
          return false;
        }
      }
      foreach (var assembly in assemblies) {
        if (false == CheckAssemblyTime (assembly, configInfoTimeUtc)) {
          log.InfoFormat ("IsConfigurationFileValid: " +
                          "config file is not ok compared to {0}",
                          assembly.FullName);
          return false;
        }
      }

      foreach (var extension in extensions) {
        var ass = extension.GetType ().Assembly;
        if (false == CheckAssemblyTime (ass, configInfoTimeUtc)) {
          log.InfoFormat ("IsConfigurationFileValid: " +
                          "config file is not ok compared to {0}",
                          ass.FullName);
          return false;
        }
      }

      string extensionsString = GetExtensionsString (extensions);
      try {
#if NETCOREAPP
        string cacheExtensionsString = await File.ReadAllTextAsync (extensionsCachePath);
#else // !NETCOREAPP
        string cacheExtensionsString;
        using (var streamReader = new StreamReader (extensionsCachePath)) {
          cacheExtensionsString = await streamReader.ReadToEndAsync ();
        }
#endif
        if (!string.Equals (extensionsString, cacheExtensionsString)) {
          log.InfoFormat ("IsConfigurationFileValid: " +
                          "extensions strings differ {0} VS {1}",
                          extensionsString, cacheExtensionsString);
          return false;
        }
      }
      catch (Exception ex) {
        log.Error ($"IsConfigurationFileValid: error reading {extensionsCachePath}", ex);
        return false;
      }


      return true;
    }

    static string GetExtensionsString (IEnumerable<INHibernateExtension> extensions)
    {
      List<string> extensionAssemblies = new List<string> ();
      foreach (var extension in extensions) {
        var assembly = extension.GetType ().Assembly;
        extensionAssemblies.Add (assembly.FullName);
      }
      var extensionString = string.Join (";", extensionAssemblies.ToArray ());
      return extensionString;
    }

    static bool CheckAssemblyTime (Assembly ass, DateTime dateTime)
    {
      Debug.Assert (null != ass);
      if (ass.Location == null) {
        log.ErrorFormat ("CheckAssemblyTime: " +
                         "unable to get the location of the assembly");
        return false;
      }
      FileInfo assInfo = new FileInfo (ass.Location);
      if (dateTime < assInfo.LastWriteTimeUtc) {
        log.InfoFormat ("CheckAssemblyTime: " +
                        "configuration cache file is too old");
        return false;
      }
      return true;
    }

    /// <summary>
    /// Return the path of the configuration cache file
    /// </summary>
    /// <returns></returns>
    string GetConfigCachePath ()
    {
      if (null != m_applicationName) {
        return System.IO.Path.Combine (PulseInfo.LocalConfigurationDirectory, m_applicationName + ".gdbcache");
      }
      else {
        return System.IO.Path.Combine (PulseInfo.LocalConfigurationDirectory, DEFAULT_GDB_CONFIG_CACHE_FILE);
      }
    }

    /// <summary>
    /// Return the path of the extensions cache file
    /// </summary>
    /// <returns></returns>
    string GetExtensionsCachePath ()
    {
      return GetConfigCachePath () + ".extensions";
    }
  }
}
