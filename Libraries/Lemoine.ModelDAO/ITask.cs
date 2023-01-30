// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Task status
  /// </summary>
  public enum TaskStatus
  {
    /// <summary>
    /// New
    /// </summary>
    New = 1,
    /// <summary>
    /// Running
    /// </summary>
    Running = 2,
    /// <summary>
    /// Completed
    /// </summary>
    Completed = 3,
    /// <summary>
    /// Hold
    /// </summary>
    Hold = 4
  }
  
  /// <summary>
  /// Task
  /// </summary>
  public interface ITask: IVersionable, IDataWithIdentifiers, IDisplayable, IEquatable<ITask>, ISerializableModel, Lemoine.Collections.IDataWithId
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
    IOperation Operation { get; }
    
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
    TaskStatus TaskStatus { get; set; }
    
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
    /// Order in which the task are scheduled
    /// </summary>
    double? Order { get; set; }
    
    /// <summary>
    /// Associated machine
    /// 
    /// May be null at start when a task is not fully scheduled yet
    /// </summary>
    IMachine Machine { get; set; }
  }
  
}
