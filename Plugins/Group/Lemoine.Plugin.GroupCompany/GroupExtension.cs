// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Business.Group;
using Lemoine.Extensions.Business.Group.Impl;
using Lemoine.ModelDAO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Plugin.GroupCompany
{
  public class GroupExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IGroupExtension
  {
    Configuration m_configuration;
    IEnumerable<IGroup> m_groups = null;

    public bool Initialize ()
    {
      return LoadConfiguration (out m_configuration);
    }

    public string GroupCategoryName
    {
      get
      {
        Debug.Assert (null != m_configuration);

        return m_configuration.GroupCategoryName;
      }
    }

    public string GroupCategoryPrefix
    {
      get
      {
        return m_configuration.GroupCategoryPrefix;
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
      get { return this.Groups.Count () <= 1; }
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
            var machines = ModelDAOHelper.DAOFactory.MachineDAO
              .FindAllForXmlSerialization ();
            var groups = machines
              .Where (m => null != m.Company)
              .GroupBy (m => m.Company.Id, (i, ms) => new GroupFromMachineList (BuildId (i), ms.First ().Company.Name, "Company", ms.OrderBy (a => a.DisplayPriority), ms.First ().Company.DisplayPriority, true, m_configuration.ZoomInMachineSelection));
            m_groups = groups
              .OrderBy (g => g.Name)
              .OrderBy (g => g.SortPriority ?? 0.0)
              .Cast<IGroup> ();
            Debug.Assert (null != m_groups);
          }
        }

        return m_groups;
      }
    }

    public GroupIdExtensionMatch GetGroupIdExtensionMatch (string groupId)
    {
      if (!this.IsGroupIdMatchFromPrefixNumber (m_configuration.GroupCategoryPrefix, groupId)) {
        return GroupIdExtensionMatch.No;
      }
      else {
        return this.IsGroupIdMatchFromGroups (groupId)
          ? GroupIdExtensionMatch.Yes
          : GroupIdExtensionMatch.Empty;
      }
    }

    public IGroup GetGroup (string groupId) => this.GetGroupFromGroups (groupId);

    string BuildId (int baseId)
    {
      return m_configuration.GroupCategoryPrefix + baseId;
    }
  }
}
