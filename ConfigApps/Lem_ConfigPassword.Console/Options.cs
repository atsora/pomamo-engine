/* -*- c# -*- ******************************************************************
 * Copyright (c) Nicolas Relange. All Rights Reserved.
 */

using CommandLine;
using Lemoine.Core.Log;
using Lemoine.Core.Options;
using System.Collections.Generic;

namespace Lem_ConfigPassword.Console
{
  /// <summary>
  /// Description of Options.
  /// </summary>
  internal class Options : IMicrosoftParameters
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Options).FullName);

    [Option ('s', "salt", HelpText = "Salt string")]
    public string Salt { get; set; } = "salt";

    [Option ('p', "password", Required = true, HelpText = "Password")]
    public string Password { get; set; } = "";

    [Option ('x', "hash", HelpText = "Hash the password")]
    public bool Hash { get; set; } = false;

    [Option ('a', "admin", HelpText = "Check the admin password")]
    public bool CheckAdmin { get; set; } = false;

    /// <summary>
    /// Additional PULSE parameters
    /// 
    /// The syntax is: Key1=Value1 Key2=Value2
    /// </summary>
    [Option ('k', "parameters", Required = false, HelpText = "Additional parameters")]
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
    [Option ('m', "msconfig", Required = false, HelpText = "Additional Microsoft configuration parameters")]
    public IEnumerable<string> MicrosoftParameters { get; set; } = new List<string> ();

    [Option ('t', "trace", HelpText = "Enable the trace")]
    public bool TraceEnabled { get; set; } = false;
  }
}
