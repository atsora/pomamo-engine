// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lemoine.Threading
{
  /// <summary>
  /// Interface for services that are based on an initialization thread
  /// </summary>
  public interface IThreadService
  {
    /// <summary>
    /// Initialize the thread
    /// </summary>
    void Initialize ();

    /// <summary>
    /// To run when the service stops
    /// </summary>
    void OnStop ();
  }

  /// <summary>
  /// Interface for services that are based on an initialization thread
  /// when the initialize method is asynchronous
  /// </summary>
  public interface IThreadServiceAsync : IThreadService
  {
    /// <summary>
    /// Initialize the thread
    /// </summary>
    System.Threading.Tasks.Task InitializeAsync (CancellationToken stoppingToken);
  }
}
