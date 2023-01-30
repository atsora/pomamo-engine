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

namespace Pulse.Web.Time
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /Time/RangeAround service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/RangeAround/", "GET", Summary = "Get a range around a specified date/time", Notes = "To be used with ?Around=&RangeType=")]
  [Route("/RangeAround/{Around}", "GET", Summary = "Get a range around a specified date/time")]
  [Route("/RangeAround/{Around}/{RangeType}", "GET", Summary = "Get a range around a specified date/time")]
  [Route("/RangeAround/{Around}/{RangeType}/{RangeSize}", "GET", Summary = "Get a range around a specified date/time")]
  [Route("/Time/RangeAround/", "GET", Summary = "Get a range around a specified date/time", Notes = "To be used with ?Around=&RangeType=")]
  [Route("/Time/RangeAround/{Around}", "GET", Summary = "Get a range around a specified date/time")]
  [Route("/Time/RangeAround/{Around}/{RangeType}", "GET", Summary = "Get a range around a specified date/time")]
  [Route("/Time/RangeAround/{Around}/{RangeType}/{RangeSize}", "GET", Summary = "Get a range around a specified date/time")]
  public class RangeAroundRequestDTO: IReturn<RangeAroundResponseDTO>
  {
    /// <summary>
    /// Date time in ISO format
    /// </summary>
    [ApiMember (Name = "Around", Description = "Date/time", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string Around { get; set; }

    /// <summary>
    /// Range Type (string) :
    ///  shift, day(default), week, month, quarter, semester, year
    /// </summary>
    [ApiMember (Name = "Around", Description = "Range type: shift / day / week / month / quarter / semester / year. Default: day", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string RangeType { get; set; }

    /// <summary>
    /// Number of RangeType requested (default 1)
    /// </summary>
    [ApiMember (Name = "RangeSize", Description = "Number of range types that is requested. Default: 1", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int? RangeSize { get; set; }
  }
}
