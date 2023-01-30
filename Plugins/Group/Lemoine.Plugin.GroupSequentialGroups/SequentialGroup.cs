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
using Lemoine.Model;
using Lemoine.Extensions.Business.Operation;

namespace Lemoine.Plugin.GroupSequentialGroups
{
  /// <summary>
  /// SequentialGroup
  /// </summary>
  internal class SequentialGroup: GroupFromGroups
  {
    readonly ILog log = LogManager.GetLogger (typeof (SequentialGroup).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="categoryReference"></param>
    /// <param name="groups"></param>
    /// <param name="sortPriority">group sort priority (optional)</param>
    /// <param name="zoomInMachineSelection"></param>
    public SequentialGroup (string id, string name, string categoryReference, IEnumerable<IGroup> groups, double? sortPriority, bool zoomInMachineSelection)
      : base (id, name, categoryReference, groups, sortPriority, zoomInMachineSelection)
    {
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public override Func<IPartProductionDataCurrentShift> PartProductionCurrentShift
    {
      get
      {
        var lastSubGroup = this.SubGroups.LastOrDefault ();
        if ((null != lastSubGroup) && (null != lastSubGroup.PartProductionCurrentShift)) {
          return lastSubGroup.PartProductionCurrentShift;
        }
        else {
          return null;
        }
      }
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public override Func<UtcDateTimeRange, UtcDateTimeRange, IPartProductionDataRange> PartProductionRange
    {
      get
      {
        var lastSubGroup = this.SubGroups.LastOrDefault ();
        if ((null != lastSubGroup) && (null != lastSubGroup.PartProductionRange)) {
          return lastSubGroup.PartProductionRange;
        }
        else {
          return null;
        }
      }
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public override Func<IMachine, bool> IsMachineAggregatingParts => this.SubGroups.LastOrDefault ()?.IsMachineAggregatingParts;
  }
}
