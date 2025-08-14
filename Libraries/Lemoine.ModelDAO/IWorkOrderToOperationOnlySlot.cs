// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Virtual model of table OperationSlot without the day and shift
  /// 
  /// Analysis table where are stored all
  /// the Operation periods of a given machine.
  /// </summary>
  public interface IWorkOrderToOperationOnlySlot
    : IWithRange
    , IPartitionedByMachine
    , IDisplayable
  {
    /// <summary>
    /// Reference to the task or null if unknown / not applicable
    /// </summary>
    IManufacturingOrder ManufacturingOrder { get; }
    
    /// <summary>
    /// Reference to the work order
    /// </summary>
    IWorkOrder WorkOrder { get; }
    
    /// <summary>
    /// Reference to the component
    /// </summary>
    IComponent Component { get; }
    
    /// <summary>
    /// Reference to the Operation
    /// </summary>
    IOperation Operation { get; }
    
    /// <summary>
    /// Run time of this operation slot
    /// </summary>
    TimeSpan? RunTime { get; }
    
    /// <summary>
    /// Number of run full cycles (from begin to end) during the slot
    /// 
    /// Use the setter very carefully, because it does not update the number of intermediate work pieces
    /// Use the IncrementTotalCycles and SetTotalCycles instead to update the number of intermediate work pieces too
    /// </summary>
    int TotalCycles { get; }
    
    /// <summary>
    /// Number of full cycles for which there is an adjusted number of intermediate work pieces.
    /// 
    /// Use the setter very carefully because it does not update the number of intermediate work pieces
    /// Use the IncrementTotalCycles and SetTotalCycles methods instead
    /// </summary>
    int AdjustedCycles { get; }
    
    /// <summary>
    /// Adjusted quantity of intermediate work pieces
    /// 
    /// Use the setter very carefully because it does not update the number of intermediate work pieces
    /// Use the IncrementTotalCycles and SetTotalCycles methods instead
    /// </summary>
    int AdjustedQuantity { get; }
    
    /// <summary>
    /// Number of partial cycles (a begin but no end) during the slot
    /// 
    /// The setter is not public any more, because it does not update the number of intermediate work pieces
    /// Use the IncrementPartialCycles and SetPartialCycles instead to update the number of intermediate work pieces too
    /// </summary>
    int PartialCycles { get; }
    
    /// <summary>
    /// Average cycle time of the full cycles during the slot
    /// </summary>
    TimeSpan? AverageCycleTime { get; }
    
    /// <summary>
    /// Production duration
    /// </summary>
    TimeSpan? ProductionDuration { get; }

    /// <summary>
    /// Duration of the slot
    /// </summary>
    TimeSpan? Duration { get; }
    
    /// <summary>
    /// Is the machine slot empty ?
    /// 
    /// If the slot is empty, it will not be inserted in the database.
    /// </summary>
    /// <returns></returns>
    bool IsEmpty ();
    
    /// <summary>
    /// IMergeable implementation
    /// 
    /// Check the reference data of the other item matches
    /// the data of other without considering the date/time
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    bool ReferenceDataEquals (IWorkOrderToOperationOnlySlot other);
  }
}
