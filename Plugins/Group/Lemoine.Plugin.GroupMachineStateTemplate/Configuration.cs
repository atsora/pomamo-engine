// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.GroupMachineStateTemplate
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    /// <summary>
    /// Category name
    /// </summary>
    [PluginConf ("Text", "Group Category Name", Description = "a name for the group")]
    public string GroupCategoryName
    {
      get; set;
    }

    /// <summary>
    /// Prefix to build the Ids
    /// </summary>
    [PluginConf ("Text", "ID prefix", Description = "a prefix for the ID")]
    public string GroupCategoryPrefix
    {
      get; set;
    }

    /// <summary>
    /// Sort priority of the group category
    /// </summary>
    [PluginConf ("Double", "Sort priority", Description = "a sort priority")]
    public double GroupCategorySortPriority
    {
      get; set;
    }

    /// <summary>
    /// Machine state templates
    /// </summary>
    [PluginConf ("MachineStateTemplate", "Machine state templates", Description = "Machine state templates to allow for this group", Multiple = true)]
    public IEnumerable<int> MachineStateTemplateIds
    {
      get; set;
    }

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration ()
    {
      this.GroupCategoryName = "Specific machine state template";
      this.GroupCategoryPrefix = "MST";
      this.GroupCategorySortPriority = 90.0;
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
