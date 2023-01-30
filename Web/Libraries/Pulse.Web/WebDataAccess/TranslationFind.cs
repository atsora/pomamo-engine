// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Core.Log;

using System.Net;


namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for Translation/Find service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/Translation/Find/", "GET", Summary = "Deprecated TranslationDAO.Find", Notes = "To use with ?Locale=&Key=")]
  [Route("/Translation/Find/{Locale}/{Key}", "GET", Summary = "Deprecated TranslationDAO.Find", Notes = "")]
  [Route("/Data/Translation/Find/", "GET", Summary = "TranslationDAO.Find", Notes = "To use with ?Locale=&Key=")]
  [Route("/Data/Translation/Find/{Locale}/{Key}", "GET", Summary = "TranslationDAO.Find", Notes = "")]
  [AllowAnonymous]
  public class TranslationFind : IReturn<TranslationDTO>
  {
    /// <summary>
    /// Locale of requested Translation
    /// </summary>
    [ApiMember(Name = "Locale", Description = "Locale", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string Locale { get; set; }

    /// <summary>
    /// Key of requested Translation
    /// </summary>
    [ApiMember(Name = "Key", Description = "Translation key", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string Key { get; set; }
  }
}
