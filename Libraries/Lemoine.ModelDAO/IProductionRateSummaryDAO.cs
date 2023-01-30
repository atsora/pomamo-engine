// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Collections;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IProductionRateSummary.
  /// </summary>
  public interface IProductionRateSummaryDAO : IGenericByMachineUpdateDAO<IProductionRateSummary, int>
  {
    /// <summary>
    /// Find the production rate summaries in a day range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IProductionRateSummary> FindInDayRange (IMachine machine,
                                                  DayRange range);

    /// <summary>
    /// Find the production rate summaries in a day range asynchronously
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task<IList<IProductionRateSummary>> FindInDayRangeAsync (IMachine machine,
                                                  DayRange range);
  }

  /// <summary>
  /// Class extensions of <see cref="IProductionRateSummaryDAO"/>
  /// </summary>
  public static class ProductionRateSummaryDAOExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ProductionRateSummaryDAOExtensions).FullName);

#if !NET48
    /// <summary>
    /// Get the production rate for the specified day range
    /// </summary>
    /// <param name="dao"></param>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public static double? GetRateInDayRange (this IProductionRateSummaryDAO dao, IMachine machine, DayRange range)
    {
      Debug.Assert (null != machine);

      var summaries = dao.FindInDayRange (machine, range);
      if (summaries.Any ()) {
        var (rate, duration) = summaries.WeightedAverage (x => x.ProductionRate, x => x.Duration);
        if (0 == duration.Ticks) {
          log.Error ($"GetRateInDayRange: total duration is 0s for {machine} and range={range}, which is unexpected");
          return null;
        }
        else {
          return rate;
        }
      }
      else {
        return null;
      }
    }
#endif // !NET48


  }
}
