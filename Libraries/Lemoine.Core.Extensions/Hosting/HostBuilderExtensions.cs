// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Core.Options;
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
    /// Set the AppConfiguration for a Pomamo Console Application
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder ConfigureConsoleAppConfiguration (this IHostBuilder hostBuilder, IMicrosoftParameters options)
    {
      return hostBuilder
        .ConfigureAppConfiguration ((hostingContext, config) => {
          config.AddCommandLine (options.MicrosoftParameters.ToArray ());
#if ATSORA
          config.AddEnvironmentVariables ("ATSORA_");
#else // !ATSORA
          config.AddEnvironmentVariables ("POMAMO_");
#endif // !ATSORA
        });
    }

    /// <summary>
    /// Set the AppConfiguration for a Pomamo Gui Application
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder ConfigureGuiAppConfiguration (this IHostBuilder hostBuilder, IMicrosoftParameters options)
    {
      return hostBuilder.ConfigureConsoleAppConfiguration (options);
    }
  }
}
