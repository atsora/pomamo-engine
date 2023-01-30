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

namespace Pulse.Web.Machine
{
  /// <summary>
  /// Response DTO
  /// </summary>
  [Api ("Machine/GroupZoomIn Response DTO")]
  public class GroupZoomInResponseDTO
  {
    /// <summary>
    /// Is the relationship dynamic ?
    /// </summary>
    public bool? Dynamic { get; set; }

    /// <summary>
    /// Group ids of the children
    /// </summary>
    public List<string> Children { get; set; }

    /// <summary>
    /// Details on the children if the request parameter Details is set
    /// </summary>
    public List<GroupDTO> ChildrenDetails { get; set; }
  }
}
