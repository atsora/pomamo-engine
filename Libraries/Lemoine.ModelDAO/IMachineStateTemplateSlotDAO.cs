// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IMachineStateTemplateSlot.
  /// </summary>
  public interface IMachineStateTemplateSlotDAO
  {
    /// <summary>
    /// Find all the slots for the specified machine
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    IList<IMachineStateTemplateSlot> FindAll (IMachine machine);
    
    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// 
    /// Be careful ! This can be used only on global slots
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    IMachineStateTemplateSlot FindAt (IMachine machine, DateTime dateTime);
    
    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IMachineStateTemplateSlot> FindOverlapsRange (IMachine machine, UtcDateTimeRange range);
    
    /// <summary>
    /// Find all the machine state template slots (on different machines)
    /// at a specific date/time with the specified machine state template
    /// 
    /// They are order by ascending begin date/time
    /// </summary>
    /// <param name="machineStateTemplate"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    IEnumerable<IMachineStateTemplateSlot> FindAt (IMachineStateTemplate machineStateTemplate, DateTime at);
    
    /// <summary>
    /// Find all the machine state template slots that match a machine state template in a specified range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="machineStateTemplate"></param>
    /// <returns></returns>
    IEnumerable<IMachineStateTemplateSlot> FindOverlapsRangeMatchingMachineStateTemplate (IMachine machine,
                                                                                          UtcDateTimeRange range,
                                                                                          IMachineStateTemplate machineStateTemplate);
  }
}
