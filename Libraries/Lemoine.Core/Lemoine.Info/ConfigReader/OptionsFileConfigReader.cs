// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// StringConfigReader using a file with the syntax of the .exe.options files
  /// 
  /// Thread safe
  /// </summary>
  public class OptionsFileStringConfigReader : IStringConfigReader
  {
    readonly string m_path;
    readonly IDictionary<string, string> m_keyValue = new ConcurrentDictionary<string, string> ();

    static readonly ILog log = LogManager.GetLogger (typeof (OptionsFileStringConfigReader).FullName);

    /// <summary>
    /// Associated file path
    /// </summary>
    public string Path => m_path;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="filePath"></param>
    internal protected OptionsFileStringConfigReader (string filePath)
    {
      m_path = filePath;
      Load ();
    }

    public void Reload ()
    {
      m_keyValue.Clear ();
      Load ();
    }

    /// <summary>
    /// Load the data
    /// </summary>
    void Load ()
    {
      if (!File.Exists (m_path)) {
        log.Warn ($"OptionsFile: file {m_path} does not exist");
        return;
      }
      string[] lines = File.ReadAllLines (m_path);
      foreach (string line in lines) {
        if (line.StartsWith ("#", StringComparison.InvariantCultureIgnoreCase)) { // comment
          continue;
        }
        string[] keyValue = line.Split (new char[] { ':', '=' }, 2);
        if ((null != keyValue) && (2 == keyValue.Length)) {
          log.Info ($"Load: got {keyValue[0]}={keyValue[1]}");
          m_keyValue[keyValue[0].Trim ()] = keyValue[1].Trim ();
        }
      }
    }

    #region IStringConfigReader implementation
    /// <summary>
    /// <see cref="IStringConfigReader"/>
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetString (string key)
    {
      if (string.IsNullOrEmpty (key)) {
        throw new ArgumentException ("Key is not expected to be null or empty", key);
      }

      try {
        return m_keyValue[key];
      }
      catch (KeyNotFoundException ex) {
        throw new ConfigKeyNotFoundException (key, ex);
      }
    }
    #endregion
  }

  /// <summary>
  /// Config reader for an option file
  /// 
  /// Thread safe
  /// </summary>
  public class OptionsFileConfigReader
    : AutoConvertConfigReader
    , IGenericConfigReader
    , IPersistentConfigWriter
  {
    static readonly ILog log = LogManager.GetLogger (typeof (OptionsFileConfigReader).FullName);

    readonly OptionsFileStringConfigReader m_stringConfigReader;

    #region Constructors
    /// <summary>
    /// Create a config reader from a file extension
    /// </summary>
    /// <param name="fileExtension"></param>
    /// <returns></returns>
    public static IGenericConfigReader CreateFromExtension (string fileExtension = ".options")
    {
      string path = ProgramInfo.AbsolutePath;
      if (null == path) {
        log.Fatal ("CreateFromExtension: the absolute path is not known => return a dummy config reader");
        return new DummyConfigReader ();
      }
      path += fileExtension;
      if (log.IsDebugEnabled) {
        log.Debug ($"CreateFromExtension: path={path}");
      }
      return new OptionsFileConfigReader (path);
    }

    /// <summary>
    /// Create a config reader from a path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static OptionsFileConfigReader CreateFromPath (string path)
    {
      return new OptionsFileConfigReader (path);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="path"></param>
    OptionsFileConfigReader (string path)
      : this (new OptionsFileStringConfigReader (path))
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="path"></param>
    OptionsFileConfigReader (OptionsFileStringConfigReader stringConfigReader)
      : base (stringConfigReader)
    {
      m_stringConfigReader = stringConfigReader;
    }
    #endregion // Constructors

    #region IPersistentConfigWriter implementation
    /// <summary>
    /// <see cref="IPersistentConfigWriter"/>
    /// </summary>
    /// <param name="key"></param>
    public void ResetPersistentConfig (string key)
    {
      var path = m_stringConfigReader.Path;
      if (!File.Exists (path)) {
        log.Warn ($"ResetPersistentConfig: file {path} does not exist");
        return;
      }

      try {
        GetString (key);
      }
      catch (ConfigKeyNotFoundException) {
        if (log.IsDebugEnabled) {
          log.Debug ($"ResetPersistentConfig: {key} is already not in file");
        }
        return;
      }

      RemoveKey (key);
      m_stringConfigReader.Reload ();
    }

    /// <summary>
    /// <see cref="IPersistentConfigWriter"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <param name="overwrite"></param>
    /// <returns></returns>
    public bool SetPersistentConfig<T> (string key, T v, bool overwrite)
    {
      var path = m_stringConfigReader.Path;
      if (!File.Exists (path)) {
        log.Warn ($"SetPersistentConfig: file {path} does not exist => create it");
        File.WriteAllText (path, "");
      }

      T existing;
      try {
        existing = Get<T> (key);
      }
      catch (ConfigKeyNotFoundException) {
        if (log.IsDebugEnabled) {
          log.Debug ($"SetPersistentConfig: {key} is not in file");
        }
        Add (key, v);
        return true;
      }

      if (object.Equals (existing, v)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"SetPersistentConfig: value {v} already in file, keep it");
        }
        return true;
      }

      if (overwrite) {
        RemoveKey (key);
        Add (key, v);
        return true;
      }
      else {
        return false;
      }
    }

    void Add<T> (string key, T v)
    {
      var path = m_stringConfigReader.Path;
      string vstring;
      if (v is double vdouble) {
        vstring = vdouble.ToString (System.Globalization.CultureInfo.InvariantCulture);
      }
      else {
        vstring = v.ToString ();
      }
      File.AppendAllLines (path, new string[] { $"{key}:{vstring}" });
      m_stringConfigReader.Reload ();
    }

    void RemoveKey (string key)
    {
      var path = m_stringConfigReader.Path;
      var allLines = File.ReadAllLines (path);
      var keptLines = allLines.Where (x => !IsMatch (x, key));
      File.Delete (path);
      File.WriteAllLines (path, keptLines);
    }

    bool IsMatch (string line, string key)
    {
      var trimmedLine = line.TrimStart ();
      if (trimmedLine.StartsWith (key)) {
        var remaining = trimmedLine.Substring (key.Length).TrimStart ();
        return remaining.StartsWith (":") || remaining.StartsWith ("=");
      }
      else {
        return false;
      }
    }
    #endregion // IPersistentConfigWriter implementation

  }
}
