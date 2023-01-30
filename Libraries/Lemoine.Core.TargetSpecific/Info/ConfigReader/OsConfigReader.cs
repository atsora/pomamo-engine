// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Lemoine.Core.Log;

namespace Lemoine.Info.ConfigReader.TargetSpecific
{
  /// <summary>
  /// OsConfigReader
  /// </summary>
  public sealed class OsConfigReader: IOsConfigReader
  {
    static readonly string LINUX_CONF_DIRECTORY = "/etc/lpulse";

    readonly ILog log = LogManager.GetLogger (typeof (OsConfigReader).FullName);

    readonly MultiConfigReader m_multiConfigReader = new MultiConfigReader ();
    readonly bool m_isWindows;
    readonly bool m_isLinux;
    readonly IPersistentConfigWriter m_configWriter = null;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public OsConfigReader ()
    {
#if NETSTANDARD || NET48 || NETCOREAPP
      m_isWindows = RuntimeInformation.IsOSPlatform (OSPlatform.Windows);
      m_isLinux = RuntimeInformation.IsOSPlatform (OSPlatform.Linux);
#else // !(NETSTANDARD || NET48 || NETCOREAPP)
      m_isWindows = true;
      m_isLinux = false;
#endif // !(NETSTANDARD || NET48 || NETCOREAPP)
#if NET40 || NET45 || NET48
      if (m_isWindows) {
        m_multiConfigReader.Add (new WindowsConfigReader ());
      }
#endif // NET40 || NET45 || NET48
      m_multiConfigReader.Add (new EnvironmentConfigReader ());
      m_multiConfigReader.Add (new EnvironmentRawConfigReader ());
      if (m_isWindows) {
        var registryConfigReader = new RegistryConfigReader ();
        m_multiConfigReader.Add (registryConfigReader);
        var commonApplicationData = System.Environment.GetFolderPath (System.Environment.SpecialFolder.CommonApplicationData);
        var commonConfigDirectory = Path.Combine (commonApplicationData, "Lemoine", "PULSE");
        var directoryConfigReader = new OptionsDirectoryConfigReader (Path.Combine (commonConfigDirectory, "lpulse.options.d"));
        m_multiConfigReader.Add (directoryConfigReader);
        m_multiConfigReader.Add (new OdbcConfigReader ());
        m_multiConfigReader.Add (new InstallerConfigReader ());
        // Note: to use registryConfigReader as the configWriter, administrator rights are required
        m_configWriter = directoryConfigReader;
      }
      if (m_isLinux) {
        m_multiConfigReader.Add (OptionsFileConfigReader.CreateFromPath (Path.Combine (LINUX_CONF_DIRECTORY, "lpulse.options")));
        var directoryConfigReader = new OptionsDirectoryConfigReader (Path.Combine (LINUX_CONF_DIRECTORY, "lpulse.options.d"));
        m_multiConfigReader.Add (directoryConfigReader);
        m_multiConfigReader.Add (OptionsFileConfigReader.CreateFromPath (Path.Combine (LINUX_CONF_DIRECTORY, "lpulse.directories")));
        m_configWriter = directoryConfigReader;
      }
#if NETCOREAPP
      m_multiConfigReader.Add (new DefaultCoreConfigReader ());
#endif // NETCOREAPP
      m_multiConfigReader.Add (new DefaultOsConfigReader ());
    }

#if NETSTANDARD || NET48 || NETCOREAPP
    /// <summary>
    /// Os specific configuration using Microsoft.Extensions.IConfiguration
    /// </summary>
    /// <param name="configuration"></param>
    public OsConfigReader (Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
#if NETSTANDARD || NET48 || NETCOREAPP
      m_isWindows = RuntimeInformation.IsOSPlatform (OSPlatform.Windows);
      m_isLinux = RuntimeInformation.IsOSPlatform (OSPlatform.Linux);
#else // !(NETSTANDARD || NET48 || NETCOREAPP)
      m_isWindows = true;
      m_isLinux = false;
#endif // !(NETSTANDARD || NET48 || NETCOREAPP)
      m_multiConfigReader.Add (new CoreConfigReader (configuration));
      m_multiConfigReader.Add (new EnvironmentConfigReader ()); // Is it necessary since CoreConfigReader already supports some kind of environment variables ?
      m_multiConfigReader.Add (new EnvironmentRawConfigReader ());
      if (m_isWindows) {
        var registryConfigReader = new RegistryConfigReader ();
        m_multiConfigReader.Add (registryConfigReader);
        var commonApplicationData = System.Environment.GetFolderPath (System.Environment.SpecialFolder.CommonApplicationData);
        var commonConfigDirectory = Path.Combine (commonApplicationData, "Lemoine", "PULSE");
        var directoryConfigReader = new OptionsDirectoryConfigReader (Path.Combine (commonConfigDirectory, "lpulse.options.d"));
        m_multiConfigReader.Add (directoryConfigReader);
        m_multiConfigReader.Add (new OdbcConfigReader ());
        m_multiConfigReader.Add (new InstallerConfigReader ());
        // Note: to use registryConfigReader as the configWriter, administrator rights are required
        m_configWriter = directoryConfigReader;
      }
      if (m_isLinux) {
        m_multiConfigReader.Add (OptionsFileConfigReader.CreateFromPath (Path.Combine (LINUX_CONF_DIRECTORY, "lpulse.options")));
        var directoryConfigReader = new OptionsDirectoryConfigReader (Path.Combine (LINUX_CONF_DIRECTORY, "lpulse.options.d"));
        m_multiConfigReader.Add (directoryConfigReader);
        m_multiConfigReader.Add (OptionsFileConfigReader.CreateFromPath (Path.Combine (LINUX_CONF_DIRECTORY, "lpulse.directories")));
        m_configWriter = directoryConfigReader;
      }
#if NETCOREAPP
      m_multiConfigReader.Add (new DefaultCoreConfigReader ());
#endif // NETCOREAPP
      m_multiConfigReader.Add (new DefaultOsConfigReader ());
    }
#endif // NETSTANDARD || NET48 || NETCOREAPP

    /// <summary>
    /// <see cref="IGenericConfigReader"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T> (string key)
    {
      return m_multiConfigReader.Get<T> (key);
    }
#endregion // Constructors

    /// <summary>
    /// <see cref="IPersistentConfigWriter"/>
    /// </summary>
    /// <param name="key"></param>
    public void ResetPersistentConfig (string key)
    {
      if (null != m_configWriter) {
        m_configWriter.ResetPersistentConfig (key);
      }
      else {
        log.Error ($"ResetPersistentConfig: no config writer is set, please run Lemoine.Info.ConfigSet.SetOsConfigReader first");
        throw new InvalidOperationException ("No config writer is set");
      }
    }

    /// <summary>
    /// <see cref="IPersistentConfigWriter"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="v"></param>
    public bool SetPersistentConfig<T> (string key, T v, bool overwrite)
    {
      if (null != m_configWriter) {
        return m_configWriter.SetPersistentConfig (key, v, overwrite);
      }
      else {
        log.Error ($"SetPersistentConfig: no config writer is set, please run Lemoine.Info.ConfigSet.SetOsConfigReader first");
        throw new InvalidOperationException ("No config writer is set");
      }
    }

  }
}
