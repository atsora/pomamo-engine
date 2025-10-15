// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.Extensions.Business;
using System.Linq;
using System.Threading.Tasks;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Request class to know if a cycle is a good cycle
  /// </summary>
  public sealed class GoodCycleMachiningTime
    : IRequest<GoodCycleExtensionResponse>
  {
    #region Members
    readonly IOperationCycle m_operationCycle;
    readonly double m_multiplicator;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (GoodCycleMachiningTime).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="operationCycle">not null</param>
    /// <param name="multiplicator"></param>
    public GoodCycleMachiningTime (IOperationCycle operationCycle, double multiplicator)
    {
      Debug.Assert (null != operationCycle);

      m_operationCycle = operationCycle;
      m_multiplicator = multiplicator;
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// 
    /// null is returned if no extension was registered.
    /// Else the first matching extension is considered
    /// </summary>
    /// <returns></returns>
    public GoodCycleExtensionResponse Get ()
    {
      var goodCycleExtensionsRequest = new Lemoine.Business.Extension
        .MachineExtensions<IGoodCycleExtension> (m_operationCycle.Machine, (ext, m) => ext.Initialize (m));
      var goodCycleExtensions = Lemoine.Business.ServiceProvider
        .Get (goodCycleExtensionsRequest);
      if (!goodCycleExtensions.Any ()) {
        if (log.IsErrorEnabled) {
          log.ErrorFormat ("Get: no good cycle extension was registered");
        }
        return GoodCycleExtensionResponse.KO;
      }
      foreach (var ext in goodCycleExtensions.OrderByDescending (ext => ext.Score)) {
        var result = ext.IsGood (m_operationCycle, m_multiplicator);
        if (result != GoodCycleExtensionResponse.NOT_APPLICABLE) {
          if (log.IsDebugEnabled) {
            log.Debug ($"Get: extension {ext} returned {result}");
          }
          return result;
        }
        else if (log.IsDebugEnabled) {
          log.Debug ($"Get: extension {ext} returned NOT_APPLICABLE, try the next one");
        }
      }
      log.Error ($"Get: all the {goodCycleExtensions.Count ()} extensions returned NOT_APPLICABLE");
      return GoodCycleExtensionResponse.NOT_APPLICABLE;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<GoodCycleExtensionResponse> GetAsync ()
    {
      var goodCycleExtensionsRequest = new Lemoine.Business.Extension
        .MachineExtensions<IGoodCycleExtension> (m_operationCycle.Machine, (ext, m) => ext.Initialize (m));
      var goodCycleExtensions = await Lemoine.Business.ServiceProvider
        .GetAsync (goodCycleExtensionsRequest);
      if (!goodCycleExtensions.Any ()) {
        if (log.IsErrorEnabled) {
          log.ErrorFormat ("GetAsync: no good cycle extension was registered");
        }
        return GoodCycleExtensionResponse.KO;
      }
      foreach (var ext in goodCycleExtensions.OrderByDescending (ext => ext.Score)) {
        var result = ext.IsGood (m_operationCycle, m_multiplicator);
        if (result != GoodCycleExtensionResponse.NOT_APPLICABLE) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetAsync: extension {ext} returned {result}");
          }
          return result;
        }
        else if (log.IsDebugEnabled) {
          log.Debug ($"GetAsync: extension {ext} returned NOT_APPLICABLE, try the next one");
        }
      }
      log.Error ($"GetAsync: all the {goodCycleExtensions.Count ()} extensions returned NOT_APPLICABLE");
      return GoodCycleExtensionResponse.NOT_APPLICABLE;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.Operation.GoodCycleMachiningTime." + m_operationCycle.Id + "." + m_multiplicator;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (GoodCycleExtensionResponse data)
    {
      return CacheTimeOut.CurrentLong.GetTimeSpan ();
      // Note: not too long because there is a good chance the request is only run once
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<GoodCycleExtensionResponse> data)
    {
      return !data.Value.Equals (GoodCycleExtensionResponse.POSTPONE);
    }
    #endregion // IRequest implementation
  }
}
