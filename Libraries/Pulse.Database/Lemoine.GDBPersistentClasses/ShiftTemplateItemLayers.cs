// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Set of <see cref="IShiftTemplateItem"/> paths that share the same restriction criteria
  /// and that are applied together, once the impacted week or day has been reset
  /// </summary>
  internal class ShiftTemplateItemLayer
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="priority">0: no restriction, 1: a specific week, 2: a specific day</param>
    /// <param name="paths">not empty</param>
    public ShiftTemplateItemLayer (int priority, IList<IList<IShiftTemplateItem>> paths)
    {
      this.Priority = priority;
      this.Paths = paths;
    }

    /// <summary>
    /// 0: no restriction, 1: a specific week, 2: a specific day
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Item paths of the layer, each one ending on an item that references a shift
    /// </summary>
    public IList<IList<IShiftTemplateItem>> Paths { get; }
  }

  /// <summary>
  /// Flatten the items of a <see cref="IShiftTemplate"/>, an item that references another
  /// shift template being replaced by the paths that lead to the items of the referenced
  /// template, and group them in layers that are applied one after the other
  /// </summary>
  internal static class ShiftTemplateItemLayers
  {
    /// <summary>
    /// Maximum number of nested shift templates that may be applied recursively
    /// </summary>
    static readonly string MAX_RECURSION_DEPTH_KEY = "ShiftTemplate.Process.MaxRecursionDepth";
    static readonly int MAX_RECURSION_DEPTH_DEFAULT = 10;

    static readonly ILog log = LogManager.GetLogger (typeof (ShiftTemplateItemLayers).FullName);

    /// <summary>
    /// Get the layers of a shift template, using the configured maximum recursion depth
    /// </summary>
    /// <param name="shiftTemplate">not null</param>
    /// <returns></returns>
    internal static IList<ShiftTemplateItemLayer> GetLayers (IShiftTemplate shiftTemplate)
    {
      var maxRecursionDepth = Lemoine.Info.ConfigSet
        .LoadAndGet<int> (MAX_RECURSION_DEPTH_KEY, MAX_RECURSION_DEPTH_DEFAULT);
      return GetLayers (shiftTemplate, maxRecursionDepth);
    }

    /// <summary>
    /// Get the layers of a shift template
    ///
    /// The returned layers are sorted so that the ones that are applied last override
    /// the previous ones: first the items with neither a specific week nor a specific day,
    /// then the ones that are restricted to a specific week, by increasing reference week,
    /// then the ones that are restricted to a specific day.
    /// The order of the items with the same criteria is not deterministic,
    /// because the items of a shift template are not ordered. But because they belong
    /// to the same layer, only the overlapping ones are impacted
    ///
    /// Note: when an item path is restricted by several criteria, for example when both
    /// an item that references a shift template and one of its items are restricted,
    /// the most specific criterion is considered
    /// </summary>
    /// <param name="shiftTemplate">not null</param>
    /// <param name="maxRecursionDepth">maximum number of nested shift templates</param>
    /// <returns></returns>
    internal static IList<ShiftTemplateItemLayer> GetLayers (IShiftTemplate shiftTemplate, int maxRecursionDepth)
    {
      if (shiftTemplate is null) {
        log.Fatal ("GetLayers: shiftTemplate is null");
        throw new ArgumentNullException (nameof (shiftTemplate));
      }

      var paths = new List<IList<IShiftTemplateItem>> ();
      AddItemPaths (shiftTemplate, new List<IShiftTemplateItem> (),
                    new List<int> { shiftTemplate.Id }, maxRecursionDepth, paths);
      return paths
        .GroupBy (p => (Priority: p.Max (i => i.GetPriority ()), WeekSortKey: p.Max (i => i.GetWeekSortKey ())))
        .OrderBy (g => g.Key.Priority)
        .ThenBy (g => g.Key.WeekSortKey)
        .Select (g => new ShiftTemplateItemLayer (g.Key.Priority, g.ToList<IList<IShiftTemplateItem>> ()))
        .ToList ();
    }

    /// <summary>
    /// Add to <paramref name="paths"/> the item paths of a shift template,
    /// applying recursively the referenced shift templates
    /// </summary>
    /// <param name="shiftTemplate">not null</param>
    /// <param name="parentPath">items that lead to this shift template</param>
    /// <param name="ancestorTemplateIds">ids of the shift templates that are being flattened, to detect the cycles</param>
    /// <param name="maxRecursionDepth"></param>
    /// <param name="paths"></param>
    static void AddItemPaths (IShiftTemplate shiftTemplate,
                              IList<IShiftTemplateItem> parentPath,
                              IList<int> ancestorTemplateIds,
                              int maxRecursionDepth,
                              IList<IList<IShiftTemplateItem>> paths)
    {
      foreach (var item in shiftTemplate.Items) {
        var path = new List<IShiftTemplateItem> (parentPath) { item };

        if (null == item.SubShiftTemplate) {
          if (null == item.Shift) {
            log.Error ($"AddItemPaths: item {item} references neither a shift nor a shift template => skip it");
            continue;
          }
          paths.Add (path);
          continue;
        }

        // The item applies recursively another shift template.
        // Note: only the id of the sub shift template is logged below, so that a lazy proxy
        // is not initialized just to build an error message
        if (ancestorTemplateIds.Contains (item.SubShiftTemplate.Id)) {
          log.Error ($"AddItemPaths: the shift template {item.SubShiftTemplate.Id} is already being applied, there is a cycle in the shift templates => skip it");
          continue;
        }
        if (maxRecursionDepth <= ancestorTemplateIds.Count) {
          log.Error ($"AddItemPaths: the maximum recursion depth {maxRecursionDepth} is reached with the shift template {item.SubShiftTemplate.Id} => skip it");
          continue;
        }

        var subAncestorTemplateIds = new List<int> (ancestorTemplateIds) { item.SubShiftTemplate.Id };
        AddItemPaths (item.SubShiftTemplate, path, subAncestorTemplateIds, maxRecursionDepth, paths);
      }
    }

    /// <summary>
    /// Does the specified item path reset the data of <paramref name="localDate"/>
    /// before the shifts of its layer are applied ?
    ///
    /// Contrarily to <see cref="IShiftTemplateItemExtensions.IsDayApplicable"/>, the week days
    /// and the time periods of the day are not considered here: a path that is restricted
    /// to a specific week resets the whole week, even the days on which it defines no shift
    /// </summary>
    /// <param name="itemPath">not empty</param>
    /// <param name="localDate">local date</param>
    /// <param name="weekYear">week year of the associated day slot</param>
    /// <param name="weekNumber">week number of the associated day slot</param>
    /// <returns></returns>
    internal static bool IsResetApplicable (IList<IShiftTemplateItem> itemPath, DateTime localDate, int weekYear, int weekNumber)
    {
      if (!itemPath.All (i => i.IsWeekApplicable (weekYear, weekNumber))) {
        return false;
      }

      // Only the specified day is reset when the path is restricted to a specific day.
      // Else the whole week is reset
      return itemPath
        .Where (i => i.Day.HasValue)
        .All (i => i.Day.Value.Date.Equals (localDate.Date));
    }

    /// <summary>
    /// Does this item restrict the dates on which it is applicable,
    /// or the time period of the day ?
    /// </summary>
    /// <param name="item">not null</param>
    /// <returns></returns>
    internal static bool IsDateRestricted (IShiftTemplateItem item)
    {
      return item.Day.HasValue
        || item.WeekNumber.HasValue
        || !item.TimePeriod.IsFullDay ();
    }
  }
}
