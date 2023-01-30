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
  /// 
  /// Note: there are different ways to make requests
  /// <item>using the observationstateslots</item>
  /// <item>using the machinestatetemplateslot view</item>
  /// </summary>
  public abstract class ProductionStateSlotDAO
    : IProductionStateSlotDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (ProductionStateSlotDAO).FullName);

    #region IProductionStateSlotDAO implementation
    /// <summary>
    /// IProductionStateSlotDAO implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public virtual IList<IProductionStateSlot> FindAll (IMachine machine)
    {
      var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
        .FindAll (machine);

      IList<IProductionStateSlot> result = new List<IProductionStateSlot> ();
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

      return result;
    }

    /// <summary>
    /// IProductionStateSlotDAO implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public abstract IProductionStateSlot FindAt (IMachine machine, DateTime dateTime);

    /// <summary>
    /// IProductionStateSlotDAO implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public abstract System.Threading.Tasks.Task<IProductionStateSlot> FindAtAsync (IMachine machine, DateTime dateTime);

    /// <summary>
    /// IProductionStateSlotDAO implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public abstract IList<IProductionStateSlot> FindOverlapsRange (IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// IProductionStateSlotDAO implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public abstract System.Threading.Tasks.Task<IList<IProductionStateSlot>> FindOverlapsRangeAsync (IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Find all the observation state slots (on different machines)
    /// at a specific date/time with the specified machine state template
    /// 
    /// They are order by ascending begin date/time
    /// </summary>
    /// <param name="machineStateTemplate"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    public abstract IEnumerable<IProductionStateSlot> FindAt (IProductionState machineStateTemplate, DateTime at);

    /// <summary>
    /// Find all the machine state template slots that match a machine state template in a specified range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="machineStateTemplate"></param>
    /// <returns></returns>
    public abstract IEnumerable<IProductionStateSlot> FindOverlapsRangeMatchingProductionState (IMachine machine,
                                                                                                 UtcDateTimeRange range,
                                                                                                 IProductionState machineStateTemplate);
    #endregion // IProductionStateSlotDAO implementation
  }
}
