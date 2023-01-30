// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Pulse.Extensions.Web
{
  /// <summary>
  /// Active or coming event (cycle completion, active stop, coming stop, coming cycle end)
  /// </summary>
  public class Event
  {
    /// <summary>
    /// Message to display above the date/time
    /// 
    /// It must be short. It should be limited to about 15 characters
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Long text with a full description of the event
    /// 
    /// It can be longer than Message and give some additional information on the event
    /// </summary>
    public string LongText { get; set; }

    /// <summary>
    /// Estimated date/time for coming events
    /// Event start date/time for active events
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Event severity
    /// </summary>
    public EventSeverity Severity { get; set; }

    /// <summary>
    /// Pause the countdown when the machine is not running 
    /// during a machining sequence
    /// </summary>
    public bool PauseWhenMachiningNotRunning { get; set; }

    /// <summary>
    /// Can it be possibly late ? Meaning the remaining duration be negative ?
    /// </summary>
    public bool PossiblyLate { get; set; }

    /// <summary>
    /// Create an active event
    /// </summary>
    /// <param name="message"></param>
    /// <param name="dateTime"></param>
    /// <param name="severity"></param>
    /// <returns></returns>
    static public Event CreateActiveEvent (string message, DateTime dateTime, EventSeverity severity)
    {
      return new Event (message, dateTime, severity, false, false);
    }

    /// <summary>
    /// Create a coming event
    /// </summary>
    /// <param name="message"></param>
    /// <param name="dateTime"></param>
    /// <param name="severity"></param>
    /// <param name="pauseWhenMachiningNotRunning"></param>
    /// <param name="possiblyLate"></param>
    /// <returns></returns>
    static public Event CreateComingEvent (string message, DateTime dateTime, EventSeverity severity, bool pauseWhenMachiningNotRunning, bool possiblyLate)
    {
      return new Event (message, dateTime, severity, pauseWhenMachiningNotRunning, possiblyLate);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message">not null</param>
    /// <param name="dateTime"></param>
    /// <param name="severity">not null</param>
    /// <param name="pauseWhenMachiningNotRunning"></param>
    /// <param name="possiblyLate"></param>
    Event (string message, DateTime dateTime, EventSeverity severity, bool pauseWhenMachiningNotRunning, bool possiblyLate)
    {
      Debug.Assert (null != message);
      Debug.Assert (null != severity);

      this.Message = message;
      this.DateTime = dateTime;
      this.Severity = severity;
      this.PauseWhenMachiningNotRunning = PauseWhenMachiningNotRunning;
      this.PossiblyLate = possiblyLate;
    }
  }
}
