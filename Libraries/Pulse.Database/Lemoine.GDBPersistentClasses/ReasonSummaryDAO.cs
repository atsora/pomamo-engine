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
  /// Implementation of <see cref="Lemoine.ModelDAO.IReasonSlotDAO">IReasonSummaryDAO</see>
  /// </summary>
  public class ReasonSummaryDAO
    : VersionableByMachineNHibernateDAO<ReasonSummary, IReasonSummary, int>
    , IReasonSummaryDAO
  {
    readonly ILog log = LogManager.GetLogger(typeof (ReasonSummaryDAO).FullName);
    
    /// <summary>
    /// Constructor
    /// </summary>
    public ReasonSummaryDAO ()
      : base ("Machine")
    { }
    
    /// <summary>
    /// Find the reason summaries in a day range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IReasonSummary> FindInDayRange (IMachine machine,
                                                 DayRange range)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonSummary> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (InDayRange (range))
        .AddOrder (Order.Asc ("Day"))
        .List<IReasonSummary> ();
    }
    
    /// <summary>
    /// Find the reason summaries in a day range
    /// with an early fetch of the reasons
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IReasonSummary> FindInDayRangeWithReason (IMachine machine,
                                                           DayRange range)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonSummary> ()
        .Fetch (SelectMode.Fetch, "Reason")
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (InDayRange (range))
        .AddOrder (Order.Asc ("Day"))
        .List<IReasonSummary> ();
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
    /// Update the ReasonSummary for a given day (add or remove a duration)
    /// </summary>
    /// <param name="transactionName"></param>
    /// <param name="machine">not null</param>
    /// <param name="machineObservationState">not null</param>
    /// <param name="reason">not null</param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="time"></param>
    /// <param name="incrementValue"></param>
    public void UpdateDay (string transactionName,
                           IMachine machine,
                           IMachineObservationState machineObservationState,
                           IReason reason,
                           DateTime day,
                           IShift shift,
                           TimeSpan time,
                           int incrementValue)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != machineObservationState);
      Debug.Assert (null != reason);

      if ( (0 == time.Ticks) && (0 == incrementValue)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"{transactionName}.{machine.Id} UpdateDay: nothing to do, time is {time} and increment is {incrementValue}");
        }
        return;
      }

      // Find any existing analysis in database
      ICriteria criteria =
        NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonSummary> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.Eq ("Day", day));
      if (null == shift) {
        criteria.Add (Restrictions.IsNull ("Shift"));
      }
      else {
        criteria.Add (Restrictions.Eq ("Shift.Id", shift.Id));
      }
      IReasonSummary reasonSummary = criteria
        .Add (Restrictions.Eq ("MachineObservationState.Id", machineObservationState.Id))
        .Add (Restrictions.Eq ("Reason.Id", reason.Id))
        .UniqueResult<ReasonSummary> ();
      if (null == reasonSummary) {
        if (log.IsDebugEnabled) {
          log.Debug ($"{transactionName}.{machine.Id} UpdateDay: time={time} increment={incrementValue} for day {day} reason {reason?.Id} for an non existent summary");
        }

        // The persistent class does not exist in database, create it
        reasonSummary = new ReasonSummary (machine,
                                           day,
                                           shift,
                                           machineObservationState,
                                           reason);
        reasonSummary.Time = time;
        reasonSummary.Number = incrementValue;
        // disable once CompareOfFloatsByEqualityOperator
        if ((0.0 == reasonSummary.Time.TotalSeconds) && (0 == reasonSummary.Number)) {
          if (log.IsInfoEnabled) {
            log.Info ($"{transactionName}.{machine.Id} UpdateDay: new reason summary with a null duration and a null number of slots at day {day} => skip it machineobservationstate={reasonSummary.MachineObservationState?.Id} shift={reasonSummary.Shift?.Id} reason={reasonSummary.Reason?.Id}");
          }
        }
        else if (0.0 == reasonSummary.Time.TotalSeconds) {
          log.Error ($"{transactionName}.{machine.Id} UpdateDay: new reason summary with a null duration and a number of slots {reasonSummary.Number} at day {day} => skip it machineobservationstate={reasonSummary.MachineObservationState?.Id} shift={reasonSummary.Shift?.Id} reason={reasonSummary.Reason?.Id} {System.Environment.StackTrace}");
        }
        else if (reasonSummary.Time.TotalSeconds < 0.0) {
          log.Error ($"{transactionName}.{machine.Id} UpdateDay: new reason summary with a strictly negative duration {reasonSummary.Time} and number {reasonSummary.Number} at day {day} which is not normal except if the accumulator has just been activated => skip it machineobservationstate={reasonSummary.MachineObservationState.Id} shift={reasonSummary.Shift?.Id ?? 0} reason={reasonSummary.Reason.Id} {System.Environment.StackTrace}");
        }
        else {
          if (reasonSummary.Number <= 0) {
            log.Error ($"{transactionName}.{machine.Id} UpdateDay: new reason summary with a negative number of slots {reasonSummary.Number} at day {day} which is not normal except if the accumulator has just been activated => because the number is not so critical, reset it to 0 machineobservationstate={reasonSummary.MachineObservationState.Id} shift={reasonSummary.Shift?.Id ?? 0} reason={reasonSummary.Reason.Id} {System.Environment.StackTrace}");
            reasonSummary.Number = 1;
          }
          MakePersistent (reasonSummary);
        }
        return;
      }
      else { // Update the existing analysis
        if (log.IsDebugEnabled) {
          log.Debug ($"{transactionName}.{machine.Id} UpdateDay: delta={time} increment={incrementValue} for day {day} reason {reason?.Id}, previously, time={reasonSummary.Time} number={reasonSummary.Number}");
        }

        var newDuration = reasonSummary.Time.Add (time);
        if (newDuration <= TimeSpan.FromTicks (0)) {
          // disable once CompareOfFloatsByEqualityOperator
          if (0.0 == newDuration.TotalSeconds) {
            if (log.IsInfoEnabled) {
              log.InfoFormat ($"{transactionName}.{machine.Id} UpdateDay: delete reason summary because the new time would be {newDuration} at day {day} machineobservationstate={reasonSummary.MachineObservationState.Id} shift={reasonSummary.Shift?.Id} reason={reasonSummary.Reason.Id}");
            }
          }
          else { // < 0
            log.Error ($"{transactionName}.{machine.Id} UpdateDay: delete reason summary because the new time would be {newDuration} at day {day} which is not normal except if the accumulator has just been activated machineobservationstate={reasonSummary.MachineObservationState.Id} shift={reasonSummary.Shift?.Id ?? 0} reason={reasonSummary.Reason.Id} {System.Environment.StackTrace}");
          }
          MakeTransient (reasonSummary);
        }
        else { // 0 < reasonSummary.Time.Add (time)
          if (TimeSpan.FromHours (25) < newDuration) {
            log.Fatal ($"{transactionName}.{machine.Id}: new duration {newDuration} (old is {reasonSummary.Time}, delta is {time}) is not valid => remove the data for day {day} at {System.Environment.StackTrace}");
            MakeTransient (reasonSummary);
            return;
          }
          reasonSummary.Time = newDuration;
          reasonSummary.Number += incrementValue;
          if (0 == reasonSummary.Number) {
            log.Error ($"{transactionName}.{machine.Id} UpdateDay: number of slots {reasonSummary.Number} at day {day} which is not normal except if the accumulator has just been activated although the time is {reasonSummary.Time} machineobservationstate={reasonSummary.MachineObservationState.Id} shift={reasonSummary.Shift?.Id ?? 0} reason={reasonSummary.Reason.Id} {System.Environment.StackTrace}");
            reasonSummary.Number = 1;
          }
          else if (reasonSummary.Number < 0) {
            log.Error ($"{transactionName}.{machine.Id} UpdateDay: negative number of slots {reasonSummary.Number} at day {day} which is not normal except if the accumulator has just been activated although the time is {reasonSummary.Time} machineobservationstate={reasonSummary.MachineObservationState} shift={reasonSummary.Shift?.Id ?? 0} reason={reasonSummary.Reason.Id} {System.Environment.StackTrace}");
            reasonSummary.Number = 1;
          }
          MakePersistent (reasonSummary);
        }
        return;
      }
    }
  }
}
