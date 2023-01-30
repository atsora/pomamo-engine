// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System;

namespace Lemoine.Plugin.AutoReasonStopSequence
{
  public sealed class Configuration
    : Lemoine.Extensions.AutoReason.AutoReasonConfiguration
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Add a condition for validating the autoreason: the sequence name must match the filter
    /// Can be empty
    /// </summary>
    [PluginConf ("Text", "Filter", Description = "Add a condition for validating the autoreason: the sequence name must match the filter. Can remain empty", Parameters = "", Multiple = false, Optional = true)]
    public String Filter { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration () {}
    #endregion // Constructors
  }
}
