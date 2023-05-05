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

namespace Pulse.Hosting
{
  /// <summary>
  /// Extensions of <see cref="IHostBuilder"/>
  /// </summary>
  public static class HostBuilderExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Pulse.Hosting.HostBuilderExtensions).FullName);

    /// <summary>
    /// Set the AppConfiguration for a Service
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder ConfigurePulseServiceAppConfiguration (this IHostBuilder hostBuilder, ServiceOptions options)
    {
      return hostBuilder.ConfigurePulseConsoleAppConfiguration (options);
    }

    /// <summary>
    /// Set the AppConfiguration for a Console Application
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder ConfigurePulseConsoleAppConfiguration (this IHostBuilder hostBuilder, IMicrosoftParameters options)
    {
      return hostBuilder
        .ConfigureAppConfiguration ((hostingContext, config) => {
          config.AddCommandLine (options.MicrosoftParameters.ToArray ());
          config.AddEnvironmentVariables ("POMAMO_");
        });
    }

    /// <summary>
    /// Set the AppConfiguration for a Gui Application
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder ConfigurePulseGuiAppConfiguration (this IHostBuilder hostBuilder, IMicrosoftParameters options)
    {
      return hostBuilder.ConfigurePulseConsoleAppConfiguration (options);
    }
  }
}
