// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IProductionStateSlot.
  /// </summary>
  public interface IProductionStateSlotDAO
  {
    /// <summary>
    /// Find all the slots for the specified machine
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    IList<IProductionStateSlot> FindAll (IMachine machine);
    
    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// 
    /// Be careful ! This can be used only on global slots
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    IProductionStateSlot FindAt (IMachine machine, DateTime dateTime);

    /// <summary>
    /// Find the unique slot at the specified UTC date/time asynchronously
    /// 
    /// Be careful ! This can be used only on global slots
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    System.Threading.Tasks.Task<IProductionStateSlot> FindAtAsync (IMachine machine, DateTime dateTime);

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IProductionStateSlot> FindOverlapsRange (IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Find all the slots that overlap the specified range asynchronously
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task<IList<IProductionStateSlot>> FindOverlapsRangeAsync (IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Find all the slots (on different machines)
    /// at a specific date/time with the specified production state
    /// 
    /// They are order by ascending begin date/time
    /// </summary>
    /// <param name="productionState"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    IEnumerable<IProductionStateSlot> FindAt (IProductionState productionState, DateTime at);
    
    /// <summary>
    /// Find all the machine state template slots that match a machine state template in a specified range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="machineStateTemplate"></param>
    /// <returns></returns>
    IEnumerable<IProductionStateSlot> FindOverlapsRangeMatchingProductionState (IMachine machine,
                                                                                UtcDateTimeRange range,
                                                                                IProductionState machineStateTemplate);
  }
}
