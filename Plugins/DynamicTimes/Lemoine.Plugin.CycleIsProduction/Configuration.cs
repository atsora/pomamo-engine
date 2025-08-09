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

namespace Lemoine.Plugin.CycleIsProduction
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
    public string NamePrefix
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
      return base.IsValid (out errors);
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
