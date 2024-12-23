// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.Hosting.AsyncInitialization;
using Microsoft.CodeAnalysis;
using System;
using System.Data.Odbc;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Lem_AnalysisService
{
  /// <summary>
  /// Initialize the ODBC drivers, so that they could be used in plugins
  /// </summary>
  public class OdbcInitializer : IApplicationInitializer
  {
    ILog log = LogManager.GetLogger<OdbcInitializer> ();

    public OdbcInitializer ()
    {
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    public void InitializeApplication (CancellationToken cancellationToken = default)
    {
      if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        try {
          // To initialize the ODBC drivers that could be used in plugins
          using (var connection = new OdbcConnection ("")) {
          }
        }
        catch (Exception ex) {
          log.Error ($"InitializeAsync: exception", ex);
        }
      }
      else if (log.IsDebugEnabled) {
        log.Debug ($"InitializeAsync: not a windows platform");
      }
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        try {
          // To initialize the ODBC drivers that could be used in plugins
          using (var connection = new OdbcConnection ("")) {
          }
        }
        catch (Exception ex) {
          log.Error ($"InitializeAsync: exception", ex);
        }
      }
      else if (log.IsDebugEnabled) {
        log.Debug ($"InitializeAsync: not a windows platform");
      }
      return Task.CompletedTask;
    }
  }
}
