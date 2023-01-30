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
using System;

namespace Lemoine.Plugin.GroupStoppedMachines
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
        "StoppedMachines",
        GetMachines,
        GetMachines,
        null,
        true);
    }

    IEnumerable<IMachine> GetMachines ()
    {
      // Full list of machines
      IList<IMachine> machines = new List<IMachine> ();
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        // Only monitored machines
        var monitoredMachines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindAll ();
        foreach (var monitoredMachine in monitoredMachines) {
          machines.Add (monitoredMachine);
        }
      }
      return GetMachines (machines);
    }

    IEnumerable<IMachine> GetMachines (IEnumerable<IMachine> machines)
    {
      if (machines == null) {
        return new List<IMachine> ();
      }

      // Range in which the machine modes are analyzed
      UtcDateTimeRange range = new UtcDateTimeRange (DateTime.UtcNow.Subtract (m_configuration.MinimumStopDuration), DateTime.UtcNow);
      
      IList<IMachine> result = new List<IMachine> ();
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        // Check the facts for all machines
        foreach (var machine in machines) {
          var facts = ModelDAOHelper.DAOFactory.FactDAO.FindAllInUtcRange (machine, range);

          // Check that all machine modes in the facts are "not running" and that there is no gap between facts
          DateTime? lastEnd = null;
          bool machineIsStopped = true;
          foreach (var fact in facts) {
            if (fact.CncMachineMode == null || fact.CncMachineMode.Running != false ||
              (lastEnd != null && lastEnd != fact.Begin)) {
              machineIsStopped = false;
              break;
            }

            lastEnd = fact.End;
          }

          // Possibly store the machine in the result
          if (machineIsStopped) {
            result.Add (machine);
          }
        }
      }

      return result;
    }
  }
}
