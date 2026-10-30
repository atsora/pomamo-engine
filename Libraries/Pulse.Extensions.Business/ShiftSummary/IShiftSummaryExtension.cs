// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Extensions;

namespace Pulse.Extensions.Business.ShiftSummary
{
  /// <summary>
  /// Section of a shift summary
  /// </summary>
  public interface IShiftSummaryResultSection
  {
    /// <summary>
    /// Kind of section, usually one of <see cref="ShiftActivityKind"/>
    /// </summary>
    string Kind { get; }

    /// <summary>
    /// Title of the section, translated
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Items of the section
    /// </summary>
    IList<IShiftActivityItem> Items { get; }
  }

  /// <summary>
  /// Summary created by a <see cref="IShiftSummaryExtension"/>
  /// </summary>
  public interface IShiftSummaryResult
  {
    /// <summary>
    /// Summary in markdown, nullable
    /// </summary>
    string Text { get; }

    /// <summary>
    /// Sections, not null
    /// </summary>
    IList<IShiftSummaryResultSection> Sections { get; }
  }

  /// <summary>
  /// Default implementation of <see cref="IShiftSummaryResultSection"/>
  /// </summary>
  public class ShiftSummaryResultSection : IShiftSummaryResultSection
  {
    /// <summary>
    /// <see cref="IShiftSummaryResultSection"/>
    /// </summary>
    public string Kind { get; set; }

    /// <summary>
    /// <see cref="IShiftSummaryResultSection"/>
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// <see cref="IShiftSummaryResultSection"/>
    /// </summary>
    public IList<IShiftActivityItem> Items { get; set; } = new List<IShiftActivityItem> ();
  }

  /// <summary>
  /// Default implementation of <see cref="IShiftSummaryResult"/>
  /// </summary>
  public class ShiftSummaryResult : IShiftSummaryResult
  {
    /// <summary>
    /// <see cref="IShiftSummaryResult"/>
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// <see cref="IShiftSummaryResult"/>
    /// </summary>
    public IList<IShiftSummaryResultSection> Sections { get; set; } = new List<IShiftSummaryResultSection> ();
  }

  /// <summary>
  /// Extension that creates a summary of the activity of a machine during a shift
  ///
  /// Several generators may be available (raw data, LLM, ...), identified by their key
  /// </summary>
  public interface IShiftSummaryExtension : IExtension
  {
    /// <summary>
    /// Initialize the extension
    /// </summary>
    /// <returns>false: the extension is not active</returns>
    bool Initialize ();

    /// <summary>
    /// Key of the generator, for example Raw or Claude
    ///
    /// not null and not empty
    /// </summary>
    string Generator { get; }

    /// <summary>
    /// Priority: the generator with the highest priority is the default one
    /// </summary>
    double Priority { get; }

    /// <summary>
    /// The generator is slow (a LLM): the content is rendered by the background analysis,
    /// and not in the analysis of the machine, which would be delayed
    /// </summary>
    bool Background { get; }

    /// <summary>
    /// Create a summary
    /// </summary>
    /// <param name="data">not null</param>
    /// <param name="cancellationToken"></param>
    /// <returns>not null</returns>
    Task<IShiftSummaryResult> CreateSummaryAsync (IShiftActivityData data, CancellationToken cancellationToken);
  }
}
