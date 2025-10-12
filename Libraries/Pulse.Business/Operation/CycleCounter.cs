// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Core.Log;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.Extensions.Business.Operation;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Request class to get number of comleted cycles in a range for the specified machine
  /// </summary>
  public sealed class CycleCounter
    : IRequest<IEnumerable<CycleCounterValue>>
  {
    static readonly string IN_PROGRESS_CACHE_TIME_OUT_KEY = "Business.Operation.CycleCounter.InProgressCacheTimeOut";
    static readonly TimeSpan IN_PROGRESS_CACHE_TIME_OUT_DEFAULT = TimeSpan.FromSeconds (5);

    readonly IMonitoredMachine m_machine;
    readonly UtcDateTimeRange m_range;
    readonly UtcDateTimeRange m_preLoadRange;

    readonly ILog log = LogManager.GetLogger (typeof (CycleCounter).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="preLoadRange"></param>
    public CycleCounter (IMonitoredMachine machine, UtcDateTimeRange range, UtcDateTimeRange preLoadRange = null)
    {
      Debug.Assert (null != machine);

      m_machine = machine;
      m_range = range;
      m_preLoadRange = preLoadRange;

      log = LogManager.GetLogger ($"{typeof (CycleCounter).FullName}.{machine.Id}");
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public IEnumerable<CycleCounterValue> Get ()
    {
      return System.Threading.Tasks.Task.Run (() => GetAsync ()).GetAwaiter ().GetResult ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IEnumerable<CycleCounterValue>> GetAsync ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetAsync: range={m_range}");
      }

      var extensionsRequest = new Lemoine.Business.Extension
        .MonitoredMachineExtensions<ICycleCounterExtension> (m_machine, (ext, m) => ext.Initialize (m));
      var extensions = await Lemoine.Business.ServiceProvider
        .GetAsync (extensionsRequest);
      if (!extensions.Any ()) {
        if (log.IsErrorEnabled) {
          log.ErrorFormat ("GetAsync: no extension was registered");
        }
        return new List<CycleCounterValue> ();
      }
      foreach (var extension in extensions.OrderByDescending (ext => ext.Score)) {
        try {
          var result = await extension.GetNumberOfCyclesAsync (m_range, m_preLoadRange);
          if (log.IsDebugEnabled) {
            log.Debug ($"GetAsync: get values from extension {extension}");
          }
          return result;
        }
        catch (Exception ex) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetAsync: extension {extension} returned an exception", ex);
          }
        }
      }

      if (log.IsErrorEnabled) {
        log.ErrorFormat ("GetAsync: no extension returned a valid value");
      }
      return new List<CycleCounterValue> ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return $"Business.Operation.CycleCounter.{m_machine.Id}.{m_range}";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (IEnumerable<CycleCounterValue> data)
    {
      if (data.Any (x => x.InProgress)) {
        // InProgress is a secondary data. The other data are managed by cache invalidation
        if (log.IsDebugEnabled) {
          log.Debug ($"GetCacheTimeout: data in progress");
        }
        return Lemoine.Info.ConfigSet
          .LoadAndGet (IN_PROGRESS_CACHE_TIME_OUT_KEY, IN_PROGRESS_CACHE_TIME_OUT_DEFAULT);
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetCacheTimeout: past");
        }
        return CacheTimeOut.OldLong.GetTimeSpan ();
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<IEnumerable<CycleCounterValue>> data)
    {
      return true;
    }
    #endregion // IRequest implementation
  }
}
