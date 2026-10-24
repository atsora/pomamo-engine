// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;

namespace Lemoine.Business.Machine
{
  /// <summary>
  /// Machine group ids that are stored, for example in a configuration: a group id in the
  /// sense of <see cref="GroupFromId"/>, including an ordered list of machine ids separated
  /// by commas
  ///
  /// Such a list is stored sorted by ascending machine id, see <see cref="Normalize"/>, so
  /// that it is found whatever the order the machines were selected in.
  ///
  /// A dynamic group is accepted: only its current machines are considered, and a warning
  /// is logged
  /// </summary>
  public static class MachineGroupIds
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MachineGroupIds).FullName);

    /// <summary>
    /// Normalize a machine group id: a list of machine ids is sorted by ascending machine
    /// id, without any duplicate. Any other group id is only trimmed
    /// </summary>
    /// <param name="machineGroup">nullable</param>
    /// <returns>not null</returns>
    public static string Normalize (string machineGroup)
    {
      var trimmed = (machineGroup ?? "").Trim ();
      if (!trimmed.Contains (GroupFromId.MACHINE_LIST_SEPARATOR)) {
        return trimmed;
      }

      var machineIds = new List<int> ();
      foreach (var item in trimmed.Split (GroupFromId.MACHINE_LIST_SEPARATOR)) {
        if (!int.TryParse (item.Trim (), out var machineId)) {
          // Not a list of machine ids: keep it as it is, GroupFromId rejects it
          return trimmed;
        }
        machineIds.Add (machineId);
      }
      return string.Join (GroupFromId.MACHINE_LIST_SEPARATOR.ToString (), machineIds.Distinct ().OrderBy (x => x));
    }

    /// <summary>
    /// Get the ids of the current machines of a machine group
    /// </summary>
    /// <param name="machineGroup">group id</param>
    /// <returns>not null</returns>
    /// <exception cref="ArgumentException">no machine group id, or no group matches it</exception>
    public static async Task<IList<int>> GetMachineIdsAsync (string machineGroup)
    {
      if (string.IsNullOrWhiteSpace (machineGroup)) {
        log.Error ("GetMachineIdsAsync: no machine group id");
        throw new ArgumentException ("A machine group is required", nameof (machineGroup));
      }

      var group = await ServiceProvider
        .GetAsync (new GroupFromId (machineGroup));
      if (group is null) {
        // Either the group id matches no group extension at all,
        // or it does but no group is associated to it for the moment
        log.Error ($"GetMachineIdsAsync: no machine group with id {machineGroup}");
        throw new ArgumentException ($"Machine group {machineGroup} does not exist", nameof (machineGroup));
      }
      if (group.Dynamic) {
        log.Warn ($"GetMachineIdsAsync: machine group {machineGroup} is dynamic, only its current machines are considered");
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"GetMachineIdsAsync: machine group {machineGroup} is {group.Name}");
      }
      return group.GetMachines ()
        .Select (x => x.Id)
        .ToList ();
    }
  }
}
