// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Interface for all the models having a translation key column.
  /// </summary>
  public interface IDataWithTranslation
  {
    /// <summary>
    /// Name
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Translation key
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    string TranslationKey { get; set; }
    
    /// <summary>
    /// Translated name of the object (if applicable, else the name of the object)
    /// </summary>
    string NameOrTranslation { get; }
  }
}
