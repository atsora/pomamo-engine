// Copyright (C) 2026 Atsora Solutions
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
  /// DAO for ReasonOverwriteRequiredSlot
  /// </summary>
  public class ReasonOverwriteRequiredSlotDAO
    : IReasonOverwriteRequiredSlotDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (ReasonOverwriteRequiredSlotDAO).FullName);

    #region IReasonOverwriteRequiredSlotDAO implementation
    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <param name="extend">Try to extend the slots</param>
    /// <returns></returns>
    public IReasonOverwriteRequiredSlot FindAt (IMachine machine, DateTime dateTime, bool extend)
    {
      Debug.Assert (null != machine);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.Reason.ReasonOverwriteRequiredSlotDAO.FindAt")) {
          var reasonSlot = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindAt (machine, dateTime);
          if (null == reasonSlot) {
            return null;
          }

          // Filter out processing and non-overwrite required slots
          if (reasonSlot.IsProcessing () || !reasonSlot.OverwriteRequired) {
            return null;
          }

          IReasonOverwriteRequiredSlot slot = new ReasonOverwriteRequiredSlot (reasonSlot);
          if (extend && (null != slot)) {
            slot = ExtendLeft (slot);
            slot = ExtendRight (slot);
          }
          return slot;
        }
      }
    }

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="extend">Try to extend the slots</param>
    /// <returns></returns>
    public IList<IReasonOverwriteRequiredSlot> FindOverlapsRange (IMachine machine, UtcDateTimeRange range, bool extend)
    {
      Debug.Assert (null != machine);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.Reason.ReasonOverwriteRequiredSlotDAO.FindOverlapsRange")) {
          IList<IReasonSlot> reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindOverlapsRange (machine, range)
            .Where (s => !s.IsProcessing () && s.OverwriteRequired)
            .ToList ();
          return Merge (reasonSlots, extend);
        }
      }
    }

    /// <summary>
    /// Check if two IMergeable object can be effectively be merged
    /// </summary>
    /// <param name="left">not null</param>
    /// <param name="right">not null</param>
    /// <returns></returns>
    bool IsMergeable (IReasonOverwriteRequiredSlot left, IReasonOverwriteRequiredSlot right)
    {
      if (left.DateTimeRange.IsEmpty ()) {
        log.Error ($"IsMergeable: left range is empty, which is unexpected => return true because it can be dismissed");
        Debug.Assert (!left.DateTimeRange.IsEmpty ());
        return true;
      }
      if (right.DateTimeRange.IsEmpty ()) {
        log.Error ($"IsMergeable: right range is empty, which is unexpected => return true because it can be dismissed");
        Debug.Assert (!right.DateTimeRange.IsEmpty ());
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
    IReasonOverwriteRequiredSlot Merge (IReasonOverwriteRequiredSlot left, IReasonOverwriteRequiredSlot right)
    {
      Debug.Assert (IsMergeable (left, right));
      Debug.Assert (left.DateTimeRange.IsStrictlyLeftOf (right.DateTimeRange));

      // Note: the Union function supports empty ranges
      UtcDateTimeRange newRange =
        new UtcDateTimeRange (left.DateTimeRange.Union (right.DateTimeRange));
      DayRange newDayRange =
        new DayRange (left.DayRange.Union (right.DayRange));

      var result = new ReasonOverwriteRequiredSlot (left.Machine, left.Color, newRange, newDayRange);
      return result;
    }

    IList<IReasonOverwriteRequiredSlot> Merge (IList<IReasonSlot> reasonSlots, bool extend)
    {
      var result = new List<IReasonOverwriteRequiredSlot> ();
      foreach (var reasonSlot in reasonSlots) {
        var newSlot = new ReasonOverwriteRequiredSlot (reasonSlot);
        if ((1 <= result.Count)
            && IsMergeable (result[result.Count - 1], newSlot)) {
          result[result.Count - 1] =
            Merge (result[result.Count - 1], newSlot);
        }
        else {
          result.Add (newSlot);
        }
      }
      if (extend && (0 < result.Count)) {
        result[0] = ExtendLeft (result[0]);
        result[result.Count - 1] = ExtendRight (result[result.Count - 1]);
      }
      return result;
    }

    /// <summary>
    /// Extend a slot to the left
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    IReasonOverwriteRequiredSlot ExtendLeft (IReasonOverwriteRequiredSlot slot)
    {
      if (null == slot) {
        return null;
      }
      if (!slot.DateTimeRange.Lower.HasValue) {
        return slot;
      }
      else {
        var leftReasonSlot = ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .FindWithEnd (slot.Machine, slot.DateTimeRange.Lower.Value);
        if (null == leftReasonSlot) {
          return slot;
        }
        else { // null != leftReasonSlot
          if (leftReasonSlot.IsEmpty ()) {
            Debug.Assert (false);
            log.Error ($"ExtendLeft: empty left reason slot {leftReasonSlot}");
            return slot;
          }
          // Filter out processing and non-overwrite required slots
          if (leftReasonSlot.IsProcessing () || !leftReasonSlot.OverwriteRequired) {
            return slot;
          }
          var leftSlot = new ReasonOverwriteRequiredSlot (leftReasonSlot);
          if (IsMergeable (leftSlot, slot)) { // Extend it
            var merged =
              Merge (leftSlot, slot);
            return this.ExtendLeft (merged);
          }
          else {
            return slot;
          }
        }
      }
    }

    /// <summary>
    /// Extend a slot to the right
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    IReasonOverwriteRequiredSlot ExtendRight (IReasonOverwriteRequiredSlot slot)
    {
      if (null == slot) {
        return null;
      }
      if (!slot.DateTimeRange.Upper.HasValue) {
        return slot;
      }
      else {
        var rightReasonSlot = ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .FindAt (slot.Machine, slot.DateTimeRange.Upper.Value);
        if (null == rightReasonSlot) {
          return slot;
        }
        else { // null != rightReasonSlot
          if (rightReasonSlot.IsEmpty ()) {
            Debug.Assert (false);
            log.Error ($"ExtendRight: empty right reason slot {rightReasonSlot}");
            return slot;
          }
          // Filter out processing and non-overwrite required slots
          if (rightReasonSlot.IsProcessing () || !rightReasonSlot.OverwriteRequired) {
            return slot;
          }
          var rightSlot = new ReasonOverwriteRequiredSlot (rightReasonSlot);
          if (IsMergeable (slot, rightSlot)) { // Extend it
            var merged = Merge (slot, rightSlot);
            return this.ExtendRight (merged);
          }
          else {
            return slot;
          }
        }
      }
    }
    #endregion // IReasonOverwriteRequiredSlotDAO implementation
  }
}
