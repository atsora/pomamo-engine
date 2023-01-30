// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.Cache;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Lemoine.Collections;

namespace Lemoine.Business.Reason
{
  /// <summary>
  /// Get all the machine mode default reasons sorted for a specified MonitoredMachine, MachineMode and MachineObservationState
  /// 
  /// The result is ordered by ascending duration
  ///
  /// The request is recursive. If no configuration was found for the specified machine mode,
  /// another try is made with the parent
  /// </summary>
  public class MachineModeDefaultReasonFind
    : IRequest<IEnumerable<IMachineModeDefaultReason>>
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (MachineModeDefaultReasonFind).FullName);

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
    /// Not null
    /// </summary>
    public IMachine Machine { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineMode">not null</param>
    /// <param name="machineObservationState">not null</param>
    public MachineModeDefaultReasonFind (IMachine machine, IMachineMode machineMode, IMachineObservationState machineObservationState)
    {
      Debug.Assert (null != machine, "Machine null");
      Debug.Assert (null != machineMode, "Machine mode null");
      Debug.Assert (null != machineObservationState, "Machine observation state null");

      this.Machine = machine;
      this.MachineMode = machineMode;
      this.MachineObservationState = machineObservationState;
    }
    #endregion // Constructors

    #region Methods

    #endregion // Methods

    #region IRequest implementation

    /// <summary>
    /// IRequest implementation
    /// </summary>
    /// <returns>List of IMachineModeDefaultReason</returns>
    public IEnumerable<IMachineModeDefaultReason> Get ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.Reason.MachineModeDefaultReasonFind")) {
          var result = ModelDAOHelper.DAOFactory.MachineModeDefaultReasonDAO
            .FindWith (this.Machine, this.MachineMode, this.MachineObservationState);
          if (!result.Any ()) {
            log.Warn ($"Get: no default reason for machine id {this.Machine.Id} machine observation state id {this.MachineObservationState.Id} and machine mode id {this.MachineMode.Id}");
          }
          if (1 < result.Count (x => !x.MaximumDuration.HasValue)) {
            log.Fatal ($"GetMachineModeDefaultReasons: invalid configuration, different configurations with no maximum duration => return an empty list instead (invalid everything)");
            return new List<IMachineModeDefaultReason> ();
          }
          if (result
            .Where (x => x.MaximumDuration.HasValue)
            .GroupBy (x => x.MaximumDuration.Value)
            .Any (g => 1 < g.Count ())) {
            log.Fatal ($"GetMachineModeDefaultReasons: different configurations with the same maximum duration => return an empty list instead (invalid everything)");
            return new List<IMachineModeDefaultReason> ();
          }
          return result;
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<IMachineModeDefaultReason>> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// IRequest implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      string cacheKey = "Business.Reason.MachineModeDefaultReasonFind."
        + ((IDataWithId<int>)MachineMode).Id
        + "."
        + ((IDataWithId<int>)MachineObservationState).Id
        + "."
        + this.Machine.Id;
      return cacheKey;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<IEnumerable<IMachineModeDefaultReason>> data)
    {
      return true;
    }

    /// <summary>
    /// IRequest implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (IEnumerable<IMachineModeDefaultReason> data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }

    #endregion
  }
}
