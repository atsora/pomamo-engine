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
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.FileRepo
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /FileRepo/GetString service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/FileRepo/GetString/Get/{NSpace}/{Path}", "GET", Summary = "Server part of FileRepoClientWeb", Notes = "")]
  [Route("/FileRepo/GetString", "GET", Summary = "Server part of FileRepoClientWeb", Notes = "to use with ?NSpace=&Path=")]
  [AllowAnonymous]
  public class GetStringRequestDTO: IReturn<ICollection<string>>
  {
    /// <summary>
    /// File repository namespace
    /// </summary>
    public string NSpace { get; set; }
    
    /// <summary>
    /// File repository path
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Is the requested data optional ? If optional is set to true and the data not found, then an empty string is returned instead
    /// </summary>
    public bool Optional { get; set; } = false;
  }
}

