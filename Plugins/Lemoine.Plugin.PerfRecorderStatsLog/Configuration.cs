// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System.ComponentModel;

namespace Lemoine.Plugin.PerfRecorderStatsLog
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    [PluginConf ("Text", "Log prefix", Description = "Category prefix to add to the log")]
    [DefaultValue ("PerfStats.")]
    public string LogPrefix
    {
      get; set;
    }

    [PluginConf ("DurationPicker", "Frequency", Description = "Frequency of the stats", Parameters = "0:00:10")]
    public TimeSpan Frequency
    {
      get; set;
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration ()
    {
      this.LogPrefix = "PerfStats.";
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
