// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Class that corresponds to a Part
  /// that is a join between the Project and the Component tables
  /// in case the data structure option ProjectComponentIsPart is set
  /// </summary>
  public interface IPart: IVersionable, IDataWithIdentifiers, IDisplayable, IEquatable<IPart>
  {
    /// <summary>
    /// Component
    /// </summary>
    IComponent Component { get; }
    
    /// <summary>
    /// Part ID that corresponds also to the Component ID
    /// </summary>
    int ComponentId { get; }
    
    /// <summary>
    /// Associated project
    /// </summary>
    IProject Project { get; }

    /// <summary>
    /// Project ID
    /// </summary>
    int ProjectId { get; }
    
    /// <summary>
    /// Full name of the part as used in the shop
    /// that corresponds also to the name of the component
    /// or the name of the project
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Code given to the component
    /// that corresponds also to the code of the component
    /// or the code of the project
    /// </summary>
    string Code { get; set; }
    
    /// <summary>
    /// External code
    /// 
    /// It may help synchronizing PUSLE data with an external database
    /// 
    /// It corresponds also to the external code of the component
    /// and the external code of the project
    /// </summary>
    string ExternalCode { get; set; }
    
    /// <summary>
    /// Associated customer (nullable)
    /// </summary>
    ICustomer Customer { get; set; }

    /// <summary>
    /// Link to the documentation in the network
    /// 
    /// It corresponds also to the document link of the component
    /// and the document link of the project
    /// </summary>
    string DocumentLink { get; set; }
    
    /// <summary>
    /// Project creation date/time
    /// </summary>
    DateTime CreationDateTime { get; }
    
    /// <summary>
    /// Creation date/time when the project is created.
    /// Else date/time of the re-activation of the project
    /// if the project has been archived
    /// </summary>
    DateTime ReactivationDateTime { get; set; }
    
    /// <summary>
    /// Unset if the project is not archived.
    /// Else date/time when the project is archived.
    /// </summary>
    DateTime? ArchiveDateTime { get; set; }
    
    /// <summary>
    /// Work orders that are associated to the project
    /// </summary>
    ICollection<IWorkOrder> WorkOrders { get; }

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
    /// List of stamps (and then ISO files) that are associated to this part
    /// </summary>
    ICollection<IStamp> Stamps { get; }
    
    /// <summary>
    /// Add a work order
    /// </summary>
    /// <param name="workOrder"></param>
    void AddWorkOrder (IWorkOrder workOrder);
    
    /// <summary>
    /// Remove a work order
    /// </summary>
    /// <param name="workOrder"></param>
    void RemoveWorkOrder (IWorkOrder workOrder);
    
    /// <summary>
    /// Add an intermediate work piece to the part
    /// without precising the code or order
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    IComponentIntermediateWorkPiece AddIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece);
    
    /// <summary>
    /// Remove an intermediate work piece from the component
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    IList<IComponentIntermediateWorkPiece> RemoveIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece);
    
    /// <summary>
    /// Check if the part is undefined
    /// 
    /// An part is considered as undefined if it has no name and no code
    /// </summary>
    /// <returns></returns>
    bool IsUndefined ();
  }
}
