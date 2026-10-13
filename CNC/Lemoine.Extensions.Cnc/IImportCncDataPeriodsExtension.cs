// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.Extensions.Cnc
{
  /// <summary>
  /// Extension called after the cnc data periods of a field were imported in table cncdataperiod
  /// and committed, for example to update a summary table without waiting for a periodic computation.
  ///
  /// Entry point in ImportCncDataFromQueue, Lemoine.CncDataImport.
  /// It is called outside the transaction of the import: it opens its own session and transaction,
  /// and an exception it raises is logged without affecting the import.
  /// Because the data are committed, it may read them back from cncdataperiod
  /// </summary>
  public interface IImportCncDataPeriodsExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialization
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <returns>the extension is active for this machine module</returns>
    bool Initialize (IMachineModule machineModule);

    /// <summary>
    /// The cnc data periods of a field were written in range and committed
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range">UTC range that contains all the periods that were created or modified (bounds included)</param>
    /// <param name="writtenPeriods">not null, the periods that were created or modified, in their state after the commit,
    /// ordered by begin (read-only copies). A modified period has its whole statistics, not only the new values.
    /// Empty for a custom aggregation: the periods are written by an <see cref="IImportCncDataPeriodExtension"/></param>
    /// <param name="increments">not null, the new numeric data of this import, ordered by end, independently of the periods
    /// (see <see cref="CncDataIncrement"/>): they are only added, which allows an incremental aggregation without reading the periods again.
    /// Empty for a custom aggregation or a non-numeric field</param>
    void AfterCommit (IMachineModule machineModule, IField field, UtcDateTimeRange range,
      System.Collections.Generic.IList<ICncDataPeriodValue> writtenPeriods,
      System.Collections.Generic.IList<CncDataIncrement> increments);
  }
}
