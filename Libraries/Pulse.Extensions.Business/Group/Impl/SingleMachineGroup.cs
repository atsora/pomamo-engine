// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Business.Operation;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Lemoine.Extensions.Business.Group.Impl
{
  /// <summary>
  /// Create the specific single machine group
  /// </summary>
  public sealed class SingleMachineGroup : IGroup
  {
    readonly IMachine m_machine;
    readonly string m_id;
    readonly Func<IPartProductionDataCurrentShift> m_getPartProductionDataCurrentShift;
    readonly Func<UtcDateTimeRange, UtcDateTimeRange, Func<IEnumerable<IObservationStateSlot>>, IPartProductionDataRange> m_getPartProductionDataRange;

    (UtcDateTimeRange range, IEnumerable<IObservationStateSlot> slots) m_preLoadObservationStateSlots = (null, null);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="getPartProductionDataCurrentShift">not null</param>
    /// <param name="getPartProductionDataRange">not null</param>
    public SingleMachineGroup (IMachine machine, Func<IMachine, IPartProductionDataCurrentShift> getPartProductionDataCurrentShift, Func<IMachine, UtcDateTimeRange, UtcDateTimeRange, Func<IEnumerable<IObservationStateSlot>>, IPartProductionDataRange> getPartProductionDataRange)
      : this (machine, getPartProductionDataCurrentShift, getPartProductionDataRange, (x) => x.ToString ())
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="getPartProductionDataCurrentShift">not null</param>
    /// <param name="getPartProductionDataRange">not null</param>
    /// <param name="buildId"></param>
    public SingleMachineGroup (IMachine machine, Func<IMachine, IPartProductionDataCurrentShift> getPartProductionDataCurrentShift, Func<IMachine, UtcDateTimeRange, UtcDateTimeRange, Func<IEnumerable<IObservationStateSlot>>, IPartProductionDataRange> getPartProductionDataRange, Func<int, string> buildId)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != getPartProductionDataCurrentShift);
      Debug.Assert (null != getPartProductionDataRange);

      m_machine = machine;
      m_id = buildId (machine.Id);
      m_getPartProductionDataCurrentShift = () => getPartProductionDataCurrentShift (machine);
      m_getPartProductionDataRange = (p, r, s) => getPartProductionDataRange (machine, p, r, s);
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Business.Group.IGroup"/>
    /// </summary>
    public bool Dynamic
    {
      get { return false; }
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Business.Group.IGroup"/>
    /// </summary>
    public string Id
    {
      get { return m_id; }
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Business.Group.IGroup"/>
    /// </summary>
    public IEnumerable<int> MachineIds
    {
      get { return new List<int> { m_machine.Id }; }
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IMachine> GetMachines ()
    {
      return new List<IMachine> { m_machine };
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    /// <param name="machines"></param>
    /// <returns></returns>
    public IEnumerable<IMachine> GetMachines (IEnumerable<IMachine> machines)
    {
      if (machines.Contains (m_machine)) {
        return new List<IMachine> { m_machine };
      }
      else {
        return new List<IMachine> ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Business.Group.IGroup"/>
    /// </summary>
    public string Name
    {
      get { return m_machine.Display; }
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public string TreeName
    {
      get { return this.Name; }
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public string CategoryReference
    {
      get { return "SingleMachine"; }
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public bool SingleMachine
    {
      get { return true; }
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Business.Group.IGroup"/>
    /// </summary>
    public double? SortPriority
    {
      get { return m_machine.DisplayPriority; }
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Business.Group.IGroup"/>
    /// </summary>
    public int SortKind
    {
      get { return 1; }
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public bool ZoomInMachineSelection
    {
      get { return false; }
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    /// <param name="machineMonitoringTypeId"></param>
    /// <returns></returns>
    public bool? IncludeSpecificMonitoringType (MachineMonitoringTypeId machineMonitoringTypeId)
    {
      return m_machine.MonitoringType.Id == (int)machineMonitoringTypeId;
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public Func<IPartProductionDataCurrentShift> PartProductionCurrentShift => m_getPartProductionDataCurrentShift;


    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public Func<UtcDateTimeRange, UtcDateTimeRange, IPartProductionDataRange> PartProductionRange =>  (r, p) => GetPartProductionDataRange (r, p);

    /// <summary>
    /// By default: single machine so it aggregates parts
    /// 
    /// <see cref="IGroup"/>
    /// </summary>
    /// <returns></returns>
    public Func<IMachine, bool> IsMachineAggregatingParts => (m) => true;

    IPartProductionDataRange GetPartProductionDataRange (UtcDateTimeRange range, UtcDateTimeRange preLoadRange)
    {
      return m_getPartProductionDataRange (range, preLoadRange, () => GetObservationStateSlots (range, preLoadRange));
    }

    IEnumerable<IObservationStateSlot> GetObservationStateSlots (UtcDateTimeRange range, UtcDateTimeRange preLoadRange)
    {
      if ((null != preLoadRange)
        && (null != m_preLoadObservationStateSlots.range) && (null != m_preLoadObservationStateSlots.slots)
        && !m_preLoadObservationStateSlots.range.ContainsRange (range)) {
        // Pre-load another set of observation state slots
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          m_preLoadObservationStateSlots = (preLoadRange, ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindOverlapsRange (m_machine, preLoadRange));
        }
      }

      if ((null != m_preLoadObservationStateSlots.range)
        && (m_preLoadObservationStateSlots.range.ContainsRange (range))) {
        return m_preLoadObservationStateSlots.slots
          .Where (s => s.DateTimeRange.Overlaps (range));
      }
      else {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          return ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindOverlapsRange (m_machine, range);
        }
      }
    }
  }
}
