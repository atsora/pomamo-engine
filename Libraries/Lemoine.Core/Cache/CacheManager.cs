// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lemoine.Core.Log;

namespace Lemoine.Core.Cache
{
  /// <summary>
  /// CacheManager (singleton)
  /// </summary>
  public sealed class CacheManager
  {
    readonly ILog log = LogManager.GetLogger (typeof (CacheManager).FullName);
    static readonly ILog slog = LogManager.GetLogger (typeof (CacheManager).FullName);

    ICacheClient m_cacheClient = null;
    readonly SemaphoreSlim m_semaphore = new SemaphoreSlim (1, 1);

    /// <summary>
    /// Reference to the cache client
    /// </summary>
    public static ICacheClient CacheClient {
      get
      {
        using (var semaphoreHolder = Lemoine.Threading.SemaphoreSlimHolder.Create (Instance.m_semaphore)) {
          return Instance.m_cacheClient;
        }
      }
      set
      {
        if (null != Instance.m_cacheClient) {
          slog.Warn ("CacheClient.set: the cache client was already set, it will be overriden");
        }
        using (var semaphoreHolder = Lemoine.Threading.SemaphoreSlimHolder.Create (Instance.m_semaphore)) {
          Instance.m_cacheClient = value;
        }
      }
    }

    #region Constructors
    /// <summary>
    /// Constructor (private because singleton)
    /// </summary>
    CacheManager ()
    {
    }
    #endregion // Constructors

    #region Instance
    static CacheManager Instance
    {
      get { return Nested.instance; }
    }

    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested ()
      {
      }

      internal static readonly CacheManager instance = new CacheManager ();
    }
    #endregion // Instance
  }
  }
