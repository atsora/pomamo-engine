// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System.Collections.Generic;
using System.ComponentModel;

namespace Lemoine.Plugin.CncAlarmByMessageAlert
{
  public  class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters

    [PluginConf ("Bool", "Filter by message (otherwise by number)", Description = "Indicates if filter must be done on the message content or on the number content")]
    [DefaultValue (true)]
    public bool FilterByMessage { get; set; } = true;
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
