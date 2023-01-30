// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table Translation
  /// </summary>
  public interface ITranslation: IVersionable
  {
    /// <summary>
    /// Locale
    /// 
    /// Use the empty string for the default translation
    /// </summary>
    string Locale { get; }
    
    /// <summary>
    /// Translation key
    /// </summary>
    string TranslationKey { get; }
    
    /// <summary>
    /// Translation value
    /// </summary>
    string TranslationValue { get; set; }
  }
}
