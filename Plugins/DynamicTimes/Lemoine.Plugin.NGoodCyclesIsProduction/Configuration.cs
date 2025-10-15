// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System.ComponentModel;

namespace Lemoine.Plugin.NGoodCyclesIsProduction
{
  public sealed class Configuration
    : Pulse.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    /// <summary>
    /// [Optional] Name prefix
    /// </summary>
    [PluginConf ("Text", "Dynamic time prefix", Description = "Prefix to add to the dynamic time names")]
    public string NamePrefix {
      get; set;
    }

    /// <summary>
    /// Minimum number of good consecutive cycles to trigger a production start
    /// </summary>
    [PluginConf ("Int", "Number of good cycles", Description = "Minimum number of good consecutive cycles to trigger a production start", Parameters = "20")]
    public int NumberOfGoodCycles {
      get; set;
    }

    /// <summary>
    /// Multiplicator to get the maximum cycle machining duration
    /// </summary>
    [PluginConf ("DoubleAsNumericUpDown", "Machining duration multiplicator", Description = "Multiplicator to get the maximum cycle machining duration", Parameters = "20.00")]
    public double MaxMachiningDurationMultiplicator
    {
      get; set;
    }

    /// <summary>
    /// Multiplicator to get the maximum loading duration
    /// </summary>
    [PluginConf ("DoubleAsNumericUpDown", "Loading duration multiplicator", Description = "Multiplicator to get the maximum loading duration", Parameters = "20.00")]
    public double MaxLoadingDurationMultiplicator
    {
      get; set;
    }

    /// <summary>
    /// Optionally a cache time out
    /// </summary>
    [PluginConf ("DurationPicker", "Cache time out", Description = "Optionally a cache time out", Parameters = "00:00:01", Optional = true)]
    public TimeSpan? CacheTimeOut
    {
      get; set;
    }

    /// <summary>
    /// Optionally an applicable time span
    /// </summary>
    [PluginConf ("DurationPicker", "Applicable time span", Description = "Optionally an applicable time span", Parameters = "1.00:00:00", Optional = true)]
    public TimeSpan? ApplicableTimeSpan
    {
      get; set;
    }

    /// <summary>
    /// Optionally, deactivate the default internal good cycle extension
    /// </summary>
    [PluginConf ("Bool", "Deactive good cycle", Description = "Optionally, deactivate the default internal good cycle extension")]
    [DefaultValue (false)]
    public bool DeactivateGoodCycleExtension
    {
      get; set;
    } = false;

    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration ()
    {
      this.NumberOfGoodCycles = 1;
      this.MaxMachiningDurationMultiplicator = 1.0;
      this.MaxLoadingDurationMultiplicator = 1.0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public override bool IsValid (out IEnumerable<string> errors)
    {
      var result = base.IsValid (out errors);

      var errorList = new List<string> ();
      if (this.NumberOfGoodCycles < 1) {
        errorList.Add ("Invalid number of good cycles (< 1)");
        result = false;
      }
      errors = errors.Concat (errorList);
      return result;
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter"/>
    /// </summary>
    /// <returns></returns>
    protected override bool IsMachineFilterRequired ()
    {
      return false;
    }
  }
}
