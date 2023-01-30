// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;

namespace Lemoine.Business
{
  /// <summary>
  /// Interface for a request
  /// </summary>
  public interface IRequest<T>
  {
    /// <summary>
    /// Run the request
    /// </summary>
    /// <returns></returns>
    T Get ();

    /// <summary>
    /// Run the request asynchronously
    /// </summary>
    /// <returns></returns>
    Task<T> GetAsync ();
   
    /// <summary>
    /// Return a cache key
    /// </summary>
    /// <returns></returns>
    string GetCacheKey ();

    /// <summary>
    /// Check if a returned cache is valid,
    /// for example if the cache key considers only some parameters of the request
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    bool IsCacheValid (CacheValue<T> data);

    /// <summary>
    /// Return the cache timeout
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    TimeSpan GetCacheTimeout (T data);
  }
}
