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

namespace Lemoine.Plugin.PerfRecorderSimpleLog
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    [PluginConf("Text", "Log prefix", Description = "Category prefix to add to the log")]
    [DefaultValue ("Perf.")]
    public string LogPrefix {
      get; set;
    }

    [PluginConf ("Text", "Regex", Description = "Regex filter of the performance key")]
    [DefaultValue ("")]
    public string Regex {
      get; set;
    }

    [PluginConf ("DurationPicker", "Info", Description = "Minimum duration to use the Info level", Parameters = "0:00:10")]
    public TimeSpan Info {
      get; set;
    }

    [PluginConf ("DurationPicker", "Warn", Description = "Minimum duration to use the Warn level", Parameters = "0:00:10")]
    public TimeSpan Warn
    {
      get; set;
    }

    [PluginConf ("DurationPicker", "Error", Description = "Minimum duration to use the Error level", Parameters = "0:00:10")]
    public TimeSpan Error
    {
      get; set;
    }

    [PluginConf ("DurationPicker", "Fatal", Description = "Minimum duration to use the Fatal level", Parameters = "0:00:10")]
    public TimeSpan Fatal
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
      this.LogPrefix = "Perf.";
      this.Regex = "";
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
