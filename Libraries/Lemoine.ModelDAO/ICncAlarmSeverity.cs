// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Stop status
  /// </summary>
  public enum CncAlarmStopStatus
  {
    /// <summary>
    /// We have no information wether the machine stops or not
    /// </summary>
    Unknown,
    /// <summary>
    /// The alarm does not make the machine stop
    /// </summary>
    No,
    /// <summary>
    /// The alarm may make the machine stop
    /// </summary>
    Possibly,
    /// <summary>
    /// The alarm makes the machine stop, but there is no additional information when
    /// </summary>
    Yes,
    /// <summary>
    /// The machine stops at the end of the program
    /// </summary>
    ProgramEnd,
    /// <summary>
    /// The machine stops at the end of the block execution
    /// </summary>
    BlockEnd,
    /// <summary>
    /// The alarm triggers a feed hold
    /// </summary>
    FeedHold,
    /// <summary>
    /// The machine stops immediately (for example for an emergency stop)
    /// </summary>
    Immediate,
    /// <summary>
    /// The machine will stop at the next tool change
    /// </summary>
    NextToolChange,
  }

  /// <summary>
  /// Extensions to CncAlarmStop
  /// </summary>
  public static class CncAlarmStopStatusExtensions
  {
    /// <summary>
    /// Priority associated to a CncAlarmStopStatus
    /// 
    /// Number between 1 and 80
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static int GetPriority (this CncAlarmStopStatus t)
    {
      switch (t) {
        case CncAlarmStopStatus.No:
          return 80;
        case CncAlarmStopStatus.Unknown:
          return 51;
        case CncAlarmStopStatus.Possibly:
          return 50;
        case CncAlarmStopStatus.ProgramEnd:
          return 40;
        case CncAlarmStopStatus.NextToolChange:
          return 35;
        case CncAlarmStopStatus.BlockEnd:
          return 30;
        case CncAlarmStopStatus.FeedHold:
          return 20;
        case CncAlarmStopStatus.Yes:
          return 11;
        case CncAlarmStopStatus.Immediate:
          return 10;
        default:
          throw new NotImplementedException ("CncAlarmStopStatus.GetPriority not implemented for this status");
      }
    }
  }

  /// <summary>
  /// Description of ICncAlarmSeverity.
  /// </summary>
  public interface ICncAlarmSeverity : IVersionable, Lemoine.Model.ISerializableModel, IDisplayable
  {
    /// <summary>
    /// Cnc that can have the severity
    /// Cannot be null or empty
    /// </summary>
    string CncInfo { get; set; }

    /// <summary>
    /// CncAlarmSeverity name
    /// Cannot be null or empty
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Status of the alarm severity, useful to know if it can be updated or not
    /// 0: new (this is a new item that won't be updated)
    /// 1: default (update allowed)
    /// 2: edited (update forbidden)
    /// 3: deactivated (update forbidden and severity disabled)
    /// </summary>
    EditStatus Status { get; set; }

    /// <summary>
    /// CncAlarmSeverity description (often taken from the manuals)
    /// Can be null or empty
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// Stop status of the cnc alarm
    /// <see cref="CncAlarmStopStatus"/>
    /// </summary>
    CncAlarmStopStatus StopStatus { get; set; }

    /// <summary>
    /// Full description based on Description, ProgramStopped,
    /// and all other properties that might characterized the severity
    /// </summary>
    string FullDescription { get; }

    /// <summary>
    /// Color associated to the severity in the format #RRGGBB
    /// Can be null or empty
    /// </summary>
    string Color { get; set; }

    /// <summary>
    /// True if the severity must be taken into account
    /// False if the severity can be ignored
    /// Null if we don't know or if the answer is sometimes
    /// </summary>
    bool? Focus { get; set; }

    /// <summary>
    /// Computed priority
    /// 
    /// The lower the priority is, the more critical the alarm is
    /// 
    /// Number between 0 and 999
    /// </summary>
    int Priority { get; }
  }
}
