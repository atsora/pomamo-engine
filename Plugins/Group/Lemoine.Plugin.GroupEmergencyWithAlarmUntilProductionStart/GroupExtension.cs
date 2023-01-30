// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Business.Group;
using Lemoine.Extensions.Business.Group.Impl;

namespace Lemoine.Plugin.GroupEmergencyWithAlarmUntilProductionStart
{
  public class GroupExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IGroupExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (GroupExtension).FullName);

    Configuration m_configuration;

    public string GroupCategoryName
    {
      get {
        Debug.Assert (null != m_configuration);

        return m_configuration.GroupName;
      }
    }

    public string GroupCategoryPrefix
    {
      get {
        return m_configuration.GroupId;
      }
    }

    public double GroupCategorySortPriority
    {
      get {
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
      return string.Equals (m_configuration.GroupId, groupId, System.StringComparison.InvariantCultureIgnoreCase)
        ? GroupIdExtensionMatch.Yes
        : GroupIdExtensionMatch.No;
    }

    public IGroup GetGroup (string groupId) => CreateGroup ();

    // TODO: when the production state is available
    //       else this is not really obvious to find the machine where the production is stopped
    IGroup CreateGroup () => throw new NotImplementedException ();

    public bool Initialize ()
    {
      return LoadConfiguration (out m_configuration);
    }
  }
}
