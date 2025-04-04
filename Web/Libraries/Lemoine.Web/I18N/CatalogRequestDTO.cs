// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Net;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Core.Log;

namespace Lemoine.Web.I18N
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /I18N/Catalog service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/I18N/Catalog/{Key}/{Default}", "GET", Summary = "Get the translation of a specific key", Notes = "")]
  [Route ("/I18N/Catalog/Get/{Key}/{Default}", "GET", Summary = "Get the translation of a specific key", Notes = "")]
  [Route ("/I18N/Catalog", "GET", Summary = "Get the translation of a specific key", Notes = "")]
  [AllowAnonymous]
  public class CatalogRequestDTO : IReturn<CatalogResponseDTO>
  {
    /// <summary>
    /// Config key
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Default value
    /// </summary>
    public string Default { get; set; }
  }
}
