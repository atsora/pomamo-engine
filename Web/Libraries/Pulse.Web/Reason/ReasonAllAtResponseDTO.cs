// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Model;
using Pulse.Business.Reason;

namespace Pulse.Web.Reason
{
  /// <summary>
  /// Response DTO for ReasonAllAt
  /// </summary>
  [Api ("Reason/AllAt Response DTO")]
  public class ReasonAllAtResponseDTO
  {
    /// <summary>
    /// Reference to the possible reasons, ordered by descending score
    /// </summary>
    public List<ReasonAllAtItemDTO> ReasonAllAtItems { get; set; }

    /// <summary>
    /// UTC Date/time of the request
    /// </summary>
    public string At { get; set; }
  }

  /// <summary>
  ///  Individual reason that is active at the specified date/time
  /// </summary>
  public class ReasonAllAtItemDTO
  {
    /// <summary>
    /// Constructor with a reason
    /// </summary>
    /// <param name="reason"></param>
    public ReasonAllAtItemDTO (IReason reason)
    {
      this.Id = reason.Id;
      this.Display = reason.Display;
      this.Color = reason.Color;
    }

    /// <summary>
    /// Constructor with a possible reason
    /// </summary>
    /// <param name="possibleReason"></param>
    public ReasonAllAtItemDTO (IPossibleReason possibleReason)
      : this (possibleReason.Reason)
    {
      this.Score = possibleReason.ReasonScore;
      this.Source = new ReasonSourceDTO (possibleReason.ReasonSource);
      this.Details = possibleReason.ReasonDetails;
      this.OverwriteRequired = possibleReason.OverwriteRequired;
      if (!ReasonData.IsJsonNullOrEmpty (possibleReason.JsonData)) {
        this.Display = ReasonData.OverwriteDisplay (this.Display, possibleReason.JsonData, false);
      }
    }

    /// <summary>
    /// Reason id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Reason display
    /// </summary>
    public string Display { get; set; }

    /// <summary>
    /// Reason color
    /// </summary>
    public string Color { get; set; }
    
    /// <summary>
    /// Reason score
    /// 
    /// Not available if the SelectableOption is on
    /// </summary>
    public double? Score { get; set; }

    /// <summary>
    /// Reason source
    /// </summary>
    public ReasonSourceDTO Source { get; set; }

    /// <summary>
    /// Reason details
    /// </summary>
    public string Details { get; set; }

    /// <summary>
    /// Overwrite required property
    /// </summary>
    public bool OverwriteRequired { get; set; }
  }
}
