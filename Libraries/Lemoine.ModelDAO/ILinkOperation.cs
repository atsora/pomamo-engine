// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Set of link directions
  /// </summary>
  public enum LinkDirection
  {
    /// <summary>
    /// Left direction: -1 in database
    /// </summary>
    Left = -1,
    /// <summary>
    /// No link direction: 0 in database
    /// </summary>
    None = 0,
    /// <summary>
    /// Right direction: +1 in database
    /// </summary>
    Right = 1
  }
  
  /// <summary>
  /// Model for table LinkOperation
  /// 
  /// This new table stores each time an operation
  /// must be automatically assigned between a given date/time
  /// and an identified operation.
  /// 
  /// It does not represent the effective operations that assigned to a machine,
  /// but all the manual or automatic link operation processes
  /// that must be processed.
  /// 
  /// To know the current operation state of a machine,
  /// the table Operation Slot that is filled in by the Analyzer must be used.
  /// </summary>
  public interface ILinkOperation: IMachineModification
  {
    /// <summary>
    /// Begin date/time of the period that must be linked to an operation
    /// </summary>
    LowerBound<DateTime> Begin { get; set; }
    
    /// <summary>
    /// End date/time of the period that must be linked to an operation. If null, the end of the period is still unknown
    /// </summary>
    UpperBound<DateTime> End { get; set; }
    
    /// <summary>
    /// Should the period be linked to an operation in the left / past (-1)
    /// or in the right / future (+1) ?
    /// </summary>
    LinkDirection Direction { get; set; }
  }
}
