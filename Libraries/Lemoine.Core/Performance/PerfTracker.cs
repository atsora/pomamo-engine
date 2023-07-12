// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Core.Performance
{
  /// <summary>
  /// PerfTracker
  /// </summary>
  public sealed class PerfTracker: IDisposable
  {
    readonly ILog log = LogManager.GetLogger (typeof (PerfTracker).FullName);

    readonly string m_key;
    readonly DateTime m_dateTime = DateTime.UtcNow;

    /// <summary>
    /// Constructor
    /// </summary>
    public PerfTracker (string key)
    {
      m_key = key;
    }

    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    public void Dispose ()
    {
      var duration = DateTime.UtcNow.Subtract (m_dateTime);
      PerfManager.Record (m_key, duration);
    }
  }
}
