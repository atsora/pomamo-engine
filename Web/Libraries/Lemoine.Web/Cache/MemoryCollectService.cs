// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;

using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;

namespace Lemoine.Web.Cache
{
  /// <summary>
  /// Description of MemoryCollectService
  /// </summary>
  public class MemoryCollectService
    : GenericNoCacheService<MemoryCollectRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MemoryCollectService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public MemoryCollectService ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (MemoryCollectRequestDTO request)
    {
      if (request.CompactLargeObjectHeap) {
        System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
      }
      if (request.CompactSmallObjectHeap || request.Optimized) {
        GCCollectionMode mode = request.Optimized
            ? GCCollectionMode.Optimized
            : GCCollectionMode.Forced;
        var compacting = request.CompactSmallObjectHeap;
        GC.Collect (2, mode, request.Blocking, compacting);
      }
      else {
        GC.Collect ();
      }

      return new OkDTO ("Memory collect requested");
    }
    #endregion // Methods
  }
}
