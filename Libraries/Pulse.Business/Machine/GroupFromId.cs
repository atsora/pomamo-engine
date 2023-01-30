// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Group;
using Lemoine.Extensions.Business;
using System.Threading.Tasks;
using Lemoine.Extensions.Business.Operation;

namespace Lemoine.Business.Machine
{
  class CombinedGroup : IGroup
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CombinedGroup).FullName);

    /// <summary>
    /// Separator for the Ids
    /// </summary>
    internal static readonly char SEPARATOR = '_'; // Note: + is not supported by the web service (replaced by ' ')

    readonly string m_id;
    IEnumerable<IGroup> m_groups;
    bool m_valid = true;

    CombinedGroup (string groupId)
    {
      m_id = groupId;
    }

    static internal IGroup Create (string groupId)
    {
      var group = new CombinedGroup (groupId);
      if (null == group.Groups) {
        if (log.IsErrorEnabled) {
          log.ErrorFormat ("CreateCombinedGroup: combined group with id {0} is not valid => return null");
        }
        return null;
      }
      else {
        return group;
      }
    }

    /// <summary>
    /// Lazy retrieve of the groups
    /// </summary>
    IEnumerable<IGroup> Groups
    {
      get {
        if (!m_valid) {
          return null;
        }

        if (null == m_groups) {
          var groupIds = m_id.Trim ()
            .Split (new char[] { SEPARATOR });
          m_groups = groupIds
            .Select (gid => GetFromGroupIdWithCache (gid))
            .ToList ();

          if (m_groups.Any (g => g is null)) {
            if (log.IsErrorEnabled) {
              log.Error ($"Groups.get: there are groups that don't exist in combined group id {m_id} => invalid it");
            }
            m_valid = false;
            m_groups = null;
          }
        }

        return m_groups;
      }
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public string Id => m_id;

    /// <summary>
    /// <see cref="IGroup"/>
    /// 
    /// The name of a combined group is made of the name of all the groups
    /// with the separator ' / '
    /// </summary>
    public string Name
    {
      get {
        return string.Join (" / ",
          Groups.Select (g => g.Name).ToArray ());
      }
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// 
    /// The tree name of a combined group is the name of the last group
    /// </summary>
    public string TreeName
    {
      get {
        if (Groups.Any ()) {
          return Groups.Last ().Name;
        }
        else {
          return "";
        }
      }
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public string CategoryReference
    {
      get {
        return string.Join (SEPARATOR.ToString (),
          Groups.Select (g => g.CategoryReference).ToArray ());
      }
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// 
    /// Simplify it: only try if it is made of a unique group and this unique group is a SingleMachine group
    /// </summary>
    public bool SingleMachine => (1 == Groups.Count ()) && Groups.First ().SingleMachine;

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    [Obsolete ("Use GetMachines instead", false)]
    public IEnumerable<int> MachineIds => GetMachines ().Select (m => m.Id);

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public bool Dynamic
    {
      get {
        return Groups.Any (g => g.Dynamic);
      }
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public double? SortPriority
    {
      get {
        if (Groups.Any (g => g.SortPriority.HasValue)) {
          return Groups
            .Where (g => g.SortPriority.HasValue)
            .Min (g => g.SortPriority.Value);
        }
        else {
          return null;
        }
      }
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public int SortKind
    {
      get {
        if (Groups.Any (g => 2 <= g.SortKind)) {
          if (1 < Groups.Count (g => 2 <= g.SortKind)) {
            return 0;
          }
          else {
            return Groups.Max (g => g.SortKind);
          }
        }
        else if (Groups.Any (g => 1 <= g.SortKind)) {
          if (1 < Groups.Count (g => 1 <= g.SortKind)) {
            return 0;
          }
          else {
            return Groups.Max (g => g.SortKind);
          }
        }
        else {
          return 0;
        }
      }
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public bool ZoomInMachineSelection
    {
      get {
        return this.Groups.All (g => g.ZoomInMachineSelection);
      }
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    /// <param name="machineMonitoringTypeId"></param>
    /// <returns></returns>
    public bool? IncludeSpecificMonitoringType (MachineMonitoringTypeId machineMonitoringTypeId)
    {
      var subValues = this.Groups
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
    /// <see cref="IGroup"/>
    /// </summary>
    public Func<IPartProductionDataCurrentShift> PartProductionCurrentShift => null;

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public Func<UtcDateTimeRange, UtcDateTimeRange, IPartProductionDataRange> PartProductionRange => null;

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public Func<IMachine, bool> IsMachineAggregatingParts
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

    public IEnumerable<IMachine> GetMachines ()
    {
      if (!Groups.Any ()) {
        return new List<IMachine> ();
      }
      else { // Groups.Any ()
        IEnumerable<IMachine> machines;
        var staticGroups = Groups
          .Where (g => !g.Dynamic);
        var dynamicGroups = Groups
          .Where (g => g.Dynamic);
        if (staticGroups.Any ()) {
          machines = staticGroups.First ().GetMachines ();
          foreach (var group in staticGroups.Skip (1)) {
            machines = machines
              .Intersect (group.GetMachines (machines));
          }
          foreach (var group in dynamicGroups) {
            machines = machines
              .Intersect (group.GetMachines (machines));
          }
        }
        else { // !staticGroups.Any ()
          Debug.Assert (dynamicGroups.Any ());
          machines = dynamicGroups.First ().GetMachines ();
          foreach (var group in dynamicGroups.Skip (1)) {
            machines = machines
              .Intersect (group.GetMachines (machines));
          }
        }

        return machines;
      }
    }

    public IEnumerable<IMachine> GetMachines (IEnumerable<IMachine> subSetOfMachines)
    {
      if (!Groups.Any ()) {
        return new List<IMachine> ();
      }
      else { // Groups.Any ()
        IEnumerable<IMachine> machines;
        var staticGroups = Groups
          .Where (g => !g.Dynamic);
        var dynamicGroups = Groups
          .Where (g => g.Dynamic);
        if (staticGroups.Any ()) {
          machines = staticGroups.First ().GetMachines (subSetOfMachines);
          foreach (var group in staticGroups.Skip (1)) {
            machines = machines
              .Intersect (group.GetMachines (machines));
          }
          foreach (var group in dynamicGroups) {
            machines = machines
              .Intersect (group.GetMachines (machines));
          }
        }
        else { // !staticGroups.Any ()
          Debug.Assert (dynamicGroups.Any ());
          machines = dynamicGroups.First ().GetMachines (subSetOfMachines);
          foreach (var group in dynamicGroups.Skip (1)) {
            machines = machines
              .Intersect (group.GetMachines (machines));
          }
        }

        return machines;
      }
    }

    IGroup GetFromGroupIdWithCache (string groupId)
    {
      var request = new GroupFromId (groupId);
      var group = ServiceProvider.Get (request);
      if (log.IsErrorEnabled && (group is null)) {
        log.Error ($"GetFromGroupIdWithCache: group with id {groupId} does not exist");
      }
      return group;
    }
  }

  /// <summary>
  /// Request class to get a group and its machine ids from a group id
  /// </summary>
  public sealed class GroupFromId
    : IRequest<IGroup>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (GroupFromId).FullName);

    /// <summary>
    /// Group Id
    /// </summary>
    string GroupId { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="groupId">not null and not empty</param>
    public GroupFromId (string groupId)
    {
      Debug.Assert (!string.IsNullOrEmpty (groupId));

      this.GroupId = groupId.Trim ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>Group or null if not found or not valid</returns>
    public IGroup Get ()
    {
      Debug.Assert (!string.IsNullOrEmpty (GroupId));

      if (log.IsDebugEnabled) {
        log.Debug ($"Get: group id {this.GroupId}");
      }

      var separator = CombinedGroup.SEPARATOR;
      if (this.GroupId.Contains (separator)) {
        return CombinedGroup.Create (this.GroupId);
      }
      else {
        return GetFromUniqueExtension ();
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<IGroup> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    IGroup GetFromUniqueExtension ()
    {
      var groupExtensionMatch = Lemoine.Business.ServiceProvider
        .Get (new GroupExtensionFromId (this.GroupId));
      if (null != groupExtensionMatch) {
        var groupExtension = groupExtensionMatch.Item1;
        var match = groupExtensionMatch.Item2;
        switch (match) {
        case GroupIdExtensionMatch.No:
          log.Fatal ($"GetFromUniqueExtension: a group extension category={groupExtension.GroupCategoryName} is returned although match is No, this is unexpected");
          Debug.Assert (false);
          throw new InvalidOperationException ("Invalid pair group extension match");
        case GroupIdExtensionMatch.Empty:
          log.Info ($"GetFromUniqueExtension: group extension category={groupExtension.GroupCategoryName} found but no associated group currently for {this.GroupId} => return null");
          return null;
        case GroupIdExtensionMatch.Yes:
        case GroupIdExtensionMatch.Dynamic:
          break;
        }
        var group = groupExtension.GetGroup (this.GroupId);
        if (group is null) {
          log.Info ($"GetFromUniqueExtension: group extension category={groupExtension.GroupCategoryName} found but no associated group currently for {this.GroupId} => return null");
          return null;
        }
        else {
          return group;
        }
      }
      else {
        log.Error ($"GetFromUniqueExtension: no group extension for id {this.GroupId} was found");
        return null;
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.Machine.GroupFromId." + this.GroupId;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<IGroup> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (IGroup data)
    {
      if (null == data) { // Not valid group
        // TODO: guess if the data because of some dynamic values or not
        return CacheTimeOut.CurrentShort.GetTimeSpan ();
      }
      else if (data.Dynamic) {
        return CacheTimeOut.CurrentShort.GetTimeSpan ();
      }
      else {
        return CacheTimeOut.Config.GetTimeSpan ();
      }
    }
  }
}
