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

namespace Lemoine.Web.Config
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /Config/FromKey service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/Config/FromKey/{Key}", "GET", Summary = "Get the config of a specific key", Notes = "")]
  [Route("/Config/FromKey/Get/{Key}", "GET", Summary = "Get the config of a specific key", Notes = "")]
  [Route("/Config/FromKey/", "GET", Summary = "Get the config of a specific key", Notes = "To be used with ?Key=")]
  public class ConfigFromKeyRequestDTO: IReturn<ConfigFromKeyResponseDTO>
  {
    /// <summary>
    /// Config key
    /// </summary>
    public string Key { get; set; }
  }
}
