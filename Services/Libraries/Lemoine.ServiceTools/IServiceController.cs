// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lemoine.ServiceTools
{
  /// <summary>
  /// Common methods to service controllers (on Windows, Linux, ...)
  /// </summary>
  public interface IServiceController
  {
    /// <summary>
    /// Service name
    /// </summary>
    string ServiceName { get; }

    /// <summary>
    /// Check if the service really exists
    /// </summary>
    bool IsInstalled { get; }

    /// <summary>
    /// Return true if the service is running or at least starting
    /// </summary>
    bool Running { get; }

    /// <summary>
    /// Start a service asynchronously, whichever its status was
    /// </summary>
    /// <returns></returns>
    Task StartServiceAsync (CancellationToken cancellationToken = default);
  }
}
