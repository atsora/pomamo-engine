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
using System.Threading.Tasks;

namespace Lemoine.Business.ProductionState
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IProductionStateSlotDAO">IProductionStateSlotDAO</see>
  /// using the observationstateslots
  /// </summary>
  public abstract class ProductionStateSlotDAOExtend
    : ProductionStateSlotDAO
    , IProductionStateSlotDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (ProductionStateSlotDAOExtend).FullName);

    #region IProductionStateSlotDAO implementation
    /// <summary>
    /// IProductionStateSlotDAO implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public override IProductionStateSlot FindAt (IMachine machine, DateTime dateTime)
    {
      Debug.Assert (null != machine);

      IProductionStateSlot slot = new ProductionStateSlot (
        ModelDAOHelper.DAOFactory.ReasonSlotDAO
        .FindAt (machine, dateTime));
      if (null != slot) {
        slot = ExtendLeft (slot);
        slot = ExtendRight (slot);
      }
      return slot;
    }

    /// <summary>
    /// IProductionStateSlotDAO implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public override async System.Threading.Tasks.Task<IProductionStateSlot> FindAtAsync (IMachine machine, DateTime dateTime)
    {
      Debug.Assert (null != machine);

      IProductionStateSlot slot = new ProductionStateSlot (
        await ModelDAOHelper.DAOFactory.ReasonSlotDAO
        .FindAtAsync (machine, dateTime));
      if (null != slot) {
        slot = ExtendLeft (slot);
        slot = ExtendRight (slot);
      }
      return slot;
    }

    /// <summary>
    /// IProductionStateSlotDAO implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public override IList<IProductionStateSlot> FindOverlapsRange (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
        .FindOverlapsRange (machine, range);
      var result = new List<IProductionStateSlot> ();
      foreach (var reasonSlot in reasonSlots) {
        IProductionStateSlot newSlot = new ProductionStateSlot (reasonSlot);
        if ((1 <= result.Count)
            && MergeableItem.IsMergeable (result[result.Count - 1], newSlot)) {
          result[result.Count - 1] = MergeableItem
            .Merge<IProductionStateSlot> (result[result.Count - 1], newSlot);
        }
        else {
          result.Add (newSlot);
        }
      }
      if (0 < result.Count) {
        result[0] = ExtendLeft (result[0]);
        result[result.Count - 1] = ExtendRight (result[result.Count - 1]);
      }
      return result;
    }

    /// <summary>
    /// IProductionStateSlotDAO implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public override async System.Threading.Tasks.Task<IList<IProductionStateSlot>> FindOverlapsRangeAsync (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      var reasonSlots = await ModelDAOHelper.DAOFactory.ReasonSlotDAO
        .FindOverlapsRangeAsync (machine, range);
      var result = new List<IProductionStateSlot> ();
      foreach (var reasonSlot in reasonSlots) {
        IProductionStateSlot newSlot = new ProductionStateSlot (reasonSlot);
        if ((1 <= result.Count)
            && MergeableItem.IsMergeable (result[result.Count - 1], newSlot)) {
          result[result.Count - 1] = MergeableItem
            .Merge<IProductionStateSlot> (result[result.Count - 1], newSlot);
        }
        else {
          result.Add (newSlot);
        }
      }
      if (0 < result.Count) {
        result[0] = ExtendLeft (result[0]);
        result[result.Count - 1] = ExtendRight (result[result.Count - 1]);
      }
      return result;
    }

    /// <summary>
    /// Find all the production state state slots (on different machines)
    /// at a specific date/time with the specified production state
    /// 
    /// They are order by ascending begin date/time
    /// </summary>
    /// <param name="productionState"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    public override IEnumerable<IProductionStateSlot> FindAt (IProductionState productionState, DateTime at)
    {
      Debug.Assert (DateTimeKind.Utc == at.Kind);

      var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
        .FindAt (productionState, at);
      IList<IProductionStateSlot> result = new List<IProductionStateSlot> ();
      foreach (var reasonSlot in reasonSlots) {
        IProductionStateSlot slot = new ProductionStateSlot (reasonSlot);
        if (null != slot) {
          slot = ExtendLeft (slot);
          slot = ExtendRight (slot);
        }
        result.Add (slot);
      }
      return result.OrderBy (slot => slot.DateTimeRange);
    }

    /// <summary>
    /// Extend a slot to the left
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    protected abstract IProductionStateSlot ExtendLeft (IProductionStateSlot slot);

    /// <summary>
    /// Extend a slot to the right
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    protected abstract IProductionStateSlot ExtendRight (IProductionStateSlot slot);

    /// <summary>
    /// Find all the machine state template slots that match a machine state template in a specified range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="productionState"></param>
    /// <returns></returns>
    public override IEnumerable<IProductionStateSlot> FindOverlapsRangeMatchingProductionState (IMachine machine,
                                                                                                UtcDateTimeRange range,
                                                                                                IProductionState productionState)
    {
      Debug.Assert (null != machine);

      var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
        .FindOverlapsRangeMatchingProductionState (machine, range, productionState);
      IList<IProductionStateSlot> result = new List<IProductionStateSlot> ();
      foreach (var observationStateSlot in reasonSlots) {
        IProductionStateSlot newSlot = new ProductionStateSlot (observationStateSlot);
        if ((1 <= result.Count)
            && MergeableItem.IsMergeable (result[result.Count - 1], newSlot)) {
          result[result.Count - 1] = MergeableItem
            .Merge<IProductionStateSlot> (result[result.Count - 1], newSlot);
        }
        else {
          result.Add (newSlot);
        }
      }
      if (0 < result.Count) {
        result[0] = ExtendLeft (result[0]);
        result[result.Count - 1] = ExtendRight (result[result.Count - 1]);
      }
      return result;
    }
    #endregion // IProductionStateSlotDAO implementation
  }
}
