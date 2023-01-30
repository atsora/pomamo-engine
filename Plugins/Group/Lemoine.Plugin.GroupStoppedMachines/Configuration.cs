// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Lemoine.Plugin.GroupStoppedMachines
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    /// <summary>
    /// Category name
    /// </summary>
    [PluginConf ("Text", "Group Category Name", Description = "a name for the group")]
    [DefaultValue ("Stopped machines")]
    public string GroupCategoryName
    {
      get; set;
    } = "Stopped machines";

    /// <summary>
    /// Prefix to build the Ids
    /// </summary>
    [PluginConf ("Text", "Group Id", Description = "Group Id")]
    [DefaultValue ("SM")]
    public string GroupCategoryPrefix
    {
      get; set;
    } = "SM";

    /// <summary>
    /// Sort priority of the group category
    /// </summary>
    [PluginConf ("Double", "Sort priority", Description = "a sort priority")]
    public double GroupCategorySortPriority
    {
      get; set;
    }

    /// <summary>
    /// Maximum remaining time of the tool lifes
    /// </summary>
    [PluginConf ("DurationPicker", "Minimum stop duration", Description = "a minimum stop duration")]
    public TimeSpan MinimumStopDuration
    {
      get; set;
    }

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration ()
    {
      this.GroupCategoryName = "StoppedMachines";
      this.GroupCategoryPrefix = "SM";
      this.GroupCategorySortPriority = 10.0;
      this.MinimumStopDuration = TimeSpan.FromMinutes (15);
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
