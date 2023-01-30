// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Lemoine.Core.Log;

namespace Lem_TestReports.Console
{
  /// <summary>
  /// Program
  /// </summary>
  class Program
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Program).FullName);

    /// <summary>
    /// Program entry point
    /// </summary>
    /// <param name="args"></param>
    public static async Task<int> Main (string[] args)
    {
      try {
        var result = CommandLine.Parser.Default.ParseArguments<Options> (args);

        Engine engine = null;
        int exitCode = 1;
        result.WithNotParsed (errors => {
          var usage = HelpText.AutoBuild (result);
          RaiseArgumentError (usage);
          Environment.ExitCode = 1;
          exitCode = 1;
        });

        result.WithParsed (opt => {
          var parameters = opt.Parameters;
          if (null != parameters) {
            Lemoine.Info.ConfigSet.AddCommandLineParameters (parameters);
          }
          LogManager.AddLog4net ();

          engine = new Engine ();

          engine.Options = opt;
          if (!string.IsNullOrEmpty (opt.LogFile)) {
            engine.LogFile = opt.LogFile;
          }
          if (!string.IsNullOrEmpty (opt.ViewerUrl)) {
            engine.ViewerUrl = opt.ViewerUrl;
          }
          if (!string.IsNullOrEmpty (opt.Out)) {
            engine.Out = opt.Out;
          }
          if (!string.IsNullOrEmpty (opt.Ref)) {
            engine.Ref = opt.Ref;
          }
          if (!string.IsNullOrEmpty (opt.JdbcServer)) {
            engine.JdbcServer = opt.JdbcServer;
          }
          if (!string.IsNullOrEmpty (opt.UrlParameters)) {
            engine.UrlParameters = opt.UrlParameters;
          }
          if (0 != opt.NumberParallelExecutions) {
            engine.NumberParallelExecutions = opt.NumberParallelExecutions;
          }
          if (!string.IsNullOrEmpty (opt.FileName)) {
            string fileName = opt.FileName;
            if (System.IO.File.Exists (fileName)) {
              engine.OpenFile (fileName);
            }
          }
        });

        if (null != engine) {
          var success = await engine.RunAsync ();
          exitCode = success ? 0 : 1;
        }

        return exitCode;
      }
      catch (Exception ex) {
        log.Error ("Main: options parsing  exception", ex);
        System.Console.Error.WriteLine ($"Options parsing exception {ex} raised");
        Environment.ExitCode = 1;
        return 1;
      }
    }

    static void RaiseArgumentError (string usage)
    {
      System.Console.WriteLine (usage);
      Environment.Exit (1);
    }
  }
}
