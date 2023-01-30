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

namespace Lem_TestDynamicTime.Console
{
  /// <summary>
  /// Description of Options.
  /// </summary>
  public class Options: IMicrosoftParameters
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Options).FullName);

    #region Options
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
    [Option ('c', "config", Required = false, HelpText = "Additional Microsoft configuration parameters")]
    public IEnumerable<string> MicrosoftParameters { get; set; } = new List<string> ();

    [Option ('n', "name", HelpText = "Name of the dynamic time", Required = true)]
    public string Name { get; set; } = "";

    [Option ('m', "machine-id", HelpText = "Machine Id", Required = true)]
    public int MachineId { get; set; } = -1;

    [Option ('t', "time", HelpText = "Start time of the dynamictime in ISO format", Required = true)]
    public string IsoDateTime { get; set; } = "";

    [Option ('i', "lowerhint", HelpText = "Lower hint in ISO format", Required = false)]
    public string LowerHint { get; set; } = "";

    [Option ('j', "upperhint", HelpText = "Upper hint in ISO format", Required = false)]
    public string UpperHint { get; set; } = "";

    [Option ('l', "lowerlimit", HelpText = "Lower limit in ISO format", Required = false)]
    public string LowerLimit { get; set; } = "";

    [Option ('u', "upperlimit", HelpText = "Upper limit in ISO format", Required = false)]
    public string UpperLimit { get; set; }= "";
    #endregion // Options
  }
}
