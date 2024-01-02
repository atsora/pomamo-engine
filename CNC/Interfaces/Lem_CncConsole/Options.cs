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

namespace Lem_CncConsole
{
  /// <summary>
  /// Description of Options.
  /// </summary>
  public class Options : IMicrosoftParameters
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

    [Option ('i', "id", HelpText = "ID of the Cnc Acquisition config", Required = true)]
    public int CncAcquisitionId { get; set; } = int.MinValue;

    [Option ('f', "file", HelpText = "Path to a specific XML configuration file. Set -i 0 to use it", Required = false)]
    public string File { get; set; } = "";

    [Option('n', "nparams", HelpText = "Numeric config parameters in a list string to be used with the -f option. For example #192.168.10.5#False for Param1=192.168.10.5 and Param2=False")]
    public string NumParameters { get; set; } = "";

    [Option('j', "jparams", HelpText = "Config parameters in a json dictionary to be used with the -f option. For example {\"Param1\": \"192.168.10.5\", \"Param2\": false}")]
    public string JsonParameters { get; set; } = "";

    [Option ('s', "stamp", HelpText = "Use a stamp file to check the process is still running correctly")]
    public bool Stamp { get; set; } = true;

    [Option ('p', "pid", HelpText = "Optional parent process ID. In case this parent process is not running any more, the application stops")]
    public int ParentProcessId { get; set; } = 0;

    [Option ('e', "every", HelpText = "Set a default every parameter in ms. Default is 2s=2000ms")]
    public int Every { get; set; } = 2000;

    [Option ('a', "stathread", HelpText = "Use a STA thread instead of a MTA thread")]
    public bool StaThread { get; set; } = false;

    [Option ('l', "light", HelpText = "Light application initialization: no database connection, no file repo")]
    public bool Light { get; set; } = false;

    /// <summary>
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

    #endregion // Options
  }
}
