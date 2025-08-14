// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Collections;
using Lemoine.Model;
using System;

namespace Lemoine.Plugin.HourlyOperationSummary
{
  /// <summary>
  /// Model for the table hourlyiwpbymachinesummary
  /// 
  /// This contains various information on an intermediate work piece by machine
  /// </summary>
  public interface IHourlyOperationSummary
    : IDataWithId, IDataWithVersion, IPartitionedByMachine
  {
    /// <summary>
    /// Reference to the Operation
    /// 
    /// Nullable
    /// </summary>
    IOperation Operation { get; }

    /// <summary>
    /// Reference to the associated Component
    /// 
    /// Nullable
    /// </summary>
    IComponent Component { get; }

    /// <summary>
    /// Reference to the Work Order if known
    /// 
    /// Nullable
    /// </summary>
    IWorkOrder WorkOrder { get; }

    /// <summary>
    /// Reference to the Line if known
    /// 
    /// Set to null if it could not be identified yet or it if is not applicable
    /// </summary>
    ILine Line { get; }

    /// <summary>
    /// Reference to the Task if known
    /// 
    /// Set to null if it could not be identified yet or it if is not applicable
    /// </summary>
    IManufacturingOrder ManufacturingOrder { get; }

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
    /// Local date + hour
    /// </summary>
    DateTime LocalDateHour { get; }

    /// <summary>
    /// Duration for this specific data
    /// </summary>
    TimeSpan Duration { get; set; }

    /// <summary>
    /// Total number of work pieces as detected from the cycle detection
    /// </summary>
    int TotalCycles { get; set; }

    /// <summary>
    /// Total number of adjusted cycles
    /// </summary>
    int AdjustedCycles { get; set; }

    /// <summary>
    /// Adjusted quantity
    /// </summary>
    int AdjustedQuantity { get; set; }

    /// <summary>
    /// Is the data empty ? It means may it be deleted because all the data are null ?
    /// </summary>
    /// <returns></returns>
    bool IsEmpty ();
  }
}
