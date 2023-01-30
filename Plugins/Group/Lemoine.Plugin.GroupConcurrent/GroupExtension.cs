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

namespace Lemoine.Plugin.GroupConcurrent
{
  public class GroupExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IGroupExtension
  {
    static readonly ILog log = LogManager.GetLogger (typeof (GroupExtension).FullName);

    Configuration m_configuration;
    IEnumerable<IGroup> m_groups = null;
    IEnumerable<IMachine> m_machines;

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
              m_machines = withSortIdMachines.Concat (otherMachines);
            }
            else {
              m_machines = allMachines
                .OrderBy (x => x.DisplayPriority);
            }
            var group = new GroupFromMachineList (m_configuration.GroupId, m_configuration.GroupName, m_configuration.GroupCategoryReference, m_machines, m_configuration.GroupSortPriority, true, m_configuration.ZoomInMachineSelection, aggregatingPartsMachines: m_machines);
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

    IPartProductionDataCurrentShift GetPartProductionDataCurrentShift ()
    {
      var data = m_machines
        .Select (x => GetPartProductionDataCurrentShift (x));
      return new Lemoine.Extensions.Business.Group.Impl
        .PartProductionDataCurrentShiftConcurrent (data);
    }

    IPartProductionDataCurrentShift GetPartProductionDataCurrentShift (IMachine machine)
    {
      var businessRequest = new Lemoine.Business.Operation.PartProductionCurrentShiftOperation (machine);
      return Lemoine.Business.ServiceProvider
        .Get (businessRequest);
    }

    IPartProductionDataRange GetPartProductionDataRange (UtcDateTimeRange range, UtcDateTimeRange preLoadRange)
    {
      var data = m_machines
        .Select (x => GetPartProductionDataRange (x, range, preLoadRange));
      return new Lemoine.Extensions.Business.Group.Impl
        .PartProductionDataRangeConcurrent (data);
    }

    IPartProductionDataRange GetPartProductionDataRange (IMachine machine, UtcDateTimeRange range, UtcDateTimeRange preLoadRange)
    {
      // TODO: pre-load the observation state slots. Warning, the cache must be done by machine

      var businessRequest = new Lemoine.Business.Operation.PartProductionRange (machine, range, preLoadRange);
      return Lemoine.Business.ServiceProvider
        .Get (businessRequest);
    }
  }
}
