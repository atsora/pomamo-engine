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
  /// Response DTO for Machine/Groups service
  /// </summary>
  [Api ("Machine/Groups Response DTO")]
  public class GroupsResponseDTO
  {
    /// <summary>
    /// Group categories
    /// </summary>
    public List<GroupCategoryDTO> GroupCategories { get; set; }

    /// <summary>
    /// Complete machine list (id, display)
    /// Not always set
    /// </summary>
    public List<MachineDTO> MachineList { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public GroupsResponseDTO ()
    {
      this.GroupCategories = new List<GroupCategoryDTO> ();
    }
  }
}
