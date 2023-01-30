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

namespace Lemoine.Plugin.GroupZoomPartOperation
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    /// <summary>
    /// Part group prefix
    /// </summary>
    [PluginConf ("Text", "ID prefix", Description = "Part group prefix. Default: P")]
    [DefaultValue ("P")]
    public string PartPrefix
    {
      get; set;
    } = "P";

    /// <summary>
    /// Operation group prefix
    /// </summary>
    [PluginConf ("Text", "ID prefix", Description = "Operation group prefix. Default: OP")]
    [DefaultValue ("OP")]
    public string OperationPrefix
    {
      get; set;
    } = "OP";

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
      if (string.IsNullOrEmpty (this.PartPrefix)) {
        errorList.Add ("Part Prefix is empty");
      }
      if (string.IsNullOrEmpty (this.OperationPrefix)) {
        errorList.Add ("Operation Prefix is empty");
      }
      errors = errorList;
      return true;
    }
  }
}
