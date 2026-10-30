// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Pulse.Extensions.Business.ShiftSummary
{
  /// <summary>
  /// Usual kinds of <see cref="IShiftActivityItem"/>
  ///
  /// A plugin may define its own kinds
  /// </summary>
  public static class ShiftActivityKind
  {
    /// <summary>
    /// Period when the machine was stopped
    /// </summary>
    public const string Stop = "Stop";
    /// <summary>
    /// Planned machine observation state that is not the expected one
    /// </summary>
    public const string ObservationState = "ObservationState";
    /// <summary>
    /// CNC alarm, with a focused severity
    /// </summary>
    public const string CncAlarm = "CncAlarm";
    /// <summary>
    /// Summary of the CNC alarms whose severity is unknown
    /// </summary>
    public const string CncAlarmSummary = "CncAlarmSummary";
    /// <summary>
    /// Tool life event (reset, expiration, ...)
    /// </summary>
    public const string ToolLife = "ToolLife";
    /// <summary>
    /// Change of a tool geometry
    /// </summary>
    public const string ToolGeometry = "ToolGeometry";
    /// <summary>
    /// Total time of a reason
    /// </summary>
    public const string Reason = "Reason";
    /// <summary>
    /// Abnormal CNC value
    /// </summary>
    public const string CncValue = "CncValue";
    /// <summary>
    /// Production (operations, parts)
    /// </summary>
    public const string Production = "Production";
    /// <summary>
    /// Operator task (late, in error)
    /// </summary>
    public const string Task = "Task";
    /// <summary>
    /// Any other event
    /// </summary>
    public const string Other = "Other";
  }

  /// <summary>
  /// Something that happened on a machine during a shift
  ///
  /// It is a plain value object that can be serialized in JSON:
  /// it may be given as is to a LLM, or stored in the database
  /// </summary>
  public interface IShiftActivityItem
  {
    /// <summary>
    /// Kind of item, see <see cref="ShiftActivityKind"/>
    ///
    /// not null and not empty
    /// </summary>
    string Kind { get; }

    /// <summary>
    /// Section the item is displayed in, by default the kind
    /// </summary>
    string Section { get; }

    /// <summary>
    /// UTC start date/time, if applicable
    /// </summary>
    DateTime? Start { get; }

    /// <summary>
    /// UTC end date/time, if applicable
    /// </summary>
    DateTime? End { get; }

    /// <summary>
    /// Duration, if applicable. It may differ from End - Start (cumulated duration)
    /// </summary>
    TimeSpan? Duration { get; }

    /// <summary>
    /// Importance between 0 and 100: the higher, the more important
    /// </summary>
    int Importance { get; }

    /// <summary>
    /// Text that describes the item, already translated
    ///
    /// not null
    /// </summary>
    string Text { get; }

    /// <summary>
    /// Additional data, for a generator that is able to use it (number of occurrences,
    /// reason id, tool number, old and new values, ...)
    ///
    /// not null. The values must be serializable in JSON
    /// </summary>
    IDictionary<string, object> Properties { get; }
  }

  /// <summary>
  /// Default implementation of <see cref="IShiftActivityItem"/>
  /// </summary>
  public class ShiftActivityItem : IShiftActivityItem
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="kind">not null and not empty</param>
    /// <param name="text">not null</param>
    public ShiftActivityItem (string kind, string text)
    {
      this.Kind = kind;
      this.Section = kind;
      this.Text = text;
    }

    /// <summary>
    /// <see cref="IShiftActivityItem"/>
    /// </summary>
    public string Kind { get; }

    /// <summary>
    /// <see cref="IShiftActivityItem"/>
    /// </summary>
    public string Section { get; set; }

    /// <summary>
    /// <see cref="IShiftActivityItem"/>
    /// </summary>
    public DateTime? Start { get; set; }

    /// <summary>
    /// <see cref="IShiftActivityItem"/>
    /// </summary>
    public DateTime? End { get; set; }

    /// <summary>
    /// <see cref="IShiftActivityItem"/>
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// <see cref="IShiftActivityItem"/>
    /// </summary>
    public int Importance { get; set; } = 50;

    /// <summary>
    /// <see cref="IShiftActivityItem"/>
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// <see cref="IShiftActivityItem"/>
    /// </summary>
    public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object> ();
  }
}
