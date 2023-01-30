// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Web.Interfaces
{
  /// <summary>
  /// Interface for handlers that can be cached
  /// </summary>
  public interface ICachedHandler: IHandler
  {
    /// <summary>
    /// Get the cache time out
    /// </summary>
    /// <param name="pathQuery">Path concatenated with the query</param>
    /// <param name="inputDTO"></param>
    /// <param name="outputDTO"></param>
    /// <returns></returns>
    TimeSpan GetCacheTimeOut (string pathQuery, object inputDTO, object outputDTO);
  }
}
