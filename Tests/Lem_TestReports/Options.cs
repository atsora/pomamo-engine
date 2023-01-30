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

namespace Lem_TestReports
{
  /// <summary>
  /// Description of Options.
  /// </summary>
  internal class Options
  {
    static readonly ILog log = LogManager.GetLogger(typeof (Options).FullName);

    #region Options
    [Option ('r', "run", HelpText = "Run all the tests")]
    public bool Run { get; set; } = false;

    [Option ('a', "auto-diff", HelpText = "Select the auto-diff option")]
    public bool AutoDiff { get; set; } = false;

    [Option ('u', "url", HelpText = "Force a specific viewer Url. For example: http://lctr:8080/pulsereporting/ (else get it from the input file)")]
    public string ViewerUrl { get; set; } = null;

    [Option ('o', "out", HelpText = "Force an output directory (else get it from the input file)")]
    public string Out { get; set; } = null;

    [Option ('s', "ref", HelpText = "Force a reference directory (else get it from the input file)")]
    public string Ref { get; set; } = null;

    [Option ('j', "jdbc", HelpText = "Force a JDBC server. For example: localhost:5432 (else get it from the input file)")]
    public string JdbcServer { get; set; } = null;

    [Option ('q', "urlparameters", HelpText = "Force the url parameters (else get it from the input file)")]
    public string UrlParameters { get; set; } = null;

    /// <summary>
    /// Additional parameters
    /// 
    /// The syntax is: Key1=Value1 Key2=Value2
    /// </summary>
    [Option ('p', "parameters", Required = false, HelpText = "Additional parameters")]
    public IEnumerable<string> Parameters { get; set; } = new List<string> ();
    #endregion // Options

    #region ValueList
    [Value (0, MetaName = "FileName", HelpText = "File name", Required = false)]
    public string FileName { get; set; } = "";
    #endregion // ValueList
  }
}
