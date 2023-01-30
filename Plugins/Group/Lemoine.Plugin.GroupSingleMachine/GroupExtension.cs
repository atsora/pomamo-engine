// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Business;
using Lemoine.Extensions.Business.Group;
using Lemoine.Extensions.Business.Group.Impl;
using Lemoine.Extensions.Business.Operation;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Plugin.GroupSingleMachine
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
      get { return false; }
    }

    public bool OmitInMachineSelection
    {
      get { return string.IsNullOrEmpty (this.GroupCategoryName); }
    }

    public IEnumerable<IGroup> Groups
    {
      get
      {
        if (null == m_groups) { // Not initialized yet
          using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            var machines = ModelDAOHelper.DAOFactory.MachineDAO
              .FindAll ();
            var groups = machines.Select (m => new SingleMachineGroup (m, GetPartProductionDataCurrentShift, GetPartProductionDataRange, BuildId));
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

    IPartProductionDataCurrentShift GetPartProductionDataCurrentShift (IMachine machine)
    {
      var businessRequest = new Lemoine.Business.Operation.PartProductionCurrentShiftOperation (machine);
      return Lemoine.Business.ServiceProvider
        .Get (businessRequest);
    }

    IPartProductionDataRange GetPartProductionDataRange (IMachine machine, UtcDateTimeRange range, UtcDateTimeRange preLoadRange, Func<IEnumerable<IObservationStateSlot>> preLoadObservationStateSlots)
    {
      var businessRequest = new Lemoine.Business.Operation.PartProductionRange (machine, range, preLoadRange, preLoadObservationStateSlots);
      return Lemoine.Business.ServiceProvider
        .Get (businessRequest);
    }
  }
}
