// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.I18N;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.ModelDAO.i18n
{
  /// <summary>
  /// Description of ModelDAOCatalog.
  /// </summary>
  public sealed class ModelDAOCatalog: ICatalog
  {
    readonly ILog log = LogManager.GetLogger (typeof (ModelDAOCatalog).FullName);
    
    #region IPulseCatalog implementation
    /// <summary>
    /// Implementation of <see cref="Lemoine.I18N.ICatalog">IPulseCatalog</see>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cultureInfo"></param>
    /// <returns></returns>
    public string GetString(string key, System.Globalization.CultureInfo cultureInfo)
    {
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
          return key;
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
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        try {
          ITranslation translation = ModelDAOHelper.DAOFactory.TranslationDAO
            .Find (cultureInfo.Name, key);
          if (null == translation) {
            log.DebugFormat ("GetTranslation: " +
                             "no translation found for key {0} culture {1}",
                             key, cultureInfo.Name);
            return null;
          }
          else {
            log.DebugFormat ("GetTranslation: " +
                             "{0} found for key {1} culture {2}",
                             translation.TranslationValue,
                             key, cultureInfo.Name);
            return translation.TranslationValue;
          }
        }
        catch (Exception ex) {
          log.Error ($"GetTranslation: error for key={key} cultureInfo={cultureInfo} => return null", ex);
          return null;
        }
      }
    }
    #endregion
  }
}
