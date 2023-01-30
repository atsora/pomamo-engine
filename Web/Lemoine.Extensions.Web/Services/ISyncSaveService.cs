// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Cache;
using Lemoine.Extensions.Web.Interfaces;
using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Web.Services
{
  /// <summary>
  /// Interface to implement to use, see cref="NServiceKitSaveService{InputDTO}"
  /// </summary>
  public interface ISyncSaveService<InputDTO>: IRemoteIpSupport
  {
    /// <summary>
    /// Reference to the cache client
    /// 
    /// Be careful, this is not necessarily set, it can be null
    /// </summary>
    ICacheClient CacheClient { get; }

    /// <summary>
    /// Get
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    object GetSync (InputDTO request);
  }
}
