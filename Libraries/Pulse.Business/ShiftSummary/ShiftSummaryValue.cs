// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lemoine.Model;
using Pulse.Extensions.Business.ShiftSummary;

namespace Lemoine.Business.ShiftSummary
{
  /// <summary>
  /// Item of a shift summary, as serialized in the database
  /// </summary>
  [Serializable]
  public sealed class ShiftSummaryItemDTO : IShiftActivityItem
  {
    /// <summary>
    /// <see cref="IShiftActivityItem"/>
    /// </summary>
    public string Kind { get; set; } = ShiftActivityKind.Other;

    /// <summary>
    /// <see cref="IShiftActivityItem"/>
    /// </summary>
    public string Section { get; set; }

    /// <summary>
    /// <see cref="IShiftActivityItem"/>
    /// </summary>
    public DateTime? Start { get; set; }

    /// <summary>
    /// <see cref="IShiftActivityItem"/>
    /// </summary>
    public DateTime? End { get; set; }

    /// <summary>
    /// <see cref="IShiftActivityItem"/>
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// <see cref="IShiftActivityItem"/>
    /// </summary>
    public int Importance { get; set; }

    /// <summary>
    /// <see cref="IShiftActivityItem"/>
    /// </summary>
    public string Text { get; set; } = "";

    /// <summary>
    /// <see cref="IShiftActivityItem"/>
    /// </summary>
    public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object> ();

    /// <summary>
    /// Copy of an item
    /// </summary>
    /// <param name="item">not null</param>
    /// <returns></returns>
    public static ShiftSummaryItemDTO From (IShiftActivityItem item) => new ShiftSummaryItemDTO {
      Kind = item.Kind,
      Section = item.Section,
      Start = item.Start,
      End = item.End,
      Duration = item.Duration,
      Importance = item.Importance,
      Text = item.Text ?? "",
      Properties = item.Properties ?? new Dictionary<string, object> (),
    };
  }

  /// <summary>
  /// Section of a shift summary, as serialized in the database
  /// </summary>
  [Serializable]
  public sealed class ShiftSummarySectionDTO
  {
    /// <summary>
    /// <see cref="IShiftSummarySection"/>
    /// </summary>
    public string Kind { get; set; } = ShiftActivityKind.Other;

    /// <summary>
    /// <see cref="IShiftSummarySection"/>
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// <see cref="IShiftSummarySection"/>
    /// </summary>
    public List<ShiftSummaryItemDTO> Items { get; set; } = new List<ShiftSummaryItemDTO> ();
  }

  /// <summary>
  /// Content of a shift summary, as serialized in the database
  /// </summary>
  [Serializable]
  public sealed class ShiftSummaryContentDTO
  {
    /// <summary>
    /// Sections
    /// </summary>
    public List<ShiftSummarySectionDTO> Sections { get; set; } = new List<ShiftSummarySectionDTO> ();

    /// <summary>
    /// Copy of a result
    /// </summary>
    /// <param name="result">not null</param>
    /// <returns></returns>
    public static ShiftSummaryContentDTO From (IShiftSummaryResult result) => new ShiftSummaryContentDTO {
      Sections = (result.Sections ?? new List<IShiftSummaryResultSection> ())
        .Select (s => new ShiftSummarySectionDTO {
          Kind = s.Kind,
          Title = s.Title ?? "",
          Items = (s.Items ?? new List<IShiftActivityItem> ()).Select (ShiftSummaryItemDTO.From).ToList (),
        })
        .ToList (),
    };
  }

  /// <summary>
  /// State of a section of a shift summary
  /// </summary>
  [Serializable]
  public sealed class ShiftSummarySectionStatusDTO
  {
    /// <summary>
    /// Key of the section
    /// </summary>
    public string Key { get; set; } = "";

    /// <summary>
    /// UTC date/time when the section was computed for the last time, null if never
    /// </summary>
    public DateTime? ComputedAt { get; set; }

    /// <summary>
    /// UTC date/time until which the data was processed when the section was computed
    /// </summary>
    public DateTime? DataUntil { get; set; }

    /// <summary>
    /// The section is to compute (again)
    /// </summary>
    public bool UpdateRequired { get; set; }
  }

  /// <summary>
  /// A shift summary, as a generator rendered it, as a plain value object detached from any
  /// database session
  ///
  /// It can be serialized in JSON: a client keeps it as the copy of what it showed,
  /// see <see cref="ToJson"/> and <see cref="FromJson(string)"/>
  /// </summary>
  [Serializable]
  public sealed class ShiftSummaryValue
  {
    /// <summary>
    /// JSON options of the data and of the content
    /// </summary>
    internal static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Id of the summary (table shiftsummary)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Id of the content that is rendered (table shiftsummarycontent)
    /// </summary>
    public int ContentId { get; set; }

    /// <summary>
    /// Version of the content: it changes each time it is rendered again
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Machine id
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Day
    /// </summary>
    public DateTime Day { get; set; }

    /// <summary>
    /// Shift id, null for the whole day
    /// </summary>
    public int? ShiftId { get; set; }

    /// <summary>
    /// Display of the shift, nullable
    /// </summary>
    public string ShiftDisplay { get; set; }

    /// <summary>
    /// UTC start of the range
    /// </summary>
    public DateTime? Start { get; set; }

    /// <summary>
    /// UTC end of the range
    /// </summary>
    public DateTime? End { get; set; }

    /// <summary>
    /// UTC range
    /// </summary>
    [JsonIgnore]
    public UtcDateTimeRange Range => new UtcDateTimeRange (new LowerBound<DateTime> (this.Start), new UpperBound<DateTime> (this.End));

    /// <summary>
    /// Key of the generator that rendered the content
    /// </summary>
    public string Generator { get; set; }

    /// <summary>
    /// UTC date/time when the content was rendered for the last time
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// The content was rendered from the current version of all the sections, and no
    /// section is to compute again
    /// </summary>
    public bool UpToDate { get; set; }

    /// <summary>
    /// Markdown text, nullable
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Rendered sections
    /// </summary>
    public IList<ShiftSummarySectionDTO> Sections { get; set; } = new List<ShiftSummarySectionDTO> ();

    /// <summary>
    /// State of the sections of the data
    /// </summary>
    public IList<ShiftSummarySectionStatusDTO> SectionStatus { get; set; } = new List<ShiftSummarySectionStatusDTO> ();

    /// <summary>
    /// Serialize it in JSON
    /// </summary>
    /// <returns></returns>
    public string ToJson () => JsonSerializer.Serialize (this, JsonOptions);

    /// <summary>
    /// Deserialize a value that <see cref="ToJson"/> serialized
    /// </summary>
    /// <param name="json">nullable</param>
    /// <returns>null if json is null or empty</returns>
    public static ShiftSummaryValue FromJson (string json) =>
      string.IsNullOrEmpty (json) ? null : JsonSerializer.Deserialize<ShiftSummaryValue> (json, JsonOptions);

    /// <summary>
    /// Versions of sections, as stored in <see cref="IShiftSummaryContent.SourceVersion"/>
    /// </summary>
    /// <param name="sections">not null</param>
    /// <returns>not null</returns>
    public static string GetSourceVersion (IEnumerable<IShiftSummarySection> sections) =>
      string.Join (";", sections
        .Where (x => x.DateTime.HasValue)
        .OrderBy (x => x.SectionKey, StringComparer.Ordinal)
        .Select (x => $"{x.SectionKey}:{x.Version}"));

    /// <summary>
    /// Build a value from a persistent content, inside its session
    /// </summary>
    /// <param name="content">not null</param>
    /// <param name="sections">sections of the summary, not null</param>
    /// <returns></returns>
    public static ShiftSummaryValue From (IShiftSummaryContent content, IList<IShiftSummarySection> sections)
    {
      var shiftSummary = content.ShiftSummary;
      var contentDTO = string.IsNullOrEmpty (content.Content)
        ? new ShiftSummaryContentDTO ()
        : JsonSerializer.Deserialize<ShiftSummaryContentDTO> (content.Content, JsonOptions) ?? new ShiftSummaryContentDTO ();
      return new ShiftSummaryValue {
        Id = shiftSummary.Id,
        ContentId = content.Id,
        Version = content.Version,
        MachineId = shiftSummary.Machine.Id,
        Day = shiftSummary.Day,
        ShiftId = shiftSummary.Shift?.Id,
        ShiftDisplay = shiftSummary.Shift?.Display,
        Start = shiftSummary.DateTimeRange.Lower.HasValue ? shiftSummary.DateTimeRange.Lower.Value : (DateTime?)null,
        End = shiftSummary.DateTimeRange.Upper.HasValue ? shiftSummary.DateTimeRange.Upper.Value : (DateTime?)null,
        Generator = content.Generator,
        DateTime = content.DateTime,
        UpToDate = !sections.Any (x => x.UpdateRequired)
          && string.Equals (content.SourceVersion, GetSourceVersion (sections), StringComparison.Ordinal),
        Text = content.Text,
        Sections = contentDTO.Sections,
        SectionStatus = sections
          .Select (x => new ShiftSummarySectionStatusDTO {
            Key = x.SectionKey,
            ComputedAt = x.DateTime,
            DataUntil = x.DataUntil,
            UpdateRequired = x.UpdateRequired,
          })
          .ToList (),
      };
    }
  }
}
