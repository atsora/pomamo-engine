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

namespace Lemoine.Business.ProductionState
{
  /// <summary>
  /// DAO for ProductionStateColorSlot
  /// </summary>
  public class ProductionStateColorSlotDAO
    : IProductionStateColorSlotDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (ProductionStateColorSlotDAO).FullName);

    #region IProductionStateColorSlotDAO implementation
    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <param name="extend">Try to extend the slots</param>
    /// <returns></returns>
    public IProductionStateColorSlot FindAt (IMachine machine, DateTime dateTime, bool extend)
    {
      Debug.Assert (null != machine);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.Reason.ProductionStateColorSlotDAO.FindAt3")) {
          IProductionStateColorSlot slot = new ProductionStateColorSlot (
            ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindAt (machine, dateTime));
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
    public IList<IProductionStateColorSlot> FindOverlapsRange (IMachine machine, UtcDateTimeRange range, bool extend)
    {
      Debug.Assert (null != machine);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.Reason.ProductionStateColorSlotDAO.FindOverlapsRange3")) {
          IList<IReasonSlot> reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindOverlapsRange (machine, range)
            .Where (s => (int)ReasonId.Processing != s.Reason.Id)
            .ToList ();
          return Merge (reasonSlots, extend);
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
    public async System.Threading.Tasks.Task<IList<IProductionStateColorSlot>> FindOverlapsRangeAsync (IMachine machine, UtcDateTimeRange range, bool extend)
    {
      Debug.Assert (null != machine);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.Reason.ProductionStateColorSlotDAO.FindOverlapsRange3")) {
          IList<IReasonSlot> reasonSlots = await ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindOverlapsRangeAsync (machine, range);
          reasonSlots = reasonSlots
            .Where (s => (int)ReasonId.Processing != s.Reason.Id)
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
    bool IsMergeable (IProductionStateColorSlot left, IProductionStateColorSlot right)
    {
      if (left.DateTimeRange.IsEmpty ()) {
        Debug.Assert (!left.DateTimeRange.IsEmpty ());
        log.ErrorFormat ("IsMergeable: " +
                         "left range is empty, which is unexpected " +
                         "=> return true because it can be dismissed");
        return true;
      }
      if (right.DateTimeRange.IsEmpty ()) {
        Debug.Assert (!right.DateTimeRange.IsEmpty ());
        log.ErrorFormat ("IsMergeable: " +
                         "right range is empty, which is unexpected " +
                         "=> return true because it can be dismissed");
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
    IProductionStateColorSlot Merge (IProductionStateColorSlot left, IProductionStateColorSlot right)
    {
      Debug.Assert (IsMergeable (left, right));
      Debug.Assert (left.DateTimeRange.IsStrictlyLeftOf (right.DateTimeRange));

      // Note: the Union function supports empty ranges
      UtcDateTimeRange newRange =
        new UtcDateTimeRange (left.DateTimeRange.Union (right.DateTimeRange));
      DayRange newDayRange =
        new DayRange (left.DayRange.Union (right.DayRange));

      IProductionStateColorSlot result = new ProductionStateColorSlot (left.Machine,
                                                     left.Color,
                                                     left.ProductionRate,
                                                     newRange,
                                                     newDayRange);
      return result;
    }

    IList<IProductionStateColorSlot> Merge (IList<IReasonSlot> reasonSlots, bool extend)
    {
      IList<IProductionStateColorSlot> result = new List<IProductionStateColorSlot> ();
      foreach (var reasonSlot in reasonSlots) {
        IProductionStateColorSlot newSlot = new ProductionStateColorSlot (reasonSlot);
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
    IProductionStateColorSlot ExtendLeft (IProductionStateColorSlot slot)
    {
      if (null == slot) {
        return null;
      }
      if (!slot.DateTimeRange.Lower.HasValue) {
        return slot;
      }
      else {
        IReasonSlot leftReasonSlot = ModelDAOHelper.DAOFactory.ReasonSlotDAO
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
          IProductionStateColorSlot leftProductionStateColorSlot = new ProductionStateColorSlot (leftReasonSlot);
          if (IsMergeable (leftProductionStateColorSlot, slot)) { // Extend it
            IProductionStateColorSlot merged =
              Merge (leftProductionStateColorSlot, slot);
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
    IProductionStateColorSlot ExtendRight (IProductionStateColorSlot slot)
    {
      if (null == slot) {
        return null;
      }
      if (!slot.DateTimeRange.Upper.HasValue) {
        return slot;
      }
      else {
        IReasonSlot rightReasonSlot = ModelDAOHelper.DAOFactory.ReasonSlotDAO
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
          IProductionStateColorSlot rightProductionStateColorSlot = new ProductionStateColorSlot (rightReasonSlot);
          if (IsMergeable (rightProductionStateColorSlot, slot)) { // Extend it
            IProductionStateColorSlot merged = Merge (slot, rightProductionStateColorSlot);
            return this.ExtendRight (merged);
          }
          else {
            return slot;
          }
        }
      }
    }
    #endregion // IProductionStateColorSlotDAO implementation
  }
}
