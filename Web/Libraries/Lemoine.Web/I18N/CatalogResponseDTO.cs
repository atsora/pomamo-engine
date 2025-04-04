// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;

namespace Lemoine.Web.I18N
{
  /// <summary>
  /// Response DTO for I18N/Catalog
  /// </summary>
  [Api ("Catalog Response DTO")]
  public class CatalogResponseDTO
  {
    /// <summary>
    /// Config value
    /// </summary>
    public string Value { get; set; }
  }
}
