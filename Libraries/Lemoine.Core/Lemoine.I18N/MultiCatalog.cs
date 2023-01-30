// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Core.Log;
using System.Diagnostics;

namespace Lemoine.I18N
{
  /// <summary>
  /// Description of MultiCatalog.
  /// </summary>
  public class MultiCatalog: ICatalog
  {
    #region Members
    readonly IList<ICatalog> m_catalogs = new List<ICatalog> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MultiCatalog).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MultiCatalog ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a catalog
    /// </summary>
    /// <param name="catalog"></param>
    /// <returns>Reference to the multi-catalog</returns>
    public ICatalog Add (ICatalog catalog)
    {
      m_catalogs.Add (catalog);
      return this;
    }
    
    /// <summary>
    /// Implementation of <see cref="Lemoine.I18N.ICatalog">IPulseCatalog</see>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cultureInfo"></param>
    /// <returns></returns>
    public string GetString(string key, System.Globalization.CultureInfo cultureInfo)
    {
      Debug.Assert (null != cultureInfo);

      string translation = GetTranslation (key, cultureInfo);
      if (null != translation) {
        log.DebugFormat ("GetString: " +
                         "{0} found for {1} and {2}",
                         translation, key, cultureInfo.Name);
        return translation;
      }
      else { // null == translation
        if (cultureInfo.Equals (System.Globalization.CultureInfo.InvariantCulture)) {
          log.WarnFormat ("GetString: " +
                          "key {0} not found in any of the parent culture",
                          key);
          return null;
        }
        else { // Not InvariantCulture: search recursively to its parent
          return GetString (key, cultureInfo.Parent);
        }
      }
    }
    
    /// <summary>
    /// Implementation of <see cref="Lemoine.I18N.ICatalog">IPulseCatalog</see>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cultureInfo"></param>
    /// <returns></returns>
    public string GetTranslation(string key, System.Globalization.CultureInfo cultureInfo)
    {
      foreach (ICatalog catalog in m_catalogs) {
        string translation = catalog.GetTranslation (key, cultureInfo);
        if (null != translation) {
          return translation;
        }
      }
      return null;
    }
    #endregion // Methods
  }
}
