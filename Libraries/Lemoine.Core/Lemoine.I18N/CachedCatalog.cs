// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.I18N
{
  /// <summary>
  /// Add a cache layer to a catalog.
  /// 
  /// This is thread safe
  /// </summary>
  public class CachedCatalog : ICatalogWithPush
  {
    readonly ILog log = LogManager.GetLogger (typeof (CachedCatalog).FullName);

    #region Members
    readonly ICatalog m_catalog;
    readonly IDictionary<CultureInfo, IDictionary<string, string>> m_cache = new ConcurrentDictionary<CultureInfo, IDictionary<string, string>> ();
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="catalog"></param>
    public CachedCatalog (ICatalog catalog)
    {
      m_catalog = catalog;
    }
    #endregion // Constructors

    #region ICatalog implementation
    /// <summary>
    /// Implementation of <see cref="Lemoine.I18N.ICatalog">IPulseCatalog</see>
    /// </summary>
    /// <param name="key">not null or empty</param>
    /// <param name="cultureInfo"></param>
    /// <returns></returns>
    public string GetString (string key, System.Globalization.CultureInfo cultureInfo)
    {
      Debug.Assert (!string.IsNullOrEmpty (key));
      Debug.Assert (null != cultureInfo);

      string translation = GetTranslation (key, cultureInfo);
      if (null != translation) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetString: {translation} found for {key} and {cultureInfo.Name}");
        }
        return translation;
      }
      else { // null == translation
        if (cultureInfo.Equals (System.Globalization.CultureInfo.InvariantCulture)) {
          log.Warn ($"GetString: key {key} not found in any of the parent culture");
          return null;
        }
        else { // Not InvariantCulture: search recursively to its parent
          translation = GetString (key, cultureInfo.Parent);
          return translation;
        }
      }
    }

    /// <summary>
    /// Implementation of <see cref="Lemoine.I18N.ICatalog">IPulseCatalog</see>
    /// </summary>
    /// <param name="key">not null or empty</param>
    /// <param name="cultureInfo">not null</param>
    /// <returns></returns>
    public string GetTranslation (string key, System.Globalization.CultureInfo cultureInfo)
    {
      Debug.Assert (!string.IsNullOrEmpty (key));
      Debug.Assert (null != cultureInfo);

      try {
        string translation;
        IDictionary<string, string> translations;
        if (m_cache.TryGetValue (cultureInfo, out translations)) {
          if (translations.TryGetValue (key, out translation)) {
            return translation;
          }
          Debug.Assert (null != translations);
        }
        else {
          translations = new ConcurrentDictionary<string, string> ();
          m_cache[cultureInfo] = translations;
        }

        translation = m_catalog
          .GetTranslation (key, cultureInfo)
          ?.Trim ();
        translations[key] = translation;
        return translation;
      }
      catch (Exception ex) {
        log.Error ($"GetTranslation: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Push a translation for a given culture into the cache
    /// </summary>
    /// <param name="key">not null or empty</param>
    /// <param name="translation">not null</param>
    /// <param name="cultureInfo">not null</param>
    /// <param name="overwrite">overwrite the value if the key already exists</param>
    /// <returns>A new data was effectively inserted</returns>
    public bool PushTranslation (string key, string translation, CultureInfo cultureInfo, bool overwrite)
    {
      Debug.Assert (!string.IsNullOrEmpty (key));
      Debug.Assert (null != translation);
      Debug.Assert (null != cultureInfo);

      try {
        IDictionary<string, string> translations;
        if (m_cache.ContainsKey (cultureInfo)) {
          translations = m_cache[cultureInfo];
          string existingTranslation;
          if (!overwrite) {
            if (translations.TryGetValue (key, out existingTranslation)
              && (null != existingTranslation)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"PushTranslation: the key {key} is already in cache");
              }
              return false;
            }
          }
          Debug.Assert (null != translations);
        }
        else {
          translations = new ConcurrentDictionary<string, string> ();
          m_cache[cultureInfo] = translations;
        }

        translations[key] = translation.Trim ();
        return true;
      }
      catch (Exception ex) {
        log.Error ($"PushTranslation: exception", ex);
        throw;
      }
    }
    #endregion

    /// <summary>
    /// Return all the keys in catalog
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetKeys ()
    {
      return m_cache.Values
        .SelectMany (x => x.Keys)
        .Distinct ();
    }
  }
}
