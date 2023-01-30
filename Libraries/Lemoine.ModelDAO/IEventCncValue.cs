// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table EventLongPeriod
  /// </summary>
  public interface IEventCncValue: IEventMachineModule
  {
    /// <summary>
    /// Message
    /// </summary>
    string Message { get; }
    
    /// <summary>
    /// Associated field
    /// </summary>
    IField Field { get; }
    
    /// <summary>
    /// String value in case the corresponding Field refers to a String
    /// </summary>
    string String { get; }
    
    /// <summary>
    /// Int value in case the corresponding Field refers to an Int32
    /// </summary>
    int? Int { get; }
    
    /// <summary>
    /// Double or average value in case the corresponding Field refers to a Double
    /// </summary>
    double? Double { get; }
    
    /// <summary>
    /// String, Int or Double value according to the Type property of the field
    /// </summary>
    object Value { get; }

    /// <summary>
    /// Duration when the cnc value followed the condition
    /// </summary>
    TimeSpan Duration { get; }
    
    /// <summary>
    /// Associated config
    /// 
    /// null if the config has been removed
    /// </summary>
    IEventCncValueConfig Config { get; }
  }
}
