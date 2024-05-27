// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.CncEngine;
using Lemoine.Core.Extensions.Hosting;
using Lemoine.Core.Log;
using Lemoine.DataControls;
using Lemoine.Extensions;
using Lemoine.Info;
using Lemoine.Info.ApplicationNameProvider;
using Lemoine.Info.ConfigReader.TargetSpecific;

namespace Lem_CncGUI
{
  /// <summary>
  /// Class with program entry point.
  /// </summary>
  public static class Program
  {
    /// <summary>
    /// Program entry point.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
      // Note: you can use configure log4net to override directly any previous file
      // There is no need to delete the log files manually here

      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());
      LogManager.AddLog4net ();

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      var builder = Pulse.Hosting.HostBuilder.CreatePulseGuiHostBuilder (args, services => services.CreateLemCncGUIServices ());
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
  }
}
