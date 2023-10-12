// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Group;
using Lemoine.Extensions.Business.Group.Impl;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.GroupPart
{
  public class GroupExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IGroupExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (GroupExtension).FullName);

    Configuration m_configuration;
    IList<IMonitoredMachine> m_monitoredMachines;

    public string GroupCategoryName => m_configuration.GroupCategoryName;

    public string GroupCategoryPrefix => m_configuration.GroupCategoryPrefix;

    public double GroupCategorySortPriority => m_configuration.GroupCategorySortPriority;

    public bool OmitGroupCategory => string.IsNullOrEmpty (this.GroupCategoryName);

    public bool OmitInMachineSelection => false;

    public IEnumerable<IGroup> Groups
    {
      get {
        var parts = new HashSet<IPart> ();

        foreach (var monitoredMachine in m_monitoredMachines) {
          var businessRequest = new Lemoine.Business.Operation.EffectiveOperationCurrentShift (monitoredMachine);
          IPart part;
          try {
            var effectiveOperationCurrentShift = Lemoine.Business.ServiceProvider
              .Get (businessRequest);
            part = effectiveOperationCurrentShift.Component?.Part;
            if (part is not null) {
              parts.Add (part);
            }
          }
          catch (Exception ex) {
            log.Error ($"Groups.get: error when trying to get the operation of the current shift for machine {monitoredMachine.Id}", ex);
          }
        }

        foreach (var part in parts.OrderBy (x => x.Display)) {
          yield return CreateGroup (part);
        }
      }
    }

    public GroupIdExtensionMatch GetGroupIdExtensionMatch (string groupId)
    {
      return this.IsGroupIdMatchFromPrefixNumber (m_configuration.GroupCategoryPrefix, groupId)
        ? GroupIdExtensionMatch.Dynamic
        : GroupIdExtensionMatch.No;
    }

    public IGroup GetGroup (string groupId)
    {
      var componentId = ExtractComponentId (groupId);
      return GetGroup (groupId, componentId);
    }

    IGroup GetGroup (string groupId, int componentId)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var part = ModelDAOHelper.DAOFactory.PartDAO
          .FindByComponentId (componentId);
        if (part is null) {
          log.Error ($"GetGroup: no part with componentId={componentId} => return a group with no machine");
          return DynamicGroup.Create (groupId,
            $"Unknown part {componentId}",
            m_configuration.GroupCategoryReference,
            () => new List<IMachine> (),
            machines => new List<IMachine> (),
            null,
            false);
        }
        else {
          return CreateGroup (part);
        }
      }
    }

    int ExtractComponentId (string groupId) => this.ExtractIdAfterPrefix (m_configuration.GroupCategoryPrefix, groupId);

    public bool Initialize ()
    {
      var result = LoadConfiguration (out m_configuration);
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        m_monitoredMachines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindAll ();
      }
      return result;
    }

    IGroup CreateGroup (IPart part)
    {
      Debug.Assert (part is not null);

      try {
        var groupId = m_configuration.GroupCategoryPrefix + part.ComponentId;
        var groupName = part.Display;
        return DynamicGroup.Create (groupId,
          groupName,
          m_configuration.GroupCategoryReference,
          () => GetMachines (part.ComponentId),
          machines => GetMachines (part.ComponentId, machines),
          null,
          false,
          zoomInMachineSelection: m_configuration.ZoomInMachineSelection);
      }
      catch (Exception ex) {
        log.Error ($"CreateGroup: exception for part componentId={part?.ComponentId}", ex);
        throw;
      }
    }

    IEnumerable<IMachine> GetMachines (int componentId)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var machines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindAll ()
          .Cast<IMachine> ();
        return GetMachines (componentId, machines);
      }
    }

    IEnumerable<IMachine> GetMachines (int componentId, IEnumerable<IMachine> machines)
    {
      foreach (var machine in machines) {
        var businessRequest = new Lemoine.Business.Operation.EffectiveOperationCurrentShift (machine);
        IPart part = null;
        try {
          var effectiveOperationCurrentShift = Lemoine.Business.ServiceProvider
            .Get (businessRequest);
          part = effectiveOperationCurrentShift.Component?.Part;
        }
        catch (Exception ex) {
          log.Error ($"GetMachines: error when trying to get the operation of the current shift for machine {machine.Id} but continue", ex);
        }
        if (part is not null && (part.ComponentId == componentId)) {
          yield return machine;
        }
      }
    }
  }
}
