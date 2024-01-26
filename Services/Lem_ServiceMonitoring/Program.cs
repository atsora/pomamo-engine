// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Core.Extensions.Hosting;
using Lemoine.Core.Log;
using Lemoine.DataControls;
using Lemoine.I18N;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Microsoft.Extensions.DependencyInjection;
using Pulse.Hosting.ApplicationInitializer;
#if CONNECTOR
using Atsora.JsonFileData;
using Lemoine.Core.Hosting;
using Lemoine.DataControls.GuiInitializer;
using Lemoine.Extensions.DummyImplementations;
using Lemoine.Extensions.Interfaces;
using Lemoine.FileRepository;
using Lemoine.WebDataAccess.Hosting;
using Pulse.Hosting;
#endif // CONNECTOR

namespace LemoineServiceMonitoring
{
  /// <summary>
  /// Class with program entry point.
  /// </summary>
  public static class Program
  {
#if CONNECTOR
    static readonly string DATA_ACCESS_KEY = "DataAccess";
    static readonly string DATA_ACCESS_WEB = "web";
    static readonly string DATA_ACCESS_DEFAULT = DATA_ACCESS_WEB;
#endif // CONNECTOR

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
#if CONNECTOR
      services
        .ConfigureFileRepoClientFactoryLctr ();
      if (Lemoine.Info.ConfigSet.LoadAndGet<string> (DATA_ACCESS_KEY, DATA_ACCESS_DEFAULT).Equals (DATA_ACCESS_WEB)) {
        services
          .AddSingleton ((IServiceProvider sp) => new JsonFileDataConnectionInitializer ())
          .ConfigureWebDataAccess<JsonFileDataConnectionInitializer> ();
      }
      else {
        services
          .ConfigureJsonFileDataOnly ();
      }
      return services
        .AddSingleton<IGuiInitializer> ((IServiceProvider sp) => new GuiInitializer (sp.GetRequiredService<IApplicationInitializer> (), sp.GetService<IFileRepoClientFactory> ()))
        .AddSingleton<IExtensionsLoader, ExtensionsLoaderDummy> ()
        .AddSingleton<IApplicationInitializer, ApplicationInitializerConnector> ()
#else // !CONNECTOR
      return services
        .CreateGuiServicesDataAccessFromConfigSet ()
        .SetApplicationInitializer<ApplicationInitializerWithDatabaseNoExtension, PulseCatalogInitializer> ()
#endif // !CONNECTOR
        .AddTransient<MainForm> ();
    }
  }
}
