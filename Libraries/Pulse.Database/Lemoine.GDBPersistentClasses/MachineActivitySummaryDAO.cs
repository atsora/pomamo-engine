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
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineActivitySummaryDAO">IMachineActivitySummaryDAO</see>
  /// </summary>
  public class MachineActivitySummaryDAO
    : VersionableByMachineNHibernateDAO<MachineActivitySummary, IMachineActivitySummary, int>
    , IMachineActivitySummaryDAO

  {
    readonly ILog log = LogManager.GetLogger(typeof (MachineActivitySummaryDAO).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public MachineActivitySummaryDAO ()
      : base ("Machine")
    { }


    /// <summary>
    /// Find the machine activity summaries in a day range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IMachineActivitySummary> FindInDayRange (IMachine machine,
                                                          DayRange range)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineActivitySummary> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (InDayRange (range))
        .AddOrder (Order.Asc ("Day"))
        .List<IMachineActivitySummary> ();
    }

    /// <summary>
    /// Update the MachineActivitySummary for a given day (add or remove a duration)
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineObservationState">not null</param>
    /// <param name="machineMode">not null</param>
    /// <param name="shift"></param>
    /// <param name="day"></param>
    /// <param name="time">delta. Should not be 0s</param>
    public void UpdateDay (string transactionName,
                           IMachine machine,
                           IMachineObservationState machineObservationState,
                           IMachineMode machineMode,
                           IShift shift,
                           DateTime day,
                           TimeSpan time)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != machineObservationState);
      Debug.Assert (null != machineMode);

      if (0 == time.Ticks) {
        log.Error ($"{transactionName}.{machine.Id} UpdateDay: nothing to do, time is {time}. It should not be called");
        return;
      }

      // Find any existing analysis in database
      ICriteria criteria =
        NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineActivitySummary> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.Eq ("Day", day))
        .Add (Restrictions.Eq ("MachineObservationState.Id", machineObservationState.Id))
        .Add (Restrictions.Eq ("MachineMode.Id", machineMode.Id));
      if (null == shift) {
        criteria.Add (Restrictions.IsNull ("Shift"));
      }
      else {
        criteria.Add (Restrictions.Eq ("Shift.Id", shift.Id));
      }
      IMachineActivitySummary activitySummary =
        criteria
        .UniqueResult<MachineActivitySummary> ();
      if (null == activitySummary) {
        // The persistent class does not exist in database, create it
        activitySummary = new MachineActivitySummary (machine,
                                                      day,
                                                      machineObservationState,
                                                      machineMode,
                                                      shift);
        activitySummary.Time = time;
        if (0.0 == activitySummary.Time.TotalSeconds) {
          log.Info ($"{transactionName}.{machine.Id} UpdateDay: new activity summary with a null duration at day {day} => skip it");
        }
        else if (activitySummary.Time.TotalSeconds < 0.0) {
          log.Fatal ($"{transactionName}.{machine.Id} UpdateDay: new activity summary with a negative duration {activitySummary.Time} at day {day} which is not normal except if the summary has been activated recently => skip it {System.Environment.StackTrace}");
          // UNDONE: log it also in analysislog (but there is no reference to the modification here...)
        }
        else {
          MakePersistent (activitySummary);
        }
        return;
      }
      else { // Update the existing analysis
        var newDuration = activitySummary.Time.Add (time);
        if (newDuration <= TimeSpan.FromTicks (0)) {
          // disable once CompareOfFloatsByEqualityOperator
          if (0.0 == newDuration.TotalSeconds) {
            log.Info ($"{transactionName}.{machine.Id} UpdateDay: delete activity summary because the new time would be {newDuration} at day {day}");
          }
          else { // < 0
            log.Fatal ($"{transactionName}.{machine.Id} UpdateDay: delete activity summary because the new time would be {newDuration} at day {day}, not expected unless the summary has been activated recently {System.Environment.StackTrace}");
            // UNDONE: log it also in analysislog (but there is no reference to the modification here)
          }
          MakeTransient (activitySummary);
          return;
        }
        else {
          if (TimeSpan.FromHours (25) < newDuration) {
            log.Fatal ($"{transactionName}.{machine.Id} UpdateDay: new duration {newDuration} is not valid => remove the data for day {day} at {System.Environment.StackTrace}");
            MakeTransient (activitySummary);
            return;
          }
          activitySummary.Time = newDuration;
          MakePersistent (activitySummary);
          return;
        }
      }
    }
    
    /// <summary>
    /// Get the run and total time for a specified day range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="beginDay"></param>
    /// <param name="endDay"></param>
    /// <returns></returns>
    public RunTotalTime? GetRunTotalTime (IMachine machine, DateTime beginDay, DateTime endDay)
    {
      Debug.Assert (DateTimeKind.Unspecified == beginDay.Kind);
      Debug.Assert (DateTimeKind.Unspecified == endDay.Kind);
      
      return GetRunTotalTime (machine, new DayRange (beginDay, endDay));
    }

    /// <summary>
    /// Get the run and total time for a specified day range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dayRange"></param>
    /// <returns></returns>
    public RunTotalTime? GetRunTotalTime (IMachine machine, DayRange dayRange)
    {
      Object[] result = (Object[]) NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("MachineActivitySummaryRunTotalTime")
        .SetParameter ("machineId", machine.Id)
        .SetParameter ("beginDay", dayRange.Lower.NullableValue)
        .SetParameter ("endDay", dayRange.Upper.NullableValue)
        .UniqueResult ();
      if ( (null != result) && (null != result [2])) {
        Debug.Assert (null != result[0]);
        Debug.Assert (null != result[1]);
        RunTotalTime runTotalTime = new RunTotalTime ();
        runTotalTime.Run = TimeSpan.FromSeconds ((double) result [0]);
        runTotalTime.NotRunning = TimeSpan.FromSeconds ((double) result[1]);
        runTotalTime.Total = TimeSpan.FromSeconds ((double) result [2]);
        return runTotalTime;
      }
      else {
        return null;
      }
    }

    /// <summary>
    /// Get the run time for a specified day range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="beginDay"></param>
    /// <param name="endDay"></param>
    /// <returns></returns>
    public TimeSpan? GetRunTime (IMachine machine, DateTime beginDay, DateTime endDay)
    {
      Debug.Assert (DateTimeKind.Unspecified == beginDay.Kind);
      Debug.Assert (DateTimeKind.Unspecified == endDay.Kind);
      
      double? result = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("MachineActivitySummaryRunTime")
        .SetParameter ("machineId", machine.Id)
        .SetParameter ("beginDay", beginDay)
        .SetParameter ("endDay", endDay)
        .UniqueResult<double?> ();
      return result.HasValue
        ? TimeSpan.FromSeconds (result.Value)
        : (TimeSpan?)null;
    }
    
    /// <summary>
    /// Find the machine activity summaries in a day range
    /// with an early fetch of the machine mode
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IMachineActivitySummary> FindInDayRangeWithMachineMode (IMachine machine,
                                                                         DayRange range)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineActivitySummary> ()
        .Fetch (SelectMode.Fetch, "MachineMode")
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (InDayRange (range))
        .AddOrder (Order.Asc ("Day"))
        .List<IMachineActivitySummary> ();
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
  }
}

