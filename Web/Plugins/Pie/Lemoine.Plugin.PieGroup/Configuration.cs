// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.PieGroup
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Score
    /// </summary>
    [PluginConf ("Double", "Score", Description = "score to give to this plugin", Multiple = false, Optional = false)]
    public double Score
    {
      get; set;
    }

    /// <summary>
    /// Pie type
    /// 
    /// If empty, no pie is displayed
    /// </summary>
    [PluginConf ("Text", "Pie type", Description = "set here the pie that must be associated to the matching machines", Multiple = false, Optional = false)]
    public string PieType
    {
      get; set;
    }

    /// <summary>
    /// Regex for the group (not empty)
    /// </summary>
    [PluginConf ("Text", "Group Regex", Description = "a regex to filter the groups (not empty)", Multiple = false, Optional = false)]
    public string GroupRegex
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

    public bool IsValid (out IEnumerable<string> errors)
    {
      var listOfErrors = new List<string> ();

      if (string.IsNullOrEmpty (this.GroupRegex)) {
        var message = "GroupRegex is empty";
        if (log.IsErrorEnabled) {
          log.ErrorFormat ("IsValid: {0}", message);
        }
        listOfErrors.Add (message);
      }

      errors = listOfErrors;
      return !errors.Any ();
    }
    #endregion // Constructors
  }
}
