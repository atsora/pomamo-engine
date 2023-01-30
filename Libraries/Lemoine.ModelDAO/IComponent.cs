// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table Component
  /// </summary>
  public interface IComponent: IVersionable, IDataWithIdentifiers, IDisplayable, IEquatable<IComponent>, ISerializableModel, Lemoine.Collections.IDataWithId
  {
    /// <summary>
    /// Associated Part
    /// </summary>
    IPart Part { get; }

    /// <summary>
    /// Full name of the component as used in the shop
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Code given to the component
    /// </summary>
    string Code { get; set; }
    
    /// <summary>
    /// External code
    /// 
    /// It may help synchronizing PUSLE data with an external database
    /// </summary>
    string ExternalCode { get; set; }
    
    /// <summary>
    /// Link to the documentation in the network
    /// </summary>
    string DocumentLink { get; set; }
    
    /// <summary>
    /// Reference to the project the component is belong to.
    /// 
    /// This field is mandatory (not null), an orphaned component is not possible.
    /// </summary>
    IProject Project { get; set; }
    
    /// <summary>
    /// Associated component type
    /// </summary>
    IComponentType Type { get; set; }
    
    /// <summary>
    /// Reference to the final work piece (in IntermediateWorkPiece table)
    /// that corresponds to the finished component
    /// </summary>
    IIntermediateWorkPiece FinalWorkPiece { get; set; }
    
    /// <summary>
    /// Estimated hours
    /// </summary>
    double? EstimatedHours { get; set; }
    
    /// <summary>
    /// Set of intermediate work pieces that are associated to the component
    /// 
    /// The intermediate work pieces are ordered by ascending order (if available)
    /// </summary>
    ICollection<IComponentIntermediateWorkPiece> ComponentIntermediateWorkPieces { get; }
    
    /// <summary>
    /// List of stamps (and then ISO files) that are associated to this component
    /// </summary>
    ICollection<IStamp> Stamps { get; }
    

    /// <summary>
    /// Check if the component is undefined
    /// 
    /// A component is considered as undefined if it has no name and no given type
    /// </summary>
    /// <returns></returns>
    bool IsUndefined ();
    
    /// <summary>
    /// Add an intermediate work piece to the component
    /// without precising the code or order
    /// 
    /// If the intermediate work piece is already part of the component,
    /// nothing is done
    /// 
    /// Warning: if a new intermediate work piece is created, don't forget to make it persistent!
    /// </summary>
    /// <param name="intermediateWorkPiece">The intermediate work piece to add (can't be null)</param>
    /// <returns></returns>
    IComponentIntermediateWorkPiece AddIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece);
    
    /// <summary>
    /// Add an intermediate work piece to the component
    /// precising the code
    /// 
    /// If the intermediate work piece is already part of the component,
    /// nothing is done
    /// 
    /// Warning: if a new intermediate work piece is created, don't forget to make it persistent!
    /// </summary>
    /// <param name="intermediateWorkPiece">The intermediate work piece to add (can't be null)</param>
    /// <param name="code"></param>
    /// <returns></returns>
    IComponentIntermediateWorkPiece AddIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece,
                                                              string code);

    /// <summary>
    /// Add an intermediate work piece to the component
    /// precising the order
    /// 
    /// If the intermediate work piece is already part of the component,
    /// nothing is done
    /// 
    /// Warning: if a new intermediate work piece is created, don't forget to make it persistent!
    /// </summary>
    /// <param name="intermediateWorkPiece">The intermediate work piece to add (can't be null)</param>
    /// <param name="order"></param>
    /// <returns></returns>
    IComponentIntermediateWorkPiece AddIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece,
                                                              int order);

    /// <summary>
    /// Remove an intermediate work piece from the component
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    IList<IComponentIntermediateWorkPiece> RemoveIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece);
  }
}
