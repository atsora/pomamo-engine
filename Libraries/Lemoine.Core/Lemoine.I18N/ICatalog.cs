// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Globalization;

namespace Lemoine.I18N
{
  /// <summary>
  /// Description of IPulseCatalog.
  /// </summary>
  public interface ICatalog
  {
    /// <summary>
    /// Get the translation value of the specified key and language
    /// 
    /// If a translation is found for the specific cultureInfo, 
    /// the translation of the parent cultureInfo may be returned instead.
    /// 
    /// If no translation is found for cultureInfo or any of its parent, 
    /// null is returned instead.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cultureInfo"></param>
    /// <returns></returns>
    string GetString (string key, CultureInfo cultureInfo);
    
    /// <summary>
    /// Get the translation value for the specific key and exact cultureInfo.
    /// 
    /// If no translation is found for the specific key and cultureInfo,
    /// null is returned
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cultureInfo"></param>
    /// <returns></returns>
    string GetTranslation (string key, CultureInfo cultureInfo);
  }

  /// <summary>
  /// Extensions to interface <see cref="ICatalog"/>
  /// </summary>
  public static class CatalogExtensions
  {
    /// <summary>
    /// Set this catalog as the default catalog <see cref="PulseCatalog"/>
    /// </summary>
    /// <param name="catalog"></param>
    public static void SetAsPulseCatalog (this ICatalog catalog)
    {
      PulseCatalog.Implementation = catalog;
    }
  }
}
