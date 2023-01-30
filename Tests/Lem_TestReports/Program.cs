// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Windows.Forms;

using CommandLine;
using CommandLine.Text;
using Lemoine.Core.Log;

namespace Lem_TestReports
{
  /// <summary>
  /// Class with program entry point.
  /// </summary>
  internal sealed class Program
  {
    static readonly ILog log = LogManager.GetLogger(typeof (Program).FullName);

    /// <summary>
    /// Program entry point.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      MainForm mainForm = new MainForm ();

      try {
        var result = CommandLine.Parser.Default.ParseArguments<Options> (args);

        result.WithNotParsed (errors =>
        {
          var usage = HelpText.AutoBuild (result);
          RaiseArgumentError (usage, null);
          Environment.ExitCode = 1;
          return;
        });

        result.WithParsed (opt =>
        {
          var parameters = opt.Parameters;
          if (null != parameters) {
            Lemoine.Info.ConfigSet.AddCommandLineParameters (parameters);
          }
          LogManager.AddLog4net ();

          mainForm.Options = opt;
          if (opt.AutoDiff) {
            mainForm.AutoDiff = true;
          }
          if (!string.IsNullOrEmpty (opt.ViewerUrl)) {
            mainForm.ViewerUrl = opt.ViewerUrl;
          }
          if (!string.IsNullOrEmpty (opt.Out)) {
            mainForm.Out = opt.Out;
          }
          if (!string.IsNullOrEmpty (opt.Ref)) {
            mainForm.Ref = opt.Ref;
          }
          if (!string.IsNullOrEmpty (opt.JdbcServer)) {
            mainForm.JdbcServer = opt.JdbcServer;
          }
          if (!string.IsNullOrEmpty (opt.UrlParameters)) {
            mainForm.UrlParameters = opt.UrlParameters;
          }
          if (!string.IsNullOrEmpty (opt.FileName)) {
            string fileName = opt.FileName;
            if (File.Exists (fileName)) {
              mainForm.OpenFile (fileName);
              if (opt.Run) {
                mainForm.AutoRun = true;
              }
            }
          }
        });
      }
      catch (Exception ex) {
        log.Error ("Main: options parsing  exception", ex);
        System.Console.Error.WriteLine ($"Options parsing exception {ex} raised");
        Environment.ExitCode = 1;
      }
      
      Application.Run(mainForm);
    }

    static void RaiseArgumentError (string usage, string additionalText)
    {
      var dialog = new Lemoine.BaseControls.UsageDialog (usage, additionalText);
      dialog.ShowDialog ();
      Environment.Exit (1);
    }
  }
}
