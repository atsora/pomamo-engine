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
using Lemoine.ModelDAO;

namespace Lemoine.Extensions.Business.Group.Impl
{
  /// <summary>
  /// Basic implementation for a dynamic group
  /// </summary>
  public class DynamicGroup: IGroup
  {
    readonly ILog log = LogManager.GetLogger (typeof (DynamicGroup).FullName);

    Func<IEnumerable<IMachine>> m_machinesGetter1;
    Func<IEnumerable<IMachine>, IEnumerable<IMachine>> m_machinesGetter2;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="categoryReference"></param>
    /// <param name="machinesGetter1"></param>
    /// <param name="machinesGetter2"></param>
    /// <param name="sortPriority">group sort priority (optional)</param>
    /// <param name="sorted"></param>
    DynamicGroup (string id, string name, string categoryReference, Func<IEnumerable<IMachine>> machinesGetter1, Func<IEnumerable<IMachine>, IEnumerable<IMachine>> machinesGetter2, double? sortPriority, bool sorted, bool zoomInMachineSelection = false)
    {
      this.Id = id;
      this.Name = name;
      this.CategoryReference = categoryReference;
      this.Dynamic = true;
      this.SortPriority = sortPriority;
      this.SortKind = sorted ? 1 : 0;
      m_machinesGetter1 = machinesGetter1;
      m_machinesGetter2 = machinesGetter2;
      this.ZoomInMachineSelection = zoomInMachineSelection;
    }

    /// <summary>
    /// Create it
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="categoryReference"></param>
    /// <param name="machinesGetter1"></param>
    /// <param name="machinesGetter2"></param>
    /// <param name="sortPriority">group sort priority (optional)</param>
    /// <param name="sorted"></param>
    public static IGroup Create (string id, string name, string categoryReference, Func<IEnumerable<IMachine>> machinesGetter1, Func<IEnumerable<IMachine>, IEnumerable<IMachine>> machinesGetter2, double? sortPriority, bool sorted, bool zoomInMachineSelection = false)
    {
      return new DynamicGroup (id, name, categoryReference, machinesGetter1, machinesGetter2, sortPriority, sorted, zoomInMachineSelection: zoomInMachineSelection);
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
      return m_machinesGetter1 ();
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    /// <param name="subSetOfMachines"></param>
    /// <returns></returns>
    public IEnumerable<IMachine> GetMachines (IEnumerable<IMachine> subSetOfMachines)
    {
      return m_machinesGetter2 (subSetOfMachines);
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
    public bool ZoomInMachineSelection { get; }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    /// <param name="machineMonitoringTypeId"></param>
    /// <returns></returns>
    public bool? IncludeSpecificMonitoringType (MachineMonitoringTypeId machineMonitoringTypeId)
    {
      return null;
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
    /// Basic implementation: if group is SingleMachine, this machine is producing parts,
    /// else not implemented (return null)
    /// 
    /// <see cref="IGroup"/>
    /// </summary>
    public virtual Func<IMachine, bool> IsMachineAggregatingParts
    {
      get {
        if (this.SingleMachine) {
          return (m) => true;
        }
        else {
          return null;
        }
      }
    }
    #endregion // IGroup implementation

  }
}
