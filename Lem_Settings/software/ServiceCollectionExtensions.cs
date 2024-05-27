// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Extensions.Hosting;
using Lemoine.Core.Log;
using Lemoine.DataControls;
using Lemoine.Extensions;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.Settings;
using Microsoft.Extensions.DependencyInjection;
using Pulse.Hosting;
using Pulse.Hosting.ApplicationInitializer;

namespace Lem_Settings
{
  /// <summary>
  /// ServiceCollectionExtensions
  /// </summary>
  public static class ServiceCollectionExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ServiceCollectionExtensions).FullName);

    public static IServiceCollection CreateLemSettingsServices (this IServiceCollection services)
    {
      return services
        .CreateGuiServicesDatabaseWithExtensions (PluginFlag.None, GetInterfaceProviders ())
        .ConfigureBusinessLruCache ()
        .SetApplicationInitializer<ApplicationInitializerWithExtensions, BusinessApplicationInitializer, PulseCatalogInitializer, RevisionManagerApplicationInitializer> ()
        .AddTransient<MainForm> ();

    }

    static IEnumerable<IExtensionInterfaceProvider> GetInterfaceProviders ()
    {
      return new List<IExtensionInterfaceProvider> {
        new Lemoine.Extensions.Alert.ExtensionInterfaceProvider (),
        new Lemoine.Extensions.Analysis.ExtensionInterfaceProvider (),
        new Lemoine.Extensions.AutoReason.ExtensionInterfaceProvider (),
        new Lemoine.Extensions.Business.ExtensionInterfaceProvider (),
        new Lemoine.Extensions.Cnc.ExtensionInterfaceProvider (),
        new Lemoine.Extensions.Database.ExtensionInterfaceProvider (),
        new Lemoine.Extensions.Web.ExtensionInterfaceProvider (),
        new Pulse.Extensions.Business.ExtensionInterfaceProvider (),
        new Pulse.Extensions.Database.ExtensionInterfaceProvider (),
        new Pulse.Extensions.Web.ExtensionInterfaceProvider (),
      };
    }
  }
}
