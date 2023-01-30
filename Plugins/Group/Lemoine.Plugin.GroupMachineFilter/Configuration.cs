// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.GroupMachineFilter
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Category name
    /// </summary>
    [PluginConf ("Text", "Group Category Name", Description = "a name for the group category")]
    public string GroupCategoryName
    {
      get; set;
    }

    /// <summary>
    /// Sort priority of the group category
    /// </summary>
    [PluginConf ("Double", "Group category sort priority", Description = "a sort priority for the group category")]
    public double GroupCategorySortPriority
    {
      get; set;
    }

    /// <summary>
    /// Category reference
    /// </summary>
    [PluginConf ("Text", "Group Category Reference", Description = "an id for the group category")]
    public string GroupCategoryReference
    {
      get; set;
    }

    /// <summary>
    /// Omit the groups in the machine selection
    /// </summary>
    [PluginConf ("Bool", "Omit in machine selection", Description = "Omit the groups in the machine selection")]
    public bool OmitInMachineSelection
    {
      get; set;
    }

    /// <summary>
    /// Allow zoom in machine selection
    /// </summary>
    [PluginConf ("Bool", "Allow zoom in machine selection", Description = "If set, zoomable in the machine selection")]
    public bool ZoomInMachineSelection
    {
      get; set;
    }

    /// <summary>
    /// Group id
    /// </summary>
    [PluginConf ("Text", "Group ID", Description = "the group ID")]
    public string GroupId
    {
      get; set;
    }

    /// <summary>
    /// Category name
    /// </summary>
    [PluginConf ("Text", "Group Name", Description = "a name for the group")]
    public string GroupName
    {
      get; set;
    }

    /// <summary>
    /// Sort priority of the group within a same group category
    /// </summary>
    [PluginConf ("Double", "Group sort priority", Description = "a sort priority for the group within a same group category")]
    public double GroupSortPriority
    {
      get; set;
    }

    /// <summary>
    /// Machine filter
    /// </summary>
    [PluginConf ("MachineFilter", "Machine filter", Description = "restrict to this machine filter (optional)", Optional = true, Multiple = false)]
    public int MachineFilterId
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
    public bool IsValid (out IEnumerable<string> errors)
    {
      var errorList = new List<string> ();
      if (string.IsNullOrEmpty (this.GroupCategoryReference)) {
        errorList.Add ("Group category reference is not defined");
      }
      if (string.IsNullOrEmpty (this.GroupId)) {
        errorList.Add ("Group ID is not defined");
      }
      if (string.IsNullOrEmpty (this.GroupName)) {
        errorList.Add ("No group name is defined");
      }
      errors = errorList;
      return true;
    }
    #endregion // Constructors
  }
}
