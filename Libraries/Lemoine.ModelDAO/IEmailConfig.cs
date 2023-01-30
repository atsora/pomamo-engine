// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// E-mail config to get dynamic recipient addresses
  /// </summary>
  public interface IEmailConfig: IVersionable
  {
    /// <summary>
    /// Config name
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Data type:
    /// <item>EventLongPeriod</item>
    /// <item>EventCncValue</item>
    /// <item>Log</item>
    /// </summary>
    string DataType { get; set; }
    
    /// <summary>
    /// A free filter to be really flexible
    /// </summary>
    string FreeFilter { get; set; }
    
    /// <summary>
    /// The name of the editor
    /// </summary>
    string Editor { get; set; }
    
    /// <summary>
    /// Maximum level priority to take into account
    /// 
    /// Default is 1000 (all)
    /// </summary>
    int MaxLevelPriority { get; set; }
    
    /// <summary>
    /// Event level filter
    /// 
    /// Default is null: no filter
    /// </summary>
    IEventLevel EventLevel { get; set; }
    
    /// <summary>
    /// Machine filter
    /// 
    /// Default is null: no filter
    /// </summary>
    IMachine Machine { get; set; }
    
    /// <summary>
    /// MachineFilter filter
    /// 
    /// Default is null: no filter
    /// </summary>
    IMachineFilter MachineFilter { get; set; }
    
    /// <summary>
    /// To: addresses
    /// </summary>
    string To { get; set; }
    
    /// <summary>
    /// Cc: addresses
    /// </summary>
    string Cc { get; set; }
    
    /// <summary>
    /// Bcc: addresses
    /// </summary>
    string Bcc { get; set; }
    
    /// <summary>
    /// Is the configuration active ?
    /// </summary>
    bool Active { get; set; }
    
    /// <summary>
    /// Applicable week days
    /// </summary>
    WeekDay WeekDays { get; set; }
    
    /// <summary>
    /// Applicable time period of day
    /// </summary>
    TimePeriodOfDay TimePeriod { get; set; }
    
    /// <summary>
    /// Only applicable from the specified UTC date/time
    /// 
    /// If null, applicable from now
    /// </summary>
    DateTime? BeginDateTime { get; set; }
    
    /// <summary>
    /// Only applicable from the specified local date/time
    /// 
    /// If null, applicable from now
    /// </summary>
    DateTime? LocalBegin { get; set; }
    
    /// <summary>
    /// Only applicable to the specified UTC date/time
    /// 
    /// If null, no limit
    /// </summary>
    DateTime? EndDateTime { get; set; }
    
    /// <summary>
    /// Only applicable to the specified local date/time
    /// 
    /// If null, no limit
    /// </summary>
    DateTime? LocalEnd { get; set; }

    /// <summary>
    /// Purge the automatically the item if EndDateTime was reached
    /// 
    /// Default is false
    /// </summary>
    bool AutoPurge { get; set; }
  }
}
