// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using CommandLine;
using CommandLine.Text;
using Lemoine.Core.Log;
using Lemoine.Core.Options;

namespace Lem_ApplyMachineModifications
{
  /// <summary>
  /// Description of Options.
  /// </summary>
  public class Options : IMicrosoftParameters
  {
    static readonly ILog log = LogManager.GetLogger(typeof (Options).FullName);

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
    [Option ('a', "config", Required = false, HelpText = "Additional Microsoft configuration parameters")]
    public IEnumerable<string> MicrosoftParameters { get; set; } = new List<string> ();

    [Option ('m', "machine",
            HelpText = "Machine ID")]
    public int MachineId { get; set; } = int.MinValue;

    [Option ('u', "updater",
            HelpText = "Updater/User ID")]
    public int UpdaterId { get; set; } = int.MinValue;
    #endregion // Options
    
    #region ValueList
    [Value (0, MetaName = "modification descriptions", HelpText = @"type;range;id[;details] where:
  - type: Reason / MachineStateTemplate / ResetTask / Task / ResetWorkOrderComponent / ResetOperation
  - range is formatted like: [2016-01-13T15:50:35,2016-01-13T16:00:00)
        The date/times are in UTC. If empty, from now to +oo
  - id: is the ID of the reason (0 or empty: default reason) or the ID of the work order or the ID 
        If type=Reason, ID of the reason (0 or empty: default reason)
        If type=MachineStateTemplate, ID of the machine state template
        If type=ResetTask, 0
        If type=Task, ID of the task
        If type=ResetWorkOrderComponent, 0
        If type=ResetOperation, 0
  - details is an extra information
        If type=Reason, it is the extra description
        If type=MachineStateTemplate, it is the Shift ID
", Required = true)]
    public IEnumerable<string> ModificationDescriptions { get; set; } = new List<string> ();
    #endregion // ValueList
  }
}
