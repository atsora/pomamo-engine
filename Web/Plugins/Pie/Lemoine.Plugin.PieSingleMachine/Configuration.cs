// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.PieSingleMachine
{
  public sealed class Configuration
    : Pulse.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    /// <summary>
    /// Score
    /// </summary>
    [PluginConf ("Double", "Score", Description = "score to give to this plugin", Multiple = false, Optional = false)]
    public double Score
    {
      get; set;
    }

    /// <summary>
    /// Pie type
    /// 
    /// If empty, no pie is displayed
    /// </summary>
    [PluginConf ("Text", "Pie type", Description = "set here the pie that must be associated to the matching machines", Multiple = false, Optional = false)]
    public string PieType
    {
      get; set;
    }

    protected override bool IsMachineFilterRequired () => false;
  }
}
