// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

using CommandLine;
using Lemoine.Core.Log;
using Lemoine.Core.Options;

namespace ReportWizardCli
{
  /// <summary>
  /// Description of Options.
  /// </summary>
  public class Options : IMicrosoftParameters
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Options).FullName);

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

    [Option ('t', "trace", Required = false, HelpText = "Enable the trace")]
    public bool TraceEnabled { get; set; } = false;

    /// <summary>
    /// Path of the .prw file
    /// </summary>
    [Value (0, MetaName = "FilePath", HelpText = "prw file path (*.prw)", Required = true)]
    public string FilePath { get; set; } = "";
  }
}
