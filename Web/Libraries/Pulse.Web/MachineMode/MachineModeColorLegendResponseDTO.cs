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

namespace Pulse.Web.MachineMode
{
  /// <summary>
  /// Reponse DTO for the MachineModeColorLegend service
  /// </summary>
  [Api("MachineModeColorLegend Response DTO")]
  public class MachineModeColorLegendResponseDTO
  {
    /// <summary>
    /// List of legend items
    /// </summary>
    public List<MachineModeColorLegendItemDTO> Items { get; set; }
  }
  
  /// <summary>
  /// List item for MachineModeColorLegend service
  /// </summary>
  public class MachineModeColorLegendItemDTO
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
    public List<MachineModeDTO> MachineModes { get; set; }
  }
}

