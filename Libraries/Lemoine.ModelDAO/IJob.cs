// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// Class that corresponds to a Job
  /// that is a join between WorkOrder and Project
  /// in case the data structure option WorkOrderProjectIsJob is set
  /// </summary>
  public interface IJob: IVersionable, IDataWithIdentifiers, IDisplayable, IEquatable<IJob>
  {
    /// <summary>
    /// Job ID that corresponds also to the Project ID
    /// </summary>
    int ProjectId { get; }
    
    /// <summary>
    /// Work Order ID
    /// </summary>
    int WorkOrderId { get; }
    
    /// <summary>
    /// Associated project
    /// </summary>
    IProject Project { get; }
    
    /// <summary>
    /// Associated work order
    /// </summary>
    IWorkOrder WorkOrder { get; }
    
    /// <summary>
    /// Job name that corresponds also to
    /// the project name and the work order name
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Job code that corresponds also to
    /// the project code and the work order code
    /// </summary>
    string Code { get; set; }
    
    /// <summary>
    /// Job external code that corresponds to
    /// the project external code and the work order external code
    /// </summary>
    string ExternalCode { get; set; }
    
    /// <summary>
    /// Associated customer (nullable)
    /// </summary>
    ICustomer Customer { get; set; }

    /// <summary>
    /// Job document link that corresponds to
    /// the project document link and the work order document link
    /// </summary>
    string DocumentLink { get; set; }
    
    /// <summary>
    /// Work order delivery date
    /// </summary>
    DateTime? DeliveryDate { get; set; }
    
    /// <summary>
    /// Work order status
    /// </summary>
    IWorkOrderStatus Status { get; set; }

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
    /// Components that are associated to the project
    /// </summary>
    ICollection <IComponent> Components { get; }
    /// <summary>
    /// Add a component
    /// </summary>
    /// <param name="component"></param>
    void AddComponent (IComponent component);
    
    /// <summary>
    /// Check if the job is undefined
    /// 
    /// A job is considered as undefined if it has no name and no code
    /// </summary>
    /// <returns></returns>
    bool IsUndefined ();
  }
}
