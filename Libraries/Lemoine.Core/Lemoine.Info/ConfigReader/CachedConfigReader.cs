// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// Cache layer for a config reader
  /// 
  /// Thread safe if the associated config reader is thread safe
  /// </summary>
  public sealed class CachedConfigReader
    : IGenericConfigReader
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CachedConfigReader).FullName);

    readonly ConcurrentDictionary<string, object> m_dictionary = new ConcurrentDictionary<string, object> (StringComparer.InvariantCultureIgnoreCase);
    readonly IGenericConfigReader m_configReader;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configReader"></param>
    public CachedConfigReader (IGenericConfigReader configReader)
    {
      m_configReader = configReader;
    }
    #endregion // Constructors

    #region IGenericConfigReader implementation
    /// <summary>
    /// <see cref="IGenericConfigReader" />
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T> (string key)
    {
      object v;
      if (m_dictionary.TryGetValue (key, out v)) {
        return (T)v;
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"Get: key {key} not in cache");
        }
        v = m_configReader.Get<T> (key);
        // Because of some recursion, not to try to insert twice the same value => use TryAdd
        m_dictionary.TryAdd (key, v);
        return (T)v;
      }
    }
    #endregion

    /// <summary>
    /// Clear the cache
    /// </summary>
    public void Clear ()
    {
      m_dictionary.Clear ();
    }

    /// <summary>
    /// Add a value in cache without adding the value in the config reader
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <param name="overwrite"></param>
    public void Add (string key, object v, bool overwrite = false)
    {
      if (overwrite) {
        m_dictionary[key] = v;
      }
      else {
        var result = m_dictionary.TryAdd (key, v);
        if (log.IsDebugEnabled) {
          if (result) {
            log.Debug ($"Add: {key}={v} was successfully added (overwrite=False)");
          }
          else {
            log.Debug ($"Add: {key} was already set (overwrite=False)");
          }
        }
      }
    }
  }
}
