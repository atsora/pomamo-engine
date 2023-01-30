// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NETSTANDARD
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;

namespace Lemoine.Core.Hosting
{
  /// <summary>
  /// DummyApplicationInitializer
  /// </summary>
  public class DummyApplicationInitializer: IApplicationInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (DummyApplicationInitializer).FullName);

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    public void InitializeApplication (CancellationToken cancellationToken = default)
    {
      return;
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      return Task.CompletedTask;
    }
  
  }
}
#endif // NETSTANDARD
