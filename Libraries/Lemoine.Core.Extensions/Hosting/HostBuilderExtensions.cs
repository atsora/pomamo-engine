// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Core.Options;
using Lemoine.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Core.Extensions.Hosting
{
  /// <summary>
  /// Extensions of <see cref="IHostBuilder"/>
  /// </summary>
  public static class HostBuilderExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (HostBuilderExtensions).FullName);

    /// <summary>
    /// Set the AppConfiguration for a Pomamo Service
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder ConfigureLemoineServiceAppConfiguration (this IHostBuilder hostBuilder, ServiceOptions options)
    {
      return hostBuilder.ConfigureLemoineConsoleAppConfiguration (options);
    }

    /// <summary>
    /// Set the AppConfiguration for a Pomamo Console Application
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder ConfigureLemoineConsoleAppConfiguration (this IHostBuilder hostBuilder, IMicrosoftParameters options)
    {
      return hostBuilder
        .ConfigureAppConfiguration ((hostingContext, config) => {
          config.AddCommandLine (options.MicrosoftParameters.ToArray ());
          config.AddEnvironmentVariables ("POMAMO_");
        });
    }

    /// <summary>
    /// Set the AppConfiguration for a Pomamo Gui Application
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder ConfigureLemoineGuiAppConfiguration (this IHostBuilder hostBuilder, IMicrosoftParameters options)
    {
      return hostBuilder.ConfigureLemoineConsoleAppConfiguration (options);
    }
  }
}
