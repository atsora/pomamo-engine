// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IShiftTemplateSlot.
  /// </summary>
  public interface IShiftTemplateSlotDAO: IGenericUpdateDAO<IShiftTemplateSlot, int>
  {
    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IShiftTemplateSlot> FindOverlapsRange (UtcDateTimeRange range);
  }
}
