// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.Collections;
using Lemoine.Core.Log;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// Store a list of strings in a specific file for a specific key,
  /// one string per line
  /// 
  /// The lines that start with # are ommitted
  /// </summary>
  public class ListStringFileConfigReader : IGenericConfigReader, IStringConfigReader
  {
    readonly ILog log = LogManager.GetLogger (typeof (ListStringFileConfigReader).FullName);

    readonly string m_key;
    readonly string m_path;
    readonly IList<string> m_values = new List<string> ();
    bool m_loaded = false;

    /// <summary>
    /// Constructor
    /// </summary>
    public ListStringFileConfigReader (string key, string path)
    {
      Debug.Assert (!string.IsNullOrEmpty (key));
      Debug.Assert (!string.IsNullOrEmpty (path));

      if (string.IsNullOrEmpty (key)) {
        log.Error ("ListStringFileConfigReader: empty key");
        throw new ArgumentException ("Empty key", "key");
      }
      if (string.IsNullOrEmpty (path)) {
        log.Error ("ListStringFileConfigReader: empty path");
        throw new ArgumentException ("Empty path", "path");
      }

      m_key = key;
      m_path = path;

      if (!File.Exists (path)) {
        log.Warn ($"ListStringFileConfigReader: file {path} does not exist");
        return;
      }
    }

    public void Load ()
    {
      if (!m_loaded) {
        if (!File.Exists (m_path)) {
          log.Warn ($"ListStringFileConfigReader: file {m_path} does not exist");
          return;
        }

        lock (m_values) {
          if (!m_loaded) {
            string[] lines = File.ReadAllLines (m_path);
            foreach (string line in lines) {
              if (line.StartsWith ("#", StringComparison.InvariantCultureIgnoreCase)) { // comment
                continue;
              }
              m_values.Add (line.Trim ());
            }

            m_loaded = true;
          }
        }
      }
    }

    /// <summary>
    /// Get the list of strings
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="ConfigKeyNotFoundException"></exception>
    public IEnumerable<string> GetStrings (string key)
    {
      if (!string.Equals (key, m_key, StringComparison.InvariantCultureIgnoreCase)) {
        throw new ConfigKeyNotFoundException (key);
      }

      Load ();
      return m_values;
    }

    /// <summary>
    /// <see cref="IStringConfigReader"/>
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetString (string key)
    {
      var values = GetStrings (key);
      return values.ToListString ();
    }

    /// <summary>
    /// <see cref="IGenericConfigReader"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="ConfigKeyNotFoundException"></exception>
    public T Get<T> (string key)
    {
      if (!string.Equals (key, m_key, StringComparison.InvariantCultureIgnoreCase)) {
        throw new ConfigKeyNotFoundException (key);
      }

      if (typeof (T).IsAssignableFrom (typeof (IEnumerable<string>))) {
        return (T)GetStrings (key);
      }
      else {
        var autoConvertConfigReader = new AutoConvertConfigReader (this);
        return autoConvertConfigReader.Get<T> (key);
      }
    }
  }
}
