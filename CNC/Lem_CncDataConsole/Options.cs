// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Reflection;

using CommandLine;
using CommandLine.Text;
using Lemoine.Info;
using Lemoine.Core.Log;
using System.Collections.Generic;
using Lemoine.Core.Options;

namespace Lem_CncDataConsole
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

    [Option ('i', "id",
            HelpText = "ID of the machine module", Required = true)]
    public int MachineModuleId { get; set; } = int.MinValue;

    [Option ('s', "stamp",
            HelpText = "Use a stamp file to check the process is still running correctly")]
    public bool Stamp { get; set; } = true;

    [Option ('p', "pid",
            HelpText = "Optional parent process ID. In case this parent process is not running any more, the application stops")]
    public int ParentProcessId { get; set; } = 0;

    [Option ('z', "sleep",
            HelpText = "Time in ms to sleep once all the work is done. Default is 2s=2000ms")]
    public int SleepTimeMs {get; set; } = -1;

    [Option ('b', "break-frequency",
            HelpText = "Frequency in ms at which a break is done. Default is 2s=2000ms")]
    public int BreakFrequency { get; set; } = -1;

    [Option ('t', "break-time",
            HelpText = "Break time in ms. Default is 100ms")]
    public int BreakTime { get; set; } = -1;

    [Option ('f', "fetch-data-number",
            HelpText = "Number of data to fetch at each loop. Default: 10 for ICncDataQueue and 60 for IMultiCncDataQueue")]
    public int FetchDataNumber { get; set; } = -1;

    [Option ('m', "min-data-to-process",
            HelpText = "Minimum number of data to process. Default is 1")]
    public int MinDataOfNumberToProcess { get; set; } = -1;

    [Option ('a', "process-after",
            HelpText = "Time period after which the data must be processed whichever it is. Default is 0:01:00")]
    public string WhicheverNbOfDataProcessAfter { get; set; } = "0:01:00";

    [Option ('v', "machine-mode-visit",
            HelpText = "Time period frequency when the machine modes must be processed. Default is 0:00:08")]
    public string VisitMachineModesEvery { get; set; } = "0:00:08";
    #endregion // Options
  }
}
