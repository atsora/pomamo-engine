// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;

using Lemoine.Info;
using Lemoine.Core.Log;
using CommandLine;
using Lemoine.Core.Options;

namespace Lem_ActivityAnalysis.Console
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
    [Option ('p', "parameters", Required = false, HelpText = "Additional parameters")]
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
    [Option ('e', "config", Required = false, HelpText = "Additional Microsoft configuration parameters")]
    public IEnumerable<string> MicrosoftParameters { get; set; } = new List<string> ();

    [Option ('v', "verbose", HelpText = "Verbose option: write the steps in console")]
    public bool Verbose { get; set; } = false;

    [Option ('i', "id", HelpText = "ID of the machine")]
    public int MachineId { get; set; } = int.MinValue;

    [Option ('g', "global", HelpText = "Global analysis switch")]
    public bool Global { get; set; } = false;

    [Option ('z', "sleep",
            HelpText = "Sleep time in ms")]
    public int SleepTime { get; set; } = 0;

    [Option ('l', "loop", HelpText = "Loop in the analysis")]
    public bool Loop { get; set; } = false;

    [Option ('d', "statemachine", HelpText = "Use the default state machine")]
    public bool StateMachine { get; set; } = false;

    [Option ('k', "catchup", HelpText = "Catch-up mode")]
    public bool CatchUp { get; set; } = false;

    [Option ('t', "machinestate", HelpText = "Run the machine-state template analysis (requires -i option)")]
    public bool MachineStateTemplateAnalysis { get; set; } = false;

    [Option ('o', "operationslotsplit", HelpText = "Run the operation slot split analysis (requires -i option)")]
    public bool OperationSlotSplitAnalysis { get; set; } = false;

    [Option ('q', "productionanalysis", HelpText = "Run the production analysis (requires -i option)")]
    public bool ProductionAnalysis { get; set; } = false;

    [Option ('a', "activity", HelpText = "Run the activity analysis of x facts, 0: none, -1: all (requires -i option)")]
    public int ActivityAnalysis { get; set; } = 0;

    [Option ('m', "modification", HelpText = "Run the modification analysis")]
    public bool ModificationAnalysis { get; set; } = false;

    [Option ('r', "processingreasonslots", HelpText = "Run the analysis of the processing reason slots maximum x times, 0: none, -1 all (requires -i option)")]
    public int ProcessingReasonSlotsAnalysis { get; set; } = 0;

    [Option ('x', "detection", HelpText = "Run x detections analysis, 0: none, -1: all (requires -i option)")]
    public int DetectionAnalysis { get; set; } = 0;

    [Option ('u', "autosequence", HelpText = "Run x auto-sequence analysis, 0: none, -1: all (requires -i option)")]
    public int AutoSequenceAnalysis { get; set; } = 0;

    [Option ('f', "shift", HelpText = "Run the shift template analysis (requires -g option)")]
    public bool ShiftTemplateAnalysis { get; set; } = false;

    [Option ('y', "day", HelpText = "Run the day template analysis (requires -g option)")]
    public bool DayTemplateAnalysis { get; set; } = false;

    [Option ('w', "week", HelpText = "Run the week number analysis (requires -g option)")]
    public bool WeekNumberAnalysis { get; set; } = false;

    [Option ('c', "cleanmodifications", HelpText = "Clean the flagged modifications")]
    public bool CleanFlaggedModifications { get; set; } = false;

    [Option ('b', "turnoncatchup", HelpText = "Turn on the catch-up mode in the analysis service")]
    public bool TurnOnCatchUpMode { get; set; } = false;
    #endregion // Options
  }
}
