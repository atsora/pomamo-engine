// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Hosting.AsyncInitialization;
using Microsoft.CodeAnalysis;
using System;
using System.Data.Odbc;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Lem_AspService
{
  /// <summary>
  /// Initialize the ODBC drivers, so that they could be used in plugins
  /// </summary>
  public class OdbcInitializer : IAsyncInitializer
  {
    ILog log = LogManager.GetLogger<OdbcInitializer> ();

    public OdbcInitializer ()
    {
    }

    public Task InitializeAsync ()
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
