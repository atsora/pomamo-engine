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

namespace Lemoine.Plugin.GroupMain
{
  public class GroupExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IGroupExtension
  {
    static readonly ILog log = LogManager.GetLogger (typeof (GroupExtension).FullName);

    Configuration m_configuration;
    IEnumerable<IGroup> m_groups = null;
    IMachine m_mainMachine = null;

    (UtcDateTimeRange range, IEnumerable<IObservationStateSlot> slots) m_preLoadObservationStateSlots = (null, null);

    public bool Initialize ()
    {
      if (!LoadConfiguration (out m_configuration)) {
        return false;
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        m_mainMachine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (m_configuration.MainMachineId);
      }
      if (null == m_mainMachine) {
        if (log.IsErrorEnabled) {
          log.ErrorFormat ("Initialize: main machine id {0} does not exist => return false", m_configuration.MainMachineId);
        }
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
            var machines = m_configuration.MachineIds
              .Select (i => ModelDAOHelper.DAOFactory.MachineDAO.FindById (i));
            if (machines.Any (x => null == x)) {
              if (log.IsErrorEnabled) {
                log.ErrorFormat ("Groups: one machine does not exist");
              }
              machines = machines
                .Where (x => null != x);
            }
            machines = machines
              .OrderBy (m => GetMachineSortPriority (m))
              .ToList (); // Not to make it lazy
            var group = new GroupFromMachineList (m_configuration.GroupId, m_configuration.GroupName, m_configuration.GroupCategoryReference, machines, m_configuration.GroupSortPriority, true, m_configuration.ZoomInMachineSelection, aggregatingPartsMachines: new IMachine[] { m_mainMachine });
            group.PartProductionCurrentShift = GetPartProductionDataCurrentShift;
            group.PartProductionRange = GetPartProductionDataRange;
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

    int GetMachineSortPriority (IMachine machine)
    {
      if (machine.Id == m_configuration.MainMachineId) {
        return int.MaxValue;
      }
      else {
        return machine.DisplayPriority ?? 0;
      }
    }

    IPartProductionDataCurrentShift GetPartProductionDataCurrentShift ()
    {
      var businessRequest = new Lemoine.Business.Operation.PartProductionCurrentShiftOperation (m_mainMachine);
      return Lemoine.Business.ServiceProvider
        .Get (businessRequest);
    }

    IPartProductionDataRange GetPartProductionDataRange (UtcDateTimeRange range, UtcDateTimeRange preLoadRange)
    {
      var businessRequest = new Lemoine.Business.Operation.PartProductionRange (m_mainMachine, range, preLoadRange, () => GetObservationStateSlots (range, preLoadRange));
      return Lemoine.Business.ServiceProvider
        .Get (businessRequest);
    }

    IEnumerable<IObservationStateSlot> GetObservationStateSlots (UtcDateTimeRange range, UtcDateTimeRange preLoadRange)
    {
      if ((null != preLoadRange)
        && (null != m_preLoadObservationStateSlots.range) && (null != m_preLoadObservationStateSlots.slots)
        && !m_preLoadObservationStateSlots.range.ContainsRange (range)) {
        // Pre-load another set of observation state slots
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          m_preLoadObservationStateSlots = (preLoadRange, ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindOverlapsRange (m_mainMachine, preLoadRange));
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
            .FindOverlapsRange (m_mainMachine, range);
        }
      }
    }
  }
}
