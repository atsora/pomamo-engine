// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Net;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.ProductionTracker
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /ProductionTracker service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/ProductionTracker", "GET", Summary = "Return the data for the production tracker web page", Notes = "To use with ?GroupId=&Range=")]
  public class ProductionTrackerRequestDTO : IReturn<ProductionTrackerResponseDTO>
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

    /// <summary>
    /// Should the global target for the requested range be included
    /// 
    /// Only applicable if a same operation is used on the whole requested period
    /// </summary>
    [ApiMember (Name = "GlobalTarget", Description = "Add the global target for the requested range. Default is false", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool GlobalTarget { get; set; } = false;

    /// <summary>
    /// Should the production capacity be added ?
    /// 
    /// Only applicable if a same operation is used on the whole requested period
    /// </summary>
    [ApiMember (Name = "ProductionCapacity", Description = "Add the production capacity. Default is false", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool ProductionCapacity { get; set; } = false;
  }
}
