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
  /// Implementation of <see cref="Lemoine.ModelDAO.IProductionStateSlotDAO">IProductionStateSummaryDAO</see>
  /// </summary>
  public class ProductionStateSummaryDAO
    : VersionableByMachineNHibernateDAO<ProductionStateSummary, IProductionStateSummary, int>
    , IProductionStateSummaryDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (ProductionStateSummaryDAO).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ProductionStateSummaryDAO ()
      : base ("Machine")
    { }

    /// <summary>
    /// <see cref="IProductionStateSummaryDAO"/>
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IProductionStateSummary> FindInDayRange (IMachine machine,
                                                          DayRange range)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ProductionStateSummary> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (InDayRange (range))
        .AddOrder (Order.Asc ("Day"))
        .List<IProductionStateSummary> ();
    }

    /// <summary>
    /// <see cref="IProductionStateSummaryDAO"/>
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IList<IProductionStateSummary>> FindInDayRangeAsync (IMachine machine,
                                                                                            DayRange range)
    {
      Debug.Assert (null != machine);

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ProductionStateSummary> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (InDayRange (range))
        .AddOrder (Order.Asc ("Day"))
        .ListAsync<IProductionStateSummary> ();
    }

    /// <summary>
    /// <see cref="IProductionStateSummaryDAO"/>
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IProductionStateSummary> FindInDayRangeWithProductionState (IMachine machine,
                                                                             DayRange range)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ProductionStateSummary> ()
        .Fetch (SelectMode.Fetch, "ProductionState")
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (InDayRange (range))
        .AddOrder (Order.Asc ("Day"))
        .List<IProductionStateSummary> ();
    }

    /// <summary>
    /// <see cref="IProductionStateSummaryDAO"/>
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IList<IProductionStateSummary>> FindInDayRangeWithProductionStateAsync (IMachine machine,
                                                                             DayRange range)
    {
      Debug.Assert (null != machine);

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ProductionStateSummary> ()
        .Fetch (SelectMode.Fetch, "ProductionState")
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (InDayRange (range))
        .AddOrder (Order.Asc ("Day"))
        .ListAsync<IProductionStateSummary> ();
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
    /// Update the ProductionStateSummary for a given day (add or remove a duration)
    /// </summary>
    /// <param name="transactionName"></param>
    /// <param name="machine">not null</param>
    /// <param name="machineObservationState">not null</param>
    /// <param name="productionState">not null</param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="duration"></param>
    public void UpdateDay (string transactionName,
                           IMachine machine,
                           IMachineObservationState machineObservationState,
                           IProductionState productionState,
                           DateTime day,
                           IShift shift,
                           TimeSpan duration)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != machineObservationState);
      Debug.Assert (null != productionState);

      if (duration.Ticks == 0) {
        if (log.IsDebugEnabled) {
          log.Debug ($"{transactionName}.{machine.Id} UpdateDay: duration {duration} is 0s => nothing to do");
        }
        return;
      }

      // Find any existing analysis in database
      ICriteria criteria =
        NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ProductionStateSummary> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.Eq ("Day", day));
      if (null == shift) {
        criteria.Add (Restrictions.IsNull ("Shift"));
      }
      else {
        criteria.Add (Restrictions.Eq ("Shift.Id", shift.Id));
      }
      IProductionStateSummary productionStateSummary = criteria
        .Add (Restrictions.Eq ("MachineObservationState.Id", machineObservationState.Id))
        .Add (Restrictions.Eq ("ProductionState.Id", productionState.Id))
        .UniqueResult<ProductionStateSummary> ();
      if (null == productionStateSummary) {
        // The persistent class does not exist in database, create it
        productionStateSummary = new ProductionStateSummary (machine,
                                           day,
                                           shift,
                                           machineObservationState,
                                           productionState);
        productionStateSummary.Duration = duration;
        // disable once CompareOfFloatsByEqualityOperator
        if (0.0 == productionStateSummary.Duration.TotalSeconds) {
          if (log.IsInfoEnabled) {
            log.Info ($"{transactionName}.{machine.Id} UpdateDay: new production state summary with a null duration and a null number of slots at day {day} => skip it machineobservationstate={productionStateSummary.MachineObservationState.Id} shift={productionStateSummary.Shift?.Id} productionState={productionStateSummary.ProductionState?.Id}");
          }
        }
        else if (productionStateSummary.Duration.TotalSeconds < 0.0) {
          log.Error ($"{transactionName}.{machine.Id} UpdateDay: new production state summary with a strictly negative duration {productionStateSummary.Duration}  at day {day} which is not normal except if the accumulator has just been activated => skip it machineobservationstate={productionStateSummary.MachineObservationState.Id} shift={productionStateSummary.Shift?.Id} productionState={productionStateSummary.ProductionState?.Id} {System.Environment.StackTrace}");
        }
        else {
          MakePersistent (productionStateSummary);
        }
        return;
      }
      else { // Update the existing analysis
        var newDuration = productionStateSummary.Duration.Add (duration);
        if (newDuration <= TimeSpan.FromTicks (0)) {
          // disable once CompareOfFloatsByEqualityOperator
          if (0.0 == newDuration.TotalSeconds) {
            if (log.IsInfoEnabled) {
              log.Info ($"{transactionName}.{machine.Id} UpdateDay: delete production state summary because the new duration would be 0s at day {day} machineobservationstate={productionStateSummary.MachineObservationState.Id} shift={productionStateSummary.Shift?.Id} productionState={productionStateSummary.ProductionState.Id}");
            }
          }
          else { // < 0
            log.Error ($"{transactionName}.{machine.Id} UpdateDay: delete production state summary because the new time would be {newDuration} which is not normal except if the accumulator has just been activated at day {day} machineobservationstate={productionStateSummary.MachineObservationState.Id} shift={productionStateSummary.Shift?.Id} productionState={productionStateSummary.ProductionState.Id} {System.Environment.StackTrace}");
          }
          MakeTransient (productionStateSummary);
        }
        else { // 0 < newDuration
          if (TimeSpan.FromHours (25) < newDuration) {
            log.Fatal ($"{transactionName}.{machine.Id} UpdateDay: new duration is {newDuration} which is unexpected => remove the data at {day} instead at {System.Environment.StackTrace}");
            MakeTransient (productionStateSummary);
            return;
          }
          productionStateSummary.Duration = newDuration;
          MakePersistent (productionStateSummary);
        }
        return;
      }
    }
  }
}
