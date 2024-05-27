// Copyright (C) 2024 Atsora Solutions

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.BaseControls;
using Lemoine.CncEngine;
using Lemoine.Core.Extensions.Hosting;
using Lemoine.Core.Log;
using Lemoine.DataControls;
using Lemoine.Extensions;
using Lemoine.I18N;
using Lemoine.Info;
using Lemoine.Info.ApplicationNameProvider;
using Microsoft.Extensions.DependencyInjection;
using Pulse.Hosting;
using Pulse.Hosting.ApplicationInitializer;

namespace Lem_CncGUI
{
  /// <summary>
  /// ServiceCollectionExtensions
  /// </summary>
  public static class ServiceCollectionExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ServiceCollectionExtensions).FullName);

    public static IServiceCollection CreateLemCncGUIServices (this IServiceCollection services)
    {
      return services
        .AddSingleton<ICncEngineConfig, CncEngineConfig> ()
        .AddSingleton<IApplicationNameProvider, ApplicationNameProviderFromProgramInfo> ()
        .CreateGuiServicesDatabaseWithNoNHibernateExtension (Lemoine.Model.PluginFlag.Cnc, GetInterfaceProviders ())
        .SetApplicationInitializer<ApplicationInitializerCncAcquisition> ()
        .AddTransient<MainForm> ();
    }

    static IEnumerable<IExtensionInterfaceProvider> GetInterfaceProviders () => Lemoine.Extensions.Cnc.ExtensionInterfaceProvider.GetInterfaceProviders ().Union (new List<IExtensionInterfaceProvider> { new Pulse.Extensions.Database.ExtensionInterfaceProvider () });
  }
}
