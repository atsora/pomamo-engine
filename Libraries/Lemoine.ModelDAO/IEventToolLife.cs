// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.SharedData;

namespace Lemoine.Model
{
  /// <summary>
  /// Extension methods to the EventToolLifeType enum
  /// </summary>
  public static class EventToolLifeTypeExtensions
  {
    /// <summary>
    /// Return the name of the tool life event type
    /// </summary>
    /// <param name="toolLifeEventType"></param>
    /// <returns></returns>
    public static string Name(this EventToolLifeType toolLifeEventType)
    {
      string txt = "";
      
      switch (toolLifeEventType) {
        case EventToolLifeType.Unknown:
          txt = "Unknown";
          break;
        case EventToolLifeType.ToolRegistration:
          txt = "Tool registration";
          break;
        case EventToolLifeType.ToolMoved:
          txt = "Tool number change";
          break;
        case EventToolLifeType.ToolRemoval:
          txt = "Tool removal";
          break;
        case EventToolLifeType.TotalLifeIncreased:
          txt = "Total tool life increased";
          break;
        case EventToolLifeType.TotalLifeDecreased:
          txt = "Total tool life decreased";
          break;
        case EventToolLifeType.CurrentLifeDecreased:
          txt = "Current tool life decreased";
          break;
        case EventToolLifeType.RestLifeIncreased:
          txt = "Current rest life increased";
          break;
        case EventToolLifeType.WarningReached:
          txt = "Warning reached";
          break;
        case EventToolLifeType.ExpirationReached:
          txt = "Expiration reached";
          break;
        case EventToolLifeType.StatusChangeToAvailable:
          txt = "Status change to available";
          break;
        case EventToolLifeType.StatusChangeToTemporaryUnavailable:
          txt = "Status change to temporary unavailable";
          break;
        case EventToolLifeType.StatusChangeToDefinitelyUnavailable:
          txt = "Status change to definitely unavailable";
          break;
        case EventToolLifeType.CurrentLifeReset:
          txt = "Current tool life reset";
          break;
        case EventToolLifeType.RestLifeReset:
          txt = "Current rest life reset";
          break;
        case EventToolLifeType.WarningChanged:
          txt = "Warning changed";
          break;
      }
      
      return txt;
    }
  }
  
  /// <summary>
  /// Type of an event that may occur during the life of a tool
  /// The level of severity is an example (info / warning / error)
  /// </summary>
  public enum EventToolLifeType {
    /// <summary>
    /// Unknown type
    /// </summary>
    Unknown = 0,
    
    /// <summary>
    /// Info: a new tool has been registered
    /// </summary>
    ToolRegistration = 1,
    
    /// <summary>
    /// Info: a tool moved (pot and / or magazine changed)
    /// </summary>
    ToolMoved = 2,
    
    /// <summary>
    /// Info: a tool has been removed
    /// </summary>
    ToolRemoval = 3,
    
    /// <summary>
    /// Error: the total life of a tool increased
    /// </summary>
    TotalLifeIncreased = 4,
    
    /// <summary>
    /// Warning: the total life of a tool decreased
    /// </summary>
    TotalLifeDecreased = 5,
    
    /// <summary>
    /// Error: current life decreased to a number higher than 0
    /// </summary>
    CurrentLifeDecreased = 6,
    
    /// <summary>
    /// Error: rest of life increased (should not happen)
    /// </summary>
    RestLifeIncreased = 7,
    
    /// <summary>
    /// Info: the rest life of a tool is weak
    /// </summary>
    WarningReached = 8,
    
    /// <summary>
    /// Warning: the expiration of a tool has been reached
    /// </summary>
    ExpirationReached = 9,
    
    /// <summary>
    /// Info: the tool is now available
    /// </summary>
    StatusChangeToAvailable = 10,
    
    /// <summary>
    /// Warning: the tool is now temporary unavailable
    /// </summary>
    StatusChangeToTemporaryUnavailable = 11,
    
    /// <summary>
    /// Error: the tool is now definitely unavailable
    /// </summary>
    StatusChangeToDefinitelyUnavailable = 12,
    
    /// <summary>
    /// Info: current life reset to 0
    /// </summary>
    CurrentLifeReset = 13,
    
    /// <summary>
    /// Info: rest life reset to the limit
    /// </summary>
    RestLifeReset = 14,
    
    /// <summary>
    /// Info: the warning of a tool changed
    /// </summary>
    WarningChanged = 15
  }
  
  /// <summary>
  /// Model for table EventToolLife
  /// </summary>
  public interface IEventToolLife: IEventMachineModule, ISerializableModel
  {
    /// <summary>
    /// Type of the event that has occured during the life of the tool
    /// </summary>
    EventToolLifeType EventType { get; }
    
    /// <summary>
    /// EventToolLife message
    /// </summary>
    string Message { get; set; }
    
    /// <summary>
    /// Location: old magazine number, may be null
    /// </summary>
    int? OldMagazine { get; set; }
    
    /// <summary>
    /// Location: new magazine number, may be null
    /// </summary>
    int? NewMagazine { get; set; }
    
    /// <summary>
    /// Location: old pot number, may be null
    /// </summary>
    int? OldPot { get; set; }
    
    /// <summary>
    /// Location: new pot number, may be null
    /// </summary>
    int? NewPot { get; set; }
    
    /// <summary>
    /// Old tool state
    /// </summary>
    ToolState OldToolState { get; set; }
    
    /// <summary>
    /// New tool state
    /// </summary>
    ToolState NewToolState { get; set; }
    
    /// <summary>
    /// Tool number
    /// </summary>
    string ToolNumber { get; set; }
    
    /// <summary>
    /// Tool id
    /// </summary>
    string ToolId { get; set; }
    
    /// <summary>
    /// Different way to count the life of a tool
    /// </summary>
    ToolLifeDirection Direction { get; set; }
    
    /// <summary>
    /// Unit characterizing the life
    /// </summary>
    IUnit Unit { get; set; }
    
    /// <summary>
    /// Old life value
    /// </summary>
    double? OldValue { get; set; }
    
    /// <summary>
    /// New life type
    /// </summary>
    double? NewValue { get; set; }
    
    /// <summary>
    /// Old warning value, absolute
    /// </summary>
    double? OldWarning { get; set; }
    
    /// <summary>
    /// New warning value, absolute
    /// </summary>
    double? NewWarning { get; set; }
    
    /// <summary>
    /// Old limit value
    /// </summary>
    double? OldLimit { get; set; }
    
    /// <summary>
    /// New limit value
    /// </summary>
    double? NewLimit { get; set; }
    
    /// <summary>
    /// Associated config
    /// 
    /// null if the config has been removed
    /// </summary>
    IEventToolLifeConfig Config { get; set; }
    
    /// <summary>
    /// Elapsed time between the last tool life data and the current tool life data, in milliseconds
    /// </summary>
    int ElapsedTime { get; set; }
    
    /// <summary>
    /// Machine observation state that is associated to the event
    /// Can be null
    /// </summary>
    IMachineObservationState MachineObservationState { get; set; }
  }
}
