// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IReasonSummary.
  /// </summary>
  public interface IReasonSummaryDAO: IGenericByMachineUpdateDAO<IReasonSummary, int>
  {
    /// <summary>
    /// Find the reason summaries in a day range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IReasonSummary> FindInDayRange (IMachine machine,
                                          DayRange range);

    /// <summary>
    /// Find the reason summaries in a day range
    /// with an early fetch of the reasons
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IReasonSummary> FindInDayRangeWithReason (IMachine machine,
                                                    DayRange range);
  }
}
