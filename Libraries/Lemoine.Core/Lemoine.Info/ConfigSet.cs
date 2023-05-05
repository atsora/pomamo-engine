// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Runtime.InteropServices;
using Lemoine.Core.Log;
using Lemoine.Info.ConfigReader;

namespace Lemoine.Info
{
  /// <summary>
  /// Singleton class to easily retrieve some configuration values.
  /// 
  /// The configuration values are retrieved in the following sources (in this order):
  /// <item>the command line parameters if provided</item>
  /// <item>the .exe.options (.NET Framework) or .dll.options (.NET Core) file</item>
  /// <item>the .exe.defaultoptions (.NET Framework) or .dll.defaultoptions (.NET Core) file</item>
  /// <item>the .exe.config file (for .NET Framework and Windows only)</item>
  /// <item>the environment values</item>
  /// <item>the registry under the key HKLM\SOFTWARE\Lemoine\PULSE</item>
  /// <item>the ODBC settings if applicable</item>
  /// <item>some specific registry keys that are set by the installer</item>
  /// <item>the value from the additional config readers</item>
  /// <item>the specified default value</item>
  /// 
  /// The OS or .NET Core specific config readers can be overridden, for example to support:
  /// <item>the .NET Core configurations from ASP.NET Core</item>
  /// <item>configuration files in /etc/pomamo on Linux</item>
  /// 
  /// The following types are supported:
  /// <item>string</item>
  /// <item>int</item>
  /// <item>double</item>
  /// <item>bool</item>
  /// <item>TimeSpan</item>
  /// </summary>
  public sealed class ConfigSet
  {
    #region Members
    readonly IGenericConfigReader m_baseConfigReader; // Config reader that is used by default
    readonly CachedConfigReader m_cachedConfigReader; // Base cached config reader
    readonly CommandLineConfigReader m_commandLineConfigReader = new CommandLineConfigReader ();
    readonly UpdatingConfigReader m_osConfigReader = new UpdatingConfigReader ();
    readonly MultiConfigReader m_additionalConfigReaders = new MultiConfigReader ();
    readonly MemoryConfigReader m_forceConfigReader = new MemoryConfigReader ();
    readonly MemoryConfigReader m_defaultValuesConfigReader = new MemoryConfigReader ();

    bool m_forceActive = false;

    IGenericConfigReader m_configReader; // Active config reader. It may be overriden
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ConfigSet).FullName);

    #region Getters / Setters
    /// <summary>
    /// Return the associated config reader
    /// </summary>
    static public IGenericConfigReader ConfigReader
    {
      get { return Instance.m_configReader; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private ConfigSet ()
    {
      // Os config reader
#if !NETSTANDARD
      var isWindows = true;
#else // NETSTANDARD
      var isWindows = RuntimeInformation.IsOSPlatform (OSPlatform.Windows);
#endif // NETSTANDARD
      var osConfigReader = new MultiConfigReader ();
      if (isWindows) {
        osConfigReader.Add (new WindowsConfigReader ());
      }
      osConfigReader.Add (new EnvironmentConfigReader ());
      if (isWindows) {
        osConfigReader.Add (new RegistryConfigReader ());
        osConfigReader.Add (new OdbcConfigReader ());
        osConfigReader.Add (new InstallerConfigReader ());
      }
      m_osConfigReader.Set (osConfigReader);

      // Base config reader
      var multiConfigReader = new MultiConfigReader ();
      multiConfigReader.Add (m_commandLineConfigReader);
      multiConfigReader.Add (OptionsFileConfigReader.CreateFromExtension ());
      multiConfigReader.Add (OptionsFileConfigReader.CreateFromExtension (".defaultoptions"));
      multiConfigReader.Add (m_osConfigReader);
      multiConfigReader.Add (m_additionalConfigReaders);
      multiConfigReader.Add (m_defaultValuesConfigReader);
      m_cachedConfigReader = new CachedConfigReader (multiConfigReader);
      m_baseConfigReader = m_cachedConfigReader;

      // Active config reader
      m_configReader = m_baseConfigReader;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add command line parameters
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="separator"></param>
    public static void AddCommandLineParameters (IEnumerable<string> parameters, char separator = '=')
    {
      if (null != parameters) {
        Instance.m_commandLineConfigReader.AddParameters (parameters, separator);

        // Reset the cache
        Instance.m_cachedConfigReader.Clear ();
      }
    }

    /// <summary>
    /// Set the OS specific config reader
    /// </summary>
    /// <param name="osConfigReader">not null</param>
    public static void SetOsConfigReader (IOsConfigReader osConfigReader)
    {
      if (null == osConfigReader) {
        log.Fatal ("SetOsConfigReader: null parameter");
        throw new ArgumentNullException ("osConfigReader");
      }

      Instance.m_osConfigReader.Set (osConfigReader, osConfigReader);

      // Reset the cache
      Instance.m_cachedConfigReader.Clear ();
    }

    /// <summary>
    /// Reset the config reader to the default one, without any additional config reader
    /// </summary>
    public static void ResetConfigReader ()
    {
      Instance.m_configReader = Instance.m_baseConfigReader;
      Instance.m_additionalConfigReaders.Clear ();

      // Reset the cache
      Instance.m_cachedConfigReader.Clear ();
    }

    /// <summary>
    /// Replace the base config reader by another one, for example for the tests
    /// </summary>
    /// <param name="configReader"></param>
    public static void ReplaceConfigReader (IGenericConfigReader configReader)
    {
      Instance.m_configReader = configReader;
    }

    /// <summary>
    /// Add an additional low priority config reader to the base config reader
    /// </summary>
    /// <param name="additionalConfigReader"></param>
    public static void AddConfigReader (IGenericConfigReader additionalConfigReader)
    {
      // Use the base config reader again
      Instance.m_configReader = Instance.m_baseConfigReader;

      // Add the additional config reader
      Instance.m_additionalConfigReaders.Add (additionalConfigReader);

      // Reset the cache
      Instance.m_cachedConfigReader.Clear ();
    }

    /// <summary>
    /// Clear any additional config readers that was added previously
    /// </summary>
    public static void ClearAdditionalConfigReaders ()
    {
      Instance.m_additionalConfigReaders.Clear ();
      Instance.m_cachedConfigReader.Clear ();
    }

    /// <summary>
    /// Reset the cache
    /// </summary>
    public static void ResetCache ()
    {
      Instance.m_cachedConfigReader.Clear ();
    }

    /// <summary>
    /// Set a persistent config
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <param name="overwrite"></param>
    /// <returns>false if the key already existed and overwrite was false</returns>
    public static bool SetPersistentConfig<T> (string key, T v, bool overwrite = true)
    {
      var result = Instance.m_osConfigReader.SetPersistentConfig (key, v, overwrite);
      ResetCache ();
      return result;
    }

    /// <summary>
    /// Reset a persistent config
    /// </summary>
    /// <param name="key"></param>
    public static void ResetPersistentConfig (string key)
    {
      Instance.m_osConfigReader.ResetPersistentConfig (key);
      ResetCache ();
    }

    /// <summary>
    /// Reset all the forced values (after a unit test for example)
    /// </summary>
    public static void ResetForceValues ()
    {
      Instance.m_configReader = Instance.m_baseConfigReader;
      Instance.m_forceActive = false;
      Instance.m_forceConfigReader.Clear ();
    }

    /// <summary>
    /// Remove a specific forced value
    /// </summary>
    /// <param name="key"></param>
    public static void RemoveForcedValue (string key)
    {
      Instance.m_forceConfigReader.Remove (key);
    }

    /// <summary>
    /// Force a value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    public static void ForceValue<T> (string key, T defaultValue)
    {
      if (!Instance.m_forceActive) {
        var multi = new MultiConfigReader ();
        multi.Add (Instance.m_forceConfigReader);
        multi.Add (Instance.m_baseConfigReader);
        Instance.m_configReader = multi;
      }

      Instance.m_forceConfigReader.Add (key, defaultValue, true);
    }

    /// <summary>
    /// Get the value associated to a give key after loading it if necessary 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static object LoadAndGet (string key, object defaultValue)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"LoadAndGet: key={key} default={defaultValue}");
      }
      Load (key, defaultValue);
      return Get (key);
    }

    /// <summary>
    /// Get the value associated to a given key or an alternate key after
    /// 
    /// If key is already loaded, return it.
    /// Else a value for alternate key is searched. If it is found it is returned.
    /// Else the default value is returned
    /// </summary>
    /// <param name="key"></param>
    /// <param name="alternateKey"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static object LoadAndGet (string key, string alternateKey, object defaultValue)
    {
      object result;

      try {
        result = Get (key);
        return result;
      }
      catch (ConfigKeyNotFoundException) {
      }

      try {
        result = Get (alternateKey);
      }
      catch (ConfigKeyNotFoundException) {
        result = defaultValue;
      }

      Load (key, result);
      return result;
    }

    /// <summary>
    /// Get the value associated to a given key after loading it if necessary 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static T LoadAndGet<T> (string key, T defaultValue)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"LoadAndGet: key={key} default={defaultValue}");
      }
      Load<T> (key, defaultValue);
      return Get<T> (key);
    }

    /// <summary>
    /// Get the value associated to a given key after loading it if necessary 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="alternateKey"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static T LoadAndGet<T> (string key, string alternateKey, T defaultValue)
    {
      T result;

      try {
        result = Get<T> (key);
        return result;
      }
      catch (ConfigKeyNotFoundException) {
      }

      try {
        result = Get<T> (alternateKey);
      }
      catch (ConfigKeyNotFoundException) {
        result = defaultValue;
      }

      Load<T> (key, result);
      return result;
    }

    /// <summary>
    /// Get the value associated to a given key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static T Get<T> (string key)
    {
      return Instance.m_configReader.Get<T> (key);
    }

    /// <summary>
    /// Get the value associated to a given key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static object Get (string key)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: key={key}");
      }
      return Instance.m_configReader.Get<object> (key);
    }

    /// <summary>
    /// Load a config key into the singleton class,
    /// specifying a default value
    /// 
    /// By default, the data is only loaded from the .options/.config files
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    public static void Load<T> (string key, T defaultValue)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Load: about to load key {key} with default value {defaultValue}");
      }
      Instance.m_defaultValuesConfigReader.Add (key, defaultValue);
    }

    /// <summary>
    /// Load a config key into the singleton class,
    /// specifying a default value
    /// 
    /// By default, the data is only loaded from the .options/.config files
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <param name="overrideDefault">overwrite the value</param>
    public static void Load<T> (string key, T defaultValue, bool overrideDefault)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Load: about to load key {key} with default value {defaultValue} overwrite {overrideDefault}");
      }
      Instance.m_defaultValuesConfigReader.Add (key, defaultValue, overrideDefault);
    }

    /// <summary>
    /// Load a config key into the singleton class,
    /// specifying a default value
    /// 
    /// By default, the data is only loaded from the .options/.config files
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    public static void Load (string key, object defaultValue)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Load: about to load key {key} with default value {defaultValue}");
      }
      Instance.m_defaultValuesConfigReader.Add (key, defaultValue);
    }
    #endregion // Methods

    #region Instance
    static ConfigSet Instance
    {
      get { return Nested.instance; }
    }

    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested ()
      {
      }

      internal static readonly ConfigSet instance = new ConfigSet ();
    }
    #endregion // Instance
  }
}
