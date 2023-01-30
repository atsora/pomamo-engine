// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Collections;

namespace Lemoine.Model
{
  /// <summary>
  /// Description of IIntermediateWorkPieceTarget.
  /// </summary>
  public interface IIntermediateWorkPieceTarget : IDataWithVersion, IDataWithId
  {
    /// <summary>
    /// Reference to the IntermediateWorkPiece
    /// 
    /// Not null
    /// </summary>
    IIntermediateWorkPiece IntermediateWorkPiece { get; }
    
    /// <summary>
    /// Reference to the associated Component
    /// 
    /// Set to null if unknown
    /// </summary>
    IComponent Component { get; }
    
    /// <summary>
    /// Reference to the Work Order if known
    /// 
    /// Set to null if it could not be identified yet
    /// </summary>
    IWorkOrder WorkOrder { get; }
    
    /// <summary>
    /// Reference to the Line if known
    /// 
    /// Set to null if it could not be identified yet or it if is not applicable
    /// </summary>
    ILine Line { get; }
    
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
    /// Number of targeted work pieces
    /// </summary>
    int Number { get; set; }
    
    /// <summary>
    /// Is the data empty? It means may it be deleted because all the data are null?
    /// </summary>
    /// <returns></returns>
    bool IsEmpty ();
  }
}
