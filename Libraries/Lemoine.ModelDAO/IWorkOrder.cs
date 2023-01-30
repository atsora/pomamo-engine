// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table WorkOrder
  /// </summary>
  public interface IWorkOrder: IVersionable, IDataWithIdentifiers, IDisplayable, IEquatable<IWorkOrder>, ISerializableModel, Lemoine.Collections.IDataWithId
  {
    /// <summary>
    /// Associated Job
    /// </summary>
    IJob Job { get; }
    
    /// <summary>
    /// Work order name
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Work order code
    /// </summary>
    string Code { get; set; }
    
    /// <summary>
    /// Work order external code
    /// </summary>
    string ExternalCode { get; set; }
    
    /// <summary>
    /// Associated customer (nullable)
    /// </summary>
    ICustomer Customer { get; set; }

    /// <summary>
    /// Work order document link
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
    /// Projects that are associated to the work order
    /// </summary>
    ICollection<IProject> Projects { get; }
    
    /// <summary>
    /// Parts that are associated to the work order
    /// </summary>
    ICollection<IPart> Parts { get; }
    
    /// <summary>
    /// Check if the work order is undefined
    /// 
    /// A work order is considered as undefined if it has no name and no code
    /// </summary>
    /// <returns></returns>
    bool IsUndefined ();
  }
}
