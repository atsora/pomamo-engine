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

namespace Lemoine.Plugin.AnalysisStateMachineProductionShop
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    static readonly int NORMAL_PRIORITY_DEFAULT = 100; // 80 includes the auto-reasons
    static readonly int LOW_PRIORITY_DEFAULT = 50;
    static readonly TimeSpan LOW_PRIORITY_FREQUENCY_DEFAULT = TimeSpan.FromMinutes (2);
    static readonly TimeSpan VERY_LOW_PRIORITY_FREQUENCY_DEFAULT = TimeSpan.FromMinutes (5);
    static readonly int AUTO_PRIORITY_DEFAULT = 50;

    #region Getters / Setters
    [PluginConf ("DoubleAsNumericUpDown", "ConfigPriority. Default is 30.0", Description = "Config priority", Parameters = "99999.99")]
    [DefaultValue (30.0)]
    public double ConfigPriority
    {
      get; set;
    }

    [PluginConf ("DoubleAsNumericUpDown", "StateMachinePriority. Default is 10.0", Description = "State machine priority", Parameters = "99999.99")]
    [DefaultValue (10.0)]
    public double StateMachinePriority
    {
      get; set;
    }

    [PluginConf ("DurationPicker", "Maximum time", Description = "Maximum time for the state machine execution. Default is 0:01:30", Parameters = "0:00:10")]
    [DefaultValue ("0:01:30")]
    public TimeSpan MaxTime
    {
      get; set;
    }

    [PluginConf ("DurationPicker", "Machine state template analysis maximum time", Description = "Maximum time for the analysis of the machine state templates. Default is 0:02:00", Parameters = "0:00:10")]
    [DefaultValue ("0:02:00")]
    public TimeSpan MachineStateTemplatesMaxTime
    {
      get; set;
    }

    [PluginConf ("DurationPicker", "Pending modifications maximum time", Description = "Maximum time for the analysis of the pending modifications. Default is 0:01:30", Parameters = "0:00:10")]
    [DefaultValue ("0:01:30")]
    public TimeSpan PendingModificationsMaxTime
    {
      get; set;
    }

    [PluginConf ("IntAsNumericUpDown", "Normal modification priority", Description = "Maximum priority of a normal priority modification. Default is 100", Parameters = "999999")]
    [DefaultValue (100)]
    public int NormalModificationPriority
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

    [PluginConf ("IntAsNumericUpDown", "Auto modification priority", Description = "Priority of an auto modification. Default is 50", Parameters = "999999")]
    [DefaultValue (50)]
    public int AutoModificationPriority
    {
      get; set;
    } = AUTO_PRIORITY_DEFAULT;

    [PluginConf ("IntAsNumericUpDown", "Number of facts", Description = "Number of facts to get. Default is 50", Parameters = "999999")]
    [DefaultValue (50)]
    public int FactNumber
    {
      get; set;
    }

    [PluginConf ("DurationPicker", "Processing reason slots analysis maximum time", Description = "Maximum time for the analysis of the processing reason slots. Default is 0:00:30", Parameters = "0:00:10")]
    [DefaultValue ("0:00:30")]
    public TimeSpan ProcessingReasonSlotsMaxTime
    {
      get; set;
    }

    [PluginConf ("DurationPicker", "Detection analysis maximum time", Description = "Maximum time for the detection analysis. Default is 0:01:00", Parameters = "0:00:10")]
    [DefaultValue ("0:01:00")]
    public TimeSpan DetectionMaxTime
    {
      get; set;
    }

    [PluginConf ("DurationPicker", "Auto-sequences analysis maximum time", Description = "Maximum time for the analysis of the auto-sequences. Default is 0:00:40", Parameters = "0:00:10")]
    [DefaultValue ("0:00:40")]
    public TimeSpan AutoSequencesMaxTime
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
      this.ConfigPriority = 30.0;
      this.StateMachinePriority = 10.0;
      this.MaxTime = TimeSpan.FromSeconds (90);
      this.MachineStateTemplatesMaxTime = TimeSpan.FromMinutes (2);
      this.PendingModificationsMaxTime = TimeSpan.FromSeconds (90);
      this.NormalModificationPriority = NORMAL_PRIORITY_DEFAULT;
      this.LowModificationPriority = LOW_PRIORITY_DEFAULT;
      this.LowPriorityFrequency = LOW_PRIORITY_FREQUENCY_DEFAULT;
      this.VeryLowPriorityFrequency = VERY_LOW_PRIORITY_FREQUENCY_DEFAULT;
      this.FactNumber = 50;
      this.ProcessingReasonSlotsMaxTime = TimeSpan.FromSeconds (30);
      this.DetectionMaxTime = TimeSpan.FromMinutes (1);
      this.AutoSequencesMaxTime = TimeSpan.FromSeconds (40);
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
