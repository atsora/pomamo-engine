// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lemoine.Core.Log;

namespace Lemoine.Threading
{
  /// <summary>
  /// Exception raised if the timeout is reached
  /// </summary>
  public sealed class SemaphoreSlimTimeoutException : Exception
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public SemaphoreSlimTimeoutException ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public SemaphoreSlimTimeoutException (string message)
        : base (message)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public SemaphoreSlimTimeoutException (string message, Exception inner)
        : base (message, inner)
    {
    }
  }

  /// <summary>
  /// SemaphoreSlimHolder
  /// </summary>
  public sealed class SemaphoreSlimHolder : IDisposable
#if NETSTANDARD2_1 || NETCOREAPP
    , IAsyncDisposable
#endif // NETSTANDARD2_1 || NETCOREAPP
  {
    readonly ILog log = LogManager.GetLogger (typeof (SemaphoreSlimHolder).FullName);

    readonly SemaphoreSlim m_semaphoreSlim;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    SemaphoreSlimHolder (SemaphoreSlim semaphoreSlim)
    {
      m_semaphoreSlim = semaphoreSlim;
    }
    #endregion // Constructors

    /// <summary>
    /// Create a new SemaphoreSlim holder
    /// </summary>
    /// <param name="semaphoreSlim"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static SemaphoreSlimHolder Create (SemaphoreSlim semaphoreSlim, CancellationToken cancellationToken = default)
    {
      var holder = new SemaphoreSlimHolder (semaphoreSlim);
      semaphoreSlim.Wait (cancellationToken);
      return holder;
    }

    /// <summary>
    /// Create a new SemaphoreSlim holder
    /// </summary>
    /// <param name="semaphoreSlim"></param>
    /// <param name="timeout"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="SemaphoreSlimTimeoutException">timeout reached</exception>
    /// <returns></returns>
    public static SemaphoreSlimHolder Create (SemaphoreSlim semaphoreSlim, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
      var holder = new SemaphoreSlimHolder (semaphoreSlim);
      if (!semaphoreSlim.Wait (timeout, cancellationToken)) {
        throw new SemaphoreSlimTimeoutException ();
      }
      return holder;
    }

#if NETSTANDARD
    /// <summary>
    /// Create a new SemaphoreSlim holder
    /// </summary>
    /// <param name="semaphoreSlim"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async System.Threading.Tasks.Task<SemaphoreSlimHolder> CreateAsync (SemaphoreSlim semaphoreSlim, CancellationToken cancellationToken = default)
    {
      var holder = new SemaphoreSlimHolder (semaphoreSlim);
      await semaphoreSlim.WaitAsync (cancellationToken);
      return holder;
    }

    /// <summary>
    /// Create a new SemaphoreSlim holder
    /// </summary>
    /// <param name="semaphoreSlim"></param>
    /// <param name="timeout"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async System.Threading.Tasks.Task<SemaphoreSlimHolder> CreateAsync (SemaphoreSlim semaphoreSlim, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
      var holder = new SemaphoreSlimHolder (semaphoreSlim);
      var success = await semaphoreSlim.WaitAsync (timeout, cancellationToken);
      if (!success) {
        throw new SemaphoreSlimTimeoutException ();
      }
      return holder;
    }
#endif // NETSTANDARD

    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    public void Dispose ()
    {
      m_semaphoreSlim.Release ();
    }

#if NETSTANDARD2_1 || NETCOREAPP
    /// <summary>
    /// <see cref="IAsyncDisposable"/>
    /// </summary>
    /// <returns></returns>
    public async System.Threading.Tasks.ValueTask DisposeAsync ()
    {
      await System.Threading.Tasks.Task.Delay (0);
      m_semaphoreSlim.Release ();
    }
#endif // NETSTANDARD2_1 || NETCOREAPP
  }
}
