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

namespace Lemoine.Plugin.GroupJob
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    /// <summary>
    /// Category name
    /// </summary>
    [PluginConf ("Text", "Group Category Name", Description = "a name for the group. Default: Job")]
    [DefaultValue ("Job")]
    public string GroupCategoryName
    {
      get; set;
    } = "Job";

    /// <summary>
    /// Prefix to build the Ids
    /// </summary>
    [PluginConf ("Text", "ID prefix", Description = "a prefix for the ID. Default: J")]
    [DefaultValue ("J")]
    public string GroupCategoryPrefix
    {
      get; set;
    } = "J";

    /// <summary>
    /// Category Reference
    /// </summary>
    [PluginConf ("Text", "Group Category Reference", Description = "a name for the category reference. Default: Job")]
    [DefaultValue ("Job")]
    public string GroupCategoryReference
    {
      get; set;
    } = "Job";

    /// <summary>
    /// Sort priority of the group category
    /// </summary>
    [PluginConf ("Double", "Sort priority", Description = "a sort priority. Default: 11.0")]
    [DefaultValue (11.0)]
    public double GroupCategorySortPriority
    {
      get; set;
    } = 11.0;

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
  }
}
