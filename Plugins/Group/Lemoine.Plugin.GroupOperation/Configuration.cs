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

namespace Lemoine.Plugin.GroupOperation
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    /// <summary>
    /// Category name
    /// </summary>
    [PluginConf ("Text", "Group Category Name", Description = "a name for the group. Default: Operation")]
    [DefaultValue ("Operation")]
    public string GroupCategoryName
    {
      get; set;
    } = "Operation";

    /// <summary>
    /// Prefix to build the Ids
    /// </summary>
    [PluginConf ("Text", "ID prefix", Description = "a prefix for the ID. Default: OP")]
    [DefaultValue ("OP")]
    public string GroupCategoryPrefix
    {
      get; set;
    } = "OP";

    /// <summary>
    /// Category Reference
    /// </summary>
    [PluginConf ("Text", "Group Category Reference", Description = "a name for the category reference. Default: Operation")]
    [DefaultValue ("Operation")]
    public string GroupCategoryReference
    {
      get; set;
    } = "Operation";

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
    /// Omit the groups in the machine selection
    /// </summary>
    [DefaultValue (true)]
    [PluginConf ("Bool", "Omit in machine selection", Description = "Omit the groups in the machine selection. Default: true")]
    public bool OmitInMachineSelection
    {
      get; set;
    } = true;

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
