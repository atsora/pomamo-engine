// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table WorkOrderProject
  /// 
  /// This new table stores the n:n relation 
  /// between the Work Order and the Project.
  /// 
  /// The field Work Order Project Quantity stores 
  /// how many projects must be manufactured in the given work order. 
  /// </summary>
  public interface IWorkOrderProject: IVersionable, IDataWithIdentifiers
  {
    /// <summary>
    /// Reference to the related work order
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    IWorkOrder WorkOrder { get; set; }
    
    /// <summary>
    /// Reference to the related project
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    IProject Project { get; set; }
    
    /// <summary>
    /// Number of project to machien for the given work order
    /// </summary>
    int Quantity { get; set; }
  }
}
