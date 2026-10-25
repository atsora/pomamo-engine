// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Manufacturing order status
  ///
  /// The values are the ids of the manuforderstatus table, see
  /// <see cref="IManufacturingOrderStatus"/> for their display and color
  /// </summary>
  public enum ManufacturingOrderStatus
  {
    /// <summary>
    /// New
    /// </summary>
    New = 1,
    /// <summary>
    /// Ready
    /// </summary>
    Ready = 2,
    /// <summary>
    /// Running
    /// </summary>
    Running = 3,
    /// <summary>
    /// Completed
    /// </summary>
    Completed = 4,
    /// <summary>
    /// Hold
    /// </summary>
    Hold = 5
  }
  
  /// <summary>
  /// Task
  /// </summary>
  public interface IManufacturingOrder: IVersionable, IDataWithIdentifiers, IDisplayable, IEquatable<IManufacturingOrder>, ISerializableModel, Lemoine.Collections.IDataWithId
  {
    /// <summary>
    /// External code
    /// 
    /// It may help synchronizing Pomamo data with en external database
    /// 
    /// Nullable
    /// </summary>
    string ExternalCode { get; set; }
    
    /// <summary>
    /// Associated operation
    /// 
    /// Nullable
    /// </summary>
    IOperation Operation { get; set; }
    
    /// <summary>
    /// Associated component (optional)
    /// 
    /// Nullable
    /// </summary>
    IComponent Component { get; set; }
    
    /// <summary>
    /// Associated work order (optional)
    /// 
    /// Nullable
    /// </summary>
    IWorkOrder WorkOrder { get; set; }
    
    /// <summary>
    /// Task status
    /// </summary>
    ManufacturingOrderStatus ManufacturingOrderStatus { get; set; }
    
    /// <summary>
    /// Quantity (optional)
    /// </summary>
    int? Quantity { get; set; }
    
    /// <summary>
    /// Set-up duration
    /// 
    /// Optional: if not set, the set-up duration is taken from the operation properties
    /// </summary>
    TimeSpan? SetupDuration { get; set; }
    
    /// <summary>
    /// Cycle duration
    /// 
    /// Optional: if not set, the cycle duration is determined from the operation properties
    /// </summary>
    TimeSpan? CycleDuration { get; set; }
    
    /// <summary>
    /// Due date/time
    /// </summary>
    DateTime? DueDateTime { get; set; }
    
    /// <summary>
    /// Order in which the manufacturing orders are scheduled
    /// </summary>
    double? Order { get; set; }
    
    /// <summary>
    /// Associated machine
    /// 
    /// May be null at start when a manufacturing order is not fully scheduled yet
    /// </summary>
    IMachine Machine { get; set; }
  }
  
}
