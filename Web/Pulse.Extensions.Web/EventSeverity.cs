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
  /// Event severity in web extensions
  /// </summary>
  public class EventSeverity
  {
    /// <summary>
    /// Severity name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Severity level
    /// </summary>
    public EventSeverityLevel Level { get; set; }

    /// <summary>
    /// Severity level name
    /// </summary>
    public string LevelName
    {
      get { return this.Level.ToString (); }
    }

    /// <summary>
    /// Severity value / importance
    /// 
    /// The lower it is, the more serious the severity is:
    /// <item>Range [40,50) corresponds to an error</item>
    /// <item>Range [50,60) corresponds to a warning</item>
    /// <item>Range [80,90) corresponds to a piece of information</item>
    /// </summary>
    public double LevelValue
    {
      get { return GetLevelValue (this.Level); }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="level"></param>
    public EventSeverity (EventSeverityLevel level)
    {
      this.Name = level.ToString ();
      this.Level = level;
    }

    double GetLevelValue (EventSeverityLevel level)
    {
      switch (level) {
        case EventSeverityLevel.Error:
          return 40.0;
        case EventSeverityLevel.Warning2:
          return 54.0;
        case EventSeverityLevel.Warning1:
          return 55.0;
        case EventSeverityLevel.Info:
          return 80.0;
        default:
          throw new NotImplementedException ("Level value not implemented for level in parameter");
      }
    }
  }

  /// <summary>
  /// Define different severity levels
  /// </summary>
  public enum EventSeverityLevel
  {
    /// <summary>
    /// Error (usually in red)
    /// </summary>
    Error,
    /// <summary>
    /// Warning 2 (usually in orange)
    /// </summary>
    Warning2,
    /// <summary>
    /// Warning 1 (usually in yellow)
    /// </summary>
    Warning1,
    /// <summary>
    /// Info (usually in blue or white)
    /// </summary>
    Info,
  }
}
