// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Lemoine.Core.Log;
using Lemoine.Model;
using Pulse.Extensions.Business.ShiftSummary;

namespace Lemoine.Business.ShiftSummary
{
  /// <summary>
  /// Default implementation of <see cref="IShiftActivityData"/>
  /// </summary>
  [Serializable]
  public sealed class ShiftActivityData : IShiftActivityData
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ShiftActivityData).FullName);

    /// <summary>
    /// <see cref="IShiftActivityData"/>
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// <see cref="IShiftActivityData"/>
    /// </summary>
    public string MachineDisplay { get; set; }

    /// <summary>
    /// <see cref="IShiftActivityData"/>
    /// </summary>
    public DateTime Day { get; set; }

    /// <summary>
    /// <see cref="IShiftActivityData"/>
    /// </summary>
    public int? ShiftId { get; set; }

    /// <summary>
    /// <see cref="IShiftActivityData"/>
    /// </summary>
    public string ShiftDisplay { get; set; }

    /// <summary>
    /// <see cref="IShiftActivityData"/>
    /// </summary>
    public UtcDateTimeRange Range { get; set; }

    /// <summary>
    /// <see cref="IShiftActivityData"/>
    /// </summary>
    public bool Completed { get; set; }

    /// <summary>
    /// <see cref="IShiftActivityData"/>
    /// </summary>
    public IList<IShiftActivityItem> Items { get; set; } = new List<IShiftActivityItem> ();

    /// <summary>
    /// Build the activity data of a summary from its stored sections: no data of the machine
    /// is read again, which makes it fast enough for any generator
    ///
    /// To be run inside a session
    /// </summary>
    /// <param name="shiftSummary">not null</param>
    /// <param name="sections">not null</param>
    /// <param name="now">UTC</param>
    /// <returns>not null</returns>
    public static ShiftActivityData FromSections (IShiftSummary shiftSummary, IEnumerable<IShiftSummarySection> sections, DateTime now)
    {
      var items = new List<IShiftActivityItem> ();
      foreach (var section in sections.Where (x => x.DateTime.HasValue)) {
        try {
          var sectionItems = string.IsNullOrEmpty (section.Items)
            ? new List<ShiftSummaryItemDTO> ()
            : JsonSerializer.Deserialize<List<ShiftSummaryItemDTO>> (section.Items, ShiftSummaryValue.JsonOptions) ?? new List<ShiftSummaryItemDTO> ();
          items.AddRange (sectionItems);
        }
        catch (Exception ex) {
          log.Error ($"FromSections: invalid items in section {section}, skip it", ex);
        }
      }
      return new ShiftActivityData {
        MachineId = shiftSummary.Machine.Id,
        MachineDisplay = shiftSummary.Machine.Display,
        Day = shiftSummary.Day,
        ShiftId = shiftSummary.Shift?.Id,
        ShiftDisplay = shiftSummary.Shift?.Display,
        Range = shiftSummary.DateTimeRange,
        Completed = shiftSummary.DateTimeRange.Upper.HasValue && (shiftSummary.DateTimeRange.Upper.Value <= now),
        Items = items
          .OrderByDescending (x => x.Importance)
          .ThenBy (x => x.Start ?? DateTime.MaxValue)
          .ToList (),
      };
    }
  }
}
