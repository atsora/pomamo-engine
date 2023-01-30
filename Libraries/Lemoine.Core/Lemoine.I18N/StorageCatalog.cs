// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Core.Log;

namespace Lemoine.I18N
{
  /// <summary>
  /// Description of MultiCatalogWithStorage.
  /// </summary>
  public class StorageCatalog : ICatalogWithPush
  {
    #region Members
    readonly ICatalog m_catalog;
    readonly ICatalogWithPush m_storage;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (StorageCatalog).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="catalog">not null</param>
    /// <param name="storage">not null</param>
    public StorageCatalog (ICatalog catalog, ICatalogWithPush storage)
    {
      Debug.Assert (null != catalog);
      Debug.Assert (null != storage);

      m_catalog = catalog;
      m_storage = storage;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Implementation of <see cref="Lemoine.I18N.ICatalog">IPulseCatalog</see>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cultureInfo"></param>
    /// <returns></returns>
    public string GetString (string key, System.Globalization.CultureInfo cultureInfo)
    {
      string translation = GetTranslation (key, cultureInfo);
      if (null == translation) {
        if (cultureInfo.Equals (System.Globalization.CultureInfo.InvariantCulture)) {
          log.WarnFormat ("GetString: " +
                          "key {0} not found in any of the parent culture",
                          key);
          return null;
        }
        else { // Not InvariantCulture: search recursively to its parent
          translation = GetString (key, cultureInfo.Parent);
        }
      }

      if (null != translation) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("GetString: " +
                           "{0} found for {1} and {2}",
                           translation, key, cultureInfo.Name);
        }
      }
      return translation;
    }

    /// <summary>
    /// Implementation of <see cref="Lemoine.I18N.ICatalog">IPulseCatalog</see>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cultureInfo"></param>
    /// <returns></returns>
    public string GetTranslation (string key, System.Globalization.CultureInfo cultureInfo)
    {
      string translation = m_storage.GetTranslation (key, cultureInfo);
      if (null != translation) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("GetTranslation: " +
                           "{0} found for {1} and {2} from storage",
                           translation, key, cultureInfo.Name);
        }
        return translation;
      }

      translation = m_catalog.GetTranslation (key, cultureInfo);
      if (null != translation) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("GetTranslation: " +
                           "{0} found for {1} and {2}",
                           translation, key, cultureInfo.Name);
        }
        m_storage.PushTranslation (key, translation, cultureInfo, false);
      }
      return translation;
    }
    #endregion // Methods

    /// <summary>
    /// Implementation of <see cref="Lemoine.I18N.ICatalogWithPush">IPulseCatalogWithPush</see>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="translation"></param>
    /// <param name="cultureInfo"></param>
    /// <param name="overwrite"></param>
    /// <returns></returns>
    public bool PushTranslation (string key, string translation, System.Globalization.CultureInfo cultureInfo, bool overwrite)
    {
      return m_storage.PushTranslation (key, translation, cultureInfo, overwrite);
    }
  }
}
