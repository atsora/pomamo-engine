// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.DynamicTimes;
using System.Threading.Tasks;

namespace Lemoine.Business.DynamicTimes
{
  /// <summary>
  /// Request class to check if a dynamic time is applicable without specifying any time
  /// 
  /// If the dynamic time is applicable only at some specific times, then true is returned
  /// </summary>
  internal sealed class DynamicTimeApplicable
    : IRequest<bool>
  {
    #region Members
    readonly IDynamicTimeExtension m_extension;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (DynamicTimeApplicable).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="extension"></param>
    public DynamicTimeApplicable (IDynamicTimeExtension extension)
    {
      m_extension = extension;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public bool Get ()
    {
      return m_extension.IsApplicable ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<bool> GetAsync ()
    {
      return await System.Threading.Tasks.Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return string.Format ("DynamicTimeApplicable.{0}.{1}",
        m_extension.Name, m_extension.Machine.Id);
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (bool data)
    {
      return CacheTimeOut.Static.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<bool> data)
    {
      return true;
    }
    #endregion // IRequest implementation
  }
}
