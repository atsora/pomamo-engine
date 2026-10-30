// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table shiftsummary
  ///
  /// Highlights of what happened on a machine during a shift (or a day when no shift is
  /// defined). The detailed figures (operations, utilization, ...) are in the other summary
  /// tables.
  ///
  /// It is created by the analysis service a little before the end of the shift. Its data is
  /// split in sections (<see cref="IShiftSummarySection"/>), each one computed separately,
  /// and rendered by one or several generators (<see cref="IShiftSummaryContent"/>).
  /// Since the data is processed asynchronously, a section may be computed again after the
  /// end of the shift.
  ///
  /// There is at most one summary per machine, day and shift.
  /// </summary>
  public interface IShiftSummary : IVersionable, Lemoine.Collections.IDataWithId
  {
    /// <summary>
    /// Machine
    ///
    /// not null
    /// </summary>
    IMachine Machine { get; }

    /// <summary>
    /// Day
    /// </summary>
    DateTime Day { get; }

    /// <summary>
    /// Shift
    ///
    /// nullable: no shift is defined, the summary is the one of the whole day
    /// </summary>
    IShift Shift { get; }

    /// <summary>
    /// UTC date/time range that is summarized
    /// </summary>
    UtcDateTimeRange DateTimeRange { get; set; }

    /// <summary>
    /// UTC date/time when the summary was created
    /// </summary>
    DateTime DateTime { get; }
  }

  /// <summary>
  /// Model of table shiftsummarysection
  ///
  /// Data of a shift summary that comes from a contributor (an implementation of
  /// IShiftActivityExtension): the longest stops, the CNC alarms, the operator tasks, ...
  ///
  /// A section is computed again when it is flagged (<see cref="UpdateRequired"/>): by the
  /// contributor itself when some relevant data changes (accumulator, import extension,
  /// plugin), or periodically around the end of the shift for the data that has no change
  /// notification.
  /// </summary>
  public interface IShiftSummarySection : IVersionable, Lemoine.Collections.IDataWithId
  {
    /// <summary>
    /// Shift summary, not null
    /// </summary>
    IShiftSummary ShiftSummary { get; }

    /// <summary>
    /// Key of the section, unique for a summary, for example Default.Stop
    ///
    /// not null and not empty
    /// </summary>
    string SectionKey { get; }

    /// <summary>
    /// Items of the section, in JSON
    /// </summary>
    string Items { get; set; }

    /// <summary>
    /// UTC date/time until which the data was processed when the section was computed
    /// </summary>
    DateTime? DataUntil { get; set; }

    /// <summary>
    /// The section must be computed (again)
    /// </summary>
    bool UpdateRequired { get; set; }

    /// <summary>
    /// UTC date/time when the section was computed for the last time, null if never
    /// </summary>
    DateTime? DateTime { get; set; }
  }

  /// <summary>
  /// Model of table shiftsummarycontent
  ///
  /// Rendering of a shift summary by a generator (Raw, a LLM, ...), from its sections
  /// </summary>
  public interface IShiftSummaryContent : IVersionable, Lemoine.Collections.IDataWithId
  {
    /// <summary>
    /// Shift summary, not null
    /// </summary>
    IShiftSummary ShiftSummary { get; }

    /// <summary>
    /// Key of the generator, for example Raw or Claude
    ///
    /// not null and not empty
    /// </summary>
    string Generator { get; }

    /// <summary>
    /// Rendered summary in a structured way (sections and items), in JSON
    /// </summary>
    string Content { get; set; }

    /// <summary>
    /// Rendered summary as a markdown text, nullable
    /// </summary>
    string Text { get; set; }

    /// <summary>
    /// Versions of the sections the content was rendered from: the content is out of date
    /// when they changed since then
    /// </summary>
    string SourceVersion { get; set; }

    /// <summary>
    /// UTC date/time when the content was rendered for the last time
    /// </summary>
    DateTime DateTime { get; set; }
  }
}
