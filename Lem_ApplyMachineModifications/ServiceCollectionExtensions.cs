// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Extensions.Hosting;
using Lemoine.Core.Log;
using Lemoine.DataControls;
using Lemoine.I18N;
using Microsoft.Extensions.DependencyInjection;
using Pulse.Hosting.ApplicationInitializer;

namespace Lem_ApplyMachineModifications
{
  /// <summary>
  /// ServiceCollectionExtensions
  /// </summary>
  public static class ServiceCollectionExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ServiceCollectionExtensions).FullName);

    public static IServiceCollection CreateLemApplyMachineModificationsServices (this IServiceCollection services, Options options)
    {
      return services
        .CreateGuiServicesDataAccessFromConfigSet ()
        .SetApplicationInitializer<ApplicationInitializerWithDatabaseNoExtension, PulseCatalogInitializer> ()
        .AddTransient<MainForm> ((IServiceProvider sp) => new MainForm (options));
    }

  }
}
