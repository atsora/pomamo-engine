// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table intermediateworkpiece
  /// 
  /// An intermediate work piece is the result of an operation.
  /// It represents a step in the machining of a component.
  /// </summary>
  public interface IIntermediateWorkPiece: IVersionable, IDataWithIdentifiers, IDisplayable, IEquatable<IIntermediateWorkPiece>, ISerializableModel, Lemoine.Collections.IDataWithId
  {
    /// <summary>
    /// Associated SimpleOperation
    /// </summary>
    ISimpleOperation SimpleOperation { get; }
    
    /// <summary>
    /// Full name of the intermediate workpiece as used in the shop
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Code of the intermediate work piece
    /// </summary>
    string Code { get; set; }
    
    /// <summary>
    /// External code
    /// 
    /// It may  help synchronizing Pomamo data with an external database
    /// </summary>
    string ExternalCode { get; set; }
    
    /// <summary>
    /// Link to the documentation in the network
    /// </summary>
    string DocumentLink { get; set; }
    
    /// <summary>
    /// Weight of the intermediate work piece when it is done
    /// 
    /// (this may help counting the number of made work pieces)
    /// </summary>
    double? Weight { get; set; }
    
    /// <summary>
    /// Reference to the operation that makes this work piece
    /// Can be null
    /// </summary>
    IOperation Operation { get; set; }
    
    /// <summary>
    /// Number of intermediate work pieces the operation makes.
    /// </summary>
    int OperationQuantity { get; set; }
    
    /// <summary>
    /// Possible next operations for this intermediate work piece
    /// </summary>
    ICollection<IOperation> PossibleNextOperations { get; }
    
    /// <summary>
    /// Set of components this intermediate work piece is known to be a part of
    /// </summary>
    ICollection<IComponentIntermediateWorkPiece> ComponentIntermediateWorkPieces { get; }

    /// <summary>
    /// Check if an intermediate work piece is undefined
    /// 
    /// An intermediate work piece is considered as undefined if it has no name and no code
    /// </summary>
    /// <returns></returns>
    bool IsUndefined ();

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
  }
}
