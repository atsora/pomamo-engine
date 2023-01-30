// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System.Collections.Generic;
using System.ComponentModel;

namespace Lemoine.Plugin.GroupDepartment
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    /// <summary>
    /// Category name
    /// </summary>
    [PluginConf ("Text", "Group Category Name", Description = "a name for the group. Default: Department")]
    [DefaultValue ("Department")]
    public string GroupCategoryName
    {
      get; set;
    } = "Department";

    /// <summary>
    /// Prefix to build the Ids
    /// </summary>
    [PluginConf ("Text", "ID prefix", Description = "a prefix for the ID. Default: D")]
    [DefaultValue ("D")]
    public string GroupCategoryPrefix
    {
      get; set;
    } = "D";

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
    [PluginConf ("Bool", "Allow zoom in machine selection", Description = "If set, each company is zoomable in the machine selection")]
    public bool ZoomInMachineSelection
    {
      get; set;
    }

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
