// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration.GuiBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Lemoine.Plugin.GroupFocusedAlarms
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Category name
    /// </summary>
    [PluginConf ("Text", "Group Category Name", Description = "a name for the group. Default: Focused alarms")]
    [DefaultValue ("Focused alarms")]
    public string GroupCategoryName
    {
      get; set;
    } = "Focused alarms";

    /// <summary>
    /// Prefix to build the Ids
    /// </summary>
    [PluginConf ("Text", "ID prefix", Description = "a prefix for the ID. Default: FA")]
    [DefaultValue ("FA")]
    public string GroupCategoryPrefix
    {
      get; set;
    } = "FA";

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
    /// Duration of the interval before now to detect alarms
    /// 
    /// Default is 2 minutes
    /// </summary>
    [PluginConf ("TimeSpan", "Maximum remaining time", Description = "a maximum remaining time. Default: 0:02:00")]
    [DefaultValue ("0:02:00")]
    public TimeSpan DetectionIntervalDuration
    {
      get; set;
    } = TimeSpan.FromMinutes (2);
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
