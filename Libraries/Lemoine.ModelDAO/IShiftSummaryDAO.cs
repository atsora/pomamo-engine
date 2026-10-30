// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for <see cref="IShiftSummary"/>
  /// </summary>
  public interface IShiftSummaryDAO : IGenericUpdateDAO<IShiftSummary, int>
  {
    /// <summary>
    /// Find the summary of a machine, day and shift
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="day"></param>
    /// <param name="shift">nullable: summary of the whole day</param>
    /// <returns>nullable</returns>
    Task<IShiftSummary> FindAsync (IMachine machine, DateTime day, IShift shift);

    /// <summary>
    /// Find all the summaries of a machine that overlap a range, the oldest one first
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range">not null</param>
    /// <returns>not null</returns>
    Task<IList<IShiftSummary>> FindOverlapsRangeAsync (IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Find all the summaries, whatever their machine, that overlap a range, the oldest one first
    /// </summary>
    /// <param name="range">not null</param>
    /// <returns>not null</returns>
    Task<IList<IShiftSummary>> FindOverlapsRangeAsync (UtcDateTimeRange range);
  }

  /// <summary>
  /// DAO interface for <see cref="IShiftSummarySection"/>
  /// </summary>
  public interface IShiftSummarySectionDAO : IGenericUpdateDAO<IShiftSummarySection, int>
  {
    /// <summary>
    /// Find the sections of a summary
    /// </summary>
    /// <param name="shiftSummary">not null</param>
    /// <returns>not null</returns>
    Task<IList<IShiftSummarySection>> FindBySummaryAsync (IShiftSummary shiftSummary);

    /// <summary>
    /// Find the sections of a machine to compute, in the summaries that overlap a range,
    /// the most recent summary first
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range">not null</param>
    /// <returns>not null</returns>
    Task<IList<IShiftSummarySection>> FindToComputeAsync (IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Flag a section of the summary of a machine, day and shift as to compute again,
    /// if the summary exists
    ///
    /// Synchronous: it is called by the accumulators and the import extensions, inside their
    /// transaction. The version of the section is not changed
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="day"></param>
    /// <param name="shiftId">null: summary of the whole day</param>
    /// <param name="sectionKey">not null and not empty</param>
    void SetUpdateRequired (int machineId, DateTime day, int? shiftId, string sectionKey);

    /// <summary>
    /// Flag a section of the summary of a machine that contains a date/time as to compute
    /// again, if the summary exists
    ///
    /// Synchronous: it is called by the accumulators and the import extensions, inside their
    /// transaction. The version of the section is not changed
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="dateTime">UTC</param>
    /// <param name="sectionKey">not null and not empty</param>
    void SetUpdateRequiredAt (int machineId, DateTime dateTime, string sectionKey);
  }

  /// <summary>
  /// DAO interface for <see cref="IShiftSummaryContent"/>
  /// </summary>
  public interface IShiftSummaryContentDAO : IGenericUpdateDAO<IShiftSummaryContent, int>
  {
    /// <summary>
    /// Find the content of a summary for a generator
    /// </summary>
    /// <param name="shiftSummary">not null</param>
    /// <param name="generator">not null and not empty</param>
    /// <returns>nullable</returns>
    Task<IShiftSummaryContent> FindAsync (IShiftSummary shiftSummary, string generator);

    /// <summary>
    /// Find the contents of a summary
    /// </summary>
    /// <param name="shiftSummary">not null</param>
    /// <returns>not null</returns>
    Task<IList<IShiftSummaryContent>> FindBySummaryAsync (IShiftSummary shiftSummary);
  }
}
