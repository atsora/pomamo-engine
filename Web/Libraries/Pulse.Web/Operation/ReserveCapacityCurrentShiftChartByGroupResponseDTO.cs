// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Extensions.Web.Responses;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Chart data for the ReserveCapacityCurrentShiftChartByGroup service
  /// </summary>
  public class ReserveCapacityChartByGroupDataDto
  { 
    /// <summary>
    /// Group Id
    /// </summary>
    public string GroupId { get; set; }

    /// <summary>
    /// Group display
    /// </summary>
    public string GroupDisplay { get; set; }

    /// <summary>
    /// Reserve Capacity
    /// </summary>
    public double ReserveCapacity { get; set; }

    /// <summary>
    /// Deprecated reserve capacity
    /// 
    /// To remove in version 11
    /// </summary>
    public double ReservedCapacity
    {
      get { return this.ReserveCapacity; }
      set { this.ReserveCapacity = value; }
  }
  }

  /// <summary>
  /// Response DTO
  /// </summary>
  [Api ("ReserveCapacityCurrentShiftChartByGroup Response DTO")]
  public class ReserveCapacityCurrentShiftChartByGroupResponseDTO
  {
    /// <summary>
    /// Date/time of the data (min date/time of all the data by group)
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Day in ISO string if it is the same for all the groups
    /// </summary>
    public string Day { get; set; }

    /// <summary>
    /// Shift if it is the same for all the groups
    /// </summary>
    public ShiftDTO Shift { get; set; }

    /// <summary>
    /// Chart data
    /// </summary>
    public List<ReserveCapacityChartByGroupDataDto> ChartData { get; set; }
  }
}
