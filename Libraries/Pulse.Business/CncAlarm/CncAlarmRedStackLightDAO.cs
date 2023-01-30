// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Business.CncAlarm
{
  /// <summary>
  /// DAO for CncAlarmRedStackLight
  /// </summary>
  public class CncAlarmRedStackLightDAO
  {
    static readonly string MAX_GAP_KEY = "Business.CncAlarm.CncAlarmRedStackLight.MaxMergeGap";
    static readonly TimeSpan MAX_GAP_DEFAULT = TimeSpan.FromSeconds (0);

    readonly ILog log = LogManager.GetLogger (typeof (CncAlarmRedStackLightDAO).FullName);

    #region ICncAlarmRedStackLightDAO implementation
    /// <summary>
    /// ICncAlarmRedStackLightDAO implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<CncAlarmRedStackLight> FindOverlapsRange (IMonitoredMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      var mainMachineModule = machine.MainMachineModule;
      if (null == mainMachineModule) {
        log.ErrorFormat ("FindOverlapsRange: no main machine module for machine id={0}", machine.Id);
        return new List<CncAlarmRedStackLight> ();
      }

      var field = ModelDAOHelper.DAOFactory.FieldDAO
        .FindById ((int)FieldId.StackLight);
      Debug.Assert (null != field);

      var redStackLights = ModelDAOHelper.DAOFactory.CncValueDAO
        .FindByMachineFieldDateRange (mainMachineModule, field, range)
        .Where (v => ((StackLight)v.Value).IsOnOrFlashing (StackLightColor.Red))
        .OrderBy (v => v.DateTimeRange.Lower.Value.Ticks);

      List<CncAlarmRedStackLight> cncAlarmRedStackLights = new List<CncAlarmRedStackLight> ();

      foreach (var redStackLight in redStackLights) {
        IEnumerable<ICncAlarm> cncAlarms = ModelDAOHelper.DAOFactory.CncAlarmDAO
          .FindOverlapsRange (machine, redStackLight.DateTimeRange)
          .OrderBy (a => a.DateTimeRange.Lower.Value.Ticks);
        foreach (var cncAlarm in cncAlarms) {
          cncAlarmRedStackLights.Add (new CncAlarmRedStackLight (cncAlarm, redStackLight));
        }
      }

      return Merge (cncAlarmRedStackLights, range);
    }

    /// <summary>
    /// Check if two IMergeable object can be effectively be merged
    /// </summary>
    /// <param name="left">not null</param>
    /// <param name="right">not null</param>
    /// <returns></returns>
    bool IsMergeable (CncAlarmRedStackLight left, CncAlarmRedStackLight right)
    {
      if (left.DateTimeRange.IsEmpty ()) {
        log.DebugFormat ("IsMergeable: " +
                         "left range is empty " +
                         "=> return true because it can be dismissed");
        return true;
      }
      if (right.DateTimeRange.IsEmpty ()) {
        log.DebugFormat ("IsMergeable: " +
                         "right range is empty " +
                         "=> return true because it can be dismissed");
        return true;
      }

      // For the moment, there is a single color, which simplifies the process
      // If the ranges overlap with each other, they can be just merged
      Debug.Assert (left.ReferenceDataEquals (right));
      if (left.DateTimeRange.Overlaps (right.DateTimeRange)) {
        log.DebugFormat ("IsMergeable: " +
                         "ranges overlap with each other " +
                         "=> return true because there is a single color for the moment");
        return true;
      }

      if (left.DateTimeRange.IsAdjacentTo (right.DateTimeRange)) {
        return true;
      }
      else { // Check the gap between them
        Debug.Assert (right.DateTimeRange.Lower.HasValue);
        Debug.Assert (left.DateTimeRange.Upper.HasValue);
        TimeSpan gap = right.DateTimeRange.Lower.Value.Subtract (left.DateTimeRange.Upper.Value);
        TimeSpan maxGap = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (MAX_GAP_KEY,
                                                                       MAX_GAP_DEFAULT);
        return gap <= maxGap;
      }
    }

    /// <summary>
    /// Merge two mergeable items
    /// </summary>
    /// <param name="left">not null</param>
    /// <param name="right">not null</param>
    /// <returns></returns>
    CncAlarmRedStackLight Merge (CncAlarmRedStackLight left, CncAlarmRedStackLight right)
    {
      Debug.Assert (IsMergeable (left, right));

      if (left.DateTimeRange.IsEmpty ()) {
        return right;
      }
      if (right.DateTimeRange.IsEmpty ()) {
        return left;
      }

      UtcDateTimeRange newRange;
      DayRange newDayRange;
      TimeSpan? newDuration;

      // For the moment, there is a single color, which simplifies the process
      // If the ranges overlap with each other, they can be just merged
      Debug.Assert (left.ReferenceDataEquals (right));
      if (left.DateTimeRange.Overlaps (right.DateTimeRange)) {
        newRange = new UtcDateTimeRange (left.DateTimeRange.Union (right.DateTimeRange));
        newDayRange = new DayRange (left.DayRange.Union (right.DayRange));
        newDuration = newRange.Duration;
      }
      else {
        // Note: the Union function supports empty ranges
        //       but it does not support any gap between left and right although it may happen
        //       because of the IsMergeable method
        newRange = new UtcDateTimeRange (left.DateTimeRange.Lower,
                                         right.DateTimeRange.Upper);
        newDayRange =
          new DayRange (left.DayRange.Lower,
                        right.DayRange.Upper);
        if (!left.Duration.HasValue || !right.Duration.HasValue) {
          newDuration = null;
        }
        else {
          newDuration = left.Duration.Value.Add (right.Duration.Value);
        }
      }

      CncAlarmRedStackLight result = new CncAlarmRedStackLight (left.Machine,
                                                                newRange,
                                                                newDayRange,
                                                                newDuration);
      return result;
    }

    IList<CncAlarmRedStackLight> Merge (IEnumerable<CncAlarmRedStackLight> cncAlarms, UtcDateTimeRange range)
    {
      IList<CncAlarmRedStackLight> result = new List<CncAlarmRedStackLight> ();

      if (!cncAlarms.Any ()) {
        return result;
      }

      IList<CncAlarmRedStackLight> list = new List<CncAlarmRedStackLight> ();
      Group (list, cncAlarms, new LowerBound<DateTime> (null));

      foreach (var newSlot in list.Where (s => s.DateTimeRange.Overlaps (range))) {
        if ((1 <= result.Count)
            && IsMergeable (result[result.Count - 1], newSlot)) {
          result[result.Count - 1] =
            Merge (result[result.Count - 1], newSlot);
        }
        else {
          if (!newSlot.IsEmpty ()) {
            result.Add (newSlot);
          }
        }
      }
      return result;
    }

    void Group (IList<CncAlarmRedStackLight> list, IEnumerable<CncAlarmRedStackLight> cncAlarms, LowerBound<DateTime> lower)
    {
      var containing = cncAlarms
        .Where (a => a.DateTimeRange.ContainsElement (lower));
      if (containing.Any ()) {
        var highestPriority = containing
          .First ();
        UpperBound<DateTime> upper = highestPriority.DateTimeRange.Upper;
        bool upperInclusive = highestPriority.DateTimeRange.UpperInclusive;
        UtcDateTimeRange range = new UtcDateTimeRange (lower, upper, true, upperInclusive);
        list.Add (new CncAlarmRedStackLight (highestPriority.Machine, range));
        if (upper.HasValue) {
          DateTime newLower = upper.Value;
          if (upperInclusive) {
            newLower = newLower.AddSeconds (1);
          }
          Debug.Assert (lower < newLower);
          Group (list, cncAlarms.Where (a => Bound.Compare<DateTime> (upper, a.DateTimeRange.Upper) <= 0), newLower);
        }
      }
      else {
        var first = cncAlarms.FirstOrDefault (a => (Bound.Compare<DateTime> (lower, a.DateTimeRange.Lower) < 0));
        if (null == first) {
          return;
        }
        else {
          Debug.Assert (lower < first.DateTimeRange.Lower);
          Group (list, cncAlarms, first.DateTimeRange.Lower);
        }
      }
    }
    #endregion // ICncAlarmRedStackLightDAO implementation
  }
}
