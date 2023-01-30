// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NET6_0_OR_GREATER
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Microsoft.Extensions.Hosting;

namespace Lemoine.Threading
{
  /// <summary>
  /// TaskQueue
  /// </summary>
  public sealed class TaskBackgroundQueue
    : BackgroundService, IDisposable
  {
    readonly ILog log = LogManager.GetLogger (typeof (TaskBackgroundQueue).FullName);

    readonly ConcurrentQueue<Func<Task>> m_queue = new ConcurrentQueue<Func<Task>> ();
    readonly CancellationTokenSource m_completeTokenSource = new CancellationTokenSource ();
    readonly CancellationTokenSource m_completedSource = new CancellationTokenSource ();

    /// <summary>
    /// Constructor
    /// </summary>
    public TaskBackgroundQueue ()
    {
    }

    /// <summary>
    /// Run the queue
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync (CancellationToken stoppingToken)
    {
      try {
        using (var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource (m_completeTokenSource.Token, stoppingToken)) {
          while (!linkedTokenSource.IsCancellationRequested) {
            await EmptyQueueAsync (stoppingToken);
            if (m_completeTokenSource.IsCancellationRequested) {
              if (log.IsDebugEnabled) {
                log.Debug ("ExecuteAsync: complete requested");
              }
              await EmptyQueueAsync (stoppingToken);
              return;
            }
            try {
              await Task.Delay (100, linkedTokenSource.Token);
            }
            catch (TaskCanceledException) { 
            }
          }
          if (m_completeTokenSource.IsCancellationRequested) {
            await EmptyQueueAsync (stoppingToken);
          }
        }
        stoppingToken.ThrowIfCancellationRequested ();
      }
      catch (TaskCanceledException ex) {
        log.Warn ($"ExecuteAsync: stopping requested before the end", ex);
      }
      catch (Exception ex) {
        log.Error ($"ExecuteAsync: exception", ex);
      }
      finally {
        m_completedSource.Cancel ();
      }
    }

    async Task EmptyQueueAsync (CancellationToken stoppingToken)
    {
      while (m_queue.TryDequeue (out var task)) {
        await task ();
        stoppingToken.ThrowIfCancellationRequested ();
      }
    }

    /// <summary>
    /// Set it as completed
    /// </summary>
    public void Complete ()
    {
      m_completeTokenSource.Cancel ();
    }

    /// <summary>
    /// Wait the reamining tasks are completed
    /// </summary>
    /// <returns></returns>
    public bool WaitCompletion ()
    {
      while (!m_completedSource.IsCancellationRequested) {
        System.Threading.Thread.Sleep (100);
      }
      return true;
    }

    /// <summary>
    /// Wait the remaining tasks are completed
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> WaitCompletionAsync (CancellationToken cancellationToken = default)
    {
      using (var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource (m_completedSource.Token, cancellationToken)) {
        try {
          await Task.Delay (-1, linkedTokenSource.Token);
        }
        catch (TaskCanceledException) {
          return m_completedSource.IsCancellationRequested;
        }
      }
      return m_completedSource.IsCancellationRequested;
    }

    /// <summary>
    /// Add a new task
    /// </summary>
    /// <param name="task"></param>
    public void Add (Func<Task> task)
    {
      if (m_completeTokenSource.IsCancellationRequested || m_completedSource.IsCancellationRequested) {
        log.Fatal ($"Add: complete is already requested");
        throw new InvalidOperationException ($"Task background queue is already completed or requested to be completed");
      }
      m_queue.Enqueue (task);
    }

    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    public override void Dispose ()
    {
      m_completeTokenSource.Dispose ();
      m_completedSource.Dispose ();
      base.Dispose ();
    }
  }
}
#endif // NET6_0_OR_GREATER
