// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Cnc.Engine;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.DataControls;
using Lemoine.Info;
using Lemoine.Info.ApplicationNameProvider;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Microsoft.Extensions.DependencyInjection;

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

      var builder = Pulse.Hosting.HostBuilder.CreatePulseGuiHostBuilder (args, services => services.CreateServices ());
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

    static IServiceCollection CreateServices (this IServiceCollection services)
    {
      return services
        .AddSingleton<ICncEngineConfig, CncEngineConfig> ()
        .AddSingleton<IApplicationNameProvider, ApplicationNameProviderFromProgramInfo> ()
        .CreateGuiServicesDatabaseWithNoNHibernateExtension (Lemoine.Model.PluginFlag.Cnc, Lemoine.Extensions.Cnc.ExtensionInterfaceProvider.GetInterfaceProviders ())
        .SetApplicationInitializer<ApplicationInitializerCncAcquisition> ()
        .AddTransient<MainForm> ();
    }
  }
}
