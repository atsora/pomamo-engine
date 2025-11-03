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
    /// Current data
    /// </summary>
    public bool Current { get; set; }

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
    public int FullCycleCount { get; set; }

    /// <summary>
    /// Number of parts in the period
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Number of parts that were validated
    /// </summary>
    public int ValidCount { get; set; }

    /// <summary>
    /// Number of parts that are made of the set-up
    /// </summary>
    public int SetupCount { get; set; }

    /// <summary>
    /// Number of scrap parts
    /// </summary>
    public int ScrapCount { get; set; }

    /// <summary>
    /// Number of parts that can be fixed
    /// </summary>
    public int FixableCount { get; set; }

    /// <summary>
    /// Number of parts that were not classified yet
    /// </summary>
    public int UnclassifiedCount { get; set; }

    /// <summary>
    /// Full operation period
    /// </summary>
    public ScrapAtOperationSlotDTO OperationSlot { get; set; }

    /// <summary>
    /// Existing report
    /// </summary>
    public ScrapReportDTO ScrapReport { get; set; }

    public void SetOperationSlot (IOperationSlot operationSlot, bool current)
    {
      this.OperationSlot = new ScrapAtOperationSlotDTO ();
      this.OperationSlot.Current = current;
      this.OperationSlot.Display = operationSlot.Display;
      this.OperationSlot.Range = operationSlot.DateTimeRange.ToString (ConvertDTO.DateTimeUtcToIsoString);
      if (operationSlot.Operation is not null) {
        this.OperationSlot.Operation = new OperationDTOAssembler ().Assemble (operationSlot.Operation);
      }
    }
  }

  /// <summary>
  /// Referenced full operation period
  /// </summary>
  public class ScrapAtOperationSlotDTO
  {
    /// <summary>
    /// Does this operation correspond to a current period
    /// </summary>
    public bool Current { get; set; }

    /// <summary>
    /// UTC date/time range of the full operation period
    /// </summary>
    public string Range { get; set; }

    /// <summary>
    /// Display of the operation slot
    /// </summary>
    public string Display { get; set; }

    /// <summary>
    /// Operation
    /// </summary>
    public OperationDTO Operation { get; set; }
  }

  /// <summary>
  /// Existing report
  /// </summary>
  public class ScrapReportDTO
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

    /// <summary>
    /// Default constructor 
    /// </summary>
    public ScrapReasonDTO () { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="scrapReasonReport"></param>
    public ScrapReasonDTO (IScrapReasonReport scrapReasonReport)
    {
      this.Quantity = scrapReasonReport.Quantity;
      this.Reason = new NonConformanceReasonDTOAssembler ().Assemble (scrapReasonReport.NonConformanceReason);
    }
  }
}
