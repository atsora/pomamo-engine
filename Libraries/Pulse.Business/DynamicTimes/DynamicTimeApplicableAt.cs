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
  /// Request class to check if a dynamic time is applicable at a specific date/time
  /// </summary>
  internal sealed class DynamicTimeApplicableAt
    : IRequest<DynamicTimeApplicableStatus>
  {
    static readonly string DEFAULT_TIMEOUT_KEY = "Business.DynamicTimeApplicableAt.DefaultTimeout";
    static readonly TimeSpan DEFAULT_TIMEOUT_DEFAULT = CacheTimeOut.CurrentLong.GetTimeSpan ();

    #region Members
    readonly IDynamicTimeExtension m_extension;
    readonly DateTime m_dateTime;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (DynamicTimeApplicable).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="extension"></param>
    /// <param name="dateTime"></param>
    public DynamicTimeApplicableAt (IDynamicTimeExtension extension, DateTime dateTime)
    {
      m_extension = extension;
      m_dateTime = dateTime;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public DynamicTimeApplicableStatus Get ()
    {
      return m_extension.IsApplicableAt (m_dateTime);
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<DynamicTimeApplicableStatus> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return string.Format ("DynamicTimeApplicableAt.{0}.{1}.{2}",
        m_extension.Name, m_extension.Machine.Id, m_dateTime);
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (DynamicTimeApplicableStatus data)
    {
      switch (data) {
      case DynamicTimeApplicableStatus.Always:
      case DynamicTimeApplicableStatus.Never:
        return CacheTimeOut.Static.GetTimeSpan ();
      case DynamicTimeApplicableStatus.YesAtDateTime:
      case DynamicTimeApplicableStatus.NoAtDateTime:
      case DynamicTimeApplicableStatus.Pending:
      default:
        return Lemoine.Info.ConfigSet.LoadAndGet (DEFAULT_TIMEOUT_KEY, DEFAULT_TIMEOUT_DEFAULT);
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<DynamicTimeApplicableStatus> data)
    {
      return true;
    }
    #endregion // IRequest implementation
  }
}
