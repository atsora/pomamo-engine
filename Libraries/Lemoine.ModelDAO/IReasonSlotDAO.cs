// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IReasonSlot.
  /// </summary>
  public interface IReasonSlotDAO : IGenericByMachineUpdateDAO<IReasonSlot, int>, IMachineSlotDAO<IReasonSlot>
  {
    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// with an early fetch of the reason
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="at">in UTC</param>
    /// <returns></returns>
    IReasonSlot FindAtWithReason (IMachine machine, Bound<DateTime> at);

    /// <summary>
    /// Find all the reason slots in a specified UTC date/time range.
    /// Include the reason slots that are in the limit
    /// (they must be excluded programmatically then)
    /// 
    /// Order them by begin date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IReasonSlot> FindAllInUtcRangeWithLimit (IMonitoredMachine machine,
                                                   UtcDateTimeRange range);

    /// <summary>
    /// Find all the reason slots in a specified UTC date/time range
    /// with an early fetch of the machine mode
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IReasonSlot> FindAllInUtcRangeWithMachineMode (IMachine machine,
                                                         UtcDateTimeRange range);

    /// <summary>
    /// Find all the reason slots in a specified UTC date/time range
    /// with an early fetch of the machine mode
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IReasonSlot> FindAllInUtcRangeWithMachineModeReason (IMachine machine,
                                                               UtcDateTimeRange range);

    /// <summary>
    /// Find all the reason slots in a specified UTC date/time range
    /// with an early fetch of the reason, the machine observation state and the machine mode
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IReasonSlot> FindAllInUtcRangeWithReasonMachineObservationStateMachineMode (IMonitoredMachine machine,
                                                                                      UtcDateTimeRange range);

    /// <summary>
    /// Get the last reason slot for the machine
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    IReasonSlot GetLast (IMachine machine);

    /// <summary>
    /// Find the reason slot with the specified end date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    IReasonSlot FindWithEnd (IMachine machine,
                             DateTime end);

    /// <summary>
    /// Find the reason slot with the specified end date/time and machine mode and machine observation state
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineMode">not null</param>
    /// <param name="machineObservationState">not null</param>
    /// <param name="end"></param>
    /// <returns></returns>
    IReasonSlot FindWithEnd (IMachine machine, IMachineMode machineMode,
                             IMachineObservationState machineObservationState,
                             DateTime end);
    
    /// <summary>
                                                       /// Find the reason slot with the specified begin date/time
                                                       /// </summary>
                                                       /// <param name="machine"></param>
                                                       /// <param name="begin"></param>
                                                       /// <returns></returns>
    IReasonSlot FindWithBegin (IMachine machine, DateTime begin);

    /// <summary>
    /// Find the processing reason slots in the specified range (by step number)
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="stepNumber">number of reason slots to retrieve in the same time</param>
    /// <param name="preLoad">Pre-load action for the reason extensions</param>
    /// <returns></returns>
    IEnumerable<IReasonSlot> FindProcessingDescending (IMonitoredMachine machine,
      UtcDateTimeRange range, int stepNumber, Action<UtcDateTimeRange> preLoad);

    /// <summary>
    /// Find all the reason slots (on different machines)
    /// at a specific date/time with the specified production state
    /// 
    /// They are order by ascending begin date/time
    /// </summary>
    /// <param name="productionState"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    IEnumerable<IReasonSlot> FindAt (IProductionState productionState, DateTime at);

    /// <summary>
    /// Find all the reason slots that match a production state in a specified range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="productionState"></param>
    /// <returns></returns>
    IEnumerable<IReasonSlot> FindOverlapsRangeMatchingProductionState (IMachine machine,
                                                                       UtcDateTimeRange range,
                                                                       IProductionState productionState);

    /// <summary>
    /// Find the first reason slot with a different production state that is strictly on the left of the specified range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="productionState"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IReasonSlot FindFirstStrictlyLeftDifferentProductionState (IMachine machine,
                                                               IProductionState productionState,
                                                               UtcDateTimeRange range);

    /// <summary>
    /// Find the first reason slot with a different production state that is strictly on the right of the specified range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="productionState"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IReasonSlot FindFirstStrictlyRightDifferentProductionState (IMachine machine,
                                                                IProductionState productionState,
                                                                UtcDateTimeRange range);

    /// <summary>
    /// Find all the reason slots that match a production state
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="productionState"></param>
    /// <returns></returns>
    IList<IReasonSlot> FindMatchingProductionState (IMachine machine,
                                                    UtcDateTimeRange range,
                                                    IProductionState productionState);
  }
}
