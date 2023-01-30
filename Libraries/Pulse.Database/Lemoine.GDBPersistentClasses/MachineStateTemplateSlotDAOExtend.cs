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
  /// using the observationstateslots
  /// </summary>
  public abstract class MachineStateTemplateSlotDAOExtend
    : MachineStateTemplateSlotDAO
    , IMachineStateTemplateSlotDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (MachineStateTemplateSlotDAOExtend).FullName);
    
    #region IMachineStateTemplateSlotDAO implementation
    /// <summary>
    /// IMachineStateTemplateSlotDAO implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public override IMachineStateTemplateSlot FindAt(IMachine machine, DateTime dateTime)
    {
      Debug.Assert (null != machine);
      
      IMachineStateTemplateSlot slot = new MachineStateTemplateSlot (
        ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
        .FindAt (machine, dateTime));
      if (null != slot) {
        slot = ExtendLeft (slot);
        slot = ExtendRight (slot);
      }
      return slot;
    }
    
    /// <summary>
    /// IMachineStateTemplateSlotDAO implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public override IList<IMachineStateTemplateSlot> FindOverlapsRange(IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);
      
      IList<IObservationStateSlot> observationStateSlots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
        .FindOverlapsRange (machine, range);
      IList<IMachineStateTemplateSlot> result = new List<IMachineStateTemplateSlot> ();
      foreach (var observationStateSlot in observationStateSlots) {
        IMachineStateTemplateSlot newSlot = new MachineStateTemplateSlot (observationStateSlot);
        if ( (1 <= result.Count)
            && MergeableItem.IsMergeable (result [result.Count - 1], newSlot)) {
          result [result.Count - 1] = MergeableItem
            .Merge<IMachineStateTemplateSlot> (result [result.Count - 1], newSlot);
        }
        else {
          result.Add (newSlot);
        }
      }
      if (0 < result.Count) {
        result [0] = ExtendLeft (result [0]);
        result [result.Count - 1] = ExtendRight (result [result.Count - 1]);
      }
      return result;
    }

    /// <summary>
    /// Find all the observation state slots (on different machines)
    /// at a specific date/time with the specified machine state template
    /// 
    /// They are order by ascending begin date/time
    /// </summary>
    /// <param name="machineStateTemplate"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    public override IEnumerable<IMachineStateTemplateSlot> FindAt (IMachineStateTemplate machineStateTemplate, DateTime at)
    {
      Debug.Assert (DateTimeKind.Utc == at.Kind);
      
      IList<IObservationStateSlot> observationStateSlots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
        .FindAt (machineStateTemplate, at);
      IList<IMachineStateTemplateSlot> result = new List<IMachineStateTemplateSlot> ();
      foreach (var observationStateSlot in observationStateSlots) {
        IMachineStateTemplateSlot slot = new MachineStateTemplateSlot (observationStateSlot);
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
    protected abstract IMachineStateTemplateSlot ExtendLeft (IMachineStateTemplateSlot slot);

    /// <summary>
    /// Extend a slot to the right
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    protected abstract IMachineStateTemplateSlot ExtendRight (IMachineStateTemplateSlot slot);
    
    /// <summary>
    /// Find all the machine state template slots that match a machine state template in a specified range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="machineStateTemplate"></param>
    /// <returns></returns>
    public override IEnumerable<IMachineStateTemplateSlot> FindOverlapsRangeMatchingMachineStateTemplate (IMachine machine,
                                                                                                          UtcDateTimeRange range,
                                                                                                          IMachineStateTemplate machineStateTemplate)
    {
      Debug.Assert (null != machine);
      
      IEnumerable<IObservationStateSlot> observationStateSlots = (new ObservationStateSlotDAO ())
        .FindOverlapsRangeMatchingMachineStateTemplate (machine, range, machineStateTemplate);
      IList<IMachineStateTemplateSlot> result = new List<IMachineStateTemplateSlot> ();
      foreach (var observationStateSlot in observationStateSlots) {
        IMachineStateTemplateSlot newSlot = new MachineStateTemplateSlot (observationStateSlot);
        if ( (1 <= result.Count)
            && MergeableItem.IsMergeable (result [result.Count - 1], newSlot)) {
          result [result.Count - 1] = MergeableItem
            .Merge<IMachineStateTemplateSlot> (result [result.Count - 1], newSlot);
        }
        else {
          result.Add (newSlot);
        }
      }
      if (0 < result.Count) {
        result [0] = ExtendLeft (result [0]);
        result [result.Count - 1] = ExtendRight (result [result.Count - 1]);
      }
      return result;
    }
    #endregion // IMachineStateTemplateSlotDAO implementation
  }
}
