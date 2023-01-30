// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Pulse.Extensions.Database;
using System;
using System.ComponentModel;

namespace Lemoine.Plugin.StampDetection
{
  /// <summary>
  /// Description of Configuration.
  /// </summary>
  public class Configuration
    : Pulse.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter
    , IOperationDetectionStatusConfiguration
    , ICycleDetectionStatusConfiguration
  {
    [PluginConf ("Int", "Cycle detection status priority", Description = "Cycle detection status priority", Parameters = "100")]
    public int CycleDetectionStatusPriority { get; set; }

    [PluginConf ("Int", "Operation detection status priority", Description = "Operation detection status priority", Parameters = "100")]
    public int OperationDetectionStatusPriority { get; set; }

    /// <summary>
    /// May the sequence variable include a milestone in its decimal part in additional to a stamp id?
    /// 
    /// Default: true
    /// </summary>
    [PluginConf ("Bool", "Include milestone", Description = "May the sequence variable include a milestone in its decimal part in additional to a stamp id")]
    [DefaultValue (true)]
    public bool IncludeMilestone { get; set; } = true;

    protected override bool IsMachineFilterRequired ()
    {
      return false;
    }
  }
}
