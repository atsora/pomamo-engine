// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System.Collections.Generic;
using System.ComponentModel;

namespace Lemoine.Plugin.ReserveCapacityAlert
{
  public  class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters

    /// <summary>
    /// Report name
    /// </summary>
    [PluginConf ("Text", "Groups", Description = "List of GroupMain or GroupConcurrentGroups", Optional = false, Multiple = false)]
    [DefaultValue ("")]
    public string Groups { get; set; } = "";

    /// <summary>
    /// Category
    /// </summary>
    [PluginConf ("Int", "Threshold", Description = "Threshold for alert sending", Optional = false, Multiple = false)]
    [DefaultValue (0)]
    public int Threshold {  get; set; } = 0;
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
      if (string.IsNullOrEmpty (this.Groups)) {
        errorList.Add ("Group list is empty");
      }
      errors = errorList;
      return true;
    }
    #endregion // Constructors
  }
}
