// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.DetectionStatusFromDatabase
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    [PluginConf ("Int", "Cycle detection status priority", Description = "Cycle detection status priority", Parameters = "999")]
    public int CycleDetectionStatusPriority { get; set; }

    [PluginConf ("Int", "Operation detection status priority", Description = "Operation detection status priority", Parameters = "999")]
    public int OperationDetectionStatusPriority { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration ()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public bool IsValid (out IEnumerable<string> errors)
    {
      var errorList = new List<string> ();
      errors = errorList;
      return true;
    }
    #endregion // Constructors
  }
}
