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

namespace Lem_Stamper.Console
{
  /// <summary>
  /// Description of Options.
  /// </summary>
  internal class Options: IMicrosoftParameters
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Options).FullName);

    #region Options
    /// <summary>
    /// Additional parameters
    /// 
    /// The syntax is: Key1=Value1 Key2=Value2
    /// </summary>
    [Option ('k', "parameters", Required = false, HelpText = "Additional parameters")]
    public IEnumerable<string> Parameters { get; set; } = new List<string> ();

    /// <summary>
    /// Additional microsoft parameters
    /// 
    /// <see cref="IMicrosoftParameters"/>
    /// 
    /// The syntax is:
    /// <item>Key1=Value1 Key2=Value2</item>
    /// <item>/Key1 Value1 /Key2 Value2</item>
    /// <item>/Key1=Value1 /Key2=Value2 /Key3=</item>
    /// 
    /// Note that the syntax --Key1 Value1 or --Key1==Value1 is not supported
    /// </summary>
    [Option ('m', "msconfig", Required = false, HelpText = "Additional Microsoft configuration parameters")]
    public IEnumerable<string> MicrosoftParameters { get; set; } = new List<string> ();

    // Note: -c or -n must be set, that's why Group="config" is set
    [Option ('c', "config", Group="config", HelpText = "Configuration file path")]
    public string ConfigFilePath { get; set; } = "";

    [Option ('n', "name", Group="config", HelpText = "Stamping configuration name")]
    public string ConfigName { get; set; } = "";

    [Option ('i', "input", HelpText = "Input NC program path")]
    public string Input { get; set; } = "";

    [Option ('o', "output", HelpText = "Output directory path")]
    public string Output { get; set; } = "";

    /// <summary>
    /// Initial stamping data
    /// 
    /// The syntax is: Key1=Value1 Key2=Value2
    /// </summary>
    [Option ('s', "data", Required = false, HelpText = "Stamping data")]
    public IEnumerable<string> Data { get; set; } = new List<string> ();

    [Option ('t', "trace", HelpText = "Enable the trace")]
    public bool TraceEnabled { get; set; } = false;
    #endregion // Options
  }
}
