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
  /// Request class to get a machine from its id
  /// </summary>
  public sealed class MachineFromId
    : IRequest<IMachine>
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (MachineFromId).FullName);

    #region Getters / Setters
    /// <summary>
    /// Machine Id
    /// </summary>
    int MachineId { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineId">not 0 normally</param>
    public MachineFromId (int machineId)
    {
      Debug.Assert (0 != machineId);

      this.MachineId = machineId;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>Group or null if not found</returns>
    public IMachine Get ()
    {
      log.DebugFormat ("Get: " +
                       "machine id {0}",
                       this.MachineId);

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        return ModelDAO.ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (this.MachineId);
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<IMachine> GetAsync ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetAsync: machine id {this.MachineId}");
      }

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        return await ModelDAO.ModelDAOHelper.DAOFactory.MachineDAO
          .FindByIdAsync (this.MachineId);
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.Machine.MachineFromId." + this.MachineId;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<IMachine> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (IMachine data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }
    #endregion // IRequest implementation
  }
}
