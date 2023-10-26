// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.DataControls;
using Lemoine.I18N;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Microsoft.Extensions.DependencyInjection;
using Pulse.Hosting;
using Pulse.Hosting.ApplicationInitializer;

namespace LemoineServiceMonitoring
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
#if NET5_0_OR_GREATER
    [SupportedOSPlatform ("windows7.0")]
#endif // NET5_0_OR_GREATER
    static void Main (string[] args)
    {
#if NET6_0_OR_GREATER
      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader (mixed6432: true));
      LogManager.AddLog4net ();

      Application.EnableVisualStyles ();
      Application.SetCompatibleTextRenderingDefault (false);

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
#endif // NET6_0_OR_GREATER
    }

    static IServiceCollection CreateServices (this IServiceCollection services)
    {
      return services
        .CreateGuiServicesDataAccessFromConfigSet ()
        .SetApplicationInitializer<ApplicationInitializerWithDatabaseNoExtension, PulseCatalogInitializer> ()
        .AddTransient<MainForm> ();
    }
  }
}
