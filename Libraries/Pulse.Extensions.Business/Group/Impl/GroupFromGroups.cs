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
  /// Create a group from a list of groups and some other properties.
  /// 
  /// Note the machines in this group are not really sorted.
  /// SortKind is set to 1 if all the sort kind of all the groups is at least 1.
  /// </summary>
  public class GroupFromGroups
    : IGroup, IZoomableGroup
  {
    IEnumerable<IGroup> m_groups;
    bool? m_dynamic;
    int? m_sortKind;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="categoryReference"></param>
    /// <param name="groups"></param>
    /// <param name="sortPriority">group sort priority (optional)</param>
    /// <param name="zoomInMachineSelection"></param>
    public GroupFromGroups (string id, string name, string categoryReference, IEnumerable<IGroup> groups, double? sortPriority, bool zoomInMachineSelection)
    {
      m_groups = groups;
      this.Id = id;
      this.Name = name;
      this.CategoryReference = categoryReference;
      this.SortPriority = sortPriority;
      this.ZoomInMachineSelection = zoomInMachineSelection;
    }

    /// <summary>
    /// <see cref="IZoomableGroup"/>
    /// </summary>
    public IEnumerable<IGroup> SubGroups
    {
      get { return m_groups; }
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Business.Group.IGroup"/>
    /// </summary>
    public bool Dynamic
    {
      get
      {
        if (!m_dynamic.HasValue) {
          m_dynamic = m_groups.Any (g => g.Dynamic);
        }
        return m_dynamic.Value;
      }
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
    [Obsolete]
    public IEnumerable<int> MachineIds
    {
      get { return m_groups
          .SelectMany (g => g.MachineIds)
          .Distinct (); }
    }
    
    /// <summary>
    /// Get the associated list of machines
    /// </summary>
    /// <returns></returns>

    public IEnumerable<IMachine> GetMachines ()
    {
      var equalityComparer =
        new LambdaEqualityComparer<IMachine> ( (a, b) => a.Id == b.Id, a => a.Id);
      return m_groups
        .SelectMany (g => g.GetMachines ())
        .Distinct (equalityComparer);
    }

    /// <summary>
    /// Get a sub-set of associated machines
    /// </summary>
    /// <param name="machines"></param>
    /// <returns></returns>
    public IEnumerable<IMachine> GetMachines (IEnumerable<IMachine> machines)
    {
      var equalityComparer =
        new LambdaEqualityComparer<IMachine> ((a, b) => a.Id == b.Id, a => a.Id);
      return m_groups
        .SelectMany (g => g.GetMachines (machines)
        .Distinct (equalityComparer));
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
      get
      {
        if (!m_sortKind.HasValue) {
          m_sortKind = m_groups.Any (g => 0 == g.SortKind) ? 0 : 1;
        }
        return m_sortKind.Value;
      }
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
      var subValues = this.SubGroups
        .Select (g => g.IncludeSpecificMonitoringType (machineMonitoringTypeId))
        .ToList ();
      if (subValues.Any (x => x.HasValue && x.Value)) {
        return true;
      }
      else if (subValues.All (x => x.HasValue && !x.Value)) {
        return false;
      }
      else {
        return null;
      }
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
  }
}
