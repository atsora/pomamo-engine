// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.GroupZoomFromRegex
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// 
    /// </summary>
    [PluginConf("Text", "Parent regex", Description = "regex for the parent group id")]
    public string ParentRegex {
      get; set;
    }

    /// <summary>
    /// 
    /// </summary>
    [PluginConf ("Text", "Children regex", Description = "regex for the ghildren group ids. It may contains the special keyword {ParentGroupId} that will be replaced by the effective parent group id or parent regex")]
    public string ChildrenRegex
    {
      get; set;
    }

    [PluginConf ("Bool", "Include the virtual children groups", Description = "When searching for children, include the virtual groups. Note that the regex must be more strict when this option is on (? are not recommended)")]
    public bool IncludeVirtualChildren
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
      if (null == this.ParentRegex) {
        errorList.Add ("Parent regex is not defined");
      }
      if (string.IsNullOrEmpty (this.ChildrenRegex)) {
        errorList.Add ("Chidlren regex is not defined");
      }
      errors = errorList;
      return true;
    }
    #endregion // Constructors
  }
}
