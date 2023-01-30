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

namespace Pulse.Web.CncValue
{
  /// <summary>
  /// Reponse DTO for CncValueLegend service
  /// </summary>
  [Api("CncValueLegend Response DTO")]
  public class CncValueLegendResponseDTO
  {
    /// <summary>
    /// Response DTO items
    /// </summary>
    public List<CncValueLegendItemDTO> Items { get; set; }
  }
  
  /// <summary>
  /// List item for the CncValueLegend service
  /// </summary>
  public class CncValueLegendItemDTO
  {
    /// <summary>
    /// Field DTO
    /// </summary>
    public FieldDTO Field { get; set; }
    
    /// <summary>
    /// Legends
    /// </summary>
    public List<LegendDTO> Legends { get; set; }
  }
  
  /// <summary>
  /// DTO for the legend text / color
  /// </summary>
  public class LegendDTO
  {
    /// <summary>
    /// Legend color
    /// </summary>
    public string Color { get; set; }

    /// <summary>
    /// Legend text
    /// </summary>
    public string Display { get; set; }    
  }
}

