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

namespace Lemoine.Plugin.GroupZoomZoomable
{
  public class GroupZoomExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IGroupZoomExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (GroupZoomExtension).FullName);

    bool m_dynamic = true;
    Configuration m_configuration;
    Regex m_parentRegex;

    public bool Dynamic
    {
      get { return m_dynamic; }
    }

    public bool Initialize ()
    {
      if (!LoadConfiguration (out m_configuration)) {
        return false;
      }

      m_parentRegex = MakeRegex (m_configuration.ParentRegex);

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

      var parentGroupBusinessRequest = new Lemoine.Business.Machine
        .GroupFromId (parentGroupId);
      var parentGroup = Lemoine.Business.ServiceProvider
        .Get (parentGroupBusinessRequest);
      if (null == parentGroup) {
        children = new List<string> ();
        return false;
      }

      if (parentGroup is IZoomableGroup) {
        var zoomableGroup = parentGroup as IZoomableGroup;
        children = zoomableGroup.SubGroups
          .Select (g => g.Id);
        m_dynamic = parentGroup.Dynamic;
        return true;
      }
      else {
        if (log.IsErrorEnabled) {
          log.ErrorFormat ("ZoomIn: group {0} is not a IZoomable group", parentGroupId);
        }
        children = new List<string> ();
        return false;
      }
    }

    public bool ZoomOut (string childGroupId, out string parentGroupId)
    {
      var groupExtensions = Lemoine.Business.ServiceProvider
  .Get (new Lemoine.Business.Extension.GlobalExtensions<IGroupExtension> (ext => ext.Initialize ()));
      var groups = groupExtensions
        .SelectMany (ext => ext.Groups);
      if (!groups.Any ()) {
        if (log.IsWarnEnabled) {
          log.WarnFormat ("Initialize: no group is defined");
        }
        parentGroupId = null;
        return false;
      }

      var zoomableGroups = groups
        .Where (g => g is IZoomableGroup);
      foreach (var zoomableGroup in zoomableGroups) {
        var zoomable = zoomableGroup as IZoomableGroup;
        if (zoomable.SubGroups.Any (g => g.Id.Equals (childGroupId, StringComparison.InvariantCultureIgnoreCase))) {
          parentGroupId = zoomableGroup.Id;
          m_dynamic = zoomableGroup.Dynamic;
          return true;
        }
      }

      parentGroupId = null;
      return false;
    }
  }
}
