// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Business;
using Lemoine.Extensions.Business.Group;
using Lemoine.Extensions.Business.Group.Impl;
using Lemoine.Extensions.Business.Operation;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Plugin.GroupSequential
{
  public class GroupExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IGroupExtension
  {
    static readonly ILog log = LogManager.GetLogger (typeof (GroupExtension).FullName);

    Configuration m_configuration;
    IEnumerable<IGroup> m_groups = null;

    (IMachine machine, UtcDateTimeRange range, IEnumerable<IObservationStateSlot> slots) m_preLoadObservationStateSlots = (null, null, null);

    public bool Initialize ()
    {
      if (!LoadConfiguration (out m_configuration)) {
        return false;
      }

      return true;
    }

    public string GroupCategoryName
    {
      get
      {
        Debug.Assert (null != m_configuration);

        return m_configuration.GroupCategoryName;
      }
    }

    public double GroupCategorySortPriority
    {
      get
      {
        return m_configuration.GroupCategorySortPriority;
      }
    }

    public bool OmitGroupCategory
    {
      get { return string.IsNullOrEmpty (this.GroupCategoryName); } // Because it is made of a unique group
    }

    public bool OmitInMachineSelection
    {
      get { return m_configuration.OmitInMachineSelection; }
    }

    public IEnumerable<IGroup> Groups
    {
      get
      {
        if (null == m_groups) { // Not initialized yet
          using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            IEnumerable<IMachine> allMachines = m_configuration.MachineIds
              .Select (i => ModelDAOHelper.DAOFactory.MachineDAO.FindById (i))
              .ToList ();
            if (allMachines.Any (x => null == x)) {
              if (log.IsErrorEnabled) {
                log.ErrorFormat ("Groups: one machine does not exist");
              }
              allMachines = allMachines
                .Where (x => null != x);
            }
            IEnumerable<IMachine> machines;
            if (null != m_configuration.MachineSortIds) {
              var machineSortIdList = m_configuration.MachineSortIds.ToList ();
              var withSortIdMachines = allMachines
                .Where (x => machineSortIdList.Contains (x.Id));
              withSortIdMachines = withSortIdMachines
                .OrderBy (x => machineSortIdList.IndexOf (x.Id));
              var otherMachines = allMachines
                .Where (x => !machineSortIdList.Contains (x.Id));
              otherMachines = otherMachines
                .OrderBy (x => x.DisplayPriority);
              machines = withSortIdMachines.Concat (otherMachines);
            }
            else {
              machines = allMachines
                .OrderBy (x => x.DisplayPriority);
            }
            var group = new GroupFromMachineList (m_configuration.GroupId, m_configuration.GroupName, m_configuration.GroupCategoryReference, machines, m_configuration.GroupSortPriority, true, m_configuration.ZoomInMachineSelection);
            var lastMachine = machines.LastOrDefault ();
            if (null != lastMachine) {
              group.IsMachineAggregatingParts = (m) => m.Equals (lastMachine);
              group.PartProductionCurrentShift = () => GetPartProductionDataCurrentShift (lastMachine);
              group.PartProductionRange = (r, p) => GetPartProductionDataRange (lastMachine, r, p);
            }
            m_groups = new List<IGroup> { group };
            Debug.Assert (null != m_groups);
          }
        }

        return m_groups;
      }
    }

    public GroupIdExtensionMatch GetGroupIdExtensionMatch (string groupId)
    {
      return string.Equals (m_configuration.GroupId, groupId, System.StringComparison.InvariantCultureIgnoreCase)
        ? GroupIdExtensionMatch.Yes
        : GroupIdExtensionMatch.No;
    }

    public IGroup GetGroup (string groupId) => this.GetGroupFromGroups (groupId);

    IPartProductionDataCurrentShift GetPartProductionDataCurrentShift (IMachine machine)
    {
      var businessRequest = new Lemoine.Business.Operation.PartProductionCurrentShiftOperation (machine);
      return Lemoine.Business.ServiceProvider
        .Get (businessRequest);
    }

    IPartProductionDataRange GetPartProductionDataRange (IMachine machine, UtcDateTimeRange range, UtcDateTimeRange preLoadRange)
    {
      var businessRequest = new Lemoine.Business.Operation.PartProductionRange (machine, range, preLoadRange, () => GetObservationStateSlots (machine, range, preLoadRange));
      return Lemoine.Business.ServiceProvider
        .Get (businessRequest);
    }

    IEnumerable<IObservationStateSlot> GetObservationStateSlots (IMachine machine, UtcDateTimeRange range, UtcDateTimeRange preLoadRange)
    {
      Debug.Assert (null != machine);

      if (machine.Id != (m_preLoadObservationStateSlots.machine?.Id ?? 0)) {
        m_preLoadObservationStateSlots = (null, null, null);
      }

      if ((null != preLoadRange)
        && ((null == m_preLoadObservationStateSlots.range) || (null == m_preLoadObservationStateSlots.slots)
          || !m_preLoadObservationStateSlots.range.ContainsRange (range))) {
        // Pre-load another set of observation state slots
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          m_preLoadObservationStateSlots = (machine, preLoadRange, ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindOverlapsRange (machine, preLoadRange));
        }
      }

      if ((null != m_preLoadObservationStateSlots.range)
        && (m_preLoadObservationStateSlots.range.ContainsRange (range))) {
        return m_preLoadObservationStateSlots.slots
          .Where (s => s.DateTimeRange.Overlaps (range));
      }
      else {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          return ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindOverlapsRange (machine, range);
        }
      }
    }
  }
}
