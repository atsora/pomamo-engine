// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.SwitchByDetectedCyclesProduction
{
  public sealed class Configuration
    : Pulse.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// [Optional] Name prefix
    /// </summary>
    [PluginConf ("Text", "Dynamic time prefix", Description = "Prefix to add to the dynamic time names")]
    public string NamePrefix
    {
      get; set;
    }

    /// <summary>
    /// 
    /// </summary>
    [PluginConf ("Text", "With cycles prefix", Description = "Dynamic time prefix if cycles are detected in the last defined period")]
    public string WithCyclesPrefix
    {
      get; set;
    }

    /// <summary>
    /// 
    /// </summary>
    [PluginConf ("Text", "No cycle prefix", Description = "Dynamic time prefix if no cycle is detected in the last defined period")]
    public string NoCyclePrefix
    {
      get; set;
    }

    /// <summary>
    /// Last period to check whether cycles exist or not
    /// </summary>
    [PluginConf ("DurationPicker", "Period with cycles", Description = "Period when to check cycles", Parameters = "1.00:00:00")]
    public TimeSpan Period
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
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public override bool IsValid (out IEnumerable<string> errors)
    {
      bool result = base.IsValid (out errors);

      var errorList = new List<string> ();

      if (string.IsNullOrEmpty (this.WithCyclesPrefix)) {
        errorList.Add ("No prefix defined if cycles are detected");
        result = false;
      }
      if (string.IsNullOrEmpty (this.NoCyclePrefix)) {
        errorList.Add ("No prefix defined if no cycle is detected");
        result = false;
      }

      errors = errors.Concat (errorList);
      return result;
    }
    #endregion // Constructors

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
