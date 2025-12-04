// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Net;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /Operation/PartProductionRange service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Operation/PartProductionRange/", "GET", Summary = "", Notes = "To use with ?GroupId=&Range=")]
  [Route ("/PartProductionRange/", "GET", Summary = "", Notes = "To use with ?GroupId=&Range=")]
  public class PartProductionRangeRequestDTO : IReturn<PartProductionRangeResponseDTO>
  {
    /// <summary>
    /// Group Id
    /// </summary>
    [ApiMember (Name = "GroupId", Description = "Group Id", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string GroupId { get; set; }

    /// <summary>
    /// Date/time range
    /// </summary>
    [ApiMember (Name = "Range", Description = "Requested range (mandatory)", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string Range { get; set; }
  }
}
