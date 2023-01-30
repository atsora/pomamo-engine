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

namespace Lemoine.Plugin.GroupEmergencyWithAlarmUntilProductionStart
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    /// <summary>
    /// Category name
    /// </summary>
    [PluginConf ("Text", "Group Category Name", Description = "a name for the group. Default: Stopped by emergency stop and alarm")]
    [DefaultValue ("Stopped by emergency stop and alarm")]
    public string GroupName
    {
      get; set;
    } = "Stopped by emergency stop and alarm";

    /// <summary>
    /// Prefix to build the Ids
    /// </summary>
    [PluginConf ("Text", "ID prefix", Description = "a prefix for the ID. Default: ESA")]
    [DefaultValue ("ESA")]
    public string GroupId
    {
      get; set;
    } = "ESA";

    /// <summary>
    /// Sort priority of the group category
    /// </summary>
    [PluginConf ("Double", "Sort priority", Description = "a sort priority. Default: 10.0")]
    [DefaultValue (10.0)]
    public double GroupCategorySortPriority
    {
      get; set;
    } = 10.0;

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
      if (string.IsNullOrEmpty (this.GroupId)) {
        errorList.Add ("ID Prefix is empty");
      }
      errors = errorList;
      return true;
    }
    #endregion // Constructors
  }
}
