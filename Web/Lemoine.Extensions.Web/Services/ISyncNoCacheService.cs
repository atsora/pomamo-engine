// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Web.Services
{
  /// <summary>
  /// Interface to implement to use, see NServiceKitNoCacheService{InputDTO}
  /// </summary>
  public interface ISyncNoCacheService<InputDTO>
  {
    /// <summary>
    /// Get implementation (asynchronous)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task<object> Get (InputDTO request);

    /// <summary>
    /// Get without cache
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    object GetWithoutCache (InputDTO request);
  }
}
