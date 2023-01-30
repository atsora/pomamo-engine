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

namespace Lem_GDBMigrator.Console
{
  /// <summary>
  /// Description of Options.
  /// </summary>
  internal class Options
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

    // Command mutually exclusive set
    [Option ('u', "upgrade",
            HelpText = "Upgrade GDB to the latest available version")]
    public bool Upgrade { get; set; } = false;
    
    [Option ('t', "to",
            HelpText = "Specific schema version to migrate GDB to")]
    public int To { get; set; } = int.MinValue;

    [Option ('l', "list",
            HelpText = "List the available migrations")]
    public bool List { get; set; } = false;
    
    [Option ('s', "simulate", HelpText = "Simulation mode")]
    public bool Simulate { get; set; } = false;
    #endregion
    
    #region ValueList
    [Value (0, MetaName = "ConnectionString", HelpText = "Connection string", Required = false)]
    public string ConnectionString { get; set; } = "";
    #endregion
  }
}
