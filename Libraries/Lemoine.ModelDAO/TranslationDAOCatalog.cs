// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Description of TranslationDAOCatalog.
  /// </summary>
  public class TranslationDAOCatalog: Lemoine.I18N.ICatalogWithPush
  {
    static readonly ILog log = LogManager.GetLogger(typeof (TranslationDAOCatalog).FullName);

    #region ICatalogWithPush implementation
    /// <summary>
    /// Implements <see cref="Lemoine.I18N.ICatalog" />
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
          return null;
        }
        else { // Not InvariantCulture: search recursively to its parent
          return GetString (key, cultureInfo.Parent);
        }
      }
    }

    /// <summary>
    /// Implements <see cref="Lemoine.I18N.ICatalog" />
    /// </summary>
    /// <param name="key">not null</param>
    /// <param name="cultureInfo">not null</param>
    /// <returns></returns>
    public string GetTranslation(string key, System.Globalization.CultureInfo cultureInfo)
    {
      Debug.Assert (!string.IsNullOrEmpty (key));
      Debug.Assert (null != cultureInfo);
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("GetTranslation"))
        {
          ITranslation translation = ModelDAOHelper.DAOFactory.TranslationDAO
            .Find (cultureInfo.Name, key);
          if (null == translation) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetTranslation: no translation found for key {key} locale {cultureInfo.Name}");
            }
            return null;
          }
          else {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetTranslation: translation {translation.TranslationValue} found for key {key} locale {cultureInfo.Name}");
            }
            return translation.TranslationValue;
          }
        }
      }
    }

    /// <summary>
    /// Implements <see cref="Lemoine.I18N.ICatalogWithPush" />
    /// </summary>
    /// <param name="key"></param>
    /// <param name="translation"></param>
    /// <param name="cultureInfo"></param>
    /// <param name="overwrite"></param>
    /// <returns></returns>
    public bool PushTranslation(string key, string translation, System.Globalization.CultureInfo cultureInfo, bool overwrite)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("GetTranslation"))
        {
          ITranslation existing = ModelDAOHelper.DAOFactory.TranslationDAO
            .Find (cultureInfo.Name, key);
          if (null != existing) {
            if (overwrite) {
              if (log.IsDebugEnabled) {
                log.Debug ($"PushTranslation: write {key}={translation} for locale={cultureInfo.Name}");
              }
              existing.TranslationValue = translation;
              ModelDAOHelper.DAOFactory.TranslationDAO.MakePersistent (existing);
              transaction.Commit ();
              return true;
            }
            else {
              transaction.Commit ();
              return false;
            }
          }
          else { // null == existing
            if (log.IsDebugEnabled) {
              log.Debug ($"PushTranslation: write {key}={translation} for locale={cultureInfo.Name}");
            }
            ITranslation newTranslation = ModelDAOHelper.ModelFactory.CreateTranslation (cultureInfo.Name, key);
            newTranslation.TranslationValue = translation;
            ModelDAOHelper.DAOFactory.TranslationDAO.MakePersistent (newTranslation);
            transaction.Commit ();
            return true;
          }
        }
      }
    }
    #endregion ICatalogWithPush implementation
  }
}
