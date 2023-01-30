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
  /// using the observationstateslots with a method to extend the slots catching observation state slots with a different machine state template
  /// </summary>
  public class MachineStateTemplateSlotDAOExtendDifferent
    : MachineStateTemplateSlotDAOExtend
    , IMachineStateTemplateSlotDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (MachineStateTemplateSlotDAOExtendDifferent).FullName);
    
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
        UtcDateTimeRange newDateTimeRange = slot.DateTimeRange;
        IObservationStateSlot left = (new ObservationStateSlotDAO ())
          .FindFirstStrictlyLeftDifferentMachineStateTemplate (slot.Machine,
                                                               slot.MachineStateTemplate,
                                                               slot.DateTimeRange);
        if (null == left) { // No different machine state template on the left
          newDateTimeRange = new UtcDateTimeRange (new LowerBound<DateTime> (null),
                                                   newDateTimeRange.Upper,
                                                   false,
                                                   newDateTimeRange.UpperInclusive);
        }
        else { // null != left
          Debug.Assert (left.DateTimeRange.Upper.HasValue);
          newDateTimeRange = new UtcDateTimeRange (left.DateTimeRange.Upper.Value,
                                                   newDateTimeRange.Upper,
                                                   !left.DateTimeRange.UpperInclusive,
                                                   newDateTimeRange.UpperInclusive);
        }
        return slot.Clone (newDateTimeRange, ModelDAOHelper.DAOFactory.DaySlotDAO.ConvertToDayRange (newDateTimeRange));
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
        UtcDateTimeRange newDateTimeRange = slot.DateTimeRange;
        IObservationStateSlot right = (new ObservationStateSlotDAO ())
          .FindFirstStrictlyRightDifferentMachineStateTemplate (slot.Machine,
                                                                slot.MachineStateTemplate,
                                                                slot.DateTimeRange);
        if (null == right) { // No different machine state template on the right
          newDateTimeRange = new UtcDateTimeRange (newDateTimeRange.Lower,
                                                   new UpperBound<DateTime> (null),
                                                   newDateTimeRange.LowerInclusive,
                                                   false);
        }
        else { // null != right
          Debug.Assert (right.DateTimeRange.Lower.HasValue);
          newDateTimeRange = new UtcDateTimeRange (newDateTimeRange.Lower,
                                                   right.DateTimeRange.Lower.Value,
                                                   newDateTimeRange.LowerInclusive,
                                                   !right.DateTimeRange.LowerInclusive);
        }
        return slot.Clone (newDateTimeRange, ModelDAOHelper.DAOFactory.DaySlotDAO.ConvertToDayRange (newDateTimeRange));
      }
    }
    #endregion // IMachineStateTemplateSlotDAO implementation
  }
}
