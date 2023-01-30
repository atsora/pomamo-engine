// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Business.Group;
using Lemoine.Extensions.Business.Group.Impl;
using Lemoine.ModelDAO;
using Lemoine.Model;
using Lemoine.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Plugin.GroupFocusedAlarms
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
        "FocusedAlarms",
        GetMachines,
        GetMachines,
        null,
        true);
    }

    IEnumerable<IMachine> GetMachines ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var machines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindAll ()
          .Cast<IMachine> ();
        return GetMachines (machines);
      }
    }

    IEnumerable<IMachine> GetMachines (IEnumerable<IMachine> machines)
    {
      var now = System.DateTime.UtcNow;
      var detectionInterval = new UtcDateTimeRange (now.Subtract (m_configuration.DetectionIntervalDuration), now);

      // Initialize a list of machines, associated with the date when the alarm started
      var machineDates = new Dictionary<IMachine, DateTime> ();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        foreach (var machine in machines) {
          IMonitoredMachine monitoredMachine;
          if (machine is IMonitoredMachine) {
            monitoredMachine = (IMonitoredMachine)machine;
            ModelDAOHelper.DAOFactory.MonitoredMachineDAO.Lock (monitoredMachine);
          }
          else {
            monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
              .FindById (machine.Id);
          }
          // List of the recent cnc alarms for the machine
          // Note: FindOverlapsRangeWithSeverity is pretty inefficient
          var alarmSlots = ModelDAOHelper.DAOFactory.CncAlarmDAO.FindOverlapsRange (monitoredMachine, detectionInterval);

          DateTime? lastDate = null;
          foreach (var alarmSlot in alarmSlots) {
            if (alarmSlot.Severity != null && alarmSlot.Severity.Focus.HasValue && alarmSlot.Severity.Focus.Value) {
              if (alarmSlot.DateTimeRange.Lower.HasValue) {
                if (!lastDate.HasValue) {
                  lastDate = alarmSlot.DateTimeRange.Lower.Value;
                }
                else if (alarmSlot.DateTimeRange.Lower.Value < lastDate.Value) {
                  lastDate = alarmSlot.DateTimeRange.Lower.Value;
                }
              }
            }
          }

          // Store the machine if there is a start date time
          if (lastDate.HasValue) {
            machineDates[machine] = lastDate.Value;
          }
        }
      }

      // Order the machines
      // cf https://stackoverflow.com/questions/289/how-do-you-sort-a-dictionary-by-value
      return from entry in machineDates orderby entry.Value ascending select entry.Key;
    }
  }
}
