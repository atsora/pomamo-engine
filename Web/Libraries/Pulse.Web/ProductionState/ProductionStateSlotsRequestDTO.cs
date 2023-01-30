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
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.ProductionState
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /ProductionStateSlots service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/ProductionStateSlots/", "GET", Summary = "Get the production state slots in a specified range", Notes = "To use with ?GroupId=&Range=")]
  [Route ("/ProductionStateSlots/{GroupId}/{Range}", "GET", Summary = "Get the production state slots in a specified range", Notes = "")]
  [Route ("/ProductionState/Slots/", "GET", Summary = "Get the production state slots in a specified range", Notes = "To use with ?GroupId=&Range=")]
  [Route ("/ProductionState/Slots/{GroupId}/{Range}", "GET", Summary = "Get the production state slots in a specified range", Notes = "")]
  public class ProductionStateSlotsRequestDTO : IReturn<ProductionStateSlotsResponseDTO>
  {
    /// <summary>
    /// Id of the machine
    /// </summary>
    [ApiMember (Name = "GroupId", Description = "Group Id", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string GroupId { get; set; }

    /// <summary>
    /// Date/time range
    /// 
    /// Default: "" that would correspond to [now, now]
    /// </summary>
    [ApiMember (Name = "Range", Description = "Requested range. Default is [now, now]", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string Range { get; set; }
  }
}
