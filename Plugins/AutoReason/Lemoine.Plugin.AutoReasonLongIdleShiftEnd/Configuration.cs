// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System;

namespace Lemoine.Plugin.AutoReasonLongIdleShiftEnd
{
  public sealed class Configuration
    : Lemoine.Extensions.AutoReason.AutoReasonConfiguration
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Type of dynamic end for the autoreasons
    /// Can be empty
    /// </summary>
    [PluginConf ("Text", "DynamicEnd", Description = "Select the type of dynamic end that will close the autoreason (NextMachineMode, NextProductionStart, ...)", Parameters = "NextMachineMode", Multiple = false, Optional = false)]
    public String DynamicEnd { get; set; }

    /// <summary>
    /// Machine mode id to track
    /// </summary>
    [PluginConf ("MachineMode", "Machine mode", Description = "a machine mode to track", Multiple = false, Optional = false)]
    public int MachineModeId { get; set; }

    /// <summary>
    /// Minimum duration of the non-productive period for triggering the reason
    /// 
    /// Default: 1 hour
    /// </summary>
    [PluginConf ("DurationPicker", "Minimum duration", Description = "minimum duration of the non-productive period for triggering the reason", Parameters = "1:00:00", Multiple = false, Optional = false)]
    public TimeSpan MinDuration { get; set; }

    /// <summary>
    /// Maximum duration of a gap between facts
    /// </summary>
    [PluginConf ("DurationPicker", "Maximum duration of a gap", Description = "maximum duration of a gap that will be attached to the previous machine mode", Parameters = "0:02:00", Multiple = false, Optional = false)]
    public TimeSpan MaxGapDuration { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration () { }
    #endregion // Constructors
  }
}
