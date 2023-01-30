// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System.Collections.Generic;

namespace Lemoine.Plugin.GroupSingleMachine
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Category name
    /// </summary>
    [PluginConf ("Text", "Group Category Name", Description = "a name for the group")]
    public string GroupCategoryName
    {
      get; set;
    } = "";

    /// <summary>
    /// Prefix to build the Ids
    /// </summary>
    [PluginConf ("Text", "ID prefix", Description = "a prefix for the ID")]
    public string GroupCategoryPrefix
    {
      get; set;
    } = "";

    /// <summary>
    /// Sort priority of the group category
    /// </summary>
    [PluginConf ("Double", "Sort priority", Description = "a sort priority")]
    public double GroupCategorySortPriority
    {
      get; set;
    } = 0.0;
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
      errors = errorList;
      return true;
    }
    #endregion // Constructors
  }
}
