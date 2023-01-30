// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Pulse.Extensions.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// Severity DTO
  /// </summary>
  public class SeverityDTO
  {
    EventSeverity m_eventSeverity;

    /// <summary>
    /// Severity name
    /// </summary>
    public string Name
    {
      get { return m_eventSeverity.Name; }
      set { }
    }

    /// <summary>
    /// Severity level name
    /// </summary>
    public string LevelName
    {
      get { return m_eventSeverity.LevelName; }
      set { }
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
      get { return m_eventSeverity.LevelValue; }
      set { }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">not null or empty</param>
    /// <param name="level"></param>
    public SeverityDTO (string name, EventSeverityLevel level)
      : this (new EventSeverity (level))
    {
      m_eventSeverity.Name = name;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="eventSeverity">not null</param>
    public SeverityDTO (EventSeverity eventSeverity)
    {
      Debug.Assert (null != eventSeverity);

      m_eventSeverity = eventSeverity;
    }
  }
}
