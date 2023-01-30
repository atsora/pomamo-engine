// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System.ComponentModel;

namespace Lemoine.Plugin.AutoReasonRedStackLight
{
  public sealed class Configuration
    : Lemoine.Extensions.AutoReason.AutoReasonConfiguration
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Write all alarms in the details of the reason
    /// </summary>
    [PluginConf ("Bool", "Write all alarms", Description = "Write all alarms in the details of the reason", Parameters = "", Multiple = false, Optional = true)]
    [DefaultValue (false)]
    public bool WriteAllAlarms { get; set; } = false;

    /// <summary>
    /// Limit the auto-reason trigger to cases where only the red light is on or flashing
    /// </summary>
    [PluginConf ("Bool", "Red light only", Description = "Limit the auto-reason trigger when only the red light is on or flashing. Default: false", Parameters = "", Multiple = false, Optional = true)]
    [DefaultValue (false)]
    public bool RedOnly { get; set; } = false;
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration () { }
    #endregion // Constructors
  }
}
