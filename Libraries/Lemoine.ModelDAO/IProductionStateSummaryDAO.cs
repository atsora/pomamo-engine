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
  /// DAO interface for IProductionStateSummary.
  /// </summary>
  public interface IProductionStateSummaryDAO : IGenericByMachineUpdateDAO<IProductionStateSummary, int>
  {
    /// <summary>
    /// Find the production state summaries in a day range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IProductionStateSummary> FindInDayRange (IMachine machine,
                                                   DayRange range);

    /// <summary>
    /// Find the production state summaries in a day range asynchronously
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    Task<IList<IProductionStateSummary>> FindInDayRangeAsync (IMachine machine,
                                                              DayRange range);

    /// <summary>
    /// Find the produciton state summaries in a day range
    /// with an early fetch of the production states
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IProductionStateSummary> FindInDayRangeWithProductionState (IMachine machine,
                                                                      DayRange range);

    /// <summary>
    /// Find the produciton state summaries in a day range
    /// with an early fetch of the production states asynchronously
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    Task<IList<IProductionStateSummary>> FindInDayRangeWithProductionStateAsync (IMachine machine,
                                                                                 DayRange range);
  }
}
