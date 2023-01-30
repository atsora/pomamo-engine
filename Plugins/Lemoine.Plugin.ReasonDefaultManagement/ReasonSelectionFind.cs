// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Business;
using Lemoine.Collections;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.ReasonDefaultManagement
{
  /// <summary>
  /// Get the reason selection items in table reasonselection only for a specific machine mode and machine observation state
  /// </summary>
  public class ReasonSelectionFind
    : IRequest<IEnumerable<IReasonSelection>>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ReasonSelectionFind).FullName);

    #region Getters / Setters
    /// <summary>
    /// Reference to the machine mode
    /// 
    /// Not null
    /// </summary>
    public IMachineMode MachineMode { get; private set; }

    /// <summary>
    /// Reference to the machine observation state
    /// 
    /// Not null
    /// </summary>
    public IMachineObservationState MachineObservationState { get; private set; }

    /// <summary>
    /// Reference to the machine
    /// 
    /// If null, applies to all the machines
    /// </summary>
    public IMachine Machine { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineMode">not null</param>
    /// <param name="machineObservationState">not null</param>
    public ReasonSelectionFind (IMachineMode machineMode, IMachineObservationState machineObservationState)
    {
      Debug.Assert (null != machineMode);
      Debug.Assert (null != machineObservationState);

      this.MachineMode = machineMode;
      this.MachineObservationState = machineObservationState;
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    public ReasonSelectionFind (IMachine machine, IMachineMode machineMode, IMachineObservationState machineObservationState)
      : this (machineMode, machineObservationState)
    {
      this.Machine = machine;
    }
    #endregion // Constructors

    #region IRequest implementation

    /// <summary>
    /// IRequest implementation
    /// </summary>
    /// <returns>List of IReasonSelection</returns>
    public IEnumerable<IReasonSelection> Get ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.Reason.ReasonSelectionFind")) {
          if (null != this.Machine) {
            return ModelDAOHelper.DAOFactory.ReasonSelectionDAO
              .FindWith (this.Machine, this.MachineMode, this.MachineObservationState);
          }
          else {
            return ModelDAOHelper.DAOFactory.ReasonSelectionDAO
              .FindWith (this.MachineMode, this.MachineObservationState);
          }
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<IReasonSelection>> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// IRequest implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      string cacheKey = "Business.Reason.ReasonSelectionFind."
        + ((IDataWithId<int>)MachineMode).Id
        + "."
        + ((IDataWithId<int>)MachineObservationState).Id;
      if (null != this.Machine) {
        cacheKey += "." + this.Machine.Id;
      }
      return cacheKey;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<IEnumerable<IReasonSelection>> data)
    {
      return true;
    }

    /// <summary>
    /// IRequest implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (IEnumerable<IReasonSelection> data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }

    #endregion
  }

}
