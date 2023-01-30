// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration.GuiBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Plugin.AnalysisStateMachineRunTimeOnly
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    static readonly int NORMAL_PRIORITY_DEFAULT = 100; // 80 includes the auto-reasons
    static readonly int LOW_PRIORITY_DEFAULT = 50;
    static readonly TimeSpan LOW_PRIORITY_FREQUENCY_DEFAULT = TimeSpan.FromMinutes (2);
    static readonly TimeSpan VERY_LOW_PRIORITY_FREQUENCY_DEFAULT = TimeSpan.FromMinutes (5);

    #region Getters / Setters
    [PluginConf ("DoubleAsNumericUpDown", "StateMachinePriority. Default is 20.0", Description = "State machine priority", Parameters = "99999.99")]
    [DefaultValue (20.0)]
    public double StateMachinePriority
    {
      get; set;
    }

    [PluginConf ("IntAsNumericUpDown", "Normal modification priority", Description = "Maximum priority of a normal priority modification. Default is 100", Parameters = "999999")]
    [DefaultValue (100)]
    public int NormalModificationPriority
    {
      get; set;
    }

    [PluginConf ("DurationPicker", "Normal priority frequency", Description = "Frequency for the normal priority modifications. Default is 0:00:30", Parameters = "0:00:30")]
    [DefaultValue ("0:00:30")]
    public TimeSpan NormalPriorityFrequency
    {
      get; set;
    }

    [PluginConf ("IntAsNumericUpDown", "Low modification priority", Description = "Maximum priority of a low priority modification. Default is 50", Parameters = "999999")]
    [DefaultValue (50)]
    public int LowModificationPriority
    {
      get; set;
    }

    [PluginConf ("DurationPicker", "Low priority frequency", Description = "Frequency for the low priority modifications. Default is 0:02:00", Parameters = "0:00:10")]
    [DefaultValue ("0:02:00")]
    public TimeSpan LowPriorityFrequency
    {
      get; set;
    }

    [PluginConf ("DurationPicker", "Very low priority frequency", Description = "Frequency for the low priority modifications. Default is 0:05:00", Parameters = "0:00:10")]
    [DefaultValue ("0:05:00")]
    public TimeSpan VeryLowPriorityFrequency
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
      this.StateMachinePriority = 20.0;
      this.NormalModificationPriority = NORMAL_PRIORITY_DEFAULT;
      this.LowModificationPriority = LOW_PRIORITY_DEFAULT;
      this.LowPriorityFrequency = LOW_PRIORITY_FREQUENCY_DEFAULT;
      this.VeryLowPriorityFrequency = VERY_LOW_PRIORITY_FREQUENCY_DEFAULT;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public bool IsValid (out IEnumerable<string> errors)
    {
      var errorList = new List<string> ();

      if (this.VeryLowPriorityFrequency < this.LowPriorityFrequency) {
        log.Error ($"IsValid: low priority frequency {this.LowPriorityFrequency} is greater than very low priority frequency {this.VeryLowPriorityFrequency}");
        errorList.Add ("Invalid priority frequencies");
      }
      if (this.NormalModificationPriority <= this.LowModificationPriority) {
        log.Error ($"IsValid: low priority {this.LowModificationPriority} is greater than normal priority {this.NormalModificationPriority}");
        errorList.Add ("Invalid priorities");
      }

      errors = errorList;
      return true;
    }
    #endregion // Constructors
  }
}
