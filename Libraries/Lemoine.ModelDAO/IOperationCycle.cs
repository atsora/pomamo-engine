// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table OperationCycle.
  /// </summary>
  public interface IOperationCycle: IVersionable, IPartitionedByMachine, Lemoine.Threading.IChecked, Lemoine.Threading.ICheckedCaller
  {
    /// <summary>
    /// Begin date/time of the operation cycle
    /// 
    /// null in case the Begin date/time is unknown
    /// </summary>
    DateTime? Begin { get; set; }
    
    /// <summary>
    /// End date/time of the operation cycle
    /// 
    /// null in case the End date/time is unknown
    /// or in case of a partial cycle
    /// 
    /// There is no setter. Use the SetRealEnd and SetEstimatedEnd methods instead
    /// </summary>
    DateTime? End { get; }
    
    /// <summary>
    /// Date/time of the operation cycle to use
    /// to sort the operation cycles
    /// 
    /// This is:
    /// <item>End in case of a full cycle</item>
    /// <item>Begin in case of a partial cycle</item>
    /// </summary>
    DateTime DateTime { get; }

    /// <summary>
    /// Reference to the operation slot
    /// that matches this operation cycle
    /// </summary>
    IOperationSlot OperationSlot { get; set; }
    
    /// <summary>
    /// Offset (in percentage) between standard and actual cycle duration
    /// </summary>
    Double? OffsetDuration { get; }
    
    /// <summary>
    /// Status of the bounds (is begin or end estimated)
    /// </summary>
    OperationCycleStatus Status { get; set; }
    
    /// <summary>
    /// Can this operation cycle be considered as a full cycle,
    /// meaning it made some parts
    /// </summary>
    bool Full { get; set; }

    /// <summary>
    /// Quantity
    /// 
    /// null: consider the default quantity
    /// </summary>
    int? Quantity { get; set; }
    
    /// <summary>
    /// Has the operation cycle a real begin (defined and not estimated) ?
    /// </summary>
    /// <returns></returns>
    bool HasRealBegin ();
    
    /// <summary>
    /// Has the operation cycle a real end (defined and not estimated) ?
    /// </summary>
    /// <returns></returns>
    bool HasRealEnd ();

    /// <summary>
    /// Set the real begin of a cycle
    /// </summary>
    /// <param name="dateTime"></param>
    void SetRealBegin (DateTime dateTime);

    /// <summary>
    /// Set the end of a full cycle
    /// </summary>
    /// <param name="dateTime"></param>
    void SetRealEnd (DateTime dateTime);

    /// <summary>
    /// set end and flag as estimated end
    /// </summary>
    /// <param name="dateTime"></param>
    void SetEstimatedEnd(DateTime? dateTime);
    
    /// <summary>
    /// set begin and flag as estimated begin
    /// </summary>
    /// <param name="dateTime"></param>
    void SetEstimatedBegin(DateTime dateTime);
  }
}