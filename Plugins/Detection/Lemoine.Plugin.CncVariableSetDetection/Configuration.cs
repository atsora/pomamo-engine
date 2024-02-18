// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Pulse.Extensions.Database;
using System;
using System.ComponentModel;

namespace Lemoine.Plugin.CncVariableSetDetection
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
    /// Decimal part of the sequence variable that is used for the milestone in minutes
    /// 
    /// For example, if the stamp value is 1234.2345 and this value is 4, then 2345 is the milestone
    /// 
    /// Default: 4
    /// Use 0 to disable it
    /// </summary>
    [PluginConf ("Int", "Milestone part", Description = "Decimal part to consider as a milestone in a sequence variable", Parameters = "10")]
    [DefaultValue (4)]
    public int MilestonePart { get; set; } = 4;

    protected override bool IsMachineFilterRequired () => false;

  }
}
