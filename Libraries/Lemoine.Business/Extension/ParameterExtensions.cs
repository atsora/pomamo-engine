// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Model;
using Lemoine.Core.Log;
using System.Threading.Tasks;

namespace Lemoine.Business.Extension
{
  /// <summary>
  /// Request class to get extensions (from cache) that depend on a parameter (machine)
  /// </summary>
  /// <typeparam name="T">Response type</typeparam>
  /// <typeparam name="U">Parameter type</typeparam>
  public class ParameterExtensions<T, U>
    : IRequest<IEnumerable<T>>
    where T : Lemoine.Extensions.IExtension
  {
    U m_parameter;

    Func<T, U, bool> m_filter = ( (x, m) => true);
    Func<U, string> m_parameterKey;

    static readonly ILog log = LogManager.GetLogger (typeof (GlobalExtensions<T>).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="parameterKey"></param>
    public ParameterExtensions (U parameter, Func<U, string> parameterKey)
      : base ()
    {
      m_parameter = parameter;
      m_parameterKey = parameterKey;
    }

    /// <summary>
    /// Constructor with a filter lambda function
    /// 
    /// To be used when the Initialize method is available in the extension and returns true
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="parameterKey"></param>
    /// <param name="filter"></param>
    public ParameterExtensions (U parameter, Func<U, string> parameterKey, Func<T, U, bool> filter)
      : this (parameter, parameterKey)
    {
      m_filter = filter;
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>Group or null if not found</returns>
    public virtual IEnumerable<T> Get ()
    {
      return Lemoine.Extensions.ExtensionManager
        .GetExtensions<T> ()
        .Where (ext => m_filter (ext, m_parameter))
        .ToList (); // ToList is mandatory else the result of the Linq command is not cached
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<T>> GetAsync ()
    {
      return await System.Threading.Tasks.Task.Run<IEnumerable<T>> (() => Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public virtual string GetCacheKey ()
    {
      var cacheKey = "Business.Extension.ParameterExtensions.";
      cacheKey += GetExtensionTypeName ();
      cacheKey += "." + m_parameterKey (m_parameter);
      return cacheKey;
    }

    /// <summary>
    /// Type name for the cache key
    /// </summary>
    /// <returns></returns>
    protected virtual string GetExtensionTypeName ()
    {
      return typeof (T).FullName;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<IEnumerable<T>> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (IEnumerable<T> data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }
    #endregion // IRequest implementation
  }
}
