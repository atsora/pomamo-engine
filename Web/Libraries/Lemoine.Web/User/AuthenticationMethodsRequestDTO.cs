// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Net;

#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;

namespace Lemoine.Web.User
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /User/AuthenticationMethods/ service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/User/AuthenticationMethods/", "GET", Summary = "", Notes = "")]
  [AllowAnonymous]
  public class AuthenticationMethodsRequestDTO : IReturn<AuthenticationMethodsResponseDTO>
  {
    /// <summary>
    /// Return only the authentication methods that are supposed to work on this application
    /// </summary>
    public string ApplicationName { get; set; } = "";
  }
}
