// Copyright (C) 2024 Atsora Solutions

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Core.Extensions.Hosting;
using Lemoine.Core.Log;
using Lemoine.DataControls;
using Lemoine.I18N;
using Microsoft.Extensions.DependencyInjection;
using Pulse.Hosting.ApplicationInitializer;

namespace Lem_ManualActivity
{
  /// <summary>
  /// ServiceCollectionExtensions
  /// </summary>
  public static class ServiceCollectionExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ServiceCollectionExtensions).FullName);

    public static IServiceCollection CreateLemManualActivityServices (this IServiceCollection services)
    {
      return services
        .CreateGuiServicesDatabaseNoExtension ()
        .SetApplicationInitializer<ApplicationInitializerWithDatabaseNoExtension, PulseCatalogInitializer> ()
        .AddTransient<MainForm> ();
    }

  }
}
