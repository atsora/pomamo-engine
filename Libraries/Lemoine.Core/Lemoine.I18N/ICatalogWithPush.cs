// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Globalization;

namespace Lemoine.I18N
{
  /// <summary>
  /// Catalog with a Push method
  /// </summary>
  public interface ICatalogWithPush: ICatalog
  {
    /// <summary>
    /// Push a translation for a given culture into the cache
    /// </summary>
    /// <param name="key">not null or empty</param>
    /// <param name="translation">not null</param>
    /// <param name="cultureInfo">not null</param>
    /// <param name="overwrite">overwrite the value if the key already exists</param>
    /// <returns>A new data was effectively inserted</returns>
    bool PushTranslation (string key, string translation, CultureInfo cultureInfo, bool overwrite);
  }
}
