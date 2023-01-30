// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration.GuiBuilder;
using System.Collections.Generic;
using System.ComponentModel;

namespace Lemoine.Plugin.ComingEventStop
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    /// <summary>
    /// Category name
    /// </summary>
    [PluginConf ("Text", "Message", Description = "message of the coming event. Default is STOP in", Multiple = false, Optional = false)]
    [DefaultValue ("STOP in")]
    public string Message
    {
      get; set;
    } = "STOP in";

    /// <summary>
    /// Sort priority of the group category
    /// </summary>
    [PluginConf ("MachineFilter", "Machine filter", Description = "optionally an application machine filter", Multiple = false, Optional = true)]
    public int MachineFilterId
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
      if (string.IsNullOrEmpty (this.Message)) {
        errorList.Add ("Message is empty");
      }
      errors = errorList;
      return true;
    }
    #endregion // Constructors
  }
}
