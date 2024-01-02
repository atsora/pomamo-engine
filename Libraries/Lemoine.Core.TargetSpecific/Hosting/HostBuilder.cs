// Copyright (c) 2023 Atsora Solutions

#if !NET40

using Lemoine.Core.Extensions.Hosting;
using Lemoine.Core.Extensions.Logging;
using Lemoine.Core.Log;
using Lemoine.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Core.TargetSpecific.Hosting
{
  /// <summary>
  /// HostBuilder
  /// </summary>
  public static class HostBuilder
  {
    static readonly ILog log = LogManager.GetLogger (typeof (HostBuilder).FullName);

    /// <summary>
    /// Create the host builder for a console application
    /// </summary>
    /// <param name="args"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder CreateConsoleHostBuilder (string[] args, IMicrosoftParameters options, Action<IServiceCollection> configureServices)
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

  }
}

#endif // !NET40
