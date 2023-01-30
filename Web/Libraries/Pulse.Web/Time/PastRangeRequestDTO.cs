// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Net;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT

namespace Pulse.Web.Time
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /Time/PastRange service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Time/PastRange/", "GET", Summary = "Get the past range (excluding today) on the server", Notes = "To be used with ?RangeDuration=")]
  [Route ("/Time/PastRange/{RangeDuration}", "GET", Summary = "Get the past range (excluding today) on the server", Notes = "")]
  [Route ("/Time/PastRange/Get/{RangeDuration}", "GET", Summary = "Get the past range (excluding today) on the server", Notes = "")]
  [AllowAnonymous]
  public class PastRangeRequestDTO : IReturn<PastRangeResponseDTO>
  {
    /// <summary>
    /// Range duration: x_(hour(s)|shift(s)|day(s)|week(s)|month(s)|quarter(s)|semester(s)|year(s))
    /// </summary>
    [ApiMember (Name = "RangeDuration", Description = "Range description", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string RangeDuration { get; set; }

    /// <summary>
    /// Current date time to consider for computing the range
    /// </summary>
    [ApiMember (Name = "CurrentDate", Description = "Date of reference", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string CurrentDate { get; set; }
  }
}
