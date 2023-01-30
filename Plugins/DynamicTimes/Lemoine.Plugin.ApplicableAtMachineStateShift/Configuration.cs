// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.ApplicableAtMachineStateShift
{
  public sealed class Configuration
    : Pulse.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter
    , Lemoine.Extensions.Configuration.IConfiguration
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
    /// Dynamic time request name to request (mandatory)
    /// </summary>
    [PluginConf ("Text", "Dynamic time request", Description = "Effective dynamic time to request (mandatory)", Optional = false)]
    public string RedirectName
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
    public override bool IsValid (out IEnumerable<string> errors)
    {
      bool result = base.IsValid (out errors);

      var errorList = new List<string> ();

      if (string.IsNullOrEmpty (this.Name)) {
        errorList.Add ("No name defined");
        result = false;
      }
      if (string.IsNullOrEmpty (this.RedirectName)) {
        errorList.Add ("No redirect name defined");
        result = false;
      }

      errors = errors.Concat (errorList);
      return result;
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="Lemoine.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter"/>
    /// </summary>
    /// <returns></returns>
    protected override bool IsMachineFilterRequired ()
    {
      return false;
    }
  }
}
