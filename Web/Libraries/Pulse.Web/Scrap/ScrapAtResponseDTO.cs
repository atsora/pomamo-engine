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
using Lemoine.Extensions.Web.Responses;

namespace Pulse.Web.Scrap
{
  /// <summary>
  /// Response DTO for ScrapAt
  /// </summary>
  [Api ("Scrap/At Response DTO")]
  public class ScrapAtResponseDTO
  {
    /// <summary>
    /// UTC Date/time of the request
    /// </summary>
    public string At { get; set; }

    /// <summary>
    /// UTC Date/time range of the scrap report (restricted to cycles)
    /// </summary>
    public string Range { get; set; }

    /// <summary>
    /// Extended UTC Date/time range of the scrap report (extended to the previous or next report)
    /// </summary>
    public string ExtendedRange { get; set; }

    /// <summary>
    /// Number of cycles in the period
    /// </summary>
    public int NbCycles { get; set; }

    /// <summary>
    /// Number of parts in the period
    /// </summary>
    public int NbParts { get; set; }

    /// <summary>
    /// Full operation period
    /// </summary>
    public ScrapAtOperationDTO OperationPeriod { get; set; }

    /// <summary>
    /// Existing report
    /// </summary>
    public ExistingReportDTO Existing { get; set; }

    public void SetOperationSlot (IOperationSlot operationSlot) {
      this.OperationPeriod = new ScrapAtOperationDTO ();
      this.OperationPeriod.Range = operationSlot.DateTimeRange.ToString (ConvertDTO.DateTimeUtcToIsoString);
      if (operationSlot.Operation is not null) {
        this.OperationPeriod.Operation = new OperationDTOAssembler ().Assemble (operationSlot.Operation);
      }
    }
  }

  /// <summary>
  /// Referenced full operation period
  /// </summary>
  public class ScrapAtOperationDTO
  {
    /// <summary>
    /// UTC date/time range of the full operation period
    /// </summary>
    public string Range { get; set; }

    /// <summary>
    /// Operation
    /// </summary>
    public OperationDTO Operation { get; set; }
  }

  /// <summary>
  /// Existing report
  /// </summary>
  public class ExistingReportDTO
  { 
    /// <summary>
    /// Modification ID of the latest existing report
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// List of reasons
    /// </summary>
    public IList<ScrapReasonDTO> Reasons { get; set; }
  }

  /// <summary>
  /// Scrap reason
  /// </summary>
  public class ScrapReasonDTO
  {
    /// <summary>
    /// Reason
    /// </summary>
    public NonConformanceReasonDTO Reason { get; set; }

    /// <summary>
    /// Quantity
    /// </summary>
    public int Quantity { get; set; }
  }
}
