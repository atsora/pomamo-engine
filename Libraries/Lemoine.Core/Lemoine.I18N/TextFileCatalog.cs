// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.I18N
{
  /// <summary>
  /// ICatalog using text files
  /// </summary>
  public class TextFileCatalog: ICatalogWithPush
  {
    #region Members
    char[] m_separators = new char[] { ':', '=' };
    string m_localePrefix = ".";
    string m_extension = ".txt";
    readonly string m_baseFileName;
    readonly string m_directory;
    IEnumerable<string> m_alternativeDirectories;
    
    bool m_initialized = false;
    readonly CachedCatalog m_cache = new CachedCatalog (new DummyCatalog ());
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (TextFileCatalog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Accepted separators between the key and the value
    /// 
    /// Default separators are ':' and '='
    /// </summary>
    public char[] Separators {
      get { return m_separators; }
      set { m_separators = value; }
    }
    
    /// <summary>
    /// Locale prefix
    /// 
    /// String that is inserted between the baseFileName and the locale
    /// 
    /// Default: "."
    /// </summary>
    public string LocalePrefix {
      get { return m_localePrefix; }
      set { m_localePrefix = value; }
    }
    
    /// <summary>
    /// Extension of the files
    /// 
    /// Default: .txt
    /// </summary>
    public string Extension {
      get { return m_extension; }
      set { m_extension = value; }
    }
    
    /// <summary>
    /// Base file name
    /// </summary>
    public string BaseFileName {
      get { return m_baseFileName; }
    }
    
    /// <summary>
    /// Main directory path
    /// </summary>
    public string Directory {
      get { return m_directory; }
    }
    
    /// <summary>
    /// Alternative directories
    /// </summary>
    public IEnumerable<string> AlternativeDirectories {
      get { return m_alternativeDirectories; }
      set { m_alternativeDirectories = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="baseFileName"></param>
    /// <param name="directory"></param>
    public TextFileCatalog (string baseFileName, string directory)
    {
      m_baseFileName = baseFileName;
      m_directory = directory;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region ICatalogWithPush implementation
    void Initialize ()
    {
      if (m_initialized) {
        return;
      }
      
      LoadFromDirectory (m_directory);
      if (null != m_alternativeDirectories) {
        foreach (var alternativeDirectory in m_alternativeDirectories) {
          LoadFromDirectory (alternativeDirectory);
        }
      }
      m_initialized = true;
    }
    
    /// <summary>
    /// Load the data from a directory
    /// </summary>
    /// <param name="directory"></param>
    void LoadFromDirectory (string directory)
    {
      // Visit all the files in this directory
      var files = System.IO.Directory.GetFiles (directory);
      foreach (var path in files) {
        Debug.Assert (null != path);
        if (!string.IsNullOrEmpty (m_extension)
            && !string.Equals (m_extension, Path.GetExtension (path), StringComparison.InvariantCultureIgnoreCase)) {
          continue; // Not a valid extension
        }
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension (path);
        if (!fileNameWithoutExtension.StartsWith (m_baseFileName, StringComparison.InvariantCultureIgnoreCase)) {
          continue; // Does not start with the right baseFileName
        }
        if (string.Equals (fileNameWithoutExtension, m_baseFileName, StringComparison.InvariantCultureIgnoreCase)) {
          LoadFromFile (path, CultureInfo.InvariantCulture);
        }
        else {
          string toRemove = m_baseFileName + m_localePrefix;
          if (!fileNameWithoutExtension.StartsWith (toRemove, StringComparison.InvariantCultureIgnoreCase)) {
            log.ErrorFormat ("LoadFromDirectory: " +
                             "invalid locale prefix in {0}",
                             path);
          }
          else {
            string locale = fileNameWithoutExtension.Substring (toRemove.Length);
            CultureInfo cultureInfo = null;
            if (locale.Equals ("default", StringComparison.InvariantCultureIgnoreCase)) {
              cultureInfo = CultureInfo.InvariantCulture;
            }
            else {
              try {
                cultureInfo = CultureInfo.CreateSpecificCulture (locale);
              }
              catch (Exception) {
                log.ErrorFormat ("LoadFromDirectory: " +
                                 "invalid locale in {0}",
                                 path);
              }
            }
            if (null != cultureInfo) {
              LoadFromFile (path, cultureInfo);
            }
          }
        }
      }
    }
    
    void LoadFromFile (string filePath, CultureInfo cultureInfo)
    {
      var lines = File.ReadAllLines (filePath);
      foreach (var line in lines) {
        string[] keyTranslation = line.Split (m_separators, 2);
        if (keyTranslation.Length < 2) {
          log.ErrorFormat ("LoadFromFile: " +
                           "line {0} does not contain any separator",
                           line);
        }
        else {
          m_cache.PushTranslation (keyTranslation[0], keyTranslation[1], cultureInfo, false);
        }
      }
    }
    
    /// <summary>
    /// Implementation of <see cref="Lemoine.I18N.ICatalog">IPulseCatalog</see>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cultureInfo"></param>
    /// <returns></returns>
    public string GetString(string key, System.Globalization.CultureInfo cultureInfo)
    {
      Initialize ();
      
      return m_cache.GetString (key, cultureInfo);
    }

    /// <summary>
    /// Implementation of <see cref="Lemoine.I18N.ICatalog">IPulseCatalog</see>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cultureInfo"></param>
    /// <returns></returns>
    public string GetTranslation(string key, System.Globalization.CultureInfo cultureInfo)
    {
      Initialize ();
      
      return m_cache.GetTranslation (key, cultureInfo);
    }

    /// <summary>
    /// Implementation of <see cref="Lemoine.I18N.ICatalogWithPush">IPulseCatalogWithPush</see>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="translation"></param>
    /// <param name="cultureInfo"></param>
    /// <param name="overwrite">not considered (always false)</param>
    /// <returns></returns>
    public bool PushTranslation(string key, string translation, System.Globalization.CultureInfo cultureInfo, bool overwrite)
    {
      if (null != GetTranslation (key, cultureInfo)) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("PushTranslation: key already exists");
        }
        return false;
      }

      string fileName;
      if (cultureInfo.Equals (CultureInfo.InvariantCulture)) {
        fileName = m_baseFileName + m_extension;
      }
      else {
        fileName = m_baseFileName + m_localePrefix + cultureInfo.Name + m_extension;
      }
      string path = Path.Combine (m_directory,
                                  fileName);
      string line = key + m_separators.First () + translation;
      
      using (var writer = File.AppendText (path))
      {
        writer.WriteLine (line);
      }
      m_cache.PushTranslation (key, translation, cultureInfo, overwrite);
      return true;
    }

    /// <summary>
    /// Get the keys in catalog
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetKeys ()
    {
      Initialize ();

      return m_cache.GetKeys ();
    }
    #endregion
  }
}
