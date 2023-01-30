// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IMachineModuleSlot.
  /// </summary>
  public interface IMachineModuleSlotDAO<I>
    where I: ISlot, IPartitionedByMachineModule
  {
    /// <summary>
    /// Find all the slots for the specified machineModule
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <returns></returns>
    IList<I> FindAll (IMachineModule machineModule);
    
    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// 
    /// Be careful ! This can be used only on global slots
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    I FindAt (IMachineModule machineModule, DateTime dateTime);
    
    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<I> FindOverlapsRange (IMachineModule machineModule, UtcDateTimeRange range);    
  }
}
