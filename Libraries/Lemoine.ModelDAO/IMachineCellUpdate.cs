// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table MachineCellUpdate
  /// 
  /// This table tracks the modifications that are made
  /// in the relations between a machine and a cell
  /// in table Machine.
  /// 
  /// It is necessary to allow the Analyzer service to update correctly
  /// all the Analysis tables.
  /// </summary>
  public interface IMachineCellUpdate: IMachineModification
  {
    /// <summary>
    /// Old cell
    /// 
    /// null in case a new machine / cell relation is set
    /// </summary>
    ICell OldCell { get; }
    
    /// <summary>
    /// New cell
    /// 
    /// null in case the machine / cell is deleted
    /// </summary>
    ICell NewCell { get; }
  }
}
