// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// Read some registry keys from the installer
  /// 
  /// Here are the supported keys:
  /// <item>DbConnection: from PulsedbName and PulsedbDatabase</item>
  /// 
  /// Thread safe
  /// </summary>
  public class InstallerStringConfigReader: IStringConfigReader
  {
    readonly ILog log = LogManager.GetLogger (typeof (InstallerStringConfigReader).FullName);

    static readonly string DB_CONNECTION_CONFIG_KEY = "DbConnection";

    readonly IGenericConfigReader m_registryConfigReader = new RegistryConfigReader (lazy: true);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public InstallerStringConfigReader ()
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
      try {
        if (key.Equals (DB_CONNECTION_CONFIG_KEY, StringComparison.InvariantCultureIgnoreCase)) {
        // Get the properties of the connection in the registry
          var host = m_registryConfigReader.Get<string> ("PulsedbName");
          var database = m_registryConfigReader.Get<string> ("PulsedbDatabase");
          return $"{host}/{database}";
        }
      }
      catch (Exception ex) {
        log.Error ("Get: exception", ex);
        throw new ConfigKeyNotFoundException (key, ex);
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"Get: not supported key {key}");
      }
      throw new ConfigKeyNotFoundException (key);
    }
    #endregion
  }


  /// <summary>
  /// Read some registry keys from the installer
  /// 
  /// Here are the supported keys:
  /// <item>DbConnection: from PulsedbName and PulsedbDatabase</item>
  /// 
  /// Thread safe
  /// </summary>
  public class InstallerConfigReader
    : AutoConvertConfigReader
    , IGenericConfigReader
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public InstallerConfigReader ()
      : base (new InstallerStringConfigReader ())
    {
    }
  }
}
