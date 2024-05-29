// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Windows.Forms;

using CommandLine;
using Lemoine.Core.Log;
using CommandLine.Text;
using System.Linq;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Lemoine.BaseControls;
using Microsoft.Extensions.DependencyInjection;

namespace Lem_ApplyMachineModifications
{
  /// <summary>
  /// Class with program entry point.
  /// </summary>
  public static class Program
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Program).FullName);

    /// <summary>
    /// Program entry point.
    /// </summary>
    [STAThread]
    private static void Main (string[] args)
    {
      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());

      Options options = null;
      try {
        var result = CommandLine.Parser.Default.ParseArguments<Options> (args);

        result.WithNotParsed<Options> (errors => {
          var usage = HelpText.AutoBuild (result);
          RaiseArgumentError (usage, null);
          Environment.ExitCode = 1;
          options = null;
        });

        result.WithParsed<Options> (opt => {
          options = opt;
          var parameters = opt.Parameters;
          if (null != parameters) {
            Lemoine.Info.ConfigSet.AddCommandLineParameters (parameters);
          }
          LogManager.AddLog4net ();
        });
      }
      catch (Exception ex) {
        log.Error ("Main: parse exception raised", ex);
        System.Console.Error.WriteLine ($"Exception {ex} raised");
        throw;
      }

      if (options is null) {
        log.Error ("Main: arguments not parsed");
        System.Console.Error.WriteLine ($"Invalid arguments");
        Environment.Exit (1);
        return;
      }

      Application.EnableVisualStyles ();
      Application.SetCompatibleTextRenderingDefault (false);

      var builder = Pulse.Hosting.HostBuilder.CreatePulseGuiHostBuilder (args, services => services.CreateLemApplyMachineModificationsServices (options));
      var host = builder.Build ();

      var serviceProvider = host.Services;
      var guiInitializer = serviceProvider.GetRequiredService<IGuiInitializer> ();

      // Wrap it in the splashscreen
      var splashScreenOptions = new SplashScreenOptions {
        Identification = false,
        LoginRequired = false
      };
      var splashScreen = new SplashScreen (x => serviceProvider.GetRequiredService<MainForm> (), guiInitializer, splashScreenOptions);
      Application.Run (splashScreen);
    }

    static void RaiseArgumentError (string usage, string additionalText)
    {
      var dialog = new Lemoine.BaseControls.UsageDialog (usage, additionalText);
      dialog.ShowDialog ();
      Environment.Exit (1);
    }
  }
}
