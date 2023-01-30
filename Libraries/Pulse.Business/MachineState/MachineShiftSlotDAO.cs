// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Business.MachineState
{
  /// <summary>
  /// MachineShiftSlotDAO
  /// </summary>
  public class MachineShiftSlotDAO
    : IMachineShiftSlotDAO
  {
    readonly ILog log = LogManager.GetLogger<MachineShiftSlotDAO> ();

    /// <summary>
    /// Find the machine shift slot withe specified day and shift
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="day">DateTimeKind=Unspecified</param>
    /// <param name="shift">not null</param>
    /// <returns></returns>
    public IMachineShiftSlot FindWith (IMachine machine, DateTime day, IShift shift)
    {
      Debug.Assert (null != machine);
      Debug.Assert (day.Kind == DateTimeKind.Unspecified);
      Debug.Assert (null != shift);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.MachineState.MachineShiftSlotDAO.FindWith")) {
          var slots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindWith (machine, day, shift);
          if (slots.Any ()) {
            return new MachineShiftSlot (slots.First (), slots.Last ());
          }
          else {
            return null;
          }
        }
      }
    }

    /// <summary>
    /// Find the machine shift slot withe specified day and shift
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="day">DateTimeKind=Unspecified</param>
    /// <param name="shift">not null</param>
    /// <returns></returns>
    public async Task<IMachineShiftSlot> FindWithAsync (IMachine machine, DateTime day, IShift shift)
    {
      Debug.Assert (null != machine);
      Debug.Assert (day.Kind == DateTimeKind.Unspecified);
      Debug.Assert (null != shift);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.MachineState.MachineShiftSlotDAO.FindWithAsync")) {
          var slots = await ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindWithAsync (machine, day, shift);
          if (slots.Any ()) {
            return new MachineShiftSlot (slots.First (), slots.Last ());
          }
          else {
            return null;
          }
        }
      }
    }

    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    public IMachineShiftSlot FindAt (IMachine machine, DateTime dateTime)
    {
      Debug.Assert (null != machine);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.MachineState.MachineShiftSlotDAO.FindAt")) {
          IMachineShiftSlot slot = new MachineShiftSlot (ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindAt (machine, dateTime));
          if (null != slot) {
            slot = ExtendLeft (slot);
            slot = ExtendRight (slot);
          }
          return slot;
        }
      }
    }

    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IMachineShiftSlot> FindAtAsync (IMachine machine, DateTime dateTime)
    {
      Debug.Assert (null != machine);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.MachineState.MachineShiftSlotDAO.FindAt")) {
          IMachineShiftSlot slot = new MachineShiftSlot (await
            ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindAtAsync (machine, dateTime));
          if (null != slot) {
            slot = await ExtendLeftAsync (slot);
            slot = await ExtendRightAsync (slot);
          }
          return slot;
        }
      }
    }

    /// <summary>
    /// Find the end of the unique slot at the specified UTC date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    public IMachineShiftSlot FindAtExtendRightOnly (IMachine machine, DateTime dateTime)
    {
      Debug.Assert (null != machine);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.MachineState.MachineShiftSlotDAO.FindAt")) {
          IMachineShiftSlot slot = new MachineShiftSlot (ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindAt (machine, dateTime));
          if (null != slot) {
            slot = ExtendRight (slot);
          }
          return slot;
        }
      }
    }

    /// <summary>
    /// Find the end of the unique slot at the specified UTC date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IMachineShiftSlot> FindAtExtendRightOnlyAsync (IMachine machine, DateTime dateTime)
    {
      Debug.Assert (null != machine);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.MachineState.MachineShiftSlotDAO.FindAt")) {
          IMachineShiftSlot slot = new MachineShiftSlot (await
            ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindAtAsync (machine, dateTime));
          if (null != slot) {
            slot = await ExtendRightAsync (slot);
          }
          return slot;
        }
      }
    }

    /// <summary>
    /// Check if two IMergeable object can be effectively be merged
    /// </summary>
    /// <param name="left">not null</param>
    /// <param name="right">not null</param>
    /// <returns></returns>
    bool IsMergeable (IMachineShiftSlot left, IMachineShiftSlot right)
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
    IMachineShiftSlot Merge (IMachineShiftSlot left, IMachineShiftSlot right)
    {
      Debug.Assert (IsMergeable (left, right));
      Debug.Assert (left.DateTimeRange.IsStrictlyLeftOf (right.DateTimeRange));

      // Note: the Union function supports empty ranges
      UtcDateTimeRange newRange =
        new UtcDateTimeRange (left.DateTimeRange.Union (right.DateTimeRange));
      DayRange newDayRange =
        new DayRange (left.DayRange.Union (right.DayRange));

      IMachineShiftSlot result = new MachineShiftSlot (left.Machine, left.Shift,
                                             newRange,
                                             newDayRange);
      return result;
    }

    async System.Threading.Tasks.Task<IList<IMachineShiftSlot>> MergeAsync (IList<IObservationStateSlot> observationStateSlots, bool extend)
    {
      IList<IMachineShiftSlot> result = new List<IMachineShiftSlot> ();
      foreach (var observationStateSlot in observationStateSlots) {
        IMachineShiftSlot newSlot = new MachineShiftSlot (observationStateSlot);
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
        result[0] = await ExtendLeftAsync (result[0]);
        result[result.Count - 1] = await ExtendRightAsync (result[result.Count - 1]);
      }
      return result;
    }

    /// <summary>
    /// Extend a slot to the left
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    IMachineShiftSlot ExtendLeft (IMachineShiftSlot slot)
    {
      if (null == slot) {
        return null;
      }
      if (!slot.DateTimeRange.Lower.HasValue) {
        return slot;
      }
      else {
        var leftObservationStateSlot = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
          .FindWithEnd (slot.Machine, slot.DateTimeRange.Lower.Value);
        if (null == leftObservationStateSlot) {
          return slot;
        }
        else { // null != leftReasonSlot
          if (leftObservationStateSlot.IsEmpty ()) {
            Debug.Assert (false);
            log.ErrorFormat ("ExtendLeft: " +
                             "empty left observation state slot {0}",
                             leftObservationStateSlot);
            return slot;
          }
          IMachineShiftSlot leftMachineShiftSlot = new MachineShiftSlot (leftObservationStateSlot);
          if (IsMergeable (leftMachineShiftSlot, slot)) { // Extend it
            IMachineShiftSlot merged =
              Merge (leftMachineShiftSlot, slot);
            return this.ExtendLeft (merged);
          }
          else {
            return slot;
          }
        }
      }
    }

    /// <summary>
    /// Extend a slot to the left
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    async System.Threading.Tasks.Task<IMachineShiftSlot> ExtendLeftAsync (IMachineShiftSlot slot)
    {
      if (null == slot) {
        return null;
      }
      if (!slot.DateTimeRange.Lower.HasValue) {
        return slot;
      }
      else {
        var leftObservationStateSlot = await ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
          .FindWithEndAsync (slot.Machine, slot.DateTimeRange.Lower.Value);
        if (null == leftObservationStateSlot) {
          return slot;
        }
        else { // null != leftReasonSlot
          if (leftObservationStateSlot.IsEmpty ()) {
            Debug.Assert (false);
            log.ErrorFormat ("ExtendLeft: " +
                             "empty left observation state slot {0}",
                             leftObservationStateSlot);
            return slot;
          }
          IMachineShiftSlot leftMachineShiftSlot = new MachineShiftSlot (leftObservationStateSlot);
          if (IsMergeable (leftMachineShiftSlot, slot)) { // Extend it
            IMachineShiftSlot merged =
              Merge (leftMachineShiftSlot, slot);
            return await this.ExtendLeftAsync (merged);
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
    IMachineShiftSlot ExtendRight (IMachineShiftSlot slot)
    {
      if (null == slot) {
        return null;
      }
      if (!slot.DateTimeRange.Upper.HasValue) {
        return slot;
      }
      else {
        var rightObservationStateSlot = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
          .FindAt (slot.Machine, slot.DateTimeRange.Upper.Value);
        if (null == rightObservationStateSlot) {
          return slot;
        }
        else { // null != rightReasonSlot
          if (rightObservationStateSlot.IsEmpty ()) {
            Debug.Assert (false);
            log.ErrorFormat ("ExtendRight: " +
                             "empty right observation state slot slot {0}",
                             rightObservationStateSlot);
            return slot;
          }
          IMachineShiftSlot rightMachineShiftSlot = new MachineShiftSlot (rightObservationStateSlot);
          if (IsMergeable (rightMachineShiftSlot, slot)) { // Extend it
            IMachineShiftSlot merged = Merge (slot, rightMachineShiftSlot);
            return this.ExtendRight (merged);
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
    async System.Threading.Tasks.Task<IMachineShiftSlot> ExtendRightAsync (IMachineShiftSlot slot)
    {
      if (null == slot) {
        return null;
      }
      if (!slot.DateTimeRange.Upper.HasValue) {
        return slot;
      }
      else {
        var rightObservationStateSlot = await ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
          .FindAtAsync (slot.Machine, slot.DateTimeRange.Upper.Value);
        if (null == rightObservationStateSlot) {
          return slot;
        }
        else { // null != rightReasonSlot
          if (rightObservationStateSlot.IsEmpty ()) {
            Debug.Assert (false);
            log.ErrorFormat ("ExtendRightAsync: " +
                             "empty right observation state slot slot {0}",
                             rightObservationStateSlot);
            return slot;
          }
          IMachineShiftSlot rightMachineShiftSlot = new MachineShiftSlot (rightObservationStateSlot);
          if (IsMergeable (rightMachineShiftSlot, slot)) { // Extend it
            IMachineShiftSlot merged = Merge (slot, rightMachineShiftSlot);
            return await this.ExtendRightAsync (merged);
          }
          else {
            return slot;
          }
        }
      }
    }
  }
}
