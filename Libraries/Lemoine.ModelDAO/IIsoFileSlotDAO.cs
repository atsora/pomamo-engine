// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IIsoFileSlot.
  /// </summary>
  public interface IIsoFileSlotDAO: IGenericDAO<IIsoFileSlot, int>
  {
    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IIsoFileSlot> FindOverlapsRange (IMachineModule machineModule, UtcDateTimeRange range);

    /// <summary>
    /// Get the last slot
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <returns></returns>
    IIsoFileSlot FindLast (IMachineModule machineModule);
  }
}
