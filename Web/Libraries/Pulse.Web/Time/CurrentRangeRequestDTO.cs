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
  [Api ("Request DTO for /Time/CurrentRange service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Time/CurrentRange/", "GET", Summary = "Get the current range on the server", Notes = "To be used with ?RangeDuration=")]
  [Route ("/Time/CurrentRange/{RangeDuration}", "GET", Summary = "Get the current range on the server", Notes = "")]
  [Route ("/Time/CurrentRange/Get/{RangeDuration}", "GET", Summary = "Get the current range on the server", Notes = "")]
  [AllowAnonymous]
  public class CurrentRangeRequestDTO : IReturn<CurrentRangeResponseDTO>
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
