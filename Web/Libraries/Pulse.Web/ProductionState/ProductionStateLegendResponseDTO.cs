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

namespace Pulse.Web.ProductionState
{
  /// <summary>
  /// Reponse DTO for the ProductionStateLegend service
  /// </summary>
  [Api ("ProductionStateLegend Response DTO")]
  public class ProductionStateLegendResponseDTO
  {
    /// <summary>
    /// List of legend items
    /// </summary>
    public List<ProductionStateLegendItemDTO> Items { get; set; }
  }

  /// <summary>
  /// List item for ProductionStateLegend service
  /// </summary>
  public class ProductionStateLegendItemDTO
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
    public List<ProductionStateDTO> ProductionStates { get; set; }
  }
}
