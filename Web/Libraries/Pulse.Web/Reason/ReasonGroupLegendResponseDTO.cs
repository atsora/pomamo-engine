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

namespace Pulse.Web.Reason
{
  /// <summary>
  /// Reponse DTO for the ReasonGroupLegend service
  /// </summary>
  [Api("ReasonGroupLegend Response DTO")]
  public class ReasonGroupLegendResponseDTO
  {
    /// <summary>
    /// List of legend items
    /// </summary>
    public List<ReasonGroupLegendItemDTO> Items { get; set; }
  }
  
  /// <summary>
  /// List item for ReasonGroupLegend service
  /// </summary>
  public class ReasonGroupLegendItemDTO
  {
    /// <summary>
    /// Color for a set of reason groups
    /// </summary>
    public string Color { get; set; }
    
    /// <summary>
    /// Display
    /// </summary>
    public string Display { get; set; }
    
    /// <summary>
    /// Legends
    /// </summary>
    public List<ReasonGroupDTO> ReasonGroups { get; set; }
  }
}

