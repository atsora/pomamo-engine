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

namespace Lem_FileRepoClient.Console
{
  /// <summary>
  /// Description of Options.
  /// </summary>
  internal class Options
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Options).FullName);

    #region Options
    [Option ('w', "web", HelpText = "Web file repo URL", Required = false)]
    public string FileRepoClientWebUrl { get; set; }

    /// <summary>
    /// Additional parameters
    /// 
    /// The syntax is: Key1=Value1 Key2=Value2
    /// </summary>
    [Option ('p', "parameters", Required = false, HelpText = "Additional parameters")]
    public IEnumerable<string> Parameters { get; set; } = new List<string> ();

    [Value (0, MetaName = "NSpace", HelpText = "Namespace", Required = true)]
    public string NSpace { get; set; }

    [Value (0, MetaName = "Path", HelpText = "Path", Required = true)]
    public string Path { get; set; }
    #endregion // Options
  }
}
