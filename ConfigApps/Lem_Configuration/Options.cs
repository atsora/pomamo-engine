// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;

using CommandLine;
using CommandLine.Text;
using Lemoine.Info;
using Lemoine.Core.Log;
using Lemoine.Core.Options;

namespace Lem_Configuration
{
  /// <summary>
  /// Options of Lem_Configuration
  /// </summary>
  internal class Options: IMicrosoftParameters
  {
    static readonly ILog log = LogManager.GetLogger(typeof (Options).FullName);

    #region Options
    [Option ('a', "advanced", HelpText = "Advanced more, requires a password")]
    public bool Advanced { get; set; } = false;
    
    [Option ('w', "password", HelpText = "Password for the advanced mode")]
    public string Password { get; set; } = "";

    /// <summary>
    /// Additional parameters
    /// 
    /// The syntax is: Key1=Value1 Key2=Value2
    /// </summary>
    [Option ('p', "parameters", Required = false, HelpText = "Additional parameters")]
    public IEnumerable<string> Parameters { get; set; } = new List<string> ();

    /// <summary>
    /// <see cref="IMicrosoftParameters"/>
    /// 
    /// Additional microsoft parameters
    /// 
    /// The syntax is:
    /// <item>Key1=Value1 Key2=Value2</item>
    /// <item>/Key1 Value1 /Key2 Value2</item>
    /// <item>/Key1=Value1 /Key2=Value2 /Key3=</item>
    /// 
    /// Note that the syntax --Key1 Value1 or --Key1==Value1 is not supported
    /// </summary>
    [Option ('m', "config", Required = false, HelpText = "Additional Microsoft configuration parameters")]
    public IEnumerable<string> MicrosoftParameters { get; set; } = new List<string> ();
    #endregion
  }
}
