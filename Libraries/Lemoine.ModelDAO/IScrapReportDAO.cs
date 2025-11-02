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
    Task<IEnumerable<IScrapReport>> FindOverlapsRange (IMachine machine, UtcDateTimeRange range);

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
