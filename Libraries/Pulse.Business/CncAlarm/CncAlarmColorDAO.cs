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
  /// Implementation of <see cref="Lemoine.ModelDAO.ICncAlarmColorDAO">ICncAlarmColorDAO</see>
  /// </summary>
  public class CncAlarmColorDAO
    : ICncAlarmColorDAO
  {
    static readonly string MAX_GAP_KEY = "Business.CncAlarm.CncAlarmColor.MaxMergeGap";
    static readonly TimeSpan MAX_GAP_DEFAULT = TimeSpan.FromSeconds (0);

    readonly ILog log = LogManager.GetLogger (typeof (CncAlarmColorDAO).FullName);

    readonly bool m_businessSeverity;

    /// <summary>
    /// Constructor: the severity is computed by the business request <see cref="CncAlarmSeverityFromAttributes"/>
    /// </summary>
    public CncAlarmColorDAO ()
      : this (true)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="businessSeverity">true: the severity is computed by the business request <see cref="CncAlarmSeverityFromAttributes"/>,
    /// false: the severity is retrieved from the dynamic column cncalarmseverityid</param>
    public CncAlarmColorDAO (bool businessSeverity)
    {
      m_businessSeverity = businessSeverity;
    }

    /// <summary>
    /// Cnc alarm with its color and priority that are computed from the severity
    /// </summary>
    sealed class CncAlarmWithSeverity
    {
      public ICncAlarm CncAlarm { get; }
      public string Color { get; }
      public int Priority { get; }
      public UtcDateTimeRange DateTimeRange => this.CncAlarm.DateTimeRange;

      public CncAlarmWithSeverity (ICncAlarm cncAlarm, ICncAlarmSeverity severity)
      {
        this.CncAlarm = cncAlarm;
        this.Color = severity?.Color;
        this.Priority = severity?.Priority ?? 1000; // Same as CncAlarm.Priority
      }
    }

    #region ICncAlarmColorDAO implementation
    /// <summary>
    /// ICncAlarmColorDAO implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<ICncAlarmColor> FindOverlapsRange (IMonitoredMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      // Note: the dynamic column cncalarmseverityid is costly
      // (and CncAlarmDAO.FindOverlapsRangeWithSeverity is pretty inefficient).
      // By default, get the severity from the cached business request CncAlarmSeverityFromAttributes instead
      IEnumerable<CncAlarmWithSeverity> cncAlarms;
      if (m_businessSeverity) {
        cncAlarms = machine.MachineModules
          .SelectMany (m => ModelDAOHelper.DAOFactory.CncAlarmDAO.FindOverlapsRangeWithoutSeverity (m, range))
          .Select (a => new CncAlarmWithSeverity (a, ServiceProvider.Get (new CncAlarmSeverityFromAttributes (a))));
      }
      else {
        cncAlarms = ModelDAOHelper.DAOFactory.CncAlarmDAO
          .FindOverlapsRange (machine, range)
          .Select (a => new CncAlarmWithSeverity (a, a.Severity));
      }
      cncAlarms = cncAlarms
        .Where (a => !string.IsNullOrEmpty (a.Color))
        .OrderBy (a => a.DateTimeRange.Lower.Value.Ticks)
        .ToList ();
      return Merge (cncAlarms, range);
    }

    /// <summary>
    /// Check if two IMergeable object can be effectively be merged
    /// </summary>
    /// <param name="left">not null</param>
    /// <param name="right">not null</param>
    /// <returns></returns>
    bool IsMergeable (ICncAlarmColor left, ICncAlarmColor right)
    {
      if (!object.Equals (left.Color, right.Color)) {
        log.DebugFormat ("IsMergeable: " +
                         "the color is not the same => return false");
        return false;
      }

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
    ICncAlarmColor Merge (ICncAlarmColor left, ICncAlarmColor right)
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

      ICncAlarmColor result = new CncAlarmColor (left.Machine,
                                                 left.Color,
                                                 newRange,
                                                 newDayRange,
                                                 newDuration);
      return result;
    }

    IList<ICncAlarmColor> Merge (IEnumerable<CncAlarmWithSeverity> cncAlarms, UtcDateTimeRange range)
    {
      IList<ICncAlarmColor> result = new List<ICncAlarmColor> ();

      if (!cncAlarms.Any ()) {
        return result;
      }

      IList<ICncAlarmColor> list = new List<ICncAlarmColor> ();
      Group (list, cncAlarms, new LowerBound<DateTime> (null));

      foreach (var newSlot in list.Where (c => c.DateTimeRange.Overlaps (range))) {
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

    /// <summary>
    /// Split the cnc alarms into consecutive slots, keeping in each slot the alarm with the highest priority
    ///
    /// Iterative implementation: a recursive one may lead to a stack overflow
    /// </summary>
    /// <param name="list"></param>
    /// <param name="cncAlarms">with a lower bound</param>
    /// <param name="lower"></param>
    void Group (IList<ICncAlarmColor> list, IEnumerable<CncAlarmWithSeverity> cncAlarms, LowerBound<DateTime> lower)
    {
      var remaining = cncAlarms.ToList ();
      while (remaining.Any ()) {
        var containing = remaining
          .Where (a => a.DateTimeRange.ContainsElement (lower))
          .ToList ();
        if (!containing.Any ()) {
          // Move to the beginning of the next alarm that starts strictly after lower
          // Note: Bound.Compare does not consider the inclusivity of the bounds,
          // so an alarm that ends at lower must not be considered here (else the process loops)
          var next = remaining
            .Where (a => Bound.Compare<DateTime> (lower, a.DateTimeRange.Lower) < 0)
            .OrderBy (a => a.DateTimeRange.Lower.Value.Ticks)
            .FirstOrDefault ();
          if (null == next) {
            return;
          }
          lower = next.DateTimeRange.Lower;
          continue;
        }

        var highestPriority = containing
          .OrderBy (a => a.Priority)
          .First ();
        var nextHigherPriority = remaining
          .Where (a => a.DateTimeRange.Overlaps (new UtcDateTimeRange (lower, highestPriority.DateTimeRange.Upper))
                       && (a.Priority < highestPriority.Priority))
          .ToList ();
        UpperBound<DateTime> upper;
        bool upperInclusive;
        if (nextHigherPriority.Any ()) {
          upper = new DateTime (nextHigherPriority.Min (a => a.DateTimeRange.Lower.Value.Ticks), DateTimeKind.Utc);
          upperInclusive = false;
        }
        else {
          upper = highestPriority.DateTimeRange.Upper;
          upperInclusive = highestPriority.DateTimeRange.UpperInclusive;
        }
        UtcDateTimeRange range = new UtcDateTimeRange (lower, upper, true, upperInclusive);
        list.Add (new CncAlarmColor (highestPriority.CncAlarm.MachineModule.MonitoredMachine, highestPriority.Color, range, range.Duration));
        if (!upper.HasValue) {
          return;
        }
        DateTime newLower = upper.Value;
        if (upperInclusive) {
          newLower = newLower.AddSeconds (1);
        }
        if (0 <= Bound.Compare<DateTime> (lower, (LowerBound<DateTime>)newLower)) {
          log.Error ($"Group: new lower {newLower} is not after {lower} => stop to prevent an infinite loop");
          return;
        }
        lower = newLower;
        remaining = remaining
          .Where (a => Bound.Compare<DateTime> (upper, a.DateTimeRange.Upper) <= 0)
          .ToList ();
      }
    }
    #endregion // ICncAlarmColorDAO implementation
  }
}
