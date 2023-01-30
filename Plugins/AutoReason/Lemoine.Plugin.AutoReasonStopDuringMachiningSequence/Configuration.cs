// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System;

namespace Lemoine.Plugin.AutoReasonStopDuringMachiningSequence
{
  public sealed class Configuration
    : Lemoine.Extensions.AutoReason.AutoReasonConfiguration
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Filter to target specific sequences
    /// By default, this field is empty and all sequences are analyzed
    /// </summary>
    [PluginConf ("Text", "SequenceNameRegex", Description = "Filter to target specific sequences. By default, this field is empty and all sequences are analyzed.", Parameters = "SequenceNameRegex", Multiple = false, Optional = true)]
    public String SequenceNameRegex { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration () {}
    #endregion // Constructors
  }
}
