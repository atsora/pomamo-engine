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
using static System.Environment;

namespace Lemoine.Info.ConfigReader.TargetSpecific
{
  /// <summary>
  /// Config reader to return the default values that are specific to the operating system
  /// </summary>
  public sealed class DefaultOsConfigReader : DefaultGenericConfigReader, IGenericConfigReader
  {
    const string COMMON_CONFIG_DIRECTORY_KEY = "CommonConfigDirectory";
    static readonly string LINUX_CONF_DIRECTORY = $"/etc/{PulseInfo.LinuxPackageName}"; // COMON_CONFIG_DIRECTORY_KEY
    // Windows: %CSIDL_COMMON_APPDATA%\Lemoine\PULSE

    const string LOGS_DIRECTORY_KEY = "LogDirectory";
    static readonly string LINUX_LOG_DIRECTORY = $"/var/log/{PulseInfo.LinuxPackageName}";
    // Windows: 'LocalConfigDirectory'\Logs

    const string PFR_DATA_DIRECTORY_KEY = "PfrDataDirectory";
    static readonly string LINUX_PFR_DATA_DIRECTORY = $"/usr/share/{PulseInfo.LinuxPackageName}/pfrdata"; // PFR_DATA_DIRECTORY_KEY
    // Windows: 'PulseServerInstallDir'\l_ctr\pfrdata

    const string SQL_REQUESTS_DIRECTORY_KEY = "SqlRequestsDirectory";
    static readonly string LINUX_SQL_REQUESTS_DIRECTORY = $"/usr/share/{PulseInfo.LinuxPackageName}/sql"; // SQL_REQUESTS_DIRECTORY_KEY
    // Windows: 'PulseServerInstallDir'\sql

    const string XML_SCHEMAS_DIRECTORY_KEY = "XmlSchemasDirectory";
    static readonly string LINUX_XML_SCHEMAS_DIRECTORY = $"/usr/share/{PulseInfo.LinuxPackageName}/xmlschemas";
    // Windows: 'CommonConfigDirectory'\XMLSchemas

    const string VERSIONS_DIRECTORY_KEY = "VersionsDirectory";
    static readonly string LINUX_VERSIONS_DIRECTORY = $"/usr/share/{PulseInfo.LinuxPackageName}";
    // Windows: empty

    const string ALERT_CONFIG_DIRECTORY_KEY = "Alert.ConfigDirectory";

    readonly ILog log = LogManager.GetLogger (typeof (DefaultOsConfigReader).FullName);

    readonly bool m_isWindows;
    readonly bool m_isLinux;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public DefaultOsConfigReader ()
    {
#if NETSTANDARD || NET48 || NETCOREAPP
      m_isWindows = RuntimeInformation.IsOSPlatform (OSPlatform.Windows);
      m_isLinux = RuntimeInformation.IsOSPlatform (OSPlatform.Linux);
#else // !(NETSTANDARD || NET48 || NETCOREAPP)
      m_isWindows = true;
      m_isLinux = false;
#endif // !(NETSTANDARD || NET48 || NETCOREAPP)
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="DefaultGenericConfigReader"/>
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    protected override object Get (string key)
    {
      // Note: they are also defined on Linux in file $"{PulseInfo.LinuxPackageName}.directories" (see OsConfigReader)

      switch (key) {
      case COMMON_CONFIG_DIRECTORY_KEY:
        return GetCommonConfigDirectory ();
      case LOGS_DIRECTORY_KEY:
        return GetLogDirectory ();
      case PFR_DATA_DIRECTORY_KEY:
        return GetPfrDataDirectory ();
      case SQL_REQUESTS_DIRECTORY_KEY:
        return GetSqlRequestsDirectory ();
      case XML_SCHEMAS_DIRECTORY_KEY:
        return GetXmlSchemasDirectory ();
      case ALERT_CONFIG_DIRECTORY_KEY:
        return GetConfigPath ("alert.d");
      default:
        break;
      }

      if (m_isLinux) {
        switch (key) {
        case VERSIONS_DIRECTORY_KEY:
          return LINUX_VERSIONS_DIRECTORY;
        default:
          break;
        }
      }

      throw new ConfigKeyNotFoundException (key);
    }

    string GetConfigPath (string subPath)
    {
      var path = Path.Combine (Lemoine.Info.PulseInfo.CommonConfigurationDirectory, subPath);
      if (log.IsWarnEnabled && !Directory.Exists (path)) {
        log.Warn ($"GetConfigPath: return a path {path} that does not exist");
      }
      return path;
    }

    string GetCommonConfigDirectory ()
    {
      string path;
      if (m_isWindows) {
        path = Path.Combine (GetFolderPath (SpecialFolder.CommonApplicationData), "Lemoine", "PULSE");
      }
      else {
        path = LINUX_CONF_DIRECTORY;
      }
      if (log.IsErrorEnabled && !Directory.Exists (path)) {
        log.Error ($"GetCommonConfigDirectory: return a path {path} that does not exist");
      }
      return path;
    }

    string GetLogDirectory ()
    {
      string path;
      if (m_isWindows) {
        path = Path.Combine (Lemoine.Info.PulseInfo.LocalConfigurationDirectory, "Logs");
      }
      else {
        path = LINUX_LOG_DIRECTORY;
      }
      if (log.IsErrorEnabled && !Directory.Exists (path)) {
        log.Error ($"GetLogDirectory: return a path {path} that does not exist");
      }
      return path;
    }

    string GetPfrDataDirectory ()
    {
      if (m_isWindows) {
        var pulseServerInstallationDirectory = Lemoine.Info.PulseInfo.PulseServerInstallationDirectory;
        if (null == pulseServerInstallationDirectory) {
          log.Error ($"GetPfrDataDirectory: Pulse server installation directory was not defined");
          throw new ConfigKeyNotFoundException (PFR_DATA_DIRECTORY_KEY);
        }
        return Path.Combine (pulseServerInstallationDirectory, "l_ctr", "pfrdata");
      }
      else {
        return LINUX_PFR_DATA_DIRECTORY;
      }
    }

    string GetSqlRequestsDirectory ()
    {
      if (m_isWindows) {
        var pulseServerInstallationDirectory = Lemoine.Info.PulseInfo.PulseServerInstallationDirectory;
        if (null == pulseServerInstallationDirectory) {
          log.Error ($"GetPfrDataDirectory: Pulse server installation directory was not defined");
          throw new ConfigKeyNotFoundException (SQL_REQUESTS_DIRECTORY_KEY);
        }
        return Path.Combine (pulseServerInstallationDirectory, "sql");
      }
      else {
        return LINUX_SQL_REQUESTS_DIRECTORY;
      }
    }

    string GetXmlSchemasDirectory ()
    {
      if (m_isWindows) {
        return Path.Combine (PulseInfo.CommonConfigurationDirectory, "XMLSchemas");
      }
      else {
        return LINUX_XML_SCHEMAS_DIRECTORY;
      }
    }
  }
}
