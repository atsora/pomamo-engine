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
using Lemoine.Core.Log;

namespace Lemoine.Business.ProductionState
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IProductionStateSlotDAO">IProductionStateSlotDAO</see>
  /// using the reasonslots with a method to extend the slots catching production state slots with a different production state slots
  /// </summary>
  public class ProductionStateSlotDAOExtendDifferent
    : ProductionStateSlotDAOExtend
    , IProductionStateSlotDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (ProductionStateSlotDAOExtendDifferent).FullName);

    #region IProductionStateSlotDAO implementation
    /// <summary>
    /// Extend a slot to the left
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    protected override IProductionStateSlot ExtendLeft (IProductionStateSlot slot)
    {
      if (null == slot) {
        return null;
      }
      if (!slot.DateTimeRange.Lower.HasValue) {
        return slot;
      }
      else {
        UtcDateTimeRange newDateTimeRange = slot.DateTimeRange;
        var left = ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .FindFirstStrictlyLeftDifferentProductionState (slot.Machine,
                                                          slot.ProductionState,
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
        return slot.Clone (newDateTimeRange, ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (newDateTimeRange)));
      }
    }

    /// <summary>
    /// Extend a slot to the right
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    protected override IProductionStateSlot ExtendRight (IProductionStateSlot slot)
    {
      if (null == slot) {
        return null;
      }
      if (!slot.DateTimeRange.Upper.HasValue) {
        return slot;
      }
      else {
        UtcDateTimeRange newDateTimeRange = slot.DateTimeRange;
        var right = ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .FindFirstStrictlyRightDifferentProductionState (slot.Machine,
                                                           slot.ProductionState,
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
        return slot.Clone (newDateTimeRange, ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (newDateTimeRange)));
      }
    }
    #endregion // IProductionStateSlotDAO implementation
  }
}

