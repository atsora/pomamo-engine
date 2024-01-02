// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Lemoine.Core.Extensions.Logging;
using Lemoine.Core.Log;
using Lemoine.Core.Options;
using Lemoine.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lemoine.Core.Extensions.Hosting
{
  /// <summary>
  /// HostBuilder
  /// </summary>
  public static class HostBuilder
  {
    static readonly ILog log = LogManager.GetLogger (typeof (HostBuilder).FullName);

    /// <summary>
    /// Create the host builder for a Lemoine console application
    /// </summary>
    /// <param name="args"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder CreateLemoineConsoleHostBuilder (string[] args, IMicrosoftParameters options, Action<IServiceCollection> configureServices)
    {
      var defaultBuilder = Host
        .CreateDefaultBuilder (args);

      return defaultBuilder
        .ConfigureConsoleAppConfiguration (options)
        .ConfigureServices ((hostContext, services) => {
#if NET6_0_OR_GREATER
          services.AddLogging (loggingBuilder => {
            loggingBuilder.AddLemoineLog ();
          });
#endif // NET6_0_OR_GREATER
          configureServices (services);
        });
    }

    /// <summary>
    /// Create the host builder for a Lemoine Gui application
    /// </summary>
    /// <param name="args"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder CreateLemoineGuiHostBuilder (string[] args, IMicrosoftParameters options, Action<IServiceCollection> configureServices)
    {
      var defaultBuilder = Host
        .CreateDefaultBuilder (args);

      return defaultBuilder
        .ConfigureGuiAppConfiguration (options)
        .ConfigureServices ((hostContext, services) => {
#if NET6_0_OR_GREATER
          services.AddLogging (loggingBuilder => {
            loggingBuilder.AddLemoineLog ();
          });
#endif // NET6_0_OR_GREATER
          configureServices (services);
        });
    }

    /// <summary>
    /// Create the host builder for a Lemoine Gui application
    /// </summary>
    /// <param name="args"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder CreateLemoineGuiHostBuilder (string[] args, Action<IServiceCollection> configureServices)
    {
      var defaultBuilder = Host
        .CreateDefaultBuilder (args);

      return defaultBuilder
        .ConfigureServices ((hostContext, services) => {
#if NET6_0_OR_GREATER
          services.AddLogging (loggingBuilder => {
            loggingBuilder.AddLemoineLog ();
          });
#endif // NET6_0_OR_GREATER
          configureServices (services);
        });
    }
  }
}
