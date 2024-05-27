// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using CommandLine;
using Lemoine.BaseControls;
using Lemoine.I18N;
using Lemoine.Core.Log;
using CommandLine.Text;
using Lemoine.Info.ConfigReader.TargetSpecific;
using System.Threading.Tasks;
using Lemoine.Core.Extensions.Hosting;
using Lemoine.DataControls;
using Pulse.Hosting.ApplicationInitializer;
using Lemoine.Info;

namespace Lem_Configuration
{
  /// <summary>
  /// Class with program entry point.
  /// </summary>
  static class Program
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Program).FullName);

    /// <summary>
    /// Program entry point.
    /// </summary>
    [STAThread]
    private static void Main (string[] args)
    {
      // The temp directories must exist, else an exception may occur
      Lemoine.Info.PulseInfo.CreateTempDirectories ();

      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());

      Options options = null;
      try {
        var result = CommandLine.Parser.Default.ParseArguments<Options> (args);

        result.WithNotParsed<Options> (errors => {
          var usage = HelpText.AutoBuild (result);
          RaiseArgumentError (usage, null);
          return;
        });

        result.WithParsed<Options> (opt => {
          var parameters = opt.Parameters;
          if (null != parameters) {
            Lemoine.Info.ConfigSet.AddCommandLineParameters (parameters);
          }
          LogManager.AddLog4net ();

          options = opt;
        });
        System.Diagnostics.Debug.Assert (null != options);
      }
      catch (Exception ex) {
        log.Error ("Parse: exception raised", ex);
        System.Console.Error.WriteLine ($"Exception {ex} raised");
        Environment.Exit (1);
      }

      if (options is null) {
        return;
      }

#if NETCOREAPP
      Application.SetHighDpiMode (HighDpiMode.SystemAware);
#endif // NETCOREAPP
      Application.EnableVisualStyles ();
      Application.SetCompatibleTextRenderingDefault (false);

      var builder = Pulse.Hosting.HostBuilder.CreatePulseGuiHostBuilder (args, options, services => services.CreateLemConfigurationServices ());
      var host = builder.Build ();

      var serviceProvider = host.Services;
      var guiInitializer = serviceProvider.GetRequiredService<IGuiInitializer> ();

      // Wrap it in the splashscreen
      var splashScreenOptions = new SplashScreenOptions {
        Identification = options.Advanced,
        LoginRequired = false,
        DefaultLogin = "admin",
        DefaultPassword = options.Password,
        Validate = ValidateUserPassword
      };
      try {
        var splashScreen = new SplashScreen (CreateMainForm, guiInitializer, splashScreenOptions);
        Application.Run (splashScreen);
      }
      catch (Exception ex) {
        log.Error ($"Main: exception raised in splash screen", ex);
        throw;
      }
    }

    static void RaiseArgumentError (string usage, string additionalText)
    {
      var dialog = new Lemoine.BaseControls.UsageDialog (usage, additionalText);
      dialog.ShowDialog ();
      Environment.Exit (1);
    }

    static object ValidateUserPassword (string login, string password, bool rememberMe)
    {
      // For now, just a simple identification
      if (login.Equals ("superadmin", StringComparison.InvariantCultureIgnoreCase)) {
        if (PasswordManager.IsSuperAdmin (password)) {
          return true;
        }
        else {
          log.Error ($"ValidateUserPassword: wrong password for superadmin");
          return null;
        }
      }
      else if (login.Equals ("admin", StringComparison.InvariantCultureIgnoreCase)) {
        if (PasswordManager.IsAdmin (password)) {
          return true;
        }
        else {
          log.Error ($"ValidateUserPassword: wrong password for admin");
          return null;
        }
      }
      else {
        return false;
      }
    }

    static Form CreateMainForm (object userInformation)
    {
      var advancedMode = false;
      if (null != userInformation) {
        advancedMode = (bool)userInformation;
      }

      var mainForm = new MainForm (advancedMode);
      return mainForm;
    }
  }
}
