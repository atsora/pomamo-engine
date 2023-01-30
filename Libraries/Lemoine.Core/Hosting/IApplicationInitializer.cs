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
  /// To implement to initialize an application
  /// </summary>
  public interface IApplicationInitializer
  {
    /// <summary>
    /// Initialize the application
    /// </summary>
    void InitializeApplication (CancellationToken cancellationToken = default);

    /// <summary>
    /// Initialize the application asynchronously
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task InitializeApplicationAsync (CancellationToken cancellationToken = default);
  }
}
