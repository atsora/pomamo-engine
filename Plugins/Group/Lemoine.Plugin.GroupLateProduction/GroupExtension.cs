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
using Lemoine.Business.Operation;

namespace Lemoine.Plugin.GroupLateProduction
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
        "LateProduction",
        GetMachines,
        GetMachines,
        null,
        true);
    }

    IEnumerable<IMachine> GetMachines ()
    {
      // Full list of machines
      IList<IMachine> machines;
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAllNotObsolete ();
      }
      return GetMachines (machines);
    }

    IEnumerable<IMachine> GetMachines (IEnumerable<IMachine> machines)
    {
      if (machines == null) {
        return new List<IMachine> ();
      }

      // Check the state for all machines
      IList<IMachine> result = new List<IMachine> ();
      foreach (var machine in machines) {
        var request = new PartProductionCurrentShiftOperation (machine);
        var response = ServiceProvider.Get<PartProductionCurrentShiftOperationResponse> (request);
        
        // A goal and a current value are available?
        if (response.NbPiecesCurrentShift.HasValue && response.GoalCurrentShift.HasValue) {

          // Add the machine if the current value is less than the target
          if (response.NbPiecesCurrentShift.Value < response.GoalCurrentShift.Value) {
            result.Add (machine);
          }
        }
      }

      return result;
    }
  }
}
