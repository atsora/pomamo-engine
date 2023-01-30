// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// DTO for TranslationDTO.
  /// </summary>
  public class TranslationDTO
  {
    /// <summary>
    /// Locale
    /// </summary>
    public string Locale { get; set; }
    
    /// <summary>
    /// Key
    /// </summary>
    public string TranslationKey { get; set; }

    /// <summary>
    /// Value
    /// </summary>
    public string TranslationValue { get; set; }
  }
}
