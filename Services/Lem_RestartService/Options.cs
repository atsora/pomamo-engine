// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;

using CommandLine;
using CommandLine.Text;
using Lemoine.Core.Log;

namespace Lem_RestartService
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
    #endregion // Options

    #region ValueList
    [Value (0)]
    public IEnumerable<string> ServiceNames { get; set; } = null;
    #endregion // ValueList
  }
}
