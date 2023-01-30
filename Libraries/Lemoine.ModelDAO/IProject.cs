// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of the Project table
  /// 
  /// A project represents a coherent set of components
  /// that must be delivered in the same time.
  /// </summary>
  public interface IProject: IVersionable, IDataWithIdentifiers, IDisplayable, IEquatable<IProject>, ISerializableModel, Lemoine.Collections.IDataWithId
  {
    /// <summary>
    /// Associated Part
    /// </summary>
    IPart Part { get; }
    
    /// <summary>
    /// Associated Job
    /// </summary>
    IJob Job { get; }
    
    /// <summary>
    /// Project name
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Project code
    /// </summary>
    string Code { get; set; }
    
    /// <summary>
    /// Project external code
    /// </summary>
    string ExternalCode { get; set; }
    
    /// <summary>
    /// Project document link
    /// </summary>
    string DocumentLink { get; set; }
    
    /// <summary>
    /// Associated customer (nullable)
    /// </summary>
    ICustomer Customer { get; set; }

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
    /// Unset if the propject is not archived.
    /// Else date/time when the project is archived.
    /// </summary>
    DateTime? ArchiveDateTime { get; set; }
    
    /// <summary>
    /// Work orders that are associated to the project
    /// </summary>
    ICollection<IWorkOrder> WorkOrders { get; }
    
    /// <summary>
    /// Components that are associated to the project
    /// </summary>
    ICollection <IComponent> Components { get; }
    
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
    /// Add a component
    /// </summary>
    /// <param name="component"></param>
    void AddComponent (IComponent component);
        
    /// <summary>
    /// Remove a component
    /// </summary>
    /// <param name="component"></param>
    void RemoveComponent (IComponent component);
    
    /// <summary>
    /// Check if the project is undefined
    /// 
    /// A project is considered as undefined if it has no name and no code
    /// </summary>
    /// <returns></returns>
    bool IsUndefined ();
  }  
}
