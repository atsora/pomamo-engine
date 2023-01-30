// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.AfterDynamicTime
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Name prefix (mandatory)
    /// </summary>
    [PluginConf ("Text", "Dynamic time name", Description = "Name of the dynamic time (mandatory)", Optional = false)]
    public string Name
    {
      get; set;
    }

    /// <summary>
    /// Effective dynamic time to request after the other dynamic time is known (mandatory)
    /// </summary>
    [PluginConf ("Text", "Dynamic time request", Description = "Effective dynamic time to request after the other dynamic time is known (mandatory)", Optional = false)]
    public string RedirectName
    {
      get; set;
    }

    /// <summary>
    /// Dynamic time after which the other dynamic time must be effective
    /// </summary>
    [PluginConf ("Text", "After name", Description = "Dynamic time name after which the other dynamic time must be effective (mandatory). If hint does not contain dateTime, then this is skipped", Optional = false)]
    public string AfterName
    {
      get; set;
    }

    /// <summary>
    /// Continue if the after dynamic time returns not applicable
    /// </summary>
    [PluginConf ("Bool", "After not applicable ok", Description = "Continue if the after dynamic time returns not applicable", Optional = true)]
    [DefaultValue (true)]
    public bool AfterNotApplicableOk
    {
      get; set;
    } = true;

    /// <summary>
    /// Optionally a time for the After dynamic time after which NotApplicable is returned
    /// </summary>
    [PluginConf ("DurationPicker", "After max duration", Description = "Optionally a time for the After dynamic time after which NotApplicable is returned", Parameters = "00:00:01", Optional = true)]
    public TimeSpan? AfterMaxDuration
    {
      get; set;
    } = null;
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
      bool result = true;

      var errorList = new List<string> ();

      if (string.IsNullOrEmpty (this.Name)) {
        errorList.Add ("No name defined");
        result = false;
      }
      if (string.IsNullOrEmpty (this.RedirectName)) {
        errorList.Add ("No redirect name defined");
        result = false;
      }
      if (string.IsNullOrEmpty (this.AfterName)) {
        errorList.Add ("No after defined");
        result = false;
      }

      errors = errorList;
      return result;
    }
    #endregion // Constructors
  }
}
