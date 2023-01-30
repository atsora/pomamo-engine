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

namespace Lemoine.Web.Info
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /Test or /Info/Test service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/Test", "GET", Summary = "Test the web service", Notes = "")]
  [Route("/Info/Test", "GET", Summary = "Test the web service", Notes = "")]
  [AllowAnonymous]
  public class TestRequestDTO: IReturn<TestResponseDTO>
  {
  }

  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /Test or /Info/Test service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/TestAuthorize", "GET", Summary = "Test the web service", Notes = "")]
  [Route ("/Info/TestAuthorize", "GET", Summary = "Test the web service", Notes = "")]
  [Authorize]
  public class TestAuthorizeRequestDTO : IReturn<TestResponseDTO>
  {
  }
}

