// Copyright (C) 2009-2023 Lemoine Automation Technologies
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
  /// Request class to get extensions (from cache)
  /// </summary>
  public sealed class GlobalExtensions<T>
    : IRequest<IEnumerable<T>>
    where T : Lemoine.Extensions.IExtension
  {
    Func<T, bool> m_filter = (x => true);

    static readonly ILog log = LogManager.GetLogger (typeof (GlobalExtensions<T>).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public GlobalExtensions ()
      : base ()
    {
    }

    /// <summary>
    /// Constructor with a filter lambda function
    /// 
    /// To be used when the Initialize method is available in the extension and returns true
    /// </summary>
    /// <param name="filter"></param>
    public GlobalExtensions (Func<T, bool> filter)
      : this ()
    {
      m_filter = filter;
    }
    #endregion // Constructors

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>Group or null if not found</returns>
    public IEnumerable<T> Get ()
    {
      var allExtensions = Lemoine.Extensions.ExtensionManager
        .GetExtensions<T> ();
      var filteredExtensions = allExtensions.Where (m_filter)
        .ToList (); // ToList is mandatory else the result of the Linq command is not cached
      if (log.IsDebugEnabled && !filteredExtensions.Any ()) {
        log.Debug ($"Get: no extension is returned. Before filter, there were {allExtensions.Count ()} extensions");
      }
      return filteredExtensions;
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
    public string GetCacheKey ()
    {
      return $"Business.Extension.GlobalExtensions.{typeof (T).FullName}";
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
