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

namespace Lemoine.Business.Reason
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IReasonOnlySlotDAO">IReasonOnlySlotDAO</see>
  /// </summary>
  public class ReasonOnlySlotDAO
    : IReasonOnlySlotDAO
  {
    static readonly string FETCH_MARGIN_KEY = "ReasonOnlySlotDAO.FetchMargin";
    static readonly TimeSpan FETCH_MARGIN_DEFAULT = TimeSpan.FromHours (12);

    readonly ILog log = LogManager.GetLogger (typeof (ReasonOnlySlotDAO).FullName);

    #region IReasonOnlySlotDAO implementation
    /// <summary>
    /// Find the unique slot at the specified UTC date/time not trying to extend the slots
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    public IReasonOnlySlot FindAt (IMachine machine, DateTime dateTime)
    {
      Debug.Assert (null != machine);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.Reason.ReasonOnlySlotDAO.FindAt2")) {
          return new ReasonOnlySlot (
            ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindAt (machine, dateTime));
        }
      }
    }

    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="extend">Try to extend the slots</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    public IReasonOnlySlot FindAt (IMachine machine, DateTime dateTime, bool extend)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.Reason.ReasonOnlySlotDAO.FindAt3")) {
          if (extend) {
            bool lowerExtendReached, upperExtendReached;
            return FindAt (machine, dateTime,
                           new UtcDateTimeRange (new LowerBound<DateTime> (null), new UpperBound<DateTime> (null)),
                           out lowerExtendReached, out upperExtendReached);
          }
          else {
            return FindAt (machine, dateTime);
          }
        }
      }
    }

    /// <summary>
    /// Find the unique slot at the specified UTC date/time trying to extend the slots
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <param name="extendLimitRange">Utc date/time limit range to extend the slot</param>
    /// <param name="lowerExtendLimitReached">the lower value of extendLimitRange was reached</param>
    /// <param name="upperExtendLimitReached">the upper value of extendLimitRange was reached</param>
    /// <returns></returns>
    public IReasonOnlySlot FindAt (IMachine machine, DateTime dateTime,
                                  UtcDateTimeRange extendLimitRange, out bool lowerExtendLimitReached, out bool upperExtendLimitReached)
    {
      Debug.Assert (null != machine);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.Reason.ReasonOnlySlotDAO.FindAt5")) {
          IReasonOnlySlot slot = this.FindAt (machine, dateTime);
          if (null != slot) {
            slot = ExtendLeft (slot, extendLimitRange.Lower, out lowerExtendLimitReached);
            slot = ExtendRight (slot, extendLimitRange.Upper, out upperExtendLimitReached);
          }
          else {
            lowerExtendLimitReached = false;
            upperExtendLimitReached = false;
          }
          return slot;
        }
      }
    }

    /// <summary>
    /// Find all the slots that overlap the specified range not trying to extend the slots
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IReasonOnlySlot> FindOverlapsRange (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.Reason.ReasonOnlySlotDAO.FindOverlapsRange2")) {
          IList<IReasonSlot> reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindOverlapsRange (machine, range);
          return Merge (reasonSlots);
        }
      }
    }

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="extend">Try to extend the slots</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IReasonOnlySlot> FindOverlapsRange (IMachine machine, UtcDateTimeRange range, bool extend)
    {
      if (extend) {
        bool lowerExtendReached, upperExtendReached;
        return FindOverlapsRange (machine, range,
                                  new UtcDateTimeRange (new LowerBound<DateTime> (null), new UpperBound<DateTime> (null)),
                                  out lowerExtendReached, out upperExtendReached);
      }
      else {
        return FindOverlapsRange (machine, range);
      }
    }

    /// <summary>
    /// Find all the slots that overlap the specified range trying to extend the slots
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="extendLimitRange">Utc date/time limit range to extend the slot</param>
    /// <param name="lowerExtendLimitReached">the lower value of extendLimitRange was reached</param>
    /// <param name="upperExtendLimitReached">the upper value of extendLimitRange was reached</param>
    /// <returns></returns>
    public IList<IReasonOnlySlot> FindOverlapsRange (IMachine machine, UtcDateTimeRange range,
                                                     UtcDateTimeRange extendLimitRange, out bool lowerExtendLimitReached, out bool upperExtendLimitReached)
    {
      Debug.Assert (null != machine);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.Reason.ReasonOnlySlotDAO.FindOverlapsRange5")) {
          UtcDateTimeRange fetchRange = GetFetchRange (range, extendLimitRange);
          IList<IReasonSlot> reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindOverlapsRange (machine, fetchRange);
          return Merge (reasonSlots, range, extendLimitRange, out lowerExtendLimitReached, out upperExtendLimitReached);
        }
      }
    }

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// with an early fetch of the reason
    /// trying to extend the slots
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="extendLimitRange">Utc date/time limit range to extend the slot</param>
    /// <param name="lowerExtendLimitReached">the lower value of extendLimitRange was reached</param>
    /// <param name="upperExtendLimitReached">the upper value of extendLimitRange was reached</param>
    /// <returns></returns>
    public IList<IReasonOnlySlot> FindOverlapsRangeWithReason (IMachine machine, UtcDateTimeRange range,
                                                               UtcDateTimeRange extendLimitRange, out bool lowerExtendLimitReached, out bool upperExtendLimitReached)
    {
      Debug.Assert (null != machine);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.Reason.ReasonOnlySlotDAO.FindOverlapsRangeWithReason5")) {
          UtcDateTimeRange fetchRange = GetFetchRange (range, extendLimitRange);
          IList<IReasonSlot> reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindAllInUtcRangeWithMachineModeReason (machine, fetchRange);
          return Merge (reasonSlots, range, extendLimitRange, out lowerExtendLimitReached, out upperExtendLimitReached);
        }
      }
    }

    UtcDateTimeRange GetFetchRange (UtcDateTimeRange range, UtcDateTimeRange extendLimitRange)
    {
      var lowerBound = range.Lower;
      if (lowerBound.HasValue) {
        var fetchExtension = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (FETCH_MARGIN_KEY,
                                                                          FETCH_MARGIN_DEFAULT);
        lowerBound = lowerBound.Value.Subtract (fetchExtension);
      }
      var upperBound = range.Upper;
      if (upperBound.HasValue) {
        var fetchExtension = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (FETCH_MARGIN_KEY,
                                                                          FETCH_MARGIN_DEFAULT);
        upperBound = upperBound.Value.Add (fetchExtension);
      }
      var fetchRange = new UtcDateTimeRange (lowerBound, upperBound);
      return new UtcDateTimeRange (fetchRange.Intersects (extendLimitRange));
    }

    /// <summary>
    /// Check if two IMergeable object can be effectively be merged
    /// </summary>
    /// <param name="left">not null</param>
    /// <param name="right">not null</param>
    /// <returns></returns>
    bool IsMergeable (IReasonOnlySlot left, IReasonOnlySlot right)
    {
      if (left.DateTimeRange.IsEmpty ()) {
        Debug.Assert (!left.DateTimeRange.IsEmpty ());
        log.Error ("IsMergeable: left range is empty, which is unexpected => return true because it can be dismissed");
        return true;
      }
      if (right.DateTimeRange.IsEmpty ()) {
        Debug.Assert (!right.DateTimeRange.IsEmpty ());
        log.Error ("IsMergeable: right range is empty, which is unexpected => return true because it can be dismissed");
        return true;
      }

      return left.ReferenceDataEquals (right)
        && left.DateTimeRange.IsAdjacentTo (right.DateTimeRange);
    }

    /// <summary>
    /// Merge two mergeable items
    /// </summary>
    /// <param name="left">not null</param>
    /// <param name="right">not null</param>
    /// <returns></returns>
    IReasonOnlySlot Merge (IReasonOnlySlot left, IReasonOnlySlot right)
    {
      Debug.Assert (IsMergeable (left, right));
      Debug.Assert (left.DateTimeRange.IsStrictlyLeftOf (right.DateTimeRange));

      // Note: the Union function supports empty ranges
      UtcDateTimeRange newRange =
        new UtcDateTimeRange (left.DateTimeRange.Union (right.DateTimeRange));
      DayRange newDayRange =
        new DayRange (left.DayRange.Union (right.DayRange));

      IReasonOnlySlot result = new ReasonOnlySlot (left.Machine,
                                                   left.Reason,
                                                   left.JsonData,
                                                   left.Running,
                                                   left.ReasonScore,
                                                   left.ReasonSource,
                                                   left.AutoReasonNumber,
                                                   left.OverwriteRequired,
                                                   left.ReasonDetails,
                                                   left.DefaultReason,
                                                   newRange,
                                                   newDayRange);
      { // MachineModes
        foreach (var subSlot in left.MachineModeSlots) {
          result.MachineModeSlots.Add (subSlot);
        }
        foreach (var subSlot in right.MachineModeSlots) {
          IMachineModeSubSlot lastSubSlot = result.MachineModeSlots.LastOrDefault ();
          if ((null != lastSubSlot) && (MergeableItem.IsMergeable (lastSubSlot, subSlot))) {
            IMachineModeSubSlot merged = MergeableItem.Merge (lastSubSlot, subSlot);
            result.MachineModeSlots[result.MachineModeSlots.Count - 1] = merged;
          }
          else {
            result.MachineModeSlots.Add (subSlot);
          }
        }
      }
      { // MachineObservationStates
        foreach (var subSlot in left.MachineObservationStateSlots) {
          result.MachineObservationStateSlots.Add (subSlot);
        }
        foreach (var subSlot in right.MachineObservationStateSlots) {
          IMachineObservationStateSubSlot lastSubSlot = result.MachineObservationStateSlots.LastOrDefault ();
          if ((null != lastSubSlot) && (MergeableItem.IsMergeable (lastSubSlot, subSlot))) {
            IMachineObservationStateSubSlot merged = MergeableItem.Merge (lastSubSlot, subSlot);
            result.MachineObservationStateSlots[result.MachineObservationStateSlots.Count - 1] = merged;
          }
          else {
            result.MachineObservationStateSlots.Add (subSlot);
          }
        }
      }
      return result;
    }

    IList<IReasonOnlySlot> Merge (IEnumerable<IReasonSlot> reasonSlots)
    {
      IList<IReasonOnlySlot> result = new List<IReasonOnlySlot> ();
      foreach (var reasonSlot in reasonSlots) {
        IReasonOnlySlot newSlot = new ReasonOnlySlot (reasonSlot);
        if ((1 <= result.Count)
            && IsMergeable (result[result.Count - 1], newSlot)) {
          result[result.Count - 1] =
            Merge (result[result.Count - 1], newSlot);
        }
        else {
          result.Add (newSlot);
        }
      }
      return result;
    }

    IList<IReasonOnlySlot> Merge (IList<IReasonSlot> reasonSlots, UtcDateTimeRange range, out bool extendLeft, out bool extendRight)
    {
      IList<IReasonOnlySlot> result = Merge (reasonSlots.Where (s => s.DateTimeRange.Overlaps (range)));
      if (0 == result.Count) {
        extendLeft = false;
        extendRight = false;
        return result;
      }

      // - Extend on the left
      extendLeft = true;
      foreach (var leftSlot in reasonSlots
               .Where (s => s.DateTimeRange.IsStrictlyLeftOf (range))
               .OrderBy (s => s.DateTimeRange)
               .Reverse ()) {
        if (leftSlot.IsEmpty ()) {
          Debug.Assert (false);
          log.ErrorFormat ("Merge: empty left reason slot {0}",
                           leftSlot);
          continue;
        }
        var firstSlot = result[0];
        var leftReasonOnlySlot = new ReasonOnlySlot (leftSlot);
        if (IsMergeable (leftReasonOnlySlot, firstSlot)) { // Extend it
          var merged =
            Merge (leftReasonOnlySlot, firstSlot);
          result[0] = merged;
        }
        else { // Not mergeable: this is not useful to try to extend it on the left afterwards
          extendLeft = false;
          break;
        }
      }

      // - Extend on the right
      extendRight = true;
      foreach (var rightSlot in reasonSlots
               .Where (s => s.DateTimeRange.IsStrictlyRightOf (range))
               .OrderBy (s => s.DateTimeRange)) {
        if (rightSlot.IsEmpty ()) {
          Debug.Assert (false);
          log.ErrorFormat ("Merge: empty right reason slot {0}",
                           rightSlot);
          continue;
        }
        var lastSlot = result[result.Count - 1];
        var rightReasonOnlySlot = new ReasonOnlySlot (rightSlot);
        if (IsMergeable (lastSlot, rightReasonOnlySlot)) { // Extend it
          var merged =
            Merge (lastSlot, rightReasonOnlySlot);
          result[result.Count - 1] = merged;
        }
        else { // Not mergeable: this is not useful to try to extend it on the left afterwards
          extendRight = false;
          break;
        }
      }

      return result;
    }

    IList<IReasonOnlySlot> Merge (IList<IReasonSlot> reasonSlots, UtcDateTimeRange range,
                                  UtcDateTimeRange extendLimitRange, out bool lowerExtendLimitReached, out bool upperExtendLimitReached)
    {
      bool extendLeft, extendRight;
      IList<IReasonOnlySlot> result = Merge (reasonSlots, range, out extendLeft, out extendRight);
      if (0 < result.Count) {
        if (extendLeft) {
          result[0] = ExtendLeft (result[0], extendLimitRange.Lower, out lowerExtendLimitReached);
        }
        else {
          lowerExtendLimitReached = false;
        }
        if (extendRight) {
          result[result.Count - 1] = ExtendRight (result[result.Count - 1], extendLimitRange.Upper, out upperExtendLimitReached);
        }
        else {
          upperExtendLimitReached = false;
        }
      }
      else {
        lowerExtendLimitReached = false;
        upperExtendLimitReached = false;
      }
      return result;
    }

    /// <summary>
    /// Extend a slot to the left
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="lowerLimit"></param>
    /// <param name="lowerLimitReached"></param>
    /// <returns></returns>
    IReasonOnlySlot ExtendLeft (IReasonOnlySlot slot, LowerBound<DateTime> lowerLimit, out bool lowerLimitReached)
    {
      if (null == slot) {
        lowerLimitReached = false;
        return null;
      }
      if (!slot.DateTimeRange.Lower.HasValue) {
        lowerLimitReached = false;
        return slot;
      }
      else if (Bound.Compare<DateTime> (slot.DateTimeRange.Lower.Value, lowerLimit) <= 0) {
        log.DebugFormat ("ExtendLeft: lower limit {0} reached",
                         lowerLimit);
        lowerLimitReached = true;
        return slot;
      }
      else {
        IReasonSlot leftReasonSlot = ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .FindWithEnd (slot.Machine, slot.DateTimeRange.Lower.Value);
        if (null == leftReasonSlot) {
          lowerLimitReached = false;
          return slot;
        }
        else { // null != leftReasonSlot
          if (leftReasonSlot.IsEmpty ()) {
            Debug.Assert (false);
            log.ErrorFormat ("ExtendLeft: empty left reason slot {0}",
                             leftReasonSlot);
            lowerLimitReached = false;
            return slot;
          }
          IReasonOnlySlot leftReasonOnlySlot = new ReasonOnlySlot (leftReasonSlot);
          if (IsMergeable (leftReasonOnlySlot, slot)) { // Extend it
            IReasonOnlySlot merged =
              Merge (leftReasonOnlySlot, slot);
            return this.ExtendLeft (merged, lowerLimit, out lowerLimitReached);
          }
          else {
            lowerLimitReached = false;
            return slot;
          }
        }
      }
    }

    /// <summary>
    /// Extend a slot to the right
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="upperLimit"></param>
    /// <param name="upperLimitReached"></param>
    /// <returns></returns>
    IReasonOnlySlot ExtendRight (IReasonOnlySlot slot, UpperBound<DateTime> upperLimit, out bool upperLimitReached)
    {
      if (null == slot) {
        upperLimitReached = false;
        return null;
      }
      if (!slot.DateTimeRange.Upper.HasValue) {
        upperLimitReached = false;
        return slot;
      }
      else if (Bound.Compare<DateTime> (upperLimit, slot.DateTimeRange.Upper.Value) <= 0) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("ExtendRight: upper limit {0} reached",
                           upperLimit);
        }
        upperLimitReached = true;
        return slot;
      }
      else {
        IReasonSlot rightReasonSlot = ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .FindAt (slot.Machine, slot.DateTimeRange.Upper.Value);
        if (null == rightReasonSlot) {
          upperLimitReached = false;
          return slot;
        }
        else { // null != rightReasonSlot
          if (rightReasonSlot.IsEmpty ()) {
            Debug.Assert (false);
            log.ErrorFormat ("ExtendRight: empty right reason slot {0}",
                             rightReasonSlot);
            upperLimitReached = false;
            return slot;
          }
          IReasonOnlySlot rightReasonOnlySlot = new ReasonOnlySlot (rightReasonSlot);
          if (IsMergeable (rightReasonOnlySlot, slot)) { // Extend it
            IReasonOnlySlot merged = Merge (slot, rightReasonOnlySlot);
            return this.ExtendRight (merged, upperLimit, out upperLimitReached);
          }
          else {
            upperLimitReached = false;
            return slot;
          }
        }
      }
    }
    #endregion // IReasonOnlySlotDAO implementation
  }
}
