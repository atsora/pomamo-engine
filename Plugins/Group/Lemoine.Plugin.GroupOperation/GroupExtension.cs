// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Group;
using Lemoine.Extensions.Business.Group.Impl;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.GroupOperation
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

    public bool OmitInMachineSelection => m_configuration.OmitInMachineSelection;

    public IEnumerable<IGroup> Groups
    {
      get {
        var operations = new HashSet<IOperation> ();

        foreach (var monitoredMachine in m_monitoredMachines) {
          var businessRequest = new Lemoine.Business.Operation.EffectiveOperationCurrentShift (monitoredMachine);
          IOperation operation;
          try {
            var effectiveOperationCurrentShift = Lemoine.Business.ServiceProvider
              .Get (businessRequest);
            operation = effectiveOperationCurrentShift.Operation;
          }
          catch (Exception ex) {
            log.Error ($"Groups.get: error when trying to get the operation of the current shift for machine {monitoredMachine.Id}", ex);
            throw;
          }
          if (operation is not null) {
            operations.Add (operation);
          }
        }

        foreach (var operation in operations.OrderBy (x => x.Display)) {
          yield return CreateGroup (operation);
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
      var projectId = ExtractProjectId (groupId);
      return GetGroup (groupId, projectId);
    }

    IGroup GetGroup (string groupId, int operationId)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var operation = ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (operationId);
        if (operation is null) {
          log.Error ($"GetGroup: no operation with operationId={operationId} => return a group with no machine");
          return DynamicGroup.Create (groupId,
            $"Unknown operation {operationId}",
            m_configuration.GroupCategoryReference,
            () => new List<IMachine> (),
            machines => new List<IMachine> (),
            null,
            false);
        }
        else {
          return CreateGroup (operation);
        }
      }
    }

    int ExtractProjectId (string groupId) => this.ExtractIdAfterPrefix (m_configuration.GroupCategoryPrefix, groupId);

    public bool Initialize ()
    {
      var result = LoadConfiguration (out m_configuration);
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        m_monitoredMachines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindAll ();
      }
      return result;
    }

    IGroup CreateGroup (IOperation Operation)
    {
      Debug.Assert (Operation is not null);

      var groupId = m_configuration.GroupCategoryPrefix + Operation.Id;
      var groupName = Operation.Display;
      return DynamicGroup.Create (groupId,
        groupName,
        m_configuration.GroupCategoryReference,
        () => GetMachines (Operation.Id),
        machines => GetMachines (Operation.Id, machines),
        null,
        false);
    }

    IEnumerable<IMachine> GetMachines (int operationId)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var machines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindAll ()
          .Cast<IMachine> ();
        return GetMachines (operationId, machines);
      }
    }

    IEnumerable<IMachine> GetMachines (int operationId, IEnumerable<IMachine> machines)
    {
      foreach (var machine in machines) {
        var businessRequest = new Lemoine.Business.Operation.EffectiveOperationCurrentShift (machine);
        IOperation operation;
        try {
          var effectiveOperationCurrentShift = Lemoine.Business.ServiceProvider
            .Get (businessRequest);
          operation = effectiveOperationCurrentShift.Operation;
        }
        catch (Exception ex) {
          log.Error ($"GetMachines: error when trying to get the operation of the current shift for machine {machine.Id}", ex);
          throw;
        }
        if (operation is not null && (operation.Id == operationId)) {
          yield return machine;
        }
      }
    }
  }
}
