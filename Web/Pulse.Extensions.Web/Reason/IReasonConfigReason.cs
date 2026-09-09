// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Pulse.Extensions.Web.Reason
{
  /// <summary>
  /// A reason as a configuration screen sees it, declared by a
  /// <see cref="IReasonConfigExtension"/>
  ///
  /// It is not necessarily a row of the reason table: a plugin that produces reason
  /// selections of its own declares here the reasons it invents
  ///
  /// It carries ids and values only, never an entity: it may be cached, and it is what an
  /// update is written from
  /// </summary>
  public interface IReasonConfigReason
  {
    /// <summary>
    /// Identifier, unique across every extension
    ///
    /// It is the ClassificationId the reason selections carry, which is not necessarily the
    /// id of an <see cref="IReason"/>: it is one when a row of the reason table backs the
    /// declaration ("42"), and an alphanumeric identifier of the declaring extension
    /// otherwise ("MST7")
    ///
    /// Null or empty on a reason that is not created yet
    /// </summary>
    string ClassificationId { get; }

    /// <summary>
    /// May this reason be created, updated or archived through the extension?
    /// </summary>
    bool ReadOnly { get; }

    /// <summary>
    /// The properties of this reason the declaring extension may not write, so that a client
    /// does not offer them
    ///
    /// They are named the way the schema names them: "name", "code", "description", "color",
    /// "reportColor", "reasonGroup", "linkDirection", "displayPriority", "productionState",
    /// "planned", "detailsRequired", "alwaysSecondLevel", "options"
    ///
    /// An id is declared once, so this list stands as it is: there is nothing to merge
    ///
    /// Never null. It is a finer answer than <see cref="ReadOnly"/>, which says that nothing
    /// at all may be written
    /// </summary>
    IEnumerable<string> ReadOnlyProperties { get; }

    /// <summary>
    /// Name, not null and not empty
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Code
    ///
    /// nullable
    /// </summary>
    string Code { get; }

    /// <summary>
    /// Description
    ///
    /// nullable
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Color
    ///
    /// nullable: the color of the reason group applies then
    /// </summary>
    string Color { get; }

    /// <summary>
    /// Color to use in the reports
    ///
    /// nullable: <see cref="Color"/> applies then
    /// </summary>
    string ReportColor { get; }

    /// <summary>
    /// Id of the associated reason group
    /// </summary>
    int ReasonGroupId { get; }

    /// <summary>
    /// Should the detected operation time be expanded to the left or to the right?
    /// </summary>
    LinkDirection LinkDirection { get; }

    /// <summary>
    /// Priority to display the reasons, lowest first
    ///
    /// nullable
    /// </summary>
    int? DisplayPriority { get; }

    /// <summary>
    /// Id of the associated production state
    ///
    /// nullable
    /// </summary>
    int? ProductionStateId { get; }

    /// <summary>
    /// Planned / unplanned, null when it is unknown
    /// </summary>
    bool? Planned { get; }

    /// <summary>
    /// Must the operator enter a free detailed entry along with the reason?
    ///
    /// It applies to the selection entries of the reason, so it is null when they do not
    /// agree on a value, or when there is none
    /// </summary>
    bool? DetailsRequired { get; }

    /// <summary>
    /// Is the reason displayed in the second level, under its reason group?
    ///
    /// It applies to the selection entries of the reason, so it is null when they do not
    /// agree on a value, or when there is none
    /// </summary>
    bool? AlwaysSecondLevel { get; }

    /// <summary>
    /// The additional options this reason carries, never null
    ///
    /// They are read only: an extension reads them to drive what it does, and to tell
    /// whether a reason is one of its own. See
    /// <see cref="IReasonConfigExtension.AvailableOptions"/>
    /// </summary>
    IEnumerable<IReasonConfigOption> Options { get; }
  }
}
