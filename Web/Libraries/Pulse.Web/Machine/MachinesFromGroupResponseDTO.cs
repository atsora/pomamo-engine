// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Pulse.Web.CommonResponseDTO;
using Lemoine.Model;
using Pulse.Extensions.Web.Responses;

namespace Pulse.Web.Machine
{
  /// <summary>
  /// Response DTO for Machine/FromGroups service
  /// </summary>
  [Api ("Machine/FromGroups Response DTO")]
  public class MachinesFromGroupsResponseDTO
  {
    /// <summary>
    /// Reference to the associated machine Ids
    /// </summary>
    public List<int> MachineIds { get; set; }

    /// <summary>
    /// Machine details
    /// </summary>
    public List<MachineDTO> MachineDetails { get; set; }

    /// <summary>
    /// One of the specified group is dynamic
    /// </summary>
    public bool Dynamic { get; set; }

    /// <summary>
    /// Sort kind when different groups are combined:
    /// <item>if the machines are not sorted in a particular way, return 0</item>
    /// <item>if the sort importance is minor, return 1. In that case, the machines can be re-organized</item>
    /// <item>if the group was designed to return sorted machines, return a number greater than 2</item>
    /// <item>in case of groups with incompatible sort criteria, do not return a sort kind</item>
    /// 
    /// Prevents the machines from being re-organized manually if the sort priority is greater than 2 or if null
    /// </summary>
    public int? SortKind { get; set; }

    /// <summary>
    /// Sort kind tip:
    /// <item>incompatible</item>
    /// <item>sorted</item>
    /// <item>minor</item>
    /// <item>unsorted</item>
    /// </summary>
    public string SortKindTip { get; set; }
  }
}
