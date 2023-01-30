// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IProductionRateSlotDAO">IProductionRateSummaryDAO</see>
  /// </summary>
  public class ProductionRateSummaryDAO
    : VersionableByMachineNHibernateDAO<ProductionRateSummary, IProductionRateSummary, int>
    , IProductionRateSummaryDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (ProductionRateSummaryDAO).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ProductionRateSummaryDAO ()
      : base ("Machine")
    { }

    /// <summary>
    /// <see cref="IProductionRateSummaryDAO"/>
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IProductionRateSummary> FindInDayRange (IMachine machine,
                                                          DayRange range)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ProductionRateSummary> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (InDayRange (range))
        .AddOrder (Order.Asc ("Day"))
        .List<IProductionRateSummary> ();
    }

    /// <summary>
    /// <see cref="IProductionRateSummaryDAO"/>
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IList<IProductionRateSummary>> FindInDayRangeAsync (IMachine machine,
                                                          DayRange range)
    {
      Debug.Assert (null != machine);

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ProductionRateSummary> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (InDayRange (range))
        .AddOrder (Order.Asc ("Day"))
        .ListAsync<IProductionRateSummary> ();
    }

    /// <summary>
    /// Range criterion
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    protected virtual AbstractCriterion InDayRange (DayRange range)
    {
      if ((null == range) || (range.IsEmpty ())) {
        return Expression.Sql ("FALSE");
      }

      Junction result = Restrictions.Conjunction ();
      if (range.Lower.HasValue) {
        if (range.LowerInclusive) {
          result = result
            .Add (Restrictions.Ge ("Day", range.Lower.Value));
        }
        else {
          result = result
            .Add (Restrictions.Gt ("Day", range.Lower.Value));
        }
      }
      if (range.Upper.HasValue) {
        if (range.UpperInclusive) {
          result = result
            .Add (Restrictions.Le ("Day", range.Upper.Value));
        }
        else {
          result = result
            .Add (Restrictions.Lt ("Day", range.Upper.Value));
        }
      }

      return result;
    }

    /// <summary>
    /// Update the ProductionRateSummary for a given day (add or remove a duration)
    /// </summary>
    /// <param name="transactionName"></param>
    /// <param name="machine">not null</param>
    /// <param name="machineObservationState">not null</param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="duration"></param>
    /// <param name="productionRate"></param>
    public void UpdateDay (string transactionName,
                           IMachine machine,
                           IMachineObservationState machineObservationState,
                           DateTime day,
                           IShift shift,
                           TimeSpan duration,
                           double productionRate)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != machineObservationState);

      if (duration.Ticks == 0) {
        if (log.IsDebugEnabled) {
          log.Debug ($"{transactionName}.{machine.Id} UpdateDay: duration {duration} is 0s => nothing to do");
        }
        return;
      }

      // Find any existing analysis in database
      ICriteria criteria =
        NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ProductionRateSummary> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.Eq ("Day", day));
      if (null == shift) {
        criteria.Add (Restrictions.IsNull ("Shift"));
      }
      else {
        criteria.Add (Restrictions.Eq ("Shift.Id", shift.Id));
      }
      IProductionRateSummary productionRateSummary = criteria
        .Add (Restrictions.Eq ("MachineObservationState.Id", machineObservationState.Id))
        .UniqueResult<ProductionRateSummary> ();
      if (productionRateSummary is null) {
        // The persistent class does not exist in database, create it
        productionRateSummary = new ProductionRateSummary (machine,
                                           day,
                                           shift,
                                           machineObservationState);
        productionRateSummary.Duration = duration;
        productionRateSummary.ProductionRate = productionRate;
        // disable once CompareOfFloatsByEqualityOperator
        if (0.0 == productionRateSummary.Duration.TotalSeconds) {
          log.Fatal ($"{transactionName}.{machine.Id} UpdateDay: new production rate summary with a null duration and a null number of slots at day {day} => skip it (although this case is normally already rejected) machineobservationstate={productionRateSummary.MachineObservationState} shift={productionRateSummary.Shift} productionRate={productionRateSummary.ProductionRate} {System.Environment.StackTrace}");
        }
        else if (productionRateSummary.Duration.TotalSeconds < 0.0) {
          log.Error ($"{transactionName}.{machine.Id} UpdateDay: new production rate summary with a strictly negative duration {productionRateSummary.Duration.TotalSeconds}s  at day {day} which is not normal except if the accumulator has just been activated => skip it machineobservationstate={productionRateSummary.MachineObservationState} shift={productionRateSummary.Shift} productionRate={productionRateSummary.ProductionRate} {System.Environment.StackTrace}");
        }
        else {
          MakePersistent (productionRateSummary);
        }
        return;
      }
      else { // Update the existing analysis
        var newDuration = productionRateSummary.Duration.Add (duration);
        if (newDuration <= TimeSpan.FromTicks (0)) {
          // disable once CompareOfFloatsByEqualityOperator
          if ((0.0 == newDuration.TotalSeconds)
            && (0 == ((productionRateSummary.ProductionRate * productionRateSummary.Duration.TotalSeconds) + (productionRate * duration.TotalSeconds)))) {
            if (log.IsInfoEnabled) {
              log.Info ($"{transactionName}.{machine.Id} UpdateDay: delete production rate summary because the new duration would be 0s with rate=0 at day {day} machineobservationstate={productionRateSummary.MachineObservationState} shift={productionRateSummary.Shift}");
            }
          }
          else { // < 0
            log.Error ($"{transactionName}.{machine.Id} UpdateDay: delete production rate summary because the new time would be {newDuration} which is not normal except if the accumulator has just been activated at day {day} machineobservationstate={productionRateSummary.MachineObservationState} shift={productionRateSummary.Shift} {System.Environment.StackTrace}");
          }
          MakeTransient (productionRateSummary);
          return;
        }
        var newRate = ((productionRateSummary.ProductionRate * productionRateSummary.Duration.TotalSeconds) + (productionRate * duration.TotalSeconds)) / newDuration.TotalSeconds;
        if (newRate < 0.0) {
          if (-0.0000000001 < newRate) { // Close to 0 
            if (log.IsDebugEnabled) {
              log.Debug ($"{transactionName}.{machine.Id} UpdateDay: {newRate} is close to 0, round it at day {day} machineobservationstate={productionRateSummary.MachineObservationState} shift={productionRateSummary.Shift}");
            }
            productionRateSummary.Duration = newDuration;
            productionRateSummary.ProductionRate = 0.0;
            MakePersistent (productionRateSummary);
          }
          else {
            log.Error ($"{transactionName}.{machine.Id} UpdateDay: delete production rate summary because the new rate would be {newRate} with duration {newDuration} which is not normal except if the accumulator has just been activated at day {day} machineobservationstate={productionRateSummary.MachineObservationState.Id} shift={productionRateSummary.Shift} {System.Environment.StackTrace}");
            MakeTransient (productionRateSummary);
          }
        }
        else { // 0 < newDuration && 0 <= newRate
          if (TimeSpan.FromHours (25) < newDuration) {
            log.Fatal ($"{transactionName}.{machine.Id} UpdateDay: new duration is {newDuration} which is unexpected => remove the data at {day} instead at {System.Environment.StackTrace}");
            MakeTransient (productionRateSummary);
            return;
          }
          productionRateSummary.Duration = newDuration;
          productionRateSummary.ProductionRate = newRate;
          MakePersistent (productionRateSummary);
        }
        return;
      }
    }
  }
}
