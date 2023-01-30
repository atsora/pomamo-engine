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

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /Operation/ReserveCapacityCurrentShiftByGroup service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Operation/ReserveCapacityCurrentShiftChartByGroup/", "GET", Summary = "", Notes = "To use with ?ParentGroupId= or ?GroupIds=")]
  [Route ("/Operation/ReservedCapacityCurrentShiftChartByGroup/", "GET", Summary = "", Notes = "To use with ?ParentGroupId= or ?GroupIds=")]
  public class ReserveCapacityCurrentShiftChartByGroupRequestDTO : IReturn<ReserveCapacityCurrentShiftChartByGroupResponseDTO>
  {
    /// <summary>
    /// Parent group Id
    /// </summary>
    [ApiMember (Name = "ParentGroupId", Description = "Parent group Id", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string ParentGroupId { get; set; }

    /// <summary>
    /// Ids of the groups
    /// </summary>
    [ApiMember (Name = "GroupIds", Description = "Group Ids, comma (',') separated list", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string GroupIds { get; set; }
  }
}
