// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Pulse.Web.Scrap
{
  /// <summary>
  /// Cycles of a scrap report, shared by the ScrapAt and ScrapSave services
  ///
  /// A scrap report covers the range (lower, upper]: it starts (excluded) at the end of
  /// the previous report (or at the start of its operation slot) and it ends at the end
  /// of its last completed cycle. A cycle is counted in the report that contains its end:
  /// a cycle that is in progress (no end yet) is not counted, it will be counted in a next report
  /// </summary>
  internal static class ScrapReportCycles
  {
    /// <summary>
    /// Round a date/time up to the second, the precision of the date/times of the web service,
    /// so that the end of the last cycle is still in the range once converted to a string
    /// </summary>
    public static DateTime CeilingToSecond (DateTime dateTime)
    {
      var truncated = new DateTime (dateTime.Ticks - dateTime.Ticks % TimeSpan.TicksPerSecond, dateTime.Kind);
      return (truncated == dateTime) ? truncated : truncated.AddSeconds (1);
    }

    /// <summary>
    /// Is a cycle counted in a scrap report (lower, upper]: completed cycle whose end is in the range
    /// </summary>
    public static bool IsInReport (IOperationCycle cycle, DateTime lower, DateTime upper) =>
      cycle.End.HasValue && (lower < cycle.End.Value) && (cycle.End.Value <= upper);

    /// <summary>
    /// Completed cycles whose end is in (lower, upper], sorted by end
    /// </summary>
    public static async Task<IList<IOperationCycle>> FindAsync (IMachine machine, DateTime lower, DateTime upper)
    {
      var cycles = await ModelDAOHelper.DAOFactory.OperationCycleDAO
        .FindOverlapsRangeAsync (machine, new UtcDateTimeRange (lower, upper, "(]"));
      return cycles
        .Where (x => IsInReport (x, lower, upper))
        .OrderBy (x => x.End.Value)
        .ToList ();
    }

    /// <summary>
    /// Number of parts of the cycles: the quantity of the adjusted cycles, else the number of parts of the operation
    /// </summary>
    public static int GetNbParts (IOperation operation, IEnumerable<IOperationCycle> cycles)
    {
      var adjustedCycles = cycles.Where (x => x.Quantity.HasValue).ToList ();
      return operation.GetTotalNumberOfIntermediateWorkPieces () * (cycles.Count () - adjustedCycles.Count)
        + adjustedCycles.Sum (x => x.Quantity.Value);
    }
  }
}
