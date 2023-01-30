// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;

namespace Lemoine.ModelDAO.Interfaces
{
  /// <summary>
  /// Interface to initialize a connection
  /// </summary>
  public interface IConnectionInitializer
  {
    /// <summary>
    /// Create and initialize the connection asynchronously
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task CreateAndInitializeConnectionAsync (CancellationToken? cancellationToken = null);

    /// <summary>
    /// Create and initialize the connection asynchronously
    /// </summary>
    /// <param name="maxNbAttempt">If reached, an exception is thrown</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task CreateAndInitializeConnectionAsync (int maxNbAttempt, CancellationToken? cancellationToken = null);

    /// <summary>
    /// Create and initialize the connection synchronously
    /// </summary>
    /// <param name="cancellationToken"></param>
    void CreateAndInitializeConnection (CancellationToken? cancellationToken = null);

    /// <summary>
    /// Create and initialize the connection synchronously
    /// </summary>
    /// <param name="maxNbAttempt">If reached, an exception is thrown</param>
    /// <param name="cancellationToken"></param>
    void CreateAndInitializeConnection (int maxNbAttempt, CancellationToken? cancellationToken = null);
  }

  /// <summary>
  /// Extensions to interface <see cref="IConnectionInitializer"/>
  /// </summary>
  public static class ConnectionInitializerExtensions
  {
    /// <summary>
    /// Initialize the connection if it has not been set yet asynchronously
    /// </summary>
    public static async System.Threading.Tasks.Task InitializeAsync (this IConnectionInitializer connectionInitializer, CancellationToken? cancellationToken = null)
    {
      if (null == Lemoine.ModelDAO.ModelDAOHelper.ModelFactory) {
        await connectionInitializer.CreateAndInitializeConnectionAsync (cancellationToken: cancellationToken);
      }
    }

    /// <summary>
    /// Initialize the connection if it has not been set yet asynchronously
    /// using a maximum number of database connection attempts
    /// </summary>
    public static async System.Threading.Tasks.Task InitializeAsync (this IConnectionInitializer connectionInitializer, int maxNbAttempt, CancellationToken? cancellationToken = null)
    {
      if (null == Lemoine.ModelDAO.ModelDAOHelper.ModelFactory) {
        await connectionInitializer.CreateAndInitializeConnectionAsync (maxNbAttempt, cancellationToken: cancellationToken);
      }
    }

    /// <summary>
    /// Initialize the connection if it has not been set yet synchronously
    /// </summary>
    /// <param name="connectionInitializer"></param>
    /// <param name="cancellationToken"></param>
    public static void Initialize (this IConnectionInitializer connectionInitializer, CancellationToken? cancellationToken = null)
    {
      if (null == Lemoine.ModelDAO.ModelDAOHelper.ModelFactory) {
        connectionInitializer.CreateAndInitializeConnection (cancellationToken: cancellationToken);
      }
    }
  }
}
