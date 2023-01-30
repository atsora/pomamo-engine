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
  /// Config reader with default values
  /// 
  /// Thread safe
  /// </summary>
  public class MemoryConfigReader
    : IGenericConfigReader
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MemoryConfigReader).FullName);

    readonly ConcurrentDictionary<string, object> m_dictionary = new ConcurrentDictionary<string, object> (StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// Constructor
    /// </summary>
    public MemoryConfigReader ()
    {
    }

    /// <summary>
    /// Add a default value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    public void Add (string key, object v)
    {
      Add (key, v, false);
    }

    /// <summary>
    /// Add a default value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <param name="overrideDefault"></param>
    public void Add (string key, object v, bool overrideDefault)
    {
      if (overrideDefault) {
        m_dictionary[key] = v;
      }
      else {
        var result = m_dictionary.TryAdd (key, v);
        if (log.IsDebugEnabled) {
          if (result) {
            log.Debug ($"Add: {key}={v} was successfully added (overrideDefault=False)");
          }
          else {
            log.Debug ($"Add: {key} was already set (overrideDefault=False)");
          }
        }
      }
    }

    /// <summary>
    /// Clear the values
    /// </summary>
    public void Clear ()
    {
      m_dictionary.Clear ();
    }

    /// <summary>
    /// Remove a specific key
    /// </summary>
    /// <param name="key"></param>
    public void Remove (string key)
    {
      if (!m_dictionary.TryRemove (key, out _)) {
        log.Debug ($"Remove: {key} was not removed");
      }
    }

    #region IGenericConfigReader implementation
    /// <summary>
    /// <see cref="IGenericConfigReader" />
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T> (string key)
    {
      if (string.IsNullOrEmpty (key)) {
        throw new ArgumentException ("Key can't be empty or null", "key");
      }

      try {
        return (T)m_dictionary[key];
      }
      catch (KeyNotFoundException ex) {
        throw new ConfigKeyNotFoundException (key, ex);
      }
    }
    #endregion
  }
}
