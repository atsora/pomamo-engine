// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IMachineModeDefaultReasonn.
  /// </summary>
  public interface IMachineModeDefaultReasonDAO: IGenericUpdateDAO<IMachineModeDefaultReason, int>
  {
    /// <summary>
    /// Get all the items with an early fetch of the reason and of the reason group
    /// </summary>
    /// <returns></returns>
    IEnumerable<IMachineModeDefaultReason> FindWithReasonGroup ();

    /// <summary>
    /// Find all the entities given a MachineMode and a MachineObservationState
    /// 
    /// with some children that are eager fetched
    /// </summary>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    /// <returns></returns>
    IList<IMachineModeDefaultReason> FindWithForConfig (IMachineMode machineMode,
                                                        IMachineObservationState machineObservationState);
    
    /// <summary>
    /// Find all the entities given one or more MachineMode and one or more MachineObservationState
    /// 
    /// with some children that are eager fetched
    /// </summary>
    /// <param name="machineModes"></param>
    /// <param name="machineObservationStates"></param>
    /// <returns></returns>
    IList<IMachineModeDefaultReason> FindWithForConfig (IList<IMachineMode> machineModes,
                                                        IList<IMachineObservationState> machineObservationStates);
    
    /// <summary>
    /// Find all the entities given a machine mode
    /// </summary>
    /// <param name="machineMode"></param>
    /// <returns></returns>
    IList<IMachineModeDefaultReason> FindByMachineMode(IMachineMode machineMode);

    /// <summary>
    /// Get all the items sorted for a specified MonitoredMachine
    ///
    /// Note: this is registered to be cacheable.
    ///       Do not consider here, the children are always automatically fetched because of the cacheable property.
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    IEnumerable<IMachineModeDefaultReason> FindWith (IMachine machine);

    /// <summary>
    /// Get all the items sorted for a specified MonitoredMachine, MachineMode and MachineObservationState
    /// 
    /// The result is ordered by ascending duration
    ///
    /// The request is recursive. If no configuration was found for the specified machine mode,
    /// another try is made with the parent
    ///
    /// Note: this is registered to be cacheable.
    ///       Do not consider here, the children are always automatically fetched because of the cacheable property.
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineMode">not null</param>
    /// <param name="machineObservationState">not null</param>
    /// <returns></returns>
    IEnumerable<IMachineModeDefaultReason> FindWith (IMachine machine,
                                                     IMachineMode machineMode,
                                                     IMachineObservationState machineObservationState);

    /// <summary>
    /// Find the first entity that matches a Machine, a MachineMode, a MachineObservationState and a duration
    /// 
    /// The request is recursive. If no configuration was found for the specified machine mode,
    /// another try is made with the parent
    ///
    /// null is returned in case no matching entity could be found, which corresponds to an unexpected configuration
    ///
    /// Note: this is registered to be cacheable.
    ///       Do not consider here, the children are always automatically fetched because of the cacheable property.
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineMode">not null</param>
    /// <param name="machineObservationState">not null</param>
    /// <param name="duration"></param>
    /// <returns></returns>
    IMachineModeDefaultReason FindWith (IMachine machine,
                                        IMachineMode machineMode,
                                        IMachineObservationState machineObservationState,
                                        TimeSpan duration);
  }
}
