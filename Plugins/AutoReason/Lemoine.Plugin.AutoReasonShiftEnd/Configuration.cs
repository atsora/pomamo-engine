// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System;

namespace Lemoine.Plugin.AutoReasonShiftEnd
{
  public sealed class Configuration
    : Lemoine.Extensions.AutoReason.AutoReasonConfiguration
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Maximum duration of an early end
    /// Default is 1 hour
    /// </summary>
    [PluginConf ("DurationPicker", "Maximum duration", Description = "the maximum duration of an 'early end' detection", Parameters="0:00:00", Multiple = false, Optional = false)]
    public TimeSpan MaxDuration { get; set; }
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
