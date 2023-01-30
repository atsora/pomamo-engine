// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if !NET40

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;

namespace Lemoine.Core.Performance
{
  /// <summary>
  /// Initialize the <see cref="IPerfRecorder"/>
  /// </summary>
  public class PerfRecorderInitializer : IApplicationInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (PerfRecorderInitializer).FullName);

    readonly IPerfRecorder m_perfRecorder;

    /// <summary>
    /// Constructor
    /// </summary>
    public PerfRecorderInitializer (IPerfRecorder perfRecorder)
    {
      m_perfRecorder = perfRecorder;
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    public void InitializeApplication (CancellationToken cancellationToken = default)
    {
      m_perfRecorder.InitializeApplication (cancellationToken);
      PerfManager.SetRecorder (m_perfRecorder);
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      await m_perfRecorder.InitializeApplicationAsync (cancellationToken);
      PerfManager.SetRecorder (m_perfRecorder);
    }
  }
}
#endif // !NET40
