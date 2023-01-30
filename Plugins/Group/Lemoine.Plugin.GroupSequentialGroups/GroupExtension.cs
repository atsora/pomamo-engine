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

namespace Lemoine.Plugin.GroupSequentialGroups
{
  public class GroupExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IGroupExtension
  {
    static readonly ILog log = LogManager.GetLogger (typeof (GroupExtension).FullName);

    Configuration m_configuration;
    IEnumerable<IGroup> m_groups = null;

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
          var subGroups = m_configuration
            .GroupIds
            .Select (i => GetSubGroup (i))
            .Where (g => null != g);
          var group = new SequentialGroup (m_configuration.GroupId,
            m_configuration.GroupName, m_configuration.GroupCategoryReference,
            subGroups, m_configuration.GroupSortPriority, m_configuration.ZoomInMachineSelection);
          m_groups = new List<IGroup> { group };
          Debug.Assert (null != m_groups);
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

    IGroup GetSubGroup (string groupId)
    {
      var request = new Lemoine.Business.Machine.GroupFromId (groupId);
      var group = Lemoine.Business.ServiceProvider
        .Get (request);
      if ((null == group) && log.IsErrorEnabled) {
        log.ErrorFormat ("GetGroup: group of id {0} does not exist", groupId);
      }
      return group;
    }
  }
}
