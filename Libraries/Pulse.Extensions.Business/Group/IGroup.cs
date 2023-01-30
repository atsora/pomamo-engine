// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Operation;
using Lemoine.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lemoine.Extensions.Business.Group
{
  /// <summary>
  /// Group interface
  /// 
  /// A group corresponds to a set of machines that can be dynamic, sorted or not
  /// </summary>
  public interface IGroup
  {
    /// <summary>
    /// Group Id
    /// 
    /// It is usally made from a prefix and a number.
    /// 
    /// Please reserve a unique number to the special group that is made of a unique machine
    /// (and then the group id will match the machine id).
    /// 
    /// It must be unique.
    /// 
    /// Specific Ids are possible for virtual groups.
    /// If the separator '_' is used, this corresponds to a special combined group
    /// that is made of the combination of different groups.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Group name.
    /// 
    /// For combined groups, this corresponds to the long name that is made
    /// of the name of all the groups (the separator is ' / ').
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Tree name to be used when a group is used in a tree or a path.
    /// 
    /// By default it is the same than Name.
    /// 
    /// For combined groups, it corresponds of the group name of last group.
    /// </summary>
    string TreeName { get; }

    /// <summary>
    /// Id that references a kind of category.
    /// 
    /// It is used to combine groups together.
    /// Machines of the same category references are added,
    /// while if the the category reference is difference, an intersection is done
    /// </summary>
    string CategoryReference { get; }

    /// <summary>
    /// This corresponds to the specific group that targets a single machine
    /// (usually the id is a number that corresponds to the machine id)
    /// </summary>
    bool SingleMachine { get; }

    /// <summary>
    /// Associated machine ids
    /// </summary>
    [Obsolete ("Use GetMachines instead", false)]
    IEnumerable<int> MachineIds { get; }

    /// <summary>
    /// Get all the associated machines
    /// </summary>
    /// <returns></returns>
    IEnumerable<IMachine> GetMachines ();

    /// <summary>
    /// Get all the associated machines that are part of the specified machines
    /// </summary>
    /// <param name="machines"></param>
    /// <returns></returns>
    IEnumerable<IMachine> GetMachines (IEnumerable<IMachine> machines);

    /// <summary>
    /// Does the group return dynamically its associated machines ?
    /// </summary>
    bool Dynamic { get; }

    /// <summary>
    /// Sort priority inside a group category if available.
    /// 
    /// Groups with the lowest priority are considered first.
    /// </summary>
    double? SortPriority { get; }

    /// <summary>
    /// Sort kind when different groups need to be combined:
    /// <item>if the machines are not sorted in a particular way, return 0</item>
    /// <item>if the sort importance is minor, return 1. In that case, the machines can be re-organized</item>
    /// <item>if the group was designed to return sorted machines, return a number greater than 2</item>
    /// 
    /// Prevents the machines from being re-organized manually if the sort priority is greater than 2
    /// </summary>
    int SortKind { get; }

    /// <summary>
    /// Zoom in machine selection
    /// 
    /// If set, a group may be displayed in the machine selection
    /// even if the parameter OmitInMachineSelection of the extension is set
    /// </summary>
    bool ZoomInMachineSelection { get; }

    /// <summary>
    /// Test if the group may include machine of a specific machine monitoring type
    /// 
    /// If it is a dynamic group or it is not obvious to know the property, null is returned
    /// </summary>
    /// <param name="machineMonitoringTypeId"></param>
    /// <returns></returns>
    bool? IncludeSpecificMonitoringType (MachineMonitoringTypeId machineMonitoringTypeId);

    /// <summary>
    /// Method to get the part production data in a group.
    /// 
    /// It may be null if there is no method to get it
    /// </summary>
    Func<IPartProductionDataCurrentShift> PartProductionCurrentShift { get; }

    /// <summary>
    /// Method to get the part production data in a group for a specific range
    /// 
    /// It may be null if there is no method to get it
    /// 
    /// The arguments of the returned method are:
    /// <item>a range</item>
    /// <item>optionally, a pre-load range</item>
    /// </summary>
    Func<UtcDateTimeRange, UtcDateTimeRange, IPartProductionDataRange> PartProductionRange { get; }

    /// <summary>
    /// Return if a machine should be considered as aggregating parts
    /// 
    /// If null, it can't be defined
    /// </summary>
    Func<IMachine, bool> IsMachineAggregatingParts { get; }
  }

  /// <summary>
  /// Extensions to interface IGroup
  /// </summary>
  public static class GroupDefaultMethods
  {
    static readonly ILog log = LogManager.GetLogger (typeof (GroupDefaultMethods));

    /// <summary>
    /// Aggregate parts using the IsMachineAggregatingParts property
    /// </summary>
    /// <param name="group"></param>
    /// <param name="partsPerMachine"></param>
    /// <returns></returns>
    public static double? AggregateParts (this IGroup group, Func<IMachine, double> partsPerMachine)
    {
      if (null == group.IsMachineAggregatingParts) {
        log.Warn ("AggregateParts: IsMachineAggretingParts is null => return null");
        return null;
      }

      var machines = group.GetMachines ()
        .Where (m => group.IsMachineAggregatingParts (m));
      return machines.Sum (m => partsPerMachine (m));
    }

    /// <summary>
    /// Return the machines that are aggregating parts using the IsMachineAggregatingParts property
    /// </summary>
    /// <param name="group"></param>
    /// <returns></returns>
    public static IEnumerable<IMachine> GetAggregatingPartsMachines (this IGroup group)
    {
      if (null == group.IsMachineAggregatingParts) {
        log.Warn ("AggregateParts: IsMachineAggretingParts is null => return an empty list");
        return new List<IMachine> ();
      }

      return group.GetMachines ()
        .Where (m => group.IsMachineAggregatingParts (m));
    }
  }

}
