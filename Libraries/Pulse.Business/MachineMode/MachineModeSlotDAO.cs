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

namespace Lemoine.Business.MachineMode
{
  /// <summary>
  /// DAO for MachineModeSlot
  /// </summary>
  public class MachineModeSlotDAO
    : IMachineModeSlotDAO
  {
    readonly ILog log = LogManager.GetLogger(typeof (MachineModeSlotDAO).FullName);
    
    #region IMachineModeSlotDAO implementation
    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// 
    /// Be careful ! This can be used only on global slots
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <param name="extend">Extend the slots</param>
    /// <returns></returns>
    public IMachineModeSlot FindAt(IMachine machine, DateTime dateTime, bool extend)
    {
      Debug.Assert (null != machine);
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.MachineMode.MachineModeSlotDAO.FindAt3"))
        {
          IMachineModeSlot slot = new MachineModeSlot (
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
    /// <param name="extend">Extend the slots</param>
    /// <returns></returns>
    public IList<IMachineModeSlot> FindOverlapsRange (IMachine machine, UtcDateTimeRange range, bool extend)
    {
      Debug.Assert (null != machine);
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.MachineMode.MachineModeSlotDAO.FindOverlapsRange3"))
        {
          IList<IReasonSlot> reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindOverlapsRange (machine, range);
          return Merge (reasonSlots, extend);
        }
      }
    }
    
    /// <summary>
    /// Find all the slots that overlap the specified range
    /// with an early fetch of the machine mode
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="extend">Extend the slots</param>
    /// <returns></returns>
    public IList<IMachineModeSlot> FindOverlapsRangeWithMachineMode (IMachine machine, UtcDateTimeRange range, bool extend)
    {
      Debug.Assert (null != machine);
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.MachineMode.MachineModeSlotDAO.FindOverlapsRangeWithMachineMode"))
        {
          IList<IReasonSlot> reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindAllInUtcRangeWithMachineMode (machine, range);
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
    bool IsMergeable (IMachineModeSlot left, IMachineModeSlot right)
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
    IMachineModeSlot Merge (IMachineModeSlot left, IMachineModeSlot right)
    {
      Debug.Assert (IsMergeable (left, right));
      Debug.Assert (left.DateTimeRange.IsStrictlyLeftOf (right.DateTimeRange));
      
      // Note: the Union function supports empty ranges
      UtcDateTimeRange newRange =
        new UtcDateTimeRange (left.DateTimeRange.Union (right.DateTimeRange));
      DayRange newDayRange =
        new DayRange (left.DayRange.Union (right.DayRange));
      
      IMachineModeSlot result = new MachineModeSlot (left.Machine,
                                                     left.MachineMode,
                                                     newRange,
                                                     newDayRange);
      { // Reasons
        foreach (var subSlot in left.ReasonSlots) {
          result.ReasonSlots.Add (subSlot);
        }
        foreach (var subSlot in right.ReasonSlots) {
          IReasonSubSlot lastSubSlot = result.ReasonSlots.LastOrDefault ();
          if ( (null != lastSubSlot) && (MergeableItem.IsMergeable (lastSubSlot, subSlot))) {
            IReasonSubSlot merged = MergeableItem.Merge (lastSubSlot, subSlot);
            result.ReasonSlots [result.ReasonSlots.Count - 1] = merged;
          }
          else {
            result.ReasonSlots.Add (subSlot);
          }
        }
      }
      { // MachineObservationStates
        foreach (var subSlot in left.MachineObservationStateSlots) {
          result.MachineObservationStateSlots.Add (subSlot);
        }
        foreach (var subSlot in right.MachineObservationStateSlots) {
          IMachineObservationStateSubSlot lastSubSlot = result.MachineObservationStateSlots.LastOrDefault ();
          if ( (null != lastSubSlot) && (MergeableItem.IsMergeable (lastSubSlot, subSlot))) {
            IMachineObservationStateSubSlot merged = MergeableItem.Merge (lastSubSlot, subSlot);
            result.MachineObservationStateSlots [result.MachineObservationStateSlots.Count - 1] = merged;
          }
          else {
            result.MachineObservationStateSlots.Add (subSlot);
          }
        }
      }
      return result;
    }
    
    IList<IMachineModeSlot> Merge (IList<IReasonSlot> reasonSlots, bool extend)
    {
      IList<IMachineModeSlot> result = new List<IMachineModeSlot> ();
      foreach (var reasonSlot in reasonSlots) {
        IMachineModeSlot newSlot = new MachineModeSlot (reasonSlot);
        if ( (1 <= result.Count)
            && IsMergeable (result [result.Count - 1], newSlot)) {
          result [result.Count - 1] =
            Merge (result [result.Count - 1], newSlot);
        }
        else {
          result.Add (newSlot);
        }
      }
      if (extend && (0 < result.Count)) {
        result [0] = ExtendLeft (result [0]);
        result [result.Count - 1] = ExtendRight (result [result.Count - 1]);
      }
      return result;
    }
    
    /// <summary>
    /// Extend a slot to the left
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    IMachineModeSlot ExtendLeft (IMachineModeSlot slot)
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
          if (leftReasonSlot.IsEmpty ()){
            Debug.Assert (false);
            log.ErrorFormat ("ExtendLeft: " +
                             "empty left reason slot {0}",
                             leftReasonSlot);
            return slot;
          }
          IMachineModeSlot leftMachineModeSlot = new MachineModeSlot (leftReasonSlot);
          if (IsMergeable (leftMachineModeSlot, slot)) { // Extend it
            IMachineModeSlot merged =
              Merge (leftMachineModeSlot, slot);
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
    IMachineModeSlot ExtendRight (IMachineModeSlot slot)
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
          if (rightReasonSlot.IsEmpty ()){
            Debug.Assert (false);
            log.ErrorFormat ("ExtendRight: " +
                             "empty right reason slot {0}",
                             rightReasonSlot);
            return slot;
          }
          IMachineModeSlot rightMachineModeSlot = new MachineModeSlot (rightReasonSlot);
          if (IsMergeable (rightMachineModeSlot, slot)) { // Extend it
            IMachineModeSlot merged = Merge (slot, rightMachineModeSlot);
            return this.ExtendRight (merged);
          }
          else {
            return slot;
          }
        }
      }
    }
    #endregion // IMachineModeSlotDAO implementation
  }
}
