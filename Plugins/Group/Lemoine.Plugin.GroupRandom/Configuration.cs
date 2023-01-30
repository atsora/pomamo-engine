// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System.Collections.Generic;
using System.ComponentModel;

namespace Lemoine.Plugin.GroupRandom
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    /// <summary>
    /// Category name
    /// </summary>
    [PluginConf ("Text", "Group Category Name", Description = "a name for the group category. Default: Random")]
    [DefaultValue ("Random")]
    public string GroupCategoryName
    {
      get; set;
    } = "Random";

    /// <summary>
    /// Prefix to build the Ids
    /// </summary>
    [PluginConf ("Text", "Group Id", Description = "Group Id. Default: Random")]
    [DefaultValue ("Random")]
    public string GroupCategoryPrefix
    {
      get; set;
    } = "Random";

    /// <summary>
    /// Category reference
    /// </summary>
    [PluginConf ("Text", "Group Category Reference", Description = "an id for the group category")]
    public string GroupCategoryReference
    {
      get; set;
    }

    /// <summary>
    /// Sort priority of the group category
    /// </summary>
    [PluginConf ("Double", "Sort priority", Description = "a sort priority. Default: 90.0")]
    [DefaultValue (90.0)]
    public double GroupCategorySortPriority
    {
      get; set;
    } = 90.0;

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
        errorList.Add ("Group Category Reference is not defined");
      }
      if (string.IsNullOrEmpty (this.GroupCategoryPrefix)) {
        errorList.Add ("ID Prefix is empty");
      }
      errors = errorList;
      return true;
    }
    #endregion // Constructors
  }
}
