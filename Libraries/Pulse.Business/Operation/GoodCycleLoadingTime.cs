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
  public sealed class GoodCycleLoadingTime
    : IRequest<GoodCycleExtensionResponse>
  {
    #region Members
    readonly DateTime m_start;
    readonly IOperationCycle m_operationCycle;
    readonly IMonitoredMachine m_monitoredMachine;
    readonly double m_multiplicator;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (GoodCycleLoadingTime).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="operationCycle">not null, start time is defined</param>
    /// <param name="start">start time of the loading period</param>
    /// <param name="multiplicator"></param>
    public GoodCycleLoadingTime (IOperationCycle operationCycle, DateTime start, double multiplicator)
    {
      Debug.Assert (null != operationCycle);
      Debug.Assert (operationCycle.Begin.HasValue);

      m_start = start;
      m_operationCycle = operationCycle;
      m_multiplicator = multiplicator;

      var monitoredMachineRequest = new Lemoine.Business.Machine
        .MonitoredMachineFromId (operationCycle.Machine.Id);
      m_monitoredMachine = Lemoine.Business.ServiceProvider
        .Get (monitoredMachineRequest);
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

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
      var firstExtension = goodCycleExtensions
        .OrderByDescending (ext => ext.Score)
        .First ();
      return firstExtension.IsGoodLoadingTime (m_operationCycle, m_monitoredMachine, m_start, m_operationCycle.Begin.Value, m_multiplicator);
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<GoodCycleExtensionResponse> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.Operation.GoodCycleLoadingTime." + m_operationCycle.Id
        + "." + m_multiplicator.ToString ();
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
