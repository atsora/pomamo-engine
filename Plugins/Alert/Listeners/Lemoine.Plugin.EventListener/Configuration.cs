// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration.GuiBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Plugin.EventListener
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    // TODO: Add here parameters, for example:
    /* 
    /// <summary>
    /// My parameter
    /// </summary>
    [PluginConf("Text", "My parameter", Description = "full description. Default: DefaultParameter")]
    [DefaultValue ("DefaultParameter")]
    public string MyParameter {
      get; set;
    } = "DefaultParameter";
    */
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
      // TODO: check the parameters here, for example:
      /* 
      if (string.IsNullOrEmpty (this.MyParameter)) {
        errorList.Add ("ID Prefix is empty");
      }
      */
      errors = errorList;
      return true;
    }
    #endregion // Constructors
  }
}
