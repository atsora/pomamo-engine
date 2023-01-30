// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Business.MachineObservationState
{
  /// <summary>
  /// Utility methods to get production properties
  /// </summary>
  public static class Production
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Production).FullName);

    /// <summary>
    /// Get the production duration for the specific range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range">not null and has a duration</param>
    /// <param name="preLoadRange">(optional). Is not null contains range</param>
    /// <returns></returns>
    public static TimeSpan GetProductionDuration (IMachine machine, UtcDateTimeRange range, UtcDateTimeRange preLoadRange = null)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != range);
      Debug.Assert (range.Duration.HasValue);
      Debug.Assert ( (preLoadRange is null) || preLoadRange.ContainsRange (range));

      if (range.IsEmpty ()) {
        return TimeSpan.FromSeconds (0);
      }

      var productionPeriodsRequest = new ProductionPeriods (machine, range, preLoadRange);
      var productionPeriods = Lemoine.Business.ServiceProvider
        .Get (productionPeriodsRequest);
      return productionPeriods
        .Where (x => x.Item2.HasValue && x.Item2.Value && x.Item1.Duration.HasValue)
        .Select (x => x.Item1.Duration.Value)
        .Aggregate (TimeSpan.FromTicks (0), (x, y) => x.Add (y));
    }
  }
}
