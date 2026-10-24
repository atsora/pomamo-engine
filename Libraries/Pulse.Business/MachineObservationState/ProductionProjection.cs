// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Business.MachineObservationState
{
  /// <summary>
  /// Planned periods of a machine, production or not: when the machine is expected to
  /// produce
  ///
  /// It is a pure computation on the periods: the machine produces in the production
  /// periods, and nothing in the other ones, nor between the periods. After the last
  /// period, nothing is planned and nothing may be said
  /// </summary>
  [Serializable]
  public class ProductionProjection
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ProductionProjection).FullName);

    readonly IReadOnlyList<(UtcDateTimeRange Range, bool Production)> m_periods;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="periods">planned periods with their production flag, not null, not overlapping</param>
    public ProductionProjection (IEnumerable<(UtcDateTimeRange Range, bool Production)> periods)
    {
      if (periods is null) {
        log.Fatal ("ProductionProjection: periods is null");
        throw new ArgumentNullException (nameof (periods));
      }

      // A period with no lower bound comes first
      m_periods = periods
        .Where (x => !x.Range.IsEmpty ())
        .OrderBy (x => x.Range.Lower.HasValue ? x.Range.Lower.Value : DateTime.MinValue)
        .ToList ();
    }

    /// <summary>
    /// Planned periods with their production flag, ordered by begin
    /// </summary>
    public IReadOnlyList<(UtcDateTimeRange Range, bool Production)> Periods => m_periods;

    /// <summary>
    /// When the machine reaches a production time, from a date/time, producing in the
    /// production periods only
    /// </summary>
    /// <param name="from">in UTC</param>
    /// <param name="productionTime">production time to reach</param>
    /// <returns>null when the production time is not reached in the planned periods</returns>
    public DateTime? GetReachedDateTime (DateTime from, TimeSpan productionTime)
    {
      if (productionTime <= TimeSpan.Zero) {
        return from;
      }

      var remaining = productionTime;
      foreach (var period in m_periods.Where (x => x.Production)) {
        var begin = (period.Range.Lower.HasValue && (from < period.Range.Lower.Value)) ? period.Range.Lower.Value : from;
        if (period.Range.Upper.HasValue && (period.Range.Upper.Value <= begin)) {
          continue;
        }
        if (!period.Range.Upper.HasValue) {
          return begin.Add (remaining);
        }
        var available = period.Range.Upper.Value.Subtract (begin);
        if (remaining <= available) {
          return begin.Add (remaining);
        }
        remaining = remaining.Subtract (available);
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"GetReachedDateTime: {remaining} of production left after the planned periods => not reached");
      }
      return null;
    }
  }
}
