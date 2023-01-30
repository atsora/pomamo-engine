// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System;

namespace Lemoine.Plugin.AutoReasonStopBetweenCycles
{
  public sealed class Configuration
    : Lemoine.Extensions.AutoReason.AutoReasonConfiguration
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Limit between a "normal" stop between cycles and an "extended" stop between cycles
    /// If null, all stops are considered as "normal"
    /// Default is null
    /// </summary>
    [PluginConf ("DurationPicker", "Extended period", Description = "if extended stops having periods longer than the trigger must be created", Parameters = "0:00:00", Multiple = false, Optional = true)]
    public TimeSpan? ExtendedPeriod { get; set; }

    /// <summary>
    /// Break associated reason id for an extended stop. If 0, consider a default reason
    /// </summary>
    [PluginConf ("Reason", "Reason for extended stops", Description = "reason for extended stops", Multiple = false, Optional = true)]
    public int ExtendedReasonId { get; set; }

    /// <summary>
    /// Reason score for long break between cycle because another reason could be more accurate
    /// (for example a late start or early end)
    /// Default is 50.0 (normal stops between cycle is 65)
    /// </summary>
    [PluginConf ("DoubleAsNumericUpDown", "Reason score for extended stops", Description = "reason score for extended stops", Parameters = "100000:3", Multiple = false, Optional = false)]
    public double ReasonScoreExtended { get; set; }
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
