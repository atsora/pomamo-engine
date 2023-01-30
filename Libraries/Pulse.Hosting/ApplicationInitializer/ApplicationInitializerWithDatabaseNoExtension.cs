// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;
using Lemoine.Extensions.Interfaces;
using Lemoine.ModelDAO.Interfaces;

namespace Pulse.Hosting.ApplicationInitializer
{
  /// <summary>
  /// Initialize the application to connect to the database with no extension
  /// </summary>
  public class ApplicationInitializerWithDatabaseNoExtension : IApplicationInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (ApplicationInitializerWithDatabaseNoExtension).FullName);

    readonly IConnectionInitializer m_connectionInitializer;

    /// <summary>
    /// Constructor
    /// </summary>
    public ApplicationInitializerWithDatabaseNoExtension (IConnectionInitializer connectionInitializer)
    {
      m_connectionInitializer = connectionInitializer;
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    public void InitializeApplication (CancellationToken cancellationToken = default)
    {
      try {
        try {
          m_connectionInitializer.Initialize (cancellationToken: cancellationToken);
        }
        catch (Exception ex1) {
          log.Error ("InitializeApplication: connection initialization failed, exit", ex1);
          throw;
        }

        cancellationToken.ThrowIfCancellationRequested ();

        // To be able to read configurations in the database
        Lemoine.Info.ConfigSet.AddConfigReader (new Lemoine.ModelDAO.Info.ModelDAOConfigReader (true));

        cancellationToken.ThrowIfCancellationRequested ();
      }
      catch (Exception ex) {
        log.Error ("InitializeApplication: exception", ex);
        throw;
      }
    }

    public async Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      try {
        try {
          await m_connectionInitializer.InitializeAsync (cancellationToken: cancellationToken);
        }
        catch (Exception ex1) {
          log.Error ("InitializeApplicationAsync: connection initialization failed, exit", ex1);
          throw;
        }

        cancellationToken.ThrowIfCancellationRequested ();

        // To be able to read configurations in the database
        Lemoine.Info.ConfigSet.AddConfigReader (new Lemoine.ModelDAO.Info.ModelDAOConfigReader (true));

        cancellationToken.ThrowIfCancellationRequested ();
      }
      catch (Exception ex) {
        log.Error ("InitializeApplicationAsync: exception", ex);
        throw;
      }
    }
  }
}
