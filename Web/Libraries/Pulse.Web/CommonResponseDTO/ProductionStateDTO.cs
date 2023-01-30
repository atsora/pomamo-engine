// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// DTO for ProductionStateDTO.
  /// </summary>
  public class ProductionStateDTO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="productionState">not null</param>
    public ProductionStateDTO (IProductionState productionState)
    {
      Debug.Assert (null != productionState);

      this.Id = productionState.Id;
      this.Display = productionState.Display;
      this.LongDisplay = productionState.LongDisplay;
      this.Description = productionState.Description;
      this.Color = productionState.Color;
    }

    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display
    /// </summary>
    public string Display { get; set; }

    /// <summary>
    /// Long display: reason group with its description
    /// </summary>
    public string LongDisplay { get; set; }

    /// <summary>
    /// Description of the reason group
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Color
    /// </summary>
    public string Color { get; set; }
  }
}
