// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Config;
using Lemoine.Info;
using Lemoine.Info.ConfigReader;

namespace Lemoine.Business.Config
{
  /// <summary>
  /// ConfigReaderFromExtensions
  /// </summary>
  public class ConfigReaderFromExtensions : IGenericConfigReader
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConfigReaderFromExtensions).FullName);

    #region Getters / Setters
    IEnumerable<IConfigExtension> m_extensions = null;
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ConfigReaderFromExtensions (bool initialize = true)
    {
      if (initialize) {
        Initialize ();
      }
    }
    #endregion // Constructors

    /// <summary>
    /// Initialize the config reader
    /// </summary>
    public void Initialize ()
    {
      if (m_extensions is null) {
        try {
          var request = new Lemoine.Business.Extension
            .GlobalExtensions<IConfigExtension> (ext => ext.Initialize ());
          m_extensions = Lemoine.Business.ServiceProvider
            .Get (request)
            .OrderByDescending (ext => ext.Priority);
        }
        catch (Exception ex) {
          log.Error ($"Initialize: exception", ex);
          throw;
        }
      }
    }

    IEnumerable<IConfigExtension> GetExtensions ()
    {
      try {
        Initialize ();
      }
      catch (Exception ex) {
        log.Error ($"GetExtensions: exception in Initialize => return null", ex);
        return null;
      }
      return m_extensions;
    }

    /// <summary>
    /// <see cref="IGenericConfigReader"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T> (string key)
    {
      var extensions = GetExtensions ();
      if (extensions is null) {
        log.Error ($"Get: error when getting the extensions");
        throw new ConfigKeyNotFoundException (key);
      }

      foreach (var extension in m_extensions) {
        try {
          return extension.Get<T> (key);
        }
        catch (ConfigKeyNotFoundException ex) {
          if (log.IsDebugEnabled) {
            log.Debug ($"Get: ConfigKeyNotFoundException for plugin {extension}, check next", ex);
          }
        }
        catch (KeyNotFoundException ex) {
          log.Fatal ($"Get: deprecated KeyNotFoundException for plugin {extension}, check next", ex);
        }
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"Get: no extension returns any value for key {key}");
      }
      throw new ConfigKeyNotFoundException (key);
    }

  }
}
