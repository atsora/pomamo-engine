// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IReasonSelection.
  /// </summary>
  public interface IReasonSelectionDAO: IGenericUpdateDAO<IReasonSelection, int>
  {
    /// <summary>
    /// Find all the entities given a MachineMode and a MachineObservationState
    /// 
    /// with some children that are eager fetched
    /// </summary>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    /// <returns></returns>
    IList<IReasonSelection> FindWithForConfig (IMachineMode machineMode,
                                               IMachineObservationState machineObservationState);
    
    /// <summary>
    /// Find all the entities given one or more MachineMode and one or more MachineObservationState
    /// 
    /// with some children that are eager fetched
    /// </summary>
    /// <param name="machineModes"></param>
    /// <param name="machineObservationStates"></param>
    /// <returns></returns>
    IList<IReasonSelection> FindWithForConfig (IList<IMachineMode> machineModes,
                                               IList<IMachineObservationState> machineObservationStates);
    
    /// <summary>
    /// Find all the entities given a MachineMode
    /// </summary>
    /// <param name="machineMode"></param>
    /// <returns></returns>
    IList<IReasonSelection> FindByMachineMode (IMachineMode machineMode);
    
    /// <summary>
    /// Get all the items for a given MachineMode and a given MachineObservationState
    /// 
    /// The request is recursive. If no configuration was found for the specified machine mode,
    /// another try is made with the parent
    ///
    /// Note: this is registered to be cacheable.
    ///       Do not consider here, the children are always automatically fetched because of the cacheable property.
    /// </summary>
    /// <param name="machineMode">not null</param>
    /// <param name="machineObservationState">not null</param>
    /// <returns></returns>
    IList<IReasonSelection> FindWith (IMachineMode machineMode,
                                      IMachineObservationState machineObservationState);

    /// <summary>
    /// Get all the items sorted for a specified MonitoredMachine, MachineMode and MachineObservationState
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
    IEnumerable<IReasonSelection> FindWith (IMachine machine,
                                            IMachineMode machineMode,
                                            IMachineObservationState machineObservationState);
    
    /// <summary>
    /// Find all the possible reasons that are set in ReasonSelection 
    /// </summary>
    /// <returns></returns>
    IEnumerable<IReason> FindReasons ();
  }
}
