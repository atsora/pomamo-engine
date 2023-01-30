// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System;
using System.ComponentModel;

namespace Lemoine.Plugin.AutoReasonUniqueMachineModeInShift
{
  public sealed class Configuration
    : Lemoine.Extensions.AutoReason.AutoReasonConfiguration
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Track only not running machine mode
    /// </summary>
    [PluginConf ("Bool", "Not Running Machine Mode", Description = "Track only not running machine modes", Optional = true)]
    [DefaultValue (true)]
    public bool NotRunningMachineMode { get; set; }

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
