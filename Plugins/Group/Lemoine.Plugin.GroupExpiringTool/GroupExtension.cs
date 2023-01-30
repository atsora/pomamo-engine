// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Business.Group;
using Lemoine.Extensions.Business.Group.Impl;
using Lemoine.ModelDAO;
using Lemoine.Model;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Business.Tool;
using Lemoine.Business;

namespace Lemoine.Plugin.GroupExpiringTool
{
  public class GroupExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IGroupExtension
  {
    Configuration m_configuration;

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
      get { return string.IsNullOrEmpty (this.GroupCategoryName); }
    }

    public bool OmitInMachineSelection
    {
      get { return false; }
    }

    public IEnumerable<IGroup> Groups => new List<IGroup> { CreateGroup () };

    public GroupIdExtensionMatch GetGroupIdExtensionMatch (string groupId)
    {
      return string.Equals (m_configuration.GroupCategoryPrefix, groupId, System.StringComparison.InvariantCultureIgnoreCase)
        ? GroupIdExtensionMatch.Yes
        : GroupIdExtensionMatch.No;
    }

    public IGroup GetGroup (string groupId) => CreateGroup ();

    IGroup CreateGroup ()
    {
      return DynamicGroup.Create (m_configuration.GroupCategoryPrefix,
        m_configuration.GroupCategoryName,
        "ExpiringTool",
        GetMachines,
        GetMachines,
        null,
        true);
    }

    IEnumerable<IMachine> GetMachines ()
    {
      var request = new MachinesWithExpiringTools (m_configuration.MaxRemainingDuration);
      var response = ServiceProvider
        .Get<MachinesWithExpiringToolsResponse> (request);
      return response.Machines;
    }

    IEnumerable<IMachine> GetMachines (IEnumerable<IMachine> machines)
    {
      var request = new MachinesWithExpiringTools (m_configuration.MaxRemainingDuration, machines);
      var response = ServiceProvider
        .Get<MachinesWithExpiringToolsResponse> (request);
      return response.Machines;
    }
  }
}
