// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate.Criterion;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineStateTemplateSlotDAO">IMachineStateTemplateSlotDAO</see>
  /// using the observationstateslots with a method to extend the slots catching the observation state slots progressively
  /// </summary>
  public class MachineStateTemplateSlotDAOExtendProgressively
    : MachineStateTemplateSlotDAOExtend
    , IMachineStateTemplateSlotDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (MachineStateTemplateSlotDAOExtendProgressively).FullName);
    
    #region IMachineStateTemplateSlotDAO implementation
    /// <summary>
    /// Extend a slot to the left
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    protected override IMachineStateTemplateSlot ExtendLeft (IMachineStateTemplateSlot slot)
    {
      if (null == slot) {
        return null;
      }
      if (!slot.DateTimeRange.Lower.HasValue) {
        return slot;
      }
      else {
        IList<IObservationStateSlot> observationStateSlots = (new ObservationStateSlotDAO ())
          .FindMatchingMachineStateTemplate (slot.Machine,
                                             new UtcDateTimeRange (new LowerBound<DateTime> (null),
                                                                   slot.DateTimeRange.Lower.Value),
                                             slot.MachineStateTemplate);
        IMachineStateTemplateSlot result = slot;
        foreach (var observationStateSlot in observationStateSlots.Reverse ()) {
          IMachineStateTemplateSlot newSlot = new MachineStateTemplateSlot (observationStateSlot);
          if (MergeableItem.IsMergeable (result, newSlot)) {
            result = MergeableItem
              .Merge<IMachineStateTemplateSlot> (newSlot, result);
          }
          else {
            break;
          }
        }
        return result;
      }
    }

    /// <summary>
    /// Extend a slot to the right
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    protected override IMachineStateTemplateSlot ExtendRight (IMachineStateTemplateSlot slot)
    {
      if (null == slot) {
        return null;
      }
      if (!slot.DateTimeRange.Upper.HasValue) {
        return slot;
      }
      else {
        IList<IObservationStateSlot> observationStateSlots = (new ObservationStateSlotDAO ())
          .FindMatchingMachineStateTemplate (slot.Machine,
                                             new UtcDateTimeRange (slot.DateTimeRange.Upper.Value),
                                             slot.MachineStateTemplate);
        IMachineStateTemplateSlot result = slot;
        foreach (var observationStateSlot in observationStateSlots) {
          IMachineStateTemplateSlot newSlot = new MachineStateTemplateSlot (observationStateSlot);
          if (MergeableItem.IsMergeable (result, newSlot)) {
            result = MergeableItem
              .Merge<IMachineStateTemplateSlot> (result, newSlot);
          }
          else {
            break;
          }
        }
        return result;
      }
    }
    #endregion // IMachineStateTemplateSlotDAO implementation
  }
}
