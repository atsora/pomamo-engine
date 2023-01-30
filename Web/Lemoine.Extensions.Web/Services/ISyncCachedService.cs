// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Cache;
using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Web.Services
{
  /// <summary>
  /// Interface to implement to use, see NServiceKitCachedService{InputDTO}
  /// </summary>
  public interface ISyncCachedService<InputDTO>
  {
    /// <summary>
    /// Default cache time out
    /// </summary>
    CacheTimeOut DefaultCacheTimeOut { get; }

    /// <summary>
    /// Get without cache for NServiceKit
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    object GetWithoutCache (InputDTO request);
  }
}
