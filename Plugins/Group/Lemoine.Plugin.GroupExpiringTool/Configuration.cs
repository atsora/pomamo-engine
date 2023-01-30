// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Lemoine.Plugin.GroupExpiringTool
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Category name
    /// </summary>
    [PluginConf ("Text", "Group Category Name", Description = "a name for the group. Default: Expiring tool")]
    [DefaultValue ("Expiring tool")]
    public string GroupCategoryName
    {
      get; set;
    } = "Expiring tool";

    /// <summary>
    /// Prefix to build the Ids
    /// </summary>
    [PluginConf ("Text", "Group Id", Description = "Group Id. Default: ET")]
    [DefaultValue ("ET")]
    public string GroupCategoryPrefix
    {
      get; set;
    } = "ET";

    /// <summary>
    /// Sort priority of the group category
    /// </summary>
    [PluginConf ("Double", "Sort priority", Description = "a sort priority. Default: 10.0")]
    [DefaultValue (10.0)]
    public double GroupCategorySortPriority
    {
      get; set;
    } = 10.0;

    /// <summary>
    /// Maximum remaining time of the tool lifes
    /// </summary>
    [PluginConf ("TimeSpan", "Maximum remaining time", Description = "a maximum remaining time. Default: 1:00:00")]
    [DefaultValue ("1:00:00")]
    public TimeSpan MaxRemainingDuration
    {
      get; set;
    } = TimeSpan.FromHours (1);
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
      if (string.IsNullOrEmpty (this.GroupCategoryPrefix)) {
        errorList.Add ("ID Prefix is empty");
      }
      errors = errorList;
      return true;
    }
    #endregion // Constructors
  }
}
