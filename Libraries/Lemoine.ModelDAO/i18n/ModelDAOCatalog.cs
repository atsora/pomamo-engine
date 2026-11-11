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
        if (log.IsDebugEnabled) {
          log.Debug ($"GetString: {translation} found for {key} and {cultureInfo.Name}");
        }
        return translation;
      }
      else { // null == translation
        if (cultureInfo.Equals (System.Globalization.CultureInfo.InvariantCulture)) {
          log.Warn ($"GetString: key {key} not found in any of the parent culture");
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
            if (log.IsDebugEnabled) {
              log.Debug ($"GetTranslation: no translation found for key {key} culture {cultureInfo.Name}");
            }
            return null;
          }
          else {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetTranslation: {translation.TranslationValue} found for key {key} culture {cultureInfo.Name}");
            }
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
