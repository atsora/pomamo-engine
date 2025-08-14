// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of the
  /// analysis table operationslot
  /// that keeps a track of all the Operation Slot periods
  /// </summary>
  public interface IOperationSlot: IDisplayable, ISlot, IComparable<IOperationSlot>, IPartitionedByMachine, Lemoine.Threading.IChecked, Lemoine.Threading.ICheckedCaller
  {
    /// <summary>
    /// Reference to the operation
    /// </summary>
    IOperation Operation { get; }
    
    /// <summary>
    /// Reference to the associated component or null if unknown
    /// </summary>
    IComponent Component { get; set; }
    
    /// <summary>
    /// Reference to the work order if known.
    /// 
    /// null if the work order could not be identified yet.
    /// </summary>
    IWorkOrder WorkOrder { get; set; }
    
    /// <summary>
    /// Reference to a line if known
    /// 
    /// null if the line was not identified
    /// </summary>
    ILine Line { get; set; }

    /// <summary>
    /// Reference to a manufacturing order if known
    /// 
    /// nullable
    /// </summary>
    IManufacturingOrder ManufacturingOrder { get; set; }
    
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
    /// Run time of this operation slot
    /// </summary>
    TimeSpan? RunTime { get; set; }
    
    /// <summary>
    /// Number of run full cycles (from begin to end) during the slot
    /// </summary>
    int TotalCycles { get; }
    
    /// <summary>
    /// Number of full cycles for which there is an adjusted number of intermediate work pieces.
    /// </summary>
    int AdjustedCycles { get; }
    
    /// <summary>
    /// Adjusted quantity of intermediate work pieces
    /// </summary>
    int AdjustedQuantity { get; }
    
    /// <summary>
    /// Number of partial cycles (a begin but no end) during the slot
    /// </summary>
    int PartialCycles { get; }
    
    /// <summary>
    /// Average cycle time of the full cycles during the slot
    /// </summary>
    TimeSpan? AverageCycleTime { get; set; }

    /// <summary>
    /// Was the manufacturing order determined automatically
    /// </summary>
    bool? AutoManufacturingOrder { get; set; }
    
    /// <summary>
    /// Production duration
    /// </summary>
    TimeSpan? ProductionDuration { get; }
    
    /// <summary>
    /// Consolidate the run time
    /// </summary>
    void ConsolidateRunTime ();
  }
}
