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
  /// using the reasonslots with a method to extend the slots catching the production state slots progressively
  /// </summary>
  public class ProductionStateSlotDAOExtendProgressively
    : ProductionStateSlotDAOExtend
    , IProductionStateSlotDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (ProductionStateSlotDAOExtendProgressively).FullName);

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
        var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .FindMatchingProductionState (slot.Machine,
                                        new UtcDateTimeRange (new LowerBound<DateTime> (null),
                                                              slot.DateTimeRange.Lower.Value),
                                        slot.ProductionState);
        IProductionStateSlot result = slot;
        foreach (var reasonSlot in reasonSlots.Reverse ()) {
          IProductionStateSlot newSlot = new ProductionStateSlot (reasonSlot);
          if (MergeableItem.IsMergeable (result, newSlot)) {
            result = MergeableItem
              .Merge<IProductionStateSlot> (newSlot, result);
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
    protected override IProductionStateSlot ExtendRight (IProductionStateSlot slot)
    {
      if (null == slot) {
        return null;
      }
      if (!slot.DateTimeRange.Upper.HasValue) {
        return slot;
      }
      else {
        var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .FindMatchingProductionState (slot.Machine,
                                        new UtcDateTimeRange (slot.DateTimeRange.Upper.Value),
                                        slot.ProductionState);
        IProductionStateSlot result = slot;
        foreach (var reasonSlot in reasonSlots) {
          IProductionStateSlot newSlot = new ProductionStateSlot (reasonSlot);
          if (MergeableItem.IsMergeable (result, newSlot)) {
            result = MergeableItem
              .Merge<IProductionStateSlot> (result, newSlot);
          }
          else {
            break;
          }
        }
        return result;
      }
    }
    #endregion // IProductionStateSlotDAO implementation
  }
}
