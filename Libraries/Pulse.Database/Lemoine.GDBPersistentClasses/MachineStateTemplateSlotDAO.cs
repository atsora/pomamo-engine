// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Info;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate.Criterion;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineStateTemplateSlotDAO">IMachineStateTemplateSlotDAO</see>
  /// 
  /// Note: there are different ways to make requests
  /// <item>using the observationstateslots</item>
  /// <item>using the machinestatetemplateslot view</item>
  /// </summary>
  public abstract class MachineStateTemplateSlotDAO
    : ReadOnlyNHibernateDAO<MachineStateTemplateSlot, IMachineStateTemplateSlot, int>
    , IMachineStateTemplateSlotDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (MachineStateTemplateSlotDAO).FullName);
    
    #region IMachineStateTemplateSlotDAO implementation
    /// <summary>
    /// IMachineStateTemplateSlotDAO implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public virtual IList<IMachineStateTemplateSlot> FindAll(IMachine machine)
    {
      IList<IObservationStateSlot> observationStateSlots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
        .FindAll (machine);
      
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
      
      return result;
    }
    
    /// <summary>
    /// IMachineStateTemplateSlotDAO implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public abstract IMachineStateTemplateSlot FindAt(IMachine machine, DateTime dateTime);
    
    /// <summary>
    /// IMachineStateTemplateSlotDAO implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public abstract IList<IMachineStateTemplateSlot> FindOverlapsRange(IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Find all the observation state slots (on different machines)
    /// at a specific date/time with the specified machine state template
    /// 
    /// They are order by ascending begin date/time
    /// </summary>
    /// <param name="machineStateTemplate"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    public abstract IEnumerable<IMachineStateTemplateSlot> FindAt (IMachineStateTemplate machineStateTemplate, DateTime at);
    
    /// <summary>
    /// Find all the machine state template slots that match a machine state template in a specified range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="machineStateTemplate"></param>
    /// <returns></returns>
    public abstract IEnumerable<IMachineStateTemplateSlot> FindOverlapsRangeMatchingMachineStateTemplate (IMachine machine,
                                                                                                 UtcDateTimeRange range,
                                                                                                 IMachineStateTemplate machineStateTemplate);
    #endregion // IMachineStateTemplateSlotDAO implementation
  }
}
