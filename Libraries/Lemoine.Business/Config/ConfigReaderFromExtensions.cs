// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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

    IEnumerable<IConfigExtension> m_extensions = null;
    /// <summary>
    /// Set while the extensions are initialized or requested in the current thread,
    /// to prevent a re-entrant call that would end in a stack overflow,
    /// for example when the load of an assembly an extension depends on
    /// requires a config value (see PluginLoadContext)
    /// </summary>
    readonly ThreadLocal<bool> m_inProgress = new ThreadLocal<bool> (() => false);

    /// <summary>
    /// Constructor
    /// </summary>
    public ConfigReaderFromExtensions (bool initialize = true)
    {
      if (initialize) {
        Initialize ();
      }
    }

    /// <summary>
    /// Initialize the config reader
    /// </summary>
    public void Initialize ()
    {
      if (m_extensions is null) {
        if (m_inProgress.Value) {
          log.Error ($"Initialize: re-entrant call, probably because an extension requires a config value during its initialization");
          throw new InvalidOperationException ("Re-entrant initialization of ConfigReaderFromExtensions");
        }
        m_inProgress.Value = true;
        try {
          var request = new Lemoine.Business.Extension
            .GlobalExtensions<IConfigExtension> (InitializeExtension);
          // Note: ToList, so that the extensions and their priority are evaluated only once, here
          m_extensions = Lemoine.Business.ServiceProvider
            .Get (request)
            .OrderByDescending (ext => ext.Priority)
            .ToList ();
        }
        catch (Exception ex) {
          log.Error ($"Initialize: exception", ex);
          throw;
        }
        finally {
          m_inProgress.Value = false;
        }
      }
    }

    /// <summary>
    /// Initialize an extension.
    /// An extension that raises an exception is skipped, so that the other extensions are still active
    /// </summary>
    /// <param name="extension">not null</param>
    /// <returns>the extension is active</returns>
    bool InitializeExtension (IConfigExtension extension)
    {
      try {
        return extension.Initialize ();
      }
      catch (Exception ex) {
        if (Lemoine.Core.ExceptionManagement.ExceptionTest.RequiresExit (ex)) {
          log.Error ($"InitializeExtension: exception in extension {extension} that requires to exit, throw it", ex);
          throw;
        }
        log.Error ($"InitializeExtension: exception in extension {extension} => skip it", ex);
        return false;
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
      if (m_inProgress.Value) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Get: re-entrant call for key {key} during the initialization or a request of the extensions => skip this config reader");
        }
        throw new ConfigKeyNotFoundException (key);
      }

      var extensions = GetExtensions ();
      if (extensions is null) {
        log.Error ($"Get: error when getting the extensions");
        throw new ConfigKeyNotFoundException (key);
      }

      m_inProgress.Value = true;
      try {
        foreach (var extension in extensions) {
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
      }
      finally {
        m_inProgress.Value = false;
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"Get: no extension returns any value for key {key}");
      }
      throw new ConfigKeyNotFoundException (key);
    }

  }
}
