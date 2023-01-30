// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Info;
//using System.Collections.Generic;
using System.Reflection;

using CommandLine;
using CommandLine.Text;
using Lemoine.Core.Log;
using System.Collections.Generic;

namespace Lemoine.ReportDatabaseMigrator
{
  /// <summary>
  /// Description of Options.
  /// </summary>
  internal class Options
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Options).FullName);

    #region Options    
    [Option ('d', "DSN",
            HelpText = "DSN of the database", Required = false)]
    public string DSN { get; set; } = "";
    [Option ('f', "SQLfile",
            HelpText = "SQL file with the queries")]
    public string SQLfile { get; set; } = "";
    [Option ('q', "SQLquery",
            HelpText = "SQL query")]
    public string SQLquery { get; set; } = "";
    [Option ('l', "logFile",
            HelpText = "File to log notice and errors")]
    public string LogFile { get; set; } = "ReportDataMigrationLogFile.txt";

    /// <summary>
    /// Additional parameters
    /// 
    /// The syntax is: Key1=Value1 Key2=Value2
    /// </summary>
    [Option ('p', "parameters", Required = false, HelpText = "Additional parameters")]
    public IEnumerable<string> Parameters { get; set; } = new List<string> ();
    #endregion // Options
  }
}
