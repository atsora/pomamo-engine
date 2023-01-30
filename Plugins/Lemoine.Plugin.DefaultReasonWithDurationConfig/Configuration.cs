// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration.GuiBuilder;

namespace Lemoine.Plugin.DefaultReasonWithDurationConfig
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    /// <summary>
    /// Parameter to turn off the plugin
    /// </summary>
    [PluginConf ("Bool", "Turn off", Description = "If this parameter is on on one of the configurations, then the plugin is turned off", Multiple = false, Optional = false)]
    [DefaultValue (false)]
    public bool TurnOff { get; set; } = false;

    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration ()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public bool IsValid (out IEnumerable<string> errors)
    {
      var errorList = new List<string> ();

      errors = errorList;
      return true;
    }
  }
}
