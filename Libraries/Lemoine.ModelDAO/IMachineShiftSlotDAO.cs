// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IIMachineShiftSlot
  /// </summary>
  public interface IMachineShiftSlotDAO
  {
    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    IMachineShiftSlot FindAt (IMachine machine, DateTime dateTime);

    /// <summary>
    /// Find the unique slot at the specified UTC date/time asynchronously
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    System.Threading.Tasks.Task<IMachineShiftSlot> FindAtAsync (IMachine machine, DateTime dateTime);
  }
}
