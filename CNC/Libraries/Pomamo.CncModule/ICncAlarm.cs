// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;

namespace Pomamo.CncModule
{
  /// <summary>
  /// Interface for the Cnc alarms
  /// </summary>
  public interface ICncAlarm
  {
    /// <summary>
    /// Alarm Cnc info
    /// Identify the CNC module that has created the alarm
    /// </summary>
    string CncInfo { get; set; }

    /// <summary>
    /// Alarm Sub info
    /// Identify more thoroughly the way an alarm has been created
    /// For example a specific version for the Fanuc CNC module (30i, 15, PH)
    /// or a file that has been used (machine alarms for Fanuc)
    /// </summary>
    string CncSubInfo { get; set; }

    /// <summary>
    /// Type of the alarm
    /// </summary>
    string Type { get; set; }

    /// <summary>
    /// Alarm Number
    /// </summary>
    string Number { get; set; }

    /// <summary>
    /// Alarm Message
    /// </summary>
    string Message { get; set; }

    /// <summary>
    /// Additionnal properties that might have an alarm
    /// Can be empty but not null
    /// Warning: using Dictionary with string / object will create a parsing bug in LemDataService (Fifo) 
    /// </summary>
    IDictionary<string, string> Properties { get; set; }
  }
}