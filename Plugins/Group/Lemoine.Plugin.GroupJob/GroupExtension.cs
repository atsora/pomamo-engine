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

namespace Lemoine.Plugin.GroupJob
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
        var jobs = new HashSet<IJob> ();

        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          foreach (var monitoredMachine in m_monitoredMachines) {
            var businessRequest = new Lemoine.Business.Operation.EffectiveOperationCurrentShift (monitoredMachine);
            IJob job = null;
            try {
              var effectiveOperationCurrentShift = Lemoine.Business.ServiceProvider
                .Get (businessRequest);
              var component = effectiveOperationCurrentShift.Component;
              if (component?.Project is not null) {
                var project = ModelDAOHelper.DAOFactory.ProjectDAO.FindById (component.Project.Id);
                if (project is null) {
                  log.Error ($"Groups.get: no project with id={component.Project.Id}");
                }
                job = project?.Job;
              }
            }
            catch (Exception ex) {
              log.Error ($"Groups.get: error when trying to get the operation of the current shift for machine {monitoredMachine.Id}", ex);
              throw;
            }
            if (job is not null) {
              jobs.Add (job);
            }
          }

          foreach (var job in jobs.OrderBy (x => x.Display)) {
            yield return CreateGroup (job);
          }
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

    IGroup GetGroup (string groupId, int projectId)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var job = ModelDAOHelper.DAOFactory.JobDAO
          .FindByProjectId (projectId);
        if (job is null) {
          log.Error ($"GetGroup: no job with projectId={projectId} => return a group with no machine");
          return DynamicGroup.Create (groupId,
            $"Unknown job {projectId}",
            m_configuration.GroupCategoryReference,
            () => new List<IMachine> (),
            machines => new List<IMachine> (),
            null,
            false);
        }
        else {
          return CreateGroup (job);
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

    IGroup CreateGroup (IJob job)
    {
      Debug.Assert (job is not null);

      var groupId = m_configuration.GroupCategoryPrefix + job.ProjectId;
      var groupName = job.Display;
      return DynamicGroup.Create (groupId,
        groupName,
        m_configuration.GroupCategoryReference,
        () => GetMachines (job.ProjectId),
        machines => GetMachines (job.ProjectId, machines),
        null,
        false);
    }

    IEnumerable<IMachine> GetMachines (int projectId)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var machines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindAll ()
          .Cast<IMachine> ();
        return GetMachines (projectId, machines);
      }
    }

    IEnumerable<IMachine> GetMachines (int projectId, IEnumerable<IMachine> machines)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        foreach (var machine in machines) {
          var businessRequest = new Lemoine.Business.Operation.EffectiveOperationCurrentShift (machine);
          IJob job = null;
          try {
            var effectiveOperationCurrentShift = Lemoine.Business.ServiceProvider
              .Get (businessRequest);
            var component = effectiveOperationCurrentShift.Component;
            if (component?.Project is not null) {
              var project = ModelDAOHelper.DAOFactory.ProjectDAO.FindById (component.Project.Id);
              if (project is null) {
                log.Error ($"GetMachines: no project with id={component.Project.Id}");
              }
              job = project?.Job;
            }
          }
          catch (Exception ex) {
            log.Error ($"GetMachines: error when trying to get the operation of the current shift for machine {machine.Id}", ex);
            throw;
          }
          if (job is not null && (job.ProjectId == projectId)) {
            yield return machine;
          }
        }
      }
    }
  }
}
