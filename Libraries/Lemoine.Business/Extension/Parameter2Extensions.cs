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
  /// Request class to get extensions (from cache) that depend on two parameters
  /// </summary>
  /// <typeparam name="T">Response type</typeparam>
  /// <typeparam name="U">Parameter 1 type</typeparam>
  /// <typeparam name="V">Parameter 2 type</typeparam>
  public class Parameter2Extensions<T, U, V>
    : IRequest<IEnumerable<T>>
    where T : Lemoine.Extensions.IExtension
  {
    U m_parameter1;
    V m_parameter2;

    Func<T, U, V, bool> m_filter = ((ext, u, v) => true);
    Func<U, V, string> m_parameterKey;

    static readonly ILog log = LogManager.GetLogger (typeof (GlobalExtensions<T>).FullName);

    /// <summary>
    /// Filter
    /// </summary>
    protected Func<T, U, V, bool> Filter
    {
      get { return m_filter; }
      set { m_filter = value; }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="parameter1"></param>
    /// <param name="parameter2"></param>
    /// <param name="parameterKey"></param>
    public Parameter2Extensions (U parameter1, V parameter2, Func<U, V, string> parameterKey)
      : base ()
    {
      m_parameter1 = parameter1;
      m_parameter2 = parameter2;
      m_parameterKey = parameterKey;
    }

    /// <summary>
    /// Constructor with a filter lambda function
    /// 
    /// To be used when the Initialize method is available in the extension and returns true
    /// </summary>
    /// <param name="parameter1"></param>
    /// <param name="parameter2"></param>
    /// <param name="parameterKey"></param>
    /// <param name="filter"></param>
    public Parameter2Extensions (U parameter1, V parameter2, Func<U, V, string> parameterKey, Func<T, U, V, bool> filter)
      : this (parameter1, parameter2, parameterKey)
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
        .Where (ext => m_filter (ext, m_parameter1, m_parameter2))
        .ToList (); // ToList is mandatory else the result of the Linq command is not cached
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<T>> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public virtual string GetCacheKey ()
    {
      var cacheKey = "Business.Extension.ParameterExtensions.";
      cacheKey += GetExtensionTypeName ();
      cacheKey += "." + m_parameterKey (m_parameter1, m_parameter2);
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
