// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table IntermediateWorkPieceOperationUpdate
  /// 
  /// This table tracks the modifications that are made
  /// in the relations between an IntermediateWorkPiece and an Operation
  /// in table IntermediateWorkPiece.
  /// 
  /// It is necessary to allow the Analyzer service to update correctly
  /// all the Analysis tables.
  /// </summary>
  public interface IIntermediateWorkPieceOperationUpdate: IGlobalModification
  {
    /// <summary>
    /// Intermediate work piece
    /// 
    /// Not null
    /// </summary>
    IIntermediateWorkPiece IntermediateWorkPiece { get; set; }
    
    /// <summary>
    /// Old operation
    /// 
    /// null in case a new intermediate work piece / operation relation is set
    /// </summary>
    IOperation OldOperation { get; set; }
    
    /// <summary>
    /// New operation
    /// 
    /// null in case the intermediate work piece / operation is deleted
    /// </summary>
    IOperation NewOperation { get; set; }
    
    /// <summary>
    /// Old quantity of intermediate work pieces to make with the operation
    /// </summary>
    int OldQuantity { get; set; }
    
    /// <summary>
    /// New quantity of intermediate work pieces to make with the operation
    /// </summary>
    int NewQuantity { get; set; }
  }
}
