// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Model;
using Lemoine.Extensions.Business.Group;
using System.Diagnostics;
using Pulse.Extensions.Web.Responses;

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// Response DTO for a IGroup
  /// </summary>
  [Api ("Group Response DTO")]
  public class GroupDTO
  {
    ILog log = LogManager.GetLogger (typeof (GroupDTO).FullName);

    /// <summary>
    /// Id of the group
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display of the group / Name
    /// </summary>
    public string Display { get; set; }

    /// <summary>
    /// Tree name of the group
    /// (to be used when a tree or a path to the group is used)
    /// </summary>
    public string TreeName { get; set; }

    /// <summary>
    /// true if this group corresponds to a special single machine group
    /// </summary>
    public bool SingleMachine { get; set; }

    /// <summary>
    /// Machine ID in case the group is a single machine group
    /// </summary>
    public int? MachineId { get; set; }

    /// <summary>
    /// Is the group dynamic ?
    /// </summary>
    public bool Dynamic { get; set; }

    /// <summary>
    /// Sort priority of the group
    /// 
    /// If not available, 0 is returned
    /// </summary>
    public double SortPriority { get; set; }

    /// <summary>
    /// Sort kind when different groups need to be combined:
    /// <item>if the machines are not sorted in a particular way, return 0</item>
    /// <item>if the sort importance is minor, return 1. In that case, the machines can be re-organized</item>
    /// <item>if the group was designed to return sorted machines, return a number greater than 2</item>
    /// 
    /// Prevents the machines from being re-organized manually if the sort priority is greater than 2
    /// </summary>
    public int SortKind { get; set; }

    /// <summary>
    /// Tip on the sort kind:
    /// <item>sorted</item>
    /// <item>minor</item>
    /// <item>unsorted</item>
    /// </summary>
    public string SortKindTip { get; set; }

    /// <summary>
    /// Zoom in (not necessarily set)
    /// </summary>
    public List<GroupDTO> Zoom { get; set; }

    /// <summary>
    /// Associated machines (not necessarily set)
    /// </summary>
    public List<MachineDTO> Machines { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="group">not null</param>
    public GroupDTO (IGroup  group)
    {
      Debug.Assert (null != group);

      this.Id = group.Id;
      this.Display = group.Name;
      this.TreeName = group.TreeName;
      this.SingleMachine = group.SingleMachine;
      if (this.SingleMachine) {
        this.MachineId = group.GetMachines ().Single ().Id;
      }
      this.Dynamic = group.Dynamic;
      this.SortKind = group.SortKind;
      if (2 <= group.SortKind) {
        this.SortKindTip = "sorted";
      }
      else if (1 <= group.SortKind) {
        this.SortKindTip = "minor";
      }
      else {
        this.SortKindTip = "unsorted";
      }
      this.SortPriority = group.SortPriority ?? 0.0;
    }

    /// <summary>
    /// Constructor with an additional zoom level
    /// </summary>
    /// <param name="group"></param>
    /// <param name="zoom"></param>
    /// <param name="filter">optional: filter on machines</param>
    public GroupDTO (IGroup group, bool zoom, Func<IMachine, bool> filter = null)
      : this (group)
    {
      if (zoom && group.ZoomInMachineSelection) {
        var businessRequest = new Lemoine.Business.Machine.GroupZoomIn (group.Id);
        var businessResponse = Lemoine.Business.ServiceProvider
          .Get (businessRequest);
        if (null == businessResponse) {
          log.FatalFormat ("GetWithoutCache: unexpected null business response");
          return;
        }
        if (!businessResponse.Dynamic.HasValue) {
          if (log.IsWarnEnabled) {
            log.WarnFormat ($"GetWithoutCache: dynamic is not set, no zoom is implemented for {group.Id}, return");
          }
          return;
        }
        if (businessResponse.Dynamic.Value) {
          if (log.IsDebugEnabled) {
            log.Info ($"GetWithoutCache: dynamic zoom for {group.Id}, skip it");
          }
          return;
        }
        var subGroups = businessResponse.Children
          .Select (x => GetGroupFromId (x))
          .Where (x => FilterSubGroup (x, filter));
        this.Zoom = subGroups
          .Select (g => new GroupDTO (g, zoom, filter))
          .ToList ();

      }
    }

    IGroup GetGroupFromId (string id)
    {
      var businessRequest = new Lemoine.Business.Machine.GroupFromId (id);
      return Lemoine.Business.ServiceProvider
        .Get (businessRequest);
    }

    bool FilterSubGroup (IGroup subGroup, Func<IMachine, bool> filterMachine)
    {
      if (filterMachine is null) {
        return true;
      }
      else if (subGroup.SingleMachine) {
        var machine = subGroup.GetMachines ().Single ();
        return filterMachine (machine);
      }
      else {
        return true;
      }
    }
  }
}
