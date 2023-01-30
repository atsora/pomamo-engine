// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Lemoine.Core.Log;

namespace Lemoine.Info.ConfigReader.TargetSpecific
{
  /// <summary>
  /// Read the ODBC configuration and set them in the config key DbConnection[.dsnname]
  /// 
  /// Thread safe
  /// </summary>
  public class OdbcStringConfigReader : IStringConfigReader
  {
    readonly ILog log = LogManager.GetLogger (typeof (OdbcStringConfigReader).FullName);

#if !NETSTANDARD
    static readonly string DB_CONNECTION_CONFIG_KEY = "DbConnection";
    static readonly string DEFAULT_DSNNAME = "DsnName";
#endif // !NETSTANDARD

#region Getters / Setters
#endregion // Getters / Setters

#region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public OdbcStringConfigReader ()
    {
    }
#endregion // Constructors

#region IStringConfigReader implementation
    /// <summary>
    /// <see cref="IStringConfigReader"/>
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetString (string key)
    {
#if NETSTANDARD
        log.Fatal ($"GetString: .Net Standard compilation is not supported");
        throw new ConfigKeyNotFoundException (key, "Not supported compilation", new NotSupportedException ("Net Standard"));
#else // !NETSTANDARD
#if NET48 || NETCOREAPP
      if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        if (log.IsDebugEnabled) {
          log.Debug ("GetString: not a windows platform, not applicable config reader, return KeyNotFound");
        }
        throw new ConfigKeyNotFoundException (key, "Not supported platform", new PlatformNotSupportedException ("Registry is not supported on this platform"));
      }
#endif // NET48 || NETCOREAPP

      if (key.StartsWith (DB_CONNECTION_CONFIG_KEY, StringComparison.InvariantCultureIgnoreCase)) {
        string dsnName;
        if (key.Equals (DB_CONNECTION_CONFIG_KEY, StringComparison.InvariantCultureIgnoreCase)) {
          dsnName = DEFAULT_DSNNAME;
        }
        else if (key.StartsWith (DB_CONNECTION_CONFIG_KEY + ".", StringComparison.InvariantCultureIgnoreCase)) {
          dsnName = key.Substring (DB_CONNECTION_CONFIG_KEY.Length + 1);
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"Get: not supported key {key}");
          }
          throw new ConfigKeyNotFoundException (key);
        }

        // Get the properties of the connection in the registry
        try {
          using (var odbcKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey ("SOFTWARE\\ODBC\\ODBC.INI\\" + dsnName)) {
            if (null == odbcKey) {
              log.Warn ($"Get: no registry entry for dsnname=${dsnName}");
              throw new ConfigKeyNotFoundException (key);
            }
            var server = (string)odbcKey.GetValue ("Servername");
            var port = (string)odbcKey.GetValue ("Port");
            var database = (string)odbcKey.GetValue ("Database");
            var username = (string)odbcKey.GetValue ("Username");
            var password = (string)odbcKey.GetValue ("Password");
            if (log.IsInfoEnabled) {
              log.InfoFormat ("Get: " +
                              "The connection parameters taken from the registry are " +
                              "server={0} port={1} database={2} user={3} password={4}",
                              server, port, database, username, password);
            }
            return $"{username}:{password}@{server}:{port}/{database}";
          }
        }
        catch (Exception ex) {
          log.Warn ($"Get: Could not get the connection parameters in registry for dsnname=${dsnName}", ex);
          throw new ConfigKeyNotFoundException (key, "Could not get the connection parameters in registry", ex);
        }
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"Get: not supported key {key}");
      }
      throw new ConfigKeyNotFoundException (key);
#endif // NETSTANDARD
    }

#endregion
  }

  /// <summary>
  /// Read the ODBC configuration and set them in the config key DbConnection[.dsnname]
  /// 
  /// Thread safe
  /// </summary>
  public class OdbcConfigReader
    : AutoConvertConfigReader
    , IGenericConfigReader
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public OdbcConfigReader ()
      : base (new OdbcStringConfigReader ())
    {
    }
  }
}
