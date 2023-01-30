// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Model;
using Lemoine.Extensions.Business.Group;
using System.Diagnostics;

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// Response DTO for a group category
  /// </summary>
  [Api ("Group category DTO")]
  public class GroupCategoryDTO
  {
    ILog log = LogManager.GetLogger (typeof (GroupCategoryDTO).FullName);

    /// <summary>
    /// Display of the group
    /// </summary>
    public string Display { get; set; }

    /// <summary>
    /// Sort priority
    /// </summary>
    public double SortPriority { get; set; }

    /// <summary>
    /// Omit the group category in the machine selection
    /// </summary>
    public bool OmitGroupCategory { get; set; }

    /// <summary>
    /// Associated groups
    /// </summary>
    public List<GroupDTO> Groups { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="sortPriority"></param>
    public GroupCategoryDTO (string name, double sortPriority)
    {
      this.Display = name;
      this.SortPriority = sortPriority;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="sortPriority"></param>
    /// <param name="omitGroupCategory">omit the group category in the machine selection</param>
    /// <param name="zoom">Zoom in groups when available</param>
    /// <param name="groups">not null</param>
    /// <param name="filter">Optional: filter on machines</param>
    public GroupCategoryDTO (string name, double sortPriority, bool omitGroupCategory, bool zoom, IEnumerable<IGroup> groups, Func<IMachine, bool> filter = null)
      : this (name, sortPriority, omitGroupCategory, groups.Select (g => new GroupDTO (g, zoom, filter)))
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="sortPriority"></param>
    /// <param name="omitGroupCategory">omit the group category in the machine selection</param>
    /// <param name="groups">not null</param>
    public GroupCategoryDTO (string name, double sortPriority, bool omitGroupCategory, IEnumerable<GroupDTO> groups)
      : this (name, sortPriority)
    {
      Debug.Assert (null != groups);

      this.Groups = groups
        .OrderBy (g => g.Display)
        .OrderBy (g => g.SortPriority)
        .ToList ();
      if (omitGroupCategory) {
        if (1 == this.Groups.Count) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("GroupCategoryDTO: omit the group category, replace display {0} by {1}",
              this.Display, this.Groups.First ().Display);
          }
          this.OmitGroupCategory = true;
          this.Display = this.Groups.First ().Display;
        }
        else {
          log.ErrorFormat ("GroupCategoryDTO: omitGroupCategory option with {0} different groups",
            this.Groups.Count);
        }
      }
    }
  }
}
