// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for the cnc data samples (table cncdatasample, high frequency)
  ///
  /// There is no entity: the samples are written in bulk and are never modified
  /// </summary>
  public interface ICncDataSampleDAO
  {
    /// <summary>
    /// Insert samples in bulk (binary COPY) in the current transaction
    ///
    /// The rows are split automatically: at most 32767 ms, never across a week bound,
    /// one kind of value (double or json) by row.
    /// The missing partitions are created
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="series">not null</param>
    /// <returns>number of inserted rows</returns>
    int Insert (IMachineModule machineModule, IEnumerable<CncDataSampleSeries> series);

    /// <summary>
    /// Read the recent and archived samples of a field in a range, ordered by date/time
    /// (function cncdatasamplerange)
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range">with a lower bound</param>
    /// <returns></returns>
    IList<CncDataSampleValue> FindInRange (IMachineModule machineModule, IField field, UtcDateTimeRange range);

    /// <summary>
    /// Read a page of the recent and archived samples of a field in a range, ordered by date/time
    ///
    /// To get the next page, call it again with a range that starts just after the date/time of the last returned sample
    /// (the samples have a millisecond precision)
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range">with a lower bound</param>
    /// <param name="maxCount">maximum number of samples, strictly positive</param>
    /// <returns></returns>
    IList<CncDataSampleValue> FindInRange (IMachineModule machineModule, IField field, UtcDateTimeRange range, int maxCount);

    /// <summary>
    /// Read the last recent sample of a field
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <returns>null if there is no recent sample</returns>
    CncDataSampleValue? FindLast (IMachineModule machineModule, IField field);
  }
}
