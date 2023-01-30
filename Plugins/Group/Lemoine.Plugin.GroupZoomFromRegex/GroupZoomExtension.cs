// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Group;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.GroupZoomFromRegex
{
  public class GroupZoomExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IGroupZoomExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (GroupZoomExtension).FullName);

    static readonly string MAX_NUMBER_SEPARATORS_KEY = "Plugin.GroupZoomFromRegex.MaxNumberSeparators";
    static readonly int MAX_NUMBER_SEPARATORS_DEFAULT = 3;

    static readonly char SEPARATOR_CHAR = '_';
    static readonly string SEPARATOR = SEPARATOR_CHAR.ToString ();

    static readonly string PARENT_GROUP_ID_KEYWORD = "{ParentGroupId}";

    IEnumerable<IGroup> m_groups = null;
    bool m_dynamic = true;
    Configuration m_configuration;
    Regex m_parentRegex;
    Regex m_genericChildrenRegex; // it does not contain {ParentGroupId}

    public bool Dynamic
    {
      get { return m_dynamic; }
    }

    public bool Initialize ()
    {
      var groupExtensions = Lemoine.Business.ServiceProvider
  .Get (new Lemoine.Business.Extension.GlobalExtensions<IGroupExtension> (ext => ext.Initialize ()));
      m_groups = groupExtensions
        .SelectMany (ext => ext.Groups);
      if (!m_groups.Any ()) {
        if (log.IsWarnEnabled) {
          log.WarnFormat ("Initialize: no group is defined");
        }
        return false;
      }

      if (!LoadConfiguration (out m_configuration)) {
        return false;
      }

      m_parentRegex = MakeRegex (m_configuration.ParentRegex);
      if (!m_configuration.ChildrenRegex.Contains (PARENT_GROUP_ID_KEYWORD)) {
        m_genericChildrenRegex = MakeRegex (m_configuration.ChildrenRegex);
      }
      else {
        m_genericChildrenRegex = null;
      }

      return true;
    }

    Regex MakeRegex (string r)
    {
      var regexOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant;
      return new Regex ("^" + r + "$", regexOptions);
    }

    public bool ZoomIn (string parentGroupId, out IEnumerable<string> children)
    {
      if (!m_parentRegex.IsMatch (parentGroupId)) {
        children = new List<string> ();
        return false;
      }

      // Note: the business class here is used to support groups with a separator '_'
      var parentGroupBusinessRequest = new Lemoine.Business.Machine
        .GroupFromId (parentGroupId);
      var parentGroup = Lemoine.Business.ServiceProvider
        .Get (parentGroupBusinessRequest);
      if (null == parentGroup) {
        children = new List<string> ();
        return false;
      }

      var childrenRegex = GetChildrenRegex (parentGroupId);
      var childrenGroups = GetGroupsFromRegex (childrenRegex, m_configuration.IncludeVirtualChildren, m_configuration.ChildrenRegex, parentGroupId);
      if (!childrenGroups.Any ()) {
        children = new List<string> ();
        return false;
      }

      m_dynamic = parentGroup.Dynamic;
      var childrenList = new HashSet<string> ();
      children = childrenList;
      var parentMachines = parentGroup.GetMachines ();
      if (!parentMachines.Any ()) {
        return true;
      }
      var equalityComparer =
        new LambdaEqualityComparer<IMachine> ((a, b) => a.Id == b.Id, x => x.Id);
      foreach (var childGroup in childrenGroups) {
        var childMachines = childGroup.GetMachines ();
        if (childMachines.Any ()) {
          var intersection = childMachines.Intersect (parentMachines, equalityComparer);
          if (intersection.Any ()) {
            m_dynamic |= childGroup.Dynamic;
            if (!parentGroup.Dynamic && !childGroup.Dynamic
              && childMachines.All (x => intersection.Contains (x, equalityComparer))) {
              childrenList.Add (childGroup.Id);
            }
            else {
              childrenList.Add (parentGroupId + "_" + childGroup.Id);
            }
          } // intersection.Any ()
        } // childMachines.Any ()
      } // Loop

      return true;
    }

    Regex GetChildrenRegex (string parentGroupId)
    {
      if (null != m_genericChildrenRegex) {
        return m_genericChildrenRegex;
      }

      if (m_configuration.ChildrenRegex.Contains (PARENT_GROUP_ID_KEYWORD)) {
        var modifiedRegex = m_configuration.ChildrenRegex
          .Replace (PARENT_GROUP_ID_KEYWORD, parentGroupId);
        return MakeRegex (modifiedRegex);
      }
      else {
        m_genericChildrenRegex = MakeRegex (m_configuration.ChildrenRegex);
        return m_genericChildrenRegex;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="regex"></param>
    /// <param name="virtualGroups">Include the virtual groups in the list of returned groups (or only the real groups)</param>
    /// <param name="rawRegex">Raw regex that may contain {ParentGroupId}</param>
    /// <param name="parentGroupId">Group id to replace in {ParentGroupId}</param>
    /// <returns></returns>
    IEnumerable<IGroup> GetGroupsFromRegex (Regex regex, bool virtualGroups, string rawRegex, string parentGroupId)
    {
      var groups = m_groups
        .Where (g => regex.IsMatch (g.Id));

      if (virtualGroups && regex.ToString ().Contains (SEPARATOR)) {
        var maxNumberSeparators = regex.ToString ()
          .Count (c => c.Equals (SEPARATOR_CHAR));
        for (int i = 2; i <= maxNumberSeparators + 1; ++i) {
          var combinedGroups = new List<IGroup> ();
          var combinedGroupIds = GetCombinedGroupIds (i, rawRegex, parentGroupId);
          var matchingCombinedGroupIds = combinedGroupIds
            .Where (x => regex.IsMatch (x));
          foreach (var combinedGroupId in matchingCombinedGroupIds) {
            var combinedGroupRequest = new Lemoine.Business.Machine
              .GroupFromId (combinedGroupId);
            var combinedGroup = Lemoine.Business.ServiceProvider
              .Get (combinedGroupRequest);
            if (null != combinedGroup) {
              if (combinedGroup.Dynamic) {
                combinedGroups.Add (combinedGroup);
              }
              else { // !Dynamic
                var machines = combinedGroup.GetMachines ();
                if (machines.Any ()) {
                  combinedGroups.Add (combinedGroup);
                } // machines.Any ()
              } // Dynamic
            } // null != combinedGroup
          } // Loop on matchingCombinedGroupIds
          groups = groups.Concat (combinedGroups);
        } // Loop
      } // regex contains SEPARATOR

      return groups;
    }

    public bool ZoomOut (string childGroupId, out string parentGroupId)
    {
      Regex childrenRegex;
      if (null != m_genericChildrenRegex) {
        childrenRegex = m_genericChildrenRegex;
      }
      else {
        if (m_configuration.ChildrenRegex.Contains (PARENT_GROUP_ID_KEYWORD)) {
          var modifiedRegex = m_configuration.ChildrenRegex
            .Replace (PARENT_GROUP_ID_KEYWORD, "(" + m_configuration.ParentRegex + ")");
          childrenRegex = MakeRegex (modifiedRegex);
        }
        else {
          m_genericChildrenRegex = MakeRegex (m_configuration.ChildrenRegex);
          childrenRegex = m_genericChildrenRegex;
        }
      }

      if (!childrenRegex.IsMatch (childGroupId)) {
        parentGroupId = null;
        return false;
      }

      // Note: the business class here is used to support groups with a separator '_'
      var childGroupBusinessRequest = new Lemoine.Business.Machine
        .GroupFromId (childGroupId);
      var childGroup = Lemoine.Business.ServiceProvider
        .Get (childGroupBusinessRequest);
      if (null == childGroup) {
        parentGroupId = null;
        return false;
      }

      var childMachines = childGroup.GetMachines ();
      if (!childMachines.Any ()) {
        parentGroupId = null;
        return false;
      }

      m_dynamic = childGroup.Dynamic;
      var equalityComparer =
        new LambdaEqualityComparer<IMachine> ((a, b) => a.Id == b.Id, x => x.Id);

      var parentGroups1 = m_groups
        .Where (g => m_parentRegex.IsMatch (g.Id));
      foreach (var parentGroup in parentGroups1) {
        var parentMachines = parentGroup.GetMachines ();
        if (parentMachines.Any ()) {
          if (childMachines.All (x => parentMachines.Contains (x, equalityComparer))) {
            m_dynamic |= parentGroup.Dynamic;
            parentGroupId = parentGroup.Id;
            return true;
          }
        } // parentMachines.Any ()
      } // Loop

      // No parent group without separator found
      // Try with one
      if (m_configuration.ParentRegex.Contains (SEPARATOR)) {
        var maxNumberSeparators = Lemoine.Info.ConfigSet
          .LoadAndGet (MAX_NUMBER_SEPARATORS_KEY, MAX_NUMBER_SEPARATORS_DEFAULT);
        for (int i = 2; i <= maxNumberSeparators + 1; ++i) {
          var combinedGroupIds = GetCombinedGroupIds (i);
          var matchingCombinedGroupIds = combinedGroupIds
            .Where (x => m_parentRegex.IsMatch (x));
          foreach (var combinedGroupId in matchingCombinedGroupIds) {
            var combinedGroupRequest = new Lemoine.Business.Machine
              .GroupFromId (combinedGroupId);
            var combinedGroup = Lemoine.Business.ServiceProvider
              .Get (combinedGroupRequest);
            if (null != combinedGroup) {
              var machines = combinedGroup.GetMachines ();
              if (machines.Any ()) {
                if (childMachines.All (x => machines.Contains (x, equalityComparer))) {
                  m_dynamic |= combinedGroup.Dynamic;
                  parentGroupId = combinedGroupId;
                  return true;
                }
              } // machines.Any ()
            } // null != combinedGroup
          } // Loop on matchingCombinedGroupIds
        } // Loop
      }

      parentGroupId = null;
      return false;
    }

    IEnumerable<string> GetCombinedGroupIds (int number, string rawRegex, string parentGroupId)
    {
      if (!string.IsNullOrEmpty (parentGroupId)
        && rawRegex.StartsWith (PARENT_GROUP_ID_KEYWORD + "_")) {
        return GetCombinedGroupIds (number - 1)
          .Select (x => parentGroupId + SEPARATOR + x);
      }
      else {
        return GetCombinedGroupIds (number);
      }
    }

    IEnumerable<string> GetCombinedGroupIds (int number)
    {
      if (0 == number) {
        return new List<string> { };
      }
      else if (1 == number) {
        return m_groups.Select (g => g.Id);
      }
      else {
        var a = GetCombinedGroupIds (1);
        var b = GetCombinedGroupIds (number - 1);
        return a.SelectMany (x => b.Select (y => x + SEPARATOR + y));
      }
    }
  }
}
