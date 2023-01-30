// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System;
using System.ComponentModel;

namespace Lemoine.Plugin.AutoReasonToolChange
{
  public sealed class Configuration
    : Lemoine.Extensions.AutoReason.AutoReasonConfiguration
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Right margin to possibly extend the auto reason period
    /// (default: 1)
    /// </summary>
    [PluginConf ("DurationPicker", "Right margin", Description = "a margin after the detection of a tool change if the set DynamicEnd does not end with '+'. Default (if 0:00:00): 10 minutes", Parameters = "0:00:00", Multiple = false, Optional = true)]
    public TimeSpan RightMargin { get; set; }

    /// <summary>
    /// Optional dynamic end
    /// </summary>
    [PluginConf ("Text", "Dynamic end", Description = "Use a dynamic end. Note, you can suffix the dynamic end by + to compute it from the tool change date/time. Default: NextProductionStart", Optional = true)]
    [DefaultValue ("NextProductionStart")]
    public string DynamicEnd { get; set; } = "NextProductionStart";
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration ()
    {
    }
    #endregion // Constructors
  }
}
