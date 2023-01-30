// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Business.Operation;
using Lemoine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Extensions.Business.Group.Impl
{
  /// <summary>
  /// Create a group from a list of machines and some other properties
  /// </summary>
  public sealed class GroupFromMachineList : IGroup
  {
    IEnumerable<IMachine> m_machines;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="categoryReference"></param>
    /// <param name="machines"></param>
    /// <param name="sortPriority">group sort priority (optional)</param>
    /// <param name="sorted"></param>
    /// <param name="zoomInMachineSelection"></param>
    /// <param name="aggregatingPartsMachines">optional</param>
    /// <param name="partProductionCurrentShift">optional</param>
    /// <param name="partProductionRange">optional</param>
    public GroupFromMachineList (string id, string name, string categoryReference, IEnumerable<IMachine> machines, double? sortPriority, bool sorted, bool zoomInMachineSelection, IEnumerable<IMachine> aggregatingPartsMachines = null, Func<IEnumerable<IMachine>, IPartProductionDataCurrentShift> partProductionCurrentShift = null, Func<IEnumerable<IMachine>, UtcDateTimeRange, UtcDateTimeRange, IPartProductionDataRange> partProductionRange = null)
    {
      this.Id = id;
      this.Name = name;
      this.CategoryReference = categoryReference;
      this.Dynamic = false;
      this.SortPriority = sortPriority;
      this.SortKind = sorted ? 1 : 0;
      this.ZoomInMachineSelection = zoomInMachineSelection;
      m_machines = machines.ToList (); // Not to make it lazy
      if (null != aggregatingPartsMachines) {
        this.IsMachineAggregatingParts = (m) => aggregatingPartsMachines.Contains (m);
      }
      if (null != partProductionCurrentShift) {
        this.PartProductionCurrentShift = () => partProductionCurrentShift (machines);
      }
      if (null != partProductionRange) {
        this.PartProductionRange = (r, p) => partProductionRange (machines, r, p);
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Business.Group.IGroup"/>
    /// </summary>
    public bool Dynamic
    {
      get; set;
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Business.Group.IGroup"/>
    /// </summary>
    public string Id
    {
      get; private set;
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Business.Group.IGroup"/>
    /// </summary>
    public IEnumerable<int> MachineIds
    {
      get { return m_machines.Select (m => m.Id); }
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    /// <returns></returns>

    public IEnumerable<IMachine> GetMachines ()
    {
      return m_machines;
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    /// <param name="machines"></param>
    /// <returns></returns>
    public IEnumerable<IMachine> GetMachines (IEnumerable<IMachine> machines)
    {
      return m_machines.Intersect (machines);
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Business.Group.IGroup"/>
    /// </summary>
    public string Name
    {
      get; private set;
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
      get; private set;
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public bool SingleMachine
    {
      get { return false; }
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Business.Group.IGroup"/>
    /// </summary>
    public double? SortPriority
    {
      get; private set;
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Business.Group.IGroup"/>
    /// </summary>
    public int SortKind
    {
      get; private set;
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public bool ZoomInMachineSelection
    {
      get; private set;
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    /// <param name="machineMonitoringTypeId"></param>
    /// <returns></returns>
    public bool? IncludeSpecificMonitoringType (MachineMonitoringTypeId machineMonitoringTypeId)
    {
      var machines = GetMachines ();
      return machines.Any (m => m.MonitoringType.Id == (int)machineMonitoringTypeId);
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public Func<IPartProductionDataCurrentShift> PartProductionCurrentShift
    {
      get; set;
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public Func<UtcDateTimeRange, UtcDateTimeRange, IPartProductionDataRange> PartProductionRange
    {
      get; set;
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public Func<IMachine, bool> IsMachineAggregatingParts
    {
      get; set;
    }
  }
}
