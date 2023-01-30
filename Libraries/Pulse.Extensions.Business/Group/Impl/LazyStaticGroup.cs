// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Operation;
using Lemoine.Model;

namespace Lemoine.Extensions.Business.Group.Impl
{
  /// <summary>
  /// static IGroup with a lazy fetch of the machines
  /// </summary>
  public class LazyStaticGroup : IGroup
  {
    readonly ILog log = LogManager.GetLogger (typeof (LazyStaticGroup).FullName);

    Func<IEnumerable<IMachine>> m_machinesGetter;
    IEnumerable<IMachine> m_machines = null;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="categoryReference"></param>
    /// <param name="machinesGetter"></param>
    /// <param name="sortPriority">group sort priority (optional)</param>
    /// <param name="sorted"></param>
    LazyStaticGroup (string id, string name, string categoryReference, Func<IEnumerable<IMachine>> machinesGetter, double? sortPriority, bool sorted)
    {
      this.Id = id;
      this.Name = name;
      this.CategoryReference = categoryReference;
      this.Dynamic = false;
      this.SortPriority = sortPriority;
      this.SortKind = sorted ? 1 : 0;
      m_machinesGetter = machinesGetter;
    }

    /// <summary>
    /// Create it
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="categoryReference"></param>
    /// <param name="machinesGetter"></param>
    /// <param name="sortPriority">group sort priority (optional)</param>
    /// <param name="sorted"></param>
    public static IGroup Create (string id, string name, string categoryReference, Func<IEnumerable<IMachine>> machinesGetter, double? sortPriority, bool sorted)
    {
      return new LazyStaticGroup (id, name, categoryReference, machinesGetter, sortPriority, sorted);
    }
    #endregion // Constructors

    #region IGroup implementation
    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public bool Dynamic
    {
      get; private set;
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public string Id
    {
      get; private set;
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public IEnumerable<int> MachineIds
    {
      get
      {
        return GetMachines ().Select (m => m.Id);
      }
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IMachine> GetMachines ()
    {
      if (null == m_machines) {
        m_machines = m_machinesGetter ();
      }
      return m_machines;
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    /// <param name="subMachines"></param>
    /// <returns></returns>
    public IEnumerable<IMachine> GetMachines (IEnumerable<IMachine> subMachines)
    {
      var machines = GetMachines ();
      return machines.Intersect (subMachines);
    }

    /// <summary>
    /// <see cref="IGroup"/>
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
    /// <see cref="IGroup"/>
    /// </summary>
    public int SortKind
    {
      get; private set;
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public double? SortPriority
    {
      get; private set;
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public bool ZoomInMachineSelection
    {
      get; set;
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
    /// Not implemented (return null)
    /// 
    /// <see cref="IGroup"/>
    /// </summary>
    public virtual Func<IPartProductionDataCurrentShift> PartProductionCurrentShift => null;

    /// <summary>
    /// Not implemented (return null)
    /// 
    /// <see cref="IGroup"/>
    /// </summary>
    public virtual Func<UtcDateTimeRange, UtcDateTimeRange, IPartProductionDataRange> PartProductionRange => null;

    /// <summary>
    /// Not implemented (return null)
    /// 
    /// <see cref="IGroup"/>
    /// </summary>
    public virtual Func<IMachine, bool> IsMachineAggregatingParts => null;
    #endregion // IGroup implementation

  }
}
