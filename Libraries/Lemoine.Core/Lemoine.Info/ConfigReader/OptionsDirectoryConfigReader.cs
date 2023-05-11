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
  /// Config reader that reads all the .options configuration files in a directory
  /// 
  /// Thread safe
  /// </summary>
  public class OptionsDirectoryConfigReader
    : IGenericConfigReader
    , IPersistentConfigWriter
  {
    static readonly ILog log = LogManager.GetLogger (typeof (OptionsFileConfigReader).FullName);

    readonly string m_directoryPath;
    readonly string m_searchPattern;
    MultiConfigReader m_multiConfigReader = new MultiConfigReader ();
    MemoryConfigReader m_persistentConfigReader = null;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="path"></param>
    /// <param name="searchPattern"></param>
    public OptionsDirectoryConfigReader (string path, string searchPattern = "*.options")
    {
      m_directoryPath = path;
      m_searchPattern = searchPattern;

      Load ();
    }

    void Load ()
    {
      if (!Directory.Exists (m_directoryPath)) {
        log.Warn ($"DirectoryConfigReader: {m_directoryPath} does not exist");
        return;
      }
      var files = Directory.GetFiles (m_directoryPath, m_searchPattern);
      foreach (var file in files) {
        var filePath = Path.Combine (m_directoryPath, file);
        m_multiConfigReader.Add (OptionsFileConfigReader.CreateFromPath (filePath));
      }
    }

    /// <summary>
    /// <see cref="IGenericConfigReader" />
    /// </summary>
    /// <param name="key"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Get<T> (string key)
    {
      return m_multiConfigReader.Get<T> (key);
    }

    /// <summary>
    /// Reload the files
    /// </summary>
    public void Reload ()
    {
      m_multiConfigReader.Clear ();
      m_persistentConfigReader = null;
      Load ();
    }

    #region IPersistentConfigWriter implementation
    /// <summary>
    /// <see cref="IPersistentConfigWriter"/>
    /// </summary>
    /// <param name="key"></param>
    public void ResetPersistentConfig (string key)
    {
      var filePath = GetPersistentConfigPath (key);
      if (File.Exists (filePath)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"ResetPersistentConfig: key={key} path={filePath} => remove this file");
        }
        File.Delete (filePath);
        Reload ();
      }
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
      if (!Directory.Exists (m_directoryPath)) {
        log.Warn ($"SetPersistentConfig: {m_directoryPath} does not exist => create it");
        try {
          Directory.CreateDirectory (m_directoryPath);
        }
        catch (Exception ex) {
          log.Error ($"SetPersistentConfig: CreateDirectory {m_directoryPath} failed", ex);
          throw;
        }
      }

      var filePath = GetPersistentConfigPath (key);
      if (File.Exists (filePath)) {
        log.Warn ($"SetPersistentConfig: {filePath} already exists");
        if (!overwrite) {
          if (log.IsDebugEnabled) {
            log.Debug ($"SetPersistentConfig: {filePath} already exists and overwrite is {overwrite} => return false");
          }
          return false;
        }
      }

      try {
        string vstring;
        if (v is double vdouble) {
          vstring = vdouble.ToString (System.Globalization.CultureInfo.InvariantCulture);
        }
        else {
          vstring = v.ToString ();
        }
        var content = $"{key}:{vstring}";
        File.WriteAllText (filePath, content);
        if (m_persistentConfigReader is null) {
          m_persistentConfigReader = new MemoryConfigReader ();
        }
        m_multiConfigReader.Add (m_persistentConfigReader);
        m_persistentConfigReader.Add (key, v);
        return true;
      }
      catch (Exception ex) {
        log.Error ("SetPersistentConfigReader: exception", ex);
        throw;
      }
    }

    string GetPersistentConfigPath (string key)
    {
      var fileName = key + ".options";
      var filePath = Path.Combine (m_directoryPath, fileName);
      return filePath;
    }
    #endregion // IPersistentConfigWriter implementation
  }
}
