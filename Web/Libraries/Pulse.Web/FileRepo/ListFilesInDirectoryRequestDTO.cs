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
  [Api("Request DTO for /FileRepo/ListFilesInDirectory service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/FileRepo/ListFilesInDirectory/Get/{NSpace}", "GET", Summary = "Server part of FileRepoClientWeb", Notes = "")]
  [Route("/FileRepo/ListFilesInDirectory", "GET", Summary = "Server part of FileRepoClientWeb", Notes = "to use with ?NSpace=")]
  [AllowAnonymous]
  public class ListFilesInDirectoryRequestDTO: IReturn<ICollection<string>>
  {
    /// <summary>
    /// File repository namespace
    /// </summary>
    public string NSpace { get; set; }
    
    /// <summary>
    /// File repository path (not used here)
    /// </summary>
    public string Path { get; set; }
  }
}

