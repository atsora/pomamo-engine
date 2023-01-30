// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IMachineModeSlot.
  /// </summary>
  public interface IMachineModeSlotDAO
  {
    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// 
    /// Be careful ! This can be used only on global slots
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <param name="extend">Extend the slots</param>
    /// <returns></returns>
    IMachineModeSlot FindAt (IMachine machine, DateTime dateTime, bool extend);
    
    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="extend">Extend the slots</param>
    /// <returns></returns>
    IList<IMachineModeSlot> FindOverlapsRange (IMachine machine, UtcDateTimeRange range, bool extend);

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// with an early fetch of the machine mode
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="extend">Extend the slots</param>
    /// <returns></returns>
    IList<IMachineModeSlot> FindOverlapsRangeWithMachineMode (IMachine machine, UtcDateTimeRange range, bool extend);
  }
}
