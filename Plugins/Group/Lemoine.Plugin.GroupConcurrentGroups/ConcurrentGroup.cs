// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Group.Impl;
using Lemoine.Extensions.Business.Group;
using Lemoine.Extensions.Business;
using Lemoine.Extensions.Business.Operation;
using Lemoine.Model;

namespace Lemoine.Plugin.GroupConcurrentGroups
{
  /// <summary>
  /// ConcurrentGroup
  /// </summary>
  internal class ConcurrentGroup : GroupFromGroups
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConcurrentGroup).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="categoryReference"></param>
    /// <param name="groups"></param>
    /// <param name="sortPriority">group sort priority (optional)</param>
    public ConcurrentGroup (string id, string name, string categoryReference, IEnumerable<IGroup> groups, double? sortPriority, bool zoomInMachineSelection)
      : base (id, name, categoryReference, groups, sortPriority, zoomInMachineSelection)
    {
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public override Func<IPartProductionDataCurrentShift> PartProductionCurrentShift
    {
      get {
        if (this.SubGroups.Any (g => null != g.PartProductionCurrentShift)) {
          return () => GetPartProductionCurrentShift (this.SubGroups);
        }
        else {
          return null;
        }
      }
    }


    IPartProductionDataCurrentShift GetPartProductionCurrentShift (IEnumerable<IGroup> subGroups)
    {
      var items = subGroups
        .Where (g => null != g.PartProductionCurrentShift)
        .Select (g => g.PartProductionCurrentShift ());
      return new PartProductionDataCurrentShiftConcurrent (items);
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public override Func<UtcDateTimeRange, UtcDateTimeRange, IPartProductionDataRange> PartProductionRange
    {
      get {
        if (this.SubGroups.Any (g => null != g.PartProductionRange)) {
          return (r, p) => GetPartProductionRange (this.SubGroups, r, p);
        }
        else {
          return null;
        }
      }
    }


    IPartProductionDataRange GetPartProductionRange (IEnumerable<IGroup> subGroups, UtcDateTimeRange range, UtcDateTimeRange preLoadRange)
    {
      var items = subGroups
        .Where (g => null != g.PartProductionRange)
        .Select (g => g.PartProductionRange (range, preLoadRange));
      return new PartProductionDataRangeConcurrent (items);
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public override Func<IMachine, bool> IsMachineAggregatingParts => (m) => this.SubGroups.Any (g => (null != g.IsMachineAggregatingParts) && g.IsMachineAggregatingParts (m));
  }
}
