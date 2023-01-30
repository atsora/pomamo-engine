// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for ITranslation.
  /// </summary>
  public interface ITranslationDAO: IGenericUpdateDAO<ITranslation, int>
  {
    /// <summary>
    /// Find the ITranslation for the specified locale and translationKey
    /// 
    /// null is returned if the specified pair was not found
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="locale"></param>
    /// <param name="translationKey"></param>
    /// <returns></returns>
    ITranslation Find (string locale, string translationKey);

    /// <summary>
    /// Get the list of distinct translation keys
    /// </summary>
    /// <returns></returns>
    IList<string> GetDistinctTranslationKeys ();
    
    /// <summary>
    /// Get the list of translations with key equal to <paramref name="key"></paramref>
    /// and locale in <paramref name="locales"></paramref>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="locales"></param>
    /// <returns></returns>
    IList<ITranslation> GetTranslationFromKeyAndLocales(string key, List<string> locales);
  }
  
  /// <summary>
  /// Extension methods to ITranslationDAO
  /// </summary>
  public static class ITranslationDAOExtensions
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ITranslationDAOExtensions).FullName);
    
    /// <summary>
    /// Implements <see cref="Lemoine.I18N.ICatalog" />
    /// </summary>
    /// <param name="translationDao"></param>
    /// <param name="key"></param>
    /// <param name="cultureInfo"></param>
    /// <returns></returns>
    public static string GetString(this ITranslationDAO translationDao, string key, System.Globalization.CultureInfo cultureInfo)
    {
      return new TranslationDAOCatalog ().GetString (key, cultureInfo);
    }

    /// <summary>
    /// Implements <see cref="Lemoine.I18N.ICatalog" />
    /// </summary>
    /// <param name="translationDao"></param>
    /// <param name="key">not null</param>
    /// <param name="cultureInfo">not null</param>
    /// <returns></returns>
    public static string GetTranslation(this ITranslationDAO translationDao, string key, System.Globalization.CultureInfo cultureInfo)
    {
      return new TranslationDAOCatalog ().GetTranslation (key, cultureInfo);
    }

    /// <summary>
    /// Implements <see cref="Lemoine.I18N.ICatalogWithPush" />
    /// </summary>
    /// <param name="translationDao"></param>
    /// <param name="key"></param>
    /// <param name="translation"></param>
    /// <param name="cultureInfo"></param>
    /// <param name="overwrite"></param>
    /// <returns></returns>
    public static bool PushTranslation(this ITranslationDAO translationDao, string key, string translation, System.Globalization.CultureInfo cultureInfo, bool overwrite)
    {
      return new TranslationDAOCatalog ().PushTranslation (key, translation, cultureInfo, overwrite);
    }
  }
}
