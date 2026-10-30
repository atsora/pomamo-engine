// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Extensions;
using Lemoine.Model;

namespace Pulse.Extensions.Business.ShiftSummary
{
  /// <summary>
  /// Extension that contributes sections to the shift summaries
  ///
  /// Each source of data (stops, alarms, tool geometry, operator tasks, ...) is a contributor.
  /// A section is computed separately, one per run of the analysis of the machine, when it
  /// is created and each time it is flagged:
  /// <item>by the contributor itself when some relevant data changes, with
  /// IShiftSummarySectionDAO.SetUpdateRequired (an accumulator, an import extension, a
  /// plugin)</item>
  /// <item>periodically around the end of the shift, for the sections whose data has no
  /// change notification, see <see cref="IsRefreshedPeriodically(string)"/></item>
  /// </summary>
  public interface IShiftActivityExtension : IExtension
  {
    /// <summary>
    /// Initialize the extension for a machine
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns>false: the extension is not active for this machine</returns>
    bool Initialize (IMachine machine);

    /// <summary>
    /// Keys of the sections of the extension, unique among all the extensions,
    /// for example Default.Stop
    /// </summary>
    IEnumerable<string> SectionKeys { get; }

    /// <summary>
    /// Is a section computed again periodically around the end of the shift? It is the case
    /// of the sections whose data has no change notification, or that depend on the time
    /// (a task that becomes late)
    /// </summary>
    /// <param name="sectionKey">one of <see cref="SectionKeys"/></param>
    /// <returns></returns>
    bool IsRefreshedPeriodically (string sectionKey);

    /// <summary>
    /// Get the items of a section
    ///
    /// It is run inside a read-only database session
    /// </summary>
    /// <param name="sectionKey">one of <see cref="SectionKeys"/></param>
    /// <param name="machine">not null</param>
    /// <param name="day"></param>
    /// <param name="shift">nullable: whole day</param>
    /// <param name="range">UTC range of the shift, not empty</param>
    /// <param name="cancellationToken"></param>
    /// <returns>not null</returns>
    Task<IEnumerable<IShiftActivityItem>> GetItemsAsync (string sectionKey, IMachine machine, DateTime day, IShift shift, UtcDateTimeRange range, CancellationToken cancellationToken);
  }
}
