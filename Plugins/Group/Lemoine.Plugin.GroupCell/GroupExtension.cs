// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Business;
using Lemoine.Extensions.Business.Group;
using Lemoine.Extensions.Business.Group.Impl;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Plugin.GroupCell
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
              .Where (m => null != m.Cell)
              .GroupBy (m => m.Cell.Id, (i, ms) => new GroupFromMachineList (BuildId (i), ms.First ().Cell.Name, "Cell", ms.OrderBy (a => a.DisplayPriority), ms.First ().Cell.DisplayPriority, true, m_configuration.ZoomInMachineSelection, partProductionCurrentShift: PartProductionDataCurrentShift (ms)));
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

    Func<IEnumerable<IMachine>, IPartProductionDataCurrentShift> PartProductionDataCurrentShift (IEnumerable<IMachine> machines)
    {
      var cell = machines.First ().Cell;
      var sortedMachines = machines.OrderBy (x => x.DisplayPriority);
      switch (cell.Kind) {
      case CellKind.None:
        return null;
      case CellKind.Concurrent:
        return GetPartProductionDataCurrentShiftConcurrent;
      case CellKind.Sequential:
        return (ms) => GetPartProductionDataCurrentShift (sortedMachines.Last ());
      default:
        throw new InvalidOperationException ();
      }
    }

    IPartProductionDataCurrentShift GetPartProductionDataCurrentShiftConcurrent (IEnumerable<IMachine> machines)
    {
      var data = machines
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
  }
}
