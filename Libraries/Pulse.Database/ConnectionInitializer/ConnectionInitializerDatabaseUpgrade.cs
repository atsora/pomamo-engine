// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.GDBPersistentClasses;
using Lemoine.ModelDAO.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace Pulse.Database.ConnectionInitializer
{
  /// <summary>
  /// <see cref="IConnectionInitializer"/> implementation for database upgrade only
  /// </summary>
  public class ConnectionInitializerDatabaseUpgrade : IConnectionInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConnectionInitializerDatabaseUpgrade).FullName);

    /// <summary>
    /// <see cref="IConnectionInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void CreateAndInitializeConnection (CancellationToken? cancellationToken = null)
    {
      Lemoine.ModelDAO.ModelDAOHelper.ModelFactory = new GDBPersistentClassFactory ();
    }

    /// <summary>
    /// <see cref="IConnectionInitializer"/>
    /// </summary>
    /// <param name="maxNbAttempt"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void CreateAndInitializeConnection (int maxNbAttempt, CancellationToken? cancellationToken = null)
    {
      CreateAndInitializeConnection (cancellationToken);
    }

    /// <summary>
    /// <see cref="IConnectionInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task CreateAndInitializeConnectionAsync (CancellationToken? cancellationToken = null)
    {
      CreateAndInitializeConnection (cancellationToken);
      return Task.CompletedTask;
    }

    /// <summary>
    /// <see cref="IConnectionInitializer"/>
    /// </summary>
    /// <param name="maxNbAttempt"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task CreateAndInitializeConnectionAsync (int maxNbAttempt, CancellationToken? cancellationToken = null)
    {
      CreateAndInitializeConnection (cancellationToken);
      return Task.CompletedTask;
    }
  }
}
