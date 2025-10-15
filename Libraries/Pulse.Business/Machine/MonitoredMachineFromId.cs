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
using Lemoine.Extensions.Business.Group;
using System.Threading.Tasks;

namespace Lemoine.Business.Machine
{
  /// <summary>
  /// Request class to get a monitored machine from its id
  /// </summary>
  public sealed class MonitoredMachineFromId
    : IRequest<IMonitoredMachine>
  {
    readonly IMachine m_machine;

    static readonly ILog log = LogManager.GetLogger (typeof (MonitoredMachineFromId).FullName);

    /// <summary>
    /// Machine Id
    /// </summary>
    int MachineId { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineId">not 0 normally</param>
    public MonitoredMachineFromId (int machineId)
    {
      Debug.Assert (0 != machineId);

      this.MachineId = machineId;
      m_machine = null;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    public MonitoredMachineFromId (IMachine machine)
    {
      Debug.Assert (null != machine);

      this.MachineId = machine.Id;
      m_machine = machine;
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>Group or null if not found</returns>
    public IMonitoredMachine Get ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: machine id {this.MachineId}");
      }

      if ((null != m_machine) && (m_machine is IMonitoredMachine)) {
        return (IMonitoredMachine)m_machine;
      }

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        return ModelDAO.ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (this.MachineId);
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<IMonitoredMachine> GetAsync ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetAsync: machine id {this.MachineId}");
      }

      if ((null != m_machine) && (m_machine is IMonitoredMachine)) {
        return (IMonitoredMachine)m_machine;
      }

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        return await ModelDAO.ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindByIdAsync (this.MachineId);
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.Machine.MonitoredMachineFromId." + this.MachineId;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<IMonitoredMachine> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (IMonitoredMachine data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }
    #endregion // IRequest implementation
  }
}
