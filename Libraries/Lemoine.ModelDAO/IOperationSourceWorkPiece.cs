// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table OperationSourceWorkPiece
  /// 
  /// The associated table lists the different source work pieces
  /// that are necessary to run an operation.
  /// 
  /// From the operations, their source and destination work pieces,
  /// we can rebuild the work process to make a component.
  /// </summary>
  public interface IOperationSourceWorkPiece: IVersionable
  {
    /// <summary>
    /// Reference to the Operation
    /// </summary>
    IOperation Operation { get; }
    
    /// <summary>
    /// Reference to the intermediate work piece
    /// </summary>
    IIntermediateWorkPiece IntermediateWorkPiece { get; }
    
    /// <summary>
    /// Number of the same work pieces that are needed for the operation
    /// This number has to be divided by QuantityDenominator
    /// 
    /// Default is 1
    /// </summary>
    int Quantity { get; set; }
    
    /// <summary>
    /// Denominator of the number of intermediate workpieces needed for an operation
    /// 
    /// Default is 1, cannot be 0 or less
    /// </summary>
    int QuantityDenominator { get; set; }
  }
}
