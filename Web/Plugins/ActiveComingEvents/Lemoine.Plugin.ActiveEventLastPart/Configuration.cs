// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System.ComponentModel;

namespace Lemoine.Plugin.ActiveEventLastPart
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Category name
    /// </summary>
    [PluginConf ("Text", "Message", Description = "message of the active event. Default is LAST PART", Multiple = false, Optional = false)]
    [DefaultValue ("Last part")]
    public string Message
    {
      get; set;
    } = "Last part";

    /// <summary>
    /// Sort priority of the group category
    /// </summary>
    [PluginConf ("MachineFilter", "Machine filter", Description = "optionally an application machine filter", Multiple = false, Optional = true)]
    public int MachineFilterId
    {
      get; set;
    }

    /// <summary>
    /// Warning delay
    /// </summary>
    [PluginConf("DurationPicker", "Warning1 delay", Description = "Delay after a full cycle duration before increasing the severity to Warning1", Multiple = false, Optional = true)]
    public TimeSpan? Warning1Delay
    {
      get; set;
    }

    /// <summary>
    /// Warning delay
    /// </summary>
    [PluginConf ("DurationPicker", "Warning2 delay", Description = "Delay after a full cycle duration before increasing the severity to Warning2", Multiple = false, Optional = true)]
    public TimeSpan? Warning2Delay
    {
      get; set;
    }

    /// <summary>
    /// Error delay
    /// </summary>
    [PluginConf ("DurationPicker", "Error delay", Description = "Delay after a full cycle duration before increasing the severity to Error", Multiple = false, Optional = true)]
    public TimeSpan? ErrorDelay
    {
      get; set;
    }
    #endregion // Getters / Setters

    #region Constructors
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
      if (string.IsNullOrEmpty (this.Message)) {
        errorList.Add ("Message is empty");
      }
      errors = errorList;
      return true;
    }
    #endregion // Constructors
  }
}
