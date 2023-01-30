// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Lemoine.Core.Log;

namespace Lemoine.Extensions.Business.Group
{
  /// <summary>
  /// Extension to define a zoom in or out between groups
  /// </summary>
  public interface IGroupZoomExtension : IExtension
  {
    /// <summary>
    /// Initialize the plugin. If false is returned, do not consider the plugin
    /// </summary>
    /// <returns></returns>
    bool Initialize ();

    /// <summary>
    /// Is the relationship between the groups dynamic or static ?
    /// </summary>
    bool Dynamic { get; }

    /// <summary>
    /// Zoom in
    /// </summary>
    /// <param name="parentGroupId"></param>
    /// <param name="children">only if true is returned</param>
    /// <returns>the plugin implements it</returns>
    bool ZoomIn (string parentGroupId, out IEnumerable<string> children);

    /// <summary>
    /// Zoom out
    /// </summary>
    /// <param name="childGroupId"></param>
    /// <param name="parentGroupId">only it true is returned</param>
    /// <returns>the plugin implements it</returns>
    bool ZoomOut (string childGroupId, out string parentGroupId);
  }

  public static class GroupZoomExtensionExtension
  {
    static readonly ILog log = LogManager.GetLogger (typeof (GroupZoomExtensionExtension).FullName);

    /// <summary>
    /// Check if a group id is made of a prefix a number
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="groupId"></param>
    /// <returns></returns>
    public static bool IsGroupIdMatchFromPrefixNumber (this IGroupZoomExtension groupZoomExtension, string prefix, string groupId)
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
    /// Extract the Id after the prefix
    /// </summary>
    /// <param name="groupZoomExtension"></param>
    /// <param name="prefix"></param>
    /// <param name="groupId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static int ExtractIdAfterPrefix (this IGroupZoomExtension groupZoomExtension, string prefix, string groupId)
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
