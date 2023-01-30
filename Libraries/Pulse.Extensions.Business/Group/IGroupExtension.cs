// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Lemoine.Core.Log;

namespace Lemoine.Extensions.Business.Group
{
  /// <summary>
  /// Result of GetGroupIdMatch
  /// </summary>
  public enum GroupIdExtensionMatch
  {
    /// <summary>
    /// The group id does not match the group extension
    /// </summary>
    No = 0,
    /// <summary>
    /// The group id matches the group extension and there is probably a static group that is associated to it
    /// </summary>
    Yes = 1,
    /// <summary>
    /// The group id matches the group extension rules but there is no group associated to it
    /// </summary>
    Empty = 2,
    /// <summary>
    /// The group id matches the group extension rules and the extension defines dynamic groups
    /// </summary>
    Dynamic = 3,
  }

  /// <summary>
  /// New extension (plugin interface) to create custom machine groups
  /// </summary>
  public interface IGroupExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialize the plugin. If false is returned, do not consider the plugin
    /// </summary>
    /// <returns></returns>
    bool Initialize ();

    /// <summary>
    /// Group category name
    /// 
    /// If null or empty, and if OmitCategory is false, 
    /// do not reference this new set of groups in category
    /// </summary>
    string GroupCategoryName { get; }

    /// <summary>
    /// Group category sort priority.
    /// 
    /// Group categories with the lowest sort priority are considered first
    /// </summary>
    double GroupCategorySortPriority { get; }

    /// <summary>
    /// If OmitCategory is true, then the groups are directly added 
    /// in the root level of the machine selection
    /// </summary>
    bool OmitGroupCategory { get; }

    /// <summary>
    /// Omit this group extension in the machine selection
    /// </summary>
    bool OmitInMachineSelection { get; }

    /// <summary>
    /// Associated groups
    /// </summary>
    IEnumerable<IGroup> Groups { get; }

    /// <summary>
    /// Does the group id match this extensions ?
    /// </summary>
    GroupIdExtensionMatch GetGroupIdExtensionMatch (string groupId);

    /// <summary>
    /// Get the group with the specified groupId
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
    IGroup GetGroup (string groupId);
  }

  /// <summary>
  /// Extension to interface <see cref="IGroupExtension">
  /// </summary>
  public static class GroupExtensionExtension
  {
    static readonly ILog log = LogManager.GetLogger (typeof (GroupExtensionExtension).FullName);

    /// <summary>
    /// Implementation of <see cref="IGroupExtension.GetGroupIdExtensionMatch(string)">
    /// when the group id is made of a prefix a number
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="groupId"></param>
    /// <returns></returns>
    public static bool IsGroupIdMatchFromPrefixNumber (this IGroupExtension groupExtension, string prefix, string groupId)
    {
      if (string.IsNullOrEmpty (prefix)) {
        return int.TryParse (groupId, out var _);
      }
      else if (!groupId.StartsWith (prefix)) {
        return false;
      }
      else {
        return Regex.IsMatch (groupId, $"{prefix}\\d+");
      }
    }

    /// <summary>
    /// Implementation of <see cref="IGroupExtension.GetGroupIdExtensionMatch(string)"/>
    /// using the list of groups
    /// </summary>
    /// <param name="groupExtension"></param>
    /// <param name="groupId"></param>
    /// <returns></returns>
    public static bool IsGroupIdMatchFromGroups (this IGroupExtension groupExtension, string groupId)
    {
      return groupExtension.Groups
        .Any (g => g.Id.Trim ().Trim ().Equals (groupId, StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    /// Get the group from a static Groups property
    /// </summary>
    /// <param name="groupExtension"></param>
    /// <param name="groupId"></param>
    /// <returns></returns>
    public static IGroup GetGroupFromGroups (this IGroupExtension groupExtension, string groupId)
    {
      return groupExtension.Groups
        .Where (g => g.Id.Trim ().Equals (groupId, StringComparison.InvariantCultureIgnoreCase))
        .FirstOrDefault ();
    }

    /// <summary>
    /// Extract the Id that is found after the prefix
    /// </summary>
    /// <param name="groupExtension"></param>
    /// <param name="prefix"></param>
    /// <param name="groupId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static int ExtractIdAfterPrefix (this IGroupExtension groupExtension, string prefix, string groupId)
    {
      if (!groupId.StartsWith (prefix)) {
        log.Fatal ($"ExtractIdAfterPrefix: invalid {groupId} that does not start with prefix={prefix}, which is unexpected");
        Debug.Assert (groupId.StartsWith (prefix));
        throw new InvalidOperationException ("groupId with the wrong prefix");
      }

      var projectIdString = groupId.Substring (prefix.Length);
      return int.Parse (projectIdString);
    }

  }
}
