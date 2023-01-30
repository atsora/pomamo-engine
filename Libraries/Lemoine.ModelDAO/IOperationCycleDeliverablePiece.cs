// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of OperationCycleDeliverablePiece.
  /// 
  /// Stores the m:n relation between an Operation Cycle
  /// and a Deliverable Piece
  /// </summary>
  public interface IOperationCycleDeliverablePiece: IVersionable, IDataWithIdentifiers, IPartitionedByMachine
  {
    /// <summary>
    /// reference to the related Operation Cycle
    ///
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    IOperationCycle OperationCycle { get ; set; }
    
    /// <summary>
    /// reference to the related Deliverable Piece
    ///
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    IDeliverablePiece DeliverablePiece { get ; set ; }

    /// <summary>
    /// reference to possible nonconformance reason
    ///
    /// </summary>
    INonConformanceReason NonConformanceReason { get ; set ; }
    
    /// <summary>
    /// reference to possible nonconformance reason details
    ///
    /// </summary>
    string NonConformanceDetails { get ; set ; }
    
  }
}


