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

namespace Pulse.Web.ProductionState
{
  /// <summary>
  /// Request DTO for the ProductionRate service
  /// 
  /// Return the production rate for the specified day range or date/time range
  /// </summary>
  [Api ("Request DTO for /ProductionRate service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/ProductionRate/", "GET", Summary = "Get the production rate in a specified range", Notes = "To use with ?GroupId=&DayRange= or ?GroupId=&Range=")]
  public class ProductionRateRequestDTO : IReturn<ProductionRateResponseDTO>
  {
    /// <summary>
    /// Id of the group
    /// </summary>
    [ApiMember (Name = "GroupId", Description = "Group Id", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string GroupId { get; set; }

    /// <summary>
    /// Day range if no Range is set
    /// 
    /// Default: "" that corresponds to [today, today] in case no Range is set
    /// </summary>
    [ApiMember (Name = "DayRange", Description = "Requested day range. Default is today.", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string DayRange { get; set; }

    /// <summary>
    /// Range
    /// 
    /// It takes the priority on DayRange
    /// 
    /// Default: use DayRange instead
    /// </summary>
    [ApiMember (Name = "Range", Description = "Requested range. Default is use DayRange instead.", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string Range { get; set; }
  }
}
