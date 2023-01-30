// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Class that corresponds to a SimpleOperation
  /// that is a join between the Operation and the IntermediateWorkPiece tables
  /// in case the data structure option IntermediateWorkPieceOperationIsSimpleOperation is set
  /// </summary>
  public interface ISimpleOperation: IVersionable, IDataWithIdentifiers, IDisplayable, IEquatable<ISimpleOperation>
  {
    /// <summary>
    /// Reference to the associated operation
    /// </summary>
    IOperation Operation { get; }

    /// <summary>
    /// Operation ID
    /// </summary>
    int OperationId { get; }
    
    /// <summary>
    /// Reference to the associated intermediate work piece
    /// </summary>
    IIntermediateWorkPiece IntermediateWorkPiece { get; }

    /// <summary>
    /// Intermediate work piece ID
    /// </summary>
    int IntermediateWorkPieceId { get; }
    
    /// <summary>
    /// Full name of the operation as used in the shop (written in the planning)
    /// 
    /// It corresponds to the operation name and the intermediate work piece name.
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Operation code
    /// 
    /// It corresponds to the operation code and the intermediate work piece code
    /// </summary>
    string Code { get; set; }
    
    /// <summary>
    /// External code
    /// 
    /// It may help synchronizing data with en external database
    /// 
    /// It corresponds to the operation external code and the intermediate work piece external code
    /// </summary>
    string ExternalCode { get; set; }
    
    /// <summary>
    /// Link to the documentation in the network
    /// 
    /// It corresponds to the operation document link and the intermediate work piece document link.
    /// </summary>
    string DocumentLink { get; set; }
    
    /// <summary>
    /// Associated operation type
    /// </summary>
    IOperationType Type { get; set; }
    
    /// <summary>
    /// Number of intermediate work pieces the operation makes.
    /// </summary>
    int Quantity { get; set; }

    /// <summary>
    /// Estimated machining duration
    /// </summary>
    TimeSpan? MachiningDuration { get; set; }
    
    /// <summary>
    /// Estimated setup duration
    /// </summary>
    TimeSpan? SetUpDuration { get; set; }
    
    /// <summary>
    /// Estimated tear down duration
    /// </summary>
    TimeSpan? TearDownDuration { get; set; }
    
    /// <summary>
    /// Estimated loading duration
    /// </summary>
    TimeSpan? LoadingDuration { get; set; }

    /// <summary>
    /// Estimated unloading duration
    /// </summary>
    TimeSpan? UnloadingDuration { get; set; }

    /// <summary>
    /// Weight of the intermediate work piece when it is done
    /// 
    /// (this may help counting the number of made work pieces)
    /// </summary>
    double? Weight { get; set; }
    
    /// <summary>
    /// First component that is associated to this operation
    /// </summary>
    IComponent Component { get; }
    
    /// <summary>
    /// Set of components this simple operation is known to be a part of
    /// </summary>
    ICollection<IComponentIntermediateWorkPiece> ComponentIntermediateWorkPieces { get; }
    
    /// <summary>
    /// Add a possible next operation
    /// </summary>
    /// <param name="operation"></param>
    void AddPossibleNextOperation (IOperation operation);

    /// <summary>
    /// Remove a possible next operation
    /// </summary>
    /// <param name="operation"></param>
    void RemovePossibleNextOperation (IOperation operation);
    
    /// <summary>
    /// Check if the simple operation is undefined
    /// 
    /// An simple operation is considered as undefined if it has no name and no given type
    /// </summary>
    /// <returns></returns>
    bool IsUndefined ();

    /// <summary>
    /// Creation date/time
    /// </summary>
    DateTime CreationDateTime { get; }

    /// <summary>
    /// Return a value if the operation has been archived
    /// </summary>
    DateTime? ArchiveDateTime { get; set; }
  }
}
