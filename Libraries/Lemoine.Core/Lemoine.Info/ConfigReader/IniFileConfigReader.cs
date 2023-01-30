// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Lemoine.Core.Log;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// StringConfigReader using a file with the syntax of a .ini file
  /// 
  /// Thread safe
  /// </summary>
  public class IniFileStringConfigReader : IStringConfigReader
  {
    static readonly ILog log = LogManager.GetLogger (typeof (IniFileStringConfigReader).FullName);

    readonly IDictionary<string, string> m_keyValue = new ConcurrentDictionary<string, string> ();
    readonly string m_prefix;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="prefix">[Optional] prefix to add to the config key</param>
    internal protected IniFileStringConfigReader (string filePath, string prefix = null)
    {
      m_prefix = prefix;

      Load (filePath);
    }

    void Load (string filePath)
    {
      if (!File.Exists (filePath)) {
        log.Warn ($"Load: file {filePath} does not exist");
        return;
      }
      string[] lines = File.ReadAllLines (filePath);
      var section = "";
      foreach (string line in lines) {
        if (line.StartsWith ("#", StringComparison.InvariantCultureIgnoreCase)) { // comment
          continue;
        }
        else if (line.StartsWith ("[")) {
          section = line.Substring (1).Trim ().TrimEnd (']');
          if (log.IsDebugEnabled) {
            log.Debug ($"Load: new section {section}");
          }
        }
        else {
          string[] keyValue = line.Split (new char[] { '=' }, 2);
          if ((null != keyValue) && (2 == keyValue.Length)) {
            log.Info ($"IniFile: got {keyValue[0]}={keyValue[1]} for section {section}");
            var keyPrefix = string.IsNullOrEmpty (m_prefix) ? "" : $"{m_prefix}.";
            var keySection = string.IsNullOrEmpty (section) ? "" : $"{section}.";
            var key = $"{keyPrefix}{keySection}{keyValue[0].Trim ()}";
            m_keyValue[key] = keyValue[1].Trim ();
          }
        }
      }
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
        return m_keyValue[key];
      }
      catch (KeyNotFoundException ex) {
        throw new ConfigKeyNotFoundException (key, ex);
      }
    }
    #endregion
  }

  /// <summary>
  /// Config reader for an ini file
  /// 
  /// Thread safe
  /// </summary>
  public class IniFileConfigReader
    : AutoConvertConfigReader
    , IGenericConfigReader
  {
    static readonly ILog log = LogManager.GetLogger (typeof (IniFileConfigReader).FullName);

    #region Constructors
    /// <summary>
    /// Create a config reader from a file extension
    /// </summary>
    /// <param name="fileExtension"></param>
    /// <param name="prefix">[Optional] prefix to add to the config key</param>
    /// <returns></returns>
    public static IGenericConfigReader CreateFromExtension (string fileExtension = ".ini", string prefix = null)
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
      return new IniFileConfigReader (path, prefix);
    }

    /// <summary>
    /// Create a config reader from a path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="prefix">[Optional] prefix to add to the config key</param>
    /// <returns></returns>
    public static IGenericConfigReader CreateFromPath (string path, string prefix = null)
    {
      return new IniFileConfigReader (path, prefix);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="prefix">[Optional] prefix to add to the config key</param>
    IniFileConfigReader (string filePath, string prefix = null)
      : base (new IniFileStringConfigReader (filePath, prefix))
    {
    }
    #endregion // Constructors
  }
}
