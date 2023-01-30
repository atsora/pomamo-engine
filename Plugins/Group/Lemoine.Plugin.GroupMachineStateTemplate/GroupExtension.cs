// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Group;
using Lemoine.Extensions.Business.Group.Impl;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Plugin.GroupMachineStateTemplate
{
  public class GroupExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IGroupExtension
  {
    static readonly ILog log = LogManager.GetLogger (typeof (GroupExtension).FullName);

    Configuration m_configuration;
    IList<IMachineStateTemplate> m_machineStateTemplates = null;
    IList<IGroup> m_groups = null;

    public bool Initialize ()
    {
      if (!LoadConfiguration (out m_configuration)) {
        return false;
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        m_machineStateTemplates = new List<IMachineStateTemplate> ();
        foreach (var machineStateTemplateId in m_configuration.MachineStateTemplateIds) {
          var machineStateTemplate = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
            .FindById (machineStateTemplateId);
          if (null == machineStateTemplate) {
            if (log.IsErrorEnabled) {
              log.Error ($"Initialize: machine state template with id {machineStateTemplateId} does not exist");
            }
          }
          else {
            m_machineStateTemplates.Add (machineStateTemplate);
          }
        }

        if (!m_machineStateTemplates.Any ()) {
          if (log.IsErrorEnabled) {
            log.ErrorFormat ("Initialize: no valid machine state template selected, return false");
          }
          return false;
        }

        m_groups = new List<IGroup> ();
        foreach (var machineStateTemplate in m_machineStateTemplates) {
          var group = new Group (m_configuration.GroupCategoryPrefix, machineStateTemplate);
          m_groups.Add (group);
        }
      }

      return true;
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
      get { return m_groups; }
    }

    public GroupIdExtensionMatch GetGroupIdExtensionMatch (string groupId)
    {
      return this.IsGroupIdMatchFromPrefixNumber (m_configuration.GroupCategoryPrefix, groupId)
        ? GroupIdExtensionMatch.Yes
        : GroupIdExtensionMatch.No;
    }

    public IGroup GetGroup (string groupId)
    {
      var machineStateTemplateId = ExtractMachineStateTemplateId (groupId);
      return GetGroup (groupId, machineStateTemplateId);
    }

    IGroup GetGroup (string groupId, int machineStateTemplateId)
    {
      var groupFromGroups = this.GetGroupFromGroups (groupId);
      if (null != groupFromGroups) {
        return groupFromGroups;
      }
      else {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var machineStateTemplate = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
            .FindById (machineStateTemplateId);
          if (machineStateTemplate is null) {
            log.Error ($"GetGroup: no machine state template with id={machineStateTemplateId} => return a group with no machine");
            return new GroupFromMachineList (groupId, $"Unknown machine state template {machineStateTemplateId}", "MachineStateTemplate", new List<IMachine> { }, null, false, false);
          }
          else {
            return new Group (m_configuration.GroupCategoryPrefix, machineStateTemplate);
          }
        }
      }
    }

    int ExtractMachineStateTemplateId (string groupId) => this.ExtractIdAfterPrefix (m_configuration.GroupCategoryPrefix, groupId);
  }
}
