// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Reason for scrap
  /// </summary>
  public interface IScrapReasonReport
  {
    /// <summary>
    /// Reference to the machine
    /// </summary>
    IMachine Machine { get; }

    /// <summary>
    /// Scrap report
    /// </summary>
    IScrapReport ScrapReport { get; }

    /// <summary>
    /// Reason
    /// </summary>
    INonConformanceReason NonConformanceReason { get; }

    /// <summary>
    /// Quantity
    /// </summary>
    int Quantity { get; }
  }

  /// <summary>
  /// Model for table scrapreport
  /// 
  /// This table records the scrap
  /// </summary>
  public interface IScrapReport : IMachineModification
  {
    /// <summary>
    /// Date/time range
    /// </summary>
    UtcDateTimeRange DateTimeRange { get; set; }

    /// <summary>
    /// If the option to split the operation slots by day is set,
    /// reference to the day.
    /// 
    /// null if the option to split the operation slot by day is not set
    /// </summary>
    DateTime? Day { get; }

    /// <summary>
    /// If the corresponding option is selected,
    /// reference to the shift.
    /// 
    /// null if there is no shift
    /// or if the option to split the operation slot by shift is not set
    /// </summary>
    IShift Shift { get; }

    /// <summary>
    /// Reference to the operation
    /// </summary>
    IOperation Operation { get; }

    /// <summary>
    /// Reference to the associated component or null if unknown
    /// </summary>
    IComponent Component { get; }

    /// <summary>
    /// Reference to the work order if known.
    /// 
    /// null if the work order could not be identified yet.
    /// </summary>
    IWorkOrder WorkOrder { get; }

    /// <summary>
    /// Reference to a manufacturing order if known
    /// 
    /// nullable
    /// </summary>
    IManufacturingOrder ManufacturingOrder { get; }

    /// <summary>
    /// Associated number of cycles
    /// </summary>
    int NbCycles { get; set; }

    /// <summary>
    /// Associated number of parts
    /// </summary>
    int NbParts { get; set; }

    /// <summary>
    /// Number of valid parts
    /// </summary>
    int NbValid { get; set; }

    /// <summary>
    /// Number of set-up parts
    /// </summary>
    int NbSetup { get; set; }

    /// <summary>
    /// Number of scrap parts
    /// </summary>
    int NbScrap { get; set; }

    /// <summary>
    /// Number of parts that can be fixed
    /// </summary>
    int NbFixable { get; set; }

    /// <summary>
    /// Scrap report details
    /// </summary>
    string Details { get; set; }

    /// <summary>
    /// Report that is updated (null by default)
    /// </summary>
    IScrapReport ReportUpdate { get; set; }

    /// <summary>
    /// Reason reports
    /// </summary>
    ICollection<IScrapReasonReport> Reasons { get; }
  }
}
