// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Core.Extensions.Hosting;
using Lemoine.Core.Log;
using Lemoine.DataControls;
using Lemoine.Extensions.Interfaces;
using Lemoine.Extensions.Plugin;
using Lemoine.I18N;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Microsoft.Extensions.DependencyInjection;
using Pulse.Hosting;
using Pulse.Hosting.ApplicationInitializer;

namespace Lem_ExcelDataImport
{
  /// <summary>
  /// Class with program entry point.
  /// </summary>
  static public class Program
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Program).FullName);

    static readonly TimeSpan? PLUGIN_SYNCHRONIZATION_TIMEOUT_DEFAULT = TimeSpan.FromSeconds (1);

    /// <summary>
    /// Program entry point.
    /// </summary>
    [STAThread]
    private static void Main (string[] args)
    {
      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());
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
      var splashScreen = new SplashScreen (x => serviceProvider.GetRequiredService<ExcelViewForm> (), guiInitializer, splashScreenOptions);
      Application.Run (splashScreen);
    }

    static IServiceCollection CreateServices (this IServiceCollection services)
    {
      return services
        .AddSingleton<IPluginSynchronizationTimeoutProvider> ((IServiceProvider sp) => new PluginSynchronizationTimeoutProviderFromConfigSet (PLUGIN_SYNCHRONIZATION_TIMEOUT_DEFAULT))
        .CreateGuiServicesDatabaseWithNoNHibernateExtension (Lemoine.Model.PluginFlag.OperationExplorer, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders ())
        .ConfigureBusinessLruCache ()
        .SetApplicationInitializer<ApplicationInitializerWithExtensions, BusinessApplicationInitializer, PulseCatalogInitializer> ()
        .AddTransient<ExcelViewForm> ();
    }

  }
}
