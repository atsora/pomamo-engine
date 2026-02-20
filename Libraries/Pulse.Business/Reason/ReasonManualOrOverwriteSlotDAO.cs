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
  /// DAO for ReasonManualOrOverwriteSlot
  /// </summary>
  public class ReasonManualOrOverwriteSlotDAO
    : IReasonManualOrOverwriteSlotDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (ReasonManualOrOverwriteSlotDAO).FullName);

    #region IReasonManualOrOverwriteSlotDAO implementation
    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <param name="extend">Try to extend the slots</param>
    /// <returns></returns>
    public IReasonManualOrOverwriteSlot FindAt (IMachine machine, DateTime dateTime, bool extend)
    {
      Debug.Assert (null != machine);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.Reason.ReasonManualOrOverwriteSlotDAO.FindAt")) {
          var reasonSlot = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindAt (machine, dateTime);
          if (null == reasonSlot) {
            return null;
          }

          // Filter: not processing AND (overwrite required OR manual reason)
          if (reasonSlot.IsProcessing ()) {
            return null;
          }
          if (!reasonSlot.OverwriteRequired && !reasonSlot.ReasonSource.HasFlag (ReasonSource.Manual)) {
            return null;
          }

          IReasonManualOrOverwriteSlot slot = new ReasonManualOrOverwriteSlot (reasonSlot);
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
    public IList<IReasonManualOrOverwriteSlot> FindOverlapsRange (IMachine machine, UtcDateTimeRange range, bool extend)
    {
      Debug.Assert (null != machine);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.Reason.ReasonManualOrOverwriteSlotDAO.FindOverlapsRange")) {
          IList<IReasonSlot> reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindOverlapsRange (machine, range)
            .Where (s => !s.IsProcessing () && (s.OverwriteRequired || s.ReasonSource.HasFlag (ReasonSource.Manual)))
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
    bool IsMergeable (IReasonManualOrOverwriteSlot left, IReasonManualOrOverwriteSlot right)
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
    IReasonManualOrOverwriteSlot Merge (IReasonManualOrOverwriteSlot left, IReasonManualOrOverwriteSlot right)
    {
      Debug.Assert (IsMergeable (left, right));
      Debug.Assert (left.DateTimeRange.IsStrictlyLeftOf (right.DateTimeRange));

      // Note: the Union function supports empty ranges
      var newRange = new UtcDateTimeRange (left.DateTimeRange.Union (right.DateTimeRange));
      var newDayRange = new DayRange (left.DayRange.Union (right.DayRange));

      var result = new ReasonManualOrOverwriteSlot (left.Machine, left.Reason, left.JsonData, left.OverwriteRequired, newRange, newDayRange);
      return result;
    }

    IList<IReasonManualOrOverwriteSlot> Merge (IList<IReasonSlot> reasonSlots, bool extend)
    {
      var result = new List<IReasonManualOrOverwriteSlot> ();
      foreach (var reasonSlot in reasonSlots) {
        var newSlot = new ReasonManualOrOverwriteSlot (reasonSlot);
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
    IReasonManualOrOverwriteSlot ExtendLeft (IReasonManualOrOverwriteSlot slot)
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
          // Filter: not processing AND (overwrite required OR manual reason)
          if (leftReasonSlot.IsProcessing ()) {
            return slot;
          }
          if (!leftReasonSlot.OverwriteRequired && !leftReasonSlot.ReasonSource.HasFlag (ReasonSource.Manual)) {
            return slot;
          }
          var leftSlot = new ReasonManualOrOverwriteSlot (leftReasonSlot);
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
    IReasonManualOrOverwriteSlot ExtendRight (IReasonManualOrOverwriteSlot slot)
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
          // Filter: not processing AND (overwrite required OR manual reason)
          if (rightReasonSlot.IsProcessing ()) {
            return slot;
          }
          if (!rightReasonSlot.OverwriteRequired && !rightReasonSlot.ReasonSource.HasFlag (ReasonSource.Manual)) {
            return slot;
          }
          var rightSlot = new ReasonManualOrOverwriteSlot (rightReasonSlot);
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
    #endregion // IReasonManualOrOverwriteSlotDAO implementation
  }
}
