// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Business
{
  /// <summary>
  /// Service interface for Lemoine.Business
  /// </summary>
  public interface IService
  {
    /// <summary>
    /// Get the result of a request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    T Get<T> (IRequest<T> request);

    /// <summary>
    /// Get asynchronously the result of a request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task<T> GetAsync<T> (IRequest<T> request);

    /// <summary>
    /// Get the data in cache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request"></param>
    /// <returns>null if no cache data</returns>
    CacheValue<T> GetCacheData<T> (IRequest<T> request);
  }
}
