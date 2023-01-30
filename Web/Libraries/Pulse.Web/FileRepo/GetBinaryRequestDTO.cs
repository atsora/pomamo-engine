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
  [Api("Request DTO for /FileRepo/GetBinary service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/FileRepo/GetBinary/Get/{NSpace}/{Path}", "GET", Summary = "Server part of FileRepoClientWeb", Notes = "")]
  [Route("/FileRepo/GetBinary", "GET", Summary = "Server part of FileRepoClientWeb", Notes = "to use with ?NSpace=&Path=")]
  [AllowAnonymous]
  public class GetBinaryRequestDTO: IReturn<ICollection<byte[]>>
  {
    /// <summary>
    /// File repository namespace
    /// </summary>
    public string NSpace { get; set; }
    
    /// <summary>
    /// File repository path
    /// </summary>
    public string Path { get; set; }
  }
}

