// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Description of ICurrentCncAlarm.
  /// </summary>
  public interface ICurrentCncAlarm: IDisplayable, IVersionable, IPartitionedByMachineModule, Lemoine.Model.ISerializableModel
  {
    /// <summary>
    /// UTC date/time stamp
    /// </summary>
    DateTime DateTime { get; set; }
    
    /// <summary>
    /// Alarm CncInfo
    /// </summary>
    string CncInfo { get; }
    
    /// <summary>
    /// Alarm CncSubInfo
    /// </summary>
    string CncSubInfo { get; }
    
    /// <summary>
    /// Alarm type
    /// </summary>
    string Type { get; }
    
    /// <summary>
    /// Alarm Number (as a string)
    /// </summary>
    string Number { get; }
    
    /// <summary>
    /// Alarm Message
    /// </summary>
    string Message { get ; set; }
    
    /// <summary>
    /// Properties (stored as jsonb in the database)
    /// </summary>
    IDictionary<string, object> Properties { get; }
    
    /// <summary>
    /// Associated color
    /// </summary>
    string Color { get; }
    
    /// <summary>
    /// Associated severity
    /// (computed with CncAlarmSeverityPattern)
    /// </summary>
    ICncAlarmSeverity Severity { get; }
    
    /// <summary>
    /// Computed priority, based on the severity if any
    /// 
    /// The lower the priority is, the more critical the alarm is
    /// </summary>
    int Priority { get; }
  }
}
