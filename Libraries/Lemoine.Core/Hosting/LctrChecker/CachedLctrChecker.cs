// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lemoine.Core.Log;
using Lemoine.Threading;

namespace Lemoine.Core.Hosting.LctrChecker
{
  /// <summary>
  /// CachedLctrChecker
  /// </summary>
  public class CachedLctrChecker : ILctrChecker
  {
    readonly ILog log = LogManager.GetLogger (typeof (CachedLctrChecker).FullName);

    readonly ILctrChecker m_lctrChecker;
    volatile bool m_cacheValid = false;
    bool m_isLctr = false;
    readonly SemaphoreSlim m_semaphore = new SemaphoreSlim (1, 1);

    /// <summary>
    /// Constructor
    /// </summary>
    public CachedLctrChecker (ILctrChecker lctrChecker)
    {
      Debug.Assert (null != lctrChecker);

      m_lctrChecker = lctrChecker;
    }

    /// <summary>
    /// <see cref="ILctrChecker"/>
    /// </summary>
    /// <returns></returns>
    public bool IsLctr ()
    {
      if (!m_cacheValid) {
        using (var semaphoreHolder = SemaphoreSlimHolder.Create (m_semaphore)) {
          if (!m_cacheValid) {
            m_isLctr = m_lctrChecker.IsLctr ();
            m_cacheValid = true;
          }
        }
      }
      return m_isLctr;
    }

  }
}
