// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Business;
using Lemoine.Extensions.Business.Group;
using Lemoine.Extensions.Business.Group.Impl;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Plugin.GroupMachineFilter
{
  public class GroupExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IGroupExtension
  {
    static readonly ILog log = LogManager.GetLogger (typeof (GroupExtension).FullName);

    Configuration m_configuration;
    IMachineFilter m_machineFilter;
    IEnumerable<IMachine> m_machines;
    IEnumerable<IGroup> m_groups = null;

    public bool Initialize ()
    {
      if (!LoadConfiguration (out m_configuration)) {
        return false;
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        m_machines = ModelDAOHelper.DAOFactory.MachineDAO
          .FindAllWithChildren ();
        if (0 < m_configuration.MachineFilterId) {
          m_machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
            .FindById (m_configuration.MachineFilterId);
          if (null == m_machineFilter) {
            if (log.IsErrorEnabled) {
              log.ErrorFormat ("Initialize: machine filter id {0} does not exist", m_configuration.MachineFilterId);
            }
            return false;
          }
          m_machines = m_machines
            .Where (m => m_machineFilter.IsMatch (m))
            .ToList (); // Else this is lazy...
        }
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
            var machines = m_machines
              .OrderBy (m => GetMachineSortPriority (m));
            var group = new GroupFromMachineList (m_configuration.GroupId, m_configuration.GroupName, m_configuration.GroupCategoryReference, machines, m_configuration.GroupSortPriority, true, m_configuration.ZoomInMachineSelection);
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
      return machine.DisplayPriority ?? 0;
    }
  }
}
