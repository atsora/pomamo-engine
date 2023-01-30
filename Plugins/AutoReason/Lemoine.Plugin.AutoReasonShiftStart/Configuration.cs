// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System;

namespace Lemoine.Plugin.AutoReasonShiftStart
{
  public sealed class Configuration
    : Lemoine.Extensions.AutoReason.AutoReasonConfiguration
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Maximum duration of a late start
    /// Default is 1 hour
    /// </summary>
    [PluginConf ("TimeSpan", "Maximum duration", Description = "the maximum duration of a 'late start' detection", Multiple = false, Optional = false)]
    public TimeSpan MaxDuration { get; set; }

    /// <summary>
    /// Period during which it is possible to detect a late start
    /// Default is 15 minutes
    /// </summary>
    [PluginConf ("TimeSpan", "Detection margin", Description = "a margin during which it is possible to detect a late start", Multiple = false, Optional = false)]
    public TimeSpan DetectionMargin { get; set; }
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
