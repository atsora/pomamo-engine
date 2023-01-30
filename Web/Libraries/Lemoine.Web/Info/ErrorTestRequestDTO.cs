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

namespace Lemoine.Web.Info
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /ErrorTest service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/ErrorTest/", "GET", Summary = "", Notes = "To use with ?ErrorMessage=&Status=")]
  [Route ("/Info/ErrorTest/", "GET", Summary = "", Notes = "To use with ?ErrorMessage=&Status=")]
  [Route ("/Info/ErrorTest/Get/{ErrorMessage}/{Status}", "GET", Summary = "", Notes = "")]
  public class ErrorTestRequestDTO : IReturn<ErrorTestResponseDTO>
  {
    /// <summary>
    /// Error message of the Error DTO
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Status of the error DTO
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Details of the erorr DTO
    /// </summary>
    public string Details { get; set; }
  }
}
