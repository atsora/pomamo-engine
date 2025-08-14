// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IOperationSlot.
  /// </summary>
  public interface IOperationSlotDAO: IMachineSlotDAO<IOperationSlot>, IGenericByMachineUpdateDAO<IOperationSlot, int>
  {
    /// <summary>
    /// FindOverlapsRange with an eager fetch of the work order / component / operation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IOperationSlot> FindOverlapsRangeWithEagerFetch (IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Get the operation slot at the specified date/time
    /// 
    /// Consider the end date/time to match the date/times.
    /// It should be used to make a cycle end match an operation slot
    /// 
    /// Returns null if no such operation slot exists
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    IOperationSlot GetOperationSlotAtConsideringEnd (IMachine machine,
                                                     Bound<DateTime> at);

    /// <summary>
    /// Get the first operation slot with a different operation after the specified date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <param name="operation"></param>
    /// <returns></returns>
    IOperationSlot GetFirstDifferentOperationAfter (IMachine machine,
                                                    DateTime dateTime,
                                                    IOperation operation);

    
    /// <summary>
    /// Get the last operation slot, which operation is not null
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    IOperationSlot GetLastOperationNotNull (IMachine machine);
    
    /// <summary>
    /// Get the last operation slot strictly before a specified date/time, which operation is not null
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IOperationSlot GetLastOperationNotNullBefore (IMachine machine, DateTime dateTime);
    
    /// <summary>
    /// Get the last operation slot whose begin date/time is strictly before a specified date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IOperationSlot GetLastStrictlyBefore (IMachine machine, DateTime dateTime);

    /// <summary>
    /// Does an operation slot exist in a given time range ?
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    bool ExistsInRange (IMachine machine,
                        UtcDateTimeRange range);

    /// <summary>
    /// Find all operation slots in a given time range, ordered by ascending begin datetime.
    /// with the specified properties.
    /// 
    /// If one property is null, it must be null in the database as well
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="operation"></param>
    /// <returns></returns>
    IList<IOperationSlot> FindAllInRangeWith (IMachine machine,
                                              UtcDateTimeRange range,
                                              IOperation operation);

    /// <summary>
    /// Find all the operation slots for a specified day and shift
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <returns></returns>
    IList<IOperationSlot> FindByDayShift (IMachine machine,
                                          DateTime day,
                                          IShift shift);

    /// <summary>
    /// Find all the operation slots for a specified day and shift asynchronously
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task<IList<IOperationSlot>> FindByDayShiftAsync (IMachine machine,
                                          DateTime day,
                                          IShift shift);

    /// <summary>
    /// Get the first operation slot whose begin date/time in strictly the specified period (begin is excluded)
    /// 
    /// null is returned is no such operation slot was found
    /// 
    /// The returned operation slot must start after begin
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin">in UTC</param>
    /// <param name="end">in UTC</param>
    /// <returns></returns>
    IOperationSlot GetFirstBeginStrictlyBetween (IMachine machine,
                                                 DateTime begin,
                                                 DateTime end);
    
    /// <summary>
    /// Check if it exists an operation slot with a different operation
    /// in the specified period
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin">in UTC</param>
    /// <param name="end">in UTC</param>
    /// <param name="operation"></param>
    /// <returns></returns>
    bool ExistsDifferentOperationBetween (IMachine machine,
                                          DateTime begin,
                                          DateTime end,
                                          IOperation operation);
    
    /// <summary>
    /// Check if it exists a period of time with a different operation or component or work order
    /// in the specified period
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin">in UTC</param>
    /// <param name="end">in UTC</param>
    /// <param name="operation"></param>
    /// <param name="component"></param>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    bool ExistsDifferentWorkOrderComponentOperationBetween (IMachine machine,
                                                            DateTime begin,
                                                            DateTime end,
                                                            IOperation operation,
                                                            IComponent component,
                                                            IWorkOrder workOrder);
    
    /// <summary>
    /// Is there the same specified operation in whole specified range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="operation"></param>
    /// <returns></returns>
    bool IsContinuousOperationInRange (IMachine machine,
                                       UtcDateTimeRange range,
                                       IOperation operation);

    /// <summary>
    /// Find all the operation slots for the specified operation
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    IList<IOperationSlot> FindByOperation (IOperation operation);

    /// <summary>
    /// Find all the operation slots for the specified component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    IList<IOperationSlot> FindByComponent (IComponent component);

    /// <summary>
    /// Find all the operation slots for the specified workOrder
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    IList<IOperationSlot> FindByWorkOrder (IWorkOrder workOrder);

    /// <summary>
    /// Find all the operation slots for the specified manufacturing order
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="manufacturingOrder">not null</param>
    /// <param name="before">in UTC</param>
    /// <returns></returns>
    IList<IOperationSlot> FindByManufacturingOrderStrictlyBefore (IMachine machine, IManufacturingOrder manufacturingOrder, DateTime before);

    /// <summary>
    /// Find all the operation slots for the specified manufacturing order
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="manufacturingOrder"></param>
    /// <returns></returns>
    IList<IOperationSlot> FindByManufacturingOrder (IMachine machine, IManufacturingOrder manufacturingOrder);

    /// <summary>
    /// Find all the operation slots for the specified manufacturing order
    /// </summary>
    /// <param name="manufacturingOrder"></param>
    /// <returns></returns>
    IList<IOperationSlot> FindByManufacturingOrder (IManufacturingOrder manufacturingOrder);

    /// <summary>
    /// Get the last effective operation slot (that is effective now)
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    IOperationSlot GetLastEffective (IMachine machine);
    
    /// <summary>
    /// Get the last effective operation slot (that is effective now)
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="lastPeriodDuration">Period before now that is taken into account</param>
    /// <returns></returns>
    IOperationSlot GetLastEffective (IMachine machine, TimeSpan lastPeriodDuration);
    
    /// <summary>
    /// Get the last effective operation slot (that is effective now)
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="after">after a specific date/time. Default: -oo</param>
    /// <returns></returns>
    IOperationSlot GetLastEffective (IMachine machine, LowerBound<DateTime> after);
    
    /// <summary>
    /// Get the effective operation slots (that are effective now)
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    IList<IOperationSlot> GetEffective (IMachine machine);
    
    /// <summary>
    /// Get the effective operation slots (that are effective now)
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="after">after a specific date/time. Default: -oo</param>
    /// <returns></returns>
    IList<IOperationSlot> GetEffective (IMachine machine,
                                        LowerBound<DateTime> after);
    
    /// <summary>
    /// Get the effective operation slots (that are effective now)
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="virtualOperationSlot">with no day and no shift</param>
    /// <returns></returns>
    IList<IOperationSlot> GetEffective (IMachine machine, out IOperationSlot virtualOperationSlot);
    
    /// <summary>
    /// Get the effective operation slots (that are effective now)
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="virtualOperationSlot">with no day and no shift</param>
    /// <param name="after">after a specific date/time. Default: -oo</param>
    /// <returns></returns>
    IList<IOperationSlot> GetEffective (IMachine machine, out IOperationSlot virtualOperationSlot,
                                        LowerBound<DateTime> after);
    
    /// <summary>
    /// Get the effective operation of the current shift
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="virtualOperationSlot">with the day and shift if found</param>
    /// <returns></returns>
    IList<IOperationSlot> GetEffectiveCurrentShift (IMachine machine, out IOperationSlot virtualOperationSlot);

    /// <summary>
    /// Get the effective operation of the current shift
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="virtualOperationSlot">with the day and shift if found</param>
    /// <param name="dateTime">at this specific date/time</param>
    /// <returns></returns>
    IList<IOperationSlot> GetEffectiveCurrentShift (IMachine machine, out IOperationSlot virtualOperationSlot, DateTime dateTime);

    /// <summary>
    /// Find the slot with the specified end date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    IOperationSlot FindWithEnd (IMachine machine,
                                DateTime end);

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="workOrder">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IOperationSlot> FindOverlapsRangeWithWorkOrder (IMachine machine,
                                                          IWorkOrder workOrder,
                                                          UtcDateTimeRange range);
  }
}
