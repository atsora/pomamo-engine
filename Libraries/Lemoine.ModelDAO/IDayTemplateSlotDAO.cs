// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IDayTemplateSlot.
  /// </summary>
  public interface IDayTemplateSlotDAO: IGenericUpdateDAO<IDayTemplateSlot, int>
  {
    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IDayTemplateSlot> FindOverlapsRange (UtcDateTimeRange range);
  }
}
