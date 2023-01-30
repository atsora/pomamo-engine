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

namespace Lem_PackageManager.Console
{
  /// <summary>
  /// Description of Options.
  /// </summary>
  public class Options: IMicrosoftParameters
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Options).FullName);

    #region Options
    [Option ('i', "install", HelpText = "Install or upgrade a package. Specify a json description path")]
    public string InstallOrUpgradePackage { get; set; } = "";

    [Option ('r', "remove", HelpText = "Remove a package. Specify the package identifier")]
    public string RemovePackage { get; set; } = "";

    [Option ('d', "deactivate", HelpText = "De-activate a package. Specify the package identifier")]
    public string DeactivatePackage { get; set; } = "";

    [Option ('c', "check", HelpText = "Install/Check the configurations of the active packages")]
    public bool Check { get; set; } = false;

    [Option ('f', "overwrite-parameters", HelpText = "Overwrite the parameters. Default is false")]
    public bool OverwriteParameters { get; set; } = false;

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
    [Option ('m', "config", Required = false, HelpText = "Additional Microsoft configuration parameters")]
    public IEnumerable<string> MicrosoftParameters { get; set; } = new List<string> ();
    #endregion // Options
  }
}
