// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IReasonOverwriteRequiredSlot
  /// </summary>
  public interface IReasonOverwriteRequiredSlotDAO
  {
    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <param name="extend">Try to extend the slots</param>
    /// <returns></returns>
    IReasonOverwriteRequiredSlot FindAt (IMachine machine, DateTime dateTime, bool extend);

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="extend">Try to extend the slots</param>
    /// <returns></returns>
    IList<IReasonOverwriteRequiredSlot> FindOverlapsRange (IMachine machine, UtcDateTimeRange range, bool extend);
  }
}
