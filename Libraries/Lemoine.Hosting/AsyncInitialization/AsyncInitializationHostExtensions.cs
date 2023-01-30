// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lemoine.Hosting.AsyncInitialization
{
  /// <summary>
  /// Provides extension methods to perform async initialization of an application.
  /// </summary>
  public static class AsyncInitializationHostExtensions
  {
    static ILog log = LogManager.GetLogger (typeof (AsyncInitializationHostExtensions).FullName);

    /// <summary>
    /// Initializes the application, by calling all registered async initializers.
    /// </summary>
    /// <param name="host">The host.</param>
    /// <returns>A task that represents the initialization completion.</returns>
    public static async Task InitAsync (this IHost host)
    {
      if (host is null) {
        log.Error ("InitAsync: host ist null");
        Debug.Assert (false);
        throw new ArgumentNullException (nameof (host));
      }

      using (var scope = host.Services.CreateScope ()) {
        var rootInitializer = scope.ServiceProvider.GetRequiredService<RootInitializer> ();
        await rootInitializer.InitializeAsync ();
      }
    }
  }
}
