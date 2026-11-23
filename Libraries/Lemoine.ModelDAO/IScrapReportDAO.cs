// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for <see cref="IScrapReport" />
  /// </summary>
  public interface IScrapReportDAO : IGenericByMachineDAO<IScrapReport, long>
  {
    /// <summary>
    /// Find the latest report at a specific time. else null is returned
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    Task<IScrapReport> FindActiveAtAsync (IMachine machine, DateTime at);

    /// <summary>
    /// Find the latest report before the specified time
    /// </summary>
    Task<IEnumerable<IScrapReport>> FindOverlapsRangeAsync (IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Find the completed scrap reports (analysis status Done) that overlap the specified range
    /// and that are not updated by another completed report
    ///
    /// Among them, the reports that are included in an operation slot of this range
    /// and whose operation is the operation of the slot are counted in it
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range">not empty</param>
    /// <returns></returns>
    IList<IScrapReport> FindCompletedNotUpdatedOverlapping (IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Is a scrap report completed (analysis status Done) and not updated by another completed report ?
    ///
    /// Only such a report may be counted in an operation slot
    /// </summary>
    /// <param name="scrapReport">not null</param>
    /// <returns></returns>
    bool IsCompletedAndNotUpdated (IScrapReport scrapReport);

    /// <summary>
    /// Find the end date/time of the latest report before the specified time
    /// </summary>
    Task<DateTime?> FindLatestReportTimeBeforeAsync (IMachine machine, IOperationSlot operationSlot, DateTime before);

    /// <summary>
    /// Find the start date/time of the next report after the specified time
    /// </summary>
    Task<DateTime?> FindNextReportTimeAfterAsync (IMachine machine, IOperationSlot operationSlot, DateTime before);
  }
}
