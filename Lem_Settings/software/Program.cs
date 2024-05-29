// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CommandLine;
using CommandLine.Text;
using Lemoine.BaseControls;
using Lemoine.Core.Log;
using Lemoine.Info;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Lemoine.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Lem_Settings
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
    static void Main (string[] args)
    {
      // The temp directories must exist, else an exception may occur
      Lemoine.Info.PulseInfo.CreateTempDirectories ();

      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());
      LogManager.AddLog4net ();

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
          ContextManager.Options = opt;
        });
      }
      catch (Exception ex) {
        log.Error ("Parse: exception raised", ex);
        System.Console.Error.WriteLine ($"Exception {ex} raised");
        Environment.Exit (1);
      }

      Application.EnableVisualStyles ();
      Application.SetCompatibleTextRenderingDefault (false);

      var builder = Pulse.Hosting.HostBuilder.CreatePulseGuiHostBuilder (args, ContextManager.Options, services => services.CreateLemSettingsServices ());
      var host = builder.Build ();

      var serviceProvider = host.Services;
      var guiInitializer = serviceProvider.GetRequiredService<IGuiInitializer> ();

      Lemoine.Core.ExceptionManagement.ExceptionTest
        .AddTest (new Lemoine.Settings.SettingsExceptionTest ());

      // Wrap it in the splashscreen
      var splashScreenOptions = new SplashScreenOptions {
        Identification = ContextManager.Options.WithLogin,
        LoginRequired = false,
        DefaultLogin = IniFilePreferences.Get (IniFilePreferences.Field.LOGIN),
        DefaultPassword = IniFilePreferences.Get (IniFilePreferences.Field.PASSWORD),
        RememberActive = true,
        Validate = ValidateUserPassword
      };
      var splashScreen = new SplashScreen (CreateMainForm, guiInitializer, splashScreenOptions);
      Application.Run (splashScreen);
    }


    static void RaiseArgumentError (string usage, string additionalText)
    {
      var dialog = new Lemoine.BaseControls.UsageDialog (usage, additionalText);
      dialog.ShowDialog ();
      Environment.Exit (1);
    }

    static object ValidateUserPassword (string login, string password, bool rememberMe)
    {
      ContextManager.UserLogin = login.ToLower ();

      // For now, just a simple identification
      if (login.Equals ("superadmin", StringComparison.InvariantCultureIgnoreCase)) {
        if (PasswordManager.IsSuperAdmin (password)) {
          return LemSettingsGlobal.UserCategory.SUPER_ADMIN;
        }
        else {
          log.Error ($"ValidateUserPassword: wrong password for superadmin");
          return null;
        }
      }
      else if (login.Equals ("admin", StringComparison.InvariantCultureIgnoreCase)) {
        if (PasswordManager.IsAdmin (password)) {
          return LemSettingsGlobal.UserCategory.ADMINISTRATOR;
        }
        else {
          log.Error ($"ValidateUserPassword: wrong password for admin");
          return null;
        }
      }
      else {
        if (rememberMe) {
          RememberMe (login, password);
        }
        else {
          RememberMe ("", "");
        }
        return LemSettingsGlobal.UserCategory.END_USER;
      }
    }

    static void RememberMe (string login, string password)
    {
      IniFilePreferences.Set (IniFilePreferences.Field.LOGIN, login);
      IniFilePreferences.Set (IniFilePreferences.Field.PASSWORD, password);
    }

    static Form CreateMainForm (object userInformation)
    {
      var mainForm = new MainForm ();
      Application.ThreadException += mainForm.UnhandledThreadExceptionHandler;
      ContextManager.UserCategory = (LemSettingsGlobal.UserCategory)userInformation;
      return mainForm;
    }
  }
}
