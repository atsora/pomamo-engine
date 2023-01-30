// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lemoine.Core.Hosting
{
  /// <summary>
  /// Interface to implement by a console application
  /// </summary>
  public interface IConsoleRunner<TOptions>
  {
    /// <summary>
    /// Set the options
    /// </summary>
    /// <param name="options"></param>
    void SetOptions (TOptions options);

    /// <summary>
    /// Main method
    /// </summary>
    Task RunConsoleAsync (CancellationToken cancellationToken = default);
  }
}
