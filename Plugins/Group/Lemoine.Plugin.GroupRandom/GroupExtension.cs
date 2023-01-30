// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Business.Group;
using Lemoine.Extensions.Business.Group.Impl;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Plugin.GroupRandom
{
  public class GroupExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IGroupExtension
  {
    Configuration m_configuration;
    IList<IMachine> m_allMachines = null;
    Random m_random = new Random ();

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
        m_configuration.GroupCategoryReference,
        GetMachines,
        GetMachines,
        null,
        true);
    }

    IEnumerable<IMachine> GetAllMachines ()
    {
      if (null == m_allMachines) {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          m_allMachines = ModelDAOHelper.DAOFactory.MachineDAO
            .FindAllForXmlSerialization ()
            .Where (m => m.MonitoringType.Id == (int)MachineMonitoringTypeId.Monitored)
            .ToList ();
        }
      }
      return m_allMachines;
    }

    IEnumerable<IMachine> GetMachines ()
    {
      return GetMachines (GetAllMachines ());
    }

    IEnumerable<IMachine> GetMachines (IEnumerable<IMachine> machines)
    {
      var totalNumberMachines = machines.Count ();
      var numberMachines = m_random.Next (0, totalNumberMachines + 1);
      IList<int> indexes = new List<int> ();
      for (int i = 1; i <= numberMachines; ++i) {
        while (true) {
          var newIndex = m_random.Next (0, totalNumberMachines);
          if (!indexes.Contains (newIndex)) {
            indexes.Add (newIndex);
            break;
          }
        }
      }
      var selectedMachines = new List<IMachine> ();
      var listOfMachines = machines.ToList ();
      foreach (var index in indexes) {
        selectedMachines.Add (listOfMachines[index]);
      }
      return selectedMachines;
    }
  }
}
