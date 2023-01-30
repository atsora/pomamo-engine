// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.Collections;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Request class to get the standard cycle duration,
  /// that includes both the machining and loading duration,
  /// for a specific operation and machine
  /// </summary>
  public sealed class StandardCycleDuration
    : IRequest<TimeSpan?>
  {
    #region Members
    readonly IMonitoredMachine m_machine;
    readonly IOperation m_operation;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (StandardCycleDuration).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="operation">not null</param>
    public StandardCycleDuration (IMonitoredMachine machine, IOperation operation)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != operation);

      m_machine = machine;
      m_operation = operation;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan? Get ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: StandardCycleDuration");
      }

      var monitoredMachineBusinessRequest = new Lemoine.Business.Machine.MonitoredMachineFromId (m_machine.Id);
      var monitoredMachine = ServiceProvider.Get (monitoredMachineBusinessRequest);
      if (null == monitoredMachine) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Get: machine {m_machine.Id} is not monitored => return null");
        }
        return null;
      }

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var operation = ModelDAO.ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (((IDataWithId)m_operation).Id);
        var result = operation.GetStandardCycleDuration (monitoredMachine);
        if (log.IsDebugEnabled) {
          log.Debug ($"Get: standard cycle duration is {result} for operation={((IDataWithId)operation).Id} and machine={m_machine.Id}");
        }
        return result;
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<TimeSpan?> GetAsync ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetAsync: StandardCycleDuration");
      }

      var monitoredMachineBusinessRequest = new Lemoine.Business.Machine.MonitoredMachineFromId (m_machine.Id);
      var monitoredMachine = await ServiceProvider.GetAsync (monitoredMachineBusinessRequest);
      if (null == monitoredMachine) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetAsync: machine {m_machine.Id} is not monitored => return null");
        }
        return null;
      }

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var operation = await ModelDAO.ModelDAOHelper.DAOFactory.OperationDAO
          .FindByIdAsync (((IDataWithId)m_operation).Id);
        var result = operation.GetStandardCycleDuration (monitoredMachine);
        if (log.IsDebugEnabled) {
          log.Debug ($"GetAsync: standard cycle duration is {result} for operation={((IDataWithId)operation).Id} and machine={m_machine.Id}");
        }
        return result;
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return $"Business.Operation.StandardCycleDuration.{m_machine.Id}.{((IDataWithId)m_operation).Id}";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (TimeSpan? data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<TimeSpan?> data)
    {
      return true;
    }
    #endregion // IRequest implementation
  }
}
