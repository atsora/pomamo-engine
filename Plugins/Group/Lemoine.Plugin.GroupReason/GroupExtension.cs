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

namespace Lemoine.Plugin.GroupReason
{
  public class GroupExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IGroupExtension
  {
    static readonly ILog log = LogManager.GetLogger (typeof (GroupExtension).FullName);

    Configuration m_configuration;
    IList<IReason> m_reasons = null;
    IList<IGroup> m_groups = null;

    public bool Initialize ()
    {
      if (!LoadConfiguration (out m_configuration)) {
        return false;
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        if (m_configuration.ReasonIds.Any ()) {
          m_reasons = new List<IReason> ();
          foreach (var reasonId in m_configuration.ReasonIds) {
            var reason = ModelDAOHelper.DAOFactory.ReasonDAO
              .FindById (reasonId);
            if (null == reason) {
              if (log.IsErrorEnabled) {
                log.ErrorFormat ("Initialize: reason with id {0} does not exist",
                  reasonId);
              }
            }
            else {
              m_reasons.Add (reason);
            }
          }
        }
        else {
          m_reasons = ModelDAOHelper.DAOFactory.ReasonDAO.FindAll ();
        }

        if (!m_reasons.Any ()) {
          if (log.IsErrorEnabled) {
            log.ErrorFormat ("Initialize: no valid reason selected, return false");
          }
          return false;
        }

        m_groups = new List<IGroup> ();
        foreach (var reason in m_reasons) {
          var group = new Group (m_configuration.GroupCategoryPrefix, reason);
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
      var reasonId = ExtractReasonId (groupId);
      return GetGroup (groupId, reasonId);
    }

    IGroup GetGroup (string groupId, int reasonId)
    {
      var groupFromGroups = this.GetGroupFromGroups (groupId);
      if (null != groupFromGroups) {
        return groupFromGroups;
      }
      else {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var reason = ModelDAOHelper.DAOFactory.ReasonDAO
            .FindById (reasonId);
          if (reason is null) {
            log.Error ($"GetGroup: no reason with id={reasonId} => return a group with no machine");
            return new GroupFromMachineList (groupId, $"Unknown reason {reasonId}", "Reason", new List<IMachine> { }, null, false, false);
          }
          else {
            return new Group (m_configuration.GroupCategoryPrefix, reason);
          }
        }
      }
    }

    int ExtractReasonId (string groupId) => this.ExtractIdAfterPrefix (m_configuration.GroupCategoryPrefix, groupId);
  }
}
