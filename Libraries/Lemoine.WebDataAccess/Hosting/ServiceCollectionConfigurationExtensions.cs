// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.ModelDAO.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Lemoine.WebDataAccess.Hosting
{
  /// <summary>
  /// ServiceCollectionConfigurationExtensions
  /// </summary>
  public static class ServiceCollectionConfigurationExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ServiceCollectionConfigurationExtensions).FullName);

    /// <summary>
    /// Configure the database with a data access using the web service or the database
    /// </summary>
    /// <typeparam name="T">Fallback IConnectionInitializer</typeparam>
    /// <returns></returns>
    public static IServiceCollection ConfigureWebDataAccess<T> (this IServiceCollection services)
      where T: class, IConnectionInitializer, IDatabaseConnectionStatus
    {
      return services
        .AddSingleton ((IServiceProvider sp) => new WebDataConnectionInitializer<T> (sp.GetService<T> ()))
        .AddSingleton<IConnectionInitializer> ((IServiceProvider sp) => sp.GetRequiredService<WebDataConnectionInitializer<T>> ())
        .AddSingleton<IDatabaseConnectionStatus> ((IServiceProvider sp) => sp.GetRequiredService<WebDataConnectionInitializer<T>> ());
    }

  }
}
