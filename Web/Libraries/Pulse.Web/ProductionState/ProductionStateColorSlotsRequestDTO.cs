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

namespace Pulse.Web.ProductionState
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /ProductionState/ColorSlots service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/ProductionState/ColorSlots/", "GET", Summary = "Get the production state color slots in a specified range", Notes = "To use with ?GroupId=&Range=")]
  public class ProductionStateColorSlotsRequestDTO : IReturn<ProductionStateColorSlotsResponseDTO>
  {
    /// <summary>
    /// Group Id
    /// </summary>
    [ApiMember (Name = "GroupId", Description = "Group Id", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string GroupId { get; set; }

    /// <summary>
    /// Date/time range
    /// 
    /// Default: "" that corresponds to Today
    /// </summary>
    [ApiMember (Name = "Range", Description = "Requested range. Default is today. CurrentShift is an accepted parameter", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string Range { get; set; }

    /// <summary>
    /// Skip the details in the answer when the data is not split by day
    /// 
    /// Default: false (return them)
    /// </summary>
    [ApiMember (Name = "SkipDetails", Description = "Skip the details in the answer when the data is not split by day. Default: false", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool SkipDetails { get; set; }

    /// <summary>
    /// Skip the details in the answer when the data is by day
    /// 
    /// Default: false (return them)
    /// </summary>
    [ApiMember (Name = "SkipDetailsByDay", Description = "Skip the details in the answer when the data is split by day. Default: false", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool SkipDetailsByDay { get; set; }

    /// <summary>
    /// Maximum number of days (inclusive) before data is split by day
    /// 
    /// Default: 7
    /// </summary>
    [ApiMember (Name = "MaxDaysNoSplitByDay", Description = "Maximum number of days (inclusive) before data is split by day. Default: 7", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int? MaxDaysNoSplitByDay { get; set; }

    /// <summary>
    /// Split the data by day
    /// 
    /// Default: consider the parameter MaxDaysNoSplitByDay
    /// </summary>
    [ApiMember (Name = "SplitByDay", Description = "Split the data by day. Default: consider the parameter MaxDaysNoSplitByDay", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool? SplitByDay { get; set; }

    /// <summary>
    /// Work order Id
    /// 
    /// Limit the periods to periods that match this work order id
    /// </summary>
    [ApiMember (Name = "WorkOrderId", Description = "Work order ID", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int WorkOrderId { get; set; }

    /// <summary>
    /// Component Id
    /// 
    /// Limit the periods to periods that match this component id
    /// </summary>
    [ApiMember (Name = "ComponentId", Description = "Component ID", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int ComponentId { get; set; }

    /// <summary>
    /// Total duration divisor for the vertical split
    /// 
    /// Default: 640
    /// which corresponds on a FullHD to about 3px
    /// and for a full day to 2,5 minutes
    /// </summary>
    [ApiMember (Name = "SplitDivisor", Description = "Total duration divisor for the vertical split. Default: 640", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int SplitDivisor { get; set; }
  }
}
