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
  [Api ("Request DTO for /Operation/ReserveCapacityCurrentShift service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Operation/ReserveCapacityCurrentShift/", "GET", Summary = "", Notes = "To use with ?GroupId=")]
  [Route ("/Operation/ReservedCapacityCurrentShift/", "GET", Summary = "", Notes = "To use with ?GroupId=")]
  public class ReserveCapacityCurrentShiftRequestDTO : IReturn<ReserveCapacityCurrentShiftResponseDTO>
  {
    /// <summary>
    /// Group Id
    /// </summary>
    [ApiMember (Name = "GroupId", Description = "Group Id", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string GroupId { get; set; }
  }
}
