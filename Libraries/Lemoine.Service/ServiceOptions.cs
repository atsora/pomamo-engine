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
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Service
{
  /// <summary>
  /// Default service options
  /// </summary>
  public class ServiceOptions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ServiceOptions).FullName);

    #region Options
    /// <summary>
    /// Debug option to allow running the service with the command line
    /// 
    /// This is obsolete. Please use the console option now
    /// </summary>
    [Option ('d', "debug", HelpText = "Debug mode")]
    public bool Debug { get; set; } = false;

    /// <summary>
    /// Console mode to run the service in interactive mode (from the command line)
    /// </summary>
    [Option ('c', "console", HelpText = "Console mode")]
    public bool Console { get; set; } = false;

    /// <summary>
    /// NoConsole mode to run the service in no console mode (from the command line)
    /// </summary>
    [Option ('n', "noconsole", HelpText = "No console mode")]
    public bool NoConsole { get; set; } = false;

    /// <summary>
    /// Option to install the service
    /// 
    /// Only available with the .NET Framework
    /// 
    /// UNDONE: not tested
    /// </summary>
#if NET45
    [Option ('i', "install",
            HelpText="Install the service")]
#endif // NET45
    public bool Install { get; set; } = false;

    /// <summary>
    /// Option to uninstall the service
    /// 
    /// Only available with the .NET Framework
    /// 
    /// UNDONE: not tested
    /// </summary>
#if NET45
    [Option ('u', "remove",
            HelpText="Remove the service")]
#endif // NET45
    public bool Remove { get; set; } = false;

    /// <summary>
    /// Additional parameters
    /// </summary>
    [Option ('p', "parameters", Required = false, HelpText = "Additional parameters")]
    public IEnumerable<string> Parameters { get; set; }

    /// <summary>
    /// Additional microsoft parameters (not available on all the services)
    /// 
    /// The syntax is:
    /// <item>Key1=Value1 Key2=Value2</item>
    /// <item>/Key1 Value1 /Key2 Value2</item>
    /// <item>/Key1=Value1 /Key2=Value2 /Key3=</item>
    /// 
    /// Note that the syntax --Key1 Value1 or --Key1==Value1 is not supported
    /// </summary>
    [Option ('m', "config", Required = false, HelpText = "Additional Microsoft configuration parameters (not available on all the services)")]
    public IEnumerable<string> Configurations { get; set; } = new List<string> ();

    /// <summary>
    /// Use the interactive mode
    /// because it was set in the command line or because a debugger is attached
    /// </summary>
    public bool Interactive => this.Console || this.Debug || (System.Environment.UserInteractive && System.Diagnostics.Debugger.IsAttached);
    #endregion

    /// <summary>
    /// Try to parse the arguments
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static ServiceOptions Parse (string[] args)
    {
      ServiceOptions serviceOptions = null;
      try {
        var result = CommandLine.Parser.Default.ParseArguments<ServiceOptions> (args);

        result.WithNotParsed<ServiceOptions> (errors =>
           {
             var helpText = HelpText.AutoBuild (result);
             throw new ArgumentException ("args");
           });

        result.WithParsed<ServiceOptions> (options =>
          {
            var parameters = options.Parameters;
            if (null != parameters) {
              Lemoine.Info.ConfigSet.AddCommandLineParameters (parameters);
            }
            serviceOptions = options;
          });
        System.Diagnostics.Debug.Assert (null != serviceOptions);
        return serviceOptions;
      }
      catch (Exception ex) {
        log.Error ("Parse: exception raised", ex);
        System.Console.Error.WriteLine ($"Exception {ex} raised");
        throw;
      }
    }
  }
}
