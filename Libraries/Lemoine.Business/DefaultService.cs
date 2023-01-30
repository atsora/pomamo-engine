// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using Lemoine.Core.Log;

namespace Lemoine.Business
{
  /// <summary>
  /// Default service running directly the request
  /// </summary>
  internal sealed class DefaultService: IService
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (DefaultService).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public DefaultService ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get the result of a request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public T Get<T> (IRequest<T> request)
    {
      return request.Get ();
    }

    /// <summary>
    /// Get the result of a request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<T> GetAsync<T> (IRequest<T> request)
    {
      return request.GetAsync ();
    }

    /// <summary>
    /// Get the data in cache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request"></param>
    /// <returns></returns>
    public CacheValue<T> GetCacheData<T> (IRequest<T> request)
    {
      return null;
    }
    #endregion // Methods
  }
}
